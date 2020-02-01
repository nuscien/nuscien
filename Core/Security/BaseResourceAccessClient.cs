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
        private IEnumerable<UserGroupRelationshipEntity> groups;

        /// <summary>
        /// The permissions set.
        /// </summary>
        private readonly Dictionary<string, UserSitePermissionSet> permissions = new Dictionary<string, UserSitePermissionSet>();

        /// <summary>
        /// Gets the cache date time of groups.
        /// </summary>
        public DateTime? GroupsCacheTime { get; private set; }

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
        public async Task<IEnumerable<UserGroupEntity>> GetGroupsAsync(string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
        {
            var col = await GetGroupRelationshipsAsync(q, relationshipState);
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
            if (permissions.TryGetValue(siteId, out var set))
            {
                if (DateTime.Now - set.CacheTime < TimeSpan.FromMinutes(2)) return set;
            }
            else
            {
                set = new UserSitePermissionSet(siteId);
                permissions[siteId] = set;
            }

            var users = GetUserPermissionsAsync(siteId);
            set.CacheTime = DateTime.Now;
            set.GroupPermissions = await GetGroupPermissionsAsync(siteId);
            set.UserPermissions = await users;
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
        public abstract Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles role, QueryArgs q = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="q">The optional query information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public abstract Task<IEnumerable<UserEntity>> ListUsersAsync(UserGroupEntity group, QueryArgs q = null, CancellationToken cancellationToken = default);

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
        /// Joins in a specific group.
        /// </summary>
        /// <param name="group">The user group entity to join in.</param>
        /// <param name="role">The role to request.</param>
        /// <returns>The user group relationship entity.</returns>
        public async Task<ChangeMethods> JoinAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles role = UserGroupRelationshipEntity.Roles.Member)
        {
            var t = Token;
            if (group == null || group.IsNew || t == null || t.IsEmpty || string.IsNullOrWhiteSpace(t.UserId)) return ChangeMethods.Invalid;
            var rela = await GetGroupRelationshipAsync(group);
            if (rela != null) return ChangeMethods.Unchanged;
            rela = new UserGroupRelationshipEntity
            {
                OwnerId = group.Id,
                TargetId = t.UserId,
                Role = role,
                State = ResourceEntityStates.Request,
                Name = User?.Nickname ?? User?.Name ?? t.UserId
            };
            var perms = await GetPermissionsAsync(group.OwnerSiteId);
            if (perms.HasAnyPermission(PermissionItems.GroupManagement, PermissionItems.SiteAdmin))
                rela.State = ResourceEntityStates.Normal;
            return await SaveEntityAsync(rela);
        }

        /// <summary>
        /// Invites a user into a specific group.
        /// </summary>
        /// <param name="group">The user group entity to join in.</param>
        /// <param name="user">The user entity.</param>
        /// <param name="role">The role to request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group relationship entity.</returns>
        public async Task<ChangeMethods> InviteAsync(UserGroupEntity group, UserEntity user, UserGroupRelationshipEntity.Roles role = UserGroupRelationshipEntity.Roles.Member, CancellationToken cancellationToken = default)
        {
            var t = Token;
            if (group == null || group.IsNew || user == null) return ChangeMethods.Invalid;
            var userId = user.Id;
            if (t != null && t.UserId == userId) return await JoinAsync(group, role);
            var rela = await GetGroupRelationshipAsync(group);
            var isAdmin = false;
            if (rela == null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var perms = await GetPermissionsAsync(group.OwnerSiteId);
                if (!perms.HasAnyPermission(PermissionItems.GroupManagement, PermissionItems.SiteAdmin))
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
            }

            if (!isAdmin && rela.Role != UserGroupRelationshipEntity.Roles.Member)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var perms = await GetPermissionsAsync(group.OwnerSiteId);
                isAdmin = perms.HasAnyPermission(PermissionItems.GroupManagement, PermissionItems.SiteAdmin);
            }

            rela = await GetRelationshipAsync(userId, group.Id, cancellationToken);
            if (rela == null)
            {
                rela = new UserGroupRelationshipEntity
                {
                    OwnerId = group.Id,
                    TargetId = t.UserId,
                    Role = UserGroupRelationshipEntity.Roles.Member,
                    State = ResourceEntityStates.Normal,
                    Name = user.Nickname ?? user.Name ?? userId
                };
            }
            else
            {
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
        /// <param name="userId">The user identifier.</param>
        /// <param name="role">The role to request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group relationship entity.</returns>
        public async Task<ChangeMethods> InviteAsync(UserGroupEntity group, string userId, UserGroupRelationshipEntity.Roles role = UserGroupRelationshipEntity.Roles.Member, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId)) return ChangeMethods.Invalid;
            var user = await GetUserByIdAsync(userId, cancellationToken);
            return await InviteAsync(group, user, role, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="value">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<ChangeMethods> Save(UserGroupEntity value, CancellationToken cancellationToken = default)
        {
            if (value == null) return ChangeMethods.Invalid;
            if (value.IsNew)
            {
                var perms = await GetPermissionsAsync(value.OwnerSiteId);
                if (!perms.HasAnyPermission(PermissionItems.GroupManagement, PermissionItems.SiteAdmin))
                    return ChangeMethods.Invalid;
                return await SaveEntityAsync(value, cancellationToken);
            }

            var groups = await GetGroupRelationshipsAsync();
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
        /// Clears cache.
        /// </summary>
        public void ClearCache()
        {
            groups = null;
            GroupsCacheTime = null;
            permissions.Clear();
        }

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        protected abstract Task<IEnumerable<UserGroupRelationshipEntity>> GetUserGroupRelationshipsAsync(string q, ResourceEntityStates relationshipState, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the user permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user permission list.</returns>
        protected abstract Task<IEnumerable<UserPermissionItemEntity>> GetUserPermissionsAsync(string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the user group permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group permission list.</returns>
        protected abstract Task<IEnumerable<UserGroupPermissionItemEntity>> GetGroupPermissionsAsync(string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="id">The user group relationship entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        protected abstract Task<UserGroupRelationshipEntity> GetRelationshipAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="groupId">The user group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        protected abstract Task<UserGroupRelationshipEntity> GetRelationshipAsync(string groupId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="value">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        protected abstract Task<ChangeMethods> SaveEntityAsync(UserGroupEntity value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a user group relationship entity.
        /// </summary>
        /// <param name="value">The user group relationship entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        protected abstract Task<ChangeMethods> SaveEntityAsync(UserGroupRelationshipEntity value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The login response.</returns>
        private async Task<IEnumerable<UserGroupRelationshipEntity>> GetGroupRelationshipsAsync(string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
        {
            var isForAll = relationshipState == ResourceEntityStates.Normal && string.IsNullOrEmpty(q);
            if (isForAll && groups != null) return groups;
            var col = await GetUserGroupRelationshipsAsync(q, relationshipState);
            GroupsCacheTime = DateTime.Now;
            if (isForAll) groups = col;
            return col;
        }

        /// <summary>
        /// Gets the user group relationship joined in.
        /// </summary>
        /// <param name="group">The group entity.</param>
        /// <returns>The login response.</returns>
        private async Task<UserGroupRelationshipEntity> GetGroupRelationshipAsync(UserGroupEntity group)
        {
            if (group == null) return null;
            var col = await GetGroupRelationshipsAsync();
            if (col is null) return null;
            return col.FirstOrDefault(ele => ele.OwnerId == group.Id);
        }
    }
}
