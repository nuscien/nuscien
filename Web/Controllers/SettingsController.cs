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

namespace NuScien.Web
{
    /// <summary>
    /// The passport and settings controller.
    /// </summary>
    public partial class ResourceAccessControllerBase : ControllerBase
    {
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
            var instance = await this.GetResourceAccessClientAsync();
            var m = await instance.GetSettingsAsync(key);
            if (m == null) return this.EmptyEntity();
            return ControllerExtensions.JsonStringResult(m.GlobalConfigString);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <returns>The value.</returns>
        [HttpGet]
        [Route("settings/site/{siteId}/{key}")]
        public async Task<IActionResult> GetSettingsJsonStringByKeyAsync(string siteId, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var isGlobal = string.IsNullOrWhiteSpace(siteId);
            var m = isGlobal ? await instance.GetSettingsAsync(key): await instance.GetSettingsAsync(key, siteId);
            if (m == null) return this.EmptyEntity();
            return ControllerExtensions.JsonStringResult(isGlobal ? m.GlobalConfigString : m.SiteConfigString);
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <returns>The value.</returns>
        [HttpPut]
        [Route("settings/global/{key}")]
        public async Task<IActionResult> SaveSettingsDataByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var body = await JsonObject.ParseAsync(Request.Body);
            var result = await instance.SaveSettingsAsync(key, null, body);
            Logger?.LogInformation(new EventId(17001103, "SaveGlobalSettings"), "Save global settings {0}.", key);
            return result.ToActionResult();
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <returns>The value.</returns>
        [HttpPut]
        [Route("settings/site/{siteId}/{key}")]
        public async Task<IActionResult> SaveSettingsJsonStringByKeyAsync(string siteId, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var body = await JsonObject.ParseAsync(Request.Body);
            var result = await instance.SaveSettingsAsync(key, siteId, body);
            Logger?.LogInformation(new EventId(17001103, "SaveGlobalSettings"), "Save global settings {0}.", key);
            return result.ToActionResult();
        }

        /// <summary>
        /// Gets the permissions for the specific target.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="targetType">The target entity type.</param>
        /// <param name="targetId">The target entity identifier.</param>
        /// <returns>The permission information.</returns>
        [HttpGet]
        [Route("settings/perms/{siteId}/{targetType}/{targetId}")]
        public async Task<IActionResult> GetPermissionsAsync(string siteId, string targetType, string targetId)
        {
            if (string.IsNullOrWhiteSpace(siteId) || string.IsNullOrWhiteSpace(targetType) || string.IsNullOrWhiteSpace(targetId)) return BadRequest();
            if (string.IsNullOrEmpty(targetId)) return await GetPermissionsAsync(siteId, targetType);
            var instance = await this.GetResourceAccessClientAsync();
            var targetTypeValue = targetType.ToLowerInvariant() switch
            {
                "user" or "u" or "1" => SecurityEntityTypes.User,
                "group" or "g" or "2" or "usergroup" => SecurityEntityTypes.UserGroup,
                "client" or "c" or "3" or "serviceclient" => SecurityEntityTypes.ServiceClient,
                _ => SecurityEntityTypes.Unknown
            };
            if (targetTypeValue == SecurityEntityTypes.Unknown) return BadRequest();
            try
            {
                var result = await instance.GetPermissionAsync(siteId, targetTypeValue, targetId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null) return er;
                throw;
            }
        }

        /// <summary>
        /// Gets the permissions for the current visitor.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="targetType">The target entity type.</param>
        /// <returns>The permission information.</returns>
        [HttpGet]
        [Route("settings/perms/{siteId}/{targetType}")]
        public async Task<IActionResult> GetPermissionsAsync(string siteId, string targetType)
        {
            if (string.IsNullOrWhiteSpace(siteId) || string.IsNullOrWhiteSpace(targetType)) return BadRequest();
            var instance = await this.GetResourceAccessClientAsync();
            var targetTypeValue = targetType.ToLowerInvariant() switch
            {
                "user" or "u" or "1" => SecurityEntityTypes.User,
                "group" or "g" or "2" or "usergroup" => SecurityEntityTypes.UserGroup,
                "client" or "c" or "3" or "serviceclient" => SecurityEntityTypes.ServiceClient,
                _ => SecurityEntityTypes.Unknown
            };
            try
            {
                switch (targetTypeValue)
                {
                    case SecurityEntityTypes.User:
                        {
                            var result = await instance.GetUserPermissionsAsync(siteId);
                            return Ok(result);
                        }
                    case SecurityEntityTypes.UserGroup:
                        {
                            var result = await instance.GetGroupPermissionsAsync(siteId);
                            return Ok(result);
                        }
                    case SecurityEntityTypes.ServiceClient:
                        {
                            var result = await instance.GetClientPermissionsAsync(siteId);
                            return Ok(result);
                        }
                    default:
                        {
                            return BadRequest();
                        }
                }
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null) return er;
                throw;
            }
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
            var instance = await this.GetResourceAccessClientAsync();
            var targetTypeValue = targetType.ToLowerInvariant() switch
            {
                "user" or "u" or "1" => SecurityEntityTypes.User,
                "group" or "g" or "2" or "usergroup" => SecurityEntityTypes.UserGroup,
                "client" or "c" or "3" or "serviceclient" => SecurityEntityTypes.ServiceClient,
                _ => SecurityEntityTypes.Unknown
            };
            if (targetTypeValue == SecurityEntityTypes.Unknown) return BadRequest();
            var result = await instance.SavePermissionAsync(siteId, targetTypeValue, targetId, permissions.Permissions);
            Logger?.LogInformation(new EventId(17001111, "SavePerm"), "Save permission for {0} {1} of site {2}.", targetType, targetId, siteId);
            return result.ToActionResult();
        }
    }
}
