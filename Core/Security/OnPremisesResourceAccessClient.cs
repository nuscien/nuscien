﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Cms;
using NuScien.Configurations;
using NuScien.Data;
using NuScien.Reflection;
using NuScien.Sns;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Security;

/// <summary>
/// The on-premises resource access client.
/// </summary>
public class OnPremisesResourceAccessClient : BaseResourceAccessClient
{
    /// <summary>
    /// The error code of invalid user name or password.
    /// </summary>
    private const string InvalidPasswordCode = "invalid_password";

    /// <summary>
    /// The authentication code verifier providers.
    /// </summary>
    private readonly Dictionary<string, IAuthorizationCodeVerifierProvider> authCodeVerifierProviders = new Dictionary<string, IAuthorizationCodeVerifierProvider>();

    /// <summary>
    /// The LDAP providers.
    /// </summary>
    private readonly Dictionary<string, IThirdPartyLoginProvider<PasswordTokenRequestBody>> ldapProviders = new Dictionary<string, IThirdPartyLoginProvider<PasswordTokenRequestBody>>();

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessClient class.
    /// </summary>
    /// <param name="provider">The account data provider.</param>
    public OnPremisesResourceAccessClient(IAccountDataProvider provider)
    {
        DataProvider = provider ?? new MemoryAccountDbSetProvider();
    }

    /// <summary>
    /// Gets the account data provider.
    /// </summary>
    protected IAccountDataProvider DataProvider { get; }

    /// <summary>
    /// Registers a third-party login provider.
    /// </summary>
    /// <param name="serviceProvider">The name or URL of authentication code service provider.</param>
    /// <param name="provider">The third-party login provider instance; or null to remove.</param>
    /// <returns>true if register or remove succeeded; otherwise, false.</returns>
    public bool Register(string serviceProvider, IAuthorizationCodeVerifierProvider provider)
    {
        if (string.IsNullOrWhiteSpace(serviceProvider)) return false;
        if (provider == null) return authCodeVerifierProviders.Remove(serviceProvider);
        else authCodeVerifierProviders[serviceProvider] = provider;
        return true;
    }

    /// <summary>
    /// Registers a third-party login provider.
    /// </summary>
    /// <param name="ldap">The LDAP server name or IP address.</param>
    /// <param name="provider">The third-party login provider instance; or null to remove.</param>
    /// <returns>true if register or remove succeeded; otherwise, false.</returns>
    public bool Register(string ldap, IThirdPartyLoginProvider<PasswordTokenRequestBody> provider)
    {
        if (string.IsNullOrWhiteSpace(ldap) || provider == null) return false;
        if (provider == null) return ldapProviders.Remove(ldap);
        else ldapProviders[ldap] = provider;
        return true;
    }

    /// <summary>
    /// Signs in.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The login response.</returns>
    public override async Task<UserTokenInfo> SignInAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
    {
        var eui = AssertTokenRequest(tokenRequest);
        if (eui != null) return eui;
        if (tokenRequest.Body.Password.Length < 1) return new UserTokenInfo
        {
            ErrorCode = InvalidPasswordCode,
            ErrorDescription = "The password should not be null."
        };
        var provider = ldapProviders.TryGetProvider(tokenRequest.Body.Ldap);
        var userTask = provider != null
            ? provider.ProcessAsync(tokenRequest.Body, cancellationToken)
            : DataProvider.GetUserByLognameAsync(tokenRequest.Body.UserName, cancellationToken);
        return await CreateTokenAsync(userTask, tokenRequest, user =>
        {
            if (user != null && user.ValidatePassword(tokenRequest.Body.Password)) return null;
            return new UserTokenInfo
            {
                ErrorCode = InvalidPasswordCode,
                ErrorDescription = "The user name or password is incorrect."
            };
        }, cancellationToken);
    }

    /// <summary>
    /// Signs in.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The login response.</returns>
    public override async Task<UserTokenInfo> SignInAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
    {
        var eui = AssertTokenRequest(tokenRequest);
        if (eui != null) return eui;
        var tokenTask = DataProvider.GetTokenByRefreshTokenAsync(tokenRequest.Body.RefreshToken, cancellationToken);
        return await CreateTokenAsync(tokenTask, tokenRequest, cancellationToken);
    }

