using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuScien.Data;
using NuScien.Security;
using NuScien.Users;
using Trivial.Reflection;
using Trivial.Security;

namespace NuScien.UnitTest.Security
{
    /// <summary>
    /// The instances and helper of resource access client.
    /// </summary>
    internal static class ResourceAccessClients
    {
        static ResourceAccessClients()
        {
            NuScien.Security.ResourceAccessClients.Setup(async () =>
            {
                var context = new AccountDbContext(UseSqlServer, "Server=.;Database=NuScien5;Integrated Security=True;");
                var hasCreated = await context.Database.EnsureCreatedAsync();
                return new AccountDbSetProvider(context);
            }, AppKey, null, NameAndPassword, null);
        }

        /// <summary>
        /// The mock app accessing key.
        /// </summary>
        public static AppAccessingKey AppKey = new AppAccessingKey("3F328617-FF17-41FF-7777-150EE49FA87B", "79EA72B7772040A89DF871711967DA1E56789A987654321776EC5CB857775EF4");

        /// <summary>
        /// The user name and password.
        /// </summary>
        public static PasswordTokenRequestBody NameAndPassword = new PasswordTokenRequestBody("kingcean@live.com", ">(-:|[+==== *P@ssw0rd! ====+]|:-)<");

        /// <summary>
        /// Gets the instance of the on-premises resource access client instance.
        /// </summary>
        public static async Task<OnPremisesResourceAccessClient> OnPremisesAsync()
        {
            var r = await NuScien.Security.ResourceAccessClients.CreateAsync();
            return r as OnPremisesResourceAccessClient;
        }

        private static DbContextOptionsBuilder UseSqlServer(DbContextOptionsBuilder builder, string conn) => SqlServerDbContextOptionsExtensions.UseSqlServer(builder, conn);
    }
}
