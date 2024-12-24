using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using NuScien.Data;
using NuScien.Security;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Data;

/// <summary>
/// The resource accessing context on-premises.
/// </summary>
public class OnPremisesResourceAccessContext : IDisposable
{
    /// <summary>
    /// The internal database context.
    /// </summary>
    internal class InternalDbContext : DbContext
    {
        private readonly List<Type> types = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the InternalDbContext class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public InternalDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var item in types)
            {
                modelBuilder.Entity(item);
            }

            types.Clear();
        }

        /// <summary>
        /// Registers an entity type.
        /// </summary>
        /// <param name="type">The type of the entity to register.</param>
        public void RegisterEntityType(Type type) => types.Add(type);
    }

    private bool disposedValue;
    private readonly DbContext db;

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="dataProvider">The account data provider.</param>
    /// <param name="dbContext">The database context.</param>
    public OnPremisesResourceAccessContext(IAccountDataProvider dataProvider, DbContext dbContext)
    {
        CoreResources = new OnPremisesResourceAccessClient(dataProvider);
        db = dbContext;
        FillProviderProperties();
    }

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="dataProvider">The account data provider.</param>
    /// <param name="options">The options for this context.</param>
    public OnPremisesResourceAccessContext(IAccountDataProvider dataProvider, DbContextOptions options)
        : this(new OnPremisesResourceAccessClient(dataProvider), new InternalDbContext(options))
    {
    }

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="dbContext">The database context.</param>
    public OnPremisesResourceAccessContext(OnPremisesResourceAccessClient client, DbContext dbContext)
    {
        CoreResources = client ?? new OnPremisesResourceAccessClient(null);
        db = dbContext;
        FillProviderProperties();
    }

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="options">The options for this context.</param>
    public OnPremisesResourceAccessContext(OnPremisesResourceAccessClient client, DbContextOptions options)
        : this(client, new InternalDbContext(options))
    {
    }

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="configureConnection">The method to configure context options with connection string.</param>
    /// <param name="connection">The database connection.</param>
    public OnPremisesResourceAccessContext(OnPremisesResourceAccessClient client, Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> configureConnection, DbConnection connection)
        : this(client, new InternalDbContext(DbResourceEntityExtensions.CreateDbContextOptions<DbContext>(configureConnection, connection)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="configureConnection">The method to configure context options with connection string.</param>
    /// <param name="connection">The connection string.</param>
    public OnPremisesResourceAccessContext(OnPremisesResourceAccessClient client, Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> configureConnection, string connection)
        : this(client, new InternalDbContext(DbResourceEntityExtensions.CreateDbContextOptions<DbContext>(configureConnection, connection)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="configureConnection">The method to configure context options with connection string.</param>
    /// <param name="connection">The database connection.</param>
    /// <param name="optionsAction">The additional options action.</param>
    public OnPremisesResourceAccessContext(OnPremisesResourceAccessClient client, Func<DbContextOptionsBuilder<DbContext>, DbConnection, Action<DbContextOptionsBuilder<DbContext>>, DbContextOptionsBuilder<DbContext>> configureConnection, DbConnection connection, Action<DbContextOptionsBuilder<DbContext>> optionsAction)
        : this(client, new InternalDbContext(DbResourceEntityExtensions.CreateDbContextOptions(configureConnection, connection, optionsAction)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the OnPremisesResourceAccessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="configureConnection">The method to configure context options with connection string.</param>
    /// <param name="connection">The connection string.</param>
    /// <param name="optionsAction">The additional options action.</param>
    public OnPremisesResourceAccessContext(OnPremisesResourceAccessClient client, Func<DbContextOptionsBuilder<DbContext>, string, Action<DbContextOptionsBuilder<DbContext>>, DbContextOptionsBuilder<DbContext>> configureConnection, string connection, Action<DbContextOptionsBuilder<DbContext>> optionsAction)
        : this(client, new InternalDbContext(DbResourceEntityExtensions.CreateDbContextOptions(configureConnection, connection, optionsAction)))
    {
    }

    /// <summary>
    /// An event fired at the beginning of a call to SaveChanges or SaveChangesAsync
    /// </summary>
    public event EventHandler<SavingChangesEventArgs> SavingChanges
    {
        add => db.SavingChanges += value;
        remove => db.SavingChanges -= value;
    }

    /// <summary>
    /// An event fired at the end of a call to SaveChanges or SaveChangesAsync
    /// </summary>
    public event EventHandler<SavedChangesEventArgs> SavedChanges
    {
        add => db.SavedChanges += value;
        remove => db.SavedChanges -= value;
    }

    /// <summary>
    /// An event fired if a call to SaveChanges or SaveChangesAsync fails with an exception.
    /// </summary>
    public event EventHandler<SaveChangesFailedEventArgs> SaveChangesFailed
    {
        add => db.SaveChangesFailed += value;
        remove => db.SaveChangesFailed -= value;
    }

    /// <summary>
    /// Gets the resources access client.
    /// </summary>
    public OnPremisesResourceAccessClient CoreResources { get; }

    /// <summary>
    /// Gets the current user information.
    /// </summary>
    public Users.UserEntity User => CoreResources.User;

    /// <summary>
    /// Gets the current client identifier.
    /// </summary>
    public string ClientId => CoreResources.ClientId;

    /// <summary>
    /// Gets a value indicating whether the access token is null, empty or consists only of white-space characters.
    /// </summary>
    public bool IsTokenNullOrEmpty => CoreResources.IsTokenNullOrEmpty;

    /// <summary>
    /// Gets a value indicating whether need disable automation of filling provider properties.
    /// </summary>
    protected virtual bool DisableProvidersAutoFilling { get; }

    /// <summary>
    /// Provides access to information and operations for entity instances this context is tracking.
    /// </summary>
    protected Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker => db.ChangeTracker;

    /// <summary>
    /// Gets the database related information and operations for this context.
    /// </summary>
    protected Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => db.Database;

    /// <summary>
    /// The metadata about the shape of entities, the relationships between them, and how they map to the database.
    /// </summary>
    protected Microsoft.EntityFrameworkCore.Metadata.IModel DbModel => db.Model;

    /// <summary>
    /// Gets the unique identifier for the context instance and pool lease, if any.
    /// This identifier is primarily intended as a correlation ID for logging and debugging
    /// such that it is easy to identify that multiple events are using the same or different
    /// context instances.
    /// </summary>
    protected DbContextId DbContextId => db.ContextId;

    /// <summary>
    /// Creates a DbSet that can be used to query and save instances of TEntity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity for which a set should be returned.</typeparam>
    /// <returns>A set for the given entity type.</returns>
    protected DbSet<TEntity> Set<TEntity>() where TEntity : BaseResourceEntity => db.Set<TEntity>();

    /// <summary>
    /// Creates a DbSet that can be used to query and save instances of TEntity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity for which a set should be returned.</typeparam>
    /// <param name="name">The name.</param>
    /// <returns>A set for the given entity type.</returns>
    protected DbSet<TEntity> Set<TEntity>(string name) where TEntity : BaseResourceEntity => db.Set<TEntity>(name);

    /// <summary>
    /// Creates a resource entity provider.
    /// </summary>
    /// <typeparam name="THandler">The type of the resource entity provider for which a set should be returned.</typeparam>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>The resource entity provider</returns>
    protected THandler Provider<THandler, TEntity>() where THandler : OnPremisesResourceEntityProvider<TEntity> where TEntity : BaseResourceEntity
    {
        var type = typeof(THandler);
        if (type.IsAbstract) return null;
        Func<CancellationToken, Task<int>> h = SaveChangesAsync;
        var c = type.GetConstructor(new Type[] { typeof(OnPremisesResourceAccessClient), typeof(DbSet<TEntity>), typeof(Func<CancellationToken, Task<int>>) });
        if (c == null) return null;
        return c.Invoke(new object[] { CoreResources, Set<TEntity>(), h }) as THandler;
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
    /// to discover any changes to entity instances before saving to the underlying database.
    /// This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
    /// Multiple active operations on the same context instance are not supported. Use
    /// 'await' to ensure that any asynchronous operations have completed before calling
    /// another method on this context.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    protected Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
    /// to discover any changes to entity instances before saving to the underlying database.
    /// This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
    /// Multiple active operations on the same context instance are not supported. Use
    /// 'await' to ensure that any asynchronous operations have completed before calling
    /// another method on this context.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges is called after the changes have been sent successfully to the database.</param>
    /// <param name="cancellationToken">An optional cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    protected Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => db.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

    /// <summary>
    /// Asynchronously ensures that the database for the context exists. If it exists,
    /// no action is taken. If it does not exist then the database and all its schema
    /// are created. If the database exists, then no effort is made to ensure it is compatible
    /// with the model for this context.
    /// Note that this API does not use migrations to create the database. In addition,
    /// the database that is created cannot be later updated using migrations. If you
    /// are targeting a relational database and using migrations, you can use the DbContext.Database.Migrate()
    /// method to ensure the database is created and all migrations are applied.
    /// </summary>
    /// <param name="cancellationToken">An optional cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>true if the database is created, false if it already existed.</returns>
    public Task<bool> EnsureDbCreatedAsync(CancellationToken cancellationToken = default) => db.Database.EnsureCreatedAsync(cancellationToken);

    /// <summary>
    /// Disposes.
    /// </summary>
    /// <param name="disposing">true if only for managed resources; otherwise, false.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue) return;
        if (disposing)
        {
            db.Dispose();
        }

        disposedValue = true;
    }

    /// <summary>
    /// Disposes.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Fills provider properties automatically.
    /// </summary>
    private void FillProviderProperties()
    {
        var properties = GetType().GetProperties();
        Action<Type> initDbContext = db is InternalDbContext dbContext ? type => dbContext.RegisterEntityType(type) : type => { };
        var fill = new List<Action>();
        Func<CancellationToken, Task<int>> h = SaveChangesAsync;
        foreach (var prop in properties)
        {
            var type = prop.PropertyType;
            if (type.IsAbstract || !prop.CanWrite || !prop.CanRead) continue;
            var cType = type;
            while (cType != null && !IsOnPremisesResourceEntityProvider(cType)) cType = cType.BaseType;
            try
            {
                if (cType == null || cType.GenericTypeArguments.Length < 1) continue;
                var gta = cType.GenericTypeArguments[0];
                if (gta == null || !gta.IsSubclassOf(typeof(BaseResourceEntity)) || prop.GetValue(this) != null) continue;
                initDbContext(gta);
                var setType = typeof(DbSet<>).MakeGenericType(gta);
                var c = type.GetConstructor(new Type[] { typeof(OnPremisesResourceAccessClient), setType, typeof(Func<CancellationToken, Task<int>>) });
                if (c == null) continue;
                var m = GetType().GetMethod("Set", 1, BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                m = m.MakeGenericMethod(gta);
                fill.Add(() =>
                {
                    var e = m.Invoke(this, null);
                    var v = c.Invoke(new object[] { CoreResources, e, h });
                    prop.SetValue(this, v);
                });
            }
            catch (NullReferenceException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (MemberAccessException)
            {
            }
            catch (TargetException)
            {
            }
            catch (TargetInvocationException)
            {
            }
            catch (TargetParameterCountException)
            {
            }
        }

        if (DisableProvidersAutoFilling) return;
        foreach (var a in fill)
        {
            try
            {
                a();
            }
            catch (NullReferenceException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (MemberAccessException)
            {
            }
            catch (TargetException)
            {
            }
            catch (TargetInvocationException)
            {
            }
            catch (TargetParameterCountException)
            {
            }
        }
    }

    private static bool IsOnPremisesResourceEntityProvider(Type type)
    {
        try
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OnPremisesResourceEntityProvider<>);
        }
        catch (NotSupportedException)
        {
        }

        return false;
    }
}