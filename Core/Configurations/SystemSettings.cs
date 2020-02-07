using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

using Trivial.Reflection;
using NuScien.Reflection;

namespace NuScien.Configurations
{
    /// <summary>
    /// The system global settings.
    /// </summary>
    public class SystemGlobalSettings : ReadonlyObservableProperties
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        public string Owner
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the logo URL.
        /// </summary>
        public string Logo
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }


        /// <summary>
        /// Gets or sets the wordmark URL.
        /// </summary>
        public string Wordmark
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the group identifier of the group administrators.
        /// </summary>
        [JsonPropertyName("admin_current")]
        public string CurrentSettingsAdminGroupId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the group identifier of the user administrators.
        /// </summary>
        [JsonPropertyName("admin_user")]
        public string UserAdminGroupId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the group identifier of the site administrators.
        /// </summary>
        [JsonPropertyName("admin_site")]
        public string SiteAdminGroupId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the group identifier of the group administrators.
        /// </summary>
        [JsonPropertyName("admin_group")]
        public string GroupAdminGroupId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the group identifier of the CMS administrators.
        /// </summary>
        [JsonPropertyName("admin_cms")]
        public string CmsAdminGroupId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The system site settings.
    /// </summary>
    public class SystemSiteSettings : ReadonlyObservableProperties
    {
        /// <summary>
        /// Gets or sets the site name.
        /// </summary>
        public string Name
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the site owner.
        /// </summary>
        public string Owner
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the logo URL.
        /// </summary>
        public string Logo
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the group identifier of the group administrators.
        /// </summary>
        [JsonPropertyName("admin")]
        public string AdminGroupId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }
}
