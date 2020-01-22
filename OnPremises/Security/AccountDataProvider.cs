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
    public class AccountDbSetProvider : IAccountDataProvider
    {
        /// <summary>
        /// Initializes a new instance of the AccountDbSetProvider class.
        /// </summary>
        /// <param name="users">The user entity database set.</param>
        /// <param name="groups">The user group entity database set.</param>
        /// <param name="clients">The client entity database set.</param>
        /// <param name="codes">The authorization code database set.</param>
        /// <param name="tokens">The token entity database set.</param>
        /// <param name="relationships">The user group relationship database set.</param>
        public AccountDbSetProvider(
            DbSet<UserEntity> users,
            DbSet<UserGroupEntity> groups,
            DbSet<AccessingClientEntity> clients,
            DbSet<AuthorizationCodeEntity> codes,
            DbSet<TokenEntity> tokens,
            DbSet<UserGroupResourceEntity<UserEntity>> relationships)
        {
            Users = users;
            Groups = groups;
            Clients = clients;
            Codes = codes;
            Tokens = tokens;
            Relationships = relationships;
        }

        /// <summary>
        /// Gets the user database set.
        /// </summary>
        protected DbSet<UserEntity> Users { get; }

        /// <summary>
        /// Gets the user database set.
        /// </summary>
        protected DbSet<UserGroupEntity> Groups { get; }

        /// <summary>
        /// Gets the client database set.
        /// </summary>
        protected DbSet<AccessingClientEntity> Clients { get; }

        /// <summary>
        /// Gets the authorization code database set.
        /// </summary>
        protected DbSet<AuthorizationCodeEntity> Codes { get; }

        /// <summary>
        /// Gets the user database set.
        /// </summary>
        protected DbSet<TokenEntity> Tokens { get; }

        /// <summary>
        /// Gets the user group relationship database set.
        /// </summary>
        protected DbSet<UserGroupResourceEntity<UserEntity>> Relationships { get; }

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return Users.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets a user entity by given login name.
        /// </summary>
        /// <param name="loginName">The login name of the user.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<UserEntity> GetUserByLognameAsync(string loginName, CancellationToken cancellationToken = default)
        {
            return Users.FirstOrDefaultAsync(ele => ele.Name == loginName, cancellationToken);
        }

        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="appId">The client credential name, aka app identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AccessingClientEntity> GetClientByNameAsync(string appId, CancellationToken cancellationToken = default)
        {
            return Clients.FirstOrDefaultAsync(ele => ele.Name == appId, cancellationToken);
        }
        /// <summary>
        /// Gets a client credential by app identifier.
        /// </summary>
        /// <param name="provider">The provider name or url.</param>
        /// <param name="code">The authorization code.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user entity matched if found; otherwise, null.</returns>
        public Task<AuthorizationCodeEntity> GetAuthorizationCodeByCodeAsync(string provider, string code, CancellationToken cancellationToken = default)
        {
            return Codes.FirstOrDefaultAsync(ele => ele.Code == code && ele.ServiceProvider == provider, cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByNameAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            return Tokens.FirstOrDefaultAsync(ele => ele.Name == accessToken, cancellationToken);
        }

        /// <summary>
        /// Gets a token entity by given identifier.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public Task<TokenEntity> GetTokenByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return Tokens.FirstOrDefaultAsync(ele => ele.RefreshToken == refreshToken, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="q">The optional name query; null for all.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public IEnumerable<UserGroupResourceEntity<UserEntity>> ListUserGroups(UserEntity user, string q = null, ResourceEntityStates relationshipState = ResourceEntityStates.Normal)
        {
            var groups = string.IsNullOrWhiteSpace(q)
                ? Groups.Where(ele => ele.StateCode == ResourceEntityExtensions.NormalStateCode)
                : Groups.Where(ele => ele.Name.Contains(q) && ele.StateCode == ResourceEntityExtensions.NormalStateCode);
            return groups.Join(
                Relationships.Where(ele => ele.TargetId == user.Id && ele.StateCode == (int)relationshipState),
                ele => ele.Id,
                ele => ele.OwnerId,
                (group, rela) => new UserGroupResourceEntity<UserEntity>(rela, group, user));
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
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="group">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupEntity group, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SaveAsync(Groups, group, cancellationToken);
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
