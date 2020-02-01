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
    /// The base resource access client.
    /// </summary>
    public abstract class BaseResourceAccessClient : TokenContainer
    {
        /// <summary>
        /// The token request route instance.
        /// </summary>
        private TokenRequestRoute<UserEntity> route;

        /// <summary>
        /// The user groups.
        /// </summary>
        private IEnumerable<UserGroupResourceEntity<UserEntity>> groups;

        /// <summary>
        /// The permissions set.
        /// </summary>
        private readonly Dictionary<string, UserSitePermissionSet> permissions = new Dictionary<string, UserSitePermissionSet>();

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
                    var r = await LoginAsync(q);
                    return (r.User, r);
                });
                route.Register(RefreshTokenRequestBody.RefreshTokenGrantType, q =>
                {
                    return RefreshTokenRequestBody.Parse(q.ToString());
                }, async q =>
                {
                    var r = await LoginAsync(q);
                    return (r.User, r);
                });
                route.Register(CodeTokenRequestBody.AuthorizationCodeGrantType, q =>
                {
                    return CodeTokenRequestBody.Parse(q.ToString());
                }, async q =>
                {
                    var r = await LoginAsync(q);
                    return (r.User, r);
                });
                route.Register(ClientTokenRequestBody.ClientCredentialsGrantType, q =>
                {
                    return ClientTokenRequestBody.Parse(q.ToString());
                }, async q =>
                {
                    var r = await LoginAsync(q);
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
        /// Gets the site identifier collection of permission cache.
        /// </summary>
        public IEnumerable<string> SiteIdsOfPermissionCache => permissions.Keys;

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<CodeTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<ClientTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="accessToken">The access request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> AuthorizeAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs out.
        /// </summary>
        /// <returns>The task.</returns>
        public virtual Task LogoutAsync()
        {
            return Task.Run(() =>
            {
                ClearCache();
                Token = null;
            });
        }

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The login response.</returns>
        public IEnumerable<UserGroupResourceEntity<UserEntity>> GetGroups(string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
        {
            var isForAll = relationshipState == ResourceEntityStates.Normal && string.IsNullOrEmpty(q);
            if (isForAll && groups != null) return groups;
            var col = GetGroups(q, relationshipState);
            if (isForAll) groups = col;
            return col;
        }

        /// <summary>
        /// Gets the permissions.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The permission set.</returns>
        public async Task<IPermissionSet> GetPermissionsAsync(string siteId)
        {
            if (permissions.TryGetValue(siteId, out var set))
            {
                if (DateTime.Now - set.CacheTime < TimeSpan.FromMinutes(2)) return set;
            }
            else
            {
                set = new UserSitePermissionSet(siteId);
                permissions[siteId] = set;
            }

            set.CacheTime = DateTime.Now;
            var userTask = Task.Run(() =>
            {
                set.UserPermissions = GetUserPermissions(siteId).ToList();
            });
            var groupTask = Task.Run(() =>
            {
                set.GroupPermissions = GetGroupPermissions(siteId).ToList();
            });
            await userTask;
            await groupTask;
            return set;
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
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The login response.</returns>
        protected abstract IEnumerable<UserGroupResourceEntity<UserEntity>> GetGroupsFromDataSource(string q, ResourceEntityStates relationshipState);

        /// <summary>
        /// Gets the user permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The user permission list.</returns>
        protected abstract IEnumerable<UserPermissionItemEntity> GetUserPermissions(string siteId);

        /// <summary>
        /// Gets the user group permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The user group permission list.</returns>
        protected abstract IEnumerable<UserGroupPermissionItemEntity> GetGroupPermissions(string siteId);

        /// <summary>
        /// Clears cache.
        /// </summary>
        public void ClearCache()
        {
            groups = null;
        }
    }
}
