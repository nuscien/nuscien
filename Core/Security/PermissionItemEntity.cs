using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using NuScien.Data;
using NuScien.Users;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The permission item entity.
    /// </summary>
    [DataContract]
    public abstract class BasePermissionItemEntity<T> : BaseResourceEntity
        where T : BaseSecurityEntity
    {
        private string config;

        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [DataMember(Name = "site")]
        [JsonPropertyName("site")]
        public string SiteId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public abstract SecurityEntityTypes TargetType { get; }

        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [DataMember(Name = "target_type")]
        [JsonPropertyName("target_type")]
        public string TargetTypeCode
        {
            get => TargetType.ToString();
            set => _ = value;
        }

        /// <summary>
        /// Gets or sets the target resource identifier.
        /// </summary>
        [Column("target")]
        [DataMember(Name = "target")]
        [JsonPropertyName("target")]
        public string TargetId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the target resource entity.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public T Target { get; set; }

        /// <summary>
        /// Gets or sets the permission list.
        /// </summary>
        [Column("permissions")]
        [DataMember(Name = "permissions")]
        [JsonPropertyName("permissions")]
        public string Permissions
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the additional message.
        /// </summary>
        [JsonPropertyName("config")]
        [NotMapped]
        public JsonObject Config
        {
            get => GetCurrentProperty<JsonObject>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the additional message.
        /// </summary>
        [DataMember(Name = "config")]
        [JsonIgnore]
        [Column("config")]
        public string ConfigString
        {
            get
            {
                return Config?.ToString() ?? string.Empty;
            }

            set
            {
                config = value;
                try
                {
                    Config = JsonObject.Parse(config);
                }
                catch (JsonException)
                {
                    Config = new JsonObject();
                }
                catch (ArgumentException)
                {
                    Config = new JsonObject();
                }
                catch (InvalidOperationException)
                {
                    Config = new JsonObject();
                }
                catch (FormatException)
                {
                    Config = new JsonObject();
                }
            }
        }
    }

    /// <summary>
    /// The permission item entity.
    /// </summary>
    [DataContract]
    public class UserPermissionItemEntity : BasePermissionItemEntity<UserEntity>
    {
        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public override SecurityEntityTypes TargetType => SecurityEntityTypes.User;
    }

    /// <summary>
    /// The permission item entity.
    /// </summary>
    [DataContract]
    public class UserGroupPermissionItemEntity : BasePermissionItemEntity<UserGroupEntity>
    {
        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public override SecurityEntityTypes TargetType => SecurityEntityTypes.UserGroup;
    }

    /// <summary>
    /// The permission item entity.
    /// </summary>
    [DataContract]
    public class ClientPermissionItemEntity : BasePermissionItemEntity<AccessingClientEntity>
    {
        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public override SecurityEntityTypes TargetType => SecurityEntityTypes.ServiceClient;
    }
}
