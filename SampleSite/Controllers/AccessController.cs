﻿using System;
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

namespace NuScien.Web.Controllers
{
    /// <summary>
    /// The passport and settings controller.
    /// </summary>
    [ApiController]
    [Route("api")]
    [Route("nuscien5")]
    public class AccessController : NuScien.Web.AccessController
    {
        /// <summary>
        /// Initializes a new instance of the AccessController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AccessController(ILogger<AccessController> logger) : base(logger)
        {
        }
    }
}