using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using NuScien.Data;
using NuScien.Users;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The data provider of the account service.
    /// </summary>
    public class AccountDbSetProvider
    {
        /// <summary>
        /// Initializes a new instance of the AccountDbSetProvider class.
        /// </summary>
        /// <param name="users">The user entity database set.</param>
        /// <param name="groups">The user group entity database set.</param>
        /// <param name="tokens">The token entity database set.</param>
        public AccountDbSetProvider(DbSet<UserEntity> users, DbSet<UserGroupEntity> groups, DbSet<TokenEntity> tokens)
        {
            Users = users;
            Groups = groups;
            Tokens = tokens;
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        protected DbSet<UserEntity> Users { get; }

        /// <summary>
        /// Gets the users.
        /// </summary>
        protected DbSet<UserGroupEntity> Groups { get; }

        /// <summary>
        /// Gets the users.
        /// </summary>
        protected DbSet<TokenEntity> Tokens { get; }

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public async Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await Users.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a user entity by given login name.
        /// </summary>
        /// <param name="loginName">The login name of the user.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public async Task<UserEntity> GetUserByLognameAsync(string loginName, CancellationToken cancellationToken = default)
        {
            return await Users.FirstOrDefaultAsync(ele => ele.Name == loginName, cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<TokenEntity> GetTokenByNameAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            return await Tokens.FirstOrDefaultAsync(ele => ele.Name == accessToken, cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public async Task<TokenEntity> GetTokenByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await Tokens.FirstOrDefaultAsync(ele => ele.RefreshToken == refreshToken, cancellationToken);
        }

        /// <summary>
        /// Deletes a set of tokens expired.
        /// </summary>
        /// <param name="userId">The user identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public async Task<int> DeleteExpiredTokensAsync(string userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var list = await Tokens.Where(ele => ele.UserId == userId && ele.ExpirationTime <= now).ToListAsync(cancellationToken);
            Tokens.RemoveRange(list);
            return list.Count;
        }

        /// <summary>
        /// Deletes a set of tokens expired.
        /// </summary>
        /// <param name="clientId">The client identifier of the token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The count of token deleted.</returns>
        public async Task<int> DeleteExpiredClientTokensAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var list = await Tokens.Where(ele => ele.ClientId == clientId && ele.ExpirationTime <= now).ToListAsync(cancellationToken);
            Tokens.RemoveRange(list);
            return list.Count;
        }

        /// <summary>
        /// Creates or updates a user entity.
        /// </summary>
        /// <param name="user">The user entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(Users, user, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a token entity.
        /// </summary>
        /// <param name="token">The token entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(TokenEntity token, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(Tokens, token, cancellationToken);
        }
    }
}
