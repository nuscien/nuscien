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
    /// <summary>
    /// Security entity types.
    /// </summary>
    public enum SecurityEntityTypes
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// User.
        /// </summary>
        User = 1,

        /// <summary>
        /// Service.
        /// </summary>
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
}
