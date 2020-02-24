// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataValue.cs" company="Nanchang Jinchen Software Co., Ltd.">
//   Copyright (c) 2010 Nanchang Jinchen Software Co., Ltd. All rights reserved.
// </copyright>
// <summary>
//   The data value for definition for a list row or a table column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using Trivial.Security;

namespace NuScien.Data
{
    /// <summary>
    /// The sources of data form value.
    /// </summary>
    public enum DataValueSource
    {
        /// <summary>
        /// The static source.
        /// </summary>
        Static = 0,

        /// <summary>
        /// The text source.
        /// </summary>
        Text = 1,

        /// <summary>
        /// The source of data linking with property path.
        /// </summary>
        Data = 2,

        /// <summary>
        /// The source linked by url.
        /// </summary>
        Url = 3,

        /// <summary>
        /// The source provided by request job.
        /// </summary>
        Job = 4,

        /// <summary>
        /// The function name.
        /// </summary>
        Function = 5
    }

    /// <summary>
    /// The types of data form field.
    /// </summary>
    public enum DataFieldType
    {
        /// <summary>
        /// The hidden field.
        /// </summary>
        Hidden = 0,

        /// <summary>
        /// The simple content field.
        /// </summary>
        SimpleContent = 1,

        /// <summary>
        /// The specific function provider field.
        /// </summary>
        Function = 2,

        /// <summary>
        /// The normal text field.
        /// </summary>
        Text = 3,

        /// <summary>
        /// The email field.
        /// </summary>
        Email = 4,

        /// <summary>
        /// The phone field.
        /// </summary>
        Phone = 5,

        /// <summary>
        /// The password field.
        /// </summary>
        Password = 6,

        /// <summary>
        /// The date field.
        /// </summary>
        Date = 7,

        /// <summary>
        /// The time field.
        /// </summary>
        Time = 8,

        /// <summary>
        /// The date and time field.
        /// </summary>
        DateTime = 9,

        /// <summary>
        /// The integer number field.
        /// </summary>
        Integer = 10,

        /// <summary>
        /// The decimal number field.
        /// </summary>
        Decimal = 11,

        /// <summary>
        /// The person name field.
        /// </summary>
        Name = 12,

        /// <summary>
        /// The single selection field.
        /// </summary>
        Single = 13,

        /// <summary>
        /// The tags field.
        /// </summary>
        Tags = 14,

        /// <summary>
        /// The checkbox field.
        /// </summary>
        Checkbox = 15,

        /// <summary>
        /// The richtext field.
        /// </summary>
        Richtext = 16,

        /// <summary>
        /// The image file field.
        /// </summary>
        Image = 17,

        /// <summary>
        /// The music file field.
        /// </summary>
        Music = 18,

        /// <summary>
        /// The movie file field.
        /// </summary>
        Movie = 19,

        /// <summary>
        /// The documentation file field.
        /// </summary>
        Doc = 20,

        /// <summary>
        /// The zip file field.
        /// </summary>
        Zip = 21,

        /// <summary>
        /// The other type of field.
        /// </summary>
        Other = 63
    }

    /// <summary>
    /// Customized value reference.
    /// </summary>
    public class CustomizedValueReference
    {
        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        public virtual object Value { get; set; }

        /// <summary>
        /// Gets or sets the data source of the field.
        /// </summary>
        public virtual DataValueSource Source { get; set; }

        /// <summary>
        /// Gets or sets the data source of the field.
        /// </summary>
        public string SourceString { get { return Source.ToString().ToLower(); } }
    }

    /// <summary>
    /// Customized value reference.
    /// </summary>
    public class DataValueReference : CustomizedValueReference
    {
        /// <summary>
        /// Gets or sets the data source of the field.
        /// </summary>
        public override DataValueSource Source { get { return DataValueSource.Data; } }

