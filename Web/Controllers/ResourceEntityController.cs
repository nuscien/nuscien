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
    public abstract class BaseResourceEntityController<TProvider, TEntity> : ControllerBase
        where TProvider : OnPremisesResourceEntityProvider<TEntity>
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
        public BaseResourceEntityController(ILogger<BaseResourceEntityController<TProvider, TEntity>> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger<BaseResourceEntityController<TProvider, TEntity>> Logger { get; set; }

        /// <summary>
        /// Gets an entity.
        /// </summary>
        /// <returns>The entity.</returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var provider = await GetProviderAsync();
                var entity = await provider.GetAsync(id);
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
        /// Searches.
        /// </summary>
        /// <returns>The collection.</returns>
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            try
            {
                var provider = await GetProviderAsync();
                var q = Request.Query.GetQueryArgs();
                var col = await provider.SearchAsync(q);
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
        /// Saves.
        /// </summary>
        /// <returns>The changing state.</returns>
        [HttpPut]
        public async Task<IActionResult> Save([FromBody] TEntity entity)
        {
            try
            {
                if (entity is null) return this.ExceptionResult(400, "Body request. Require to send the entity in JSON format as request body.", "NoBody");
                var provider = await GetProviderAsync();
                var result = await provider.SaveAsync(entity);
                Logger?.LogInformation(new EventId(17002003, "SaveEntity"), $"Save ({result.State}) entity {entity.GetType().Name} {entity.Name} ({entity.Id}).");
                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                if (er != null)
                {
                    Logger?.LogError(new EventId(17002003, "SaveEntity"), $"Failed save entity {entity.GetType().Name} {entity.Name} ({entity.Id}). {ex.GetType().Name} {ex.Message}");
                    return er;
                }

                throw;
            }
        }

        /// <summary>
        /// Gets the entity provider.
        /// </summary>
        protected abstract Task<TProvider> GetProviderAsync();
    }
}
