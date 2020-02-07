﻿using System;
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
using Trivial.Security;
using Trivial.Text;

namespace NuScien.UnitTest.Security
{
    /// <summary>
    /// The login unit test suite.
    /// </summary>
    [TestClass]
    public class LoginUnitTest
    {
        /// <summary>
        /// Tests user name and password login logic.
        /// </summary>
        /// <returns>The async task.</returns>
        [TestMethod]
        public async Task TestPasswordAsync()
        {
            var client = await ResourceAccessClients.OnPremisesAsync();

            // Error handling.
            var resp = await client.SignInByPasswordAsync(ResourceAccessClients.AppKey, ResourceAccessClients.NameAndPassword.UserName, "Wrong Password");
            Assert.IsTrue(resp.IsEmpty);
            resp = await client.SignInByPasswordAsync(ResourceAccessClients.AppKey, "Someone", "Password");
            Assert.IsTrue(resp.IsEmpty);

            // Happy path.
            resp = await client.SignInAsync(ResourceAccessClients.AppKey, ResourceAccessClients.NameAndPassword);
            Assert.IsFalse(resp.IsEmpty);
            Assert.IsNotNull(resp.AccessToken);
            Assert.IsNotNull(resp.User);
            Assert.AreEqual(ResourceAccessClients.NameAndPassword.UserName, resp.User.Name);

            // Authorize by access token.
            resp = await client.AuthorizeAsync(resp.AccessToken);
            Assert.IsFalse(resp.IsEmpty);
            Assert.IsNotNull(resp.AccessToken);

            // Authorize by access token again.
            resp = await client.AuthorizeAsync(resp.AccessToken);
            Assert.IsFalse(resp.IsEmpty);
            Assert.IsNotNull(resp.AccessToken);

            // Logout.
            var accessToken = resp.AccessToken;
            await client.SignOutAsync();
            Assert.IsTrue(client.IsTokenNullOrEmpty);
            resp = await client.AuthorizeAsync(accessToken);
            Assert.IsTrue(resp.IsEmpty);
        }
    }
}