using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Reflection;
using NuScien.Security;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Tasks;

namespace NuScien.Sns
{
    /// <summary>
    /// The resources of social network.
    /// </summary>
    public static class SocialNetworkResources
    {
        private static Func<BaseResourceAccessClient, Task<BaseSocialNetworkResourceContext>> factory;

        /// <summary>
        /// Sets up the social network resource context.
        /// </summary>
        /// <param name="factory">The instance factory.</param>
        public static void Setup(Func<BaseResourceAccessClient, Task<BaseSocialNetworkResourceContext>> factory)
        {
            SocialNetworkResources.factory = factory;
        }

        /// <summary>
        /// Sets up the social network resource context.
        /// </summary>
        /// <param name="factory">The instance factory.</param>
        public static void Setup(Func<BaseResourceAccessClient, BaseSocialNetworkResourceContext> factory)
        {
            SocialNetworkResources.factory = client => Task.FromResult(factory(client));
        }

        /// <summary>
        /// Sets up the social network resource context.
        /// </summary>
        /// <param name="singleton">The singleton instance.</param>
        public static void Setup(BaseSocialNetworkResourceContext singleton)
        {
            factory = client => Task.FromResult(singleton);
        }

        /// <summary>
        /// Sets up the social network resource context.
        /// </summary>
        /// <param name="providerFactory">The host URI.</param>
        public static void Setup(Func<ISocialNetworkResourceDataProvider> providerFactory)
        {
            factory = client =>
            {
                var provider = providerFactory();
                if (provider is null) return Task.FromResult<BaseSocialNetworkResourceContext>(null);
                return Task.FromResult<BaseSocialNetworkResourceContext>(new OnPremisesSocialNetworkResourceContext(client, provider));
            };
        }

        /// <summary>
        /// Sets up the social network resource context.
        /// </summary>
        /// <param name="providerFactory">The host URI.</param>
        public static void Setup(Func<Task<ISocialNetworkResourceDataProvider>> providerFactory)
        {
            factory = async client =>
            {
                var task = providerFactory();
                var provider = await task;
                if (provider is null) return null;
                return new OnPremisesSocialNetworkResourceContext(client, provider);
            };
        }

        /// <summary>
        /// Gets the social network resource context instance.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <returns>The instance of the social network resource context.</returns>
        public static async Task<BaseSocialNetworkResourceContext> CreateAsync(BaseResourceAccessClient client)
        {
            if (client == null) client = await ResourceAccessClients.CreateAsync();
            if (factory != null) return await factory(client);
            if (client is HttpResourceAccessClient h) return new HttpSocialNetworkResourceContext(h);
            return null;
        }
    }
}
