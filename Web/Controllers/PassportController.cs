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

namespace NuScien.Web.Controllers
{
    /// <summary>
    /// The passport controller.
    /// </summary>
    [ApiController]
    [Route("api")]
    [Route("nuscien5")]
    public class PassportController : ControllerBase
    {
        #pragma warning disable IDE0052
        private readonly ILogger<PassportController> _logger;
        #pragma warning restore IDE0052

        /// <summary>
        /// Initializes a new instance of the PassportController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PassportController(ILogger<PassportController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("passport/login")]
        public async Task<UserTokenInfo> LoginAsync()
        {
            var instance = await ResourceAccessClients.ResolveAsync();
            var login = false;
            try
            {
                login = Request.Body != null && Request.Body.Length > 0;
            }
            catch (NotSupportedException)
            {
            }

            if (login) return await instance.SignInAsync(Request.Body);
            return await instance.AuthorizeAsync(Request.Headers);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("passport/logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var instance = await ResourceAccessClients.ResolveAsync();
            await instance.SignOutAsync();
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
            var instance = await ResourceAccessClients.ResolveAsync();
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
            var instance = await ResourceAccessClients.ResolveAsync();
            var result = await instance.SetAuthorizationCodeAsync(serviceProvider, code, isInserting);
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
            var instance = await ResourceAccessClients.ResolveAsync();
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
            var instance = await ResourceAccessClients.ResolveAsync();
            var result = await instance.GetUserGroupByIdAsync(id);
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
            var instance = await ResourceAccessClients.ResolveAsync();
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
            var instance = await ResourceAccessClients.ResolveAsync();
            var result = await instance.RenewAppClientKeyAsync(appId);
            if (result == null) return this.EmptyEntity();
            return new JsonResult(result);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        [HttpGet]
        [Route("passport/users/group/{id}")]
        protected async Task<IActionResult> ListUsersByGroupAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var query = Request.Query.GetQueryArgs();
            var roleIndex = Request.Query.TryGetInt32Value("role");
            var instance = await ResourceAccessClients.ResolveAsync();
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
        protected async Task<IActionResult> SaveUserAsync([FromBody] UserEntity entity)
        {
            if (entity == null) return ChangeMethods.Invalid.ToActionResult();
            var instance = await ResourceAccessClients.ResolveAsync();
            var result = await instance.SaveAsync(entity);
            return result.ToActionResult();
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/group")]
        protected async Task<IActionResult> SaveGroupAsync([FromBody] UserGroupEntity entity)
        {
            if (entity == null) return ChangeMethods.Invalid.ToActionResult();
            var instance = await ResourceAccessClients.ResolveAsync();
            var result = await instance.SaveAsync(entity);
            return result.ToActionResult();
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("passport/rela")]
        protected async Task<IActionResult> SaveUserAsync([FromBody] UserGroupRelationshipEntity entity)
        {
            if (entity == null) return ChangeMethods.Invalid.ToActionResult();
            var instance = await ResourceAccessClients.ResolveAsync();
            if (entity.OwnerId == instance.UserId)
            {
                var group = await instance.GetUserGroupByIdAsync(entity.OwnerId);
                var joinInResult = await instance.JoinAsync(group);
                return joinInResult.ToActionResult();
            }

            var groupTask = instance.GetUserGroupByIdAsync(entity.OwnerId);
            var user = await instance.GetUserByIdAsync(entity.TargetId);
            var result = await instance.InviteAsync(await groupTask, user);
            return result.ToActionResult();
        }
    }
}