    /// <summary>
    /// Signs in.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The login response.</returns>
    public override async Task<UserTokenInfo> SignInAsync(TokenRequest<CodeTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
    {
        var eui = AssertTokenRequest(tokenRequest);
        if (eui != null) return eui;
        var provider = authCodeVerifierProviders.TryGetProvider(tokenRequest.Body.ServiceProvider);
        if (provider?.HasSaved == false)
        {
            var userTask = provider.ProcessAsync(tokenRequest.Body, cancellationToken);
            return await CreateTokenAsync(userTask, tokenRequest, null, cancellationToken);
        }

        AuthorizationCodeEntity code;
        try
        {
            code = await DataProvider.GetAuthorizationCodeByCodeAsync(tokenRequest.Body.ServiceProvider, tokenRequest.Body.Code, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
        }
        catch (InvalidOperationException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotImplementedException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NullReferenceException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotSupportedException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
        }

        if (code == null)
        {
            if (provider?.HasSaved == true)
            {
                var userTask = provider.ProcessAsync(tokenRequest.Body, cancellationToken);
                return await CreateTokenAsync(userTask, tokenRequest, null, cancellationToken);
            }

            return new UserTokenInfo
            {
                ErrorCode = "invalid_code",
                ErrorDescription = "The code is invalid."
            };
        }

        return code.OwnerType switch
        {
            SecurityEntityTypes.User => await CreateTokenAsync(DataProvider.GetUserByIdAsync(code.OwnerId, cancellationToken), tokenRequest, null, cancellationToken),
            SecurityEntityTypes.ServiceClient => await CreateTokenAsync(null as UserEntity, tokenRequest, cancellationToken),
            _ => new UserTokenInfo
            {
                ErrorCode = "invalid_code",
                ErrorDescription = "The resource is invalid."
            },
        };
    }

    /// <summary>
    /// Signs in.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The login response.</returns>
    public override async Task<UserTokenInfo> SignInAsync(TokenRequest<ClientTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
    {
        var eui = AssertTokenRequest(tokenRequest);
        if (eui != null) return eui;
        try
        {
            var client = await DataProvider.GetClientByNameAsync(tokenRequest.ClientId, cancellationToken);
            if (client == null) return new UserTokenInfo
            {
                ErrorCode = TokenInfo.ErrorCodeConstants.UnauthorizedClient,
                ErrorDescription = "The client app identifier or secret key is incorrect."
            };
        }
        catch (ArgumentException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
        }
        catch (InvalidOperationException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotImplementedException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NullReferenceException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotSupportedException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
        }

        return await CreateTokenAsync(null as UserEntity, tokenRequest, cancellationToken);
    }

    /// <summary>
    /// Signs in.
    /// </summary>
    /// <param name="accessToken">The access request.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The login response.</returns>
    public override async Task<UserTokenInfo> AuthorizeAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return new UserTokenInfo
        {
            ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
            ErrorDescription = "The access token was null, empty or consists only of white-space characters."
        };
        var tokenTask = DataProvider.GetTokenByNameAsync(accessToken, cancellationToken);
        return await CreateTokenAsync(tokenTask, null, cancellationToken);
    }

    /// <summary>
    /// Sets a new authorization code.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="code">The original authorization code.</param>
    /// <param name="insertNewOne">true if need add a new one; otherwise, false.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The status of changing result.</returns>
    public async override Task<ChangingResultInfo> SetAuthorizationCodeAsync(string serviceProvider, string code, bool insertNewOne = false, CancellationToken cancellationToken = default)
    {
        string id;
        SecurityEntityTypes kind;
        if (!string.IsNullOrWhiteSpace(UserId))
        {
            kind = SecurityEntityTypes.User;
            id = UserId;
        }
        else if (IsClientCredentialVerified)
        {
            kind = SecurityEntityTypes.ServiceClient;
            id = ClientId;
        }
        else
        {
            return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, "No authentication passed.");
        }

        AuthorizationCodeEntity entity = null;
        if (!insertNewOne)
        {
            var col = await DataProvider.GetAuthorizationCodesByOwnerAsync(serviceProvider, kind, id, cancellationToken);
            if (col != null) entity = col.FirstOrDefault();
        }

        if (entity == null) entity = new AuthorizationCodeEntity
        {
            OwnerId = id,
            OwnerType = kind,
            ServiceProvider = serviceProvider
        };
        entity.SetCode(code);
        var user = User;
        if (kind == SecurityEntityTypes.User && user != null)
        {
            if (!string.IsNullOrWhiteSpace(user.Avatar)) entity.Avatar = user.Avatar;
            entity.Name = user.Nickname ?? user.Name ?? UserId;
        }

        return await DataProvider.SaveAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Signs out.
    /// </summary>
    /// <returns>The task.</returns>
    public override async Task SignOutAsync()
    {
        var t = Token;
        if (t == null || t.IsEmpty) return;
        Task<int> task = null;
        try
        {
            await DataProvider.DeleteAccessTokenAsync(t.AccessToken);
            var uId = t.UserId;
            if (!string.IsNullOrWhiteSpace(uId))
                task = DataProvider.DeleteExpiredClientTokensAsync(uId);
        }
        finally
        {
            await base.SignOutAsync();
        }

        if (task != null) await task;
    }

    /// <summary>
    /// Gets a value indicating whether the specific user login name has been registered.
    /// </summary>
    /// <param name="logname">The user login name.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>true if the specific user login name has been registered; otherwise, false.</returns>
    public override async Task<bool> HasUserNameAsync(string logname, CancellationToken cancellationToken = default)
    {
        return await DataProvider.GetUserByLognameAsync(logname, cancellationToken) != null;
    }

    /// <summary>
    /// Gets a user entity by given identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The user group entity matched if found; otherwise, null.</returns>
    public override async Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var user = await DataProvider.GetUserByIdAsync(id, cancellationToken);
        return user ?? await DataProvider.GetUserByLognameAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets a user group entity by given identifier.
    /// </summary>
    /// <param name="id">The user group identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The user group entity matched if found; otherwise, null.</returns>
    public override Task<UserGroupEntity> GetUserGroupByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetUserGroupByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Searches users.
    /// </summary>
    /// <param name="group">The user group entity.</param>
    /// <param name="role">The role to search; or null for all roles.</param>
    /// <param name="q">The optional name query; or null for all.</param>
    /// <param name="relationshipState">The relationship entity state.</param>
    /// <returns>The token entity matched if found; otherwise, null.</returns>
    public IEnumerable<UserEntity> ListUsers(UserGroupEntity group, UserGroupRelationshipEntity.Roles role, string q, ResourceEntityStates relationshipState)
    {
        var col = DataProvider.ListUsers(group, role, q, relationshipState);
        if (col == null) return new List<UserEntity>();
        return col.Select(ele => ele.Target);
    }

    /// <summary>
    /// Searches users.
    /// </summary>
    /// <param name="group">The user group entity.</param>
    /// <param name="role">The role to search; or null for all roles.</param>
    /// <param name="q">The optional name query; or null for all.</param>
    /// <returns>The token entity matched if found; otherwise, null.</returns>
    public IEnumerable<UserEntity> ListUsers(UserGroupEntity group, UserGroupRelationshipEntity.Roles role, string q = null)
    {
        var col = DataProvider.ListUsers(group, role, q);
        if (col == null) return new List<UserEntity>();
        return col.Select(ele => ele.Target);
    }

    /// <summary>
    /// Searches users.
    /// </summary>
    /// <param name="group">The user group entity.</param>
    /// <param name="q">The optional name query; or null for all.</param>
    /// <param name="relationshipState">The relationship entity state.</param>
    /// <returns>The token entity matched if found; otherwise, null.</returns>
    public IEnumerable<UserEntity> ListUsers(UserGroupEntity group, string q, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
    {
        var col = DataProvider.ListUsers(group, null, q, relationshipState);
        if (col == null) return new List<UserEntity>();
        return col.Select(ele => ele.Target);
    }

    /// <summary>
    /// Searches user groups.
    /// </summary>
    /// <param name="q">The optional query request information.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The token entity matched if found; otherwise, null.</returns>
    public override Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListGroupsAsync(q, false, cancellationToken);
    }

    /// <summary>
    /// Searches user groups.
    /// </summary>
    /// <param name="q">The optional query request information.</param>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The token entity matched if found; otherwise, null.</returns>
    public override async Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, string siteId, CancellationToken cancellationToken = default)
    {
        var perms = await GetPermissionsAsync(siteId);
        var onlyPublic = perms == null || !perms.HasAnyPermission(PermissionItems.GroupManagement, PermissionItems.SiteAdmin);
        return await DataProvider.ListGroupsAsync(q, siteId, onlyPublic, cancellationToken);
    }

    /// <summary>
    /// Renews the client app key.
    /// </summary>
    /// <param name="appId">The client app identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The client app identifier and secret key.</returns>
    public override async Task<AppAccessingKey> RenewAppClientKeyAsync(string appId, CancellationToken cancellationToken = default)
    {
        var client = await DataProvider.GetClientByNameAsync(appId, cancellationToken);
        if (client == null) client = new AccessingClientEntity
        {
            State = ResourceEntityStates.Normal
        };
        var key = client.RenewCredentialKey();
        var r = await DataProvider.SaveAsync(client, cancellationToken);
        return r switch
        {
            ChangeMethods.Invalid => null,
            _ => key
        };
    }

    /// <summary>
    /// Gets a specific publish content.
    /// </summary>
    /// <param name="id">The identifier of the publish content.</param>
    /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity to get.</returns>
    public override Task<ContentEntity> GetContentAsync(string id, bool includeAllStates, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetContentAsync(id, includeAllStates, cancellationToken);
    }

    /// <summary>
    /// Lists the publish contents.
    /// </summary>
    /// <param name="siteId">The owner site identifier.</param>
    /// <param name="parent">The optional parent content identifier.</param>
    /// <param name="q">The optional query arguments.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<IEnumerable<ContentEntity>> ListContentAsync(string siteId, string parent = null, QueryArgs q = null, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListContentAsync(siteId, parent, q, cancellationToken);
    }

