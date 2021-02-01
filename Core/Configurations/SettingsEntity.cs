using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Configurations
{
    /// <summary>
    /// Settings entity.
    /// </summary>
    [Table("nssettings")]
    public class SettingsEntity : BaseSiteOwnerResourceEntity
    {
        /// <summary>
        /// The model.
        /// </summary>
        public class Model
        {
            /// <summary>
            /// Initializes a new instance of the SettingsEntity.Model class.
            /// </summary>
            /// <param name="key">The settings key.</param>
            /// <param name="globalConfig">The global settings configuration data.</param>
            public Model(string key, JsonObject globalConfig)
            {
                Key = key;
                GlobalConfigString = globalConfig?.ToString();
            }

            /// <summary>
            /// Initializes a new instance of the SettingsEntity.Model class.
            /// </summary>
            /// <param name="key">The settings key.</param>
            /// <param name="globalConfig">The global settings configuration data.</param>
            public Model(string key, string globalConfig)
            {
                Key = key;
                GlobalConfigString = globalConfig;
            }

            /// <summary>
            /// Initializes a new instance of the SettingsEntity.Model class.
            /// </summary>
            /// <param name="key">The settings key.</param>
            /// <param name="siteId">The site identifier.</param>
            /// <param name="siteConfig">The site settings configuration data.</param>
            /// <param name="globalConfig">The global settings configuration data.</param>
            public Model(string key, string siteId, JsonObject siteConfig, JsonObject globalConfig = null)
            {
                Key = key;
                SiteId = siteId;
                SiteConfigString = siteConfig?.ToString();
                GlobalConfigString = globalConfig?.ToString();
            }

            /// <summary>
            /// Initializes a new instance of the SettingsEntity.Model class.
            /// </summary>
            /// <param name="key">The settings key.</param>
            /// <param name="siteId">The site identifier.</param>
            /// <param name="siteConfig">The site settings configuration data.</param>
            /// <param name="globalConfig">The global settings configuration data.</param>
            public Model(string key, string siteId, string siteConfig, string globalConfig = null)
            {
                Key = key;
                SiteId = siteId;
                SiteConfigString = siteConfig;
                GlobalConfigString = globalConfig;
            }

            /// <summary>
            /// Gets the settings key.
            /// </summary>
            public string Key { get; }

            /// <summary>
            /// Gets the site identifier.
            /// </summary>
            public string SiteId { get; }
            
            /// <summary>
            /// Gets the JSON string of the site configuration.
            /// </summary>
            public string SiteConfigString { get; }

            /// <summary>
            /// Gets the JSON string of the global configuration.
            /// </summary>
            public string GlobalConfigString { get; }

            /// <summary>
            /// Gets the configuration data of the site.
            /// </summary>
            public JsonObject GetSiteConfig()
            {
                return JsonObject.Parse(SiteConfigString);
            }

            /// <summary>
            /// Gets the configuration data from global.
            /// </summary>
            public JsonObject GetGlobalConfig()
            {
                return JsonObject.Parse(GlobalConfigString);
            }

            /// <summary>
            /// Deserializes the configuration data of the site.
            /// </summary>
            public T DeserializeSiteConfig<T>()
            {
                var s = SiteConfigString;
                return string.IsNullOrWhiteSpace(s) ? default : JsonSerializer.Deserialize<T>(SiteConfigString);
            }

            /// <summary>
            /// Deserializes the configuration data of the site.
            /// </summary>
            /// <param name="options">The options.</param>
            public T DeserializeSiteConfig<T>(JsonSerializerOptions options)
            {
                var s = SiteConfigString;
                return string.IsNullOrWhiteSpace(s) ? default : JsonSerializer.Deserialize<T>(SiteConfigString, options);
            }

            /// <summary>
            /// Deserializes the configuration data from global.
            /// </summary>
            public T DeserializeGlobalConfig<T>()
            {
                var s = GlobalConfigString;
                return string.IsNullOrWhiteSpace(s) ? default : JsonSerializer.Deserialize<T>(GlobalConfigString);
            }

            /// <summary>
            /// Deserializes the configuration data from global.
            /// </summary>
            /// <param name="options">The options.</param>
            public T DeserializeGlobalConfig<T>(JsonSerializerOptions options)
            {
                var s = GlobalConfigString;
                return string.IsNullOrWhiteSpace(s) ? default : JsonSerializer.Deserialize<T>(GlobalConfigString, options);
            }
        }
    }
}
