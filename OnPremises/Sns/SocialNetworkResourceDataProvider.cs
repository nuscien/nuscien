using System;
using System.Collections.Generic;
using System.Linq;
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
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Sns
{
    ///// <summary>
    ///// The resource context of social network.
    ///// </summary>
    //public class OnPremisesSocialNetworkResourceContext : BaseSocialNetworkResourceContext
    //{
    //    /// <summary>
    //    /// The database context.
    //    /// </summary>
    //    private readonly ISocialNetworkDbContext dbContext;

    //    /// <summary>
    //    /// Initializes a new instance of the OnPremisesSocialNetworkResourceContext class.
    //    /// </summary>
    //    /// <param name="dataProvider">The account data provider.</param>
    //    /// <param name="dbContext">The database context.</param>
    //    public OnPremisesSocialNetworkResourceContext(IAccountDataProvider dataProvider, ISocialNetworkDbContext dbContext)
    //        : base(new OnPremisesResourceAccessClient(dataProvider))
    //    {
    //        CoreResources = base.CoreResources as OnPremisesResourceAccessClient;
    //        this.dbContext = dbContext;
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the OnPremisesSocialNetworkResourceContext class.
    //    /// </summary>
    //    /// <param name="client">The resource access client.</param>
    //    /// <param name="dbContext">The database context.</param>
    //    public OnPremisesSocialNetworkResourceContext(OnPremisesResourceAccessClient client, ISocialNetworkDbContext dbContext)
    //        : base(client ?? new OnPremisesResourceAccessClient(null))
    //    {
    //        CoreResources = base.CoreResources as OnPremisesResourceAccessClient;
    //        this.dbContext = dbContext;
    //    }

    //    /// <summary>
    //    /// Gets the resources access client.
    //    /// </summary>
    //    public new OnPremisesResourceAccessClient CoreResources { get; }

    //    /// <summary>
    //    /// Lists contacts.
    //    /// </summary>
    //    /// <param name="q">The query arguments.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The collection of result.</returns>
    //    public override async Task<IEnumerable<ContactEntity>> ListContactsAsync(QueryArgs q, CancellationToken cancellationToken)
    //    {
    //        return await dbContext.Contacts.ToListAsync(q, cancellationToken);
    //    }

    //    /// <summary>
    //    /// Gets a specific contact.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The entity result.</returns>
    //    public override async Task<ContactEntity> GetContactAsync(string id, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Lists blogs.
    //    /// </summary>
    //    /// <param name="q">The query arguments.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The collection of result.</returns>
    //    public override async Task<IEnumerable<BlogEntity>> ListBlogsAsync(QueryArgs q, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Lists blogs.
    //    /// </summary>
    //    /// <param name="ownerType">The owner type.</param>
    //    /// <param name="owner">The owner identifier.</param>
    //    /// <param name="q">The query arguments.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The collection of result.</returns>
    //    public override async Task<IEnumerable<BlogEntity>> ListBlogsAsync(SecurityEntityTypes ownerType, string owner, QueryArgs q, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Gets a specific blogs.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The entity result.</returns>
    //    public override async Task<BlogEntity> GetBlogAsync(string id, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Lists user activities.
    //    /// </summary>
    //    /// <param name="owner">The user identifier.</param>
    //    /// <param name="q">The query arguments.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The collection of result.</returns>
    //    public override async Task<IEnumerable<UserActivityEntity>> ListUserActivitiesAsync(Users.UserEntity owner, QueryArgs q, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Lists user group activities.
    //    /// </summary>
    //    /// <param name="owner">The user identifier.</param>
    //    /// <param name="q">The query arguments.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The collection of result.</returns>
    //    public override async Task<IEnumerable<UserGroupActivityEntity>> ListUserGroupActivitiesAsync(Users.UserGroupEntity owner, QueryArgs q, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Lists mails received.
    //    /// </summary>
    //    /// <param name="folder">The folder name.</param>
    //    /// <param name="q">The query arguments.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The collection of result.</returns>
    //    public override async Task<IEnumerable<ReceivedMailEntity>> ListReceivedMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Lists mails sent or draft.
    //    /// </summary>
    //    /// <param name="folder">The folder name.</param>
    //    /// <param name="q">The query arguments.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The collection of result.</returns>
    //    public override async Task<IEnumerable<SentMailEntity>> ListSentMailAsync(string folder, QueryArgs q, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="entity">The entity to save.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> SaveAsync(ContactEntity entity, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="entity">The entity to save.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> SaveAsync(BlogEntity entity, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="entity">The entity to save.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> SaveAsync(BlogCommentEntity entity, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="entity">The entity to save.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> SaveAsync(UserActivityEntity entity, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="entity">The entity to save.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> SaveAsync(UserGroupActivityEntity entity, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="entity">The entity to save.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> SaveAsync(ReceivedMailEntity entity, CancellationToken cancellationToken);

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="entity">The entity to save.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> SaveAsync(SentMailEntity entity, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="state">The state to change.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> UpdateContactStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="state">The state to change.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> UpdateBlogStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="state">The state to change.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> UpdateBlogCommentStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="state">The state to change.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> UpdateUserActivityStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="state">The state to change.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> UpdateUserGroupActivityStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="state">The state to change.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> UpdateReceivedMailStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
    //    {

    //    }

    //    /// <summary>
    //    /// Saves an entity.
    //    /// </summary>
    //    /// <param name="id">The entity identifier.</param>
    //    /// <param name="state">The state to change.</param>
    //    /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
    //    /// <returns>The changing result information.</returns>
    //    public override async Task<ChangingResultInfo> UpdateSentMailStateAsync(string id, ResourceEntityStates state, CancellationToken cancellationToken)
    //    {

    //    }
    //}
}
