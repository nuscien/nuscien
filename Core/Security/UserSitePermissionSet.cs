using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The permission set.
    /// </summary>
    public interface IPermissionSet
    {
        /// <summary>
        /// Tests if contains the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasPermission(string value);

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAnyPermission(string value, params string[] otherValues);

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAnyPermission(IEnumerable<string> values);

        /// <summary>
        /// Tests if contains all of the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAllPermission(string value, params string[] otherValues);

        /// <summary>
        /// Tests if contains all of the specific permission item.
        /// </summary>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAllPermission(IEnumerable<string> values);
    }

    /// <summary>
    /// The permission set for a user per site.
    /// </summary>
    public class UserSitePermissionSet : IPermissionSet
    {
        /// <summary>
        /// Initializes a new instance of the UserSitePermissionSet class.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        public UserSitePermissionSet(string siteId)
        {
            SiteId = siteId;
        }

        /// <summary>
        /// Initializes a new instance of the UserSitePermissionSet class.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="userPermissions">The user permissions.</param>
        /// <param name="groupPermissions">The user group permissions.</param>
        public UserSitePermissionSet(string siteId, IList<UserPermissionItemEntity> userPermissions, IList<UserGroupPermissionItemEntity> groupPermissions)
            : this(siteId)
        {
            UserPermissions = userPermissions;
            GroupPermissions = groupPermissions;
            CacheTime = DateTime.Now;
        }

        /// <summary>
        /// Gets the site identifier.
        /// </summary>
        public string SiteId { get; }

        /// <summary>
        /// Gets the cache date time.
        /// </summary>
        public DateTime CacheTime { get; protected internal set; }

        /// <summary>
        /// Gets a value indicating whether has user permission cache.
        /// </summary>
        public bool HasUserPermissionCache => UserPermissions != null;

        /// <summary>
        /// Gets a value indicating whether has user group permission cache.
        /// </summary>
        public bool HasGroupPermissionCache => GroupPermissions != null;

        /// <summary>
        /// Gets the user permissions.
        /// </summary>
        protected internal IEnumerable<UserPermissionItemEntity> UserPermissions { get; internal set; }

        /// <summary>
        /// Gets the user group permissions.
        /// </summary>
        protected internal IEnumerable<UserGroupPermissionItemEntity> GroupPermissions { get; internal set; }

        /// <summary>
        /// Gets the collection of the group identifier joined in.
        /// </summary>
        public IEnumerable<string> GroupIds => GroupPermissions.Select(ele => ele.TargetId).Distinct();

        /// <summary>
        /// Tests if contains the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasPermission(string value)
        {
            return HasPermission(ele => ele?.HasPermission(value) == true);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAnyPermission(string value, params string[] otherValues)
        {
            return HasPermission(ele => ele?.HasAnyPermission(value, otherValues) == true);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAnyPermission(IEnumerable<string> values)
        {
            return HasPermission(ele => ele?.HasAnyPermission(values) == true);
        }

        /// <summary>
        /// Tests if contains all of the specific permission item.
        /// </summary>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool HasAllPermission(string value, params string[] otherValues)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!HasPermission(value)) return false;
            }

            foreach (var item in otherValues)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (!HasPermission(value)) return false;
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
            foreach (var item in values)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (!HasPermission(item)) return false;
            }

            return true;
        }

        private bool HasPermission(Func<BasePermissionItemEntity, bool> filter)
        {
            return HasPermission(UserPermissions, filter)
                || HasPermission(GroupPermissions, filter);
        }

        private static bool HasPermission<T>(IEnumerable<T> set, Func<T, bool> filter) where T : BasePermissionItemEntity
        {
            return set != null && set.FirstOrDefault(filter) != null;
        }
    }
}
