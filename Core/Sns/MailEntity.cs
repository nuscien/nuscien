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

namespace NuScien.Sns;

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
    /// Gets or sets the sender display name.
    /// </summary>
    [DataMember(Name = "sender")]
    [JsonPropertyName("sender")]
    [Column("sender")]
    public string SenderName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    [DataMember(Name = "addr")]
    [JsonPropertyName("addr")]
    [Column("addr")]
    public string SenderAddress
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
    /// Gets or sets the mail attachment list.
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public IEnumerable<MailAttachmentInfo> Attachment
    {
        get => TryDeserializeConfigValue<IEnumerable<MailAttachmentInfo>>("attach");
        set => SetConfigValue("attach", value);
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
        if (entity is not BaseMailEntity e) return;
        Folder = e.Folder;
        SenderName = e.SenderName;
        SenderAddress = e.SenderAddress;
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
    /// Initializes a new instance of the ReceivedMailEntity class.
    /// </summary>
    public SentMailEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ReceivedMailEntity class.
    /// </summary>
    internal SentMailEntity(ReceivedMailEntity m, string ownerId)
    {
        FillBaseProperties(m);
        OwnerId = ownerId;
        SendTime = DateTime.Now;
        CopyConfigItself("send", "receive", "links", "preference");
    }

    /// <summary>
    /// Gets or sets the application name that sent this mail.
    /// </summary>
    [DataMember(Name = "app")]
    [JsonPropertyName("app")]
    [Column("app")]
    public string ApplicationName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
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
    /// Creates the receive mails.
    /// </summary>
    /// <returns>A collection of receive mail.</returns>
    public IEnumerable<ReceivedMailEntity> ToReceiveMails()
    {
        var addr = AddressList?.GetAllMailAddresses();
        var currentUserId = OwnerId;
        if (addr == null || string.IsNullOrWhiteSpace(currentUserId)) return new List<ReceivedMailEntity>();
        return addr.Select(ele =>
        {
            if (string.IsNullOrWhiteSpace(ele.UserId)) return null;
            return new ReceivedMailEntity(this, ele.UserId)
            {
                TargetId = currentUserId,
                ThreadId = ThreadId ?? Id
            };
        }).Where(ele => ele != null);
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
        OwnerId = ownerId;
        SendTime = DateTime.Now;
        CopyConfigItself("send", "receive", "links", "preference");
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
    [NotMapped]
    [JsonPropertyName("flag")]
    [JsonConverter(typeof(JsonIntegerEnumCompatibleConverter<MailFlags>))]
    public MailFlags Flag
    {
        get => GetCurrentProperty<MailFlags>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the flag code.
    /// </summary>
    [JsonIgnore]
    [DataMember(Name = "flag")]
    [Column("flag")]
    public int FlagCode
    {
        get => (int)Flag;
        set => Flag = (MailFlags)value;
    }

    /// <summary>
    /// Creates the sent mails.
    /// </summary>
    /// <param name="clearRecipients">true if for foward; otherwise, false.</param>
    /// <returns>A collection of sent mail.</returns>
    public SentMailEntity ToSentMails(bool clearRecipients)
    {
        var m = new SentMailEntity(this, TargetId)
        {
            ThreadId = ThreadId ?? Id
        };
        if (clearRecipients)
        {
            m.AddressList = new MailAddressListInfo();
            return m;
        }

        if (m.AddressList == null) m.AddressList = new MailAddressListInfo();
        var reply = m.AddressList.Reply;
        var to = m.AddressList.To?.ToList() ?? new List<MailAddressInfo>();
        m.AddressList.To = to;
        var sender = new MailAddressInfo
        {
            Name = m.SenderName,
            Address = m.SenderAddress,
            UserId = TargetId
        };
        to.Insert(0, sender);
        m.AddressList.Reply = null;
        if (!string.IsNullOrWhiteSpace(reply))
        {
            var containReply = false;
            foreach (var ele in to)
            {
                if (ele?.Address != reply) continue;
                containReply = true;
                break;
            }

            if (!containReply) sender.Address = reply;
        }

        return m;
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
