using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuScien.Data;
using NuScien.Security;
using NuScien.Sns;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Web
{
    /// <summary>
    /// The controller extensions.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Convert to an action result.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="value">The value.</param>
        /// <returns>The action result.</returns>
        public static ActionResult ResourceEntityResult<T>(this ControllerBase controller, T value) where T : BaseResourceEntity
        {
            if (value == null) return controller.NotFound();
            if (!controller.Response.Headers.ContainsKey("Etag")) controller.Response.Headers.Add("Etag", new Microsoft.Extensions.Primitives.StringValues($"{value.Id}-{value.Revision}"));
            return new JsonResult(value);
        }

        /// <summary>
        /// Convert to an action result.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="value">The value.</param>
        /// <param name="offset">The optional offset.</param>
        /// <param name="count">The optional total count.</param>
        /// <returns>The action result.</returns>
        public static ActionResult ResourceEntityResult<T>(this ControllerBase controller, IEnumerable<T> value, int? offset = null, int? count = null) where T : BaseResourceEntity
        {
            if (value == null) return controller.NotFound();
            return new JsonResult(new CollectionResult<T>(value?.ToList(), offset, count));
        }

        /// <summary>
        /// Gets the query arguments instance.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The query arguments instance.</returns>
        public static QueryArgs GetQueryArgs(this IQueryCollection request)
        {
            if (request == null) return null;
            var q = new QueryArgs
            {
                NameQuery = request.GetFirstStringValue("q"),
                NameExactly = request.GetFirstStringValue("eq_name")?.ToLowerInvariant() == JsonBooleanNode.TrueString,
                Count = request.TryGetInt32Value("count") ?? ResourceEntityExtensions.PageSize,
                Offset = request.TryGetInt32Value("offset") ?? 0,
            };
            var state = request.TryGetInt32Value("state");
            if (state.HasValue) q.State = (ResourceEntityStates)state.Value;
            return q;
        }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="doNotWrite">true if do not fill header and write cookie in response automatically; otherwise, false.</param>
        /// <returns>A resource access client.</returns>
        public static async Task<BaseResourceAccessClient> GetResourceAccessClientAsync(this ControllerBase controller, bool doNotWrite = false)
        {
            var r = controller.User is OnPremisesPrincipal principal && principal?.ResourceAccessClient != null
                ? principal.ResourceAccessClient
                : await GetResourceAccessClientAsync(controller.Request);
            if (doNotWrite) return r;

            // Fill header.
            controller.Response.Headers.Remove("X-Auth-Token");
            var token = r?.Token?.AccessToken;
            if (!string.IsNullOrWhiteSpace(token))
                controller.Response.Headers.Add("X-Auth-Token", new Microsoft.Extensions.Primitives.StringValues(r.Token.ToString()));
            
            // Write cookie.
            if (!controller.Request.Cookies.TryGetValue("ns_t", out var cookie) || string.IsNullOrWhiteSpace(cookie))
                cookie = string.Empty;
            try
            {
                var data = QueryData.Parse(cookie) ?? new QueryData();
                var bearerTokenString = data["t"]?.Trim();
                if (string.IsNullOrWhiteSpace(token))
                {
                    controller.Response.Cookies.Delete("ns_t");
                }
                else if (bearerTokenString != token)
                {
                    data["t"] = token;
                    var options = new CookieOptions
                    {
                        HttpOnly = true
                    };
                    if (r.Token != null && r.Token.ExpiredAfter.HasValue) options.MaxAge = r.Token?.ExpiredAfter;
                    controller.Response.Cookies.Append("ns_t", data.ToString(), options);
                }
            }
            catch (FormatException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (NotSupportedException)
            {
            }

            // Return result.
            return r;
        }

        internal static async Task<IActionResult> SaveEntityAsync<T>(this ControllerBase controller, Func<string, BaseResourceAccessClient, Task<T>> entityResolver, Func<T, BaseResourceAccessClient, JsonObjectNode, Task<ChangingResultInfo>> save, string id) where T : BaseResourceEntity
        {
            if (string.IsNullOrWhiteSpace(id)) return ChangeErrorKinds.Argument.ToActionResult("Requires entity identifier.");
            var instance = await controller.GetResourceAccessClientAsync();
            var entity = await entityResolver(id, instance);
            if (controller.Request.Body == null) return new ChangingResultInfo<T>(ChangeMethods.Unchanged, entity).ToActionResult();
            if (entity == null) return ChangeErrorKinds.NotFound.ToActionResult("The resource does not exist.");
            var delta = await JsonObjectNode.ParseAsync(controller.Request.Body);
            if (delta.Count == 0) return new ChangingResultInfo<T>(ChangeMethods.Unchanged, entity, "Nothing update request.").ToActionResult();
            entity.SetProperties(delta);
            var result = await save(entity, instance, delta);
            return result.ToActionResult();
        }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A resource access client.</returns>
        internal static async Task<BaseResourceAccessClient> GetResourceAccessClientAsync(HttpRequest request)
        {
            var client = await ResourceAccessClients.CreateAsync() ?? ResourceAccessClients.MemoryInstance;
            var bearerToken = TryGetStringValue(request.Headers, "Authorization");
            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                if (bearerToken.ToLowerInvariant().StartsWith("bearer "))
                {
                    var bearerTokenString = bearerToken[7..].Trim();
                    await client.AuthorizeAsync(bearerTokenString);
                    return client;
                }
                else if (bearerToken.ToLowerInvariant().StartsWith("basic "))
                {
                    var basicStr = bearerToken[6..].Trim();
                    var basicBytes = Convert.FromBase64String(basicStr);
                    var basicArr = Encoding.UTF8.GetString(basicBytes)?.Split(':');
                    if (basicArr != null && basicArr.Length == 2)
                    {
                        await client.SignInByPasswordAsync(new AppAccessingKey(), basicArr[0], basicArr[1]);
                        return client;
                    }
                }
            }

            if (request.Cookies.TryGetValue("ns_t", out var cookie) && !string.IsNullOrWhiteSpace(cookie))
            {
                try
                {
                    var data = QueryData.Parse(cookie);
                    var bearerTokenString = data["t"]?.Trim();
                    if (!string.IsNullOrWhiteSpace(bearerToken)) await client.AuthorizeAsync(bearerTokenString);
                    return client;
                }
                catch (FormatException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }

            // await client.SignInAsync(request.Body);
            return client;
        }

        internal static ActionResult EmptyEntity(this ControllerBase controller)
        {
            return controller.NotFound();
        }

        internal static ContentResult JsonStringResult(string json)
        {
            return new ContentResult
            {
                StatusCode = 200,
                ContentType = WebFormat.JsonMIME,
                Content = json
            };
        }

        internal static async Task<IActionResult> SaveSnsEntityAsync<T>(this ControllerBase controller, T entity, Func<BaseSocialNetworkResourceContext, T, CancellationToken, Task<ChangingResultInfo>> save, ILogger logger, EventId eventId, CancellationToken cancellationToken = default)
            where T : BaseResourceEntity
        {
            var instance = await controller.GetResourceAccessClientAsync();
            var sns = await SocialNetworkResources.CreateAsync(instance);
            if (sns?.CoreResources == null) return new ChangingResultInfo(ChangeErrorKinds.Unsupported, "Do not support this feature.").ToActionResult();
            if (!sns.CoreResources.IsUserSignedIn) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, "Requires login firstly.").ToActionResult();
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity body.").ToActionResult();
            var result = await save(sns, entity, cancellationToken) ?? new ChangingResultInfo(ChangeMethods.Invalid);
            logger?.LogInformation(eventId, $"Save ({result.State}) entity {entity.GetType().Name} {entity.Name} ({entity.Id}).");
            return result.ToActionResult();
        }

        internal static async Task<IActionResult> UpdateSnsEntityAsync<T>(this ControllerBase controller, string id, Func<BaseSocialNetworkResourceContext, string, CancellationToken, Task<T>> resolver, Func<BaseSocialNetworkResourceContext, T, CancellationToken, Task<ChangingResultInfo>> save, ILogger logger, EventId eventId, CancellationToken cancellationToken = default)
            where T : BaseResourceEntity
        {
            var instance = await controller.GetResourceAccessClientAsync();
            var sns = await SocialNetworkResources.CreateAsync(instance);
            if (sns?.CoreResources == null) return new ChangingResultInfo(ChangeErrorKinds.Unsupported, "Do not support this feature.").ToActionResult();
            if (!sns.CoreResources.IsUserSignedIn) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, "Requires login firstly.").ToActionResult();
            var content = await JsonObjectNode.ParseAsync(controller.Request.Body, default, cancellationToken);
            var entity = await resolver(sns, id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The entity does not exist.").ToActionResult();
            if (content == null || content.Count < 1) return new ChangingResultInfo<T>(ChangeMethods.Unchanged, entity, "Nothing need update.").ToActionResult();
            entity.SetProperties(content);
            var result = await save(sns, entity, cancellationToken) ?? new ChangingResultInfo(ChangeMethods.Invalid);
            logger?.LogInformation(eventId, $"Save ({result.State}) entity {entity.GetType().Name} {entity.Name} ({entity.Id}).");
            return result.ToActionResult();
        }

        internal static async Task<IActionResult> UpdateSnsEntityAsync(this ControllerBase controller, string id, Func<BaseSocialNetworkResourceContext, string, ResourceEntityStates, CancellationToken, Task<ChangingResultInfo>> save, ILogger logger, EventId eventId, CancellationToken cancellationToken = default)
        {
            var instance = await controller.GetResourceAccessClientAsync();
            var sns = await SocialNetworkResources.CreateAsync(instance);
            if (sns?.CoreResources == null) return new ChangingResultInfo(ChangeErrorKinds.Unsupported, "Do not support this feature.").ToActionResult();
            if (!sns.CoreResources.IsUserSignedIn) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, "Requires login firstly.").ToActionResult();
            var content = await JsonObjectNode.ParseAsync(controller.Request.Body, default, cancellationToken);
            if (content == null || content.Count < 1 || !content.TryGetEnumValue<ResourceEntityStates>("state", out var state)) return new ChangingResultInfo(ChangeMethods.Unchanged, "Nothing need update.").ToActionResult();
            var result = await save(sns, id, state, cancellationToken) ?? new ChangingResultInfo(ChangeMethods.Invalid);
            logger?.LogInformation(eventId, $"Update state to {state} ({result.State}) of an entity ({id}).");
            return result.ToActionResult();
        }

        /// <summary>
        /// Tries to get the string value.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="key">The header key.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        private static string TryGetStringValue(IHeaderDictionary header, string key)
        {
            if (!header.TryGetValue(key, out var col)) return null;
            return col.FirstOrDefault(ele => !string.IsNullOrWhiteSpace(ele));
        }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="ignoreUnknownException">true if return null for unknown exception; otherwise, false.</param>
        /// <returns>The action result.</returns>
        private static int? GetStatusCode(Exception ex, bool ignoreUnknownException = false)
        {
            if (ex == null) return 500;
            if (ex.InnerException != null)
            {
                if (ex is AggregateException)
                {
                    ex = ex.InnerException;
                }
                else if (ex is InvalidOperationException)
                {
                    ex = ex.InnerException;
                    ignoreUnknownException = false;
                }
            }

            if (ex is SecurityException) return 403;
            else if (ex is UnauthorizedAccessException) return 401;
            else if (ex is NotSupportedException) return 502;
            else if (ex is NotImplementedException) return 502;
            else if (ex is TimeoutException) return 408;
            else if (ex is OperationCanceledException) return 408;
            if (ignoreUnknownException && !(
                ex is InvalidOperationException
                || ex is ArgumentException
                || ex is NullReferenceException
                || ex is System.Data.Common.DbException
                || ex is System.Text.Json.JsonException
                || ex is System.Runtime.Serialization.SerializationException
                || ex is FailedHttpException
                || ex is IOException
                || ex is ApplicationException
                || ex is InvalidCastException
                || ex is FormatException
                || ex is InvalidDataException)) return null;
            return 500;
        }
    }
}
