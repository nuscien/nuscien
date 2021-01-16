using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Gets an entity.
        /// </summary>
        /// <returns>The entity.</returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var provider = await GetProviderAsync();
            var entity = await provider.GetAsync(id);
            return this.ResourceEntityResult(entity);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <returns>The collection.</returns>
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var provider = await GetProviderAsync();
            var q = Request.Query.GetQueryArgs();
            var col = await provider.SearchAsync(q);
            return this.ResourceEntityResult(col.Value, col.Offset, col.TotalCount);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing state.</returns>
        [HttpPut]
        public async Task<ChangeMethodResult> Save([FromBody] TEntity entity)
        {
            var provider = await GetProviderAsync();
            var result = await provider.SaveAsync(entity);
            return result;
        }

        /// <summary>
        /// Gets the entity provider.
        /// </summary>
        protected abstract Task<TProvider> GetProviderAsync();
    }
}
