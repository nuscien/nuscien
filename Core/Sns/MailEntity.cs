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
using NuScien.Tasks;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Sns
{
    /// <summary>
    /// Mail flags.
    /// </summary>
    public enum MailFlags
    {
        /// <summary>
        /// Unread.
        /// </summary>
        Unread = 0,

        /// <summary>
        /// Has been read.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Marked as flag.
        /// </summary>
        Flagged = 2,

        /// <summary>
        /// Not important.
        /// </summary>
        Unimportant = 3,

        /// <summary>
        /// Junk.
        /// </summary>
        Spam = 4
    }

    /// <summary>
    /// The mail address list.
    /// </summary>
    public class MailAddressListInfo
    {
        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        [JsonPropertyName("from")]
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the receiver address list.
        /// </summary>
        [JsonPropertyName("to")]
        public IEnumerable<string> To { get; set; }

        /// <summary>
        /// Gets or sets the address list to carbon copy.
        /// </summary>
        [JsonPropertyName("cc")]
        public IEnumerable<string> Cc { get; set; }

        /// <summary>
        /// Gets or sets the reply address.
        /// </summary>
        [JsonPropertyName("re")]
        public string Reply { get; set; }
    }

    /// <summary>
    /// The information for mail sending.
    /// </summary>
    public class MailSendingInfo
    {
        /// <summary>
        /// Gets or sets the address list to secret carbon copy.
        /// </summary>
        [JsonPropertyName("bcc")]
        public IEnumerable<string> Bcc { get; set; }
    }

    /// <summary>
    /// The mail attachment information.
    /// </summary>
    public class MailAttachmentInfo
    {
        /// <summary>
        /// Gets or sets the reference identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the MIME of the attachment.
        /// </summary>
        [JsonPropertyName("mime")]
        public string Mime { get; set; }

        /// <summary>
        /// Gets or sets the path of the attachment.
        /// </summary>
        [JsonPropertyName("url")]
        public string Path { get; set; }
    }

    /// <summary>
    /// Base mail entity.
    /// </summary>
    public class BaseMailEntity : BaseOwnerResourceEntity
    {
        /// <summary>
        /// Gets or sets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public SecurityEntityTypes OwnerType
        {
            get => GetCurrentProperty<SecurityEntityTypes>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the security entity type code.
        /// </summary>
        [DataMember(Name = "kind")]
        [Column("kind")]
        [JsonPropertyName("kind")]
        public int OwnerTypeCode
        {
            get => (int)OwnerType;
            set => OwnerType = (SecurityEntityTypes)value;
        }

        /// <summary>
        /// Gets or sets the thread identifier.
        /// </summary>
        [DataMember(Name = "thread")]
        [JsonPropertyName("thread")]
        [Column("thread")]
        public string ThreadId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the folder name.
        /// </summary>
        [DataMember(Name = "folder")]
        [JsonPropertyName("folder")]
        [Column("folder")]
        public string Folder
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        [DataMember(Name = "sender")]
        [JsonPropertyName("sender")]
        [Column("sender")]
        public string Sender
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the date time when send.
        /// </summary>
        [DataMember(Name = "send")]
        [JsonPropertyName("send")]
        [Column("send")]
        public DateTime SendTime
        {
            get => GetCurrentProperty<DateTime>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public BasicPriorities Priority
        {
            get => GetCurrentProperty<BasicPriorities>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the priority code.
        /// </summary>
        [DataMember(Name = "priority")]
        [JsonPropertyName("priority")]
        [Column("priority")]
        public int PriorityCode
        {
            get => (int)Priority;
            set => Priority = (BasicPriorities)value;
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
        /// Gets or sets the mail address list.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public MailAddressListInfo AddressList
        {
            get => TryDeserializeConfigValue<MailAddressListInfo>("addr");
            set => SetConfigValue("addr", value);
        }

        /// <summary>
        /// Gets or sets the sending information.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public MailSendingInfo SendingInfo
        {
            get => TryDeserializeConfigValue<MailSendingInfo>("send");
            set => SetConfigValue("send", value);
        }

        /// <summary>
        /// Gets or sets the mail attachment list.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public IEnumerable<MailAttachmentInfo> Attachment
        {
            get => TryDeserializeConfigValue<IEnumerable<MailAttachmentInfo>>("attach");
            set => SetConfigValue("attach", value);
        }

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not BaseMailEntity e) return;
            Folder = e.Folder;
            Sender = e.Sender;
            SendTime = e.SendTime;
            Priority = e.Priority;
            Content = e.Content;
        }
    }

    /// <summary>
    /// Mail entity for sent or draft.
    /// </summary>
    [DataContract]
    [Table("nsmails1")]
    public class SentMailEntity : BaseMailEntity
    {
        /// <summary>
        /// Gets or sets the sender identifier.
        /// </summary>
        [DataMember(Name = "app")]
        [JsonPropertyName("app")]
        [Column("app")]
        public string ApplicationName
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        internal IEnumerable<ReceivedMailEntity> ToReceiveMails()
        {
            var col = new List<ReceivedMailEntity>();
            var addr = AddressList;
            if (addr == null) return col;

            return col;
        }

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not SentMailEntity e) return;
            ApplicationName = e.ApplicationName;
        }
    }

    /// <summary>
    /// Mail entity for received.
    /// </summary>
    [DataContract]
    [Table("nsmails2")]
    public class ReceivedMailEntity : BaseMailEntity
    {
        /// <summary>
        /// Initializes a new instance of the ReceivedMailEntity class.
        /// </summary>
        public ReceivedMailEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ReceivedMailEntity class.
        /// </summary>
        internal ReceivedMailEntity(SentMailEntity m, string ownerId)
        {
            FillBaseProperties(m);
            CopyConfigItself("send", "preference");
        }

        /// <summary>
        /// Gets or sets the sender identifier.
        /// </summary>
        [DataMember(Name = "res")]
        [JsonPropertyName("res")]
        [Column("res")]
        public string TargetId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public MailFlags Flag
        {
            get => GetCurrentProperty<MailFlags>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the flag code.
        /// </summary>
        [DataMember(Name = "flag")]
        [JsonPropertyName("flag")]
        [Column("flag")]
        public int FlagCode
        {
            get => (int)Flag;
            set => Flag = (MailFlags)value;
        }


        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not ReceivedMailEntity e) return;
            TargetId = e.TargetId;
            Flag = e.Flag;
        }
    }
}
