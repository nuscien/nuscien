using System;
using System.Collections.Generic;
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

namespace NuScien.UnitTest.Data
{
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
                    SiteId = string.Empty,
                    State = ResourceEntityStates.Normal
                };
            }
            else
            {
                entity = col.TryGet(0);
            }

            const string PhoneNumber = "114";
            entity.PhoneNumber = PhoneNumber;
            await context.Customers.SaveAsync(entity);
            col = await context.Customers.SearchAsync(new QueryData
            {
                { "phone", PhoneNumber }
            });
            Assert.IsTrue(col.CurrentCount >= 1);
            Assert.AreEqual(PhoneNumber, col.TryGet(0).PhoneNumber);

            // Goods.
            var goods = await context.Goods.SearchAsync("Test");
            Assert.AreEqual(0, goods.CurrentCount);
        }
    }
}
