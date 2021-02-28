using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
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
    public abstract class ResourceEntityControllerBase<TProvider, TEntity> : ControllerBase
        where TProvider : OnPremisesResourceEntityProvider<TEntity>
        where TEntity : BaseResourceEntity
    {
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
        public ResourceEntityControllerBase(ILogger<ResourceEntityControllerBase<TProvider, TEntity>> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger<ResourceEntityControllerBase<TProvider, TEntity>> Logger { get; set; }

        /// <summary>
        /// Gets an entity.
        /// </summary>
        /// <returns>The entity.</returns>
        [HttpGet]
        [Route("e/{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            try
            {
                var provider = await GetProviderAsync();
                var entity = await provider.GetAsync(id, Request.Query.TryGetBoolean("all") == true);
                return this.ResourceEntityResult(entity);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null) return er;
                Logger?.LogError(new EventId(17006001, "GetEntity"), $"Get entity {id} failed with internal error. {ex.GetType().Name} {ex.Message}");
                throw;
            }
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
                var provider = await GetProviderAsync();
                var q = Request.Query?.GetQueryData() ?? new QueryData();
                if (q.Count == 1)
                {
                    var ids = q.GetValues("id")?.ToList() ?? new List<string>();
                    if (ids.Count == 1 && !string.IsNullOrWhiteSpace(ids[0]))
                    {
                        var entity = await provider.GetAsync(q.GetFirstValue("id"));
                        return this.ResourceEntityResult(entity);
                    }
                }

                var col = await provider.SearchAsync(q);
                return this.ResourceEntityResult(col.Value, col.Offset, col.TotalCount);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null) return er;
                Logger?.LogError(new EventId(17006002, "SearchEntity"), $"Search entities failed with internal error. {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The changing state.</returns>
        [HttpPut]
        public async Task<IActionResult> SaveAsync([FromBody] TEntity entity)
        {
            try
            {
                if (entity is null) return this.ExceptionResult(400, "Require to send the entity in JSON format as request body.", "NoBody");
                var provider = await GetProviderAsync();
                var result = await provider.SaveAsync(entity) ?? new ChangingResultInfo(ChangeMethods.Invalid);
                Logger?.LogInformation(new EventId(17006003, "SaveEntity"), $"Save ({result.State}) entity {entity.GetType().Name} {entity.Name} ({entity.Id}).");
                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006003, "SaveEntity"), $"Save entity failed, {entity.GetType().Name} {entity.Name} ({entity.Id}). {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                Logger?.LogError(new EventId(17006003, "SaveEntity"), $"Save entity failed with internal error, {entity.GetType().Name} {entity.Name} ({entity.Id}). {ex.GetType().Name} {ex.Message}");
                throw;
            }
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
                if (string.IsNullOrWhiteSpace(id)) return this.ExceptionResult(400, "An entity identifier is required in path.", "NoBody");
                var provider = await GetProviderAsync();
                var content = await JsonObject.ParseAsync(Request.Body);
                var r = await provider.SaveAsync(id, content);
                var entity = (r as ChangingResultInfo<TEntity>)?.Data;
                Logger?.LogInformation(new EventId(17006004, "UpdateEntity"), entity != null ? $"Update entity {entity.GetType().Name} {entity.Name} ({entity.Id})." : $"Failed update entity {id} because of non-existing.");
                return r.ToActionResult();
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006004, "UpdateEntity"), $"Update entity {id} failed. {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                Logger?.LogError(new EventId(17006004, "UpdateEntity"), $"Update entity {id} failed with internal error. {ex.GetType().Name} {ex.Message}");
                throw;
            }
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
                var provider = await GetProviderAsync();
                var entity = await provider.UpdateStateAsync(id, ResourceEntityStates.Deleted);
                Logger?.LogInformation(new EventId(17006005, "DeleteEntity"), entity != null ? $"Delete entity {entity.GetType().Name} {entity.Name} ({entity.Id})." : $"Failed delete entity {id} because of non-existing.");
                return this.ResourceEntityResult(entity);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17006005, "DeleteEntity"), $"Delete entity {id} failed. {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                Logger?.LogError(new EventId(17006005, "DeleteEntity"), $"Delete entity {id} failed with internal error. {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the entity provider.
        /// </summary>
        protected abstract Task<TProvider> GetProviderAsync();
    }
}
