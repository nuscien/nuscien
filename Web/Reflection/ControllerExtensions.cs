using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static JsonResult ToActionResult(this ChangeMethods value)
        {
            return new JsonResult(new ChangeMethodResult(value));
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
            if (value != null) return new JsonResult(value);
            return controller.NotFound();
        }

        /// <summary>
        /// Gets the query arguments instance.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The query arguments instance.</returns>
        public static QueryArgs GetQueryArgs(this IQueryCollection request)
        {
            var q = new QueryArgs
            {
                NameQuery = GetFirstStringValue(request, "q"),
                NameExactly = GetFirstStringValue(request, "eq_name")?.ToLowerInvariant() == JsonBoolean.TrueString,
                Count = TryGetInt32Value(request, "count") ?? 100,
                Offset = TryGetInt32Value(request, "offset") ?? 0,
            };
            var state = TryGetInt32Value(request, "state");
            if (state.HasValue) q.State = (ResourceEntityStates)state.Value;
            return q;
        }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns>A resource access client.</returns>
        public static async Task<BaseResourceAccessClient> GetResourceAccessClientAsync(this ControllerBase controller)
        {
            var r = await GetResourceAccessClientAsync(controller.Request);
            if (controller.Request.Cookies.TryGetValue("ns_t", out var cookie) && !string.IsNullOrWhiteSpace(cookie))
            {
                try
                {
                    var data = QueryData.Parse(cookie);
                    var bearerTokenString = data["t"]?.Trim();
                    var token = r.Token?.AccessToken;
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
            }

            return r;
        }

        /// <summary>
        /// Gets the resource access client.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A resource access client.</returns>
        internal static async Task<BaseResourceAccessClient> GetResourceAccessClientAsync(HttpRequest request)
        {
            var client = await ResourceAccessClients.CreateAsync();
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
    }
}
