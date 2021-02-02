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
        public abstract Task<IEnumerable<ContactEntity>> ListContactsAsync(QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public abstract Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<BlogEntity>> ListBlogsAsync(QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="owner">The owner entity.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogEntity>> ListBlogsAsync(BaseSecurityEntity owner, QueryArgs q, CancellationToken cancellationToken)
            => owner == null ? Task.FromResult<IEnumerable<BlogEntity>>(new List<BlogEntity>().AsReadOnly()) : ListBlogsAsync(owner.SecurityEntityType, owner.Id, q, cancellationToken);

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific blogs.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public abstract Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, CancellationToken cancellationToken)
            => ListBlogCommentsAsync(blog, null, cancellationToken);

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public abstract Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateContactStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateBlogStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateBlogCommentStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateUserActivityStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateUserGroupActivityStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateReceivedMailStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public abstract Task<ChangingResultInfo> UpdateSentMailStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken);

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
        public override Task<IEnumerable<ContactEntity>> ListContactsAsync(QueryArgs q, CancellationToken cancellationToken)
        {
            return QueryAsync<ContactEntity>(RootFolderName + "passport/contact", q, cancellationToken);
        }

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken)
        {
            return GetDataAsync<ContactEntity>(HttpMethod.Get, $"{RootFolderName}passport/contact/{id}", cancellationToken);
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<BlogEntity>> ListBlogsAsync(QueryArgs q, CancellationToken cancellationToken)
        {
            return QueryAsync<BlogEntity>(RootFolderName + "sns/blog/c", q, cancellationToken);
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <param name="ownerType">The owner type.</param>
        /// <param name="owner">The owner identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken)
        {
            return ownerType switch
            {
                SecurityEntityTypes.User => QueryAsync<BlogEntity>($"{RootFolderName}sns/blog/u/{owner}", q, cancellationToken),
                SecurityEntityTypes.UserGroup => QueryAsync<BlogEntity>($"{RootFolderName}sns/blog/g/{owner}", q, cancellationToken),
                _ => QueryAsync<BlogEntity>()
            };
        }

        /// <summary>
        /// Gets a specific blogs.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The entity result.</returns>
        public override Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken)
        {
            return GetDataAsync<BlogEntity>(HttpMethod.Get, $"{RootFolderName}sns/blog/c/{id}", cancellationToken);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="blog">The blog identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<BlogCommentEntity>> ListBlogCommentsAsync(string blog, QueryArgs q, CancellationToken cancellationToken)
        {
            return QueryAsync<BlogCommentEntity>($"{RootFolderName}sns/blog/c/{blog}/comments", q, cancellationToken);
        }

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="owner">The user identifier.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken)
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
        public override Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken)
        {
            if (owner == null) return QueryAsync<UserGroupActivityEntity>();
            return QueryAsync<UserGroupActivityEntity>($"{RootFolderName}sns/activity/g/{owner.Id}", q, cancellationToken);
        }

        /// <summary>
        /// Lists mails received.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken)
        {
            return QueryAsync<ReceivedMailEntity>($"{RootFolderName}mail/i/{folder ?? string.Empty}", q, cancellationToken);
        }

        /// <summary>
        /// Lists mails sent or draft.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="q">The query arguments.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The collection of result.</returns>
        public override Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken)
        {
            return QueryAsync<SentMailEntity>($"{RootFolderName}mail/o/{folder ?? string.Empty}", q, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}passport/contact", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/c", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/cc", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activity/u", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activity/g", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/i", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/o", null, entity, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateContactStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}passport/contact/{id}", null, new QueryData
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateBlogStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/c/{id}", null, new QueryData
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateBlogCommentStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/blog/cc/{id}", null, new QueryData
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateUserActivityStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activit/u/{id}", null, new QueryData
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateUserGroupActivityStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}sns/activit/g/{id}", null, new QueryData
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateReceivedMailStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/i/{id}", null, new QueryData
            {
                { "state", state.ToString() }
            }, cancellationToken);
        }

        /// <summary>
        /// Saves an entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="state">The state to change.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns>The changing result information.</returns>
        public override Task<ChangingResultInfo> UpdateSentMailStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
        {
            return GetDataAsync<ChangingResultInfo>(HttpMethod.Put, $"{RootFolderName}mail/o/{id}", null, new QueryData
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