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
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Cms
{
    /// <summary>
    /// Base content entity.
    /// </summary>
    public abstract class BaseContentEntity : BaseSiteOwnerResourceEntity
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
        /// Gets or sets the parent content identifier.
        /// </summary>
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
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        [JsonPropertyName("templ")]
        [Column("template")]
        public string Template
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The content entity.
    /// </summary>
    [Table("nscontent")]
    public class ContentEntity : BaseContentEntity
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
    }

    /// <summary>
    /// The revision entity of content.
    /// </summary>
    [Table("nscontrev")]
    public class ContentRevisionEntity : BaseContentEntity
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
        [JsonPropertyName("msg")]
        [Column("msg")]
        public string Message
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }
}
