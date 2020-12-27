using NuScien.Data;
using NuScien.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Reflection;
using Trivial.Security;

namespace NuScien.Security
{
    /// <summary>
    /// The resource access clients.
    /// </summary>
    public static class ResourceAccessClients
    {
        private static Func<Task<BaseResourceAccessClient>> h;
        private static Guid working = Guid.Empty;

        /// <summary>
        /// Gets the in-memory instance of the resource access client.
        /// It can be used for local demo.
        /// </summary>
        public static OnPremisesResourceAccessClient MemoryInstance { get; } = new OnPremisesResourceAccessClient(new MemoryAccountDbSetProvider());

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <typeparam name="T">The type of the object used to initialize the resource access client, such as an account data provider.</typeparam>
        /// <param name="init">The initialization handler.</param>
        /// <param name="factory">The factory of resource access client.</param>
        public static void Setup<T>(Func<Task<T>> init, Func<T, BaseResourceAccessClient> factory)
        {
            if (factory == null) return;
            if (init == null)
            {
                h = () => Task.FromResult(factory(default));
                working = Guid.Empty;
                return;
            }

            var guid = Guid.NewGuid();
            working = guid;
            var task = init();
            var hasInit = false;
            T result = default;
            if (working == guid) h = async () =>
            {
                if (!hasInit)
                {
                    result = await task;
                    hasInit = true;
                    if (working == guid) h = () => Task.FromResult(factory(result));
                }

                return factory(result);
            };
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="factory">The factory of resource access client.</param>
        public static void Setup(Func<BaseResourceAccessClient> factory)
        {
            if (factory == null) return;
            h = () => Task.FromResult(factory());
            working = Guid.Empty;
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="factory">The factory of resource access client.</param>
        public static void Setup(Func<Task<BaseResourceAccessClient>> factory)
        {
            if (factory == null) return;
            h = factory;
            working = Guid.Empty;
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="dataProvider">The account data provider.</param>
        public static void Setup(IAccountDataProvider dataProvider)
        {
            if (dataProvider == null) return;
            Setup(() => Task.FromResult(dataProvider), d => new OnPremisesResourceAccessClient(d));
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="dataProvider">The account data provider.</param>
        /// <param name="appKey">The first accessing client identifier and secret key to initialize.</param>
        /// <param name="clientInit">Other actions for the accessing client to initialize.</param>
        /// <param name="nameAndPassword">The first user credential to initialize.</param>
        /// <param name="userInit">Other actions for the user to initialize.</param>
        /// <param name="rest">The additional rest actions.</param>
        public static void Setup(IAccountDataProvider dataProvider, AppAccessingKey appKey, Action<AccessingClientEntity> clientInit, PasswordTokenRequestBody nameAndPassword, Action<UserEntity> userInit, Func<IAccountDataProvider, Task> rest = null)
        {
            if (dataProvider == null) return;
            Setup(async () =>
            {
                await InitAsync(dataProvider, appKey, clientInit, nameAndPassword, userInit, rest);
                return dataProvider;
            }, d => new OnPremisesResourceAccessClient(d));
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="dataProvider">The account data provider maker.</param>
        /// <param name="appKey">The first accessing client identifier and secret key to initialize.</param>
        /// <param name="clientInit">Other actions for the accessing client to initialize.</param>
        /// <param name="nameAndPassword">The first user credential to initialize.</param>
        /// <param name="userInit">Other actions for the user to initialize.</param>
        /// <param name="rest">The additional rest actions.</param>
        public static void Setup(Func<Task<IAccountDataProvider>> dataProvider, AppAccessingKey appKey, Action<AccessingClientEntity> clientInit, PasswordTokenRequestBody nameAndPassword, Action<UserEntity> userInit, Func<IAccountDataProvider, Task> rest = null)
        {
            if (dataProvider == null) return;
            Setup(async () =>
            {
                var provider = await dataProvider();
                await InitAsync(provider, appKey, clientInit, nameAndPassword, userInit, rest);
                return provider;
            }, d => new OnPremisesResourceAccessClient(d));
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="dataProvider">The account data provider.</param>
        public static void Setup(Task<IAccountDataProvider> dataProvider)
        {
            if (dataProvider == null) return;
            Setup(() => dataProvider, d => new OnPremisesResourceAccessClient(d));
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="dataProvider">The account data provider maker.</param>
        public static void Setup(Func<Task<IAccountDataProvider>> dataProvider)
        {
            if (dataProvider == null) return;
            Setup(dataProvider, d => new OnPremisesResourceAccessClient(d));
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="appKey">The accessing client app identifier and secret key.</param>
        /// <param name="host">The host URI.</param>
        public static void Setup(AppAccessingKey appKey, Uri host)
        {
            if (appKey == null || host == null) return;
            var dataProvider = new HttpResourceAccessClient(appKey, host);
            h = () => Task.FromResult<BaseResourceAccessClient>(dataProvider);
            working = Guid.Empty;
        }

        /// <summary>
        /// Creates a new resource access client.
        /// </summary>
        /// <returns>A resource access client.</returns>
        public static Task<BaseResourceAccessClient> CreateAsync()
        {
            return h?.Invoke() ?? Task.FromResult<BaseResourceAccessClient>(MemoryInstance);
        }

        /// <summary>
        /// Sets up the resource access client factory.
        /// </summary>
        /// <param name="dataProvider">The account data provider maker.</param>
        /// <param name="appKey">The first accessing client identifier and secret key to initialize.</param>
        /// <param name="clientInit">Other actions for the accessing client to initialize.</param>
        /// <param name="nameAndPassword">The first user credential to initialize.</param>
        /// <param name="userInit">Other actions for the user to initialize.</param>
        /// <param name="rest">The additional rest actions.</param>
        private static async Task InitAsync(IAccountDataProvider dataProvider, AppAccessingKey appKey, Action<AccessingClientEntity> clientInit, PasswordTokenRequestBody nameAndPassword, Action<UserEntity> userInit, Func<IAccountDataProvider, Task> rest = null)
        {
            AccessingClientEntity client = null;
            UserEntity user = null;

            // Initialize a client app.
            if (appKey != null && !string.IsNullOrWhiteSpace(appKey.Id))
            {
                client = await dataProvider.GetClientByNameAsync(appKey.Id);
                if (client == null)
                {
                    client = new AccessingClientEntity
                    {
                        Nickname = "General client",
                        State = ResourceEntityStates.Normal
                    };
                    client.SetKey(appKey);
                    clientInit?.Invoke(client);
                    await dataProvider.SaveAsync(client);
                }
            }

            // Initialize a user.
            if (nameAndPassword != null && !string.IsNullOrWhiteSpace(nameAndPassword.UserName))
            {
                user = await dataProvider.GetUserByLognameAsync(nameAndPassword.UserName);
                if (user == null)
                {
                    user = new UserEntity
                    {
                        Name = nameAndPassword.UserName,
                        Nickname = "Admin",
                        Gender = Genders.Machine,
                        Birthday = new DateTime(2000, 1, 1),
                        State = ResourceEntityStates.Normal
                    };
                    user.SetPassword(nameAndPassword.Password);
                    userInit?.Invoke(user);
                    await dataProvider.SaveAsync(user);
                }
            }

            // Complete rest actions.
            if (rest != null) await rest(dataProvider);
        }
    }
}
