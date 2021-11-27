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

using NuScien.Cms;
using NuScien.Configurations;
using NuScien.Data;
using NuScien.Sns;
using NuScien.Users;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The data provider of the account service.
    /// </summary>
    public interface IAccountDataProvider
    {
        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user entity by given login name.
        /// </summary>
        /// <param name="loginName">The login name of the user.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByLognameAsync(string loginName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="appId">The client credential name, aka app identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AccessingClientEntity> GetClientByNameAsync(string appId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a client credential by accessing client entity identifier.
        /// </summary>
        /// <param name="id">The client entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AccessingClientEntity> GetClientByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="provider">The provider name or url.</param>
        /// <param name="code">The authorization code.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AuthorizationCodeEntity> GetAuthorizationCodeByCodeAsync(string provider, string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="provider">The provider name or url.</param>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public IEnumerable<AuthorizationCodeEntity> GetAuthorizationCodesByOwner(string provider, SecurityEntityTypes ownerType, string ownerId);

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="provider">The provider name or url.</param>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<AuthorizationCodeEntity>> GetAuthorizationCodesByOwnerAsync(string provider, SecurityEntityTypes ownerType, string ownerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByNameAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user group entity by given identifier.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupEntity> GetUserGroupByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="id">The user group relationship entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupRelationshipEntity> GetRelationshipByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="groupId">The user group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public Task<UserGroupRelationshipEntity> GetRelationshipByIdAsync(string groupId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="q">The optional query request information.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupRelationshipEntity>> ListUserGroupsAsync(UserEntity user, string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IEnumerable<UserGroupRelationshipEntity> ListUsers(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role = null, string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal);

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional query request information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupRelationshipEntity>> ListUsersAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <param name="state">The entity state.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IEnumerable<UserGroupEntity> ListGroups(string q, string siteId, bool onlyPublic = false, ResourceEntityStates state = ResourceEntityStates.Normal);

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional query request information.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, string siteId, bool onlyPublic = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional name query; or null for all.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IEnumerable<UserGroupEntity> ListGroups(string q, bool onlyPublic = false);

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional query request information.</param>
        /// <param name="onlyPublic">true if only public; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, bool onlyPublic = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entity matched if found; otherwise, null.</returns>
        public Task<UserPermissionItemEntity> GetUserPermissionsAsync(UserEntity user, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>The permission entities.</returns>
        public IEnumerable<UserGroupPermissionItemEntity> ListGroupPermissions(UserEntity user, string siteId);

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entities.</returns>
        public Task<IEnumerable<UserGroupPermissionItemEntity>> ListGroupPermissionsAsync(UserEntity user, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a collection of user group permissions.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entity matched if found; otherwise, null.</returns>
        public Task<UserGroupPermissionItemEntity> GetGroupPermissionsAsync(UserGroupEntity group, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a collection of user permissions.
        /// </summary>
        /// <param name="client">The client entity.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The permission entities.</returns>
        public Task<ClientPermissionItemEntity> GetClientPermissionsAsync(AccessingClientEntity client, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public Task<JsonObjectNode> GetSettingsAsync(string key, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        public Task<string> GetSettingsJsonStringAsync(string key, string siteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a set of token expired.
        /// </summary>
        /// <param name="userId">The user identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public Task<int> DeleteExpiredTokensAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a set of token expired.
        /// </summary>
        /// <param name="clientId">The client identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public Task<int> DeleteExpiredClientTokensAsync(string clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a specific access token.
        /// </summary>
        /// <param name="accessToken">The access token to delete.</param>
        /// <returns>The async task.</returns>
        public Task DeleteAccessTokenAsync(string accessToken);

        /// <summary>
        /// Gets a specific publish content.
        /// </summary>
        /// <param name="id">The identifier of the publish content.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity to get.</returns>
        public Task<ContentEntity> GetContentAsync(string id, bool includeAllStates, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the publish contents.
        /// </summary>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="parent">The optional parent content identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<IEnumerable<ContentEntity>> ListContentAsync(string siteId, string parent = null, QueryArgs q = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the publish contents.
        /// </summary>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="all">true if search all contents; otherise, false.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<IEnumerable<ContentEntity>> ListContentAsync(string siteId, bool all, QueryArgs q = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the revision entities.
        /// </summary>
        /// <param name="source">The source owner identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<IEnumerable<ContentRevisionEntity>> ListContentRevisionAsync(string source, QueryArgs q = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the revisions.
        /// </summary>
        /// <param name="id">The revision entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<ContentRevisionEntity> GetContentRevisionAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific publish content template.
        /// </summary>
        /// <param name="id">The identifier of the publish content template.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity to get.</returns>
        public Task<ContentTemplateEntity> GetContentTemplateAsync(string id, bool includeAllStates, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the publish content templates.
        /// </summary>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<IEnumerable<ContentTemplateEntity>> ListContentTemplateAsync(string siteId, QueryArgs q = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the revision entities.
        /// </summary>
        /// <param name="source">The source owner identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<IEnumerable<ContentTemplateRevisionEntity>> ListContentTemplateRevisionAsync(string source, QueryArgs q = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the revisions.
        /// </summary>
        /// <param name="id">The revision entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<ContentTemplateRevisionEntity> GetContentTemplateRevisionAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific publish content comment.
        /// </summary>
        /// <param name="id">The identifier of the publish content template.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity to get.</returns>
        public Task<ContentCommentEntity> GetContentCommentAsync(string id, bool includeAllStates, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the publish content comments.
        /// </summary>
        /// <param name="content">The owner content comment identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<IEnumerable<ContentCommentEntity>> ListContentCommentsAsync(string content, bool plain, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists the child comments of a specific publish content comment.
        /// </summary>
        /// <param name="id">The parent identifier of the content comment.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public Task<IEnumerable<ContentCommentEntity>> ListContentChildCommentsAsync(string id, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a user entity.
        /// </summary>
        /// <param name="user">The user entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangeMethods> SaveAsync(UserEntity user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="group">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupEntity group, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates an accessing app client item entity.
        /// </summary>
        /// <param name="client">The accessing app client item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(AccessingClientEntity client, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a token entity.
        /// </summary>
        /// <param name="token">The token entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(TokenEntity token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(UserPermissionItemEntity permissionItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupPermissionItemEntity permissionItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a permission item entity.
        /// </summary>
        /// <param name="permissionItem">The permission item entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(ClientPermissionItemEntity permissionItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a relationship entity.
        /// </summary>
        /// <param name="relationship">The user group relationship entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupRelationshipEntity relationship, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates an authorization code entity.
        /// </summary>
        /// <param name="code">The authorization code entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method result.</returns>
        public Task<ChangeMethods> SaveAsync(AuthorizationCodeEntity code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangeMethods> SaveSettingsAsync(string key, string siteId, JsonObjectNode value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a publish content entity.
        /// </summary>
        /// <param name="content">The publish content entity to save.</param>
        /// <param name="message">The commit message.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangeMethods> SaveAsync(ContentEntity content, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a publish content template entity.
        /// </summary>
        /// <param name="template">The publish content template entity to save.</param>
        /// <param name="message">The commit message.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangeMethods> SaveAsync(ContentTemplateEntity template, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a publish content comment entity.
        /// </summary>
        /// <param name="comment">The publish content comment entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangeMethods> SaveAsync(ContentCommentEntity comment, CancellationToken cancellationToken = default);
    }
}
