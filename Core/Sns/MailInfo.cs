﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
using Trivial.Collection;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Web;

namespace NuScien.Sns;

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
/// The mail address information.
/// </summary>
public class MailAddressInfo
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the contact identifier.
    /// </summary>
    [JsonPropertyName("contact")]
    public string ContactId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [JsonPropertyName("user")]
    public string UserId { get; set; }

    /// <summary>
    /// Converts an instance to mail address.
    /// </summary>
    /// <param name="m">The mail address information instance to convert.</param>
    /// <returns>The mail address.</returns>
    public static explicit operator System.Net.Mail.MailAddress(MailAddressInfo m)
    {
        if (m == null) return null;
        return new System.Net.Mail.MailAddress(m.Address, m.Name);
    }

    /// <summary>
    /// Converts mail address.
    /// </summary>
    /// <param name="m">The mail address to convert.</param>
    /// <returns>The mail address instance converted.</returns>
    public static explicit operator MailAddressInfo(System.Net.Mail.MailAddress m)
    {
        if (m == null) return null;
        return new MailAddressInfo
        {
            Name = m.DisplayName,
            Address = m.Address
        };
    }
}

/// <summary>
/// The mail address list.
/// </summary>
public class MailAddressListInfo
{
    /// <summary>
    /// Gets or sets the receiver address list.
    /// </summary>
    [JsonPropertyName("to")]
    public IEnumerable<MailAddressInfo> To { get; set; }

    /// <summary>
    /// Gets or sets the address list to carbon copy.
    /// </summary>
    [JsonPropertyName("cc")]
    public IEnumerable<MailAddressInfo> Cc { get; set; }

    /// <summary>
    /// Gets or sets the reply address.
    /// </summary>
    [JsonPropertyName("re")]
    public string Reply { get; set; }

    /// <summary>
    /// Gets all of mail address to send.
    /// </summary>
    /// <returns>The mail address collection.</returns>
    public IEnumerable<MailAddressInfo> GetAllMailAddresses()
    {
        return GetAllMailAddresses(null);
    }

    /// <summary>
    /// Gets all of mail address to send.
    /// </summary>
    /// <param name="others">The other mail address collection.</param>
    /// <returns>The mail address collection.</returns>
    public IEnumerable<MailAddressInfo> GetAllMailAddresses(IEnumerable<MailAddressInfo> others)
    {
        var list = new List<MailAddressInfo>();
        var col = To;
        if (col != null) list.AddRange(col);
        col = Cc;
        if (col != null) list.AddRange(col);
        if (others != null) list.AddRange(others);
        return list;
    }
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
    public IEnumerable<MailAddressInfo> Bcc { get; set; }
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
/// The additional filter for mail.
/// </summary>
public class MailAdditionalFilterInfo
{
    /// <summary>
    /// Gets or sets the mail thread identifier.
    /// </summary>
    [JsonPropertyName("thread")]
    public string ThreadId { get; set; }

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    [JsonPropertyName("addr")]
    public string SenderAddress { get; set; }

    /// <summary>
    /// Gets or sets the date time when send.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? SendStartTime { get; set; }

    /// <summary>
    /// Gets or sets the date time when send.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? SendEndTime { get; set; }

    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    [JsonPropertyName("priority")]
    public BasicPriorities? Priority { get; set; }

    /// <summary>
    /// Gets or sets the flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public MailFlags? Flag { get; set; }

    /// <summary>
    /// Converts to query data.
    /// </summary>
    /// <returns>The query data.</returns>
    public virtual QueryData ToQueryData(QueryData q)
    {
        if (q == null) q = new QueryData();
        if (string.IsNullOrWhiteSpace(ThreadId)) q.Set("thread", ThreadId);
        if (string.IsNullOrWhiteSpace(SenderAddress)) q.Set("addr", SenderAddress);
        if (SendStartTime.HasValue) q.Set("start", WebFormat.ParseDate(SendStartTime.Value).ToString("g", CultureInfo.InvariantCulture));
        if (SendEndTime.HasValue) q.Set("end", WebFormat.ParseDate(SendEndTime.Value).ToString("g", CultureInfo.InvariantCulture));
        if (Priority.HasValue) q.Set("priority", ((int)Priority.Value).ToString("g", CultureInfo.InvariantCulture));
        if (Flag.HasValue) q.Set("flag", ((int)Flag.Value).ToString("g", CultureInfo.InvariantCulture));
        return q;
    }

    /// <summary>
    /// Converts to query data.
    /// </summary>
    /// <returns>The query data.</returns>
    public virtual QueryData ToQueryData(QueryArgs q)
    {
        return ToQueryData(q != null ? (QueryData)q : null);
    }

    /// <summary>
    /// Converts to query data.
    /// </summary>
    /// <returns>The query data.</returns>
    public virtual QueryData ToQueryData()
    {
        return ToQueryData(new QueryData());
    }

    /// <summary>
    /// Filters the collection.
    /// </summary>
    /// <param name="col">The collection.</param>
    /// <returns>A collection after filter.</returns>
    public virtual IQueryable<ReceivedMailEntity> Where(IQueryable<ReceivedMailEntity> col)
    {
        if (string.IsNullOrWhiteSpace(ThreadId)) col = col.Where(ele => ele.ThreadId == ThreadId);
        if (string.IsNullOrWhiteSpace(SenderAddress)) col = col.Where(ele => ele.SenderAddress == SenderAddress);
        if (SendStartTime.HasValue)
        {
            var d = SendStartTime.Value;
            col = col.Where(ele => ele.SendTime >= d);
        }

        if (SendEndTime.HasValue)
        {
            var d = SendEndTime.Value;
            col = col.Where(ele => ele.SendTime <= d);
        }

        if (Priority.HasValue)
        {
            var i = (int)Priority.Value;
            col = col.Where(ele => ele.PriorityCode == i);
        }

        if (Flag.HasValue)
        {
            var i = (int)Flag.Value;
            col = col.Where(ele => ele.FlagCode == i);
        }

        return col;
    }
}
