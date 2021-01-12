using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Security;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The extensions for resource entity.
    /// </summary>
    public static class ResourceEntityExtensions
    {
        private static int pageSize = 100;

        /// <summary>
        /// The normal state code.
        /// </summary>
        public static readonly int NormalStateCode = (int)ResourceEntityStates.Normal;

        /// <summary>
        /// Gets or sets the default page size.
        /// </summary>
        public static int PageSize
        {
            get => pageSize;
            set => pageSize = value > 0 ? value : 20;
        }

        /// <summary>
        /// Sets a query arguments into a query data instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="q">The query arguments.</param>
        /// <returns>The count of items changed.</returns>
        public static int AddRange(this Trivial.Net.QueryData source, QueryArgs q)
        {
            if (source == null || q == null) return 0;
            var count = 3;
            if (!string.IsNullOrEmpty(q.NameQuery))
            {
                source["q"] = q.NameQuery;
                count++;
            }

            source.Add("eq_name", q.NameExactly, true);
            if (q.Offset > 0)
            {
                source.Add("offset", q.Offset, true);
                count++;
            }

            source.Add("count", q.Count, true);
            source.Add("state", (int)q.State, true);
            return count;
        }

        /// <summary>
        /// Filters a sequence of normal resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate = null) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            if (predicate != null) source = source.Where(predicate);
            return source.Where(ele => ele.StateCode == NormalStateCode);
        }

        /// <summary>
        /// Filters a sequence of normal resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, Expression<Func<T, int, bool>> predicate) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            if (predicate != null) source = source.Where(predicate);
            return source.Where(ele => ele.StateCode == NormalStateCode);
        }

        /// <summary>
        /// Filters a sequence of resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="state">The state.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, ResourceEntityStates state, Expression<Func<T, bool>> predicate = null) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            if (predicate != null) source = source.Where(predicate);
            return source.Where(ele => ele.StateCode == (int)state);
        }

        /// <summary>
        /// Filters a sequence of resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="state">The state.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, ResourceEntityStates state, Expression<Func<T, int, bool>> predicate) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            if (predicate != null) source = source.Where(predicate);
            return source.Where(ele => ele.StateCode == (int)state);
        }

        /// <summary>
        /// Filters a sequence of resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="name">The name to search.</param>
        /// <param name="like">true if search by like; otherwise, false, to equals to.</param>
        /// <param name="state">The state.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, string name, bool like = false, ResourceEntityStates state = ResourceEntityStates.Normal) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            if (!string.IsNullOrEmpty(name))
                source = like ? source.Where(ele => ele.Name.Contains(name)) : source.Where(ele => ele.Name == name);
            return source.Where(ele => ele.StateCode == (int)state);
        }

        /// <summary>
        /// Filters a sequence of resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, QueryArgs q, Expression<Func<T, bool>> predicate = null) where T : BaseResourceEntity
        {
            if (q == null)
            {
                if (predicate == null) return source;
                InternalAssertion.IsNotNull(source);
                return source.Where(predicate);
            }

            InternalAssertion.IsNotNull(source);
            if (!string.IsNullOrEmpty(q.NameQuery))
                source = q.NameExactly ? source.Where(ele => ele.Name == q.NameQuery && ele.StateCode == (int)q.State) : source.Where(ele => ele.Name.Contains(q.NameQuery) && ele.StateCode == (int)q.State);
            if (predicate != null) source = source.Where(predicate);
            if (q.Offset > 0) source = source.Skip(q.Offset);
            return q.Count > 0 ? source.Take(q.Count) : source;
        }

        /// <summary>
        /// Filters a sequence of resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, QueryArgs q, Expression<Func<T, int, bool>> predicate) where T : BaseResourceEntity
        {
            if (q == null)
            {
                if (predicate == null) return source;
                InternalAssertion.IsNotNull(source);
                return source.Where(predicate);
            }

            InternalAssertion.IsNotNull(source);
            if (!string.IsNullOrEmpty(q.NameQuery))
            source = q.NameExactly ? source.Where(ele => ele.Name == q.NameQuery && ele.StateCode == (int)q.State) : source.Where(ele => ele.Name.Contains(q.NameQuery) && ele.StateCode == (int)q.State);
            if (predicate != null) source = source.Where(predicate);
            if (q.Offset > 0) source = source.Skip(q.Offset);
            return q.Count > 0 ? source.Take(q.Count) : source;
        }

        /// <summary>
        /// Filters a sequence of resource entities based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="filter">A WHERE handler.</param>
        /// <returns>A queryable resource entity collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        /// <exception cref="ArgumentNullException">source was null.</exception>
        public static IQueryable<T> ListEntities<T>(this IQueryable<T> source, QueryArgs q, Func<IQueryable<T>, IQueryable<T>> filter) where T : BaseResourceEntity
        {
            if (q == null)
            {
                if (filter == null) return source;
                InternalAssertion.IsNotNull(source);
                return filter(source);
            }

            InternalAssertion.IsNotNull(source);
            if (!string.IsNullOrEmpty(q.NameQuery))
                source = q.NameExactly ? source.Where(ele => ele.Name == q.NameQuery && ele.StateCode == (int)q.State) : source.Where(ele => ele.Name.Contains(q.NameQuery) && ele.StateCode == (int)q.State);
            if (filter != null) source = filter(source);
            if (q.Offset > 0) source = source.Skip(q.Offset);
            return q.Count > 0 ? source.Take(q.Count) : source;
        }

        /// <summary>
        /// Gets a resource entity by identifier.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <returns>A resource entity if exists; or null.</returns>
        /// <exception cref="ArgumentNullException">source or id was null.</exception>
        /// <exception cref="ArgumentException">id was empty or consists only of white-space characters.</exception>
        public static T GetById<T>(this IQueryable<T> source, string id, bool includeAllStates = false) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            InternalAssertion.IsNotNullOrWhiteSpace(id, nameof(id));
            var entity = source.FirstOrDefault(ele => ele.Id == id);
            if (entity is null) return null;
            return includeAllStates || entity.State == ResourceEntityStates.Normal ? entity : null;
        }

        /// <summary>
        /// Gets the owner resource entities from the relationship collection.
        /// </summary>
        /// <typeparam name="T">The type of owner resource.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>A collection of owner resource entity.</returns>
        public static IEnumerable<T> GetTargetResources<T>(IEnumerable<OwnerResourceEntity<T>> source)
            where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            return source.Select(ele => ele.Owner);
        }

        /// <summary>
        /// Gets the target resource entities from the relationship collection.
        /// </summary>
        /// <typeparam name="TOwner">The type of owner resource.</typeparam>
        /// <typeparam name="TTarget">The type of target resource.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>A collection of target resource entity.</returns>
        public static IEnumerable<TTarget> GetTargetResources<TOwner, TTarget>(IEnumerable<OwnerResourceEntity<TOwner, TTarget>> source)
            where TOwner : BaseResourceEntity
            where TTarget : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            return source.Select(ele => ele.Target);
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="add">The add action handler.</param>
        /// <param name="update">The update action handler.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public static async Task<ChangeMethods> SaveAsync<T>(Action<T> add, Action<T> update, T entity, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            if (entity is null) return ChangeMethods.Invalid;
            if (entity.IsNew)
            {
                cancellationToken.ThrowIfCancellationRequested();
                InternalAssertion.IsNotNull(add, nameof(add));
                return await Task.Run(() =>
                {
                    var isSucc = false;
                    entity.PrepareForSaving();
                    try
                    {
                        add(entity);
                        isSucc = true;
                        return ChangeMethods.Add;
                    }
                    finally
                    {
                        if (!isSucc) entity.RollbackSaving();
                    }
                });
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
                InternalAssertion.IsNotNull(update, nameof(update));
                return await Task.Run(() =>
                {
                    var isSucc = false;
                    entity.PrepareForSaving();
                    try
                    {
                        update(entity);
                        isSucc = true;
                        return entity.State switch
                        {
                            ResourceEntityStates.Deleted => ChangeMethods.Remove,
                            _ => ChangeMethods.MemberModify
                        };
                    }
                    finally
                    {
                        if (!isSucc) entity.RollbackSaving();
                    }
                });
            }
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <typeparam name="TResult">The type of action result.</typeparam>
        /// <param name="add">The add action handler.</param>
        /// <param name="update">The update action handler.</param>
        /// <param name="save">The save action handler.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public static async Task<ChangeMethods> SaveAsync<TEntity, TResult>(Func<TEntity, TResult> add, Func<TEntity, TResult> update, Func<CancellationToken, Task<int>> save, TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseResourceEntity
        {
            if (entity is null) return ChangeMethods.Invalid;
            if (entity.IsNew)
            {
                cancellationToken.ThrowIfCancellationRequested();
                InternalAssertion.IsNotNull(add, nameof(add));
                var isSucc = false;
                entity.PrepareForSaving();
                try
                {
                    add(entity);
                    await save(cancellationToken);
                    isSucc = true;
                    return ChangeMethods.Add;
                }
                finally
                {
                    if (!isSucc) entity.RollbackSaving();
                }
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
                InternalAssertion.IsNotNull(update, nameof(update));
                var isSucc = false;
                entity.PrepareForSaving();
                try
                {
                    update(entity);
                    await save(cancellationToken);
                    isSucc = true;
                    return entity.State switch
                    {
                        ResourceEntityStates.Deleted => ChangeMethods.Remove,
                        _ => ChangeMethods.MemberModify
                    };
                }
                finally
                {
                    if (!isSucc) entity.RollbackSaving();
                }
            }
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <typeparam name="TResult">The type of action result.</typeparam>
        /// <param name="add">The add action handler.</param>
        /// <param name="update">The update action handler.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public static async Task<ChangeMethods> SaveAsync<TEntity, TResult>(Func<TEntity, Task<TResult>> add, Func<TEntity, Task<TResult>> update, TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseResourceEntity
        {
            if (entity is null) return ChangeMethods.Invalid;
            if (entity.IsNew)
            {
                cancellationToken.ThrowIfCancellationRequested();
                InternalAssertion.IsNotNull(add, nameof(add));
                var isSucc = false;
                entity.PrepareForSaving();
                try
                {
                    await add(entity);
                    isSucc = true;
                    return ChangeMethods.Add;
                }
                finally
                {
                    if (!isSucc) entity.RollbackSaving();
                }
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
                InternalAssertion.IsNotNull(update, nameof(update));
                var isSucc = false;
                entity.PrepareForSaving();
                try
                {
                    await update(entity);
                    isSucc = true;
                    return entity.State switch
                    {
                        ResourceEntityStates.Deleted => ChangeMethods.Remove,
                        _ => ChangeMethods.MemberModify
                    };
                }
                finally
                {
                    if (!isSucc) entity.RollbackSaving();
                }
            }
        }

        /// <summary>
        /// Tries to get the third-party login provider.
        /// </summary>
        /// <param name="providers">The third-party login provider pool.</param>
        /// <param name="ldap">The LDAP host name or IP address.</param>
        /// <returns>The third-party login provider matched; or null if non-existed.</returns>
        public static IThirdPartyLoginProvider<PasswordTokenRequestBody> TryGetProvider(this Dictionary<string, IThirdPartyLoginProvider<PasswordTokenRequestBody>> providers, string ldap)
        {
            if (ldap == null) return null;
            ldap = ldap.Trim();
            if (string.IsNullOrEmpty(ldap)) return null;
            var startOffset = ldap.IndexOf("://");
            if (ldap.StartsWith("//")) startOffset = 2;
            else if (startOffset >= 0) startOffset += 3;
            var host = startOffset > 0 ? ldap[startOffset..] : ldap;
            if (host.EndsWith("/")) host = host[0..^1];
            return providers.TryGetValue(host, out var provider) ? provider : null;
        }

        /// <summary>
        /// Tries to get the third-party login provider.
        /// </summary>
        /// <param name="providers">The third-party login provider pool.</param>
        /// <param name="serviceProvider">The authorization code service provider name or URL.</param>
        /// <returns>The third-party login provider matched; or null if non-existed.</returns>
        public static IAuthorizationCodeVerifierProvider TryGetProvider(this Dictionary<string, IAuthorizationCodeVerifierProvider> providers, string serviceProvider)
        {
            if (serviceProvider == null) return null;
            serviceProvider = serviceProvider.Trim();
            if (string.IsNullOrEmpty(serviceProvider)) return null;
            return providers.TryGetValue(serviceProvider, out var provider) ? provider : null;
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="h">The resource entity handler.</param>
        /// <param name="name">The name to search.</param>
        /// <param name="like">true if search by like; otherwise, false, to equals to.</param>
        /// <param name="state">The state.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection.</returns>
        public static Task<CollectionResult<T>> SearchAsync<T>(this IResourceEntityProvider<T> h, string name, bool like = false, ResourceEntityStates state = ResourceEntityStates.Normal, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            if (h == null) return Task.FromResult<CollectionResult<T>>(null);
            return h.SearchAsync(new QueryArgs { NameQuery = name, NameExactly = !like, State = state }, cancellationToken);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="h">The resource entity handler.</param>
        /// <param name="name">The name to search.</param>
        /// <param name="like">true if search by like; otherwise, false, to equals to.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection.</returns>
        public static Task<CollectionResult<T>> SearchAsync<T>(this IResourceEntityProvider<T> h, string name, bool like, CancellationToken cancellationToken) where T : BaseResourceEntity
        {
            if (h == null) return Task.FromResult<CollectionResult<T>>(null);
            return h.SearchAsync(new QueryArgs { NameQuery = name, NameExactly = !like }, cancellationToken);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="h">The resource entity handler.</param>
        /// <param name="name">The name to search.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection.</returns>
        public static Task<CollectionResult<T>> SearchAsync<T>(this IResourceEntityProvider<T> h, string name, CancellationToken cancellationToken) where T : BaseResourceEntity
        {
            if (h == null) return Task.FromResult<CollectionResult<T>>(null);
            return h.SearchAsync(new QueryArgs { NameQuery = name }, cancellationToken);
        }

        /// <summary>
        /// Gets by a specific entity identifier.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="h">The resource entity handler.</param>
        /// <param name="id">The identifier of the entity to get.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An entity instance.</returns>
        public static Task<T> GetAsync<T>(this IResourceEntityProvider<T> h, string id, CancellationToken cancellationToken) where T : BaseResourceEntity
        {
            if (h == null) return Task.FromResult<T>(null);
            return h.GetAsync(id, false, cancellationToken);
        }
    }
}
