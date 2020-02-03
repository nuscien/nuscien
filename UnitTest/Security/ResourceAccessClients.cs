using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NuScien.Security
{
    /// <summary>
    /// The instances and helper of resource access client.
    /// </summary>
    internal static class ResourceAccessClients
    {
        private readonly static Lazy<OnPremisesResourceAccessClient> onPremises = new Lazy<OnPremisesResourceAccessClient>(() =>
        {
            var context = new AccountDbContext(UseSqlServer, "Server=.;Database=NuScien5;Integrated Security=True;");
            var provider = new AccountDbSetProvider(context);
            return new OnPremisesResourceAccessClient(provider);
        });

        /// <summary>
        /// Gets the instance of the on-premises resource access client instance.
        /// </summary>
        public static OnPremisesResourceAccessClient OnPremises => onPremises.Value;

        private static DbContextOptionsBuilder UseSqlServer(DbContextOptionsBuilder builder, string conn) => SqlServerDbContextOptionsExtensions.UseSqlServer(builder, conn);
    }
}
