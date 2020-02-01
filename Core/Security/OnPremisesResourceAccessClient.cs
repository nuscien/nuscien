using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;

namespace NuScien.Security
{
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
        /// Initializes a new instance of the OnPremisesResourceAccessClient class.
        /// </summary>
        /// <param name="provider">The account data provider.</param>
        public OnPremisesResourceAccessClient(IAccountDataProvider provider)
        {
            DataProvider = provider ?? new FakeAccountDbSetProvider();
        }

        /// <summary>
        /// Gets the account data provider.
        /// </summary>
        protected IAccountDataProvider DataProvider { get; }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            var eui = AssertTokenRequest(tokenRequest);
            if (eui != null) return eui;
            if (tokenRequest.Body.Password.Length < 1) return new UserTokenInfo
            {
                ErrorCode = "invalid_password",
                ErrorDescription = "The password should not be null."
            };
            var userTask = DataProvider.GetUserByLognameAsync(tokenRequest.Body.UserName, cancellationToken);
            return await CreateTokenAsync(userTask, tokenRequest, user =>
            {
                if (user.ValidatePassword(tokenRequest.Body.Password)) return null;
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
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
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
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<CodeTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            var eui = AssertTokenRequest(tokenRequest);
            if (eui != null) return eui;
            AuthorizationCodeEntity code;
            try
            {
                code = await DataProvider.GetAuthorizationCodeByCodeAsync(tokenRequest.Body.ServiceProvider, tokenRequest.Body.Code);
            }
            catch (ArgumentException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
            }
            catch (InvalidOperationException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotImplementedException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NullReferenceException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotSupportedException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
            }

            return code.OwnerType switch
            {
                SecurityEntityTypes.User => await CreateTokenAsync(DataProvider.GetUserByIdAsync(code.OwnerId, cancellationToken), tokenRequest, null, cancellationToken),
                SecurityEntityTypes.ServiceClient => await CreateTokenAsync(null as UserEntity, tokenRequest, cancellationToken),
                _ => new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
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
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<ClientTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            var eui = AssertTokenRequest(tokenRequest);
            if (eui != null) return eui;
            try
            {
                var client = await DataProvider.GetClientByNameAsync(tokenRequest.ClientId);
                if (client == null) return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.UnauthorizedClient,
                    ErrorDescription = "The client app identifier or secret key is incorrect."
                };
            }
            catch (ArgumentException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
            }
            catch (InvalidOperationException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotImplementedException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NullReferenceException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotSupportedException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
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
        /// Signs out.
        /// </summary>
        /// <returns>The task.</returns>
        public override async Task LogoutAsync()
        {
            var t = Token;
            if (t == null || t.IsEmpty) return;
            Task<int> task = null;
            try
            {
                await DataProvider.DeleteAccessToken(t.AccessToken);
                var uId = t.UserId;
                if (!string.IsNullOrWhiteSpace(uId))
                    task = DataProvider.DeleteExpiredClientTokensAsync(uId);
            }
            finally
            {
                await base.LogoutAsync();
            }

            if (task != null) await task;
        }

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public override async Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
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
        /// <param name="q">The optional query information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public override async Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles role, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            if (q == null) q = InternalAssertion.DefaultQueryArgs;
            var col = await DataProvider.ListUsersAsync(group, role, q, cancellationToken);
            if (col == null) return new List<UserEntity>();
            return col.Select(ele => ele.Target);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="q">The optional query information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public override async Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            if (q == null) q = InternalAssertion.DefaultQueryArgs;
            var col = await DataProvider.ListUsersAsync(group, null, q, cancellationToken);
            if (col == null) return new List<UserEntity>();
            return col.Select(ele => ele.Target);
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
        /// Gets user groups.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        protected override Task<IEnumerable<UserGroupRelationshipEntity>> GetUserGroupRelationshipsAsync(string q, ResourceEntityStates relationshipState, CancellationToken cancellationToken = default)
        {
            return DataProvider.ListUserGroupsAsync(User, q, relationshipState, cancellationToken);
        }

        /// <summary>
        /// Gets the user permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user permission list.</returns>
        protected override Task<IEnumerable<UserPermissionItemEntity>> GetUserPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
        {
            return DataProvider.ListUserPermissionsAsync(User, siteId);
        }

        /// <summary>
        /// Gets the user group permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group permission list.</returns>
        protected override Task<IEnumerable<UserGroupPermissionItemEntity>> GetGroupPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
        {
            return DataProvider.ListGroupPermissionsAsync(User, siteId);
        }

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="id">The user group relationship entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        protected override Task<UserGroupRelationshipEntity> GetRelationshipAsync(string id, CancellationToken cancellationToken = default)
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
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="value">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        protected override Task<ChangeMethods> SaveEntityAsync(UserGroupEntity value, CancellationToken cancellationToken = default)
        {
            return DataProvider.SaveAsync(value, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user group relationship entity.
        /// </summary>
        /// <param name="value">The user group relationship entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        protected override Task<ChangeMethods> SaveEntityAsync(UserGroupRelationshipEntity value, CancellationToken cancellationToken = default)
        {
            return DataProvider.SaveAsync(value, cancellationToken);
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
            var clientId = tokenRequest.ClientId;
            var resId = user is null ? clientId : user.Id;
            if (needSave)
            {
                token = new TokenEntity
                {
                    GrantType = tokenRequest.GrantType,
                    UserId = user?.Id,
                    ClientId = clientId
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

            try
            {
                if (needSave)
                {
                    token.ScopeString = tokenRequest.ScopeString;
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
                return UserTokenInfo.CreateError(user, clientId, ex);
            }
            catch (InvalidOperationException ex)
            {
                return UserTokenInfo.CreateError(user, clientId, ex);
            }
            catch (NotImplementedException ex)
            {
                return UserTokenInfo.CreateError(user, clientId, ex);
            }
            catch (NullReferenceException ex)
            {
                return UserTokenInfo.CreateError(user, clientId, ex);
            }
            catch (NotSupportedException ex)
            {
                return UserTokenInfo.CreateError(user, clientId, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                return UserTokenInfo.CreateError(user, clientId, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
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
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
            }
            catch (InvalidOperationException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotImplementedException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NullReferenceException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotSupportedException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
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

        private UserTokenInfo CheckUser(UserEntity user)
        {
            if (user is null || user.IsNew || string.IsNullOrWhiteSpace(user.Name)) return new UserTokenInfo
            {
                ErrorCode = InvalidPasswordCode,
                ErrorDescription = "The login name or password is not correct."
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
                if (token is null) return null;
                if (token.IsExpired)
                {
                    await DataProvider.DeleteExpiredTokensAsync(token.UserId, cancellationToken);
                    return null;
                }

                user = await DataProvider.GetUserByIdAsync(token.UserId, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
            }
            catch (InvalidOperationException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotImplementedException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NullReferenceException ex)
            {
                return UserTokenInfo.CreateError(null, ex);
            }
            catch (NotSupportedException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.InvalidRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                return UserTokenInfo.CreateError(null, ex, TokenInfo.ErrorCodeConstants.AccessDenied);
            }

            if (token.IsClosedToExpiration)
            {
                if (string.IsNullOrWhiteSpace(tokenRequest.ClientId)) tokenRequest.ClientId = token.ClientId;
                token = null;
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
}
