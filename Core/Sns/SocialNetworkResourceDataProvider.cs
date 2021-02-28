using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
    public interface ISocialNetworkResourceDataProvider
    {
        /// <summary>
        /// Lists contacts.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<ContactEntity>> ListContactsAsync(string user, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogEntity>> ListBlogsAsync(QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific blog.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific blog comment.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<BlogCommentEntity> GetBlogCommentAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, bool plain, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="commentId">The identifier of the parent comment.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogCommentEntity>> ListBlogChildCommentsAsync(string commentId, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific user activity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<UserActivityEntity> GetUserActivityAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific user group activity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<UserGroupActivityEntity> GetUserGroupActivityAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific mail received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<ReceivedMailEntity> GetReceivedMailAsync(string user, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific mail sent or draft.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<SentMailEntity> GetSentMailAsync(string user, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="filter">The filter information.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string user, string folder, QueryArgs q, MailAdditionalFilterInfo filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="thread">The mail thread identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailThreadAsync(string user, string thread, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string user, string folder, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(ContactEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(BlogEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangeMethods> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The resource context of social network.
    /// </summary>
    public class OnPremisesSocialNetworkResourceContext : BaseSocialNetworkResourceContext
    {
        private const string LoginErrorTips = "Requires login firstly.";
        private const string EntityNullTips = "The entity was null.";

        /// <summary>
        /// Initializes a new instance of the OnPremisesSocialNetworkResourceContext class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="snsDataProvider">The social network resource data provider.</param>
        internal OnPremisesSocialNetworkResourceContext(BaseResourceAccessClient client, ISocialNetworkResourceDataProvider snsDataProvider)
            : base(client ?? new OnPremisesResourceAccessClient(null))
        {
            CoreResources = base.CoreResources as OnPremisesResourceAccessClient;
            DataProvider = snsDataProvider;
        }

        /// <summary>
        /// Initializes a new instance of the OnPremisesSocialNetworkResourceContext class.
        /// </summary>
        /// <param name="dataProvider">The account data provider.</param>
        /// <param name="snsDataProvider">The social network resource data provider.</param>
        public OnPremisesSocialNetworkResourceContext(IAccountDataProvider dataProvider, ISocialNetworkResourceDataProvider snsDataProvider)
            : base(new OnPremisesResourceAccessClient(dataProvider))
        {
            CoreResources = base.CoreResources as OnPremisesResourceAccessClient;
            DataProvider = snsDataProvider;
        }

        /// <summary>
        /// Initializes a new instance of the OnPremisesSocialNetworkResourceContext class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        /// <param name="snsDataProvider">The social network resource data provider.</param>
        public OnPremisesSocialNetworkResourceContext(OnPremisesResourceAccessClient client, ISocialNetworkResourceDataProvider snsDataProvider)
            : base(client ?? new OnPremisesResourceAccessClient(null))
        {
            CoreResources = base.CoreResources as OnPremisesResourceAccessClient;
            DataProvider = snsDataProvider;
        }

        /// <summary>
        /// Gets the resources access client.
        /// </summary>
        public new OnPremisesResourceAccessClient CoreResources { get; }

        /// <summary>
        /// Gets the social network resource data provider.
        /// </summary>
        public ISocialNetworkResourceDataProvider DataProvider { get; }

        /// <summary>
        /// Lists contacts.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<ContactEntity>> ListContactsAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.ListContactsAsync(userId, q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override async Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken = default)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            var c = await DataProvider.GetContactAsync(id, cancellationToken);
            if (c.OwnerId != id) return null;
            return c;
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="currentUser">true if searches blogs of the current user; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<BlogEntity>> ListBlogsAsync(bool currentUser, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await DataProvider.ListBlogsAsync(q, cancellationToken);
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await DataProvider.ListBlogsAsync(ownerType, owner, q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific blog.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override async Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken = default)
        {
            return await DataProvider.GetBlogAsync(id, cancellationToken);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, bool plain, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await DataProvider.ListBlogCommentsAsync(blog, plain, q, cancellationToken);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="commentId">The identifier of the parent comment.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<BlogCommentEntity>> ListBlogChildCommentsAsync(string commentId, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await DataProvider.ListBlogChildCommentsAsync(commentId, q, cancellationToken);
        }

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await DataProvider.ListUserActivitiesAsync(owner, q, cancellationToken);
        }

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return await DataProvider.ListUserGroupActivitiesAsync(owner, q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail received.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override async Task<ReceivedMailEntity> GetReceivedMailAsync(string id, CancellationToken cancellationToken = default)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.GetReceivedMailAsync(userId, id, cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail sent or draft.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override async Task<SentMailEntity> GetSentMailAsync(string id, CancellationToken cancellationToken = default)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.GetSentMailAsync(userId, id, cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="filter">The filter information.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, MailAdditionalFilterInfo filter, CancellationToken cancellationToken = default)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.ListReceivedMailAsync(userId, folder, q, filter, cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="thread">The mail thread identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailThreadAsync(string thread, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(thread)) throw new ArgumentNullException(nameof(thread), "thread should not be null or empty.");
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.ListReceivedMailThreadAsync(userId, thread, q, cancellationToken);
        }

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.ListSentMailAsync(userId, folder, q, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<ContactEntity>(result, entity, result.ToString() + " contact entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<BlogEntity>(result, entity, result.ToString() + " blog entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            if (string.IsNullOrWhiteSpace(entity.PublisherId) && entity.IsNew) entity.PublisherId = UserId;
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<BlogCommentEntity>(result, entity, result.ToString() + " blog comment entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<UserActivityEntity>(result, entity, result.ToString() + " user activity entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<UserGroupActivityEntity>(result, entity, result.ToString() + " user group activity entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<ReceivedMailEntity>(result, entity, result.ToString() + " received mail entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            if (entity.IsNew)
            {
                var mailList = entity.ToReceiveMails().ToList();
                _ = Task.Run(async () =>
                {
                    foreach (var m in mailList)
                    {
                        await DataProvider.SaveAsync(m, cancellationToken);
                    }
                }, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<SentMailEntity>(result, entity, result.ToString() + " sent mail entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> UpdateBlogCommentAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            var entity = await DataProvider.GetBlogCommentAsync(id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The entity does not exist.");
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<BlogCommentEntity>(result, entity, result.ToString() + " blog comment entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> UpdateUserActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            var entity = await DataProvider.GetUserActivityAsync(id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The entity does not exist.");
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<UserActivityEntity>(result, entity, result.ToString() + " user activity entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> UpdateUserGroupActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            var entity = await DataProvider.GetUserGroupActivityAsync(id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The entity does not exist.");
            try
            {
                var result = await DataProvider.SaveAsync(entity, cancellationToken);
                if (ResourceEntityExtensions.IsSuccessful(result))
                    return new ChangingResultInfo<UserGroupActivityEntity>(result, entity, result.ToString() + " user group activity entity.");
                return result;
            }
            catch (Exception ex)
            {
                var err = ResourceEntityExtensions.TryCatch(ex);
                if (err != null) return err;
                throw;
            }
        }
    }
}