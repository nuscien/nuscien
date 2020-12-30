using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
    public abstract class BasePermissionItemEntity : BaseResourceEntity
    {
        /// <summary>
        /// The split string for permission item.
        /// </summary>
        public const string PermissionSplit = "\n";

        private List<string> cache;

        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [Column("site")]
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
        [Required]
        public int TargetTypeCode
        {
            get => (int)TargetType;
            set => _ = value;
        }

        /// <summary>
        /// Gets or sets the target resource identifier.
        /// </summary>
        [Column("target")]
        [DataMember(Name = "target")]
        [JsonPropertyName("target")]
        [Required]
        public string TargetId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the permission list.
        /// </summary>
        [Column("permissions")]
        [DataMember(Name = "permissions")]
        [JsonPropertyName("permissions")]
        public string Permissions
        {
            get
            {
                return GetCurrentProperty<string>();
            }

            set
            {
                cache = null;
                SetCurrentProperty(value);
            }
        }

        /// <summary>
        /// Gets permission list.
        /// </summary>
        /// <returns>The permission list.</returns>
        public IEnumerable<string> GetPermissionList()
        {
            return GetPermissionListInternal().AsReadOnly();
        }

        /// <summary>
        /// Adds the permission item.
        /// </summary>
        /// <param name="value">The permission item to add.</param>
        /// <param name="otherValues">The optional further permissions to add.</param>
        /// <returns>The count of item added.</returns>
        public int AddPermission(string value, params string[] otherValues)
        {
            if (IsPropertyReadonly) return 0;
            var c = GetPermissionListInternal();
            var count = 0;
            if (!string.IsNullOrWhiteSpace(value))
            {
                c.Add(value.Trim());
                count++;
            }

            var col = otherValues.Where(ele => !string.IsNullOrWhiteSpace(ele)).Select(ele => ele.Trim()).ToList();
            c.AddRange(col);
            Permissions = string.Join(PermissionSplit, c);
            cache = c;
            return count + col.Count;
        }

        /// <summary>
        /// Adds the permission item.
        /// </summary>
        /// <param name="values">The permissions to add.</param>
        /// <returns>The count of item added.</returns>
        public int AddPermission(IEnumerable<string> values)
        {
            if (IsPropertyReadonly) return 0;
            var c = GetPermissionListInternal();
            var col = values.Where(ele => !string.IsNullOrWhiteSpace(ele)).Select(ele => ele.Trim()).ToList();
            c.AddRange(col);
            Permissions = string.Join(PermissionSplit, c);
            cache = c;
            return col.Count;
        }

        /// <summary>
        /// Removes the permission item.
        /// </summary>
        /// <param name="value">The permission item to remove.</param>
        /// <param name="otherValues">The optional further permissions to remove.</param>
        /// <returns>The count of item removed.</returns>
        public int RemovePermission(string value, params string[] otherValues)
        {
            if (IsPropertyReadonly) return 0;
            var c = GetPermissionListInternal();
            var count = 0;
            if (!string.IsNullOrWhiteSpace(value))
            {
                c.Remove(value.Trim());
                count++;
            }

            foreach (var item in otherValues)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (c.Remove(item)) count++;
            }

            Permissions = string.Join(PermissionSplit, c);
            cache = c;
            return count;
        }

        /// <summary>
        /// Removes the permission item.
        /// </summary>
        /// <param name="values">The permissions to remove.</param>
        /// <returns>The count of item removed.</returns>
        public int RemovePermission(IEnumerable<string> values)
        {
            if (IsPropertyReadonly) return 0;
            var c = GetPermissionListInternal();
            var count = 0;

            foreach (var item in values)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (c.Remove(item)) count++;
            }

            Permissions = string.Join(PermissionSplit, c);
            cache = c;
            return count;
        }

        /// <summary>
        /// Tests if contains the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasPermission(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var c = GetPermissionList();
            return c.Contains(value.Trim());
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAnyPermission(string value, params string[] otherValues)
        {
            var c = GetPermissionList();
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (c.Contains(value.Trim())) return true;
            }

            foreach (var item in otherValues)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (c.Contains(item.Trim())) return true;
            }

            return false;
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAnyPermission(IEnumerable<string> values)
        {
            if (values == null) return false;
            var c = GetPermissionList();
            foreach (var item in values)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (c.Contains(item.Trim())) return true;
            }

            return false;
        }

        /// <summary>
        /// Tests if contains all of the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAllPermission(string value, params string[] otherValues)
        {
            var c = GetPermissionList();
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!c.Contains(value.Trim())) return false;
            }

            foreach (var item in otherValues)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (!c.Contains(item.Trim())) return false;
            }

            return true;
        }

        /// <summary>
        /// Tests if contains all of the specific permission item.
        /// </summary>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAllPermission(IEnumerable<string> values)
        {
            if (values == null) return false;
            var c = GetPermissionList();
            foreach (var item in values)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (!c.Contains(item.Trim())) return false;
            }

            return true;
        }

        /// <summary>
        /// Gets permission list.
        /// </summary>
        /// <returns>The permission list.</returns>
        private List<string> GetPermissionListInternal()
        {
            var c = cache;
            if (c == null)
            {
                var s = Permissions;
                if (string.IsNullOrWhiteSpace(s)) c = new List<string>();
                else c = StringExtensions.ReadLines(s.Trim(), true).ToList();
                cache = c;
            }

            return c;
        }
    }

    /// <summary>
    /// The permission item entity.
    /// </summary>
    /// <typeparam name="T">The type of target entity.</typeparam>
    [DataContract]
    public abstract class BasePermissionItemEntity<T> : BasePermissionItemEntity
        where T : BaseSecurityEntity
    {
        private T target;

        /// <summary>
        /// Gets or sets the target resource entity.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public T Target
        {
            get
            {
                return target;
            }

            set
            {
                if (value is null) target = null;
                if (string.IsNullOrWhiteSpace(Id)) Id = value.Id;
                target = value;
            }
        }
    }

    /// <summary>
    /// The permission item entity.
    /// </summary>
    [DataContract]
    [Table("nsuserperms")]
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
    [Table("nsusergroupperms")]
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
    [Table("nsclientperms")]
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
