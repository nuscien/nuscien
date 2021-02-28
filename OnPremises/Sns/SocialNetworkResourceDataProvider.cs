using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using NuScien.Data;
using NuScien.Reflection;
using NuScien.Security;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Sns
{
    /// <summary>
    /// The resource data provider of social network.
    /// </summary>
    public class SocialNetworkResourceDataProvider : ISocialNetworkResourceDataProvider
    {
        private readonly Func<bool, ISocialNetworkDbContext> contextFactory;

        /// <summary>
        /// Initializes a new instance of the SocialNetworkResourceDataProvider class.
        /// </summary>
        /// <param name="context">The database context with full-access.</param>
        /// <param name="readonlyContext">The optional database context readonly.</param>
        protected SocialNetworkResourceDataProvider(ISocialNetworkDbContext context, ISocialNetworkDbContext readonlyContext = null)
        {
            if (readonlyContext == null) readonlyContext = context;
            contextFactory = isReadonly => isReadonly ? readonlyContext : context;
        }

        /// <summary>
        /// Initializes a new instance of the SocialNetworkResourceDataProvider class.
        /// </summary>
        /// <param name="contextFactory">The database context factory.</param>
        public SocialNetworkResourceDataProvider(Func<bool, ISocialNetworkDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        /// <summary>
        /// Lists contacts.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<ContactEntity>> ListContactsAsync(string user, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await GetContext(true).Contacts.ListEntities(q, ele => ele.OwnerId == user).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken = default)
        {
            var db = GetContext(true);
            return db.Contacts.GetByIdAsync(id, true, cancellationToken);
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<BlogEntity>> ListBlogsAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await GetContext(true).Blogs.ListEntities(q).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var t = (int)ownerType;
            return await GetContext(true).Blogs.ListEntities(q, ele => ele.OwnerId == owner && ele.OwnerTypeCode == t).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a specific blog.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetContext(true).Blogs.GetByIdAsync(id, true, cancellationToken);
        }

        /// <summary>
        /// Gets a specific blog comment.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<BlogCommentEntity> GetBlogCommentAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetContext(true).BlogComments.GetByIdAsync(id, true, cancellationToken);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, bool plain, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var context = GetContext(true);
            var col = context.BlogComments.Where(ele => ele.OwnerId == blog);
            if (!plain) col = col.Where(ele => ele.SourceMessageId == null || ele.SourceMessageId == string.Empty);
            return await col.ToListAsync(q, null, ResourceEntityOrders.Latest, cancellationToken);
        }

        /// <summary>
        /// Lists the child comments of a specific blog comment.
        /// </summary>
        /// <param name="commentId">The identifier of the parent comment.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<BlogCommentEntity>> ListBlogChildCommentsAsync(string commentId, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var context = GetContext(true);
            return await context.BlogComments.ToListAsync(q, ele => ele.SourceMessageId == commentId, ResourceEntityOrders.Latest, cancellationToken);
        }

        /// <summary>
        /// Gets a specific user activity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<UserActivityEntity> GetUserActivityAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetContext(true).UserActivities.GetByIdAsync(id, true, cancellationToken);
        }

        /// <summary>
        /// Gets a specific user group activity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<UserGroupActivityEntity> GetUserGroupActivityAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetContext(true).GroupActivities.GetByIdAsync(id, true, cancellationToken);
        }

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await GetContext(true).UserActivities.ListEntities(q, ele => ele.OwnerId == owner.Id).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await GetContext(true).GroupActivities.ListEntities(q, ele => ele.OwnerId == owner.Id).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<ReceivedMailEntity> GetReceivedMailAsync(string user, string id, CancellationToken cancellationToken = default)
        {
            var db = GetContext(true);
            return db.ReceivedMails.GetByIdAsync(id, true, cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail sent or draft.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<SentMailEntity> GetSentMailAsync(string user, string id, CancellationToken cancellationToken = default)
        {
            var db = GetContext(true);
            return db.SentMails.GetByIdAsync(id, true, cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="filter">The filter information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string user, string folder, QueryArgs q, MailAdditionalFilterInfo filter, CancellationToken cancellationToken = default)
        {
            var col = GetContext(true).ReceivedMails.Where(ele => ele.OwnerId == user);
            if (folder == "_" || string.IsNullOrWhiteSpace(folder)) col = col.Where(ele => ele.Folder == null || ele.Folder == string.Empty);
            else if (folder != "*" && folder?.ToLowerInvariant() != "_all") col = col.Where(ele => ele.Folder == folder);
            if (filter != null) col = filter.Where(col);
            return await col.ListEntities(q).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="thread">The mail thread identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailThreadAsync(string user, string thread, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await GetContext(true).ReceivedMails.ListEntities(q, ele => ele.OwnerId == user && (ele.ThreadId == thread || ele.Id == thread)).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string user, string folder, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await GetContext(true).SentMails.ListEntities(q, ele => ele.OwnerId == user && ele.Folder == folder).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(ContactEntity entity, CancellationToken cancellationToken = default)
        {
            var context = GetContext(false);
            return DbResourceEntityExtensions.SaveAsync(context.Contacts, context.SaveChangesAsync, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(BlogEntity entity, CancellationToken cancellationToken = default)
        {
            var context = GetContext(false);
            return DbResourceEntityExtensions.SaveAsync(context.Blogs, context.SaveChangesAsync, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken = default)
        {
            var context = GetContext(false);
            return DbResourceEntityExtensions.SaveAsync(context.BlogComments, context.SaveChangesAsync, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken = default)
        {
            var context = GetContext(false);
            return DbResourceEntityExtensions.SaveAsync(context.UserActivities, context.SaveChangesAsync, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken = default)
        {
            var context = GetContext(false);
            return DbResourceEntityExtensions.SaveAsync(context.GroupActivities, context.SaveChangesAsync, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken = default)
        {
            var context = GetContext(false);
            return DbResourceEntityExtensions.SaveAsync(context.ReceivedMails, context.SaveChangesAsync, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken = default)
        {
            var context = GetContext(false);
            return DbResourceEntityExtensions.SaveAsync(context.SentMails, context.SaveChangesAsync, entity, cancellationToken);
        }

        /// <summary>
        /// Gets database context.
        /// </summary>
        /// <param name="isReadonly">true if get the readonly instance; otherwise, false.</param>
        /// <returns>The database context.</returns>
        protected ISocialNetworkDbContext GetContext(bool isReadonly = false)
        {
            return contextFactory?.Invoke(isReadonly);
        }
    }
}
