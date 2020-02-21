using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuScien.Security;
using Trivial.Security;

namespace NuScien.Web.Controllers
{
    /// <summary>
    /// The passport controller.
    /// </summary>
    [ApiController]
    public class PassportController : ControllerBase
    {
        #pragma warning disable IDE0052
        private readonly ILogger<PassportController> _logger;
        #pragma warning restore IDE0052

        /// <summary>
        /// Initializes a new instance of the PassportController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PassportController(ILogger<PassportController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("api/passport/login")]
        [Route("nuscien5/passport/login")]
        public async Task<UserTokenInfo> LoginAsync()
        {
            var instance = await ResourceAccessClients.ResolveAsync();
            return await instance.SignInAsync(Request.Body);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpGet]
        [Route("api/passport/login")]
        [Route("nuscien5/passport/login")]
        public async Task<UserTokenInfo> TestTokenAsync()
        {
            var instance = await ResourceAccessClients.ResolveAsync();
            return await instance.AuthorizeAsync(Request.Headers);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("api/passport/logout")]
        [Route("nuscien5/passport/logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var instance = await ResourceAccessClients.ResolveAsync();
            await instance.SignOutAsync();
            return Ok();
        }
    }
}
