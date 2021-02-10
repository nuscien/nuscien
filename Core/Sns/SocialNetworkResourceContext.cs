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
    /// The resource context of social network.
    /// </summary>
    public abstract class BaseSocialNetworkResourceContext
    {
        /// <summary>
        /// Initializes a new instance of the BaseSocialNetworkResourceContext class.
        /// </summary>
        protected BaseSocialNetworkResourceContext()
        {
            CoreResources = ResourceAccessClients.MemoryInstance;
        }

        /// <summary>
        /// Initializes a new instance of the BaseSocialNetworkResourceContext class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        public BaseSocialNetworkResourceContext(BaseResourceAccessClient client)
        {
            if (client != null)
                CoreResources = client;
            else
                _ = FillDefaultCoreResources();
        }

        /// <summary>
        /// Gets the resources access client.
        /// </summary>
        public BaseResourceAccessClient CoreResources { get; private set; }

        /// <summary>
        /// Gets the current user information.
        /// </summary>
        protected Users.UserEntity User => CoreResources?.User;

        /// <summary>
        /// Gets the current user identifier.
        /// </summary>
        protected string UserId => CoreResources?.User?.Id;

        /// <summary>
        /// Gets the current client identifier.
        /// </summary>
        protected string ClientId => CoreResources?.ClientId;

        /// <summary>
        /// Gets a value indicating whether the access token is null, empty or consists only of white-space characters.
        /// </summary>
        protected bool IsTokenNullOrEmpty => CoreResources?.IsTokenNullOrEmpty == true;

        /// <summary>
        /// Lists contacts.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<ContactEntity>> ListContactsAsync(QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public abstract Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="currentUser">true if searches blogs of the current user; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<BlogEntity>> ListBlogsAsync(bool currentUser, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="owner">The owner entity.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogEntity>> ListBlogsAsync(BaseSecurityEntity owner, QueryArgs q, CancellationToken cancellationToken = default)
            => owner == null ? Task.FromResult<IEnumerable<BlogEntity>>(new List<BlogEntity>().AsReadOnly()) : ListBlogsAsync(owner.SecurityEntityType, owner.Id, q, cancellationToken);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific blogs.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public abstract Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, bool plain, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, bool plain, CancellationToken cancellationToken = default)
            => ListBlogCommentsAsync(blog, plain, null, cancellationToken);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="commentId">The identifier of the parent comment.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<BlogCommentEntity>> ListBlogChildCommentsAsync(string commentId, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(string owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var user = await CoreResources.GetUserByIdAsync(owner, cancellationToken);
            return await ListUserActivitiesAsync(user, q, cancellationToken);
        }

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public async Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(string owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var group = await CoreResources.GetUserGroupByIdAsync(owner, cancellationToken);
            return await ListUserGroupActivitiesAsync(group, q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail received.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public abstract Task<ReceivedMailEntity> GetReceivedMailAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific mail sent or draft.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public abstract Task<SentMailEntity> GetSentMailAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public virtual Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, CancellationToken cancellationToken = default)
            => ListReceivedMailAsync(folder, null, null, cancellationToken);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public virtual Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken = default)
            => ListReceivedMailAsync(folder, q, null, cancellationToken);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="all">true if search for all folders; otherwise, false.</param>
        /// <param name="filter">The filter information.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public virtual Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(bool all, QueryArgs q, MailAdditionalFilterInfo filter, CancellationToken cancellationToken = default)
            => ListReceivedMailAsync(all ? "*" : string.Empty, q, null, cancellationToken);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="filter">The filter information.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, MailAdditionalFilterInfo filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="thread">The mail thread identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailThreadAsync(string thread, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public virtual Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, CancellationToken cancellationToken = default)
            => ListSentMailAsync(folder, null, cancellationToken);

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateContactAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetContactAsync(id, cancellationToken);
            if (delta == null || delta.Count == 0) return new ChangingResultInfo<ContactEntity>(ChangeMethods.Unchanged, entity, "Nothing request to update.");
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.SetProperties(delta);
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateContactAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetContactAsync(id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.State = state;
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateBlogAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetBlogAsync(id, cancellationToken);
            if (delta == null || delta.Count == 0) return new ChangingResultInfo<BlogEntity>(ChangeMethods.Unchanged, entity, "Nothing request to update.");
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.SetProperties(delta);
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateBlogAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetBlogAsync(id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.State = state;
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateBlogCommentAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateUserActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateUserGroupActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateReceivedMailAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetReceivedMailAsync(id, cancellationToken);
            if (delta == null || delta.Count == 0) return new ChangingResultInfo<ReceivedMailEntity>(ChangeMethods.Unchanged, entity, "Nothing request to update.");
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.SetProperties(delta);
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateReceivedMailAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetReceivedMailAsync(id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.State = state;
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateSentMailAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetSentMailAsync(id, cancellationToken);
            if (delta == null || delta.Count == 0) return new ChangingResultInfo<SentMailEntity>(ChangeMethods.Unchanged, entity, "Nothing request to update.");
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.SetProperties(delta);
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public virtual async Task<ChangingResultInfo> UpdateSentMailAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return new ChangingResultInfo(ChangeErrorKinds.Argument, "Requires an entity identifier.");
            var entity = await GetSentMailAsync(id, cancellationToken);
            if (entity == null) return new ChangingResultInfo(ChangeErrorKinds.NotFound, "The resource does not exist.");
            entity.State = state;
            return await SaveAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="value">The permission item to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasPermissionAsync(string siteId, string value)
        {
            if (CoreResources == null) return false;
            return await CoreResources.HasPermissionAsync(siteId, value);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="value">The permission item to test.</param>
        /// <param name="otherValues">Other permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasAnyPermissionAsync(string siteId, string value, params string[] otherValues)
        {
            if (CoreResources == null) return false;
            return await CoreResources.HasAnyPermissionAsync(siteId, value, otherValues);
        }

        /// <summary>
        /// Tests if contains any of the specific permission item.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="values">The permission items to test.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        protected async Task<bool> HasAnyPermissionAsync(string siteId, IEnumerable<string> values)
        {
            if (CoreResources == null) return false;
            return await CoreResources.HasAnyPermissionAsync(siteId, values);
        }

        /// <summary>
        /// Fills the resource access client by default instance.
        /// </summary>
        /// <returns>The asynchronous task instance.</returns>
        private async Task FillDefaultCoreResources()
        {
            if (CoreResources == null) CoreResources = ResourceAccessClients.MemoryInstance;
            var core = await ResourceAccessClients.CreateAsync();
            if (CoreResources == null || CoreResources == ResourceAccessClients.MemoryInstance) CoreResources = core;
        }
    }

    /// <summary>
    /// The resource context of social network.
    /// </summary>
    public class HttpSocialNetworkResourceContext : BaseSocialNetworkResourceContext
    {
        private const string LoginErrorTips = "Requires login firstly.";
        private const string EntityNullTips = "The entity was null.";
        private const string EmptyIdTips = "Requires an entity identifer.";

        /// <summary>
        /// The relative path.
        /// </summary>
        private string folder = "nuscien5/";

        /// <summary>
        /// Initializes a new instance of the HttpSocialNetworkResourceContext class.
        /// </summary>
        /// <param name="client">The resource access client.</param>
        public HttpSocialNetworkResourceContext(HttpResourceAccessClient client)
            : base(client ?? new HttpResourceAccessClient(null, null))
        {
            CoreResources = base.CoreResources as HttpResourceAccessClient;
        }

        /// <summary>
        /// Initializes a new instance of the HttpSocialNetworkResourceContext class.
        /// </summary>
        /// <param name="appKey">The app secret key for accessing API.</param>
        /// <param name="host">The host URI.</param>
        public HttpSocialNetworkResourceContext(AppAccessingKey appKey, Uri host)
            : base(new HttpResourceAccessClient(appKey, host))
        {
            CoreResources = base.CoreResources as HttpResourceAccessClient;
        }

        /// <summary>
        /// Gets the resources access client.
        /// </summary>
        public new HttpResourceAccessClient CoreResources { get; }

        /// <summary>
        /// Gets or sets the relative root path.
        /// </summary>
        public string RootFolderName
        {
            get
            {
                return folder;
            }

            set
            {
                if (value == null) folder = string.Empty;
                if (value.StartsWith("/")) value = value[1..];
                if (!value.EndsWith("/")) value += "/";
                folder = value;
            }
        }

        /// <summary>
        /// Lists contacts.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<ContactEntity>> ListContactsAsync(QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return QueryAsync<ContactEntity>(RootFolderName + "passport/contact", q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return GetDataAsync<ContactEntity>(HttpMethod.Get, $"{RootFolderName}passport/contact/{id}", cancellationToken);
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="currentUser">true if searches blogs of the current user; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<BlogEntity>> ListBlogsAsync(bool currentUser, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (!currentUser) return QueryAsync<BlogEntity>(RootFolderName + "sns/blog/all", q, cancellationToken);
            var userId = UserId;
            if (string.IsNullOrWhiteSpace(userId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return ListBlogsAsync(SecurityEntityTypes.User, userId, q, cancellationToken);
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return ownerType switch
            {
                SecurityEntityTypes.User => QueryAsync<BlogEntity>($"{RootFolderName}sns/blog/u/{owner}", q, cancellationToken),
                SecurityEntityTypes.UserGroup => QueryAsync<BlogEntity>($"{RootFolderName}sns/blog/g/{owner}", q, cancellationToken),
                _ => QueryAsync<BlogEntity>()
            };
        }

        /// <summary>
        /// Gets a specific blog.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetDataAsync<BlogEntity>(HttpMethod.Get, $"{RootFolderName}sns/blog/c/{id}", cancellationToken);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="plain">true if returns from all in plain mode; otherwise, false.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, bool plain, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var query = q != null ? (QueryData)q : new QueryData();
            if (plain) query.Set("plain", JsonBoolean.TrueString);
            return QueryAsync<BlogCommentEntity>($"{RootFolderName}sns/blog/c/{blog}/comments", q, cancellationToken);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="commentId">The identifier of the parent comment.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<BlogCommentEntity>> ListBlogChildCommentsAsync(string commentId, QueryArgs q, CancellationToken cancellationToken = default)
        {
            return QueryAsync<BlogCommentEntity>($"{RootFolderName}sns/blog/cc/{commentId}/children", q, cancellationToken);
        }

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (owner == null) return QueryAsync<UserActivityEntity>();
            return QueryAsync<UserActivityEntity>($"{RootFolderName}sns/activity/u/{owner.Id}", q, cancellationToken);
        }

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (owner == null) return QueryAsync<UserGroupActivityEntity>();
            return QueryAsync<UserGroupActivityEntity>($"{RootFolderName}sns/activity/g/{owner.Id}", q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail received.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override Task<ReceivedMailEntity> GetReceivedMailAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return GetDataAsync<ReceivedMailEntity>(HttpMethod.Get, $"{RootFolderName}mail/m/{id}", cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail sent or draft.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override Task<SentMailEntity> GetSentMailAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return GetDataAsync<SentMailEntity>(HttpMethod.Get, $"{RootFolderName}mail/e/{id}", cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="filter">The filter information.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, MailAdditionalFilterInfo filter, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return filter != null
                ? QueryAsync<ReceivedMailEntity>($"{RootFolderName}mail/i/{folder ?? "_"}", filter.ToQueryData(q), cancellationToken)
                : QueryAsync<ReceivedMailEntity>($"{RootFolderName}mail/i/{folder ?? "_"}", q, cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="thread">The mail thread identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailThreadAsync(string thread, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) throw new UnauthorizedAccessException(LoginErrorTips);
            var query = q != null ? (QueryData)q : new QueryData();
            query.Set("thread", thread);
            return QueryAsync<ReceivedMailEntity>($"{RootFolderName}mail/i/*", q, cancellationToken);
        }

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) throw new UnauthorizedAccessException(LoginErrorTips);
            return QueryAsync<SentMailEntity>($"{RootFolderName}mail/o/{folder ?? "_"}", q, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}passport/contact", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/c", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            if (string.IsNullOrWhiteSpace(entity.PublisherId) && entity.IsNew) entity.PublisherId = UserId;
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/cc", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activity/u", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activity/g", null, entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/m", null, entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="entity">The resource entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EntityNullTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            if (string.IsNullOrWhiteSpace(entity.OwnerId) && entity.IsNew) entity.OwnerId = UserId;
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/e", null, entity, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateContactAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}passport/contact/{id}", null, delta, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateContactAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EmptyIdTips));
            return UpdateContactAsync(id, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateBlogAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/c/{id}", null, delta, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateBlogAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EmptyIdTips));
            return UpdateBlogAsync(id, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateBlogCommentAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EmptyIdTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/cc/{id}", null, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateUserActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EmptyIdTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activit/u/{id}", null, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateUserGroupActivityAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EmptyIdTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activit/g/{id}", null, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateReceivedMailAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/m/{id}", null, delta, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateReceivedMailAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EmptyIdTips));
            return UpdateReceivedMailAsync(id, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="delta">The changeset.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateSentMailAsync(string id, JsonObject delta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/e/{id}", null, delta, cancellationToken);
        }

        /// <summary>
        /// Updates a specific entity.
        /// </summary>
        /// <param name="id">The resource entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateSentMailAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Argument, EmptyIdTips));
            if (string.IsNullOrWhiteSpace(UserId)) return Task.FromResult(new ChangingResultInfo(ChangeErrorKinds.Unauthorized, LoginErrorTips));
            return UpdateSentMailAsync(id, new JsonObject
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Combines path to root to generate a URI.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <param name="query">The optional query data.</param>
        /// <returns>A URI.</returns>
        public Uri GetUri(string path, QueryData query = null) => CoreResources.GetUri(path, query);

        /// <summary>
        /// Creates a JSON HTTP client.
        /// </summary>
        /// <typeparam name="TResult">The type of response.</typeparam>
        /// <param name="callback">An optional callback raised on data received.</param>
        /// <returns>A new JSON HTTP client.</returns>
        public virtual JsonHttpClient<TResult> CreateHttp<TResult>(Action<ReceivedEventArgs<TResult>> callback = null) => CoreResources.Create(callback);

        /// <summary>
        /// Gets data via network.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="method">HTTP method.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The result.</returns>
        public Task<TResult> GetDataAsync<TResult>(HttpMethod method, string relativePath, CancellationToken cancellationToken = default)
        {
            return CreateHttp<TResult>().SendJsonAsync(method, GetUri(relativePath), null, cancellationToken);
        }

        /// <summary>
        /// Gets data via network.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="method">HTTP method.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="q">The query data.</param>
        /// <param name="content">The body.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The result.</returns>
        public Task<TResult> GetDataAsync<TResult>(HttpMethod method, string relativePath, QueryData q, object content, CancellationToken cancellationToken = default)
        {
            return CreateHttp<TResult>().SendJsonAsync(method, GetUri(relativePath, q), content, cancellationToken);
        }

        /// <summary>
        /// Gets data via network.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="method">HTTP method.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="q">The query data.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The result.</returns>
        public Task<TResult> GetDataAsync<TResult>(HttpMethod method, string relativePath, QueryData q, CancellationToken cancellationToken = default)
        {
            return CreateHttp<TResult>().SendJsonAsync(method, GetUri(relativePath, q), null, cancellationToken);
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(string path, QueryData q, CancellationToken cancellationToken = default)
        {
            var url = GetUri(path, q);
            var result = await CreateHttp<CollectionResult<T>>().SendJsonAsync(HttpMethod.Get, url, null, cancellationToken);
            return result?.Value;
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(string path, QueryArgs q, CancellationToken cancellationToken = default)
        {
            var url = GetUri(path, (QueryData)q);
            var result = await CreateHttp<CollectionResult<T>>().SendJsonAsync(HttpMethod.Get, url, null, cancellationToken);
            return result?.Value;
        }

        private static Task<IEnumerable<T>> QueryAsync<T>()
        {
            var list = new List<T>();
            IEnumerable<T> col = list.AsReadOnly();
            return Task.FromResult(col);
        }
    }
}