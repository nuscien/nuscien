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
        public string NameQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the name is exact to search.
        /// </summary>
        public bool NameExactly { get; set; }

        /// <summary>
        /// Gets or sets the maximum count to return.
        /// </summary>
        public int Count { get; set; } = 100;

        /// <summary>
        /// Gets or sets the offset to return.
        /// </summary>
        public int Offset { get; set; } = 0;
    }
}
