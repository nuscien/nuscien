using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
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
    /// Login and session information.
    /// </summary>
    public class LoginSessionInfo
    {
        /// <summary>
        /// The user groups.
        /// </summary>
        private IList<UserGroupEntity> groups;

        /// <summary>
        /// The service provider.
        /// </summary>
        private readonly ILoginServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the LoginSessionInfo class.
        /// </summary>
        /// <param name="provider">The login service provider.</param>
        /// <param name="user">The user information.</param>
        /// <param name="tokenType">The token type.</param>
        /// <param name="accessToken">The access token.</param>
        internal LoginSessionInfo(ILoginServiceProvider provider, UserEntity user, string tokenType, string accessToken)
        {
            serviceProvider = provider;
            User = user;
            if (string.IsNullOrWhiteSpace(accessToken)) return;
            AccessToken = accessToken;
            TokenType = tokenType;
            TokenCopy = new UserTokenInfo
            {
                User = user,
                TokenType = tokenType,
                UserId = user?.Id,
                AccessToken = accessToken
            };
        }

        /// <summary>
        /// Initializes a new instance of the LoginSessionInfo class.
        /// </summary>
        /// <param name="provider">The login service provider.</param>
        /// <param name="user">The user information.</param>
        /// <param name="token">The token information.</param>
        internal LoginSessionInfo(ILoginServiceProvider provider, UserEntity user, UserTokenInfo token)
        {
            serviceProvider = provider;
            User = user;
            if (token == null) return;
            AccessToken = token.AccessToken;
            TokenType = token.TokenType;
            TokenCopy = token;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        internal string TokenType { get; }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        internal string AccessToken { get; }

        /// <summary>
        /// Gets the token information instance.
        /// </summary>
        public UserTokenInfo TokenCopy { get; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public UserEntity User { get; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        public string UserId => User?.Id;

        /// <summary>
        /// Gets the user groups.
        /// </summary>
        /// <returns>A user group list.</returns>
        public async Task<IList<UserGroupEntity>> GetGroupsAsync()
        {
            if (groups != null) return groups;
            if (serviceProvider == null || User == null)
            {
                groups = new List<UserGroupEntity>();
                return groups;
            }

            var col = await serviceProvider.ListGroups(User);
            groups = col.ToList();
            return groups;
        }
    }

    /// <summary>
    /// Token information.
    /// </summary>
    [DataContract]
    public class UserTokenInfo : TokenInfo
    {
        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        [DataMember(Name = "user", EmitDefaultValue = true)]
        [JsonPropertyName("user")]
        public UserEntity User { get; set; }

        /// <summary>
        /// Creates an error token information.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="errorCode">The optional error code.</param>
        /// <returns>The user token information instance with error information.</returns>
        public static UserTokenInfo CreateError(UserEntity user, Exception ex, string errorCode = null)
        {
            return new UserTokenInfo
            {
                User = user,
                UserId = user?.Id,
                ResourceId = user?.Id,
                ErrorCode = errorCode ?? ErrorCodeConstants.ServerError,
                ErrorDescription = ex?.Message
            };
        }

        /// <summary>
        /// Creates an error token information.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="clientId">The app client identifier.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="errorCode">The optional error code.</param>
        /// <returns>The user token information instance with error information.</returns>
        public static UserTokenInfo CreateError(UserEntity user, string clientId, Exception ex, string errorCode = null)
        {
            return new UserTokenInfo
            {
                User = user,
                UserId = user?.Id,
                ResourceId = user is null ? clientId : user.Id,
                ErrorCode = errorCode ?? ErrorCodeConstants.ServerError,
                ErrorDescription = ex?.Message
            };
        }
    }
}
