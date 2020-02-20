using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Configurations;
using NuScien.Data;
using NuScien.Users;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The data provider of the account service.
    /// </summary>
    public class MemoryAccountDbSetProvider : IAccountDataProvider
    {
        /// <summary>
        /// The user database set.
        /// </summary>
        private readonly List<UserEntity> users = new List<UserEntity>();

        /// <summary>
        /// The user database set.
        /// </summary>
        private readonly List<UserGroupEntity> groups = new List<UserGroupEntity>();

        /// <summary>
        /// The client database set.
        /// </summary>
        private readonly List<AccessingClientEntity> clients = new List<AccessingClientEntity>();

        /// <summary>
        /// The authorization code database set.
        /// </summary>
        private readonly List<AuthorizationCodeEntity> codes = new List<AuthorizationCodeEntity>();

        /// <summary>
        /// The user database set.
        /// </summary>
        private readonly List<TokenEntity> tokens = new List<TokenEntity>();

        /// <summary>
        /// The user group relationship database set.
        /// </summary>
        private readonly List<UserGroupRelationshipEntity> relationships = new List<UserGroupRelationshipEntity>();

        /// <summary>
        /// The user permissions database set.
        /// </summary>
        private readonly List<UserPermissionItemEntity> userPermissions = new List<UserPermissionItemEntity>();

        /// <summary>
        /// The user group permissions database set.
        /// </summary>
        private readonly List<UserGroupPermissionItemEntity> groupPermissions = new List<UserGroupPermissionItemEntity>();
        /// <summary>
        /// The client permissions database set.
        /// </summary>
        private readonly List<ClientPermissionItemEntity> clientPermissions = new List<ClientPermissionItemEntity>();

        /// <summary>
        /// The settings database set.
        /// </summary>
        private readonly List<SettingsEntity> settings = new List<SettingsEntity>();

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(users, ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a user entity by given login name.
        /// </summary>
        /// <param name="loginName">The login name of the user.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByLognameAsync(string loginName, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(users, ele => ele.Name == loginName, cancellationToken);
        }

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="appId">The client credential name, aka app identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AccessingClientEntity> GetClientByNameAsync(string appId, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(clients, ele => ele.Name == appId, cancellationToken);
        }

        /// <summary>
        /// Gets a client credential by accessing client entity identifier.
        /// </summary>
        /// <param name="id">The client entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AccessingClientEntity> GetClientByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(clients, ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="provider">The provider name or url.</param>
        /// <param name="code">The authorization code.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AuthorizationCodeEntity> GetAuthorizationCodeByCodeAsync(string provider, string code, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(codes, ele => ele.Name == code && ele.ServiceProvider == provider, cancellationToken);
        }

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="provider">The provider name or url.</param>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public IEnumerable<AuthorizationCodeEntity> GetAuthorizationCodesByOwner(string provider, SecurityEntityTypes ownerType, string ownerId)
        {
            return codes.Where(ele => ele.OwnerId == ownerId && ele.OwnerType == ownerType);
        }

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="provider">The provider name or url.</param>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<AuthorizationCodeEntity>> GetAuthorizationCodesByOwnerAsync(string provider, SecurityEntityTypes ownerType, string ownerId, CancellationToken cancellationToken = default)
        {
            return ToListAsync(GetAuthorizationCodesByOwner(provider, ownerType, ownerId), cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByNameAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(tokens, ele => ele.Name == accessToken, cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(tokens, ele => ele.RefreshToken == refreshToken, cancellationToken);
        }

        /// <summary>
        /// Gets a user group entity by given identifier.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupEntity> GetUserGroupByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(groups, ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="id">The user group relationship entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupRelationshipEntity> GetRelationshipByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(relationships, ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="groupId">The user group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupRelationshipEntity> GetRelationshipByIdAsync(string groupId, string userId, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(relationships, ele => ele.OwnerId == groupId && ele.TargetId == userId, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupRelationshipEntity>> ListUserGroupsAsync(UserEntity user, string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal, CancellationToken cancellationToken = default)
        {
            var col = relationships.Where(ele => ele.TargetId == user?.Id && ele.State == relationshipState);
            if (!string.IsNullOrWhiteSpace(q)) col = col.Where(ele => ele.Name.Contains(q));
            return ToListAsync(col, cancellationToken);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IEnumerable<UserGroupRelationshipEntity> ListUsers(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role = null, string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
        {
            var col = relationships.Where(ele => ele.OwnerId == group?.Id && ele.State == relationshipState);
            if (role.HasValue) col = col.Where(ele => ele.Role == role.Value);
            if (!string.IsNullOrWhiteSpace(q)) col = col.Where(ele => ele.Name.Contains(q));
            return col;
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupRelationshipEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var col = relationships.Where(ele => ele.OwnerId == group?.Id);
            if (role.HasValue) col = col.Where(ele => ele.Role == role.Value);
            if (q != null) q = InternalAssertion.DefaultQueryArgs;
            if (!string.IsNullOrWhiteSpace(q.NameQuery))
            {
                if (q.NameExactly) col = col.Where(ele => ele.Name == q.NameQuery);
                else col = col.Where(ele => ele.Name.Contains(q.NameQuery));
            }

            return ToListAsync(col.Where(ele => ele.State == q.State).Skip(q.Offset).Take(q.Count), cancellationToken);
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <param name="state">The entity state.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IEnumerable<UserGroupEntity> ListGroups(string q, string siteId, bool onlyPublic = false, ResourceEntityStates state = ResourceEntityStates.Normal)
        {
            var col = groups.Where(ele => ele.OwnerSiteId == siteId);
            if (!string.IsNullOrWhiteSpace(q)) col = col.Where(ele => ele.Name.Contains(q));
            if (onlyPublic) col = col.Where(ele => ele.Visibility == UserGroupVisibilities.Visible);
            return col.Where(ele => ele.State == state);
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional query request information.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, string siteId, bool onlyPublic = false, CancellationToken cancellationToken = default)
        {
            var col = groups.Where(ele => ele.OwnerSiteId == siteId);
            if (onlyPublic) col = col.Where(ele => ele.Visibility == UserGroupVisibilities.Visible);
            if (!string.IsNullOrWhiteSpace(q.NameQuery))
            {
                if (q.NameExactly) col = col.Where(ele => ele.Name == q.NameQuery);
                else col = col.Where(ele => ele.Name.Contains(q.NameQuery));
            }

            return ToListAsync(col.Where(ele => ele.State == q.State).Skip(q.Offset).Take(q.Count), cancellationToken);
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IEnumerable<UserGroupEntity> ListGroups(string q, bool onlyPublic = false)
        {
            IEnumerable<UserGroupEntity> col = groups;
            if (!string.IsNullOrWhiteSpace(q)) col = col.Where(ele => ele.Name.Contains(q));
            if (onlyPublic) col = col.Where(ele => ele.Visibility == UserGroupVisibilities.Visible);
            return col;
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, bool onlyPublic = false, CancellationToken cancellationToken = default)
        {

            IEnumerable<UserGroupEntity> col = groups;
            if (onlyPublic) col = col.Where(ele => ele.Visibility == UserGroupVisibilities.Visible);
            if (!string.IsNullOrWhiteSpace(q.NameQuery))
            {
                if (q.NameExactly) col = col.Where(ele => ele.Name == q.NameQuery);
                else col = col.Where(ele => ele.Name.Contains(q.NameQuery));
            }

            return ToListAsync(col.Where(ele => ele.State == q.State).Skip(q.Offset).Take(q.Count), cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entity matched if found; otherwise, null.</returns>
        public Task<UserPermissionItemEntity> GetUserPermissionsAsync(UserEntity user, string siteId, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(userPermissions, ele => ele.TargetId == user?.Id && ele.SiteId == siteId, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The permission entities.</returns>
        public IEnumerable<UserGroupPermissionItemEntity> ListGroupPermissions(UserEntity user, string siteId)
        {
            if (user == null) return new List<UserGroupPermissionItemEntity>();
            var g = relationships.Where(ele => ele.TargetId == user.Id).Select(ele => ele.OwnerId);
            return groupPermissions.Where(ele => g.Contains(ele.TargetId) && ele.SiteId == siteId);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entities.</returns>
        public Task<IEnumerable<UserGroupPermissionItemEntity>> ListGroupPermissionsAsync(UserEntity user, string siteId, CancellationToken cancellationToken = default)
        {
            return ToListAsync(ListGroupPermissions(user, siteId), cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entity matched if found; otherwise, null.</returns>
        public Task<UserGroupPermissionItemEntity> GetGroupPermissionsAsync(UserGroupEntity group, string siteId, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(groupPermissions, ele => ele.TargetId == group?.Id && ele.SiteId == siteId, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="client">The client entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entity matched if found; otherwise, null.</returns>
        public Task<ClientPermissionItemEntity> GetClientPermissionsAsync(AccessingClientEntity client, string siteId, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(clientPermissions, ele => ele.TargetId == client?.Id && ele.SiteId == siteId, cancellationToken);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public async Task<JsonObject> GetSettingsAsync(string key, string siteId, CancellationToken cancellationToken = default)
        {
            var s = await FirstOrDefaultAsync(settings, ele => ele.Name == key && ele.OwnerSiteId == siteId, cancellationToken);
            return s.Config;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public async Task<string> GetSettingsJsonStringAsync(string key, string siteId, CancellationToken cancellationToken = default)
        {
            var json = await GetSettingsAsync(key, siteId, cancellationToken);
            return json?.ToString();
        }

        /// <summary>
        /// Deletes a set of token expired.
        /// </summary>
        /// <param name="userId">The user identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public Task<int> DeleteExpiredTokensAsync(string userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return tokens.RemoveAll(ele => ele.UserId == userId && ele.ExpirationTime <= now);
            });
        }

        /// <summary>
        /// Deletes a set of token expired.
        /// </summary>
        /// <param name="clientId">The client identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public Task<int> DeleteExpiredClientTokensAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return tokens.RemoveAll(ele => ele.ClientId == clientId && ele.ExpirationTime <= now);
            });
        }

        /// <summary>
        /// Deletes a specific access token.
        /// </summary>
        /// <param name="accessToken">The access token to delete.</param>
        /// <returns>The async task.</returns>
        public Task DeleteAccessToken(string accessToken)
        {
            return Task.Run(() =>
            {
                tokens.RemoveAll(ele => ele.Name == accessToken);
            });
        }

        /// <summary>
        /// Creates or updates a user entity.
        /// </summary>
        /// <param name="user">The user entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            return SaveAsync(user, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="group">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupEntity group, CancellationToken cancellationToken = default)
        {
            return SaveAsync(group, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a token entity.
        /// </summary>
        /// <param name="token">The token entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(TokenEntity token, CancellationToken cancellationToken = default)
        {
            return SaveAsync(token, cancellationToken);
        }

        /// <summary>
        /// Creates or updates an accessing app client item entity.
        /// </summary>
        /// <param name="client">The accessing app client item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(AccessingClientEntity client, CancellationToken cancellationToken = default)
        {
            return SaveAsync(client, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserPermissionItemEntity permissionItem, CancellationToken cancellationToken = default)
        {
            return SaveAsync(permissionItem, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupPermissionItemEntity permissionItem, CancellationToken cancellationToken = default)
        {
            return SaveAsync(permissionItem, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(ClientPermissionItemEntity permissionItem, CancellationToken cancellationToken = default)
        {
            return SaveAsync(permissionItem, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a relationship entity.
        /// </summary>
        /// <param name="relationship">The user group relationship entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupRelationshipEntity relationship, CancellationToken cancellationToken = default)
        {
            return SaveAsync(relationship, cancellationToken);
        }


        /// <summary>
        /// Creates or updates an authorization code entity.
        /// </summary>
        /// <param name="code">The authorization code entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(AuthorizationCodeEntity code, CancellationToken cancellationToken = default)
        {
            return SaveAsync(code, cancellationToken);
        }

        /// <summary>
        /// Creates or updates the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<ChangeMethods> SaveSettingsAsync(string key, string siteId, JsonObject value, CancellationToken cancellationToken = default)
        {
            var s = await FirstOrDefaultAsync(settings, ele => ele.Name == key && ele.OwnerSiteId == siteId, cancellationToken);
            if (s != null)
            {
                s.Config = value;
                return await SaveAsync(s, cancellationToken);
            }

            s = new SettingsEntity
            {
                Name = key,
                OwnerSiteId = siteId,
                State = ResourceEntityStates.Normal,
                Config = value
            };
            return await SaveAsync(s, cancellationToken);
        }

        private Task<IEnumerable<T>> ToListAsync<T>(IEnumerable<T> col, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() => col);
        }

        private Task<T> FirstOrDefaultAsync<T>(IEnumerable<T> col, Func<T, bool> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return predicate is null ? Task.Run(() => col.FirstOrDefault()) : Task.Run(() => col.FirstOrDefault(predicate));
        }

        private Task<ChangeMethods> SaveAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            return ResourceEntityExtensions.SaveAsync(EmptyAction, EmptyAction, entity, cancellationToken);
        }

        private void EmptyAction<T>(T data)
        {
        }
    }
}
