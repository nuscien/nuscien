using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Users;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The third-party login provider.
    /// </summary>
    /// <typeparam name="T">The type of token request body.</typeparam>
    public interface IThirdPartyLoginProvider<T> where T : TokenRequestBody
    {
        /// <summary>
        /// Signs in by the token request body and returns the user information.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A user entity for sign in succeeded; or null for failure.</returns>
        public Task<UserEntity> ProcessAsync(T request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// The authorization code token login provider.
    /// </summary>
    public interface IAuthorizationCodeVerifierProvider : IThirdPartyLoginProvider<CodeTokenRequestBody>
    {
        /// <summary>
        /// Gets a value indicating whether enables to test internal service.
        /// </summary>
        public bool HasSaved { get; }
    }
}
