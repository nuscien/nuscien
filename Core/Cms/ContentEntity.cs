using System;
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
/// Base publish content entity.
/// </summary>
public abstract class BaseContentEntity : BaseSiteOwnerResourceEntity
{
    /// <summary>
    /// Gets or sets the introduction.
    /// </summary>
    [DataMember(Name = "intro")]
    [JsonPropertyName("intro")]
    [Column("intro")]
    public string Introduction
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the parent content identifier.
    /// </summary>
    [DataMember(Name = "parent")]
    [JsonPropertyName("parent")]
    [Column("parent")]
    public string ParentId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the publisher identifier.
    /// </summary>
    [DataMember(Name = "publisher")]
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
    [DataMember(Name = "thumb")]
    [JsonPropertyName("thumb")]
    [Column("thumb")]
    public string Thumbnail
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    [DataMember(Name = "templ")]
    [JsonPropertyName("templ")]
    [Column("templ")]
    public string TemplateId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the keywords.
    /// </summary>
    [DataMember(Name = "keywords")]
    [JsonPropertyName("keywords")]
    [Column("keywords")]
    public string Keywords
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the content body.
    /// </summary>
    [DataMember(Name = "content")]
    [JsonPropertyName("content")]
    [Column("content")]
    public string Content
    {
        get => GetCurrentPropertyWhenNotSlim<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the customized template content.
    /// </summary>
    [DataMember(Name = "templc")]
    [JsonPropertyName("templc")]
    [Column("templc")]
    public string TemplateContent
    {
        get => GetCurrentPropertyWhenNotSlim<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the MIME of the content.
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public string Mime => TryGetStringConfigValue("mime");

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not BaseContentEntity e) return;
        Introduction = e.Introduction;
        ParentId = e.ParentId;
        PublisherId = e.PublisherId;
        Thumbnail = e.Thumbnail;
        TemplateId = e.TemplateId;
        Keywords = e.Keywords;
        Content = e.Content;
        TemplateContent = e.TemplateContent;
    }
}

/// <summary>
/// The publish content entity.
/// </summary>
[Table("nscontents")]
[DataContract]
public class ContentEntity : BaseContentEntity
{
    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    [DataMember(Name = "creator")]
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
    public virtual ContentRevisionEntity CreateRevision(string message)
    {
        return new ContentRevisionEntity
        {
            Name = Name,
            Config = Config,
            State = ResourceEntityStates.Normal,
            PublisherId = PublisherId ?? CreatorId,
            Introduction = Introduction,
            Thumbnail = Thumbnail,
            Content = Content,
            TemplateContent = TemplateContent,
            TemplateId = TemplateId,
            ParentId = ParentId,
            SourceId = Id,
            Message = message
        };
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not ContentEntity e) return;
        CreatorId = e.CreatorId;
    }
}

/// <summary>
/// The revision entity of publish content.
/// </summary>
[DataContract]
[Table("nscontrev")]
public class ContentRevisionEntity : BaseContentEntity
{
    /// <summary>
    /// Gets or sets the owner source identifier.
    /// </summary>
    [DataMember(Name = "owner")]
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
    [DataMember(Name = "message")]
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
        if (entity is not ContentRevisionEntity e) return;
        SourceId = e.SourceId;
        Message = e.Message;
    }
}
