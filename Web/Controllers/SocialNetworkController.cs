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
        ///// <summary>
        ///// Lists contacts.
        ///// </summary>
        ///// <returns>The user token information.</returns>
        //[HttpGet]
        //[Route("passport/contact")]
        //public async Task<CollectionResult<ContactEntity>> ListContactsAsync()
        //{
        //    var instance = await this.GetResourceAccessClientAsync();
        //    var login = false;
        //    try
        //    {
        //        login = Request?.Body != null && Request.Body.Length > 0;
        //    }
        //    catch (NotSupportedException)
        //    {
        //    }

        //    var user = login
        //        ? await instance.SignInAsync(Request.Body)
        //        : await instance.AuthorizeAsync(Request.Headers);
        //    if (user is null) Logger?.LogInformation(new EventId(17001001, "passport"), "User tried to login but failed.");
        //    else Logger?.LogInformation(new EventId(17001001, "Login"), "User {0} logged in.", user.User?.Name ?? user.UserId);
        //    return user;
        //}
    }
}
