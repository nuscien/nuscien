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
using NuScien.Cms;
using NuScien.Configurations;
using NuScien.Data;
using NuScien.Sns;
using NuScien.Users;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The SNS database set context.
    /// </summary>
    public interface ISocialNetworkDbContext : IDisposable
    {
        /// <summary>
        /// Gets the user contact database set.
        /// </summary>
        public DbSet<ContactEntity> Contacts { get; }

        /// <summary>
        /// Gets the blog database set.
        /// </summary>
        public DbSet<BlogEntity> Blogs { get; }

        /// <summary>
        /// Gets the comment database set for blog.
        /// </summary>
        public DbSet<BlogCommentEntity> BlogComments { get; }

        /// <summary>
        /// Gets the user activity database set.
        /// </summary>
        public DbSet<UserActivityEntity> UserActivities { get; }

        /// <summary>
        /// Gets the user group activity database set.
        /// </summary>
        public DbSet<UserGroupActivityEntity> GroupActivities { get; }

        /// <summary>
        /// Gets the mail received database set.
        /// </summary>
        public DbSet<ReceivedMailEntity> ReceivedMails { get; }

        /// <summary>
        /// Gets the mail sent database set.
        /// </summary>
        public DbSet<SentMailEntity> SentMails { get; }

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
    /// The SNS database set context.
    /// </summary>
    public class SocialNetworkDbContext : DbContext, ISocialNetworkDbContext
    {
        /// <summary>
        /// Initializes a new instance of the SocialNetworkDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will be called to configure the database (and other options) to be used for this context.
        /// </summary>
        protected SocialNetworkDbContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SocialNetworkDbContext class.
        /// It can use a specified options.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public SocialNetworkDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SocialNetworkDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The database connection.</param>
        public SocialNetworkDbContext(Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> configureConnection, DbConnection connection)
            : base(DbResourceEntityExtensions.CreateDbContextOptions<SocialNetworkDbContext>(configureConnection, connection))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SocialNetworkDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The connection string.</param>
        public SocialNetworkDbContext(Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> configureConnection, string connection)
            : base(DbResourceEntityExtensions.CreateDbContextOptions<SocialNetworkDbContext>(configureConnection, connection))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SocialNetworkDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="optionsAction">The additional options action.</param>
        public SocialNetworkDbContext(Func<DbContextOptionsBuilder<SocialNetworkDbContext>, DbConnection, Action<DbContextOptionsBuilder<SocialNetworkDbContext>>, DbContextOptionsBuilder<SocialNetworkDbContext>> configureConnection, DbConnection connection, Action<DbContextOptionsBuilder<SocialNetworkDbContext>> optionsAction)
            : base(DbResourceEntityExtensions.CreateDbContextOptions(configureConnection, connection, optionsAction))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SocialNetworkDbContext class.
        /// The Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)
        /// method will still be called to allow further configuration of the options.
        /// </summary>
        /// <param name="configureConnection">The method to configure context options with connection string.</param>
        /// <param name="connection">The connection string.</param>
        /// <param name="optionsAction">The additional options action.</param>
        public SocialNetworkDbContext(Func<DbContextOptionsBuilder<SocialNetworkDbContext>, string, Action<DbContextOptionsBuilder<SocialNetworkDbContext>>, DbContextOptionsBuilder<SocialNetworkDbContext>> configureConnection, string connection, Action<DbContextOptionsBuilder<SocialNetworkDbContext>> optionsAction)
            : base(DbResourceEntityExtensions.CreateDbContextOptions(configureConnection, connection, optionsAction))
        {
        }

        /// <summary>
        /// Gets or sets the user contact database set.
        /// </summary>
        public DbSet<ContactEntity> Contacts { get; set; }

        /// <summary>
        /// Gets or sets the blog database set.
        /// </summary>
        public DbSet<BlogEntity> Blogs { get; set; }

        /// <summary>
        /// Gets or sets the comment database set for blog.
        /// </summary>
        public DbSet<BlogCommentEntity> BlogComments { get; set; }

        /// <summary>
        /// Gets or sets the user activity database set.
        /// </summary>
        public DbSet<UserActivityEntity> UserActivities { get; set; }

        /// <summary>
        /// Gets or sets the user group activity database set.
        /// </summary>
        public DbSet<UserGroupActivityEntity> GroupActivities { get; set; }

        /// <summary>
        /// Gets or sets the mail received database set.
        /// </summary>
        public DbSet<ReceivedMailEntity> ReceivedMails { get; set; }

        /// <summary>
        /// Gets or sets the mail sent database set.
        /// </summary>
        public DbSet<SentMailEntity> SentMails { get; set; }
    }
}
