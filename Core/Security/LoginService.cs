using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using NuScien.Users;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;

namespace NuScien.Security
{
    public enum SecurityEntityTypes
    {
        Unknown = 0,
        User = 1,
        Service = 2
    }

    /// <summary>
    /// The login service provider.
    /// </summary>
    public interface ILoginServiceProvider
    {
        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> LoginAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> LoginAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> LoginAsync(TokenRequest<CodeTokenRequestBody> tokenRequest);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <returns>The login response.</returns>
        public Task<TokenInfo> LoginAsync(TokenRequest<ClientTokenRequestBody> tokenRequest);

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The login response.</returns>
        public Task<UserTokenInfo> AuthAsync(string accessToken);

        /// <summary>
        /// Registers or updates.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <returns>Async task.</returns>
        public Task Save(UserEntity user);

        /// <summary>
        /// Gets the user groups list.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A user group information list.</returns>
        public Task<IEnumerable<UserGroupEntity>> ListGroups(UserEntity user);

        /// <summary>
        /// Gets the user groups list.
        /// </summary>
        /// <param name="q">The name query; null for all.</param>
        /// <returns>A user group information list.</returns>
        public Task<IEnumerable<UserGroupEntity>> ListGroups(string q);

        /// <summary>
        /// Gets a specific group.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The user info.</returns>
        public Task<UserEntity> GetUser(string id);

        /// <summary>
        /// Gets a specific group.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The group info.</returns>
        public Task<UserGroupEntity> GetGroup(string id);

        /// <summary>
        /// Creates or updates.
        /// </summary>
        /// <param name="group">The user group information.</param>
        /// <returns>Async task.</returns>
        public Task Save(UserGroupEntity group);
    }

    /// <summary>
    /// The login service.
    /// </summary>
    public class LoginService : TokenRequestRoute<UserEntity>
    {
        /// <summary>
        /// The options.
        /// </summary>
        public class OptionsInfo
        {
            /// <summary>
            /// Gets or sets the error code for empty provider.
            /// </summary>
            public string EmptyProviderCode { get; set; }

            /// <summary>
            /// Gets or sets the error description for empty provider.
            /// </summary>
            public string EmptyProviderDescription { get; set; }

            /// <summary>
            /// Gets or sets the error URI for empty provider.
            /// </summary>
            public Uri EmptyProviderUri { get; set; }
        }

        /// <summary>
        /// The service provider.
        /// </summary>
        private readonly ILoginServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the LoginService class.
        /// </summary>
        public LoginService(ILoginServiceProvider provider)
        {
            if (provider == null) return;
            serviceProvider = provider;

            Register(PasswordTokenRequestBody.PasswordGrantType, q =>
            {
                return PasswordTokenRequestBody.Parse(q.ToString());
            }, async q =>
            {
                var r = await serviceProvider.LoginAsync(q);
                return (r.User, r);
            });
            Register(RefreshTokenRequestBody.RefreshTokenGrantType, q =>
            {
                return RefreshTokenRequestBody.Parse(q.ToString());
            }, async q =>
            {
                var r = await serviceProvider.LoginAsync(q);
                return (r.User, r);
            });
            Register(CodeTokenRequestBody.AuthorizationCodeGrantType, q =>
            {
                return CodeTokenRequestBody.Parse(q.ToString());
            }, async q =>
            {
                var r = await serviceProvider.LoginAsync(q);
                return (r.User, r);
            });
            Register(ClientTokenRequestBody.ClientCredentialsGrantType, q =>
            {
                return ClientTokenRequestBody.Parse(q.ToString());
            }, async q =>
            {
                var r = await serviceProvider.LoginAsync(q);
                return (null, r);
            });
        }

        /// <summary>
        /// The options.
        /// </summary>
        public OptionsInfo Options { get; } = new OptionsInfo();

        /// <summary>
        /// Signs in by access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The login response.</returns>
        public async Task<UserTokenInfo> AuthAsync(string accessToken)
        {
            if (serviceProvider == null) return new UserTokenInfo
            {
                ErrorCode = string.IsNullOrWhiteSpace(Options.EmptyProviderCode) ? "NotImplemented" : Options.EmptyProviderCode,
                ErrorDescription = string.IsNullOrWhiteSpace(Options.EmptyProviderDescription) ? "No implementation injected." : Options.EmptyProviderDescription,
                ErrorUri = Options.EmptyProviderUri
            };
            var bearer = TokenInfo.BearerTokenType + " ";
            if (accessToken.StartsWith(bearer)) accessToken = accessToken.Substring(bearer.Length);
            return await serviceProvider.AuthAsync(accessToken);
        }
    }
}
