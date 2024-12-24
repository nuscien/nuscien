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

namespace NuScien.Security;

/// <summary>
/// The on-premises principal.
/// </summary>
public class OnPremisesPrincipal : ClaimsPrincipal
{
    /// <summary>
    /// Initializes a new instance of the OnPremisesPrincipal class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    internal OnPremisesPrincipal(BaseResourceAccessClient client)
        : base ((ClaimsIdentity)client?.User)
    {
        ResourceAccessClient = client;
    }

    /// <summary>
    /// Gets the resource access client.
    /// </summary>
    public BaseResourceAccessClient ResourceAccessClient { get; }
}

/// <summary>
/// The authentication options.
/// </summary>
public class OnPremisesAuthenticationOptions : AuthenticationSchemeOptions
{
}

/// <summary>
/// The authentication handler.
/// </summary>
/// <param name="options">The options.</param>
/// <param name="logger">A factory to configure the logging system and create instances of logger.</param>
/// <param name="encoder">The URL encoder.</param>
public class OnPremisesAuthenticationHandler(IOptionsMonitor<OnPremisesAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<OnPremisesAuthenticationOptions>(options, logger, encoder)
{
    /// <summary>
    /// Processes authentication.
    /// </summary>
    /// <returns>The authenticate result.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var client = await ControllerExtensions.GetResourceAccessClientAsync(Request);
        if (!client.IsUserSignedIn) return AuthenticateResult.Fail(new UnauthorizedAccessException(client.Token?.ErrorDescription ?? "No content to login or invalid access token."));
        var principal = new OnPremisesPrincipal(client);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, "bearer"));
    }
}
