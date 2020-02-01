using System;
using System.Collections.Generic;
using System.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The permission item constants.
    /// </summary>
    public static class PermissionItems
    {
        /// <summary>
        /// The permission management for a site.
        /// </summary>
        public const string PermissionManagement = "site-perm";

        /// <summary>
        /// The group administration.
        /// </summary>
        public const string GroupManagement = "site-group";

        /// <summary>
        /// The site information manager.
        /// </summary>
        public const string SiteInformationManagement = "site-modify";

        /// <summary>
        /// The site administration.
        /// </summary>
        public const string SiteAdmin = "site-admin";
    }
}
