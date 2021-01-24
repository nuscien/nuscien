﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeMethodResult.cs" company="Nanchang Jinchen Software Co., Ltd.">
//   Copyright (c) 2010 Nanchang Jinchen Software Co., Ltd. All rights reserved.
// </copyright>
// <summary>
//   The change method result model and its accessories.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

using Trivial.Data;
using Trivial.Security;

namespace NuScien.Data
{
    /// <summary>
    /// The kinds of change error.
    /// </summary>
    public enum ChangeErrorKinds
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// Invalid argument or bad request.
        /// </summary>
        Argument = 1,

        /// <summary>
        /// Unauthorized access.
        /// </summary>
        Unauthorized = 2,

        /// <summary>
        /// Forbidden.
        /// </summary>
        Forbidden = 3,

        /// <summary>
        /// The source resource is not found or is gone.
        /// </summary>
        NotFound = 4,

        /// <summary>
        /// The key is out of range or invalid.
        /// </summary>
        Key = 5,

        /// <summary>
        /// The data provider works incorrectly.
        /// </summary>
        Provider = 6,

        /// <summary>
        /// The service is busy or the request is rejected.
        /// </summary>
        Busy = 7,

        /// <summary>
        /// Cancellation request.
        /// </summary>
        Canceled = 8,

        /// <summary>
        /// Timeout.
        /// </summary>
        Timeout = 9,

        /// <summary>
        /// Internal service error.
        /// </summary>
        Service = 10,

