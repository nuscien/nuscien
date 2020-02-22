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
using Trivial.Net;
using Trivial.Security;

namespace NuScien.Web.Controllers
{
    /// <summary>
    /// The passport controller.
    /// </summary>
    [ApiController]
    [Route("api")]
    [Route("nuscien5")]
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
        [Route("passport/login")]
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
        [Route("passport/login")]
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
        [Route("passport/logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var instance = await ResourceAccessClients.ResolveAsync();
            await instance.SignOutAsync();
            return Ok();
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <returns>The user token information.</returns>
        [HttpPost]
        [Route("passport/users/exist")]
        public async Task<IActionResult> IsUserExistedAsync()
        {
            var key = Request.Form["key"].FirstOrDefault();
            var value = Request.Form["value"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return BadRequest();
            var instance = await ResourceAccessClients.ResolveAsync();
            switch (key)
            {
                case "logname":
                case "name":
                    if (await instance.HasUserNameAsync(value)) return Ok();
                    return NotFound();
                default:
                    return StatusCode(501);
            }
        }
    }
}
