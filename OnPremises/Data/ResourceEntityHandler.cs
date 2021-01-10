using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using NuScien.Security;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The base resource entity accessing service.
    /// </summary>
    /// <typeparam name="T">The type of the resouce entity.</typeparam>
    public abstract class OnPremisesResourceEntityHandler<T> : IResourceEntityHandler<T> where T : BaseResourceEntity
    {
        private readonly Func<CancellationToken, Task<int>> saveHandler;

        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceEntityHandler class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="set">The database set.</param>
        /// <param name="save">The entity save handler.</param>
        public OnPremisesResourceEntityHandler(OnPremisesResourceAccessClient client, DbSet<T> set, Func<CancellationToken, Task<int>> save)
        {
            CoreResources = client;
            Set = set;
            saveHandler = save ?? DbResourceEntityExtensions.SaveChangesFailureAsync;
        }

        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceEntityHandler class.
        /// </summary>
        /// <param name="dataProvider">The resource data provider.</param>
        /// <param name="set">The database set.</param>
        /// <param name="save">The entity save handler.</param>
        public OnPremisesResourceEntityHandler(IAccountDataProvider dataProvider, DbSet<T> set, Func<CancellationToken, Task<int>> save)
        {
            CoreResources = new OnPremisesResourceAccessClient(dataProvider);
            Set = set;
            saveHandler = save ?? DbResourceEntityExtensions.SaveChangesFailureAsync;
        }

        /// <summary>
        /// Adds or removes the event handler on save.
        /// </summary>
        public event ChangeEventHandler<T> Saved;

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected DbSet<T> Set { get; }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected OnPremisesResourceAccessClient CoreResources { get; }

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        protected Users.UserEntity User => CoreResources.User;

        /// <summary>
        /// Gets the current client identifier.
        /// </summary>
        protected string ClientId => CoreResources.ClientId;

        /// <summary>
        /// Gets a value indicating whether the access token is null, empty or consists only of white-space characters.
        /// </summary>
        protected bool IsTokenNullOrEmpty => CoreResources.IsTokenNullOrEmpty;

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        BaseResourceAccessClient IResourceEntityHandler<T>.CoreResources => CoreResources;

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public virtual Task<T> GetAsync(string id, bool includeAllStates = false, CancellationToken cancellationToken = default)
        {
            return Set.GetByIdAsync(id, includeAllStates, cancellationToken);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public virtual async Task<CollectionResult<T>> SearchAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            return new CollectionResult<T>(await Set.ListEntities(q).ToListAsync(cancellationToken), q?.Offset ?? 0);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public virtual async Task<CollectionResult<T>> SearchAsync(QueryData q, CancellationToken cancellationToken = default)
        {
            if (q == null) return new CollectionResult<T>(await Set.ListEntities(new QueryArgs()).ToListAsync(cancellationToken), 0);
            QueryArgs args = q;
            var col = Set.ListEntities(args, l => Filter(l, q));
            if (col is null) return new CollectionResult<T>(null, args.Offset);
            return new CollectionResult<T>(await col.ToListAsync(cancellationToken), args.Offset);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public virtual async Task<ChangeMethodResult> SaveAsync(T value, CancellationToken cancellationToken = default)
        {
            var isNew = value.IsNew;
            if (isNew) OnAdd(value);
            else OnUpdate(value);
            var change = await DbResourceEntityExtensions.SaveAsync(Set, SaveChangesAsync, value, cancellationToken);
            Saved?.Invoke(this, new ChangeEventArgs<T>(isNew ? null : value, value, change));
            return new ChangeMethodResult(change);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="q">The full query data.</param>
        /// <returns>The result.</returns>
        protected abstract IQueryable<T> Filter(IQueryable<T> source, QueryData q);

        /// <summary>
        /// Tests if the new entity is valid.
        /// </summary>
        /// <param name="value">The entity to add.</param>
        /// <returns>true if it is valid; otherwise, false.</returns>
        protected virtual void OnAdd(T value)
        {
        }

        /// <summary>
        /// Tests if the new entity is valid.
        /// </summary>
        /// <param name="value">The entity to add.</param>
        /// <returns>true if it is valid; otherwise, false.</returns>
        protected virtual void OnUpdate(T value)
        {
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
        /// <param name="cancellationToken"></param>
        /// <returns>A number of state entries written to the database.</returns>
        /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
        protected virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return saveHandler?.Invoke(cancellationToken) ?? Task.FromResult(0);
        }
    }
}
