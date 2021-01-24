// --------------------------------------------------------------------------------------------------------------------
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
        /// Forbidden, unauthorization or other security issue.
        /// </summary>
        Security = 2,

        /// <summary>
        /// The key is invalid.
        /// </summary>
        Key = 3,

        /// <summary>
        /// The resource is not found.
        /// </summary>
        NotFound = 4,

        /// <summary>
        /// The data provider works incorrecly.
        /// </summary>
        Provider = 5,

        /// <summary>
        /// Timeout.
        /// </summary>
        Timeout = 6,

        /// <summary>
        /// Unknown issue or internal error.
        /// </summary>
        Unknown = 7
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
        public ChangeMethodResult(ChangeMethods state)
        {
            State = state;
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
            : this(state, ex?.Message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChangeMethodResult class.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public ChangeMethodResult(Exception ex)
        {
            State = ChangeMethods.Invalid;
            if (ex == null) return;
            Message = ex.Message;
        }

        /// <summary>
        /// Gets or sets the change method result.
        /// </summary>
        [DataMember(Name = "state")]
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChangeMethods State { get; set; }

        /// <summary>
        /// Gets or sets the change method result.
        /// </summary>
        [DataMember(Name = "message")]
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
