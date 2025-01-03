﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Cms;

/// <summary>
/// Base publish content template entity.
/// </summary>
public abstract class BaseContentTemplateEntity : BaseSiteOwnerResourceEntity
{
    /// <summary>
    /// Gets or sets the introduction.
    /// </summary>
    [JsonPropertyName("intro")]
    [Column("intro")]
    public string Introduction
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the publisher identifier.
    /// </summary>
    [JsonPropertyName("publisher")]
    [Column("publisher")]
    public string PublisherId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the URL of thumbnail.
    /// </summary>
    [JsonPropertyName("thumb")]
    [Column("thumb")]
    public string Thumbnail
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the content body.
    /// </summary>
    [JsonPropertyName("content")]
    [Column("content")]
    public string Content
    {
        get => GetCurrentPropertyWhenNotSlim<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not BaseContentTemplateEntity e) return;
        Introduction = e.Introduction;
        PublisherId = e.PublisherId;
        Thumbnail = e.Thumbnail;
        Content = e.Content;
    }
}

/// <summary>
/// The publish content template entity.
/// </summary>
[Table("nstemplates")]
public class ContentTemplateEntity : BaseContentTemplateEntity
{
    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    [JsonPropertyName("creator")]
    [Column("creator")]
    public string CreatorId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Creates a revision entity.
    /// </summary>
    /// <param name="message">The commit message.</param>
    /// <returns>A revision entity.</returns>
    public virtual ContentTemplateRevisionEntity CreateRevision(string message)
    {
        return new ContentTemplateRevisionEntity
        {
            Name = Name,
            Config = Config,
            State = ResourceEntityStates.Normal,
            PublisherId = PublisherId ?? CreatorId,
            Introduction = Introduction,
            Thumbnail = Thumbnail,
            Content = Content,
            SourceId = Id,
            Message = message
        };
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not ContentTemplateEntity e) return;
        CreatorId = e.CreatorId;
    }
}

/// <summary>
/// The revision entity of publish content template.
/// </summary>
[Table("nstemplrev")]
public class ContentTemplateRevisionEntity : BaseContentTemplateEntity
{
    /// <summary>
    /// Gets or sets the owner source identifier.
    /// </summary>
    [JsonPropertyName("owner")]
    [Column("owner")]
    public string SourceId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the commit message.
    /// </summary>
    [JsonPropertyName("message")]
    [Column("message")]
    public string Message
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not ContentTemplateRevisionEntity e) return;
        SourceId = e.SourceId;
        Message = e.Message;
    }
}
