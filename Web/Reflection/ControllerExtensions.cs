using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuScien.Data;
using NuScien.Security;
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
        /// Gets the first string value.
        /// </summary>
        /// <param name="request">The form collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="trim">true if need trim; otherwise, false.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        public static string GetFirstStringValue(this IFormCollection request, string key, bool trim = false)
        {
            var col = request[key];
            string str = null;
            if (trim)
            {
                foreach (var ele in col)
                {
                    if (ele == null) continue;
                    var s = ele.Trim();
                    if (s.Length == 0)
                    {
                        if (str == null) str = string.Empty;
                        continue;
                    }

                    str = s;
                }
            }
            else
            {
                foreach (var ele in col)
                {
                    if (ele == null) continue;
                    if (ele.Length == 0)
                    {
                        if (str == null) str = string.Empty;
                        continue;
                    }

                    str = ele;
                }
            }

            return str;
        }

        /// <summary>
        /// Gets the merged string value.
        /// </summary>
        /// <param name="request">The form collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="split">The splitter charactor.</param>
        /// <param name="trim">true if need trim; otherwise, false.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        public static string GetMergedStringValue(this IFormCollection request, string key, char split = ',', bool trim = false)
        {
            IEnumerable<string> col = request[key];
            if (trim) col = col.Select(ele => ele?.Trim()).Where(ele => !string.IsNullOrEmpty(ele));
            return string.Join(split, col);
        }

        /// <summary>
        /// Gets the first string value.
        /// </summary>
        /// <param name="request">The query collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="trim">true if need trim; otherwise, false.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        public static string GetFirstStringValue(this IQueryCollection request, string key, bool trim = false)
        {
            var col = request[key];
            string str = null;
            if (trim)
            {
                foreach (var ele in col)
                {
                    if (ele == null) continue;
                    var s = ele.Trim();
                    if (s.Length == 0)
                    {
                        if (str == null) str = string.Empty;
                        continue;
                    }

                    str = s;
                }
            }
            else
            {
                foreach (var ele in col)
                {
                    if (ele == null) continue;
                    if (ele.Length == 0)
                    {
                        if (str == null) str = string.Empty;
                        continue;
                    }

                    str = ele;
                }
            }

            return str;
        }

        /// <summary>
        /// Gets the merged string value.
        /// </summary>
        /// <param name="request">The query collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="split">The splitter charactor.</param>
        /// <param name="trim">true if need trim; otherwise, false.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        public static string GetMergedStringValue(this IQueryCollection request, string key, char split = ',', bool trim = false)
        {
            IEnumerable<string> col = request[key];
            if (trim) col = col.Select(ele => ele?.Trim()).Where(ele => !string.IsNullOrEmpty(ele));
            return string.Join(split, col);
        }

        /// <summary>
        /// Gets the integer value.
        /// </summary>
        /// <param name="request">The query collection.</param>
        /// <param name="key">The key.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        public static int? TryGetInt32Value(this IQueryCollection request, string key)
        {
            var s = request[key].Select(ele => ele?.Trim()).FirstOrDefault(ele => !string.IsNullOrEmpty(ele));
            if (int.TryParse(s, out var r)) return r;
            return null;
        }

        /// <summary>
        /// Gets the query data.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="encoding">The optional encoding.</param>
        /// <returns>The string value; or null, if non-exist.</returns>
        public static async Task<QueryData> ReadBodyAsQueryDataAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (request == null || request.Body == null) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            using var reader = new StreamReader(request.Body, encoding);
            var query = await reader.ReadToEndAsync();
            var q = new QueryData();
            q.ParseSet(query, false, encoding);
            return q;
        }

        /// <summary>
        /// Convert to an action result.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The action result.</returns>
        public static ActionResult ToActionResult(this ChangingResultInfo value)
        {
            if (value == null) return new NotFoundResult();
            var ex = value.GetException();
            var status = ex != null ? (GetStatusCode(ex) ?? 500) : 200;
            if (status >= 300)
            {
                status = value.ErrorCode switch
                {
                    ChangeErrorKinds.NotFound => 404,
                    ChangeErrorKinds.Busy => 503,
                    ChangeErrorKinds.Unsupported => 501,
                    ChangeErrorKinds.Conflict => 409,
                    _ => status
                };
            }

            return new JsonResult(value)
            {
                StatusCode = status
            };
        }

        /// <summary>
        /// Convert to an action result.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The action result.</returns>
        public static ActionResult ToActionResult(this ChangeMethods value)
        {
            return ToActionResult(new ChangingResultInfo(value));
        }

        /// <summary>
        /// Convert to an action result.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The action result.</returns>
        public static ContentResult ToActionResult(this JsonObject value)
        {
            return new ContentResult
            {
                ContentType = WebFormat.JsonMIME,
                StatusCode = 200,
                Content = value.ToString()
            };
        }

        /// <summary>
        /// Convert to an action result.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The action result.</returns>
        public static ContentResult ToActionResult(this JsonArray value)
        {
            return new ContentResult
            {
                ContentType = WebFormat.JsonMIME,
                StatusCode = 200,
                Content = value.ToString()
            };
        }

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
        /// Converts an exception to action result with exception message.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="ignoreUnknownException">true if return null for unknown exception; otherwise, false.</param>
        /// <returns>The action result.</returns>
        public static ActionResult ExceptionResult(this ControllerBase controller, Exception ex, bool ignoreUnknownException = false)
        {
            if (ex == null) return controller.StatusCode(500);
            var result = new ErrorMessageResult(ex);
            var status = GetStatusCode(ex, ignoreUnknownException);
            if (!status.HasValue) return null;
            return new JsonResult(result)
            {
                StatusCode = status.Value
            };
        }

        /// <summary>
        /// Converts an exception to action result with exception message.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="ex">The exception.</param>
        /// <returns>The action result.</returns>
        public static ActionResult ExceptionResult(this ControllerBase controller, int status, Exception ex)
        {
            if (ex == null) return controller.StatusCode(status);
            var result = new ErrorMessageResult(ex);
            return new JsonResult(result)
            {
                StatusCode = status
            };
        }

        /// <summary>
        /// Converts an exception to action result with exception message.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="ex">The exception message.</param>
        /// <param name="errorCode">The optional error code.</param>
        /// <returns>The action result.</returns>
        #pragma warning disable IDE0060
        public static ActionResult ExceptionResult(this ControllerBase controller, int status, string ex, string errorCode = null)
        #pragma warning restore IDE0060
        {
            var result = new ErrorMessageResult(ex, errorCode);
            return new JsonResult(result)
            {
                StatusCode = status
            };
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
                NameQuery = GetFirstStringValue(request, "q"),
                NameExactly = GetFirstStringValue(request, "eq_name")?.ToLowerInvariant() == JsonBoolean.TrueString,
                Count = TryGetInt32Value(request, "count") ?? ResourceEntityExtensions.PageSize,
                Offset = TryGetInt32Value(request, "offset") ?? 0,
            };
            var state = TryGetInt32Value(request, "state");
            if (state.HasValue) q.State = (ResourceEntityStates)state.Value;
            return q;
        }

        /// <summary>
        /// Gets the query data instance.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The query data instance.</returns>
        public static QueryData GetQueryData(this IQueryCollection request)
        {
            if (request == null) return null;
            var q = new QueryData();
            foreach (var item in request)
            {
                q.Add(item.Key, item.Value as IEnumerable<string>);
            }
            
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
