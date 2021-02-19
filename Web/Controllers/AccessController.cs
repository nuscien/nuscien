using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Gets the core client script file.
        /// </summary>
        /// <returns>A JavaScript file result.</returns>
        [HttpGet]
        [Route("js/nuscien.js")]
        public IActionResult GetClientScript()
        {
            var fileProvider = new Microsoft.Extensions.FileProviders.EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            var file = fileProvider.GetFileInfo("core.js");
            if (!file.Exists) return NotFound();
            return File(file.CreateReadStream(), WebFormat.JavaScriptMIME, "nuscien.core.js");
        }

        /// <summary>
        /// Gets the core client script file.
        /// </summary>
        /// <returns>A JavaScript file result.</returns>
        [HttpGet]
        [Route("js/nuscien.d.ts")]
        public IActionResult GetClientScriptDefinition()
        {
            var fileProvider = new Microsoft.Extensions.FileProviders.EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            var file = fileProvider.GetFileInfo("core.d.ts");
            if (!file.Exists) return NotFound();
            return File(file.CreateReadStream(), "application/x-typescript", "nuscien.core.d.ts");
        }
    }
}