        /// <summary>
        /// Gets or sets a value indicating whether the source is from original.
        /// </summary>
        public bool IsFromOriginal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source is only an appending parameter.
        /// </summary>
        public bool IsAppendParameter { get; set; }
    }

    /// <summary>
    /// The change method result.
    /// </summary>
    [DataContract]
    public class ChangeMethodResult
    {
        /// <summary>
        /// Initializes a new instance of the ChangeMethodResult class.
        /// </summary>
        public ChangeMethodResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChangeMethodResult class.
        /// </summary>
        /// <param name="state">The change method result.</param>
        public ChangeMethodResult(Trivial.Data.ChangeMethods state)
        {
            State = state;
        }

        /// <summary>
        /// Gets or sets the change method result.
        /// </summary>
        [DataMember(Name = "state")]
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Trivial.Data.ChangeMethods State { get; set; }
    }

    /// <summary>
    /// The collection result.
    /// </summary>
    [DataContract]
    public class CollectionResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the CollectionResult class.
        /// </summary>
        public CollectionResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CollectionResult class.
        /// </summary>
        /// <param name="col">The result collection.</param>
        /// <param name="offset">The offset of the result.</param>
        public CollectionResult(IEnumerable<T> col, int? offset = null)
        {
            Value = col;
            Offset = offset;
        }

        /// <summary>
        /// Gets or sets the offset of the result.
        /// </summary>
        [DataMember(Name = "offset")]
        [JsonPropertyName("offset")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the result collection.
        /// </summary>
        [DataMember(Name = "col")]
        [JsonPropertyName("col")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IEnumerable<T> Value { get; set; }
    }

    /// <summary>
    /// The collection result.
    /// </summary>
    [DataContract]
    public class MessageResult
    {
        /// <summary>
        /// Initializes a new instance of the MessageResult class.
        /// </summary>
        public MessageResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MessageResult class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageResult(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets or sets the offset of the result.
        /// </summary>
        [DataMember(Name = "message")]
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// The collection result.
    /// </summary>
    [DataContract]
    public class ErrorMessageResult : MessageResult
    {
        /// <summary>
        /// Initializes a new instance of the ErrorMessageResult class.
        /// </summary>
        public ErrorMessageResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ErrorMessageResult class.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public ErrorMessageResult(Exception ex) : this(ex, ex?.GetType()?.Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ErrorMessageResult class.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="errorCode">The error code.</param>
        public ErrorMessageResult(Exception ex, string errorCode) : base(ex?.Message)
        {
            ErrorCode = errorCode;
            if (ex == null) return;
            var innerEx = ex?.InnerException;
            if (ex is AggregateException aggEx && aggEx.InnerExceptions != null)
            {
                if (aggEx.InnerExceptions.Count == 1)
                {
                    innerEx = aggEx.InnerExceptions[0];
                }
                else
                {
                    Details = aggEx.InnerExceptions.Select(ele => ele?.Message).Where(ele => ele != null).ToList();
                    return;
                }
            }

            if (innerEx == null) return;
            Details = new List<string>
            {
                innerEx.Message
            };
            var msg = innerEx.InnerException?.Message;
            if (string.IsNullOrWhiteSpace(msg)) return;
            Details.Add(msg);
            msg = innerEx.InnerException.InnerException?.Message;
            if (string.IsNullOrWhiteSpace(msg)) return;
            Details.Add(msg);
        }

        /// <summary>
        /// Initializes a new instance of the ErrorMessageResult class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ErrorMessageResult(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ErrorMessageResult class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorCode">The error code.</param>
        public ErrorMessageResult(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets or sets the offset of the result.
        /// </summary>
        [DataMember(Name = TokenInfo.ErrorCodeProperty)]
        [JsonPropertyName(TokenInfo.ErrorCodeProperty)]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the offset of the result.
        /// </summary>
        [DataMember(Name = "details")]
        [JsonPropertyName("details")]
        public List<string> Details { get; set; }
    }
}
