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
        public string NameQuery { get; set; }

        public bool NameExactly { get; set; }

        public int Count { get; set; } = 100;

        public int Offset { get; set; } = 0;
    }
}
