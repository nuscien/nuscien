using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.Json;
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
    public interface IResourceEntityProvider<T> where T : BaseResourceEntity
    {
        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected BaseResourceAccessClient CoreResources { get; }

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
        public Task<ChangingResultInfo> SaveAsync(T value, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The base resource entity accessing service.
    /// </summary>
    /// <typeparam name="T">The type of the resouce entity.</typeparam>
    public abstract class HttpResourceEntityProvider<T> : IResourceEntityProvider<T> where T : BaseResourceEntity
    {
        /// <summary>
        /// Initializes a new instance of the HttpResourceEntityProvider class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="relativePath">The relative path.</param>
        public HttpResourceEntityProvider(HttpResourceAccessClient client, string relativePath)
        {
            CoreResources = client ?? new HttpResourceAccessClient(null, null);
            RelativePath = relativePath;
        }

        /// <summary>
        /// Initializes a new instance of the HttpResourceEntityProvider class.
        /// </summary>
        /// <param name="appKey">The app secret key for accessing API.</param>
        /// <param name="host">The host URI.</param>
        /// <param name="relativePath">The relative path.</param>
        public HttpResourceEntityProvider(AppAccessingKey appKey, Uri host, string relativePath)
        {
            CoreResources = new HttpResourceAccessClient(appKey, host);
            RelativePath = relativePath;
        }

        /// <summary>
        /// Adds or removes the event handler on save.
        /// </summary>
        public event ChangeEventHandler<T> Saved;

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        protected HttpResourceAccessClient CoreResources { get; }

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        protected Users.UserEntity User => CoreResources.User;

        /// <summary>
        /// Gets the current user identifier.
        /// </summary>
        protected string UserId => CoreResources?.User?.Id;

        /// <summary>
        /// Gets the current client identifier.
        /// </summary>
        protected string ClientId => CoreResources.ClientId;

        /// <summary>
        /// Gets a value indicating whether the access token is null, empty or consists only of white-space characters.
        /// </summary>
        protected bool IsTokenNullOrEmpty => CoreResources.IsTokenNullOrEmpty;

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        BaseResourceAccessClient IResourceEntityProvider<T>.CoreResources => CoreResources;

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
            var entity = await client.GetAsync(GetUri("e/" + id), cancellationToken);
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
            var col = await client.GetAsync(GetUri((QueryData)q), cancellationToken);
            return col ?? new CollectionResult<T>(null, q.Offset);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="q">The query data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public async Task<CollectionResult<T>> SearchAsync(QueryData q, CancellationToken cancellationToken = default)
        {
            var client = CreateHttp<CollectionResult<T>>();
            var col = await client.GetAsync(GetUri(q), cancellationToken);
            return col ?? new CollectionResult<T>();
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="value">The entity to add or update.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<ChangingResultInfo> SaveAsync(T value, CancellationToken cancellationToken = default)
        {
            if (value == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity.");
            var client = CreateHttp<ChangingResultInfo>();
            client.SerializeEvenIfFailed = true;
            try
            {
                var change = await client.SendJsonAsync(HttpMethod.Put, GetUri(), value, cancellationToken) ?? new ChangingResultInfo(ChangeMethods.Invalid);
                Saved?.Invoke(this, new ChangeEventArgs<T>(change.State == ChangeMethods.Add ? null : value, value, change.State));
                return change;
            }
            catch (JsonException ex)
            {
                return new ChangingResultInfo(ChangeErrorKinds.Provider, "Cannot serialize the response.", ex);
            }
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public async Task<ChangingResultInfo> SaveAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires identifier.");
            if (delta == null) return new ChangingResultInfo(ChangeMethods.Unchanged, "Nothing need to update.");
            var client = CreateHttp<ChangingResultInfo>();
            client.SerializeEvenIfFailed = true;
            try
            {
                var change = await client.SendJsonAsync(HttpMethod.Put, GetUri("e/" + id), delta, cancellationToken) ?? new ChangingResultInfo(ChangeMethods.Invalid);
                T value = null;
                if (change is ChangingResultInfo<T> r) value = r.Data;
                if (value == null && change.IsSuccessful && change.State != ChangeMethods.Remove)
                {
                    try
                    {
                        value = await GetAsync(id, true, cancellationToken);
                    }
                    catch (FailedHttpException)
                    {
                    }
                    catch (JsonException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (NullReferenceException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                }

                Saved?.Invoke(this, new ChangeEventArgs<T>(change.State == ChangeMethods.Add ? null : value, value, change.State));
                return change;
            }
            catch (JsonException ex)
            {
                return new ChangingResultInfo(ChangeErrorKinds.Provider, "Cannot serialize the response.", ex);
            }
        }

        /// <summary>
        /// Updates the entity state.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="state">The state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public Task<ChangingResultInfo> UpdateStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            return SaveAsync(id, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Combines path to root to generate a URI.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <param name="query">The optional query data.</param>
        /// <returns>A URI.</returns>
        public Uri GetUri(string path, QueryData query = null)
        {
            if (string.IsNullOrWhiteSpace(path)) return CoreResources.GetUri(RelativePath, query);
            if (string.IsNullOrWhiteSpace(RelativePath)) return CoreResources.GetUri(path, query);
            if (path.Contains("://"))
            {
                if (query != null) path = query.ToString(path);
                return new Uri(path);
            }

            var a = RelativePath + (RelativePath[^1] == '/' ? string.Empty : "/");
            var b = path[0] == '/' ? path[0..^1] : path;
            return CoreResources.GetUri(a + b, query);
        }

        /// <summary>
        /// Combines path to root to generate a URI.
        /// </summary>
        /// <param name="query">The optional query data.</param>
        /// <returns>A URI.</returns>
        public Uri GetUri(QueryData query = null) => CoreResources.GetUri(RelativePath, query);

        /// <summary>
        /// Creates a JSON HTTP client.
        /// </summary>
        /// <typeparam name="TResult">The type of response.</typeparam>
        /// <param name="callback">An optional callback raised on data received.</param>
        /// <returns>A new JSON HTTP client.</returns>
        public virtual JsonHttpClient<TResult> CreateHttp<TResult>(Action<ReceivedEventArgs<TResult>> callback = null) => CoreResources.Create(callback);

        /// <summary>
        /// Gets data via network.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="method">HTTP method.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="q">The query data.</param>
        /// <param name="content">The body.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The result.</returns>
        public Task<TResult> GetDataAsync<TResult>(HttpMethod method, string relativePath, QueryData q, object content, CancellationToken cancellationToken = default)
        {
            return CreateHttp<TResult>().SendJsonAsync(method, GetUri(relativePath, q), content, cancellationToken);
        }

        /// <summary>
        /// Gets data via network.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="method">HTTP method.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="q">The query data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The result.</returns>
        public Task<TResult> GetDataAsync<TResult>(HttpMethod method, string relativePath, QueryData q, CancellationToken cancellationToken = default)
        {
            return CreateHttp<TResult>().SendJsonAsync(method, GetUri(relativePath, q), null, cancellationToken);
        }
    }
}