        /// <summary>
        /// Other special error defined by application.
        /// </summary>
        Application = 11
    }

    /// <summary>
    /// The exception for changing failure.
    /// </summary>
    public class FailedChangeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the FailedChangeException class.
        /// </summary>
        public FailedChangeException()
        {
            Kind = ChangeErrorKinds.Service;
        }

        /// <summary>
        /// Initializes a new instance of the FailedChangeException class.
        /// </summary>
        /// <param name="kind">The error kind.</param>
        public FailedChangeException(ChangeErrorKinds kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Initializes a new instance of the FailedChangeException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FailedChangeException(string message)
            : base(message)
        {
            Kind = ChangeErrorKinds.Service;
        }

        /// <summary>
        /// Initializes a new instance of the FailedChangeException class.
        /// </summary>
        /// <param name="kind">The error kind.</param>
        /// <param name="message">The message that describes the error.</param>
        public FailedChangeException(ChangeErrorKinds kind, string message)
            : base(message)
        {
            Kind = kind;
        }

        /// <summary>
        /// Initializes a new instance of the FailedChangeException class.
        /// </summary>
        /// <param name="kind">The error kind.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
        public FailedChangeException(ChangeErrorKinds kind, string message, Exception innerException)
            : base(message, innerException)
        {
            Kind = kind;
        }

        /// <summary>
        /// Initializes a new instance of the FailedChangeException class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected FailedChangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null) return;
            var kind = info.GetInt32(nameof(Kind));
            if (kind < 0) return;
            Kind = (ChangeErrorKinds)kind;
        }

        /// <summary>
        /// Gets the error kind.
        /// </summary>
        public ChangeErrorKinds Kind { get; }
    }

    /// <summary>
    /// The change method result.
    /// </summary>
    [DataContract]
    public class ChangeMethodResult
    {
        private readonly Exception exception;

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
        public ChangeMethodResult(ChangeMethods state)
        {
            State = state;
            if (state == ChangeMethods.Invalid) ErrorCode = ChangeErrorKinds.Service;
        }

        /// <summary>
        /// Initializes a new instance of the ChangeMethodResult class.
        /// </summary>
        /// <param name="state">The change method result.</param>
        /// <param name="message">The message.</param>
        public ChangeMethodResult(ChangeMethods state, string message)
            : this(state)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the ChangeMethodResult class.
        /// </summary>
        /// <param name="state">The change method result.</param>
        /// <param name="ex">The exception.</param>
        public ChangeMethodResult(ChangeMethods state, Exception ex)
            : this(ex)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the ChangeMethodResult class.
        /// </summary>
        /// <param name="code">The change eror kind.</param>
        /// <param name="message">The message.</param>
        public ChangeMethodResult(ChangeErrorKinds code, string message)
            : this()
        {
            State = ChangeMethods.Invalid;
            Message = message;
            ErrorCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the ChangeMethodResult class.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public ChangeMethodResult(Exception ex)
        {
            exception = ex;
            State = ChangeMethods.Invalid;
            ErrorCode = ChangeErrorKinds.Service;
            if (ex == null) return;
            Message = ex.Message;
            if (ex is System.Security.SecurityException) ErrorCode = ChangeErrorKinds.Forbidden;
            else if (ex is UnauthorizedAccessException) ErrorCode = ChangeErrorKinds.Unauthorized;
            else if (ex is ArgumentException) ErrorCode = ChangeErrorKinds.Argument;
            else if (ex is TimeoutException) ErrorCode = ChangeErrorKinds.Timeout;
            else if (ex is OperationCanceledException) ErrorCode = ChangeErrorKinds.Canceled;
            else if (ex is System.Data.Common.DbException) ErrorCode = ChangeErrorKinds.Provider;
            else if (ex is Trivial.Net.FailedHttpException) ErrorCode = ChangeErrorKinds.Provider;
        }

        /// <summary>
        /// Gets or sets the change method result.
        /// </summary>
        [DataMember(Name = "state")]
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChangeMethods State { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [DataMember(Name = "code")]
        [JsonPropertyName("code")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChangeErrorKinds ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the change method result.
        /// </summary>
        [DataMember(Name = "message")]
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is any thing different.
        /// </summary>
        [JsonIgnore]
        public bool IsSomethingChanged => State switch
        {
            ChangeMethods.Add => true,
            ChangeMethods.Update => true,
            ChangeMethods.MemberModify => true,
            ChangeMethods.Remove => true,
            _ => false,
        };

        /// <summary>
        /// Gets a value indicating whether it is successful.
        /// </summary>
        [JsonIgnore]
        public bool IsSuccessful => ResourceEntityExtensions.IsSuccessful(State);

        /// <summary>
        /// Throws an exception if changes failed.
        /// </summary>
        /// <param name="failedChangeEx">true if throw FailedChangeException only; otherwise, false.</param>
        public void ThrowIfInvalid(bool failedChangeEx = false)
        {
            var ex = GetException(failedChangeEx);
            if (ex == null) return;
            throw ex;
        }

        /// <summary>
        /// Throws an exception if changes failed.
        /// </summary>
        /// <param name="failedChangeEx">true if throw FailedChangeException only; otherwise, false.</param>
        public Exception GetException(bool failedChangeEx = false)
        {
            if (State != ChangeMethods.Invalid && State != ChangeMethods.Unknown) return null;
            if (ErrorCode == ChangeErrorKinds.None) return null;
            var message = $"Failed change. ({State})";
            if (exception != null) return failedChangeEx ? new FailedChangeException(ErrorCode, Message ?? message, exception) : exception;
            Exception innerEx = failedChangeEx ? new InvalidOperationException(message) : new FailedChangeException(ErrorCode, message);
            Exception ex = ErrorCode switch
            {
                ChangeErrorKinds.Argument => new ArgumentException(Message ?? "Argument is invalid or bad request.", innerEx),
                ChangeErrorKinds.Unauthorized => new UnauthorizedAccessException(Message ?? "Unauthorized access.", innerEx),
                ChangeErrorKinds.Forbidden => new System.Security.SecurityException(Message ?? "Forbidden.", innerEx),
                ChangeErrorKinds.NotFound => new InvalidOperationException(Message ?? "The source resource is not found or is gone.", innerEx),
                ChangeErrorKinds.Key => new ArgumentException(Message ?? "The key is out of range or invalid", innerEx),
                ChangeErrorKinds.Provider => new InvalidOperationException(Message ?? "The data provider does not work as expect.", innerEx),
                ChangeErrorKinds.Busy => new TimeoutException(Message ?? "The service is busy or the request is rejected.", innerEx),
                ChangeErrorKinds.Canceled => new OperationCanceledException(Message ?? "The operation is canceled.", innerEx),
                ChangeErrorKinds.Timeout => new TimeoutException(Message ?? "The operation is timeout.", innerEx),
                ChangeErrorKinds.Service => new InvalidOperationException(Message ?? "Something wrong.", innerEx),
                ChangeErrorKinds.Application => new InvalidOperationException(Message ?? "Application error.", innerEx),
                _ => new InvalidOperationException(Message ?? "Unknown error.", innerEx),
            };
            return failedChangeEx ? new FailedChangeException(ErrorCode, Message ?? message, ex) : ex;
        }
    }
}
