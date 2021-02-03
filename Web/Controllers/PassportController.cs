using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuScien.Data;
using NuScien.Security;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Web
{
    /// <summary>
    /// The passport and settings controller.
    /// </summary>
    public partial class ResourceAccessController : ControllerBase
    {
        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("passport/login")]
        public async Task<UserTokenInfo> LoginAsync()
        {
            var instance = await this.GetResourceAccessClientAsync();
            var login = false;
            try
            {
                login = Request?.Body != null && Request.Body.Length > 0;
            }
            catch (NotSupportedException)
            {
            }

            var user = login
                ? await instance.SignInAsync(Request.Body)
                : await instance.AuthorizeAsync(Request.Headers);
            if (user is null) Logger?.LogInformation(new EventId(17001001, "passport"), "User tried to login but failed.");
            else Logger?.LogInformation(new EventId(17001001, "Login"), "User {0} logged in.", user.User?.Name ?? user.UserId);
            return user;
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("passport/logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var instance = await this.GetResourceAccessClientAsync();
            await instance.SignOutAsync();
            Logger?.LogInformation(new EventId(17001002, "Logout"), "The user logged out.");
            return Ok();
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("passport/users/exist")]
        public async Task<IActionResult> IsUserExistedAsync()
        {
            var q = await Request.ReadBodyAsQueryDataAsync();
            var key = q.GetFirstValue("key", true);
            var value = q.GetFirstValue("value", true);
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            switch (key)
            {
                case "is":
                    return Ok();
                case "logname":
                case "name":
                    if (await instance.HasUserNameAsync(value)) return Ok();
                    return NotFound();
                default:
                    return StatusCode(501);
            }
        }

        /// <summary>
        /// Sets a new authorization code.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/authcode/{serviceProvider}")]
        public async Task<IActionResult> SetAuthorizationCodeAsync(string serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(serviceProvider)) return BadRequest();
            var q = await Request.ReadBodyAsQueryDataAsync();
            var code = q.GetFirstValue(CodeTokenRequestBody.CodeProperty, true);
            if (string.IsNullOrWhiteSpace(code)) return BadRequest();
            var isInserting = q.GetFirstValue("insert", true) == JsonBoolean.TrueString;
            var instance = await this.GetResourceAccessClientAsync();
            var result = await instance.SetAuthorizationCodeAsync(serviceProvider, code, isInserting);
            Logger?.LogInformation(new EventId(17001005, "AuthCode"), "Set auth code of {0} for user {1}.", serviceProvider, instance?.User?.Name ?? instance?.UserId);
            return result.ToActionResult();
        }

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/user/{id}")]
        public async Task<IActionResult> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var result = await instance.GetUserByIdAsync(id);
            return this.ResourceEntityResult(result);
        }

        /// <summary>
        /// Gets a user group entity by given identifier.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/group/{id}")]
        public async Task<IActionResult> GetUserGroupByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var result = await instance.GetUserGroupByIdAsync(id);
            return this.ResourceEntityResult(result);
        }

        /// <summary>
        /// Gets the user group relationship.
        /// </summary>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/rela")]
        public async Task<IActionResult> ListRelationshipsAsync()
        {
            var q = Request.Query.GetQueryData();
            var instance = await this.GetResourceAccessClientAsync();
            var name = q["name"]?.Trim();
            if (string.IsNullOrEmpty(name)) name = null;
            var state = q.TryGetEnumValue<ResourceEntityStates>("state", true) ?? ResourceEntityStates.Normal;
            var result = await instance.ListRelationshipsAsync(name, state);
            return new JsonResult(new UserGroupRelationshipCollection(result));
        }

        /// <summary>
        /// Gets the user group relationship.
        /// </summary>
        /// <param name="groupId">The user group identifier.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/rela/g/{groupId}")]
        public async Task<IActionResult> GetRelationshipByGroupIdAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var group = await instance.GetUserGroupByIdAsync(groupId);
            var result = await instance.GetRelationshipAsync(group);
            return this.ResourceEntityResult(result);
        }

        /// <summary>
        /// Gets the user group relationship.
        /// </summary>
        /// <param name="groupId">The user group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/rela/g/{groupId}/{userId}")]
        public async Task<IActionResult> GetRelationshipByGroupIdAsync(string groupId, string userId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var group = await instance.GetUserGroupByIdAsync(groupId);
            UserGroupRelationshipEntity result;
            if (string.IsNullOrWhiteSpace(userId) || userId == instance.User?.Id)
            {
                result = await instance.GetRelationshipAsync(group);
            }
            else
            {
                var user = await instance.GetUserByIdAsync(userId);
                result = await instance.GetRelationshipAsync(group, user);
            }

            return this.ResourceEntityResult(result);
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/groups")]
        public async Task<CollectionResult<UserGroupEntity>> ListGroupsAsync()
        {
            var q = Request.Query.GetQueryArgs();
            var siteId = Request.Query.GetFirstStringValue("site");
            var instance = await this.GetResourceAccessClientAsync();
            var col = string.IsNullOrWhiteSpace(siteId)
                ? await instance.ListGroupsAsync(q)
                : await instance.ListGroupsAsync(q, siteId);
            return new CollectionResult<UserGroupEntity>(col, q.Offset);
        }

        /// <summary>
        /// Renews the client app key.
        /// </summary>
        /// <param name="appId">The client app identifier.</param>
        /// <returns>The client app identifier and secret key.</returns>
        [HttpPost]
        [Route("passport/credential/client/{appId}/renew")]
        public async Task<IActionResult> RenewAppClientKeyAsync(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var result = await instance.RenewAppClientKeyAsync(appId);
            if (result == null) return this.EmptyEntity();
            Logger?.LogInformation(new EventId(17001007, "RenewAppKey"), "Renew app client key {0}.", appId);
            return new JsonResult(result);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/users/group/{id}")]
        public async Task<IActionResult> ListUsersByGroupAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var query = Request.Query.GetQueryArgs();
            var roleIndex = Request.Query.TryGetInt32Value("role");
            var instance = await this.GetResourceAccessClientAsync();
            var group = await instance.GetUserGroupByIdAsync(id);
            var result = roleIndex.HasValue ? await instance.ListUsersAsync(group, (UserGroupRelationshipEntity.Roles)roleIndex.Value, query) : await instance.ListUsersAsync(group, query);
            if (result == null) result = new List<UserEntity>();
            return new JsonResult(new CollectionResult<UserEntity>(result));
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/user")]
        public async Task<IActionResult> SaveUserAsync([FromBody] UserEntity entity)
        {
            if (entity == null) return ChangeErrorKinds.Argument.ToActionResult("Requires an entity in body.");
            var instance = await this.GetResourceAccessClientAsync();
            var result = await instance.SaveAsync(entity);
            Logger?.LogInformation(new EventId(17001011, "SaveUserInfo"), "Save user information {0}.", entity.Name ?? entity.Id);
            return result.ToActionResult();
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/user/{id}")]
        public Task<IActionResult> SaveUserAsync(string id)
        {
            return this.SaveEntityAsync(async (i, instance) =>
            {
                return await instance.GetUserByIdAsync(id);
            }, async (entity, instance, delta) =>
            {
                var result = await instance.SaveAsync(entity);
                Logger?.LogInformation(new EventId(17001012, "SaveUserInfo"), "Save user information {0}.", entity.Name ?? entity.Id);
                return result;
            }, id);
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/group")]
        public async Task<IActionResult> SaveGroupAsync([FromBody] UserGroupEntity entity)
        {
            if (entity == null) return ChangeErrorKinds.Argument.ToActionResult("Requires an entity in body.");
            var instance = await this.GetResourceAccessClientAsync();
            var result = await instance.SaveAsync(entity);
            Logger?.LogInformation(new EventId(17001013, "SaveUserGroupInfo"), "Save user group information {0}.", entity.Name ?? entity.Id);
            return result.ToActionResult();
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/group/{id}")]
        public Task<IActionResult> SaveGroupAsync(string id)
        {
            return this.SaveEntityAsync(async (i, instance) =>
            {
                return await instance.GetUserGroupByIdAsync(id);
            }, async (entity, instance, delta) =>
            {
                var result = await instance.SaveAsync(entity);
                Logger?.LogInformation(new EventId(17001014, "SaveUserGroupInfo"), "Save user group information {0}.", entity.Name ?? entity.Id);
                return result;
            }, id);
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/rela")]
        public async Task<IActionResult> SaveRelationshipAsync([FromBody] UserGroupRelationshipEntity entity)
        {
            if (entity == null) return ChangeErrorKinds.Argument.ToActionResult("Requires an entity in body.");
            var instance = await this.GetResourceAccessClientAsync();
            var result = await instance.SaveAsync(entity);
            Logger?.LogInformation(new EventId(17001017, "SaveUserGroupRela"), "Save user group relationship {0}, owner {1}, target {2}.", entity.Id, entity.OwnerId, entity.TargetId);
            return result.ToActionResult();
        }
    }
}
