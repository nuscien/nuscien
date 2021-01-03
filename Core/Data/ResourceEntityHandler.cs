using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Security;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The base resource entity accessing service.
    /// </summary>
    /// <typeparam name="T">The type of the resouce entity.</typeparam>
    public interface IResourceEntityHandler<T> where T : BaseResourceEntity
    {
        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected BaseResourceAccessClient Client { get; }

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public Task<T> GetAsync(string id, bool includeAllStates = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public Task<CollectionResult<T>> SearchAsync(QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangeMethodResult> SaveAsync(T value, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The base resource entity accessing service.
    /// </summary>
    /// <typeparam name="T">The type of the resouce entity.</typeparam>
    public abstract class HttpResourceEntityHandler<T> : IResourceEntityHandler<T> where T : BaseResourceEntity
    {
        /// <summary>
        /// Initializes a new instance of the HttpResourceEntityHandler class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="relativePath">The relative path.</param>
        public HttpResourceEntityHandler(HttpResourceAccessClient client, string relativePath)
        {
            Client = client ?? new HttpResourceAccessClient(null, null);
            RelativePath = relativePath;
        }

        /// <summary>
        /// Initializes a new instance of the HttpResourceEntityHandler class.
        /// </summary>
        /// <param name="appKey">The app secret key for accessing API.</param>
        /// <param name="host">The host URI.</param>
        /// <param name="relativePath">The relative path.</param>
        public HttpResourceEntityHandler(AppAccessingKey appKey, Uri host, string relativePath)
        {
            Client = new HttpResourceAccessClient(appKey, host);
            RelativePath = relativePath;
        }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected HttpResourceAccessClient Client { get; }

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        protected Users.UserEntity User => Client.User;

        /// <summary>
        /// Gets the current client identifier.
        /// </summary>
        protected string ClientId => Client.ClientId;

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        BaseResourceAccessClient IResourceEntityHandler<T>.Client => Client;

        /// <summary>
        /// Gets the relative path.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public async Task<T> GetAsync(string id, bool includeAllStates = false, CancellationToken cancellationToken = default)
        {
            var client = CreateHttp<T>();
            var entity = await client.GetAsync(GetUri(id));
            return entity;
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public async Task<CollectionResult<T>> SearchAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            var client = CreateHttp<CollectionResult<T>>();
            var col = await client.GetAsync(GetUri());
            return col;
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<ChangeMethodResult> SaveAsync(T value, CancellationToken cancellationToken = default)
        {
            var client = CreateHttp<ChangeMethodResult>();
            var change = await client.SendJsonAsync(HttpMethod.Put, GetUri(), value);
            return change;
        }

        /// <summary>
        /// Combines path to root to generate a URI.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <param name="query">The optional query data.</param>
        /// <returns>A URI.</returns>
        public Uri GetUri(string path, QueryData query = null)
        {
            if (string.IsNullOrWhiteSpace(path)) return Client.GetUri(RelativePath, query);
            if (string.IsNullOrWhiteSpace(RelativePath)) return Client.GetUri(path, query);
            var a = RelativePath + (RelativePath[^1] == '/' ? string.Empty : "/");
            var b = path[0] == '/' ? path[0..^1] : path;
            return Client.GetUri(a + b, query);
        }

        /// <summary>
        /// Combines path to root to generate a URI.
        /// </summary>
        /// <param name="query">The optional query data.</param>
        /// <returns>A URI.</returns>
        public Uri GetUri(QueryData query = null) => Client.GetUri(RelativePath, query);

        /// <summary>
        /// Creates a JSON HTTP client.
        /// </summary>
        /// <typeparam name="TResult">The type of response.</typeparam>
        /// <param name="callback">An optional callback raised on data received.</param>
        /// <returns>A new JSON HTTP client.</returns>
        public virtual JsonHttpClient<TResult> CreateHttp<TResult>(Action<ReceivedEventArgs<TResult>> callback = null) => Client.Create(callback);
    }
}
