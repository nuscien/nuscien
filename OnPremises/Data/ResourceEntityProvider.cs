using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using NuScien.Security;
using Trivial.Collection;
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
    public abstract class OnPremisesResourceEntityProvider<T> : IResourceEntityProvider<T> where T : BaseResourceEntity
    {
        /// <summary>
        /// The save handler.
        /// </summary>
        private readonly Func<CancellationToken, Task<int>> saveHandler;

        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceEntityProvider class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="set">The database set.</param>
        /// <param name="save">The entity save handler.</param>
        public OnPremisesResourceEntityProvider(OnPremisesResourceAccessClient client, DbSet<T> set, Func<CancellationToken, Task<int>> save)
        {
            CoreResources = client ?? new OnPremisesResourceAccessClient(null);
            Set = set;
            saveHandler = save ?? DbResourceEntityExtensions.SaveChangesFailureAsync;
        }

        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceEntityProvider class.
        /// </summary>
        /// <param name="dataProvider">The resource data provider.</param>
        /// <param name="set">The database set.</param>
        /// <param name="save">The entity save handler.</param>
        public OnPremisesResourceEntityProvider(IAccountDataProvider dataProvider, DbSet<T> set, Func<CancellationToken, Task<int>> save)
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
        /// Gets the current user identifier.
        /// </summary>
        protected string UserId => CoreResources?.User?.Id;

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
        BaseResourceAccessClient IResourceEntityProvider<T>.CoreResources => CoreResources;

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public Task<T> GetAsync(string id, CancellationToken cancellationToken = default) => GetAsync(id, false, cancellationToken);

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public virtual Task<T> GetAsync(string id, bool includeAllStates, CancellationToken cancellationToken = default)
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
        public virtual Task<CollectionResult<T>> SearchAsync(QueryData q, CancellationToken cancellationToken = default)
        {
            return DbResourceEntityExtensions.SearchAsync(Set, q, MapQuery, cancellationToken);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public virtual async Task<ChangingResultInfo> SaveAsync(T value, CancellationToken cancellationToken = default)
        {
            if (value == null) return new ChangingResultInfo(ChangeMethods.Unchanged);
            var isNew = value.IsNew;
            if (isNew) OnAdd(value);
            else OnUpdate(value);
            try
            {
                var result = await DbResourceEntityExtensions.SaveAsync(Set, SaveChangesAsync, value, cancellationToken);
                Saved?.Invoke(this, new ChangeEventArgs<T>(isNew ? null : value, value, result));
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<T>(result, value, $"{result} {typeof(T).Name} entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = DbResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="delta">The data to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public virtual async Task<ChangingResultInfo> SaveAsync(string id, JsonObjectNode delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                if (delta == null) return null;
                try
                {
                    var newEntity = delta.Deserialize<T>();
                    return await SaveAsync(newEntity, cancellationToken) ?? new ChangingResultInfo(ChangeErrorKinds.Service, "No response.");
                }
                catch (System.Text.Json.JsonException)
                {
                    return new ChangingResultInfo(ChangeErrorKinds.Argument, "Failed to convert the JSON object."); ;
                }
            }

            var entity = await GetAsync(id, true, cancellationToken);
            if (delta == null || delta.Count == 0) return new ChangingResultInfo<T>(ChangeMethods.Unchanged, entity, "Update properties."); ;
            entity.SetProperties(delta);
            await SaveAsync(entity, cancellationToken);
            return new ChangingResultInfo<T>(ChangeMethods.MemberModify, entity, "Update properties.");
        }

        /// <summary>
        /// Updates the entity state.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="state">The state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<T> UpdateStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            var entity = await GetAsync(id, true, cancellationToken);
            if (entity == null) return null;
            if (entity.State == state) return entity;
            entity.State = state;
            await SaveAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="predication">The query predication.</param>
        /// <returns>The result.</returns>
        protected abstract void MapQuery(QueryPredication<T> predication);

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
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A number of state entries written to the database.</returns>
        /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
        protected virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return saveHandler?.Invoke(cancellationToken) ?? Task.FromResult(0);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="value">The permission item to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasPermissionAsync(string siteId, string value)
        {
            if (CoreResources == null) return false;
            return await CoreResources.HasPermissionAsync(siteId, value);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasAnyPermissionAsync(string siteId, string value, params string[] otherValues)
        {
            if (CoreResources == null) return false;
            return await CoreResources.HasAnyPermissionAsync(siteId, value, otherValues);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasAnyPermissionAsync(string siteId, IEnumerable<string> values)
        {
            if (CoreResources == null) return false;
            return await CoreResources.HasAnyPermissionAsync(siteId, values);
        }
    }
}
