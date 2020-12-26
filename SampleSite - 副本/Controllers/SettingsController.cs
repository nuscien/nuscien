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
    public class SettingsController : ControllerBase
    {
        #pragma warning disable IDE0052
        private readonly ILogger<PassportController> _logger;
        #pragma warning restore IDE0052

        /// <summary>
        /// Initializes a new instance of the PassportController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public SettingsController(ILogger<PassportController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <returns>The value.</returns>
        [HttpGet]
        [Route("settings/global/{key}")]
        public async Task<IActionResult> GetSettingsDataByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest();
            var instance = await ResourceAccessClients.ResolveAsync();
            var m = await instance.GetSettingsAsync(key);
            if (m == null) this.EmptyEntity();
            return ControllerExtensions.JsonStringResult(m.GlobalConfigString);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <returns>The value.</returns>
        [HttpGet]
        [Route("settings/global/{key}/{siteId}")]
        public async Task<IActionResult> GetSettingsJsonStringByKeyAsync(string key, string siteId)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest();
            var instance = await ResourceAccessClients.ResolveAsync();
            var isGlobal = string.IsNullOrWhiteSpace(siteId);
            var m = isGlobal ? await instance.GetSettingsAsync(key): await instance.GetSettingsAsync(key, siteId);
            if (m == null) this.EmptyEntity();
            return ControllerExtensions.JsonStringResult(isGlobal ? m.GlobalConfigString : m.SiteConfigString);
        }

        /// <summary>
        /// Creates or updates the permissions.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="targetType">The target entity type.</param>
        /// <param name="targetId">The target entity identifier.</param>
        /// <param name="permissions">The permissions to save.</param>
        /// <returns>The status of changing result.</returns>
        [HttpPut]
        [Route("settings/perms/{siteId}/{targetType}/{targetId}")]
        public async Task<IActionResult> SavePermissionsAsync(string siteId, string targetType, string targetId, [FromBody] PermissionRequestArgs permissions)
        {
            if (string.IsNullOrWhiteSpace(siteId) || string.IsNullOrWhiteSpace(targetType) || string.IsNullOrWhiteSpace(targetId)) return BadRequest();
            var instance = await ResourceAccessClients.ResolveAsync();
            var targetTypeValue = targetType.ToLowerInvariant() switch
            {
                "user" => SecurityEntityTypes.User,
                "group" => SecurityEntityTypes.UserGroup,
                "client" => SecurityEntityTypes.ServiceClient,
                _ => SecurityEntityTypes.Unknown
            };
            if (targetTypeValue == SecurityEntityTypes.Unknown) return BadRequest();
            var result = await instance.SavePermissionAsync(siteId, targetTypeValue, targetId, permissions.Permissions);
            return result.ToActionResult();
        }
    }
}
