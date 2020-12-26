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
    public partial class AccessController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the AccessController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AccessController(ILogger<AccessController> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger<AccessController> Logger { get; set; }
    }
}
