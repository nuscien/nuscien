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

using Microsoft.EntityFrameworkCore;
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
    public class AccountDbSetProvider : IAccountDataProvider
    {
        /// <summary>
        /// The database context factory.
        /// </summary>
        private readonly Func<bool, IAccountDbContext> contextFactory;

        /// <summary>
        /// Initializes a new instance of the AccountDbSetProvider class.
        /// </summary>
        /// <param name="context">The database context with full-access.</param>
        /// <param name="readonlyContext">The optional database context readonly.</param>
        public AccountDbSetProvider(IAccountDbContext context, IAccountDbContext readonlyContext = null)
        {
            if (readonlyContext == null) readonlyContext = context;
            contextFactory = isReadonly => isReadonly ? readonlyContext : context;
        }

        /// <summary>
        /// Initializes a new instance of the AccountDbSetProvider class.
        /// </summary>
        /// <param name="contextFactory">The database context factory.</param>
        public AccountDbSetProvider(Func<bool, IAccountDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Users.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a user entity by given login name.
        /// </summary>
        /// <param name="loginName">The login name of the user.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByLognameAsync(string loginName, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Users.FirstOrDefaultAsync(ele => ele.Name == loginName, cancellationToken);
        }

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="appId">The client credential name, aka app identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AccessingClientEntity> GetClientByNameAsync(string appId, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Clients.FirstOrDefaultAsync(ele => ele.Name == appId, cancellationToken);
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
            var codeHash = AuthorizationCodeEntity.ComputeCodeHash(code);
            return GetContext(true).Codes.FirstOrDefaultAsync(ele => ele.CodeEncrypted == codeHash && ele.ServiceProvider == provider, cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByNameAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Tokens.FirstOrDefaultAsync(ele => ele.Name == accessToken, cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Tokens.FirstOrDefaultAsync(ele => ele.RefreshToken == refreshToken, cancellationToken);
        }

        /// <summary>
        /// Gets a user group entity by given identifier.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupEntity> GetUserGroupByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Groups.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="id">The user group relationship entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupRelationshipEntity> GetRelationshipByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Relationships.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
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
            return GetContext(true).Relationships.FirstOrDefaultAsync(ele => ele.OwnerId == groupId && ele.TargetId == userId, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserGroupRelationshipEntity>> ListUserGroupsAsync(UserEntity user, string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal, CancellationToken cancellationToken = default)
        {
            var context = GetContext(true);
            var groups = string.IsNullOrWhiteSpace(q)
                ? context.Groups.Where(ele => ele.StateCode == ResourceEntityExtensions.NormalStateCode)
                : context.Groups.Where(ele => ele.Name.Contains(q) && ele.StateCode == ResourceEntityExtensions.NormalStateCode);
            return await context.Relationships.Where(ele => ele.TargetId == user.Id && ele.StateCode == (int)relationshipState).Join(
                groups,
                ele => ele.OwnerId,
                ele => ele.Id,
                (rela, group) => new UserGroupRelationshipEntity(rela, group, user)).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IQueryable<UserGroupRelationshipEntity> ListUsers(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role = null, string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
        {
            var context = GetContext(true);
            var users = string.IsNullOrWhiteSpace(q)
                ? context.Users.Where(ele => ele.StateCode == ResourceEntityExtensions.NormalStateCode)
                : context.Users.Where(ele => ele.Nickname.Contains(q) && ele.StateCode == ResourceEntityExtensions.NormalStateCode);
            var relationships = context.Relationships.Where(ele => ele.OwnerId == group.Id && ele.StateCode == (int)relationshipState);
            if (role.HasValue) relationships = relationships.Where(ele => ele.RoleCode == (int)role.Value);
            return relationships.Join(
                users,
                ele => ele.OwnerId,
                ele => ele.Id,
                (rela, user) => new UserGroupRelationshipEntity(rela, group, user));
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        IEnumerable<UserGroupRelationshipEntity> IAccountDataProvider.ListUsers(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role, string q, ResourceEntityStates relationshipState)
        {
            return ListUsers(group, role, q, relationshipState);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserGroupRelationshipEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (q == null) q = InternalAssertion.DefaultQueryArgs;
            return await ListUsers(group, role, q.NameQuery, q.State).ToListAsync(q, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IQueryable<UserPermissionItemEntity> ListUserPermissions(UserEntity user, string siteId)
        {
            return GetContext(true).UserPermissions.Where(ele => ele.TargetId == user.Id && ele.TargetTypeCode == (int)SecurityEntityTypes.User && ele.SiteId == siteId && ele.StateCode == ResourceEntityExtensions.NormalStateCode);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        IEnumerable<UserPermissionItemEntity> IAccountDataProvider.ListUserPermissions(UserEntity user, string siteId)
        {
            return ListUserPermissions(user, siteId);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserPermissionItemEntity>> ListUserPermissionsAsync(UserEntity user, string siteId, CancellationToken cancellationToken = default)
        {
            return await ListUserPermissions(user, siteId).ToListAsync(null, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IQueryable<UserGroupPermissionItemEntity> ListGroupPermissions(UserEntity user, string siteId)
        {
            var context = GetContext(true);
            return context.GroupPermissions.Where(ele => ele.TargetTypeCode == (int)SecurityEntityTypes.UserGroup && ele.SiteId == siteId && ele.StateCode == ResourceEntityExtensions.NormalStateCode).Join(
                context.Relationships.Where(ele => ele.TargetId == user.Id && ele.StateCode == ResourceEntityExtensions.NormalStateCode),
                ele => ele.TargetId,
                ele => ele.OwnerId,
                (perm, rela) => perm);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        IEnumerable<UserGroupPermissionItemEntity> IAccountDataProvider.ListGroupPermissions(UserEntity user, string siteId)
        {
            return ListGroupPermissions(user, siteId);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserGroupPermissionItemEntity>> ListGroupPermissionsAsync(UserEntity user, string siteId, CancellationToken cancellationToken = default)
        {
            return await ListGroupPermissions(user, siteId).ToListAsync(null, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IQueryable<UserGroupPermissionItemEntity> ListGroupPermissions(UserGroupEntity group, string siteId)
        {
            return GetContext(true).GroupPermissions.Where(ele => ele.TargetId == group.Id && ele.TargetTypeCode == (int)SecurityEntityTypes.User && ele.SiteId == siteId && ele.StateCode == ResourceEntityExtensions.NormalStateCode);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        IEnumerable<UserGroupPermissionItemEntity> IAccountDataProvider.ListGroupPermissions(UserGroupEntity group, string siteId)
        {
            return ListGroupPermissions(group, siteId);
        }

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserGroupPermissionItemEntity>> ListGroupPermissionsAsync(UserGroupEntity group, string siteId, CancellationToken cancellationToken = default)
        {
            return await ListGroupPermissions(group, siteId).ToListAsync(null, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="client">The client entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IQueryable<UserPermissionItemEntity> ListClientPermissions(AccessingClientEntity client, string siteId)
        {
            return GetContext(true).UserPermissions.Where(ele => ele.TargetId == client.Id && ele.TargetTypeCode == (int)SecurityEntityTypes.ServiceClient && ele.SiteId == siteId && ele.StateCode == ResourceEntityExtensions.NormalStateCode);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="client">The client entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        IEnumerable<UserPermissionItemEntity> IAccountDataProvider.ListClientPermissions(AccessingClientEntity client, string siteId)
        {
            return ListClientPermissions(client, siteId);
        }

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="client">The client entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<IEnumerable<UserPermissionItemEntity>> ListClientPermissionsAsync(AccessingClientEntity client, string siteId, CancellationToken cancellationToken = default)
        {
            return await ListClientPermissions(client, siteId).ToListAsync(null, cancellationToken);
        }

        /// <summary>
        /// Deletes a set of tokens expired.
        /// </summary>
        /// <param name="userId">The user identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public async Task<int> DeleteExpiredTokensAsync(string userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var tokens = GetContext().Tokens;
            var list = await tokens.Where(ele => ele.UserId == userId && ele.ExpirationTime <= now).ToListAsync(cancellationToken);
            tokens.RemoveRange(list);
            return list.Count;
        }

        /// <summary>
        /// Deletes a set of tokens expired.
        /// </summary>
        /// <param name="clientId">The client identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public async Task<int> DeleteExpiredClientTokensAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var tokens = GetContext().Tokens;
            var list = await tokens.Where(ele => ele.ClientId == clientId && ele.ExpirationTime <= now).ToListAsync(cancellationToken);
            tokens.RemoveRange(list);
            return list.Count;
        }

        /// <summary>
        /// Deletes a specific access token.
        /// </summary>
        /// <param name="accessToken">The access token to delete.</param>
        /// <returns>The async task.</returns>
        public async Task DeleteAccessToken(string accessToken)
        {
            var tokens = GetContext().Tokens;
            var list = await tokens.Where(ele => ele.Name == accessToken).ToListAsync();
            tokens.RemoveRange(list);
        }

        /// <summary>
        /// Creates or updates a user entity.
        /// </summary>
        /// <param name="user">The user entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(GetContext().Users, user, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="group">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupEntity group, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(GetContext().Groups, group, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a token entity.
        /// </summary>
        /// <param name="token">The token entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(TokenEntity token, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(GetContext().Tokens, token, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserPermissionItemEntity permissionItem, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(GetContext().UserPermissions, permissionItem, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupPermissionItemEntity permissionItem, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(GetContext().GroupPermissions, permissionItem, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(ClientPermissionItemEntity permissionItem, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(GetContext().ClientPermissions, permissionItem, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a relationship entity.
        /// </summary>
        /// <param name="relationship">The user group relationship entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupRelationshipEntity relationship, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(GetContext().Relationships, relationship, cancellationToken);
        }

        /// <summary>
        /// Gets database context.
        /// </summary>
        /// <param name="isReadonly">true if get the readonly instance; otherwise, false.</param>
        /// <returns>The database context.</returns>
        protected IAccountDbContext GetContext(bool isReadonly = false)
        {
            return contextFactory?.Invoke(isReadonly);
        }
    }
}
