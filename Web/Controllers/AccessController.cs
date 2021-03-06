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
    public partial class ResourceAccessControllerBase : ControllerBase
    {
        private const string assemblyVersionPrefix = "5.0";

        /// <summary>
        /// Initializes a new instance of the ResourceAccessControllerBase class.
        /// </summary>
        public ResourceAccessControllerBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceAccessControllerBase class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ResourceAccessControllerBase(ILogger<ResourceAccessControllerBase> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger<ResourceAccessControllerBase> Logger { get; set; }

        /// <summary>
        /// Gets the core client script file.
        /// </summary>
        /// <returns>A JavaScript file result.</returns>
        [HttpGet]
        [Route("js/nuscien.js")]
        [Route("js/nuscien.core.js")]
        public IActionResult GetClientScript()
        {
            var ver = (Request.Query?["ver"])?.ToString();
            if (string.IsNullOrEmpty(ver) || ver.StartsWith(assemblyVersionPrefix) || ver.StartsWith("1.0"))
                return GetEmbeddedJsFile("core.js", "nuscien.core.js");
            return NotFound();
        }

        /// <summary>
        /// Gets the core client script file.
        /// </summary>
        /// <returns>A JavaScript file result.</returns>
        [HttpGet]
        [Route("js/nuscien.d.ts")]
        [Route("js/nuscien.core.d.ts")]
        public IActionResult GetClientScriptDefinition()
        {
            var ver = (Request.Query?["ver"])?.ToString();
            if (string.IsNullOrEmpty(ver) || ver.StartsWith(assemblyVersionPrefix) || ver.StartsWith("1.0"))
                return GetEmbeddedJsFile("core.d.ts", "nuscien.core.d.ts", "application/x-typescript");
            return NotFound();
        }

        private IActionResult GetEmbeddedJsFile(string subpath, string downloadName, string mime = null)
        {
            return GetEmbeddedFile("NuScien.Js." + subpath, downloadName, mime ?? WebFormat.JavaScriptMIME);
        }

        private IActionResult GetEmbeddedFile(string subpath, string downloadName, string mime)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(subpath);
            if (stream == null) return NotFound();
            return File(stream, mime, downloadName);

            //var fileProvider = new Microsoft.Extensions.FileProviders.EmbeddedFileProvider(assembly);
            //var file = fileProvider.GetFileInfo(subpath);
            //if (!file.Exists) return NotFound();
            //return File(file.CreateReadStream(), mime, downloadName);
        }
    }
}
