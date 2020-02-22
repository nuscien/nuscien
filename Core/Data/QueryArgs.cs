using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// Base entity information.
    /// </summary>
    [DataContract]
    public class QueryArgs
    {
        /// <summary>
        /// Gets or sets the name query.
        /// </summary>
        [DataMember(Name = "q")]
        [JsonPropertyName("q")]
        public string NameQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the name is exact to search.
        /// </summary>
        [DataMember(Name = "eq_name")]
        [JsonPropertyName("eq_name")]
        public bool NameExactly { get; set; }

        /// <summary>
        /// Gets or sets the maximum count to return.
        /// </summary>
        [DataMember(Name = "count")]
        [JsonPropertyName("count")]
        public int Count { get; set; } = 100;

        /// <summary>
        /// Gets or sets the offset to return.
        /// </summary>
        [DataMember(Name = "offset")]
        [JsonPropertyName("offset")]
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the resource entity state.
        /// </summary>
        [DataMember(Name = "state")]
        [JsonPropertyName("state")]
        public ResourceEntityStates State { get; set; } = ResourceEntityStates.Normal;
    }
}
