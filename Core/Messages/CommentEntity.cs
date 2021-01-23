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

using NuScien.Cms;
using NuScien.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Messages
{
    /// <summary>
    /// Base comment entity.
    /// </summary>
    public abstract class BaseCommentEntity : ConfigurableResourceEntity
    {
        /// <summary>
        /// Gets or sets the identifier of the owner entity.
        /// </summary>
        [JsonPropertyName("owner")]
        [Column("owner")]
        public string SourceId
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
        /// Gets or sets the content body.
        /// </summary>
        [JsonPropertyName("content")]
        [Column("content")]
        public string Content
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The base comment entity.
    /// </summary>
    public class BaseCommentEntity<T> : BaseCommentEntity
    {
        /// <summary>
        /// Gets or sets the content entity.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public T OwnerContent { get; set; }
    }

    /// <summary>
    /// The content comment entity.
    /// </summary>
    [Table("nscontcomment")]
    public class ContentCommentEntity : BaseCommentEntity<ContentEntity>
    {
    }
}
