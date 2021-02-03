using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using NuScien.Cms;
using NuScien.Configurations;
using NuScien.Data;
using NuScien.Sns;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The on-premises resource access client.
    /// </summary>
    public class HttpResourceAccessClient : BaseResourceAccessClient, IDisposable
    {
        /// <summary>
        /// The OAuth client.
        /// </summary>
        private readonly AppAccessingKey appKey;

        /// <summary>
        /// The relative path of core resources.
        /// </summary>
        private string coreResPath = "nuscien5/";

        /// <summary>
        /// Initializes a new instance of the HttpResourceAccessClient class.
        /// </summary>
        /// <param name="appKey">The app secret key for accessing API.</param>
        /// <param name="host">The host URI.</param>
        public HttpResourceAccessClient(AppAccessingKey appKey, Uri host)
        {
            this.appKey = appKey ?? new AppAccessingKey();
            if (host == null)
            {
                Host = new Uri("http://localhost");
                return;
            }

            var url = host.OriginalString;
            var i = url.IndexOf('?');
            if (i >= 0) url = url.Substring(0, i);
            i = url.IndexOf('#');
            if (i >= 0) url = url.Substring(0, i);
            if (url != host.OriginalString) Host = new Uri(url);
            IsLongCacheDuration = true;
        }

        /// <summary>
        /// Initializes a new instance of the OAuthBasedClient class.
        /// </summary>
        /// <param name="appId">The app id.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="host">The host URI.</param>
        public HttpResourceAccessClient(string appId, string secretKey, Uri host) : this(new AppAccessingKey(appId, secretKey), host)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OAuthBasedClient class.
        /// </summary>
        /// <param name="appId">The app id.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="host">The host URI.</param>
        public HttpResourceAccessClient(string appId, SecureString secretKey, Uri host) : this(new AppAccessingKey(appId, secretKey), host)
        {
        }

        /// <summary>
        /// Adds or removes a handler raised on sending.
        /// </summary>
        public event EventHandler<SendingEventArgs> Sending;

        /// <summary>
        /// Adds or removes a handler raised on token is resolving.
        /// </summary>
        protected event EventHandler<SendingEventArgs> TokenResolving;

        /// <summary>
        /// Adds or removes a handler raised on token has resolved.
        /// </summary>
        protected event EventHandler<ReceivedEventArgs<UserTokenInfo>> TokenResolved;

        /// <summary>
        /// Gets the host URI.
        /// </summary>
        public Uri Host { get; }

        /// <summary>
        /// Gets or sets the relative path of core resource.
        /// </summary>
        public string CoreResourceFolderName
        {
            get
            {
                return coreResPath;
            }

            set
            {
                if (value == null) coreResPath = string.Empty;
                if (value.StartsWith("/")) value = value[1..];
                if (!value.EndsWith("/")) value += "/";
                coreResPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP web client handler for sending message.
        /// But token resolving will not use this.
        /// </summary>
        public HttpClientHandler HttpClientHandler { get; set; }

        /// <summary>
        /// Gets or sets the timespan to wait before the request times out.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of bytes to buffer when reading the response content.
        /// The default value for this property is 2 gigabytes.
        /// </summary>
        public long? MaxResponseContentBufferSize { get; set; }

        /// <summary>
        /// Gets the scope to use.
        /// </summary>
        protected IList<string> Scope => Token?.Scope;

        /// <summary>
        /// Gets the app identifier.
        /// </summary>
        public string AppId => appKey?.Id;

        /// <summary>
        /// Gets or sets the JSON deserializer.
        /// </summary>
        public Func<string, Type, object> Deserializer { get; set; }

        /// <summary>
        /// Creates a JSON HTTP client.
        /// </summary>
        /// <typeparam name="T">The type of response.</typeparam>
        /// <param name="callback">An optional callback raised on data received.</param>
        /// <returns>A new JSON HTTP client.</returns>
        public virtual JsonHttpClient<T> Create<T>(Action<ReceivedEventArgs<T>> callback = null)
        {
            var httpClient = new JsonHttpClient<T>();
            if (HttpClientHandler != null) httpClient.Client = new HttpClient(HttpClientHandler, false);
            httpClient.Timeout = Timeout;
            httpClient.MaxResponseContentBufferSize = MaxResponseContentBufferSize;
            httpClient.Sending += (sender, ev) =>
            {
                WriteAuthenticationHeaderValue(ev.RequestMessage.Headers);
                Sending?.Invoke(sender, ev);
            };
            if (Deserializer != null) httpClient.Deserializer = json => (T)Deserializer(json, typeof(T));
            if (callback != null) httpClient.Received += (sender, ev) =>
            {
                callback(ev);
            };
            return httpClient;
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request), "request should not be null");
            var client = CreateHttpClient();
            WriteAuthenticationHeaderValue(request.Headers);
            Sending?.Invoke(this, new SendingEventArgs(request));
            return client.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="completionOption">When the operation should complete (as soon as a response is available or after reading the whole response content).</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request), "request should not be null");
            var client = CreateHttpClient();
            WriteAuthenticationHeaderValue(request.Headers);
            Sending?.Invoke(this, new SendingEventArgs(request));
            return client.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return Create<T>().SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="callback">An optional callback raised on data received.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Task<T> SendAsync<T>(HttpRequestMessage request, Action<ReceivedEventArgs<T>> callback, CancellationToken cancellationToken = default)
        {
            return Create<T>().SendAsync(request, callback, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Task<T> SendAsync<T>(HttpMethod method, Uri requestUri, CancellationToken cancellationToken = default)
        {
            return Create<T>().SendAsync(method ?? HttpMethod.Get, requestUri, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="content">The request message body to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Task<T> SendAsync<T>(HttpMethod method, Uri requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return Create<T>().SendAsync(method ?? (content != null ? HttpMethod.Post : HttpMethod.Get), requestUri, content, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="content">The request message body to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Task<T> SendAsync<T>(HttpMethod method, Uri requestUri, QueryData content, CancellationToken cancellationToken = default)
        {
            return Create<T>().SendAsync(method ?? (content != null ? HttpMethod.Post : HttpMethod.Get), requestUri, content, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="content">The request message body to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Task<T> SendJsonAsync<T>(HttpMethod method, Uri requestUri, object content, CancellationToken cancellationToken = default)
        {
            return Create<T>().SendJsonAsync(method ?? (content != null ? HttpMethod.Post : HttpMethod.Get), requestUri, content, cancellationToken);
        }

        /// <summary>
        /// Releases all resources used by the current OAuth client object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by this instance and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (appKey != null) appKey.Dispose();
        }

        /// <summary>
        /// Creates a token request instance by a specific body.
        /// The current app secret information will be filled into the instance.
        /// </summary>
        /// <typeparam name="T">The type of the body.</typeparam>
        /// <param name="body">The token request body.</param>
        /// <param name="scope">The additional scope.</param>
        /// <returns>A token request instance with given body and the current app secret information.</returns>
        protected TokenRequest<T> CreateTokenRequest<T>(T body, IEnumerable<string> scope = null) where T : TokenRequestBody
        {
            return new TokenRequest<T>(body, appKey, scope);
        }

        /// <summary>
        /// Sends a POST request and gets the result serialized by JSON.
        /// </summary>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>A result serialized.</returns>
        /// <exception cref="ArgumentNullException">content was null.</exception>
        /// <exception cref="InvalidOperationException">TokenResolverUri property was null.</exception>
        /// <exception cref="FailedHttpException">HTTP response contains failure status code.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="OperationCanceledException">The task is cancelled.</exception>
        protected Task<UserTokenInfo> ResolveTokenAsync(TokenRequestBody content, CancellationToken cancellationToken = default)
        {
            var uri = GetUri(coreResPath + "passport/login");
            if (content == null) throw new ArgumentNullException(nameof(content), "content should not be null.");
            var c = new TokenRequest<TokenRequestBody>(content, appKey, Scope);
            return CreateTokenResolveHttpClient().SendAsync(HttpMethod.Post, uri, c.ToQueryData(), cancellationToken);
        }

        /// <summary>
        /// Sends a POST request and gets the result serialized by JSON.
        /// </summary>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>A result serialized.</returns>
        /// <exception cref="ArgumentNullException">content was null.</exception>
        /// <exception cref="InvalidOperationException">TokenResolverUri property was null.</exception>
        /// <exception cref="FailedHttpException">HTTP response contains failure status code.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="OperationCanceledException">The task is cancelled.</exception>
        public async Task<UserTokenInfo> ResolveTokenAsync(CancellationToken cancellationToken = default)
        {
            var uri = GetUri(coreResPath + "passport/login");
            var client = CreateTokenResolveHttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            WriteAuthenticationHeaderValue(request.Headers);
            return await client.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> SignInAsync(TokenRequest<PasswordTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            var eui = AssertTokenRequest(tokenRequest);
            if (eui != null) return eui;
            if (tokenRequest.Body.Password.Length < 1) return new UserTokenInfo
            {
                ErrorCode = "invalid_password",
                ErrorDescription = "The password should not be null."
            };
            return await ResolveTokenAsync(tokenRequest.Body, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> SignInAsync(TokenRequest<RefreshTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            var eui = AssertTokenRequest(tokenRequest);
            if (eui != null) return eui;
            return await ResolveTokenAsync(tokenRequest.Body, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> SignInAsync(TokenRequest<CodeTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            var eui = AssertTokenRequest(tokenRequest);
            if (eui != null) return eui;
            return await ResolveTokenAsync(tokenRequest.Body, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> SignInAsync(TokenRequest<ClientTokenRequestBody> tokenRequest, CancellationToken cancellationToken = default)
        {
            var eui = AssertTokenRequest(tokenRequest);
            if (eui != null) return eui;
            return await ResolveTokenAsync(tokenRequest.Body, cancellationToken);
        }

        /// <summary>
        /// Signs in.
        /// </summary>
        /// <param name="accessToken">The access request.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The login response.</returns>
        public override async Task<UserTokenInfo> AuthorizeAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return await ResolveTokenAsync(cancellationToken);
            if (Token == null) Token = new UserTokenInfo();
            var oldToken = Token;
            Token.AccessToken = accessToken;
            var token = await ResolveTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(token.AccessToken)) Token = oldToken;
            return token;
        }

        /// <summary>
        /// Sets a new authorization code.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="code">The original authorization code.</param>
        /// <param name="insertNewOne">true if need add a new one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public override Task<ChangingResultInfo> SetAuthorizationCodeAsync(string serviceProvider, string code, bool insertNewOne = false, CancellationToken cancellationToken = default)
        {
            return SendChangeAsync(HttpMethod.Put, coreResPath + "passport/authcode/" + serviceProvider, new JsonObject
            {
                { CodeTokenRequestBody.CodeProperty, code },
                { "insert", insertNewOne }
            }, cancellationToken);
        }

        /// <summary>
        /// Signs out.
        /// </summary>
        /// <returns>The task.</returns>
        public override async Task SignOutAsync()
        {
            var t = Token;
            if (t == null || t.IsEmpty) return;
            using var req = new HttpRequestMessage(HttpMethod.Get, GetUri(coreResPath + "passport/logout"));
            await SendAsync(req);
            Token = null;
        }

        /// <summary>
        /// Gets a value indicating whether the specific user login name has been registered.
        /// </summary>
        /// <param name="logname">The user login name.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>true if the specific user login name has been registered; otherwise, false.</returns>
        public override Task<bool> HasUserNameAsync(string logname, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(logname)) return Task.FromResult(false);
            return SendTestingAsync(HttpMethod.Post, coreResPath + "passport/users/exist", new JsonObject
            {
                { "key", "logname" },
                { "value", logname }
            }, cancellationToken);
        }

        /// <summary>
        /// Gets a user entity by given identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public override Task<UserEntity> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return SendAsync<UserEntity>(HttpMethod.Get, GetUri(coreResPath + "passport/user/" + id), cancellationToken);
        }

        /// <summary>
        /// Updates a specific publish content entity.
        /// </summary>
        /// <param name="id">The publish content entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public override Task<ChangingResultInfo> UpdateUserAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires the entity identifier."));
            if (delta == null || delta.Count == 0) return Task.FromResult(new ChangingResultInfo(ChangeMethods.Unchanged, "Nothing request to update."));
            return SendChangeAsync(HttpMethod.Put, $"{coreResPath}passport/user/{id}", delta, cancellationToken);
        }

        /// <summary>
        /// Gets a user group entity by given identifier.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        public override Task<UserGroupEntity> GetUserGroupByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return SendAsync<UserGroupEntity>(HttpMethod.Get, GetUri(coreResPath + "passport/group/" + id), cancellationToken);
        }

        /// <summary>
        /// Updates a specific publish content entity.
        /// </summary>
        /// <param name="id">The publish content entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public override Task<ChangingResultInfo> UpdateUserGroupAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires the entity identifier."));
            if (delta == null || delta.Count == 0) return Task.FromResult(new ChangingResultInfo(ChangeMethods.Unchanged, "Nothing request to update."));
            return SendChangeAsync(HttpMethod.Put, $"{coreResPath}passport/group/{id}", delta, cancellationToken);
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional query request information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public override Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            var query = ToQueryData(q);
            return QueryAsync<UserGroupEntity>(coreResPath + "passport/groups", query, cancellationToken);
        }

        /// <summary>
        /// Searches user groups.
        /// </summary>
        /// <param name="q">The optional query request information.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        public override Task<IEnumerable<UserGroupEntity>> ListGroupsAsync(QueryArgs q, string siteId, CancellationToken cancellationToken = default)
        {
            var query = ToQueryData(q);
            if (!string.IsNullOrWhiteSpace(siteId)) query["site"] = siteId;
            return QueryAsync<UserGroupEntity>(coreResPath + "passport/groups", query, cancellationToken);
        }

        /// <summary>
        /// Renews the client app key.
        /// </summary>
        /// <param name="appId">The client app identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The client app identifier and secret key.</returns>
        public override async Task<AppAccessingKey> RenewAppClientKeyAsync(string appId, CancellationToken cancellationToken = default)
        {
            var r = await SendAsync<JsonObject>(HttpMethod.Post, GetUri($"{coreResPath}passport/credential/client/{appId}/renew"), cancellationToken);
            if (r == null) return null;
            var id = r.GetStringValue("id");
            var secret = r.GetStringValue("secret");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret)) return null;
            return new AppAccessingKey(id, secret);
        }
        /// <summary>
        /// Gets a specific publish content.
        /// </summary>
        /// <param name="id">The identifier of the publish content.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity to get.</returns>
        public override Task<ContentEntity> GetContentAsync(string id, CancellationToken cancellationToken = default)
        {
            return SendAsync<ContentEntity>(HttpMethod.Get, GetUri(coreResPath + "cms/c/" + id), cancellationToken);
        }

        /// <summary>
        /// Lists the publish contents.
        /// </summary>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="parent">The optional parent content identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<IEnumerable<ContentEntity>> ListContentAsync(string siteId, string parent = null, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            var query = ToQueryData(q);
            if (!string.IsNullOrWhiteSpace(siteId)) query["site"] = siteId;
            if (!string.IsNullOrWhiteSpace(parent)) query["parent"] = parent;
            return QueryAsync<ContentEntity>(coreResPath + "cms/c", query, cancellationToken);
        }

        /// <summary>
        /// Lists the publish contents.
        /// </summary>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="all">true if search all contents; otherise, false.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<IEnumerable<ContentEntity>> ListContentAsync(string siteId, bool all, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            return ListContentAsync(siteId, all ? "*" : null, q, cancellationToken);
        }

        /// <summary>
        /// Lists the revision entities.
        /// </summary>
        /// <param name="source">The source owner identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<IEnumerable<ContentRevisionEntity>> ListContentRevisionAsync(string source, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            var query = ToQueryData(q);
            return QueryAsync<ContentRevisionEntity>($"{coreResPath}cms/c/{source}/rev", query, cancellationToken);
        }

        /// <summary>
        /// Lists the revisions.
        /// </summary>
        /// <param name="id">The revision entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<ContentRevisionEntity> GetContentRevisionAsync(string id, CancellationToken cancellationToken = default)
        {
            return SendAsync<ContentRevisionEntity>(HttpMethod.Get, GetUri(coreResPath + "cms/cr/" + id), cancellationToken);
        }

        /// <summary>
        /// Updates a specific publish content entity.
        /// </summary>
        /// <param name="id">The publish content entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public override Task<ChangingResultInfo> UpdateContentAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires the entity identifier."));
            if (delta == null || delta.Count == 0) return Task.FromResult(new ChangingResultInfo(ChangeMethods.Unchanged, "Nothing request to update."));
            return SendChangeAsync(HttpMethod.Put, $"{coreResPath}cms/c/{id}", delta, cancellationToken);
        }

        /// <summary>
        /// Gets a specific publish content template.
        /// </summary>
        /// <param name="id">The identifier of the publish content template.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity to get.</returns>
        public override Task<ContentTemplateEntity> GetContentTemplateAsync(string id, CancellationToken cancellationToken = default)
        {
            return SendAsync<ContentTemplateEntity>(HttpMethod.Get, GetUri(coreResPath + "cms/t/" + id), cancellationToken);
        }

        /// <summary>
        /// Lists the publish content templates.
        /// </summary>
        /// <param name="siteId">The owner site identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<IEnumerable<ContentTemplateEntity>> ListContentTemplateAsync(string siteId, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            var query = ToQueryData(q);
            if (!string.IsNullOrWhiteSpace(siteId)) query["site"] = siteId;
            return QueryAsync<ContentTemplateEntity>(coreResPath + "cms/t", query, cancellationToken);
        }

        /// <summary>
        /// Lists the revision entities.
        /// </summary>
        /// <param name="source">The source owner identifier.</param>
        /// <param name="q">The optional query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<IEnumerable<ContentTemplateRevisionEntity>> ListContentTemplateRevisionAsync(string source, QueryArgs q = null, CancellationToken cancellationToken = default)
        {
            var query = ToQueryData(q);
            return QueryAsync<ContentTemplateRevisionEntity>($"{coreResPath}cms/t/{source}/rev", query, cancellationToken);
        }

        /// <summary>
        /// Lists the revisions.
        /// </summary>
        /// <param name="id">The revision entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<ContentTemplateRevisionEntity> GetContentTemplateRevisionAsync(string id, CancellationToken cancellationToken = default)
        {
            return SendAsync<ContentTemplateRevisionEntity>(HttpMethod.Get, GetUri(coreResPath + "cms/tr/" + id), cancellationToken);
        }
        /// <summary>
        /// Updates a specific publish content entity.
        /// </summary>
        /// <param name="id">The publish content entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public override Task<ChangingResultInfo> UpdateContentTemplateAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires the entity identifier."));
            if (delta == null || delta.Count == 0) return Task.FromResult(new ChangingResultInfo(ChangeMethods.Unchanged, "Nothing request to update."));
            return SendChangeAsync(HttpMethod.Put, $"{coreResPath}cms/t/{id}", delta, cancellationToken);
        }

        /// <summary>
        /// Lists the publish content comments.
        /// </summary>
        /// <param name="content">The owner content comment identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<IEnumerable<ContentCommentEntity>> ListContentCommentsAsync(string content, bool plain, CancellationToken cancellationToken = default)
        {
            QueryData query = null;
            if (plain) query = new QueryData
            {
                { "plain", true }
            };
            return QueryAsync<ContentCommentEntity>($"{coreResPath}cms/c/{content}/comments", query, cancellationToken);
        }

        /// <summary>
        /// Gets a specific publish content comment.
        /// </summary>
        /// <param name="id">The identifier of the publish content template.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity to delete.</returns>
        public override Task<ContentCommentEntity> GetContentCommentAsync(string id, CancellationToken cancellationToken = default)
        {
            return SendAsync<ContentCommentEntity>(HttpMethod.Get, GetUri($"{coreResPath}cms/cc/{id}"), cancellationToken);
        }

        /// <summary>
        /// Lists the child comments of a specific publish content comment.
        /// </summary>
        /// <param name="id">The parent identifier of the content comment.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity list.</returns>
        public override Task<IEnumerable<ContentCommentEntity>> ListChildContentCommentsAsync(string id, CancellationToken cancellationToken = default)
        {
            return QueryAsync<ContentCommentEntity>($"{coreResPath}cms/cc/{id}/children", null, cancellationToken);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        protected override async Task<SettingsEntity.Model> GetSettingsModelByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            var t = GetSettingsJsonStringByKeyAsync(key, null, cancellationToken);
            if (string.IsNullOrWhiteSpace(siteId))
                return new SettingsEntity.Model(key, await t);
            var m = await GetSettingsJsonStringByKeyAsync(key, siteId, cancellationToken);
            return new SettingsEntity.Model(key, siteId, m, await t);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        protected override Task<JsonObject> GetSettingsDataByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return SendAsync<JsonObject>(HttpMethod.Get, GetUri(string.IsNullOrWhiteSpace(siteId) ? $"{coreResPath}settings/global/{key.Trim()}" : $"{coreResPath}settings/site/{siteId}/{key.Trim()}"), cancellationToken);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier; null for global configuration data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The value.</returns>
        protected override Task<string> GetSettingsJsonStringByKeyAsync(string key, string siteId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return SendAsync<string>(HttpMethod.Get, GetUri(string.IsNullOrWhiteSpace(siteId) ? $"{coreResPath}settings/global/{key.Trim()}" : $"{coreResPath}settings/site/{siteId}/{key.Trim()}"), cancellationToken);
        }

        /// <summary>
        /// Creates or updates the settings.
        /// </summary>
        /// <param name="key">The settings key with optional namespace.</param>
        /// <param name="siteId">The owner site identifier if bound to a site; otherwise, null.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        public override Task<ChangingResultInfo> SaveSettingsAsync(string key, string siteId, JsonObject value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires key."));
            key = key.Trim();
            return SendChangeAsync(HttpMethod.Put, string.IsNullOrWhiteSpace(siteId) ? $"{coreResPath}settings/global/{key}" : $"{coreResPath}settings/site/{siteId}/{key}", value, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user permission item entity.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="targetType">The target entity type.</param>
        /// <param name="targetId">The target entity identifier.</param>
        /// <param name="permissionList">The permission list.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        public override Task<ChangingResultInfo> SavePermissionAsync(string siteId, SecurityEntityTypes targetType, string targetId, IEnumerable<string> permissionList, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(siteId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires site identifier."));
            siteId = siteId.Trim();
            var content = new PermissionRequestArgs
            {
                Permissions = permissionList
            };
            return targetType switch
            {
                SecurityEntityTypes.User => SendChangeAsync(HttpMethod.Put, $"{coreResPath}settings/perms/{siteId}/user/{targetId}", content, cancellationToken),
                SecurityEntityTypes.UserGroup => SendChangeAsync(HttpMethod.Put, $"{coreResPath}settings/perms/{siteId}/group/{targetId}", content, cancellationToken),
                SecurityEntityTypes.ServiceClient => SendChangeAsync(HttpMethod.Put, $"{coreResPath}settings/perms/{siteId}/client/{targetId}", content, cancellationToken),
                _ => Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unsupported, "The target type is not supported.")),
            };
        }

        /// <summary>
        /// Combines path to root to generate a URI.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <param name="query">The optional query data.</param>
        /// <returns>A URI.</returns>
        public Uri GetUri(string path, QueryData query = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return query == null ? Host : new Uri(query.ToString(Host.OriginalString));
            }

            if (path.Contains("://"))
            {
                if (query != null) path = query.ToString(path);
                return new Uri(path);
            }

            var q = Host.OriginalString;
            if (!string.IsNullOrEmpty(q) && q.EndsWith('/')) q += '/';
            if (path.StartsWith('/')) path = path[1..];
            q += path;
            if (query != null) q = query.ToString(q);
            return new Uri(q);
        }

        /// <summary>
        /// Searches users.
        /// </summary>
        /// <param name="group">The user group entity.</param>
        /// <param name="role">The role to search; or null for all roles.</param>
        /// <param name="q">The optional query information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The token entity matched if found; otherwise, null.</returns>
        protected override Task<IEnumerable<UserEntity>> ListUsersByGroupAsync(UserGroupEntity group, UserGroupRelationshipEntity.Roles? role, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (group == null) return QueryAsync<UserEntity>();
            if (q == null) q = InternalAssertion.DefaultQueryArgs;
            var query = ToQueryData(q);
            query["role"] = ((int)role).ToString();
            return QueryAsync<UserEntity>(coreResPath + "passport/users/group/" + group.Id, query, cancellationToken);
        }

        /// <summary>
        /// Gets a collection of user groups joined in.
        /// </summary>
        /// <param name="q">The optional query for group.</param>
        /// <param name="relationshipState">The relationship entity state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group relationships.</returns>
        protected override async Task<IEnumerable<UserGroupRelationshipEntity>> GetRelationshipsAsync(string q, ResourceEntityStates relationshipState, CancellationToken cancellationToken = default)
        {
            var query = new QueryData();
            if (!string.IsNullOrWhiteSpace(q)) query["q"] = q;
            query["state"] = ((int)relationshipState).ToString();
            var col = await SendAsync<UserGroupRelationshipCollection>(HttpMethod.Get, GetUri(coreResPath + "passport/rela", query), cancellationToken);
            return col.Relationships;
        }

        /// <summary>
        /// Gets the user permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user permission list.</returns>
        protected override Task<UserPermissionItemEntity> GetUserPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
        {
            return SendAsync<UserPermissionItemEntity>(HttpMethod.Get, GetUri($"{coreResPath}settings/perms/{siteId}/user"), cancellationToken);
        }

        /// <summary>
        /// Gets the user group permissions of the current user.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group permission list.</returns>
        protected override Task<IEnumerable<UserGroupPermissionItemEntity>> GetGroupPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
        {
            return QueryAsync<UserGroupPermissionItemEntity>(HttpMethod.Get, $"{coreResPath}settings/perms/{siteId}/groups", null, cancellationToken);
        }

        /// <summary>
        /// Gets the client permissions of the current client.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The client permission list.</returns>
        protected override Task<ClientPermissionItemEntity> GetClientPermissionsAsync(string siteId, CancellationToken cancellationToken = default)
        {
            return SendAsync<ClientPermissionItemEntity>(HttpMethod.Get, GetUri($"{coreResPath}settings/perms/{siteId}/client"), cancellationToken);
        }

        /// <summary>
        /// Gets a user group relationship entity.
        /// </summary>
        /// <param name="groupId">The user group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The user group entity matched if found; otherwise, null.</returns>
        protected override Task<UserGroupRelationshipEntity> GetRelationshipAsync(string groupId, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(userId)) return Task.FromResult<UserGroupRelationshipEntity>(null);
            return SendAsync<UserGroupRelationshipEntity>(HttpMethod.Get, GetUri($"{coreResPath}passport/rela/g/{groupId}/{userId}"), cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user entity.
        /// </summary>
        /// <param name="value">The user entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        protected override Task<ChangingResultInfo> SaveEntityAsync(UserEntity value, CancellationToken cancellationToken = default)
        {
            return SendChangeAsync(HttpMethod.Put, coreResPath + "passport/user", value, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user group entity.
        /// </summary>
        /// <param name="value">The user group entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        protected override Task<ChangingResultInfo> SaveEntityAsync(UserGroupEntity value, CancellationToken cancellationToken = default)
        {
            return SendChangeAsync(HttpMethod.Put, coreResPath + "passport/group", value, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a user group relationship entity.
        /// </summary>
        /// <param name="value">The user group relationship entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The status of changing result.</returns>
        protected override Task<ChangingResultInfo> SaveEntityAsync(UserGroupRelationshipEntity value, CancellationToken cancellationToken = default)
        {
            return SendChangeAsync(HttpMethod.Put, coreResPath + "passport/rela", value, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a publish content entity.
        /// </summary>
        /// <param name="content">The publish content entity to save.</param>
        /// <param name="message">The commit message.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        protected override Task<ChangingResultInfo> SaveEntityAsync(ContentEntity content, string message, CancellationToken cancellationToken = default)
        {
            if (content == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires the content."));
            var json = JsonObject.ConvertFrom(content);
            json.SetValue("message", message);
            return SendChangeAsync(HttpMethod.Put, coreResPath + "cms/c", json, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a publish content template entity.
        /// </summary>
        /// <param name="template">The publish content template entity to save.</param>
        /// <param name="message">The commit message.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        protected override Task<ChangingResultInfo> SaveEntityAsync(ContentTemplateEntity template, string message, CancellationToken cancellationToken = default)
        {
            if (template == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires the content template."));
            var json = JsonObject.ConvertFrom(template);
            json.SetValue("message", message);
            return SendChangeAsync(HttpMethod.Put, coreResPath + "cms/t", json, cancellationToken);
        }

        /// <summary>
        /// Creates or updates a publish content comment entity.
        /// </summary>
        /// <param name="comment">The publish content comment entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The change method.</returns>
        protected override Task<ChangingResultInfo> SaveEntityAsync(ContentCommentEntity comment, CancellationToken cancellationToken = default)
        {
            return SendChangeAsync(HttpMethod.Put, coreResPath + "cms/cc", comment, cancellationToken);
        }

        private async Task<ChangingResultInfo> SendChangeAsync(HttpMethod method, string path, object content, CancellationToken cancellationToken = default)
        {
            var url = GetUri(path);
            try
            {
                var result = await SendJsonAsync<ChangingResultInfo>(method, url, content, cancellationToken);
                if (result == null) return ChangeMethods.Unknown;
                return result;
            }
            catch (JsonException ex)
            {
                return new ChangingResultInfo(ChangeErrorKinds.Provider, "Cannot serialize the response.", ex);
            }
        }

        private async Task<bool> SendTestingAsync(HttpMethod method, string path, object content, CancellationToken cancellationToken = default)
        {
            var url = GetUri(path);
            using var req = new HttpRequestMessage(method, url);
            if (content != null) req.Content = HttpClientExtensions.CreateJsonContent(content);
            var result = await SendAsync(req, cancellationToken);
            return result?.IsSuccessStatusCode == true;
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(HttpMethod method, string path, object content, CancellationToken cancellationToken = default)
        {
            var url = GetUri(path);
            var result = await SendJsonAsync<CollectionResult<T>>(method, url, content, cancellationToken);
            return result?.Value;
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(string path, QueryData q, CancellationToken cancellationToken = default)
        {
            var url = GetUri(path, q);
            var result = await SendJsonAsync<CollectionResult<T>>(HttpMethod.Get, url, null, cancellationToken);
            return result?.Value;
        }

        private static Task<IEnumerable<T>> QueryAsync<T>()
        {
            var list = new List<T>();
            IEnumerable<T> col = list.AsReadOnly();
            return Task.FromResult(col);
        }

        /// <summary>
        /// Creates the JSON HTTP web client for resolving token.
        /// </summary>
        /// <returns>A result serialized.</returns>
        private JsonHttpClient<UserTokenInfo> CreateTokenResolveHttpClient()
        {
            var httpClient = new JsonHttpClient<UserTokenInfo>
            {
                SerializeEvenIfFailed = true,
                Timeout = Timeout,
                MaxResponseContentBufferSize = MaxResponseContentBufferSize
            };
            httpClient.Received += (sender, ev) =>
            {
                if (!ev.IsSuccessStatusCode)
                {
                    TokenResolved?.Invoke(sender, ev);
                    return;
                }

                Token = ev.Result;
                TokenResolved?.Invoke(sender, ev);
            };
            if (Deserializer != null) httpClient.Deserializer = json => (UserTokenInfo)Deserializer(json, typeof(UserTokenInfo));
            if (TokenResolving != null) httpClient.Sending += (sender, ev) =>
            {
                TokenResolving?.Invoke(sender, ev);
            };
            return httpClient;
        }

        /// <summary>
        /// Creats an HTTP web client.
        /// </summary>
        /// <returns>The HTTP web client.</returns>
        private HttpClient CreateHttpClient()
        {
            var client = HttpClientHandler != null ? new HttpClient(HttpClientHandler, false) : new HttpClient();
            var timeout = Timeout;
            if (timeout.HasValue)
            {
                try
                {
                    client.Timeout = timeout.Value;
                }
                catch (ArgumentException)
                {
                }
            }

            var maxBufferSize = MaxResponseContentBufferSize;
            if (maxBufferSize.HasValue)
            {
                try
                {
                    client.MaxResponseContentBufferSize = maxBufferSize.Value;
                }
                catch (ArgumentException)
                {
                }
            }

            return client;
        }

        private static QueryData ToQueryData(QueryArgs q)
        {
            if (q == null) q = InternalAssertion.DefaultQueryArgs;
            var query = new QueryData();
            if (!string.IsNullOrWhiteSpace(q.NameQuery)) query["q"] = q.NameQuery;
            if (q.NameExactly) query["eq_name"] = JsonBoolean.TrueString;
            query["count"] = q.Count.ToString();
            query["offset"] = q.Offset.ToString();
            query["state"] = ((int)q.State).ToString();
            return query;
        }

        private UserTokenInfo AssertTokenRequest(TokenRequest tokenRequest)
        {
            if (tokenRequest == null || tokenRequest.Body == null) return new UserTokenInfo
            {
                ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest,
                ErrorDescription = "Miss the token request information."
            };
            if (tokenRequest.ClientCredentials != appKey) return new UserTokenInfo
            {
                ErrorCode = TokenInfo.ErrorCodeConstants.InvalidClient,
                ErrorDescription = "Cannot login by this client because the token request is not for this."
            };
            return null;
        }
    }
}
