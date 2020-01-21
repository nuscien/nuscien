using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;

namespace NuScien.Security
{
    /// <summary>
    /// The base resource access client.
    /// </summary>
    public abstract class BaseResourceAccessClient : TokenContainer
    {
        /// <summary>
        /// Gets or sets the token cache.
        /// </summary>
        public new UserTokenInfo Token
        {
            get => base.Token as UserTokenInfo;
            set => base.Token = value;
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<CodeTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> LoginAsync(TokenRequest<ClientTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="accessToken">The access request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public abstract Task<UserTokenInfo> AuthorizeAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}
