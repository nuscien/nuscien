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

        /// <summary>
        /// Gets the permission items as a dictionary.
        /// </summary>
        /// <returns>The permission items.</returns>
        public IReadOnlyList<PermissionItemStatus> GetItems();
    }

    /// <summary>
    /// Permission item status.
    /// </summary>
    public class PermissionItemStatus
    {
        /// <summary>
        /// Initializes a new instance of the PermissionItemStatus class.
        /// </summary>
        /// <param name="value">The permission item.</param>
        /// <param name="isSetToUser">true if it is set to user; otherwise, false.</param>
        /// <param name="groups">The group list.</param>
        public PermissionItemStatus(string value, bool isSetToUser, IEnumerable<string> groups)
        {
            Value = value;
            IsSetToUser = isSetToUser;
            Groups = (groups?.ToList() ?? new List<string>()).AsReadOnly();
        }

        /// <summary>
        /// Gets the permission item.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether contains the permission item.
        /// </summary>
        public bool HasPermission => IsSetToUser || Groups.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the permission item is set .
        /// </summary>
        public bool IsSetToUser { get; }

        /// <summary>
        /// Gets a value indicating whether the permission item is from group.
        /// </summary>
        public bool IsFromGroup => Groups.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the permission item is derived.
        /// </summary>
        public bool IsDerived => !IsSetToUser && Groups.Count > 0;

        /// <summary>
        /// Gets the group identifiers.
        /// </summary>
        public IReadOnlyList<string> Groups { get; set; }
    }

    /// <summary>
    /// The permission set for a user per site.
    /// </summary>
    public class UserSitePermissionSet : IPermissionSet
    {
        private IReadOnlyList<PermissionItemStatus> items;

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
        /// <param name="userPermission">The user permissions.</param>
        /// <param name="groupPermissions">The user group permissions.</param>
        public UserSitePermissionSet(string siteId, UserPermissionItemEntity userPermission, IList<UserGroupPermissionItemEntity> groupPermissions)
            : this(siteId)
        {
            UserPermission = userPermission;
            GroupPermissions = groupPermissions;
            CacheTime = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the UserSitePermissionSet class.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="userPermission">The user permissions.</param>
        /// <param name="clientPermission">The verified client permissions.</param>
        /// <param name="groupPermissions">The user group permissions.</param>
        public UserSitePermissionSet(string siteId, UserPermissionItemEntity userPermission, ClientPermissionItemEntity clientPermission, IList<UserGroupPermissionItemEntity> groupPermissions)
            : this(siteId)
        {
            UserPermission = userPermission;
            ClientPermission = clientPermission;
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
        /// Gets a value indicating whether has user group permission cache.
        /// </summary>
        public bool HasGroupPermissionCache => GroupPermissions != null;

        /// <summary>
        /// Gets the user permissions.
        /// </summary>
        protected internal UserPermissionItemEntity UserPermission { get; internal set; }

        /// <summary>
        /// Gets the user group permissions.
        /// </summary>
        protected internal IEnumerable<UserGroupPermissionItemEntity> GroupPermissions { get; internal set; }

        /// <summary>
        /// Gets the client permissions.
        /// </summary>
        protected internal ClientPermissionItemEntity ClientPermission { get; internal set; }

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
            if (items != null)
            {
                var col = new List<string>();
                if (!string.IsNullOrWhiteSpace(value)) col.Add(value);
                col.AddRange(otherValues);
                foreach (var item in col)
                {
                    if (items.FirstOrDefault(ele => ele.Value == item) == null) return false;
                }

                return true;
            }

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
            if (items != null)
            {
                foreach (var item in values)
                {
                    if (items.FirstOrDefault(ele => ele.Value == item) == null) return false;
                }

                return true;
            }

            foreach (var item in values)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (!HasPermission(item)) return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the permission items as a dictionary.
        /// </summary>
        /// <returns>The permission items.</returns>
        public IReadOnlyList<PermissionItemStatus> GetItems()
        {
            if (items != null) return items;
            var temp = new Dictionary<string, List<string>>();
            foreach (var item in GroupPermissions)
            {
                var perms = item.GetPermissionList();
                foreach (var p in perms)
                {
                    if (string.IsNullOrWhiteSpace(p)) continue;
                    var key = p.Trim();
                    if (!temp.ContainsKey(key)) temp[key] = new List<string>();
                    temp[key].Add(item.TargetId);
                }
            }

            if (items != null) return items;
            var col = UserPermission?.GetPermissionList()?.ToList() ?? new List<string>();
            if (ClientPermission != null) col.AddRange(ClientPermission.GetPermissionList());
            items = col.Distinct().Select(p =>
            {
                if (string.IsNullOrWhiteSpace(p)) return null;
                var key = p.Trim();
                temp.TryGetValue(key, out var g);
                return new PermissionItemStatus(key, true, g);
            }).Where(p => p != null).ToList().AsReadOnly();
            return items;
        }

        private bool HasPermission(Func<BasePermissionItemEntity, bool> filter)
        {
            return filter(UserPermission) || HasPermission(GroupPermissions, filter) || filter(ClientPermission);
        }

        private static bool HasPermission<T>(IEnumerable<T> set, Func<T, bool> filter) where T : BasePermissionItemEntity
        {
            return set != null && set.FirstOrDefault(filter) != null;
        }
    }
}
