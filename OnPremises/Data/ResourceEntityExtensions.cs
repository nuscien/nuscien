using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Trivial.Data;
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
        public static async Task<T> GetByIdAsync<T>(this IQueryable<T> source, string id, bool includeAllStates = false, CancellationToken cancellationToken = default) where T : BaseResourceEntity
        {
            InternalAssertion.IsNotNull(source);
            InternalAssertion.IsNotNullOrWhiteSpace(id, nameof(id));
            var entity = await source.FirstOrDefaultAsync(ele => ele.Id == id, cancellationToken);
            if (entity is null) return null;
            return includeAllStates || entity.State == ResourceEntityStates.Normal ? entity : null;
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
            return ResourceEntityExtensions.SaveAsync(set.Add, set.Update, save, entity, cancellationToken);
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

        internal static Task<List<T>> ToListAsync<T>(this IQueryable<T> col, QueryArgs q, CancellationToken cancellationToken)
        {
            if (q != null)
            {
                if (q.Offset > 0) col = col.Skip(q.Offset);
                col = col.Take(q.Count);
            }
            else
            {
                col = col.Take(100);
            }

            return col.ToListAsync(cancellationToken);
        }
    }
}
