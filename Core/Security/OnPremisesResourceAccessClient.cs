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
    /// The on-premises resource access client.
    /// </summary>
    public class OnPremisesResourceAccessClient : BaseResourceAccessClient
    {
        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceAccessClient class.
        /// </summary>
        /// <param name="provider">The account data provider.</param>
        public OnPremisesResourceAccessClient(IAccountDataProvider provider)
        {
            DataProvider = provider ?? new FakeAccountDbSetProvider();
        }

        /// <summary>
        /// Gets the account data provider.
        /// </summary>
        protected IAccountDataProvider DataProvider { get; }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            AssertTokenRequest(tokenRequest);
            var userTask = DataProvider.GetUserByLognameAsync(tokenRequest.Body.UserName, cancellationToken);
            return await CreateTokenAsync(userTask, tokenRequest, null, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            AssertTokenRequest(tokenRequest);
            var tokenTask = DataProvider.GetTokenByRefreshTokenAsync(tokenRequest.Body.RefreshToken, cancellationToken);
            return await CreateTokenAsync(tokenTask, tokenRequest.ClientId, null, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<CodeTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            AssertTokenRequest(tokenRequest);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> LoginAsync(TokenRequest<ClientTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            AssertTokenRequest(tokenRequest);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="accessToken">The access request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> AuthorizeAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            InternalAssertion.IsNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            var tokenTask = DataProvider.GetTokenByNameAsync(accessToken, cancellationToken);
            return await CreateTokenAsync(tokenTask, null, null, cancellationToken);
        }

        public async Task<UserTokenInfo> CreateTokenAsync(UserEntity user, TokenRequest tokenRequest, string state, CancellationToken cancellationToken = default)
        {
            InternalAssertion.IsNotNull(tokenRequest, nameof(tokenRequest));
            if (user is null || user.IsNew || string.IsNullOrWhiteSpace(user.Name)) return new UserTokenInfo
            {
                ErrorCode = "invalid_password",
                ErrorDescription = "The login name or password is not correct."
            };
            return await CreateTokenAsync(user, null, tokenRequest.ClientId, null, cancellationToken);
        }

        public async Task<UserTokenInfo> CreateTokenAsync(UserEntity user, TokenEntity token, string clientId, string state, CancellationToken cancellationToken = default)
        {
            if (user is null || user.IsNew || string.IsNullOrWhiteSpace(user.Name)) return new UserTokenInfo
            {
                ErrorCode = "invalid_password",
                ErrorDescription = "The login name or password is not correct."
            };
            var needSave = token is null;
            if (needSave)
            {
                token = new TokenEntity
                {
                    UserId = user.Id,
                    ClientId = clientId
                };
                token.CreateToken(true);
            }
            else if (!string.IsNullOrWhiteSpace(clientId) && token.ClientId != clientId)
            {
                return new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidClient,
                    ErrorDescription = "The client is not for this token."
                };
            }

            try
            {
                if (needSave)
                {
                    var r = await DataProvider.SaveAsync(token, cancellationToken);
                    if (r == ChangeMethods.Invalid) return new UserTokenInfo
                    {
                        User = user,
                        UserId = user.Id,
                        ErrorCode = "failure_token_creation",
                        ErrorDescription = "Generate token failed."
                    };
                }

                return Token = new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    AccessToken = token.Name,
                    RefreshToken = token.RefreshToken,
                    ExpiredAfter = token.ExpirationTime - DateTime.Now,
                    ResourceId = user.Id,
                    TokenType = TokenInfo.BearerTokenType,
                    State = state
                };
            }
            catch (ArgumentException ex)
            {
                return new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (InvalidOperationException ex)
            {
                return new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NotImplementedException ex)
            {
                return new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NullReferenceException ex)
            {
                return new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NotSupportedException ex)
            {
                return new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                return new UserTokenInfo
                {
                    User = user,
                    UserId = user.Id,
                    ErrorCode = TokenInfo.ErrorCodeConstants.AccessDenied,
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<UserTokenInfo> CreateTokenAsync(Task<UserEntity> userResolver, TokenRequest<PasswordTokenRequestBody> tokenRequest, string state, CancellationToken cancellationToken = default)
        {
            InternalAssertion.IsNotNull(userResolver, nameof(userResolver));
            InternalAssertion.IsNotNull(tokenRequest, nameof(tokenRequest));
            UserEntity user;
            try
            {
                user = await userResolver;
            }
            catch (ArgumentException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                    ErrorDescription = ex.Message
                };
            }
            catch (InvalidOperationException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NotImplementedException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NullReferenceException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NotSupportedException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                    ErrorDescription = ex.Message
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.AccessDenied,
                    ErrorDescription = ex.Message
                };
            }

            return await CreateTokenAsync(user, tokenRequest, state, cancellationToken);
        }

        public async Task<UserTokenInfo> CreateTokenAsync(Task<TokenEntity> tokenResolver, string clientId, string state, CancellationToken cancellationToken = default)
        {
            InternalAssertion.IsNotNull(tokenResolver, nameof(tokenResolver));
            TokenEntity token;
            UserEntity user;
            try
            {
                token = await tokenResolver;
                if (token is null) return null;
                if (token.IsExpired)
                {
                    await DataProvider.DeleteExpiredTokensAsync(token.UserId, cancellationToken);
                    return null;
                }

                user = await DataProvider.GetUserByIdAsync(token.UserId, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                    ErrorDescription = ex.Message
                };
            }
            catch (InvalidOperationException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NotImplementedException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NullReferenceException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.ServerError,
                    ErrorDescription = ex.Message
                };
            }
            catch (NotSupportedException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                    ErrorDescription = ex.Message
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                return new UserTokenInfo
                {
                    ErrorCode = TokenInfo.ErrorCodeConstants.AccessDenied,
                    ErrorDescription = ex.Message
                };
            }

            if (token.IsClosedToExpiration)
            {
                if (string.IsNullOrWhiteSpace(clientId)) clientId = token.ClientId;
                token = null;
            }

            return await CreateTokenAsync(user, token, clientId, state, cancellationToken);
        }

        private static void AssertTokenRequest(TokenRequest tokenRequest)
        {
            InternalAssertion.IsNotNull(tokenRequest, nameof(tokenRequest));
            InternalAssertion.IsNotNull(tokenRequest.Body, $"{nameof(tokenRequest)}.{nameof(tokenRequest.Body)}");
            InternalAssertion.IsNotNull(tokenRequest.ClientId, $"{nameof(tokenRequest)}.{nameof(tokenRequest.ClientId)}");
        }
    }
}
