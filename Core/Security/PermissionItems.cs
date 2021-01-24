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

        /// <summary>
        /// The CMS administration.
        /// </summary>
        public const string CmsAdmin = "cms-admin";

        /// <summary>
        /// The CMS insertion.
        /// </summary>
        public const string CmsInsertion = "cms-add";

        /// <summary>
        /// The CMS request.
        /// </summary>
        public const string CmsRequest = "cms-request";

        /// <summary>
        /// The CMS modification.
        /// </summary>
        public const string CmsModification = "cms-modify";

        /// <summary>
        /// The CMS deletion.
        /// </summary>
        public const string CmsDeletion = "cms-delete";

        /// <summary>
        /// The CMS template management.
        /// </summary>
        public const string CmsTemplate = "cms-template";

        /// <summary>
        /// The CMS comments management.
        /// </summary>
        public const string CmsComments = "cms-comments";
    }
}
