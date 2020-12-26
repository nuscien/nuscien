using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trivial.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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
        /// The header key of authorization.
        /// </summary>
        public const string TokenHeaderKey = "Authorization";

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
            var client = await ResourceAccessClients.ResolveAsync();
            var bearerToken = TryGetStringValue(Request.Headers, TokenHeaderKey);
            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                if (bearerToken.ToLowerInvariant().StartsWith("bearer "))
                {
                    var bearerTokenString = bearerToken.Substring(7).Trim();
                    var tokenInfo2 = await client.AuthorizeAsync(bearerTokenString);
                    return ConvertToAuthenticateResult(tokenInfo2, "bearer");
                }
                else if (bearerToken.ToLowerInvariant().StartsWith("basic "))
                {
                    var basicStr = bearerToken.Substring(6).Trim();
                    var basicBytes = Convert.FromBase64String(basicStr);
                    var basicArr = Encoding.UTF8.GetString(basicBytes)?.Split(':');
                    if (basicArr != null && basicArr.Length == 2)
                    {
                        var tokenInfo1 = await client.SignInByPasswordAsync(new AppAccessingKey(), basicArr[0], basicArr[1]);
                        return ConvertToAuthenticateResult(tokenInfo1, "oauth");
                    }
                }
            }

            var tokenInfo = await client.SignInAsync(Request.Body);
            return ConvertToAuthenticateResult(tokenInfo, "oauth");
        }

        private static AuthenticateResult ConvertToAuthenticateResult(UserTokenInfo tokenInfo, string authenticationScheme)
        {
            if (tokenInfo.IsEmpty || tokenInfo.User == null) return AuthenticateResult.Fail(new UnauthorizedAccessException(tokenInfo.ErrorDescription));
            var principal = new ClaimsPrincipal((ClaimsIdentity)tokenInfo.User);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, authenticationScheme));
        }

        /// <summary>
        /// Tries to get the string value.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="key">The header key.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        private static string TryGetStringValue(IHeaderDictionary header, string key)
        {
            if (!header.TryGetValue(key, out var col)) return null;
            return col.FirstOrDefault(ele => !string.IsNullOrWhiteSpace(ele));
        }
    }
}
