using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// Security entity types.
    /// </summary>
    public enum SecurityEntityTypes
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// User.
        /// </summary>
        User = 1,

        /// <summary>
        /// User group.
        /// </summary>
        UserGroup = 2,

        /// <summary>
        /// Service.
        /// </summary>
        ServiceClient = 3
    }

    /// <summary>
    /// The base security entity.
    /// </summary>
    public abstract class BaseSecurityEntity : BaseResourceEntity
    {
        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public abstract SecurityEntityTypes SecurityEntityType { get; }

        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [DataMember(Name = "security_type")]
        [JsonPropertyName("security_type")]
        public string SecurityEntityTypeString
        {
            get => SecurityEntityType.ToString();
            set => _ = value;
        }
    }
}
