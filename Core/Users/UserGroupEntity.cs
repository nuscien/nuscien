using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using NuScien.Security;
using Trivial.Text;

namespace NuScien.Users
{
    /// <summary>
    /// User group information.
    /// </summary>
    [DataContract]
    public class UserGroupEntity : BaseSecurityEntity
    {
        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public override SecurityEntityTypes SecurityEntityType => SecurityEntityTypes.UserGroup;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [DataMember(Name = "desc")]
        [JsonPropertyName("desc")]
        [Column("desc")]
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
        [Column("avatar")]
        public string Avatar
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The user group owner relationship resource entity.
    /// </summary>
    [DataContract]
    public class UserGroupResourceEntity : OwnerResourceEntity<UserGroupEntity>
    {
        /// <summary>
        /// Initializes a new instance of the UserGroupResourceEntity class.
        /// </summary>
        public UserGroupResourceEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the UserGroupResourceEntity class.
        /// </summary>
        /// <param name="copy">The source to copy.</param>
        /// <param name="owner">The owner resource entity.</param>
        public UserGroupResourceEntity(OwnerResourceEntity<UserGroupEntity> copy, UserGroupEntity owner)
            : base(copy, owner)
        {
        }
    }

    /// <summary>
    /// The user group owner relationship resource entity.
    /// </summary>
    /// <typeparam name="T">The type of target resource.</typeparam>
    [DataContract]
    public class UserGroupResourceEntity<T> : OwnerResourceEntity<UserGroupEntity, T> where T : BaseResourceEntity
    {
        /// <summary>
        /// Initializes a new instance of the UserGroupResourceEntity class.
        /// </summary>
        public UserGroupResourceEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the UserGroupResourceEntity class.
        /// </summary>
        /// <param name="copy">The source to copy.</param>
        /// <param name="owner">The owner resource entity.</param>
        /// <param name="target">The target resource entity.</param>
        public UserGroupResourceEntity(OwnerResourceEntity<UserGroupEntity, T> copy, UserGroupEntity owner, T target)
            : base(copy, owner, target)
        {
            if (!string.IsNullOrWhiteSpace(target?.Name)) Name = target.Name;
        }
    }

    /// <summary>
    /// The user group and user relationship entity.
    /// </summary>
    [DataContract]
    public class UserGroupRelationshipEntity : UserGroupResourceEntity<UserEntity>
    {
        /// <summary>
        /// The roles in user group.
        /// </summary>
        public enum Roles
        {
            /// <summary>
            /// None.
            /// </summary>
            None = 0,

            /// <summary>
            /// Guest.
            /// </summary>
            Guest = 1,

            /// <summary>
            /// Member.
            /// </summary>
            Member = 2,

            /// <summary>
            /// Information modification.
            /// </summary>
            InformationModification = 3,

            /// <summary>
            /// Member manager.
            /// </summary>
            UserManager = 4,

            /// <summary>
            /// Administrator.
            /// </summary>
            Admin = 5
        }

        /// <summary>
        /// Initializes a new instance of the UserGroupResourceEntity class.
        /// </summary>
        public UserGroupRelationshipEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the UserGroupResourceEntity class.
        /// </summary>
        /// <param name="copy">The source to copy.</param>
        /// <param name="owner">The owner resource entity.</param>
        /// <param name="target">The target resource entity.</param>
        public UserGroupRelationshipEntity(OwnerResourceEntity<UserGroupEntity, UserEntity> copy, UserGroupEntity owner, UserEntity target)
            : base(copy, owner, target)
        {
            if (!string.IsNullOrWhiteSpace(target?.Name)) Name = target.Name;
        }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public Roles Role
        {
            get => GetCurrentProperty<Roles>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        [DataMember(Name = "role")]
        [JsonPropertyName("role")]
        [Column("role")]
        public int RoleCode
        {
            get => (int)Role;
            set => Role = (Roles)value;
        }
    }
}
