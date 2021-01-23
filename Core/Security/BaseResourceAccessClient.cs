using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Configurations;
using NuScien.Data;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The base resource access client.
    /// </summary>
    public abstract class BaseResourceAccessClient : TokenContainer
    {
        /// <summary>
        /// The flag to use long cache duration
        /// </summary>
        private bool isLongCacheDuration;

        /// <summary>
        /// The token request route instance.
        /// </summary>
        private TokenRequestRoute<UserEntity> route;

        /// <summary>
        /// The system global settings.
        /// </summary>
        private SystemGlobalSettings globalSettings;

        /// <summary>
        /// The expiration of global settings cache.
        /// </summary>
        private DateTime globalSettingsExpiration = DateTime.Now;

        /// <summary>
        /// The user groups.
        /// </summary>
        private System.Collections.Concurrent.ConcurrentBag<UserGroupRelationshipEntity> groupsCache;

        /// <summary>
        /// The permissions set.
        /// </summary>
        private readonly Dictionary<string, UserSitePermissionSet> permissions = new Dictionary<string, UserSitePermissionSet>();

        /// <summary>
        /// The settings set.
        /// </summary>
        private readonly DataCacheCollection<string> settings = new DataCacheCollection<string>
        {
            Expiration = TimeSpan.FromMinutes(3)
        };

        /// <summary>
        /// The settings set.
        /// </summary>
        private readonly DataCacheCollection<SystemSiteSettings> siteSettings = new DataCacheCollection<SystemSiteSettings>
        {
            Expiration = TimeSpan.FromMinutes(10)
        };

        /// <summary>
        /// The user groups.
        /// </summary>
        protected ConcurrentBag<UserGroupRelationshipEntity> JoinedGroupsCache
        {
            get
            {
                var time = GroupsCacheTime;
                if (!time.HasValue || groupsCache == null || (DateTime.Now - time.Value).TotalMinutes > (isLongCacheDuration ? 4 : 2)) return null;
                return groupsCache;
            }

            set
            {
                groupsCache = value;
                GroupsCacheTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use long cache duration
        /// </summary>
        protected bool IsLongCacheDuration
        {
            get
            {
                return isLongCacheDuration;
            }

            set
            {
                if (isLongCacheDuration == value) return;
                isLongCacheDuration = value;
                settings.Expiration = value ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(3);
                siteSettings.Expiration = value ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(10);
            }
        }

        /// <summary>
        /// Gets the cache date time of groups.
        /// </summary>
        public DateTime? GroupsCacheTime { get; private set; }

        /// <summary>
        /// Gets the user identifier signed in.
        /// </summary>
        public string UserId { get; protected set; }

        /// <summary>
        /// Gets the client identifier used to login.
        /// </summary>
        public string ClientId { get; protected set; }

        /// <summary>
        /// Gets the client verified which is used to login.
        /// </summary>
        public AccessingClientEntity ClientVerified { get; protected set; }

        /// <summary>
        /// Gets the a value indicating whether the client used to login is verified.
        /// </summary>
        public bool IsClientCredentialVerified => ClientVerified != null;

        /// <summary>
        /// Gets the token request route instance.
        /// </summary>
        public TokenRequestRoute<UserEntity> TokenRequestRoute
        {
            get
            {
                if (route != null) return route;
                route = new TokenRequestRoute<UserEntity>();
                route.Register(PasswordTokenRequestBody.PasswordGrantType, q =>
                {
                    return PasswordTokenRequestBody.Parse(q.ToString());
                }, async q =>
                {
                    var r = await SignInAsync(q);
                    return (r.User, r);
                });
                route.Register(RefreshTokenRequestBody.RefreshTokenGrantType, q =>
                {
                    return RefreshTokenRequestBody.Parse(q.ToString());
                }, async q =>
                {
                    var r = await SignInAsync(q);
                    return (r.User, r);
                });
                route.Register(CodeTokenRequestBody.AuthorizationCodeGrantType, q =>
                {
                    return CodeTokenRequestBody.Parse(q.ToString());
                }, async q =>
                {
                    var r = await SignInAsync(q);
                    return (r.User, r);
                });
                route.Register(ClientTokenRequestBody.ClientCredentialsGrantType, q =>
                {
                    return ClientTokenRequestBody.Parse(q.ToString());
                }, async q =>
                {
                    var r = await SignInAsync(q);
                    return (null, r);
                });

                return route;
            }
        }

        /// <summary>
        /// Gets or sets the token cache.
        /// </summary>
        public new UserTokenInfo Token
        {
            get => base.Token as UserTokenInfo;
            set => base.Token = value;
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        public UserEntity User => Token?.User;

        /// <summary>
        /// Gets the user information.
        /// </summary>
        public bool IsUserSignedIn => !IsTokenNullOrEmpty && !string.IsNullOrWhiteSpace(UserId) && Token?.User != null;

        /// <summary>
        /// Gets the site identifier collection of permission cache.
        /// </summary>
        public IEnumerable<string> SiteIdsOfPermissionCache => permissions.Keys;

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="utf8Stream">The UTF-8 stream input.</param>
        /// <returns>The login response.</returns>
        public async Task<UserTokenInfo> SignInAsync(Stream utf8Stream)
        {
            try
            {
                var r = await TokenRequestRoute.SignInAsync(utf8Stream);
                return r.ItemSelected as UserTokenInfo;
            }
            catch (ArgumentException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                    ErrorDescription = ex.Message
                };
            }
            catch (IOException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }

        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="input">The input query data.</param>
        /// <returns>The login response.</returns>
        public async Task<UserTokenInfo> SignInAsync(QueryData input)
        {
            var r = await TokenRequestRoute.SignInAsync(input);
            return r.ItemSelected as UserTokenInfo;
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The login response.</returns>
        public async Task<UserTokenInfo> SignInAsync(string input)
        {
            var r = await TokenRequestRoute.SignInAsync(input);
            return r.ItemSelected as UserTokenInfo;
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> SignInAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="requestBody">The token request body.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInAsync(AppAccessingKey appKey, PasswordTokenRequestBody requestBody, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<PasswordTokenRequestBody>(requestBody, appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="logname">The user login name.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInByPasswordAsync(AppAccessingKey appKey, string logname, SecureString password, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<PasswordTokenRequestBody>(new PasswordTokenRequestBody(logname, password), appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="logname">The user login name.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInByPasswordAsync(AppAccessingKey appKey, string logname, string password, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<PasswordTokenRequestBody>(new PasswordTokenRequestBody(logname, password), appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> SignInAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="requestBody">The token request body.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInAsync(AppAccessingKey appKey, RefreshTokenRequestBody requestBody, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<RefreshTokenRequestBody>(requestBody, appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInByRefreshTokenAsync(AppAccessingKey appKey, string refreshToken, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<RefreshTokenRequestBody>(new RefreshTokenRequestBody(refreshToken), appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInByRefreshTokenAsync(AppAccessingKey appKey, SecureString refreshToken, CancellationToken cancellationToken = default)
        {
            return SignInByRefreshTokenAsync(appKey, refreshToken.ToUnsecureString(), cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> SignInAsync(TokenRequest<CodeTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="requestBody">The token request body.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInAsync(AppAccessingKey appKey, CodeTokenRequestBody requestBody, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<CodeTokenRequestBody>(requestBody, appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="code">The authorization code token.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInByAutherizationCodeWithProviderAsync(AppAccessingKey appKey, string code, string serviceProvider, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<CodeTokenRequestBody>(new CodeTokenRequestBody(code), appKey);
            req.Body.ServiceProvider = serviceProvider;
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="code">The authorization code token.</param>
        /// <param name="verifier">The code verifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInByAutherizationCodeWithVerifierAsync(AppAccessingKey appKey, string code, string verifier, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<CodeTokenRequestBody>(new CodeTokenRequestBody(code), appKey);
            req.Body.CodeVerifier = verifier;
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> SignInAsync(TokenRequest<ClientTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="requestBody">The token request body.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInAsync(AppAccessingKey appKey, ClientTokenRequestBody requestBody, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<ClientTokenRequestBody>(requestBody, appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="appKey">The app accessing key.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> SignInByClientAsync(AppAccessingKey appKey, CancellationToken cancellationToken = default)
        {
            var req = new TokenRequest<ClientTokenRequestBody>(new ClientTokenRequestBody(), appKey);
            return SignInAsync(req, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="accessToken">The access request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> AuthorizeAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public async Task<UserTokenInfo> AuthorizeAsync<T>(IDictionary<string, T> header, CancellationToken cancellationToken = default) where T : IEnumerable<string>
        {
            if (header is null || !header.TryGetValue("Authorization", out var col)) return new UserTokenInfo
            {
                ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                ErrorDescription = "No authorization information."
            };
            var token = col.FirstOrDefault(ele => !string.IsNullOrWhiteSpace(ele));
            if (string.IsNullOrWhiteSpace(token)) return new UserTokenInfo
            {
                ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                ErrorDescription = "No authorization information."
            };
            return await AuthorizeAsync(token, cancellationToken);
        }

        /// <summary>
        /// Sets a new authorization code.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="code">The authorization code.</param>
        /// <param name="insertNewOne">true if need add a new one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public abstract Task<ChangeMethods> SetAuthorizationCodeAsync(string serviceProvider, string code, bool insertNewOne = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs out.
        /// </summary>
        /// <returns>The task.</returns>
        public virtual Task SignOutAsync()
        {
            return Task.Run(() =>
            {
                UserId = null;
                ClientId = null;
                ClientVerified = null;
                Token = null;
                ClearCache();
            });
        }

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The login response.</returns>
        public async Task<IEnumerable<UserGroupEntity>> GetGroupsJoinedInAsync(string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
        {
            var col = await ListRelationshipsAsync(q, relationshipState);
            if (col is null) return new List<UserGroupEntity>();
            return col.Select(ele => ele.Owner);
        }

        /// <summary>
        /// Gets the permissions.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The permission set.</returns>
        public async Task<IPermissionSet> GetPermissionsAsync(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId)) return null;
            if (permissions.TryGetValue(siteId, out var set))
            {
                if ((DateTime.Now - set.CacheTime) < (isLongCacheDuration ? TimeSpan.FromHours(2) : TimeSpan.FromMinutes(3)))
                    return set;
            }
            else
            {
                set = new UserSitePermissionSet(siteId);
                permissions[siteId] = set;
            }

            var users = GetUserPermissionsAsync(siteId);
            set.CacheTime = DateTime.Now;
            var perms = await GetGroupPermissionsAsync(siteId);
            set.UserPermission = await users;
            set.UserPermission.SetPropertiesReadonly();
            set.GroupPermissions = perms.ToList().Select(ele =>
            {
                ele.SetPropertiesReadonly();
                return ele;
            });
            return set;
        }

        /// <summary>
        /// Gets the cache date time of permission.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The cache date time.</returns>
        public DateTime? GetPermissionCacheTime(string siteId)
        {
            if (!permissions.TryGetValue(siteId, out var set)) return null;
            return set.CacheTime;
        }

        /// <summary>
        /// Clears the permission cache.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        public void ClearPermissionCache(string siteId)
        {
            permissions.Remove(siteId);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="value">The permission item to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> HasPermissionAsync(string siteId, string value)
        {
            var perms = await GetPermissionsAsync(siteId);
            return perms != null && perms.HasPermission(value);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> HasAnyPermissionAsync(string siteId, string value, params string[] otherValues)
        {
            var perms = await GetPermissionsAsync(siteId);
            return perms != null && perms.HasAnyPermission(value, otherValues);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> HasAnyPermissionAsync(string siteId, IEnumerable<string> values)
        {
            var perms = await GetPermissionsAsync(siteId);
            return perms != null && perms.HasAnyPermission(values);
        }

        /// <summary>
        /// Tests if contains the admin permissions of the system settings.
        /// </summary>
        /// <param name="siteId">The site identifier; or null for global.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> IsSystemSettingsAdminAsync(string siteId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(siteId) && await HasAnyPermissionAsync(siteId.Trim(), PermissionItems.SiteInformationManagement, PermissionItems.SiteAdmin)) return true;
            var settings = await GetSystemSettingsAsync(cancellationToken);
            var groupId = settings?.CurrentSettingsAdminGroupId?.Trim();
            if (string.IsNullOrEmpty(groupId)) return true;
            var groups = await GetGroupsJoinedInAsync();
            return groups.FirstOrDefault(ele => ele?.Id == groupId) != null;
        }

        /// <summary>
        /// Tests if contains the admin permissions of the system settings.
        /// </summary>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public Task<bool> IsSystemSettingsAdminAsync(CancellationToken cancellationToken)
        {
            return IsSystemSettingsAdminAsync(null, cancellationToken);
        }

        /// <summary>
        /// Tests if contains the admin permissions of the client.
        /// </summary>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> IsClientAdminAsync()
        {
            var c = ClientVerified;
            if (c == null) return false;
            var groupId = c.AdminGroupId;
            var has = groupId == null;
            if (!has)
            {
                var groups = await GetGroupsJoinedInAsync();
                has = groups.FirstOrDefault(ele => ele?.Id == groupId) != null;
            }

            if (has) c.UnlockPropertiesReadonly();
            return has;
        }

        /// <summary>
        /// Tests if the user has groups administrator permission.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> IsGroupsAdminAsync(string siteId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(siteId)) siteId = null;
            else siteId = siteId.Trim();
            var perms = await GetPermissionsAsync(siteId);
            if (perms != null && perms.HasAnyPermission(PermissionItems.GroupManagement, PermissionItems.SiteAdmin))
                return true;
            var settings = await GetSystemSettingsAsync(cancellationToken);
            var groupId = settings?.GroupAdminGroupId ?? settings?.CurrentSettingsAdminGroupId;
            if (string.IsNullOrWhiteSpace(groupId)) return true;
            groupId = groupId.Trim();
            var groups = await GetGroupsJoinedInAsync();
            return groups.FirstOrDefault(ele => ele?.Id == groupId) != null;
        }

        /// <summary>
        /// Tests if the user has site administrator permission.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> IsSiteAdminAsync(string siteId, CancellationToken cancellationToken = default)
        {
            if (await HasAnyPermissionAsync(siteId, PermissionItems.SiteAdmin)) return true;
            var settings = await GetSystemSettingsAsync(cancellationToken);
            var groupId = settings?.SiteAdminGroupId ?? settings?.CurrentSettingsAdminGroupId;
            if (string.IsNullOrWhiteSpace(groupId)) return true;
            var groups = await GetGroupsJoinedInAsync();
            return groups.FirstOrDefault(ele => ele?.Id == groupId) != null;
        }

        /// <summary>
        /// Tests if can set permission.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> IsPermissionAdminAsync(string siteId, CancellationToken cancellationToken = default)
        {
            if (await HasAnyPermissionAsync(siteId, PermissionItems.PermissionManagement, PermissionItems.SiteAdmin)) return true;
            var settings = await GetSystemSettingsAsync(cancellationToken);
            var groupId = settings?.SiteAdminGroupId ?? settings?.CurrentSettingsAdminGroupId;
            if (string.IsNullOrWhiteSpace(groupId)) return true;
            var groups = await GetGroupsJoinedInAsync();
            return groups.FirstOrDefault(ele => ele?.Id == groupId) != null;
        }

        /// <summary>
        /// Tests if contains the admin permissions of user.
        /// </summary>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> IsUserAdminAsync(CancellationToken cancellationToken = default)
        {
            var settings = await GetSystemSettingsAsync(cancellationToken);
            var groupId = settings?.UserAdminGroupId ?? settings?.CurrentSettingsAdminGroupId;
            if (string.IsNullOrWhiteSpace(groupId)) return true;
            groupId = groupId.Trim();
            var groups = await GetGroupsJoinedInAsync();
            return groups.FirstOrDefault(ele => ele?.Id == groupId) != null;
        }

        /// <summary>
        /// Tests if contains the admin permissions of CMS.
        /// </summary>
        /// <param name="siteId">The site identifier; or null for global.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public async Task<bool> IsCmsAdminAsync(string siteId, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(siteId) && await HasAnyPermissionAsync(siteId.Trim(), PermissionItems.CmsAdmin, PermissionItems.SiteAdmin)) return true;
            var settings = await GetSystemSettingsAsync(cancellationToken);
            var groupId = settings?.UserAdminGroupId ?? settings?.CurrentSettingsAdminGroupId;
            if (string.IsNullOrWhiteSpace(groupId)) return true;
            groupId = groupId.Trim();
            var groups = await GetGroupsJoinedInAsync();
            return groups.FirstOrDefault(ele => ele?.Id == groupId) != null;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public async Task<SettingsEntity.Model> GetSettingsAsync(string key, string siteId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (string.IsNullOrWhiteSpace(siteId)) siteId = null;
            else siteId = siteId.Trim();
            var globalKey = key + " > ";
            var hasGlobal = settings.TryGet(globalKey, out var globalModel);
            if (siteId == null)
            {
                if (hasGlobal) return new SettingsEntity.Model(key, globalModel);
                globalModel = await GetSettingsJsonStringByKeyAsync(globalKey, null, cancellationToken);
                settings[globalKey] = globalModel;
                return new SettingsEntity.Model(key, globalModel);
            }

            var siteKey = globalKey + siteId;
            var hasSite = settings.TryGet(siteKey, out var siteModel);
            if (hasSite && hasGlobal) return new SettingsEntity.Model(key, siteId, siteModel, globalModel);
            if (!hasSite && !hasGlobal)
            {
                var r = await GetSettingsModelByKeyAsync(key, siteId, cancellationToken);
                settings[globalKey] = r.GlobalConfigString;
                settings[siteKey] = r.SiteConfigString;
                return r;
            }

            if (!hasGlobal)
            {
                globalModel = await GetSettingsJsonStringByKeyAsync(globalKey, null, cancellationToken);
                settings[globalKey] = globalModel;
            }

            if (!hasSite)
            {
                siteModel = await GetSettingsJsonStringByKeyAsync(siteKey, null, cancellationToken);
                settings[siteKey] = siteModel;
            }

            return new SettingsEntity.Model(key, siteId, siteModel, globalModel);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public Task<SettingsEntity.Model> GetSettingsAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetSettingsAsync(key, null, cancellationToken);
        }

        /// <summary>
        /// Gets the system settings.
        /// </summary>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public async Task<SystemSiteSettings> GetSystemSettingsAsync(string siteId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(siteId)) return null;
            siteId = siteId.Trim();
            if (siteSettings.TryGet(siteId, out var s)) return s;
            var settings = await GetSettingsAsync("system", siteId, cancellationToken);
            s = settings?.DeserializeGlobalConfig<SystemSiteSettings>();
            s.SetPropertiesReadonly();
            siteSettings[siteId] = s;
            return s;
        }

        /// <summary>
        /// Gets the system settings.
        /// </summary>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public async Task<SystemGlobalSettings> GetSystemSettingsAsync(CancellationToken cancellationToken = default)
        {
            var s = globalSettings;
            if (s != null || globalSettingsExpiration > DateTime.Now) return s;
            var settings = await GetSettingsAsync("system", null, cancellationToken);
            s = settings?.DeserializeGlobalConfig<SystemGlobalSettings>();
            s.SetPropertiesReadonly();
            globalSettings = s;
            globalSettingsExpiration = DateTime.Now.AddMinutes(10);
            return s;
        }

        /// <summary>
        /// Gets a value indicating whether the specific user login name has been registered.
        /// </summary>
        /// <param name="logname">The user login name.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if the specific user login name has been registered; otherwise, false.</returns>
        public abstract Task<bool> HasUserNameAsync(string logname, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public abstract Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user group entity by given identifier.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public abstract Task<UserGroupEntity> GetUserGroupByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional query information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles role, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            if (!await CanViewMembersAsync(group)) return new List<UserEntity>();
            return await ListUsersByGroupAsync(group, role, q, cancellationToken);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="q">The optional query information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            if (!await CanViewMembersAsync(group)) return new List<UserEntity>();
            return await ListUsersByGroupAsync(group, null, q, cancellationToken);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles role, string q, ResourceEntityStates relationshipState = ResourceEntityStates.Normal, CancellationToken cancellationToken = default)
        {
            return ListUsersAsync(group, role, new QueryArgs
            {
                NameQuery = q,
                State = relationshipState
            }, cancellationToken);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles role, string q, CancellationToken cancellationToken)
        {
            return ListUsersAsync(group, role, new QueryArgs
            {
                NameQuery = q
            }, cancellationToken);
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional query request information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public abstract Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional query request information.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public abstract Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public async Task<IEnumerable<UserGroupRelationshipEntity>> ListRelationshipsAsync(string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal, CancellationToken cancellationToken = default)
        {
            var isForAll = relationshipState == ResourceEntityStates.Normal && string.IsNullOrEmpty(q);
            if (isForAll && JoinedGroupsCache != null) return JoinedGroupsCache;
            var col = await GetRelationshipsAsync(q, relationshipState, cancellationToken);
            if (isForAll) JoinedGroupsCache = new ConcurrentBag<UserGroupRelationshipEntity>(col);
            return col;
        }

        /// <summary>
        /// Gets the relationship between current user and the specific group.
        /// </summary>
        /// <param name="group">The group to test.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The relationship.</returns>
        public async Task<UserGroupRelationshipEntity> GetRelationshipAsync(UserGroupEntity group, CancellationToken cancellationToken = default)
        {
            var userId = User?.Id;
            if (string.IsNullOrWhiteSpace(group.Id) || string.IsNullOrWhiteSpace(userId) || group.IsNew) return null;
            return JoinedGroupsCache?.FirstOrDefault(ele => ele.OwnerId == group.Id) ?? await GetRelationshipAsync(group.Id, userId, cancellationToken);
        }

        /// <summary>
        /// Gets the relationship between the specific user and the specific group.
        /// </summary>
        /// <param name="group">The group to test.</param>
        /// <param name="user">The user to test.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The relationship.</returns>
        public async Task<UserGroupRelationshipEntity> GetRelationshipAsync(UserGroupEntity group, UserEntity user, CancellationToken cancellationToken = default)
        {
            return string.IsNullOrWhiteSpace(group.Id) || string.IsNullOrWhiteSpace(user?.Id) || group.IsNew || user.IsNew ? null : await GetRelationshipAsync(group.Id, user.Id, cancellationToken);
        }

        /// <summary>
        /// Joins in a specific group.
        /// </summary>
        /// <param name="group">The user group entity to join in.</param>
        /// <param name="role">The role to request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public async Task<ChangeMethods> JoinAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles role = UserGroupRelationshipEntity.Roles.Member, CancellationToken cancellationToken = default)
        {
            if (group == null || group.IsNew || IsTokenNullOrEmpty || string.IsNullOrWhiteSpace(UserId)) return ChangeMethods.Invalid;
            var rela = await GetRelationshipAsync(group, cancellationToken);
            if (rela != null) return ChangeMethods.Unchanged;
            if (group.MembershipPolicy == UserGroupMembershipPolicies.Forbidden) return ChangeMethods.Invalid;
            rela = new UserGroupRelationshipEntity
            {
                OwnerId = group.Id,
                TargetId = UserId,
                Role = role,
                State = ResourceEntityStates.Request,
                Name = User?.Nickname ?? User?.Name ?? UserId
            };
            if (role == UserGroupRelationshipEntity.Roles.Member && group.MembershipPolicy == UserGroupMembershipPolicies.Allow)
            {
                rela.State = ResourceEntityStates.Normal;
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await IsGroupsAdminAsync(group.OwnerSiteId, cancellationToken))
                    rela.State = ResourceEntityStates.Normal;
            }

            return await SaveEntityAsync(rela, cancellationToken);
        }

        /// <summary>
        /// Invites a user into a specific group.
        /// </summary>
        /// <param name="group">The user group entity to join in.</param>
        /// <param name="user">The user entity.</param>
        /// <param name="role">The role to request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public async Task<ChangeMethods> InviteAsync(UserGroupEntity group, UserEntity user, UserGroupRelationshipEntity.Roles role, CancellationToken cancellationToken = default)
        {
            if (group == null || group.IsNew || user == null) return ChangeMethods.Invalid;
            var userId = user.Id;
            if (!IsTokenNullOrEmpty && UserId == userId) return await JoinAsync(group, role, cancellationToken);
            var rela = await GetRelationshipAsync(group, cancellationToken);
            var isAdmin = false;
            if (rela == null || rela.State != ResourceEntityStates.Normal)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!await IsGroupsAdminAsync(group.OwnerSiteId, cancellationToken))
                    return ChangeMethods.Invalid;
            }
            else
            {
                isAdmin = rela.Role switch
                {
                    UserGroupRelationshipEntity.Roles.Owner => true,
                    UserGroupRelationshipEntity.Roles.Master => true,
                    _ => false
                };

                if (!isAdmin)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    isAdmin = await IsGroupsAdminAsync(group.OwnerSiteId, cancellationToken);
                }
            }

            if (!isAdmin && group.MembershipPolicy != UserGroupMembershipPolicies.Allow) return ChangeMethods.Invalid;
            rela = string.IsNullOrWhiteSpace(userId) ? null : await GetRelationshipAsync(group.Id, userId, cancellationToken);
            if (rela == null)
            {
                rela = new UserGroupRelationshipEntity
                {
                    OwnerId = group.Id,
                    TargetId = user.Id,
                    Role = UserGroupRelationshipEntity.Roles.Member,
                    State = ResourceEntityStates.Normal,
                    Name = user.Nickname ?? user.Name ?? userId
                };
            }
            else
            {
                rela.State = ResourceEntityStates.Normal;
                var nickName = user.Nickname ?? user.Name;
                if (!string.IsNullOrWhiteSpace(nickName)) rela.Name = nickName;
            }

            if (isAdmin) rela.Role = role;
            return await SaveEntityAsync(rela, cancellationToken);
        }

        /// <summary>
        /// Invites a user into a specific group.
        /// </summary>
        /// <param name="group">The user group entity to join in.</param>
        /// <param name="user">The user entity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public Task<ChangeMethods> InviteAsync(UserGroupEntity group, UserEntity user, CancellationToken cancellationToken = default)
        {
            return InviteAsync(group, user, UserGroupRelationshipEntity.Roles.Member, cancellationToken);
        }

        /// <summary>
        /// Invites a user into a specific group.
        /// </summary>
        /// <param name="group">The user group entity to join in.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="role">The role to request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public async Task<ChangeMethods> InviteAsync(UserGroupEntity group, string userId, UserGroupRelationshipEntity.Roles role = UserGroupRelationshipEntity.Roles.Member, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId)) return ChangeMethods.Invalid;
            var user = await GetUserByIdAsync(userId, cancellationToken);
            return await InviteAsync(group, user, role, cancellationToken);
        }

        /// <summary>
        /// Invites a user into a specific group.
        /// </summary>
        /// <param name="group">The user group entity to join in.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public Task<ChangeMethods> InviteAsync(UserGroupEntity group, string userId, CancellationToken cancellationToken = default)
        {
            return InviteAsync(group, userId, UserGroupRelationshipEntity.Roles.Member, cancellationToken);
        }

        /// <summary>
        /// Registers a new user or update current user.
        /// </summary>
        /// <param name="value">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public async Task<ChangeMethods> SaveAsync(UserEntity value, CancellationToken cancellationToken = default)
        {
            var isCurrentUser = value.Id == UserId;
            if (string.IsNullOrWhiteSpace(value.PasswordEncrypted) && value.ExtensionSerializationData != null && value.ExtensionSerializationData.TryGetValue("password", out var password) == true && password.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var pswdStr = password.GetString() ?? string.Empty;
                if (pswdStr.Length > 5) value.SetPassword(pswdStr);
            }

            if (value.IsNew)
            {
                var settings = await GetSystemSettingsAsync(cancellationToken);
                if (settings.ForbidUserRegister && !await IsUserAdminAsync(cancellationToken)) return ChangeMethods.Invalid;
                if (await HasUserNameAsync(value.Name, cancellationToken)) return ChangeMethods.Invalid;
            }
            else
            {
                if (!isCurrentUser && !await IsUserAdminAsync(cancellationToken)) return ChangeMethods.Invalid;
                if (string.IsNullOrWhiteSpace(value.PasswordEncrypted))
                {
                    var u = await GetUserByIdAsync(value.Id, cancellationToken);
                    if (u != null) value.PasswordEncrypted = u.PasswordEncrypted;
                }
            }

            try
            {
                if (string.IsNullOrWhiteSpace(value.Market))
                {
                    var culture = Thread.CurrentThread.CurrentUICulture ?? Thread.CurrentThread.CurrentCulture;
                    if (culture != null) value.Market = culture.Name;
                }
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }

            return await SaveEntityAsync(value, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="value">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public async Task<ChangeMethods> SaveAsync(UserGroupEntity value, CancellationToken cancellationToken = default)
        {
            if (value == null) return ChangeMethods.Invalid;
            if (value.IsNew)
            {
                if (!await IsGroupsAdminAsync(value.OwnerSiteId, cancellationToken))
                    return ChangeMethods.Invalid;
                return await SaveEntityAsync(value, cancellationToken);
            }

            var groups = await ListRelationshipsAsync();
            foreach (var g in groups)
            {
                if (g == null || g.OwnerId != value.Id) continue;
                return g.Role switch
                {
                    UserGroupRelationshipEntity.Roles.Owner => await SaveEntityAsync(value, cancellationToken),
                    UserGroupRelationshipEntity.Roles.Master => await SaveEntityAsync(value, cancellationToken),
                    _ => ChangeMethods.Unchanged
                };
            }

            return ChangeMethods.Unchanged;
        }

        /// <summary>
        /// Creates or updates a user group relationship entity.
        /// </summary>
        /// <param name="value">The user group relationship entity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public async Task<ChangeMethods> SaveAsync(UserGroupRelationshipEntity value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(value?.OwnerId)) return ChangeMethods.Invalid;
            if (value.Owner == null) value.Owner = await GetUserGroupByIdAsync(value.OwnerId, cancellationToken);
            var group = value.Owner;
            var rela = await GetRelationshipAsync(group, cancellationToken);
            var isAdmin = rela == null || rela.State != ResourceEntityStates.Normal
                ? await IsGroupsAdminAsync(group.OwnerSiteId, cancellationToken)
                : rela.Role switch
                {
                    UserGroupRelationshipEntity.Roles.Owner => true,
                    UserGroupRelationshipEntity.Roles.Master => true,
                    _ => await IsGroupsAdminAsync(group.OwnerSiteId, cancellationToken)
                };
            if (!isAdmin) return ChangeMethods.Invalid;
            return await SaveEntityAsync(value, cancellationToken);
        }

        /// <summary>
        /// Checks if current user is the admin of a specific group.
        /// </summary>
        /// <param name="group">The user group entity to test.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public async Task<bool> IsGroupAdminAsync(UserGroupEntity group, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(group.Id)) return false;
            var rela = await GetRelationshipAsync(group, cancellationToken);
            if (rela == null || rela.State != ResourceEntityStates.Normal)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await IsGroupsAdminAsync(group.OwnerSiteId, cancellationToken);
            }

            return rela.Role switch
            {
                UserGroupRelationshipEntity.Roles.Owner => true,
                UserGroupRelationshipEntity.Roles.Master => true,
                _ => await IsGroupsAdminAsync(group.OwnerSiteId, cancellationToken)
            };
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
        public abstract Task<ChangeMethods> SavePermissionAsync(string siteId, SecurityEntityTypes targetType, string targetId, IEnumerable<string> permissionList, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears cache.
        /// </summary>
        public void ClearCache()
        {
            groupsCache = null;
            GroupsCacheTime = null;
            permissions.Clear();
            globalSettings = null;
            siteSettings.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether the group members can be visible.
        /// </summary>
        /// <param name="group">The group entity.</param>
        /// <returns>true if can view members; otherwise, false.</returns>
        public async Task<bool> CanViewMembersAsync(UserGroupEntity group)
        {
            if (group.Visibility == UserGroupVisibilities.Visible) return true;
            if (IsTokenNullOrEmpty || string.IsNullOrWhiteSpace(UserId)) return false;
            var g = JoinedGroupsCache;
            if (g == null)
            {
                var rela = await GetRelationshipAsync(group.Id, UserId);
                return rela != null;
            }

            return g.FirstOrDefault(ele => ele.OwnerId == UserId) != null;
        }

        /// <summary>
        /// Renews the client app key.
        /// </summary>
        /// <param name="appId">The client app identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The client app identifier and secret key.</returns>
        public abstract Task<AppAccessingKey> RenewAppClientKeyAsync(string appId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public abstract Task<ChangeMethods> SaveSettingsAsync(string key, string siteId, JsonObject value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        protected abstract Task<SettingsEntity.Model> GetSettingsModelByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        protected abstract Task<JsonObject> GetSettingsDataByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        protected abstract Task<string> GetSettingsJsonStringByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional query information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        protected abstract Task<IEnumerable<UserEntity>> ListUsersByGroupAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the user permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user permission list.</returns>
        protected abstract Task<UserPermissionItemEntity> GetUserPermissionsAsync(string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the user group permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group permission list.</returns>
        protected abstract Task<IEnumerable<UserGroupPermissionItemEntity>> GetGroupPermissionsAsync(string siteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the client permissions of the current client.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The client permission list.</returns>
        protected abstract Task<ClientPermissionItemEntity> GetClientPermissionsAsync(string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group relationships.</returns>
        protected abstract Task<IEnumerable<UserGroupRelationshipEntity>> GetRelationshipsAsync(string q, ResourceEntityStates relationshipState, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="groupId">The user group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        protected abstract Task<UserGroupRelationshipEntity> GetRelationshipAsync(string groupId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a user entity.
        /// </summary>
        /// <param name="value">The user entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        protected abstract Task<ChangeMethods> SaveEntityAsync(UserEntity value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="value">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        protected abstract Task<ChangeMethods> SaveEntityAsync(UserGroupEntity value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a user group relationship entity.
        /// </summary>
        /// <param name="value">The user group relationship entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        protected abstract Task<ChangeMethods> SaveEntityAsync(UserGroupRelationshipEntity value, CancellationToken cancellationToken = default);
    }
}
