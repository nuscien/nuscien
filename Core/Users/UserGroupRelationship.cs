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
    /// User group membership policies.
    /// </summary>
    public enum UserGroupMembershipPolicies
    {
        /// <summary>
        /// Disallow to join in.
        /// </summary>
        Forbidden = 0,

        /// <summary>
        /// Need apply for membership with approval.
        /// </summary>
        Application = 1,

        /// <summary>
        /// Allow to join in directly.
        /// </summary>
        Allow = 2
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
            /// Member.
            /// </summary>
            Member = 0,

            /// <summary>
            /// Power user.
            /// </summary>
            PowerUser = 1,

            /// <summary>
            /// Co-administrator.
            /// </summary>
            Master = 2,

            /// <summary>
            /// Owner.
            /// </summary>
            Owner = 3
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
        /// Gets or sets the role.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public Roles Role
        {
            get => GetCurrentProperty<Roles>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the role code.
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
