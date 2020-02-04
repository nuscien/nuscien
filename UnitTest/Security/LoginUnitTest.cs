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
        /// <returns></returns>
        [TestMethod]
        public async Task TestPasswordAsync()
        {
            var client = await ResourceAccessClients.OnPremisesAsync();
            var resp = await client.LoginAsync(new TokenRequest<PasswordTokenRequestBody>(ResourceAccessClients.NameAndPassword, ResourceAccessClients.AppKey));
            if (resp.IsEmpty)
            {
                var user = new UserEntity
                {
                    Name = ResourceAccessClients.NameAndPassword.UserName,
                    Nickname = "Kingcean",
                    Gender = Genders.Male,
                    Birthday = new DateTime(1987, 7, 17),
                    State = ResourceEntityStates.Normal
                };
                user.SetPassword(ResourceAccessClients.NameAndPassword.Password);
                var r = await client.SaveAsync(user);
                Assert.AreEqual(ChangeMethods.Add, r);
                resp = await client.LoginAsync(new TokenRequest<PasswordTokenRequestBody>(ResourceAccessClients.NameAndPassword, ResourceAccessClients.AppKey));
            }

            Assert.IsFalse(resp.IsEmpty);
            Assert.IsNotNull(resp.AccessToken);
            Assert.IsNotNull(resp.User);
            Assert.AreEqual(ResourceAccessClients.NameAndPassword.UserName, resp.User.Name);
        }
    }
}
