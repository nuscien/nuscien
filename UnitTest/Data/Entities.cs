﻿using System;
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

using Microsoft.EntityFrameworkCore;
using NuScien.Data;
using NuScien.Security;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;
using Trivial.Net;

using TestResourceAccessClients = NuScien.UnitTest.Security.ResourceAccessClients;

namespace NuScien.UnitTest.Data;

/// <summary>
/// The customer entity.
/// </summary>
[Table("testcustomers")]
public class CustomerEntity : SiteOwnedResourceEntity
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

    #endregion

    #region Member methods

    #endregion

    #region Static methods

    #endregion
}

/// <summary>
/// The customer entity.
/// </summary>
[Table("testgoods")]
public class GoodEntity : SiteOwnedResourceEntity
{
    #region Constructors

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the image.
    /// </summary>
    [JsonPropertyName("image")]
    [Column("image")]
    public string Image
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
    protected override void MapQuery(QueryPredication<CustomerEntity> source)
    {
        source.AddForString("site", info => info.Source.Where(ele => ele.OwnerSiteId == info.Value), true);
        source.AddForString("addr", info => info.Source.Where(ele => ele.Address != null && ele.Address.Contains(info.Value)), true);
        source.AddForString("phone", info => info.Source.Where(ele => ele.PhoneNumber == info.Value), true);
    }
}

/// <summary>
/// The data provider for goods.
/// </summary>
public class GoodEntityProvider : OnPremisesResourceEntityProvider<GoodEntity>
{
    /// <summary>
    /// Initializes a new instance of the GoodEntityProvider class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="set">The database set.</param>
    /// <param name="save">The entity save handler.</param>
    public GoodEntityProvider(OnPremisesResourceAccessClient client, DbSet<GoodEntity> set, Func<CancellationToken, Task<int>> save)
        : base(client, set, save)
    {
    }

    /// <summary>
    /// Initializes a new instance of the GoodEntityProvider class.
    /// </summary>
    /// <param name="dataProvider">The resource data provider.</param>
    /// <param name="set">The database set.</param>
    /// <param name="save">The entity save handler.</param>
    public GoodEntityProvider(IAccountDataProvider dataProvider, DbSet<GoodEntity> set, Func<CancellationToken, Task<int>> save)
        : base(dataProvider, set, save)
    {
    }

    /// <inheritdoc />
    public override async Task<ChangingResultInfo> SaveAsync(GoodEntity value, CancellationToken cancellationToken = default)
    {
        if (!CoreResources.IsUserSignedIn) return new ChangingResultInfo(ChangeMethods.Invalid);
        return await base.SaveAsync(value, cancellationToken);
    }

    /// <inheritdoc />
    protected override void MapQuery(QueryPredication<GoodEntity> source)
    {
        source.AddForString("site", info => info.Source.Where(ele => ele.OwnerSiteId == info.Value), true);
    }
}

/// <summary>
/// Test business context.
/// </summary>
public class TestBusinessContext : OnPremisesResourceAccessContext
{
    /// <summary>
    /// Gets an instance.
    /// </summary>
    /// <param name="core">The core resources.</param>
    /// <returns>An instance.</returns>
    public static async Task<TestBusinessContext> CreateAsync(OnPremisesResourceAccessClient core = null)
    {
        if (core == null) core = await TestResourceAccessClients.CreateAsync();
        var options = TestResourceAccessClients.CreateDbContextOptions();
        var result = new TestBusinessContext(core, options);
        await result.EnsureDbCreatedAsync();
        return result;
    }

    /// <summary>
    /// Gets or sets customer entity provider.
    /// </summary>
    public CustomerEntityProvider Customers { get; set;}

    /// <summary>
    /// Gets or sets good entity provider.
    /// </summary>
    public GoodEntityProvider Goods { get; set; }

    /// <summary>
    /// Initializes a new instance of the TestBusinessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="dbContext">The database context.</param>
    public TestBusinessContext(OnPremisesResourceAccessClient client, DbContext dbContext)
        : base(client, dbContext)
    {
    }

    /// <summary>
    /// Initializes a new instance of the TestBusinessContext class.
    /// </summary>
    /// <param name="client">The resource access client.</param>
    /// <param name="options">The options for this context.</param>
    public TestBusinessContext(OnPremisesResourceAccessClient client, DbContextOptions options)
        : base(client, options)
    {
    }
}
