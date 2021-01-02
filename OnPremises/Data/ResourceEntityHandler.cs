using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceEntityHandler class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="set">The database set.</param>
        public OnPremisesResourceEntityHandler(OnPremisesResourceAccessClient client, DbSet<T> set)
        {
            Client = client;
            Set = set;
        }

        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceEntityHandler class.
        /// </summary>
        /// <param name="dataProvider">The resource data provider.</param>
        /// <param name="set">The database set.</param>
        public OnPremisesResourceEntityHandler(IAccountDataProvider dataProvider, DbSet<T> set)
        {
            Client = new OnPremisesResourceAccessClient(dataProvider);
            Set = set;
        }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected DbSet<T> Set { get; }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected OnPremisesResourceAccessClient Client { get; }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        BaseResourceAccessClient IResourceEntityHandler<T>.Client => Client;

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public Task<T> GetAsync(string id, bool includeAllStates = false, CancellationToken cancellationToken = default)
        {
            return Set.GetByIdAsync(id, includeAllStates, cancellationToken);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public async Task<CollectionResult<T>> SearchAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            return new CollectionResult<T>(await Set.ListEntities(q).ToListAsync(cancellationToken), q?.Offset ?? 0);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<ChangeMethodResult> SaveAsync(T value, CancellationToken cancellationToken = default)
        {
            //if (value.IsNew
            //    ? string.IsNullOrWhiteSpace(value.SiteId)   // Property check for new entity.
            //    : !await client.HasPermissionAsync(value.SiteId, "sample-customer-management")) // Permission check.
            //    return ChangeMethods.Invalid;
            var change = await DbResourceEntityExtensions.SaveAsync(Set, SaveChangesAsync, value, cancellationToken);
            return new ChangeMethodResult(change);
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
        protected abstract Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
