using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Reflection
{
    /// <summary>
    /// The extensions for resource entity.
    /// </summary>
    internal static class InternalAssertion
    {
        internal static QueryArgs DefaultQueryArgs = new QueryArgs();

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

        internal static string Range(this string value, int start, int? end = null)
        {
            if (value == null) return null;
#if NETSTANDARD2_0 || NETFRAMEWORK
            else if (start < 0) start = value.Length + start;
            if (!end.HasValue) end = value.Length - 1;
            else if (end.Value < 0) end = value.Length + end;
            return value.Substring(start, end.Value - start + 1);
#else
            if (end.HasValue)
            {
                if (start > 0)
                    return end.Value > 0 ? value[start..end.Value] : value[start..^end.Value];
                return end.Value > 0 ? value[^start..end.Value] : value[^start..^end.Value];
            }

            return start > 0 ? value[start..] : value[^start..];
#endif
        }

#if NETSTANDARD2_0 || NETFRAMEWORK
        internal static bool StartsWith(this string value, char c)
        {
            return value.StartsWith(c);
        }

        internal static bool EndsWith(this string value, char c)
        {
            return value.EndsWith(c);
        }
#endif

        internal static bool TryParse(Type enumType, string value, out object result)
        {
#if NETSTANDARD2_0 || NETFRAMEWORK
            try
            {
                result = Enum.Parse(enumType, value);
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (OverflowException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (FormatException)
            {
            }

            result = null;
            return false;
#else
            return Enum.TryParse(enumType, value, out result);
#endif
        }
    }
}
