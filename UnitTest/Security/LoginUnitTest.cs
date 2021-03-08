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
            var client = await ResourceAccessClients.CreateAsync();

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

            // Test for settings.
            await client.SaveSettingsAsync("system", "site", (JsonObject)new Configurations.SystemSiteSettings
            {
                Name = "Sample official website",
                Owner = "Kingcean Tuan"
            });
            var settings = await client.GetSystemSettingsAsync("site");
            Assert.AreEqual("Sample official website", settings.Name);

            // Test for CMS.
            var contents = await client.ListContentAsync("site", true, new QueryArgs());
            Assert.AreEqual(0, contents.Count());
            contents = await client.ListContentAsync("site", true, new QueryArgs
            {
                NameQuery = "Test content",
                NameExactly = true,
                State = ResourceEntityStates.Deleted
            });
            var content = contents.FirstOrDefault() ?? new Cms.ContentEntity
            {
                Name = "Test content",
                OwnerSiteId = "site",
            };
            content.State = ResourceEntityStates.Normal;
            if (content.Config == null) content.Config = new JsonObject();
            content.Config.SetValue("test", "Hellow world!");
            await client.SaveAsync(content, "Test to submit a content.");
            contents = await client.ListContentAsync("site", true, new QueryArgs());
            Assert.AreEqual(1, contents.Count());
            Assert.IsTrue(contents.First().Config.ContainsKey("test"));
            await client.UpdateContentAsync(content.Id, ResourceEntityStates.Deleted, "Remove the test content.");

            // Group.
            var groups = await client.ListGroupsAsync(new QueryArgs
            {
                NameQuery = "TestGroup",
                NameExactly = true
            });
            Assert.AreEqual(0, groups.Count());
            groups = await client.ListGroupsAsync(new QueryArgs
            {
                NameQuery = "TestGroup",
                NameExactly = true,
                State = ResourceEntityStates.Deleted
            }, "site");
            var group = groups.FirstOrDefault() ?? new UserGroupEntity
            {
                Name = "TestGroup",
                OwnerSiteId = "site",
                Nickname = "Test Group",
                MembershipPolicy = UserGroupMembershipPolicies.Application,
                Visibility = UserGroupVisibilities.Public
            };
            group.State = ResourceEntityStates.Normal;
            await client.SaveAsync(group);
            groups = await client.ListGroupsAsync(new QueryArgs
            {
                NameQuery = "TestGroup",
                NameExactly = true
            }, "site");
            Assert.AreEqual(1, groups.Count());
            group = groups.First();
            var relaResult = await client.JoinAsync(group) as ChangingResultInfo<UserGroupRelationshipEntity>;
            Assert.IsNotNull(relaResult);
            Assert.AreEqual(UserGroupRelationshipEntity.Roles.Member, relaResult.Data.Role);
            relaResult.Data.State = ResourceEntityStates.Deleted;
            await client.SaveAsync(relaResult.Data);
            group.State = ResourceEntityStates.Deleted;
            await client.SaveAsync(group);

            // Logout.
            var accessToken = resp.AccessToken;
            await client.SignOutAsync();
            Assert.IsTrue(client.IsTokenNullOrEmpty);
            resp = await client.AuthorizeAsync(accessToken);
            Assert.IsTrue(resp.IsEmpty);
        }
    }
}
