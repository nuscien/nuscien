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

namespace NuScien.Sns
{
    /// <summary>
    /// Base comment entity.
    /// </summary>
    public abstract class BaseCommentEntity : BaseOwnerResourceEntity
    {
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
        /// Gets or sets the parent message identifier.
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
        /// Gets or sets the ancestor message identifier.
        /// </summary>
        [DataMember(Name = "ancestor")]
        [JsonPropertyName("ancestor")]
        [Column("ancestor")]
        public string SourceMessageId
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
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        ///// <summary>
        ///// Gets or sets the mail address list.
        ///// </summary>
        //[JsonIgnore]
        //[NotMapped]
        //public Trivial.Geography.Geolocation.Model Location
        //{
        //    get => TryDeserializeConfigValue<Trivial.Geography.Geolocation.Model>("location");
        //    set => SetConfigValue("location", value);
        //}

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not BaseCommentEntity e) return;
            PublisherId = e.PublisherId;
            ParentId = e.ParentId;
            SourceMessageId = e.SourceMessageId;
            Content = e.Content;
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
    [DataContract]
    [Table("nscontcomments")]
    public class ContentCommentEntity : BaseCommentEntity<ContentEntity>
    {
    }

    /// <summary>
    /// The blog comment entity.
    /// </summary>
    [DataContract]
    [Table("nsblogcomments")]
    public class BlogCommentEntity : BaseCommentEntity<BlogEntity>
    {
    }

    /// <summary>
    /// The user activity entity.
    /// </summary>
    [DataContract]
    [Table("nsuseractivities")]
    public class UserActivityEntity : BaseCommentEntity<Users.UserEntity>
    {
    }

    /// <summary>
    /// The user group activity entity.
    /// </summary>
    [DataContract]
    [Table("nsgroupactivities")]
    public class UserGroupActivityEntity : BaseCommentEntity<Users.UserEntity>
    {
    }
}
