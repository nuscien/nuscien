﻿using System;
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
    /// User group visibilities.
    /// </summary>
    public enum UserGroupVisibilities
    {
        /// <summary>
        /// The group information is private.
        /// </summary>
        Hidden = 0,

        /// <summary>
        /// The group information is visible but the members are private for guest.
        /// </summary>
        MembersHidden = 1,

        /// <summary>
        /// Public.
        /// </summary>
        Visible = 2
    }

    /// <summary>
    /// User group information.
    /// </summary>
    [DataContract]
    [Table("nsusergroups")]
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

        /// <summary>
        /// Gets or sets the owner site identifier.
        /// </summary>
        [DataMember(Name = "site")]
        [JsonPropertyName("site")]
        [Column("site")]
        public string OwnerSiteId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(string.IsNullOrWhiteSpace(value) ? null : value.Trim());
        }

        /// <summary>
        /// Gets or sets the membership policy.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public UserGroupMembershipPolicies MembershipPolicy
        {
            get => GetCurrentProperty<UserGroupMembershipPolicies>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the membership policy code.
        /// </summary>
        [DataMember(Name = "membership")]
        [JsonPropertyName("membership")]
        [Column("membership")]
        public int MembershipPolicyCode
        {
            get => (int)MembershipPolicy;
            set => MembershipPolicy = (UserGroupMembershipPolicies)value;
        }

        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public UserGroupVisibilities Visibility
        {
            get => GetCurrentProperty<UserGroupVisibilities>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the visibility code.
        /// </summary>
        [DataMember(Name = "visible")]
        [JsonPropertyName("visible")]
        [Column("visible")]
        public int VisibilityCode
        {
            get => (int)Visibility;
            set => Visibility = (UserGroupVisibilities)value;
        }
    }
}
