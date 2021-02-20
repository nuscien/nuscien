using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuScien.Data;
using NuScien.Sns;
using NuScien.Security;
using NuScien.Users;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Web
{
    /// <summary>
    /// The passport and settings controller.
    /// </summary>
    public partial class ResourceAccessController : ControllerBase
    {
        /// <summary>
        /// Lists contacts.
        /// </summary>
        /// <returns>The collection of contact.</returns>
        [HttpGet]
        [Route("passport/contact")]
        public Task<IActionResult> ListContactsAsync()
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListContactsAsync(q), new EventId(17001301, "ListContacts"), "List contacts.");
        }

        /// <summary>
        /// Gets a specific contact.
        /// </summary>
        /// <returns>The contact.</returns>
        [HttpGet]
        [Route("passport/contact/{id}")]
        public Task<IActionResult> GetContactAsync(string id)
        {
            return FromSnsResourcesAsync((sns, id, eventId) => sns.GetContactAsync(id), id, new EventId(17001304, "GetContacts"), "Get details of a contact.");
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("passport/contact")]
        public Task<IActionResult> SaveAsync([FromBody] ContactEntity entity)
        {
            return this.SaveSnsEntityAsync(entity, (sns, e, cancellationToken) => sns.SaveAsync(entity, cancellationToken), Logger, new EventId(17001306, "SaveEntity"));
        }

        /// <summary>
        /// Updates.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("passport/contact/{id}")]
        public Task<IActionResult> UpdateContactAsync(string id)
        {
            return this.UpdateSnsEntityAsync(id, (sns, s, cancellationToken) => sns.GetContactAsync(id, cancellationToken), (sns, e, cancellationToken) => sns.SaveAsync(e, cancellationToken), Logger, new EventId(17001307, "SaveEntity"));
        }

        /// <summary>
        /// Lists blogs.
        /// </summary>
        /// <returns>The collection of blog.</returns>
        [HttpGet]
        [Route("sns/blog/all")]
        public Task<IActionResult> ListBlogsAsync()
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListBlogsAsync(false, q), new EventId(17001311, "ListBlogs"), "List blogs.");
        }

        /// <summary>
        /// Lists blogs of a specific user.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>The collection of blog.</returns>
        [HttpGet]
        [Route("sns/blog/u/{id}")]
        public Task<IActionResult> ListUserBlogsAsync(string id)
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListBlogsAsync(SecurityEntityTypes.User, id, q), new EventId(17001312, "ListBlogs"), $"List blogs of a specific user {id}.", null, true);
        }

        /// <summary>
        /// Lists blogs of a specific user group.
        /// </summary>
        /// <param name="id">The user group identifier.</param>
        /// <returns>The collection of blog.</returns>
        [HttpGet]
        [Route("sns/blog/u/{id}")]
        public Task<IActionResult> ListGroupBlogsAsync(string id)
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListBlogsAsync(SecurityEntityTypes.UserGroup, id, q), new EventId(17001313, "ListContacts"), $"List blogs of a specific user group {id}.", null, true);
        }

        /// <summary>
        /// Gets a specific blog.
        /// </summary>
        /// <returns>The blog.</returns>
        [HttpGet]
        [Route("sns/blog/c/{id}")]
        public Task<IActionResult> GetBlogAsync(string id)
        {
            return FromSnsResourcesAsync((sns, id, eventId) => sns.GetBlogAsync(id), id, new EventId(17001320, "GetBlog"), "Get details of a blog.");
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/blog/c")]
        public Task<IActionResult> SaveAsync([FromBody] BlogEntity entity)
        {
            return this.SaveSnsEntityAsync(entity, (sns, e, cancellationToken) => sns.SaveAsync(entity, cancellationToken), Logger, new EventId(17001322, "SaveEntity"));
        }

        /// <summary>
        /// Updates.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/blog/c/{id}")]
        public Task<IActionResult> UpdateBlogAsync(string id)
        {
            return this.UpdateSnsEntityAsync(id, (sns, s, cancellationToken) => sns.GetBlogAsync(id, cancellationToken), (sns, e, cancellationToken) => sns.SaveAsync(e, cancellationToken), Logger, new EventId(17001323, "SaveEntity"));
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="id">The blog identifier.</param>
        /// <returns>The collection of blog.</returns>
        [HttpGet]
        [Route("sns/blog/c/{id}/comments")]
        public Task<IActionResult> ListBlogCommentsAsync(string id)
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListBlogCommentsAsync(id, Request.Query.TryGetBoolean("plain") ?? false, q), new EventId(17001326, "ListBlogComments"), "List blog comments.", null, true);
        }

        /// <summary>
        /// Lists blog comments.
        /// </summary>
        /// <param name="id">The blog identifier.</param>
        /// <returns>The collection of blog.</returns>
        [HttpGet]
        [Route("sns/blog/cc/{id}/children")]
        public Task<IActionResult> ListBlogChildCommentsAsync(string id)
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListBlogChildCommentsAsync(id, q), new EventId(17001326, "ListBlogComments"), "List blog comments.", null, true);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/blog/cc")]
        public Task<IActionResult> SaveAsync([FromBody] BlogCommentEntity entity)
        {
            return this.SaveSnsEntityAsync(entity, (sns, e, cancellationToken) => sns.SaveAsync(entity, cancellationToken), Logger, new EventId(17001328, "SaveEntity"));
        }

        /// <summary>
        /// Updates state.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/blog/cc/{id}")]
        public Task<IActionResult> UpdateContactCommentAsync(string id)
        {
            return this.UpdateSnsEntityAsync(id, (sns, s, state, cancellationToken) => sns.UpdateBlogCommentAsync(id, state, cancellationToken), Logger, new EventId(17001329, "SaveEntity"));
        }

        /// <summary>
        /// Lists user activities.
        /// </summary>
        /// <param name="id">The blog identifier.</param>
        /// <returns>The collection of blog.</returns>
        [HttpGet]
        [Route("sns/activity/u/{id}")]
        public Task<IActionResult> ListUserActivitiesAsync(string id)
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListUserActivitiesAsync(id, q), new EventId(17001331, "ListUserActivities"), "List user activities.", null, true);
        }

        /// <summary>
        /// Lists user group activities.
        /// </summary>
        /// <param name="id">The blog identifier.</param>
        /// <returns>The collection of blog.</returns>
        [HttpGet]
        [Route("sns/activity/g/{id}")]
        public Task<IActionResult> ListUserGroupActivitiesAsync(string id)
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListUserGroupActivitiesAsync(id, q), new EventId(17001341, "ListGroupActivities"), "List user group activities.", null, true);
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/activity/u")]
        public Task<IActionResult> SaveAsync([FromBody] UserActivityEntity entity)
        {
            return this.SaveSnsEntityAsync(entity, (sns, e, cancellationToken) => sns.SaveAsync(entity, cancellationToken), Logger, new EventId(17001335, "SaveEntity"));
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/activity/g")]
        public Task<IActionResult> SaveAsync([FromBody] UserGroupActivityEntity entity)
        {
            return this.SaveSnsEntityAsync(entity, (sns, e, cancellationToken) => sns.SaveAsync(entity, cancellationToken), Logger, new EventId(17001345, "SaveEntity"));
        }

        /// <summary>
        /// Updates state.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/activity/u/{id}")]
        public Task<IActionResult> UpdateUserActivityAsync(string id)
        {
            return this.UpdateSnsEntityAsync(id, (sns, s, state, cancellationToken) => sns.UpdateUserActivityAsync(id, state, cancellationToken), Logger, new EventId(17001336, "SaveEntity"));
        }

        /// <summary>
        /// Updates state.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("sns/activity/g/{id}")]
        public Task<IActionResult> UpdateUserGroupActivityAsync(string id)
        {
            return this.UpdateSnsEntityAsync(id, (sns, s, state, cancellationToken) => sns.UpdateUserGroupActivityAsync(id, state, cancellationToken), Logger, new EventId(17001346, "SaveEntity"));
        }

        /// <summary>
        /// Lists mail received.
        /// </summary>
        /// <returns>The collection of mail.</returns>
        [HttpGet]
        [Route("mail/i")]
        public Task<IActionResult> ListReceivedMailAsync()
        {
            return ListReceivedMailAsync(null);
        }

        /// <summary>
        /// Lists mail received.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>The collection of mail.</returns>
        [HttpGet]
        [Route("mail/i/{folder}")]
        public Task<IActionResult> ListReceivedMailAsync(string folder)
        {
            return FromSnsResourcesAsync((sns, q, eventId) =>
            {
                var filter = new MailAdditionalFilterInfo();
                return sns.ListReceivedMailAsync(folder, q, filter);
            }, new EventId(17001411, "ListReceivedMails"), $"List received mails.", null, true);
        }

        /// <summary>
        /// Gets a mail received.
        /// </summary>
        /// <returns>The mail received.</returns>
        [HttpGet]
        [Route("mail/m/{id}")]
        public Task<IActionResult> GetReceivedMailAsync(string id)
        {
            return FromSnsResourcesAsync((sns, id, eventId) => sns.GetReceivedMailAsync(id), id, new EventId(17001410, "GetReceivedMail"), "Get a mail received.");
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("mail/m")]
        public Task<IActionResult> SaveAsync([FromBody] ReceivedMailEntity entity)
        {
            return this.SaveSnsEntityAsync(entity, (sns, e, cancellationToken) => sns.SaveAsync(entity, cancellationToken), Logger, new EventId(17001416, "SaveEntity"));
        }

        /// <summary>
        /// Updates.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("mail/m/{id}")]
        public Task<IActionResult> UpdateReceivedMailAsync(string id)
        {
            return this.UpdateSnsEntityAsync(id, (sns, s, cancellationToken) => sns.GetReceivedMailAsync(id, cancellationToken), (sns, e, cancellationToken) => sns.SaveAsync(e, cancellationToken), Logger, new EventId(17001417, "SaveEntity"));
        }

        /// <summary>
        /// Lists mail sent or draft.
        /// </summary>
        /// <returns>The collection of mail.</returns>
        [HttpGet]
        [Route("mail/o")]
        public Task<IActionResult> ListSentMailAsync()
        {
            return ListSentMailAsync(null);
        }

        /// <summary>
        /// Lists mail sent or draft.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>The collection of mail.</returns>
        [HttpGet]
        [Route("mail/o/{folder}")]
        public Task<IActionResult> ListSentMailAsync(string folder)
        {
            return FromSnsResourcesAsync((sns, q, eventId) => sns.ListSentMailAsync(folder, q), new EventId(17001421, "ListSentMails"), $"List sent mails.", null, true);
        }

        /// <summary>
        /// Gets a mail sent or draft.
        /// </summary>
        /// <returns>The mail sent or draft.</returns>
        [HttpGet]
        [Route("mail/e/{id}")]
        public Task<IActionResult> GetSentMailAsync(string id)
        {
            return FromSnsResourcesAsync((sns, id, eventId) => sns.GetSentMailAsync(id), id, new EventId(17001420, "GetSentMail"), "Get a mail sent.");
        }

        /// <summary>
        /// Saves.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("mail/e")]
        public Task<IActionResult> SaveAsync([FromBody] SentMailEntity entity)
        {
            return this.SaveSnsEntityAsync(entity, (sns, e, cancellationToken) => sns.SaveAsync(entity, cancellationToken), Logger, new EventId(17001426, "SaveEntity"));
        }

        /// <summary>
        /// Updates.
        /// </summary>
        /// <returns>The changing result.</returns>
        [HttpPut]
        [Route("mail/e/{id}")]
        public Task<IActionResult> UpdateSentMailAsync(string id)
        {
            return this.UpdateSnsEntityAsync(id, (sns, s, cancellationToken) => sns.GetSentMailAsync(id, cancellationToken), (sns, e, cancellationToken) => sns.SaveAsync(e, cancellationToken), Logger, new EventId(17001427, "SaveEntity"));
        }

        private async Task<IActionResult> FromSnsResourcesAsync<T>(Func<BaseSocialNetworkResourceContext, EventId, Task<T>> h, Func<T, IActionResult> toActionResult, EventId eventId, string message = null, Action<Exception, bool, EventId> onError = null, bool noNeedLogin = false)
        {
            var instance = await this.GetResourceAccessClientAsync();
            var sns = await SocialNetworkResources.CreateAsync(instance);
            if (sns?.CoreResources == null) return this.ExceptionResult(501, "Do not support this feature.");
            if (!noNeedLogin && !sns.CoreResources.IsUserSignedIn) return this.ExceptionResult(403, "Requires login firstly.");
            try
            {
                var result = await h(sns, eventId);
                Logger?.LogInformation(eventId, message);
                return toActionResult(result);
            }
            catch (Exception ex)
            {
                var er = this.ExceptionResult(ex, true);
                var hasHandled = er != null;
                onError?.Invoke(ex, hasHandled, eventId);
                if (hasHandled) return er;
                throw;
            }
        }

        private Task<IActionResult> FromSnsResourcesAsync<T>(Func<BaseSocialNetworkResourceContext, QueryArgs, EventId, Task<IEnumerable<T>>> h, EventId eventId, string message = null, Action<Exception, bool, EventId> onError = null, bool noNeedLogin = false)
            where T : BaseResourceEntity
        {
            QueryArgs q = null;
            return FromSnsResourcesAsync((context, eventId) =>
            {
                q = Request.Query.GetQueryArgs();
                return h(context, q, eventId);
            }, col => this.ResourceEntityResult(col, q.Offset), eventId, message, onError, noNeedLogin);
        }

        private Task<IActionResult> FromSnsResourcesAsync<T>(Func<BaseSocialNetworkResourceContext, string, EventId, Task<T>> h, string id, EventId eventId, string message = null, Action<Exception, bool, EventId> onError = null, bool noNeedLogin = false)
            where T : BaseResourceEntity
        {
            return FromSnsResourcesAsync((context, eventId) =>
            {
                return h(context, id, eventId);
            }, entity => this.ResourceEntityResult(entity), eventId, message, onError, noNeedLogin);
        }
    }
}
