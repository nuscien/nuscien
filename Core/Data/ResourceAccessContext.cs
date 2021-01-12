using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Security;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The resource accessing context by HTTP client.
    /// </summary>
    public class HttpResourceAccessContext
    {
        /// <summary>
        /// Initializes a new instance of the HttpResourceAccessContext class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        public HttpResourceAccessContext(HttpResourceAccessClient client)
        {
            CoreResources = client ?? new HttpResourceAccessClient(null, null);
            FillProperties();
        }

        /// <summary>
        /// Initializes a new instance of the HttpResourceAccessContext class.
        /// </summary>
        /// <param name="appKey">The app secret key for accessing API.</param>
        /// <param name="host">The host URI.</param>
        public HttpResourceAccessContext(AppAccessingKey appKey, Uri host)
            : this(new HttpResourceAccessClient(appKey, host))
        {
        }

        /// <summary>
        /// Adds or removes a handler raised on sending.
        /// </summary>
        public event EventHandler<SendingEventArgs> Sending
        {
            add => CoreResources.Sending += value;
            remove => CoreResources.Sending -= value;
        }


        /// <summary>
        /// Gets the resources access client.
        /// </summary>
        public HttpResourceAccessClient CoreResources { get; }

        /// <summary>
        /// Gets the host URI.
        /// </summary>
        public Uri Host => CoreResources.Host;

        /// <summary>
        /// Gets the app identifier.
        /// </summary>
        public string AppId => CoreResources.AppId;

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        public Users.UserEntity User => CoreResources.User;

        /// <summary>
        /// Gets the current client identifier.
        /// </summary>
        public string ClientId => CoreResources.ClientId;

        /// <summary>
        /// Gets a value indicating whether the access token is null, empty or consists only of white-space characters.
        /// </summary>
        public bool IsTokenNullOrEmpty => CoreResources.IsTokenNullOrEmpty;

        /// <summary>
        /// Creates a resource entity provider.
        /// </summary>
        /// <typeparam name="THandler">The type of the resource entity provider for which a set should be returned.</typeparam>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <returns>The resource entity provider</returns>
        protected THandler Provider<THandler, TEntity>() where THandler : HttpResourceEntityProvider<TEntity> where TEntity : BaseResourceEntity
        {
            var type = typeof(THandler);
            if (type.IsAbstract) return null;
            var c = type.GetConstructor(new Type[] { typeof(HttpResourceAccessClient) });
            if (c == null) return null;
            return c.Invoke(new object[] { CoreResources }) as THandler;
        }

        /// <summary>
        /// Fills properties automatically.
        /// </summary>
        /// <returns>The count of property filled.</returns>
        protected virtual int FillProperties()
        {
            var i = 0;
            if (CoreResources == null) return i;
            var properties = GetType().GetProperties();
            foreach (var prop in properties)
            {
                var type = prop.PropertyType;
                if (type.IsAbstract || !prop.CanWrite || !prop.CanRead) continue;
                var cType = type;
                while (cType != null && cType != typeof(HttpResourceEntityProvider<>)) cType = cType.BaseType;
                try
                {
                    if (cType == null || cType.GenericTypeArguments.Length < 1) continue;
                    var gta = type.GenericTypeArguments[0];
                    if (gta == null || !gta.IsSubclassOf(typeof(BaseResourceEntity)) || prop.GetValue(this) != null) continue;
                    var c = type.GetConstructor(new Type[] { typeof(HttpResourceAccessClient) });
                    if (c == null) continue;
                    var v = c.Invoke(new object[] { CoreResources });
                    prop.SetValue(this, v);
                    i++;
                }
                catch (NullReferenceException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (MemberAccessException)
                {
                }
                catch (System.Reflection.TargetException)
                {
                }
            }

            return i;
        }

        /// <summary>
        /// Creates a JSON HTTP client.
        /// </summary>
        /// <typeparam name="T">The type of response.</typeparam>
        /// <param name="callback">An optional callback raised on data received.</param>
        /// <returns>A new JSON HTTP client.</returns>
        public virtual JsonHttpClient<T> CreateHttp<T>(Action<ReceivedEventArgs<T>> callback = null) => CoreResources.Create(callback);

        /// <summary>
        /// Combines path to root to generate a URI.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <param name="query">The optional query data.</param>
        /// <returns>A URI.</returns>
        public Uri GetUri(string path, QueryData query = null) => CoreResources.GetUri(path, query);
    }
}