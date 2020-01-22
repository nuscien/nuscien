using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The client entity.
    /// </summary>
    public class AuthorizationCodeEntity : BaseOwnerResourceEntity
    {
        /// <summary>
        /// Gets or sets the avatar or icon URL.
        /// </summary>
        [NotMapped]
        [JsonPropertyName("kind")]
        public SecurityEntityTypes OwnerType
        {
            get => GetCurrentProperty<SecurityEntityTypes>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the avatar or icon URL.
        /// </summary>
        [Column("kind")]
        [JsonIgnore]
        public int OwnerTypeCode
        {
            get => (int)OwnerType;
            set => OwnerType = (SecurityEntityTypes)value;
        }

        /// <summary>
        /// Gets or sets the avatar or icon URL.
        /// </summary>
        [Column("avatar")]
        [JsonPropertyName("avatar")]
        public string Avatar
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        [JsonIgnore]
        [Column("code")]
        public string Code
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the service provider name or url.
        /// </summary>
        [Column("provider")]
        [JsonPropertyName("provider")]
        public string ServiceProvider
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }
}