    /// <summary>
    /// Lists the publish contents.
    /// </summary>
    /// <param name="siteId">The owner site identifier.</param>
    /// <param name="all">true if search all contents; otherise, false.</param>
    /// <param name="q">The optional query arguments.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<IEnumerable<ContentEntity>> ListContentAsync(string siteId, bool all, QueryArgs q = null, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListContentAsync(siteId, all, q, cancellationToken);
    }

    /// <summary>
    /// Lists the revision entities.
    /// </summary>
    /// <param name="source">The source owner identifier.</param>
    /// <param name="q">The optional query arguments.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<IEnumerable<ContentRevisionEntity>> ListContentRevisionAsync(string source, QueryArgs q = null, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListContentRevisionAsync(source, q, cancellationToken);
    }

    /// <summary>
    /// Lists the revisions.
    /// </summary>
    /// <param name="id">The revision entity identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<ContentRevisionEntity> GetContentRevisionAsync(string id, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetContentRevisionAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets a specific publish content template.
    /// </summary>
    /// <param name="id">The identifier of the publish content template.</param>
    /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity to get.</returns>
    public override Task<ContentTemplateEntity> GetContentTemplateAsync(string id, bool includeAllStates, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetContentTemplateAsync(id, includeAllStates, cancellationToken);
    }

    /// <summary>
    /// Lists the publish content templates.
    /// </summary>
    /// <param name="siteId">The owner site identifier.</param>
    /// <param name="q">The optional query arguments.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<IEnumerable<ContentTemplateEntity>> ListContentTemplateAsync(string siteId, QueryArgs q = null, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListContentTemplateAsync(siteId, q, cancellationToken);
    }

    /// <summary>
    /// Lists the revision entities.
    /// </summary>
    /// <param name="source">The source owner identifier.</param>
    /// <param name="q">The optional query arguments.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<IEnumerable<ContentTemplateRevisionEntity>> ListContentTemplateRevisionAsync(string source, QueryArgs q = null, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListContentTemplateRevisionAsync(source, q, cancellationToken);
    }

    /// <summary>
    /// Lists the revisions.
    /// </summary>
    /// <param name="id">The revision entity identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<ContentTemplateRevisionEntity> GetContentTemplateRevisionAsync(string id, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetContentTemplateRevisionAsync(id, cancellationToken);
    }

    /// <summary>
    /// Lists the publish content comments.
    /// </summary>
    /// <param name="content">The owner content comment identifier.</param>
    /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
    /// <param name="q">The optional query arguments.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<IEnumerable<ContentCommentEntity>> ListContentCommentsAsync(string content, bool plain, QueryArgs q, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListContentCommentsAsync(content, plain, q, cancellationToken);
    }

    /// <summary>
    /// Gets a specific publish content comment.
    /// </summary>
    /// <param name="id">The identifier of the publish content template.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity to delete.</returns>
    public override Task<ContentCommentEntity> GetContentCommentAsync(string id, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetContentCommentAsync(id, true, cancellationToken);
    }

    /// <summary>
    /// Lists the child comments of a specific publish content comment.
    /// </summary>
    /// <param name="commentId">The parent identifier of the content comment.</param>
    /// <param name="q">The optional query arguments.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The entity list.</returns>
    public override Task<IEnumerable<ContentCommentEntity>> ListContentChildCommentsAsync(string commentId, QueryArgs q, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListContentChildCommentsAsync(commentId, q, cancellationToken);
    }

    /// <summary>
    /// Gets the settings.
    /// </summary>
    /// <param name="key">The settings key with optional namespace.</param>
    /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The value.</returns>
    protected async override Task<SettingsEntity.Model> GetSettingsModelByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        if (string.IsNullOrWhiteSpace(siteId)) siteId = null;
        else siteId = siteId.Trim();
        var task = DataProvider.GetSettingsAsync(key, null, cancellationToken);
        return siteId == null
            ? new SettingsEntity.Model(key, await task)
        : new SettingsEntity.Model(key,
            siteId,
            await DataProvider.GetSettingsAsync(key, siteId, cancellationToken),
            await task);
    }

    /// <summary>
    /// Gets the settings.
    /// </summary>
    /// <param name="key">The settings key with optional namespace.</param>
    /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The value.</returns>
    protected override Task<JsonObjectNode> GetSettingsDataByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        if (string.IsNullOrWhiteSpace(siteId)) siteId = null;
        else siteId = siteId.Trim();
        return DataProvider.GetSettingsAsync(key, siteId, cancellationToken);
    }

    /// <summary>
    /// Gets the settings.
    /// </summary>
    /// <param name="key">The settings key with optional namespace.</param>
    /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The value.</returns>
    protected override Task<string> GetSettingsJsonStringByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        if (string.IsNullOrWhiteSpace(siteId)) siteId = null;
        else siteId = siteId.Trim();
        return DataProvider.GetSettingsJsonStringAsync(key, siteId, cancellationToken);
    }

    /// <summary>
    /// Creates or updates the settings.
    /// </summary>
    /// <param name="key">The settings key with optional namespace.</param>
    /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
    /// <param name="value">The value.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The change method.</returns>
    public override async Task<ChangingResultInfo> SaveSettingsAsync(string key, string siteId, JsonObjectNode value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires a settings key.");
        else key = key.Trim();
        if (string.IsNullOrWhiteSpace(siteId))
        {
            siteId = null;
            if (!await IsSystemSettingsAdminAsync(cancellationToken)) return new ChangingResultInfo(ChangeErrorKinds.Forbidden, "No permission to save the global settings.");
        }
        else
        {
            siteId = siteId.Trim();
            if (!await IsSystemSettingsAdminAsync(siteId, cancellationToken)) return new ChangingResultInfo(ChangeErrorKinds.Forbidden, "No permission to save the settings of the specific site.");
        }

        ClearSettingsCache();
        return await DataProvider.SaveSettingsAsync(key, siteId, value, cancellationToken);
    }

    /// <summary>
    /// Creates or updates a user permission item entity.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="targetType">The target entity type.</param>
    /// <param name="targetId">The target entity identifier.</param>
    /// <param name="permissionList">The permission list.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The status of changing result.</returns>
    public override async Task<ChangingResultInfo> SavePermissionAsync(string siteId, SecurityEntityTypes targetType, string targetId, IEnumerable<string> permissionList, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(siteId)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires the site identifier.");
        siteId = siteId.Trim();
        if (!await IsPermissionAdminAsync(siteId, cancellationToken)) return new ChangingResultInfo(ChangeErrorKinds.Forbidden, "No permission to set the permission.");
        switch (targetType)
        {
            case SecurityEntityTypes.User:
                {
                    var user = await DataProvider.GetUserByIdAsync(targetId, cancellationToken);
                    var perm = await DataProvider.GetUserPermissionsAsync(user, siteId, cancellationToken);
                    if (perm == null) perm = new UserPermissionItemEntity();
                    else perm.Permissions = null;
                    FillPermissionItemEntity(perm, siteId, user);
                    perm.AddPermission(permissionList);
                    return await DataProvider.SaveAsync(perm, cancellationToken);
                }
            case SecurityEntityTypes.UserGroup:
                {
                    var group = await DataProvider.GetUserGroupByIdAsync(targetId, cancellationToken);
                    var perm = await DataProvider.GetGroupPermissionsAsync(group, siteId, cancellationToken);
                    if (perm == null) perm = new UserGroupPermissionItemEntity();
                    else perm.Permissions = null;
                    FillPermissionItemEntity(perm, siteId, group);
                    perm.AddPermission(permissionList);
                    return await DataProvider.SaveAsync(perm, cancellationToken);
                }
            case SecurityEntityTypes.ServiceClient:
                {
                    var client = await DataProvider.GetClientByIdAsync(targetId, cancellationToken);
                    var perm = await DataProvider.GetClientPermissionsAsync(client, siteId, cancellationToken);
                    if (perm == null) perm = new ClientPermissionItemEntity();
                    else perm.Permissions = null;
                    FillPermissionItemEntity(perm, siteId, client);
                    perm.AddPermission(permissionList);
                    return await DataProvider.SaveAsync(perm, cancellationToken);
                }
            default:
                {
                    return new ChangingResultInfo(ChangeErrorKinds.Argument, "The target type is not supported.");
                }
        }
    }

    /// <summary>
    /// Gets the permission items.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="targetType">The target entity type.</param>
    /// <param name="targetId">The target entity identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The permission list.</returns>
    /// <exception cref="ArgumentNullException">siteId was null or empty.</exception>
    /// <exception cref="ArgumentException">The target type was invalid.</exception>
    /// <exception cref="UnauthorizedAccessException">No permission.</exception>
    public override async Task<CollectionResult<string>> GetPermissionAsync(string siteId, SecurityEntityTypes targetType, string targetId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(siteId)) throw new ArgumentNullException(nameof(siteId), "siteId should not be null or empty.");
        siteId = siteId.Trim();
        bool hasPerm;
        switch (targetType)
        {
            case SecurityEntityTypes.User:
                {
                    hasPerm = (targetId == UserId && !string.IsNullOrWhiteSpace(UserId)) || await IsPermissionAdminAsync(siteId, cancellationToken);
                    break;
                }
            case SecurityEntityTypes.UserGroup:
                {
                    hasPerm = true;
                    break;
                }
            case SecurityEntityTypes.ServiceClient:
                {
                    hasPerm = true;
                    break;
                }
            default:
                {
                    throw new ArgumentException("The target type is not supported.", nameof(targetType));
                }
        }

        if (!hasPerm) throw new UnauthorizedAccessException("No permission!");
        switch (targetType)
        {
            case SecurityEntityTypes.User:
                {
                    var user = await DataProvider.GetUserByIdAsync(targetId, cancellationToken);
                    var perm = await DataProvider.GetUserPermissionsAsync(user, siteId, cancellationToken);
                    return new CollectionResult<string>(perm?.GetPermissionList());
                }
            case SecurityEntityTypes.UserGroup:
                {
                    var group = await DataProvider.GetUserGroupByIdAsync(targetId, cancellationToken);
                    var perm = await DataProvider.GetGroupPermissionsAsync(group, siteId, cancellationToken);
                    return new CollectionResult<string>(perm?.GetPermissionList());
                }
            case SecurityEntityTypes.ServiceClient:
                {
                    var client = await DataProvider.GetClientByIdAsync(targetId, cancellationToken);
                    var perm = await DataProvider.GetClientPermissionsAsync(client, siteId, cancellationToken);
                    return new CollectionResult<string>(perm?.GetPermissionList());
                }
            default:
                {
                    throw new ArgumentException("The target type is not supported.", nameof(targetType));
                }
        }
    }

    /// <summary>
    /// Gets the user permissions of the current user.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The user permission list.</returns>
    public override Task<UserPermissionItemEntity> GetUserPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
    {
        var u = User;
        if (u == null) return Task.FromResult<UserPermissionItemEntity>(null);
        return DataProvider.GetUserPermissionsAsync(u, siteId, cancellationToken);
    }

    /// <summary>
    /// Gets the user group permissions of the current user.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The user group permission list.</returns>
    public override Task<IEnumerable<UserGroupPermissionItemEntity>> GetGroupPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
    {
        var u = User;
        if (u == null) return Task.FromResult<IEnumerable<UserGroupPermissionItemEntity>>(new List<UserGroupPermissionItemEntity>());
        return DataProvider.ListGroupPermissionsAsync(User, siteId, cancellationToken);
    }

    /// <summary>
    /// Gets the client permissions of the current client.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The client permission list.</returns>
    public override Task<ClientPermissionItemEntity> GetClientPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
    {
        return ClientVerified != null ? DataProvider.GetClientPermissionsAsync(ClientVerified, siteId, cancellationToken) : null;
    }

    private static void FillPermissionItemEntity<T>(BasePermissionItemEntity<T> entity, string siteId, T target) where T : BaseSecurityEntity
    {
        entity.Name = target.Nickname ?? target.Nickname;
        entity.SiteId = siteId;
        entity.Target = target;
        entity.State = ResourceEntityStates.Normal;
    }

    /// <summary>
    /// Searches users.
    /// </summary>
    /// <param name="group">The user group entity.</param>
    /// <param name="role">The role to search; or null for all roles.</param>
    /// <param name="q">The optional query information.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The token entity matched if found; otherwise, null.</returns>
    protected override async Task<IEnumerable<UserEntity>> ListUsersByGroupAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role, QueryArgs q, CancellationToken cancellationToken = default)
    {
        if (q == null) q = InternalAssertion.DefaultQueryArgs;
        var col = await DataProvider.ListUsersAsync(group, role, q, cancellationToken);
        if (col == null) return new List<UserEntity>();
        return col.Select(ele => ele.Target);
    }

    /// <summary>
    /// Gets a collection of user groups joined in.
    /// </summary>
    /// <param name="q">The optional query for group.</param>
    /// <param name="relationshipState">The relationship entity state.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The user group relationships.</returns>
    protected override Task<IEnumerable<UserGroupRelationshipEntity>> GetRelationshipsAsync(string q, ResourceEntityStates relationshipState, CancellationToken cancellationToken = default)
    {
        return DataProvider.ListUserGroupsAsync(User, q, relationshipState, cancellationToken);
    }

    /// <summary>
    /// Gets a user group relationship entity.
    /// </summary>
    /// <param name="id">The user group relationship entity identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The user group entity matched if found; otherwise, null.</returns>
    protected Task<UserGroupRelationshipEntity> GetRelationshipAsync(string id, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetRelationshipByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets a user group relationship entity.
    /// </summary>
    /// <param name="groupId">The user group identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The user group entity matched if found; otherwise, null.</returns>
    protected override Task<UserGroupRelationshipEntity> GetRelationshipAsync(string groupId, string userId, CancellationToken cancellationToken = default)
    {
        return DataProvider.GetRelationshipByIdAsync(groupId, userId, cancellationToken);
    }

    /// <summary>
    /// Creates or updates a user entity.
    /// </summary>
    /// <param name="value">The user entity to save.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The status of changing result.</returns>
    protected override async Task<ChangingResultInfo> SaveEntityAsync(UserEntity value, CancellationToken cancellationToken = default)
    {
        var result = await DataProvider.SaveAsync(value, cancellationToken);
        if (ResourceEntityExtensions.IsSuccessful(result))
            return new ChangingResultInfo<UserEntity>(result, value, result.ToString() + " user entity.");
        return result;
    }

    /// <summary>
    /// Creates or updates a user group entity.
    /// </summary>
    /// <param name="value">The user group entity to save.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The status of changing result.</returns>
    protected override async Task<ChangingResultInfo> SaveEntityAsync(UserGroupEntity value, CancellationToken cancellationToken = default)
    {
        var result = await DataProvider.SaveAsync(value, cancellationToken);
        if (ResourceEntityExtensions.IsSuccessful(result))
            return new ChangingResultInfo<UserGroupEntity>(result, value, result.ToString() + " user group entity.");
        return result;
    }

    /// <summary>
    /// Creates or updates a user group relationship entity.
    /// </summary>
    /// <param name="value">The user group relationship entity to save.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The status of changing result.</returns>
    protected override async Task<ChangingResultInfo> SaveEntityAsync(UserGroupRelationshipEntity value, CancellationToken cancellationToken = default)
    {
        var result = await DataProvider.SaveAsync(value, cancellationToken);
        if (ResourceEntityExtensions.IsSuccessful(result))
            return new ChangingResultInfo<UserGroupRelationshipEntity>(result, value, result.ToString() + " user group relationship entity.");
        return result;
    }

    /// <summary>
    /// Creates or updates a publish content entity.
    /// </summary>
    /// <param name="content">The publish content entity to save.</param>
    /// <param name="message">The commit message.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The change method.</returns>
    protected override async Task<ChangingResultInfo> SaveEntityAsync(ContentEntity content, string message, CancellationToken cancellationToken = default)
    {
        var result = await DataProvider.SaveAsync(content, message, cancellationToken);
        if (ResourceEntityExtensions.IsSuccessful(result))
            return new ChangingResultInfo<ContentEntity>(result, content, result.ToString() + " publish content entity.");
        return result;
    }

    /// <summary>
    /// Creates or updates a publish content template entity.
    /// </summary>
    /// <param name="template">The publish content template entity to save.</param>
    /// <param name="message">The commit message.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The change method.</returns>
    protected override async Task<ChangingResultInfo> SaveEntityAsync(ContentTemplateEntity template, string message, CancellationToken cancellationToken = default)
    {
        var result = await DataProvider.SaveAsync(template, message, cancellationToken);
        if (ResourceEntityExtensions.IsSuccessful(result))
            return new ChangingResultInfo<ContentTemplateEntity>(result, template, result.ToString() + " publish content template entity.");
        return result;
    }

    /// <summary>
    /// Creates or updates a publish content comment entity.
    /// </summary>
    /// <param name="comment">The publish content comment entity to save.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The change method.</returns>
    protected override async Task<ChangingResultInfo> SaveEntityAsync(ContentCommentEntity comment, CancellationToken cancellationToken = default)
    {
        var result = await DataProvider.SaveAsync(comment, cancellationToken);
        if (ResourceEntityExtensions.IsSuccessful(result))
            return new ChangingResultInfo<ContentCommentEntity>(result, comment, result.ToString() + " comment entity for publish content.");
        return result;
    }

    /// <summary>
    /// Creates a token by given user.
    /// </summary>
    /// <param name="user">The user entity to create token.</param>
    /// <param name="tokenRequest">The token request to login.</param>
    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    /// <returns>The login response.</returns>
    private async Task<UserTokenInfo> CreateTokenAsync(UserEntity user, TokenRequest tokenRequest, CancellationToken cancellationToken = default)
    {
        var eui = AssertParameter(tokenRequest, nameof(tokenRequest));
        if (eui != null) return eui;
        eui = CheckUser(user);
        if (eui != null) return eui;
        return await CreateTokenAsync(user, null, tokenRequest, cancellationToken);
    }

    private async Task<UserTokenInfo> CreateTokenAsync(UserEntity user, TokenEntity token, TokenRequest tokenRequest, CancellationToken cancellationToken = default)
    {
        var needSave = token is null;
        var clientId = tokenRequest != null ? tokenRequest.ClientId : token?.ClientId;
        var resId = user is null ? clientId : user.Id;
        if (needSave)
        {
            token = new TokenEntity
            {
                GrantType = tokenRequest.GrantType,
                UserId = user?.Id,
                ClientId = clientId,
                ScopeString = tokenRequest.ScopeString
            };
            token.CreateToken(true);
        }
        else if (!string.IsNullOrWhiteSpace(clientId) && token.ClientId != clientId)
        {
            return new UserTokenInfo
            {
                User = user,
                UserId = user?.Id,
                ResourceId = resId,
                ErrorCode = TokenInfo.ErrorCodeConstants.InvalidClient,
                ErrorDescription = "The client is not for this token."
            };
        }
        else if (token.IsClosedToExpiration)
        {
            if (tokenRequest != null && string.IsNullOrWhiteSpace(tokenRequest.ClientId)) tokenRequest.ClientId = token.ClientId;
            token = new TokenEntity
            {
                GrantType = token.GrantType,
                UserId = token.UserId,
                ClientId = clientId,
                ScopeString = token.ScopeString,
                RefreshToken = token.RefreshToken,
            };
            token.CreateToken();
            needSave = true;
        }

        try
        {
            if (needSave)
            {
                var r = await DataProvider.SaveAsync(token, cancellationToken);
                if (r == ChangeMethods.Invalid) return new UserTokenInfo
                {
                    User = user,
                    UserId = user?.Id,
                    ResourceId = resId,
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = "Generate token failed."
                };
            }

            ClientVerified = null;
            UserId = user?.Id;
            ClientId = token.ClientId;
            if (!string.IsNullOrWhiteSpace(token.ClientId) && tokenRequest?.ClientCredentials?.Secret != null && tokenRequest.ClientCredentials.Secret.Length > 0)
            {
                var clientInfo = await DataProvider.GetClientByNameAsync(token.ClientId, cancellationToken);
                if (clientInfo != null && clientInfo.ValidateCredentialKey(tokenRequest.ClientCredentials.Secret))
                {
                    ClientVerified = clientInfo;
                    clientInfo.SetPropertiesReadonly();
                }

                else return new UserTokenInfo
                {
                    User = user,
                    UserId = user?.Id,
                    ResourceId = resId,
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidClient,
                    ErrorDescription = "The client secret credential key is invalid."
                };
            }

            return Token = new UserTokenInfo
            {
                User = user,
                UserId = user?.Id,
                ResourceId = resId,
                AccessToken = token.Name,
                RefreshToken = token.RefreshToken,
                ExpiredAfter = token.ExpirationTime - DateTime.Now,
                TokenType = TokenInfo.BearerTokenType,
                ScopeString = token.ScopeString
            };
        }
        catch (ArgumentException ex)
        {
            return new UserTokenInfo(user, clientId, ex);
        }
        catch (InvalidOperationException ex)
        {
            return new UserTokenInfo(user, clientId, ex);
        }
        catch (NotImplementedException ex)
        {
            return new UserTokenInfo(user, clientId, ex);
        }
        catch (NullReferenceException ex)
        {
            return new UserTokenInfo(user, clientId, ex);
        }
        catch (NotSupportedException ex)
        {
            return new UserTokenInfo(user, clientId, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new UserTokenInfo(user, clientId, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
        }
    }

    private async Task<UserTokenInfo> CreateTokenAsync(Task<UserEntity> userResolver, TokenRequest tokenRequest, Func<UserEntity, UserTokenInfo> callback, CancellationToken cancellationToken = default)
    {
        var eui = AssertParameter(userResolver, nameof(userResolver));
        if (eui != null) return eui;
        UserEntity user;
        try
        {
            user = await userResolver;
        }
        catch (ArgumentException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
        }
        catch (InvalidOperationException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotImplementedException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NullReferenceException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotSupportedException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new UserTokenInfo(null, ex);
        }

        var e = callback?.Invoke(user);
        if (e != null) return e;
        if (user == null) return new UserTokenInfo
        {
            ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
            ErrorDescription = "Cannot resolve the user."
        };
        return await CreateTokenAsync(user, tokenRequest, cancellationToken);
    }

    private static UserTokenInfo CheckUser(UserEntity user)
    {
        if (user is null || user.IsNew || string.IsNullOrWhiteSpace(user.Name)) return new UserTokenInfo
        {
            ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
            ErrorDescription = "The user does not exist."
        };
        return null;
    }

    private async Task<UserTokenInfo> CreateTokenAsync(Task<TokenEntity> tokenResolver, TokenRequest tokenRequest, CancellationToken cancellationToken = default)
    {
        var eui = AssertParameter(tokenResolver, nameof(tokenResolver));
        if (eui != null) return eui;
        TokenEntity token;
        UserEntity user;
        try
        {
            token = await tokenResolver;
            if (token is null) return new UserTokenInfo
            {
                ErrorCode = "invalid_access_token",
                ErrorDescription = "The access token is invalid.",
                ClientId = tokenRequest?.ClientId
            };
            if (token.IsExpired)
            {
                await DataProvider.DeleteExpiredTokensAsync(token.UserId, cancellationToken);
                return new UserTokenInfo
                {
                    ErrorCode = "invalid_access_token",
                    ErrorDescription = "The access token is expired.",
                    ClientId = tokenRequest?.ClientId
                };
            }

            user = await DataProvider.GetUserByIdAsync(token.UserId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
        }
        catch (InvalidOperationException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotImplementedException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NullReferenceException ex)
        {
            return new UserTokenInfo(null, ex);
        }
        catch (NotSupportedException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new UserTokenInfo(null, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
        }

        var errInfo = CheckUser(user);
        if (errInfo != null) return errInfo;
        return await CreateTokenAsync(user, token, tokenRequest, cancellationToken);
    }

    private static UserTokenInfo AssertTokenRequest(TokenRequest tokenRequest)
    {
        if (tokenRequest == null || tokenRequest.Body == null || tokenRequest.ClientId == null) return new UserTokenInfo
        {
            ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
            ErrorDescription = "Miss the token request information."
        };
        return null;
    }

    private static UserTokenInfo AssertParameter(object obj, string parameterName = null)
    {
        if (obj == null) return new UserTokenInfo
        {
            ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
            ErrorDescription = (parameterName ?? "The parameter") + " was null."
        };
        return null;
    }
}
