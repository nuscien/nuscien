using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using NuScien.Configurations;
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
    public interface IAccountDbContext : IDisposable
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

        /// <summary>
        /// Gets the settings database set.
        /// </summary>
        public DbSet<SettingsEntity> Settings { get; }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        /// to discover any changes to entity instances before saving to the underlying database.
        /// This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
        /// Multiple active operations on the same context instance are not supported. Use
        /// 'await' to ensure that any asynchronous operations have completed before calling
        /// another method on this context.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A number of state entries written to the database.</returns>
        /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The account database set context.
    /// </summary>
    public class AccountDbContext : DbContext, IAccountDbContext
    {
        /// <summary>
        /// Initializes a new instance of the AccountDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will be called to configure the database (and other options) to be used for this context.
        /// </summary>
        protected AccountDbContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccountDbContext class.
        /// It can use a specified options.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public AccountDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccountDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The database connection.</param>
        public AccountDbContext(Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> configureConnection, DbConnection connection)
            : base(DbResourceEntityExtensions.CreateDbContextOptions<AccountDbContext>(configureConnection, connection))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccountDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The connection string.</param>
        public AccountDbContext(Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> configureConnection, string connection)
            : base(DbResourceEntityExtensions.CreateDbContextOptions<AccountDbContext>(configureConnection, connection))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccountDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="optionsAction">The additional options action.</param>
        public AccountDbContext(Func<DbContextOptionsBuilder<AccountDbContext>, DbConnection, Action<DbContextOptionsBuilder<AccountDbContext>>, DbContextOptionsBuilder<AccountDbContext>> configureConnection, DbConnection connection, Action<DbContextOptionsBuilder<AccountDbContext>> optionsAction)
            : base(DbResourceEntityExtensions.CreateDbContextOptions(configureConnection, connection, optionsAction))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccountDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The connection string.</param>
        /// <param name="optionsAction">The additional options action.</param>
        public AccountDbContext(Func<DbContextOptionsBuilder<AccountDbContext>, string, Action<DbContextOptionsBuilder<AccountDbContext>>, DbContextOptionsBuilder<AccountDbContext>> configureConnection, string connection, Action<DbContextOptionsBuilder<AccountDbContext>> optionsAction)
            : base(DbResourceEntityExtensions.CreateDbContextOptions(configureConnection, connection, optionsAction))
        {
        }

        /// <summary>
        /// Gets or sets the user database set.
        /// </summary>
        public DbSet<UserEntity> Users { get; set; }

        /// <summary>
        /// Gets or sets the user database set.
        /// </summary>
        public DbSet<UserGroupEntity> Groups { get; set; }

        /// <summary>
        /// Gets or sets the client database set.
        /// </summary>
        public DbSet<AccessingClientEntity> Clients { get; set; }

        /// <summary>
        /// Gets or sets the authorization code database set.
        /// </summary>
        public DbSet<AuthorizationCodeEntity> Codes { get; set; }

        /// <summary>
        /// Gets or sets the user database set.
        /// </summary>
        public DbSet<TokenEntity> Tokens { get; set; }

        /// <summary>
        /// Gets or sets the user group relationship database set.
        /// </summary>
        public DbSet<UserGroupRelationshipEntity> Relationships { get; set; }

        /// <summary>
        /// Gets or sets the user permissions database set.
        /// </summary>
        public DbSet<UserPermissionItemEntity> UserPermissions { get; set; }

        /// <summary>
        /// Gets or sets the user group permissions database set.
        /// </summary>
        public DbSet<UserGroupPermissionItemEntity> GroupPermissions { get; set; }

        /// <summary>
        /// Gets or sets the client permissions database set.
        /// </summary>
        public DbSet<ClientPermissionItemEntity> ClientPermissions { get; set; }

        /// <summary>
        /// Gets or sets the settings database set.
        /// </summary>
        public DbSet<SettingsEntity> Settings { get; set; }
    }
}
