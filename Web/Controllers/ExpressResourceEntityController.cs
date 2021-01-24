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
using NuScien.Collection;
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
    public abstract class BaseResourceEntityController<TEntity> : ControllerBase, IResourceEntityProvider<TEntity>
        where TEntity : BaseResourceEntity
    {
        /// <summary>
        /// Initializes a new instance of the BaseResourceEntityController class.
        /// </summary>
        public BaseResourceEntityController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ResourceAccessController class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public BaseResourceEntityController(ILogger<BaseResourceEntityController<TEntity>> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger<BaseResourceEntityController<TEntity>> Logger { get; set; }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected OnPremisesResourceAccessClient CoreResources { get; }

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        protected string UserId => CoreResources?.UserId;

        /// <summary>
        /// Gets the current client identifier.
        /// </summary>
        protected string ClientId => CoreResources?.ClientId;

        /// <summary>
        /// Gets a value indicating whether the access token is null, empty or consists only of white-space characters.
        /// </summary>
        protected bool IsTokenNullOrEmpty => CoreResources?.IsTokenNullOrEmpty != false;

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        BaseResourceAccessClient IResourceEntityProvider<TEntity>.CoreResources => CoreResources;

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
                throw;
            }
        }

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public virtual async Task<TEntity> GetAsync(string id, bool includeAllStates = false, CancellationToken cancellationToken = default)
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
            var set = await GetDbSetAsync();
            if (q == null) return new CollectionResult<TEntity>(await set.ListEntities(new QueryArgs()).ToListAsync(cancellationToken), 0);
            if (q.Count == 1 && q.ContainsKey("id"))
            {
                var ids = q.GetValues("id").Where(ele => !string.IsNullOrWhiteSpace(ele));
                var list = new List<TEntity>();
                foreach (var id in ids)
                {
                    var entity = await set.GetByIdAsync(id, false, cancellationToken);
                    list.Add(entity);
                }

                var result = new CollectionResult<TEntity>(list, 0, list.Count);
                return result;
            }

            QueryArgs args = q;
            var col = set.ListEntities(args, l =>
            {
                var info = new QueryPredication<TEntity>(l, q);
                MapQuery(info);
                return info.Data;
            });
            if (col is null) return new CollectionResult<TEntity>(null, args.Offset);
            return new CollectionResult<TEntity>(await col.ToListAsync(cancellationToken), args.Offset);
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
                var result = await SaveAsync(entity, default) ?? new ChangeMethodResult(ChangeMethods.Invalid);
                Logger?.LogInformation(new EventId(17006003, "SaveEntity"), $"Save ({result.State}) entity {entity.GetType().Name} {entity.Name} ({entity.Id}).");
                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006003, "SaveEntity"), $"Failed save entity {entity.GetType().Name} {entity.Name} ({entity.Id}). {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                throw;
            }
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public virtual async Task<ChangeMethodResult> SaveAsync(TEntity value, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync();
            if (value == null) return new ChangeMethodResult(ChangeMethods.Unchanged);
            var isNew = value.IsNew;
            if (isNew) OnAdd(value);
            else OnUpdate(value);
            var set = GetDbSet(context);
            var change = await DbResourceEntityExtensions.SaveAsync(set, context.SaveChangesAsync, value, cancellationToken);
            return new ChangeMethodResult(change);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="changes">The data to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public virtual async Task<TEntity> SaveAsync(string id, JsonObject changes, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                if (changes == null) return null;
                try
                {
                    var newEntity = changes.Deserialize<TEntity>();
                    var result = await SaveAsync(newEntity, cancellationToken);
                    if (result == null) return null;
                    return result.State switch
                    {
                        ChangeMethods.Add => newEntity,
                        ChangeMethods.Same => newEntity,
                        ChangeMethods.Unchanged => newEntity,
                        ChangeMethods.Remove => newEntity,
                        ChangeMethods.MemberModify => newEntity,
                        ChangeMethods.Update => newEntity,
                        _ => null
                    };
                }
                catch (System.Text.Json.JsonException)
                {
                    return null;
                }
            }

            var entity = await GetAsync(id, true, cancellationToken);
            if (changes == null || changes.Count == 0) return entity;
            if (FillProperties(entity, changes)) await SaveAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The changing state.</returns>
        [HttpPut]
        [Route("e/{id}")]
        public virtual async Task<IActionResult> UpdateAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) return this.ExceptionResult(400, "An entity identifier is required in path.", "NoBody");
                var content = await JsonObject.ParseAsync(Request.Body);
                var entity = await SaveAsync(id, content);
                Logger?.LogInformation(new EventId(17006004, "UpdateEntity"), entity != null ? $"Update entity {entity.GetType().Name} {entity.Name} ({entity.Id})." : $"Failed update entity {id} because of non-existing.");
                return this.ResourceEntityResult(entity);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006004, "UpdateEntity"), $"Failed update entity {id}. {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                throw;
            }
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
                    Logger?.LogError(new EventId(17006005, "DeleteEntity"), $"Failed delete entity {id}. {ex.GetType().Name} {ex.Message}");
                    return er;
                }

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
        /// Searches.
        /// </summary>
        /// <param name="predication">The query predication.</param>
        /// <returns>The result.</returns>
        protected abstract void MapQuery(QueryPredication<TEntity> predication);

        /// <summary>
        /// Fills the data into the entity.
        /// </summary>
        /// <param name="entity">The target entity.</param>
        /// <param name="changes">The data to change.</param>
        /// <returns>true if the change set is valid; otherwise, false.</returns>
        protected virtual bool FillProperties(TEntity entity, JsonObject changes)
        {
            var state = changes.TryGetEnumValue<ResourceEntityStates>("state", true);
            if (state.HasValue) entity.State = state.Value;
            var name = changes.TryGetStringValue("name");
            entity.Name = name;
            return true;
        }

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
