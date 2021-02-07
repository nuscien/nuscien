using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Collection;
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
        public Task<IEnumerable<ContactEntity>> ListContactsAsync(string user, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogEntity>> ListBlogsAsync(QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific blogs.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific mail received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<ReceivedMailEntity> GetReceivedMailAsync(string user, string id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific mail sent or draft.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public Task<SentMailEntity> GetSentMailAsync(string user, string id, CancellationToken cancellationToken);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string user, string folder, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="user">The user identifier.</param>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string user, string folder, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> UpdateBlogCommentAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> UpdateUserActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public Task<ChangingResultInfo> UpdateUserGroupActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);
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
        public override async Task<IEnumerable<ContactEntity>> ListContactsAsync(QueryArgs q, CancellationToken cancellationToken)
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
        public override async Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken)
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
        public override async Task<IEnumerable<BlogEntity>> ListBlogsAsync(bool currentUser, QueryArgs q, CancellationToken cancellationToken)
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
        public override async Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken)
        {
            return await DataProvider.ListBlogsAsync(ownerType, owner, q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific blogs.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override async Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken)
        {
            return await DataProvider.GetBlogAsync(id, cancellationToken);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, QueryArgs q, CancellationToken cancellationToken)
        {
            return await DataProvider.ListBlogCommentsAsync(blog, q, cancellationToken);
        }

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken)
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
        public override async Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken)
        {
            return await DataProvider.ListUserGroupActivitiesAsync(owner, q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail received.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override async Task<ReceivedMailEntity> GetReceivedMailAsync(string id, CancellationToken cancellationToken)
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
        public override async Task<SentMailEntity> GetSentMailAsync(string id, CancellationToken cancellationToken)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.GetSentMailAsync(userId, id, cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken)
        {
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return await DataProvider.ListReceivedMailAsync(userId, folder, q, cancellationToken);
        }

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override async Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken)
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
        public override async Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken)
        {
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips);
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken)
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

            return await DataProvider.SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> UpdateBlogCommentAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.UpdateBlogCommentAsync(id, state, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> UpdateUserActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.UpdateUserActivityAsync(id, state, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override async Task<ChangingResultInfo> UpdateUserGroupActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            if (string.IsNullOrWhiteSpace(UserId)) return new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips);
            return await DataProvider.UpdateUserGroupActivityAsync(id, state, cancellationToken);
        }
    }
}