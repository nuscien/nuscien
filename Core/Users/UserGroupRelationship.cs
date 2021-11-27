using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
    [Table("nsusergrouprelas")]
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
        }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        [JsonPropertyName("role")]
        [JsonConverter(typeof(JsonIntegerEnumCompatibleConverter))]
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
        [JsonIgnore]
        [Column("role")]
        public int RoleCode
        {
            get => (int)Role;
            set => Role = (Roles)value;
        }
    }

    /// <summary>
    /// The collection of user group relationship.
    /// </summary>
    public class UserGroupRelationshipCollection
    {
        /// <summary>
        /// The user list related.
        /// </summary>
        private List<UserEntity> users;

        /// <summary>
        /// The user group list related.
        /// </summary>
        private List<UserGroupEntity> groups;

        /// <summary>
        /// The user group relationship list.
        /// </summary>
        private List<UserGroupRelationshipEntity> relationships;

        /// <summary>
        /// Initializes a new instance of the UserGroupRelationshipCollection class.
        /// </summary>
        public UserGroupRelationshipCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the UserGroupRelationshipCollection class.
        /// </summary>
        /// <param name="col">The relationship collection.</param>
        public UserGroupRelationshipCollection(IEnumerable<UserGroupRelationshipEntity> col)
        {
            Relationships = col;
        }

        /// <summary>
        /// Gets or sets the user list related.
        /// </summary>
        [JsonPropertyName("users")]
        public IEnumerable<UserEntity> Users
        {
            get
            {
                return users;
            }

            set
            {
                if (users == value) return;
                users = value.DistinctById().ToList();
                if (value == null || relationships == null) return;
                foreach (var item in relationships)
                {
                    var id = item?.TargetId;
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (item.Target == null) item.Target = value.FirstOrDefault(ele => ele?.Id == id);
                }
            }
        }

        /// <summary>
        /// Gets or sets the user group list related.
        /// </summary>
        [JsonPropertyName("groups")]
        public IEnumerable<UserGroupEntity> Groups
        {
            get
            {
                return groups;
            }

            set
            {
                if (groups == value) return;
                groups = value.DistinctById().ToList();
                if (value == null || relationships == null) return;
                foreach (var item in relationships)
                {
                    var id = item?.OwnerId;
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (item.Owner == null) item.Owner = value.FirstOrDefault(ele => ele?.Id == id);
                }
            }
        }

        /// <summary>
        /// Gets or sets the user group relationship list.
        /// </summary>
        [JsonPropertyName("relas")]
        public IEnumerable<UserGroupRelationshipEntity> Relationships
        {
            get
            {
                return relationships;
            }

            set
            {
                if (relationships == value) return;
                relationships = value.Where(ele => !string.IsNullOrWhiteSpace(ele.Id)).ToList();
                if (value == null) return;
                if (users == null) users = value.Select(ele => ele.Target).DistinctById().ToList();
                if (groups == null) groups = value.Select(ele => ele.Owner).DistinctById().ToList();
            }
        }
    }
}
