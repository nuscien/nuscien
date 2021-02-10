using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using NuScien.Collection;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The extensions for resource entity.
    /// </summary>
    public static class DbResourceEntityExtensions
    {
        /// <summary>
        /// Gets a resource entity by identifier.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="includeAllStates">true if includes all states but not only normal one; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A resource entity if exists; or null.</returns>
        /// <exception cref="ArgumentNullException">source or id was null.</exception>
        /// <exception cref="ArgumentException">id was empty or consists only of white-space characters.</exception>
        public static async Task<T> GetByIdAsync<T>(this IQueryable<T> source, string id, bool includeAllStates, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            InternalAssertion.IsNotNullOrWhiteSpace(id, nameof(id));
            var entity = await source.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
            if (entity is null) return null;
            return includeAllStates || entity.State == ResourceEntityStates.Normal ? entity : null;
        }

        /// <summary>
        /// Gets a resource entity by identifier.
        /// </summary>
        /// <typeparam name="T">The type of the resource entity.</typeparam>
        /// <param name="source">A queryable resource entity collection to filter.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A resource entity if exists; or null.</returns>
        /// <exception cref="ArgumentNullException">source or id was null.</exception>
        /// <exception cref="ArgumentException">id was empty or consists only of white-space characters.</exception>
        public static async Task<T> GetByIdAsync<T>(this IQueryable<T> source, string id, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            InternalAssertion.IsNotNullOrWhiteSpace(id, nameof(id));
            var entity = await source.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="set">The entity set.</param>
        /// <param name="save">The save action handler.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>An async task result.</returns>
        public static Task<ChangeMethods> SaveAsync<T>(DbSet<T> set, Func<CancellationToken, Task<int>> save, T entity, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(set, nameof(set));
            return ResourceEntityExtensions.SaveAsync(entity, set.Add, set.Update, save, cancellationToken);
        }

        /// <summary>
        /// Creates an instance of the database context options.
        /// </summary>
        /// <typeparam name="T">The type of database context.</typeparam>
        /// <param name="configureConnection">The context options builder maker.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>A database context options instance.</returns>
        public static DbContextOptions<T> CreateDbContextOptions<T>(Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> configureConnection, DbConnection connection)
             where T : DbContext
        {
            var b = new DbContextOptionsBuilder<T>();
            configureConnection(b, connection);
            return b.Options;
        }

        /// <summary>
        /// Creates an instance of the database context options.
        /// </summary>
        /// <typeparam name="T">The type of database context.</typeparam>
        /// <param name="configureConnection">The context options builder maker.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>A database context options instance.</returns>
        public static DbContextOptions<T> CreateDbContextOptions<T>(Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> configureConnection, string connection)
             where T : DbContext
        {
            var b = new DbContextOptionsBuilder<T>();
            configureConnection(b, connection);
            return b.Options;
        }

        /// <summary>
        /// Creates an instance of the database context options.
        /// </summary>
        /// <typeparam name="T">The type of database context.</typeparam>
        /// <param name="configureConnection">The context options builder maker.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="optionsAction">An action.</param>
        /// <returns>A database context options instance.</returns>
        public static DbContextOptions<T> CreateDbContextOptions<T>(Func<DbContextOptionsBuilder<T>, DbConnection, Action<DbContextOptionsBuilder<T>>, DbContextOptionsBuilder<T>> configureConnection, DbConnection connection, Action<DbContextOptionsBuilder<T>> optionsAction)
             where T : DbContext
        {
            var b = new DbContextOptionsBuilder<T>();
            configureConnection(b, connection, optionsAction);
            return b.Options;
        }

        /// <summary>
        /// Creates an instance of the database context options.
        /// </summary>
        /// <typeparam name="T">The type of database context.</typeparam>
        /// <param name="configureConnection">The context options builder maker.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="optionsAction">An action.</param>
        /// <returns>A database context options instance.</returns>
        public static DbContextOptions<T> CreateDbContextOptions<T>(Func<DbContextOptionsBuilder<T>, string, Action<DbContextOptionsBuilder<T>>, DbContextOptionsBuilder<T>> configureConnection, string connection, Action<DbContextOptionsBuilder<T>> optionsAction)
             where T : DbContext
        {
            var b = new DbContextOptionsBuilder<T>();
            configureConnection(b, connection, optionsAction);
            return b.Options;
        }

        /// <summary>
        /// Filters the entity collection.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <param name="col">The entity collection.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="isDesc">true if descending; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A list of entity.</returns>
        public static Task<List<T>> ToListAsync<T, TKey>(this IQueryable<T> col, QueryArgs q, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> keySelector, bool isDesc = false, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            if (col == null) return null;
            if (q != null)
            {
                if (!string.IsNullOrWhiteSpace(q.NameQuery)) col = q.NameExactly ? col.Where(ele => ele.Name == q.NameQuery) : col.Where(ele => ele.Name != null && ele.Name.Contains(q.NameQuery));
                col = col.Where(ele => ele.StateCode == (int)q.State);
                if (predicate != null) col = col.Where(predicate);
                if (keySelector == null)
                {
                    col = OrderBy(col, q.Order == ResourceEntityOrders.Default ? ResourceEntityOrders.Latest : q.Order);
                }
                else
                {
                    col = isDesc ? col.OrderByDescending(keySelector) : col.OrderBy(keySelector);
                }

                if (q.Offset > 0) col = col.Skip(q.Offset);
                col = col.Take(q.Count > 0 ? q.Count : ResourceEntityExtensions.PageSize);
            }
            else
            {
                if (predicate != null) col = col.Where(predicate);
                if (keySelector != null) col = isDesc ? col.OrderByDescending(keySelector) : col.OrderBy(keySelector);
                col = col.Take(ResourceEntityExtensions.PageSize);
            }

            return col.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Filters the entity collection.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="col">The entity collection.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="defaultOrder">The default order.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A list of entity.</returns>
        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> col, QueryArgs q, Expression<Func<T, bool>> predicate, ResourceEntityOrders defaultOrder, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            if (col == null) return null;
            if (q != null)
            {
                if (!string.IsNullOrWhiteSpace(q.NameQuery)) col = q.NameExactly ? col.Where(ele => ele.Name == q.NameQuery) : col.Where(ele => ele.Name != null && ele.Name.Contains(q.NameQuery));
                if (predicate != null) col = col.Where(predicate);
                col = OrderBy(col.Where(ele => ele.StateCode == (int)q.State), q.Order == ResourceEntityOrders.Default ? defaultOrder : q.Order);
                if (q.Offset > 0) col = col.Skip(q.Offset);
                col = col.Take(q.Count > 0 ? q.Count : ResourceEntityExtensions.PageSize);
            }
            else
            {
                if (predicate != null) col = col.Where(predicate);
                col = col.Take(ResourceEntityExtensions.PageSize);
            }

            return col.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Searches.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="mapQuery">The map query handler.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>A collection of entity.</returns>
        public static async Task<CollectionResult<T>> SearchAsync<T>(DbSet<T> set, QueryData q, Action<QueryPredication<T>> mapQuery, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            if (set == null) return null;
            if (q == null) return new CollectionResult<T>(await set.ListEntities(new QueryArgs()).ToListAsync(cancellationToken), 0);
            if (q.Count == 1 && q.ContainsKey("id"))
            {
                var ids = q.GetValues("id").Where(ele => !string.IsNullOrWhiteSpace(ele));
                var list = new List<T>();
                foreach (var id in ids)
                {
                    var entity = await set.GetByIdAsync(id, false, cancellationToken);
                    list.Add(entity);
                }

                var result = new CollectionResult<T>(list, 0, list.Count);
                return result;
            }

            QueryArgs args = q;
            var col = set.ListEntities(args, l =>
            {
                var info = new QueryPredication<T>(l, q);
                mapQuery?.Invoke(info);
                return info.Data;
            });
            if (col is null) return new CollectionResult<T>(null, args.Offset);
            return new CollectionResult<T>(await col.ToListAsync(cancellationToken), args.Offset);
        }

        internal static Task<List<T>> ToListAsync<T>(this IQueryable<T> col, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (q != null)
            {
                if (q.Offset > 0) col = col.Skip(q.Offset);
                col = col.Take(q.Count > 0 ? q.Count : ResourceEntityExtensions.PageSize);
            }
            else
            {
                col = col.Take(ResourceEntityExtensions.PageSize);
            }

            return col.ToListAsync(cancellationToken);
        }

        internal static Task<int> SaveChangesFailureAsync(CancellationToken cancellationToken)
        {
            throw new DbUpdateException("No implementation for save handler.", new NotImplementedException("Cannot find SaveChangesAsync method."));
        }

        internal static ChangingResultInfo TryCatch(Exception ex)
        {
            if (ex == null) return null;
            var isInvalid = false;
            if (ex.InnerException != null)
            {
                if (ex is AggregateException)
                {
                    ex = ex.InnerException;
                }
                else if (ex is InvalidOperationException)
                {
                    ex = ex.InnerException;
                    isInvalid = true;
                }
            }

            return isInvalid
                || ex is SecurityException
                || ex is UnauthorizedAccessException
                || ex is NotImplementedException
                || ex is TimeoutException
                || ex is OperationCanceledException
                || ex is InvalidOperationException
                || ex is ArgumentException
                || ex is NullReferenceException
                || ex is System.Data.Common.DbException
                || ex is System.Text.Json.JsonException
                || ex is System.Runtime.Serialization.SerializationException
                || ex is ObjectDisposedException
                || ex is Trivial.Net.FailedHttpException
                || ex is System.IO.IOException
                || ex is ApplicationException
                || ex is InvalidCastException
                || ex is FormatException
                || ex is System.IO.InvalidDataException
                ? new ChangingResultInfo(ex)
                : null;
        }

        private static IQueryable<T> OrderBy<T>(IQueryable<T> source, ResourceEntityOrders order) where T : BaseResourceEntity
        {
            return order switch
            {
                ResourceEntityOrders.Latest => source.OrderByDescending(ele => ele.LastModificationTime),
                ResourceEntityOrders.Time => source.OrderBy(ele => ele.LastModificationTime),
                ResourceEntityOrders.Name => source.OrderBy(ele => ele.Name),
                ResourceEntityOrders.Z2A => source.OrderByDescending(ele => ele.Name),
                _ => source
            };
        }
    }
}
