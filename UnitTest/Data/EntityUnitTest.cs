using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuScien.Data;
using NuScien.Security;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

using ResourceAccessClients = NuScien.UnitTest.Security.ResourceAccessClients;

namespace NuScien.UnitTest.Data;

/// <summary>
/// The unit test for entity.
/// </summary>
[TestClass]
public class EntityUnitTest
{
    /// <summary>
    /// Tests user name and password login logic.
    /// </summary>
    /// <returns>The async task.</returns>
    [TestMethod]
    public async Task TestEntityAsync()
    {
        var context = await TestBusinessContext.CreateAsync();

        // Customers.
        var col = await context.Customers.SearchAsync("Test");
        CustomerEntity entity;
        if (col.CurrentCount == 0)
        {
            entity = new CustomerEntity
            {
                Name = "Test",
                OwnerSiteId = string.Empty,
                State = ResourceEntityStates.Normal
            };
        }
        else
        {
            entity = col.TryGetValue(0);
        }

        const string PhoneNumber = "114";
        entity.PhoneNumber = PhoneNumber;
        await context.Customers.SaveAsync(entity);
        col = await context.Customers.SearchAsync(new QueryData
        {
            { "phone", PhoneNumber }
        });
        Assert.IsTrue(col.CurrentCount >= 1);
        Assert.AreEqual(PhoneNumber, col.TryGetValue(0).PhoneNumber);

        // Goods.
        var goods = await context.Goods.SearchAsync("Test");
        Assert.AreEqual(0, goods.CurrentCount);
        goods = await context.Goods.SearchAsync(new QueryArgs
        {
            NameQuery = "Test",
            State = ResourceEntityStates.Deleted
        });
        var good = (goods.CurrentCount > 0 ? goods.Data.FirstOrDefault() : null) ?? new GoodEntity
        {
            Name = "Test",
            OwnerSiteId = string.Empty
        };
        good.State = ResourceEntityStates.Normal;
        var result = await context.Goods.SaveAsync(good);
        Assert.AreEqual(ChangeMethods.Invalid, result.State);
        await context.CoreResources.SignInAsync(ResourceAccessClients.AppKey, ResourceAccessClients.NameAndPassword);
        result = await context.Goods.SaveAsync(good);
        Assert.AreNotEqual(ChangeMethods.Invalid, result.State);
        goods = await context.Goods.SearchAsync("Test");
        Assert.AreEqual(1, goods.CurrentCount);
        Assert.AreEqual(good.Id, goods.Data.FirstOrDefault()?.Id);
        good = goods.Data.First();
        var json = JsonSerializer.Serialize(good);
        good = JsonSerializer.Deserialize<GoodEntity>(json);
        Assert.IsNotNull(good);
        good.State = ResourceEntityStates.Deleted;
        context = await TestBusinessContext.CreateAsync(context.CoreResources);
        await context.Goods.SaveAsync(good);
        goods = await context.Goods.SearchAsync("Test");
        Assert.AreEqual(0, goods.CurrentCount);
        good = await context.Goods.GetAsync(good.Id, true);
        Assert.IsNotNull(good);
        Assert.AreEqual("Test", good.Name);
        good = await context.Goods.GetAsync(good.Id);
        Assert.IsNull(good);

        // Sign out
        await context.CoreResources.SignOutAsync();
    }
}
