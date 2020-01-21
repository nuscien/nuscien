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

using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The extensions for resource entity.
    /// </summary>
    public static class ResourceEntityExtensions
    {
        /// <summary>
        /// The normal state code.
        /// </summary>
        public static readonly int NormalStateCode = (int)ResourceEntityStates.Normal;

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
    }
}
