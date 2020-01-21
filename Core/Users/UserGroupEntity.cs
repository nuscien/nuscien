using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Text;

namespace NuScien.Users
{
    /// <summary>
    /// User group information.
    /// </summary>
    public class UserGroupEntity : BaseResourceEntity
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [DataMember(Name = "desc")]
        [JsonPropertyName("desc")]
        public string Description
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        [DataMember(Name = "avatar")]
        [JsonPropertyName("avatar")]
        public string Avatar
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }
}
