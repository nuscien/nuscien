using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
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
    public class SettingsEntity : ConfigurableResourceEntity
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
                GlobalConfig = globalConfig;
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
                SiteConfig = siteConfig;
                GlobalConfig = globalConfig;
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
            /// Gets the configuration data of the site.
            /// </summary>
            public JsonObject SiteConfig { get; }

            /// <summary>
            /// Gets the configuration data from global.
            /// </summary>
            public JsonObject GlobalConfig { get; }
        }

        /// <summary>
        /// Gets or sets the owner site identifier.
        /// </summary>
        [Column("site")]
        [DataMember(Name = "site")]
        [JsonPropertyName("site")]
        public string OwnerSiteId
        {
            get
            {
                return GetCurrentProperty<string>();
            }

            set
            {
                SetCurrentProperty(value);
            }
        }
    }
}
