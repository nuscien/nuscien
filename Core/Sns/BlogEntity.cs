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
using NuScien.Security;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Sns;

/// <summary>
/// Blog entity.
/// </summary>
[Table("nsblogs")]
public class BlogEntity : BaseOwnerResourceEntity
{
    /// <summary>
    /// Gets or sets the security entity type.
    /// </summary>
    [NotMapped]
    [JsonPropertyName("kind")]
    public SecurityEntityTypes OwnerType
    {
        get => GetCurrentProperty<SecurityEntityTypes>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the security entity type code.
    /// </summary>
    [Column("kind")]
    [JsonIgnore]
    public int OwnerTypeCode
    {
        get => (int)OwnerType;
        set => OwnerType = (SecurityEntityTypes)value;
    }

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
    /// Gets or sets the category.
    /// </summary>
    [JsonPropertyName("category")]
    [Column("category")]
    public string Category
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the keywords.
    /// </summary>
    [JsonPropertyName("keywords")]
    [Column("keywords")]
    public string Keywords
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
        if (entity is not BlogEntity e) return;
        OwnerType = e.OwnerType;
        Introduction = e.Introduction;
        PublisherId = e.PublisherId;
        Thumbnail = e.Thumbnail;
        Content = e.Content;
    }
}
