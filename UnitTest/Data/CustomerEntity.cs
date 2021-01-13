using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Net;
using NuScien.Data;
using NuScien.Security;
using Trivial.Security;
using Microsoft.EntityFrameworkCore;

namespace NuScien.Sample
{
    /// <summary>
    /// The customer entity.
    /// </summary>
    public class CustomerEntity : BaseResourceEntity
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        [JsonPropertyName("address")]
        [Column("address")]
        public string Address
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [JsonPropertyName("phone")]
        [Column("phone")]
        public string PhoneNumber
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the identifier of the owner site.
        /// </summary>
        [JsonPropertyName("site")]
        [Column("site")]
        public string SiteId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        #endregion

        #region Member methods

        #endregion

        #region Static methods

        #endregion
    }

    /// <summary>
    /// The data provider for customers.
    /// </summary>
    public class CustomerEntityProvider : OnPremisesResourceEntityProvider<CustomerEntity>
    {
        /// <summary>
        /// Initializes a new instance of the HttpResourceEntityHandler class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="set">The database set.</param>
        /// <param name="save">The entity save handler.</param>
        public CustomerEntityProvider(OnPremisesResourceAccessClient client, DbSet<CustomerEntity> set, Func<CancellationToken, Task<int>> save)
            : base(client, set, save)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OnPremisesResourceEntityHandler class.
        /// </summary>
        /// <param name="dataProvider">The resource data provider.</param>
        /// <param name="set">The database set.</param>
        /// <param name="save">The entity save handler.</param>
        public CustomerEntityProvider(IAccountDataProvider dataProvider, DbSet<CustomerEntity> set, Func<CancellationToken, Task<int>> save)
            : base(dataProvider, set, save)
        {
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="siteId"></param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public Task<CollectionResult<CustomerEntity>> SearchAsync(QueryArgs q, string siteId, CancellationToken cancellationToken)
        {
            var query = q != null ? (QueryData)q : new QueryData();
            if (string.IsNullOrWhiteSpace(siteId)) query["site"] = siteId;
            return SearchAsync(query, cancellationToken);
        }

        /// <inheritdoc />
        protected override IQueryable<CustomerEntity> Filter(IQueryable<CustomerEntity> source, QueryData q)
        {
            var s = q["site"];
            if (!string.IsNullOrWhiteSpace(s)) source = source.Where(ele => ele.SiteId == s);
            s = q["addr"];
            if (!string.IsNullOrWhiteSpace(s)) source = source.Where(ele => ele.Address != null && ele.Address.Contains(s));
            return source;
        }
    }

    /// <summary>
    /// Test business context.
    /// </summary>
    public class TestBusinessContext : OnPremisesResourceAccessContext
    {
        /// <summary>
        /// Gets or sets customer entity provider.
        /// </summary>
        public CustomerEntityProvider Customers { get; set;}

        /// <summary>
        /// Initializes a new instance of the TestBusinessContext class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="dbContext">The database context.</param>
        public TestBusinessContext(OnPremisesResourceAccessClient client, DbContext dbContext)
            : base(client, dbContext)
        {
        }
    }
}
