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

namespace Trivial.Reflection
{
    /// <summary>
    /// The extensions for resource entity.
    /// </summary>
    internal static class InternalAssertion
    {
        internal static void IsNotNull<T>(IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source), "source was null.");
        }

        internal static void IsNotNullOrWhiteSpace(string s, string name)
        {
            if (s == null) throw new ArgumentNullException(name, name + " was null.");
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException(name + " was empty or consists only of white-space characters.", name);
        }

        internal static void IsNotNullOrEmpty(string s, string name)
        {
            if (string.IsNullOrEmpty(s)) throw new ArgumentNullException(name, name + " was null or an empty string.");
        }

        internal static void IsNotNull(object obj, string name)
        {
            if (obj == null) throw new ArgumentNullException(name, name + " was null.");
        }
    }
}
