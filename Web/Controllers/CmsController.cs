﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuScien.Cms;
using NuScien.Data;
using NuScien.Security;
using NuScien.Users;
using NuScien.Sns;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Web;

/// <summary>
/// The passport and settings controller.
/// </summary>
public partial class ResourceAccessControllerBase : ControllerBase
{
    /// <summary>
    /// Gets the publish content.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/c/{id}")]
    public async Task<IActionResult> GetContentById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var m = await instance.GetContentAsync(id, Request.Query.TryGetBoolean("all") == true);
        return this.ResourceEntityResult(m);
    }

    /// <summary>
    /// Lists the publish content.
    /// </summary>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/c")]
    public async Task<IActionResult> ListContent()
    {
        var instance = await this.GetResourceAccessClientAsync();
        var q = Request.Query?.GetQueryArgs();
        var parent = Request.Query?["parent"].ToString();
        var col = parent == "*"
            ? await instance.ListContentAsync(Request.Query?["site"], true, q)
            : await instance.ListContentAsync(Request.Query?["site"], parent, q);
        return this.ResourceEntityResult(col.Select(ele =>
        {
            if (ele == null) return null;
            ele.IsSlim = true;
            return ele;
        }), q?.Offset);
    }

    /// <summary>
    /// Lists the publish content revision.
    /// </summary>
    /// <param name="id">The source entity identifier.</param>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/c/{id}/rev")]
    public async Task<IActionResult> ListContentRevisionById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var q = Request.Query?.GetQueryArgs();
        var col = await instance.ListContentRevisionAsync(id, q);
        return this.ResourceEntityResult(col.Select(ele =>
        {
            if (ele == null) return null;
            ele.IsSlim = true;
            return ele;
        }), q?.Offset);
    }

    /// <summary>
    /// Gets the publish content revision.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/cr/{id}")]
    public async Task<IActionResult> GetContentRevisionById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var m = await instance.GetContentRevisionAsync(id);
        return this.ResourceEntityResult(m);
    }

    /// <summary>
    /// Creates or updates an entity.
    /// </summary>
    /// <param name="entity">The entity to save.</param>
    /// <returns>The status of changing result.</returns>
    [HttpPut]
    [Route("cms/c")]
    public async Task<IActionResult> SaveContentAsync([FromBody] ContentEntity entity)
    {
        if (entity == null) return ChangeErrorKinds.Argument.ToActionResult("Requires an entity in body.");
        var instance = await this.GetResourceAccessClientAsync();
        string message = null;
        if (entity.ExtensionSerializationData != null && entity.ExtensionSerializationData.TryGetValue("message", out var msg) && msg.ValueKind == JsonValueKind.String)
            message = msg.GetString();
        var result = await instance.SaveAsync(entity, message);
        Logger?.LogInformation(new EventId(17001211, "SaveContentInfo"), "Save publish content information {0}. {1}", entity.Name ?? entity.Id, message);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates a specific entity.
    /// </summary>
    /// <param name="id">The entity to save.</param>
    /// <returns>The status of changing result.</returns>
    [HttpPut]
    [Route("cms/c/{id}")]
    public Task<IActionResult> SaveContentAsync(string id)
    {
        return this.SaveEntityAsync(async (i, instance) =>
        {
            return await instance.GetContentAsync(id);
        }, async (entity, instance, delta) =>
        {
            var message = delta.TryGetStringValue("message");
            var result = await instance.SaveAsync(entity, message);
            Logger?.LogInformation(new EventId(17001212, "SaveUserInfo"), "Save user information {0}.", entity.Name ?? entity.Id);
            return result;
        }, id);
    }

    /// <summary>
    /// Gets the publish content template.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/t/{id}")]
    public async Task<IActionResult> GetContentTemplateById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var m = await instance.GetContentTemplateAsync(id, Request.Query?.TryGetBoolean("all") == true);
        return this.ResourceEntityResult(m);
    }

    /// <summary>
    /// Lists the publish content template.
    /// </summary>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/t")]
    public async Task<IActionResult> ListContentTemplate()
    {
        var instance = await this.GetResourceAccessClientAsync();
        var q = Request.Query?.GetQueryArgs();
        var col = await instance.ListContentTemplateAsync(Request.Query?["site"], q);
        return this.ResourceEntityResult(col.Select(ele =>
        {
            if (ele == null) return null;
            ele.IsSlim = true;
            return ele;
        }), q?.Offset);
    }

    /// <summary>
    /// Lists the publish content template revision.
    /// </summary>
    /// <param name="id">The source entity identifier.</param>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/t/{id}/rev")]
    public async Task<IActionResult> ListContentTemplateRevisionById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var q = Request.Query?.GetQueryArgs();
        var col = await instance.ListContentTemplateRevisionAsync(id, q);
        return this.ResourceEntityResult(col.Select(ele =>
        {
            if (ele == null) return null;
            ele.IsSlim = true;
            return ele;
        }), q?.Offset);
    }

    /// <summary>
    /// Gets the publish content template revision.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The value.</returns>
    [HttpGet]
    [Route("cms/tr/{id}")]
    public async Task<IActionResult> GetContentTemplateRevisionById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var m = await instance.GetContentTemplateRevisionAsync(id);
        return this.ResourceEntityResult(m);
    }

    /// <summary>
    /// Creates or updates an entity.
    /// </summary>
    /// <param name="entity">The entity to save.</param>
    /// <returns>The status of changing result.</returns>
    [HttpPut]
    [Route("cms/t")]
    public async Task<IActionResult> SaveContentTemplateAsync([FromBody] ContentTemplateEntity entity)
    {
        if (entity == null) return ChangeErrorKinds.Argument.ToActionResult("Requires an entity in body.");
        var instance = await this.GetResourceAccessClientAsync();
        string message = null;
        if (entity.ExtensionSerializationData != null && entity.ExtensionSerializationData.TryGetValue("message", out var msg) && msg.ValueKind == JsonValueKind.String)
            message = msg.GetString();
        var result = await instance.SaveAsync(entity, message);
        Logger?.LogInformation(new EventId(17001213, "SaveContentTemplateInfo"), "Save publish content template information {0}. {1}", entity.Name ?? entity.Id, message);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates a specific entity.
    /// </summary>
    /// <param name="id">The entity to save.</param>
    /// <returns>The status of changing result.</returns>
    [HttpPut]
    [Route("cms/t/{id}")]
    public Task<IActionResult> SaveContentTemplateAsync(string id)
    {
        return this.SaveEntityAsync(async (i, instance) =>
        {
            return await instance.GetContentTemplateAsync(id);
        }, async (entity, instance, delta) =>
        {
            var message = delta.TryGetStringValue("message");
            var result = await instance.SaveAsync(entity, message);
            Logger?.LogInformation(new EventId(17001214, "SaveContentTemplateInfo"), "Save publish content template information {0}. {1}", entity.Name ?? entity.Id, message);
            return result;
        }, id);
    }

    /// <summary>
    /// Lists the publish content comments.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The entity list.</returns>
    [HttpGet]
    [Route("cms/c/{id}/comments")]
    public async Task<IActionResult> ListContentCommentsAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var plain = JsonBooleanNode.TryParse(Request.Query?["plain"]);
        var m = await instance.ListContentCommentsAsync(id, plain == true);
        return this.ResourceEntityResult(m);
    }

    /// <summary>
    /// Gets a specific publish content comment.
    /// </summary>
    /// <param name="id">The parent identifier of the content comment.</param>
    /// <returns>The entity list.</returns>
    [HttpGet]
    [Route("cms/cc/{id}")]
    public async Task<IActionResult> GetContentCommentAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var m = await instance.GetContentCommentAsync(id);
        return this.ResourceEntityResult(m);
    }

    /// <summary>
    /// Deletes a specific publish content comment.
    /// </summary>
    /// <param name="id">The parent identifier of the content comment.</param>
    /// <returns>The entity list.</returns>
    [HttpDelete]
    [Route("cms/cc/{id}")]
    public async Task<IActionResult> DeleteContentCommentAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var r = await instance.DeleteContentCommentAsync(id);
        return r.ToActionResult();
    }

    /// <summary>
    /// Lists the child comments of a specific publish content comment.
    /// </summary>
    /// <param name="id">The parent identifier of the content comment.</param>
    /// <returns>The entity list.</returns>
    [HttpGet]
    [Route("cms/cc/{id}/children")]
    public async Task<IActionResult> ListContentChildCommentsAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();
        var instance = await this.GetResourceAccessClientAsync();
        var m = await instance.ListContentChildCommentsAsync(id);
        return this.ResourceEntityResult(m);
    }

    /// <summary>
    /// Creates or updates an entity.
    /// </summary>
    /// <param name="entity">The entity to save.</param>
    /// <returns>The status of changing result.</returns>
    [HttpPut]
    [Route("cms/cc")]
    public async Task<IActionResult> SaveContentCommentAsync([FromBody] ContentCommentEntity entity)
    {
        if (entity == null) return ChangeErrorKinds.Argument.ToActionResult("Requires an entity in body.");
        var instance = await this.GetResourceAccessClientAsync();
        var result = await instance.SaveAsync(entity);
        Logger?.LogInformation(new EventId(17001217, "PostContentComment"), "Post a comment to a publish content.");
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates a specific entity.
    /// </summary>
    /// <param name="id">The entity to save.</param>
    /// <returns>The status of changing result.</returns>
    [HttpPut]
    [Route("cms/cc/{id}")]
    public Task<IActionResult> SaveContentCommentAsync(string id)
    {
        return this.SaveEntityAsync(async (i, instance) =>
        {
            return await instance.GetContentCommentAsync(id);
        }, async (entity, instance, delta) =>
        {
            var result = await instance.SaveAsync(entity);
            Logger?.LogInformation(new EventId(17001218, "PostContentComment"), "Post a comment to a publish content.");
            return result;
        }, id);
    }
}
