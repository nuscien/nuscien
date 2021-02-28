using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NuScien.Data;
using NuScien.Security;
using NuScien.Users;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Web
{
    /// <summary>
    /// The web API controller for base resource entity.
    /// </summary>
    public abstract class ResourceEntityControllerBase<TEntity> : ControllerBase, IResourceEntityProvider<TEntity>
        where TEntity : BaseResourceEntity
    {
        /// <summary>
        /// The resource access client.
        /// </summary>
        private BaseResourceAccessClient coreResources;

        /// <summary>
        /// Initializes a new instance of the ResourceEntityControllerBase class.
        /// </summary>
        public ResourceEntityControllerBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceEntityControllerBase class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ResourceEntityControllerBase(ILogger<ResourceEntityControllerBase<TEntity>> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger<ResourceEntityControllerBase<TEntity>> Logger { get; set; }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        BaseResourceAccessClient IResourceEntityProvider<TEntity>.CoreResources => coreResources;

        /// <summary>
        /// Gets an entity.
        /// </summary>
        /// <returns>The entity.</returns>
        [HttpGet]
        [Route("e/{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                var entity = await GetAsync(id);
                return this.ResourceEntityResult(entity);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null) return er;
                Logger?.LogError(new EventId(17006011, "GetEntity"), $"Get entity {id} failed with internal reason. {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public Task<TEntity> GetAsync(string id, CancellationToken cancellationToken = default) => GetAsync(id, false, cancellationToken);

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public virtual async Task<TEntity> GetAsync(string id, bool includeAllStates, CancellationToken cancellationToken = default)
        {
            var set = await GetDbSetAsync();
            return await set.GetByIdAsync(id, includeAllStates, cancellationToken);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <returns>The collection.</returns>
        [HttpGet]
        public async Task<IActionResult> SearchAsync()
        {
            try
            {
                var q = Request.Query?.GetQueryData() ?? new QueryData();
                var col = await SearchAsync(q);
                return this.ResourceEntityResult(col.Value, col.Offset, col.TotalCount);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null) return er;
                Logger?.LogError(new EventId(17006012, "GetEntity"), $"Search entities failed with internal reason. {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public virtual async Task<CollectionResult<TEntity>> SearchAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            var set = await GetDbSetAsync();
            return new CollectionResult<TEntity>(await set.ListEntities(q).ToListAsync(cancellationToken), q?.Offset ?? 0);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public virtual async Task<CollectionResult<TEntity>> SearchAsync(QueryData q, CancellationToken cancellationToken = default)
        {
            return await DbResourceEntityExtensions.SearchAsync(await GetDbSetAsync(), q, MapQuery, cancellationToken);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The changing state.</returns>
        [HttpPut]
        public async Task<IActionResult> SaveBodyAsync([FromBody] TEntity entity)
        {
            try
            {
                if (entity is null) return this.ExceptionResult(400, "Require to send the entity in JSON format as request body.", "NoBody");
                var result = await SaveAsync(entity, default) ?? new ChangingResultInfo(ChangeMethods.Invalid);
                Logger?.LogInformation(new EventId(17006013, "SaveEntity"), $"Save ({result.State}) entity {entity.GetType().Name} {entity.Name} ({entity.Id}).");
                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006013, "SaveEntity"), $"Save entity failed, {entity.GetType().Name} {entity.Name} ({entity.Id}). {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                Logger?.LogError(new EventId(17006013, "SaveEntity"), $"Save entity failed with internal error, {entity.GetType().Name} {entity.Name} ({entity.Id}). {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public virtual async Task<ChangingResultInfo> SaveAsync(TEntity value, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync();
            if (value == null) return new ChangingResultInfo(ChangeMethods.Unchanged);
            var isNew = value.IsNew;
            if (isNew) OnAdd(value);
            else OnUpdate(value);
            var set = GetDbSet(context);
            var change = await DbResourceEntityExtensions.SaveAsync(set, context.SaveChangesAsync, value, cancellationToken);
            return new ChangingResultInfo(change);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The changing state.</returns>
        [HttpPut]
        [Route("e/{id}")]
        public virtual async Task<IActionResult> SaveAsync(string id)
        {
            try
            {
                var delta = await JsonObject.ParseAsync(Request.Body);
                var r = await SaveAsync(id, delta);
                return r.ToActionResult();
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006014, "UpdateEntity"), $"Update entity {id} failed. {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                Logger?.LogError(new EventId(17006014, "UpdateEntity"), $"Update entity {id} failed with internal error. {ex.GetType().Name} {ex.Message}");
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
        public async Task<ChangingResultInfo> SaveAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                if (delta == null) return null;
                try
                {
                    var newEntity = delta.Deserialize<TEntity>();
                    return await SaveAsync(newEntity, cancellationToken) ?? new ChangingResultInfo(ChangeErrorKinds.Service, "No response.");
                }
                catch (System.Text.Json.JsonException)
                {
                    return new ChangingResultInfo(ChangeErrorKinds.Argument, "Failed to convert the JSON object."); ;
                }
            }

            var entity = await GetAsync(id, true, cancellationToken);
            if (delta == null || delta.Count == 0) return new ChangingResultInfo<TEntity>(ChangeMethods.Unchanged, entity, "Update properties."); ;
            entity.SetProperties(delta);
            await SaveAsync(entity, cancellationToken);
            return new ChangingResultInfo<TEntity>(ChangeMethods.MemberModify, entity, "Update properties.");
        }

        /// <summary>
        /// Updates the entity state.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="state">The state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<TEntity> UpdateStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            var entity = await GetAsync(id, true, cancellationToken);
            if (entity == null) return null;
            if (entity.State == state) return entity;
            entity.State = state;
            await SaveAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Deletes a specific entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The changing state.</returns>
        [HttpDelete]
        [Route("e/{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) return this.ExceptionResult(400, "An entity identifier is required in path.", "NoBody");
                var entity = await UpdateStateAsync(id, ResourceEntityStates.Deleted);
                Logger?.LogInformation(new EventId(17006005, "DeleteEntity"), entity != null ? $"Delete entity {entity.GetType().Name} {entity.Name} ({entity.Id})." : $"Failed delete entity {id} because of non-existing.");
                return this.ResourceEntityResult(entity);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006015, "DeleteEntity"), $"Delete entity {id} failed. {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                Logger?.LogError(new EventId(17006015, "DeleteEntity"), $"Delete entity {id} failed with internal error. {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        protected abstract Task<DbContext> GetDbContextAsync();

        /// <summary>
        /// Gets the database set.
        /// </summary>
        /// <param name="context">The database context.</param>
        protected virtual DbSet<TEntity> GetDbSet(DbContext context)
        {
            return context.Set<TEntity>();
        }

        /// <summary>
        /// Gets the database set.
        /// </summary>
        protected async virtual Task<DbSet<TEntity>> GetDbSetAsync()
        {
            var context = await GetDbContextAsync();
            return GetDbSet(context);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="value">The permission item to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasPermissionAsync(string siteId, string value)
        {
            if (coreResources == null) coreResources = await this.GetResourceAccessClientAsync();
            return await coreResources.HasPermissionAsync(siteId, value);
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
            if (coreResources == null) coreResources = await this.GetResourceAccessClientAsync();
            return await coreResources.HasAnyPermissionAsync(siteId, value, otherValues);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasAnyPermissionAsync(string siteId, IEnumerable<string> values)
        {
            if (coreResources == null) coreResources = await this.GetResourceAccessClientAsync();
            return await coreResources.HasAnyPermissionAsync(siteId, values);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="predication">The query predication.</param>
        /// <returns>The result.</returns>
        protected abstract void MapQuery(QueryPredication<TEntity> predication);

        /// <summary>
        /// Tests if the new entity is valid.
        /// </summary>
        /// <param name="value">The entity to add.</param>
        /// <returns>true if it is valid; otherwise, false.</returns>
        protected virtual void OnAdd(TEntity value)
        {
        }

        /// <summary>
        /// Tests if the new entity is valid.
        /// </summary>
        /// <param name="value">The entity to add.</param>
        /// <returns>true if it is valid; otherwise, false.</returns>
        protected virtual void OnUpdate(TEntity value)
        {
        }
    }
}
