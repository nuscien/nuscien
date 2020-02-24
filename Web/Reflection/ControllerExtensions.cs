using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuScien.Data;
using Trivial.Data;
using Trivial.Net;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Web.Controllers
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
    }
}
