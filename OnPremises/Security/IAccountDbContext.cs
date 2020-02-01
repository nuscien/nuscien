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
    /// The account database set context.
    /// </summary>
    public interface IAccountDbContext
    {
        /// <summary>
        /// Gets the user database set.
        /// </summary>
        public DbSet<UserEntity> Users { get; }

        /// <summary>
        /// Gets the user database set.
        /// </summary>
        public DbSet<UserGroupEntity> Groups { get; }

        /// <summary>
        /// Gets the client database set.
        /// </summary>
        public DbSet<AccessingClientEntity> Clients { get; }

        /// <summary>
        /// Gets the authorization code database set.
        /// </summary>
        public DbSet<AuthorizationCodeEntity> Codes { get; }

        /// <summary>
        /// Gets the user database set.
        /// </summary>
        public DbSet<TokenEntity> Tokens { get; }

        /// <summary>
        /// Gets the user group relationship database set.
        /// </summary>
        public DbSet<UserGroupRelationshipEntity> Relationships { get; }

        /// <summary>
        /// Gets the user permissions database set.
        /// </summary>
        public DbSet<UserPermissionItemEntity> UserPermissions { get; }

        /// <summary>
        /// Gets the user group permissions database set.
        /// </summary>
        public DbSet<UserGroupPermissionItemEntity> GroupPermissions { get; }

        /// <summary>
        /// Gets the client permissions database set.
        /// </summary>
        public DbSet<ClientPermissionItemEntity> ClientPermissions { get; }
    }
}
