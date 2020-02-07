using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Reflection;

namespace NuScien.Security
{
    /// <summary>
    /// The resource access clients.
    /// </summary>
    public static class ResourceAccessClients
    {
        private static readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);
        private static Func<Task<BaseResourceAccessClient>> resolver;

        /// <summary>
        /// Resolves the singleton instance.
        /// </summary>
        /// <returns>The resource access client instance.</returns>
        public static async Task<BaseResourceAccessClient> ResolveAsync()
        {
            if (SingletonResolver.Instance.TryResolve<BaseResourceAccessClient>(out var r)) return r;
            var h = resolver;
            if (h == null) return SingletonResolver.Instance.Resolve<BaseResourceAccessClient>();
            try
            {
                resolver = null;
                await locker.WaitAsync();
                r = await h();
                SingletonResolver.Instance.Register(r);
            }
            finally
            {
                locker.Release();
            }

            return r;
        }

        /// <summary>
        /// Resolves the singleton instance. Registers one if non-exist.
        /// </summary>
        /// <returns>The resource access client instance.</returns>
        public static async Task<BaseResourceAccessClient> EnsureResolveAsync(Func<Task<BaseResourceAccessClient>> resolve)
        {
            if (SingletonResolver.Instance.TryResolve<BaseResourceAccessClient>(out var r)) return r;
            try
            {
                r = await SingletonResolver.Instance.EnsureResolveAsync(resolve);
            }
            finally
            {
                locker.Release();
            }

            return r;
        }

        /// <summary>
        /// Gets the singleton result.
        /// </summary>
        /// <returns>The resource access client instance.</returns>
        public static BaseResourceAccessClient GetResult()
        {
            return SingletonResolver.Instance.Resolve<BaseResourceAccessClient>();
        }

        /// <summary>
        /// Registers an instance.
        /// </summary>
        /// <param name="instance">The resource access client instance.</param>
        public static void Register(BaseResourceAccessClient instance)
        {
            SingletonResolver.Instance.Register(instance);
        }

        /// <summary>
        /// Registers an instance.
        /// </summary>
        /// <param name="resolve">The resource access client resolve handler.</param>
        public static void Register(Func<Task<BaseResourceAccessClient>> resolve)
        {
            resolver = resolve;
        }
    }
}
