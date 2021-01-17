using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuScien.Data;
using NuScien.Security;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Web
{
    /// <summary>
    /// The passport and settings controller.
    /// </summary>
    [Route("nuscien5")]
    public partial class ResourceAccessController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the ResourceAccessController class.
        /// </summary>
        public ResourceAccessController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceAccessController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ResourceAccessController(ILogger<ResourceAccessController> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger<ResourceAccessController> Logger { get; set; }
    }
}
