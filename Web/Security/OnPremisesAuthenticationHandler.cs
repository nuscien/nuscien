using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuScien.Web;
using Trivial.Security;

namespace NuScien.Security
{
    /// <summary>
    /// The authentication options.
    /// </summary>
    public class OnPremisesAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    /// <summary>
    /// The authentication handler.
    /// </summary>
    public class OnPremisesAuthenticationHandler : AuthenticationHandler<OnPremisesAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the OnPremisesAuthenticationHandler class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">A factory to configure the logging system and create instances of logger.</param>
        /// <param name="encoder">The URL encoder.</param>
        /// <param name="clock">The system clock.</param>
        public OnPremisesAuthenticationHandler(IOptionsMonitor<OnPremisesAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        /// <summary>
        /// Processes authentication.
        /// </summary>
        /// <returns>The authenticate result.</returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var client = await ControllerExtensions.GetResourceAccessClientAsync(Request);
            var tokenInfo = client?.Token;
            if (tokenInfo.IsEmpty || tokenInfo.User == null) return AuthenticateResult.Fail(new UnauthorizedAccessException(tokenInfo.ErrorDescription));
            var principal = new ClaimsPrincipal((ClaimsIdentity)tokenInfo.User);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, "bearer"));
        }
    }
}
