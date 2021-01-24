// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryArgs.cs" company="Nanchang Jinchen Software Co., Ltd.">
//   Copyright (c) 2010 Nanchang Jinchen Software Co., Ltd. All rights reserved.
// </copyright>
// <summary>
//   The query model for basic commonly used request.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The query arguments.
    /// </summary>
    [DataContract]
    public class QueryArgs : IEquatable<QueryArgs>
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
        public int Count { get; set; } = ResourceEntityExtensions.PageSize;

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

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        [DataMember(Name = "order")]
        [JsonPropertyName("order")]
        public ResourceEntityOrders Order { get; set; } = ResourceEntityOrders.Default;

        /// <summary>
        /// Gets the query condition.
        /// </summary>
        /// <returns>The string condition instance of query name.</returns>
        public Trivial.Data.StringCondition GetQueryCondition()
        {
            if (string.IsNullOrWhiteSpace(NameQuery)) return null;
            return new Trivial.Data.StringCondition(NameExactly ? Trivial.Data.DbCompareOperator.Equal : Trivial.Data.DbCompareOperator.Contains, NameQuery);
        }

        /// <summary>
        /// Gets the index interval.
        /// </summary>
        /// <returns>The interval instance of index.</returns>
        public Trivial.Maths.StructValueSimpleInterval<int> GetIndexInterval()
        {
            return new Trivial.Maths.StructValueSimpleInterval<int>(Offset, Offset + Count, false, true);
        }

        /// <summary>
        /// Returns a string that represents the current query arguments.
        /// </summary>
        /// <returns>A string format instance.</returns>
        public override string ToString()
        {
            return ((QueryData)this).ToString();
        }

        /// <summary>
        /// Write as UTF-8 JSON.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer.</param>
        public void WriteTo(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            if (!string.IsNullOrWhiteSpace(NameQuery))
            {
                writer.WriteString("q", NameQuery);
                writer.WriteBoolean("eq_name", NameExactly);
            }

            writer.WriteNumber("offset", Offset);
            writer.WriteNumber("count", Count);
            writer.WriteNumber("state", (int)State);
            writer.WriteNumber("order", (int)Order);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is QueryArgs q) return Equals(q);
            if (obj is QueryData d) return Equals(d);
            return base.Equals(obj);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(QueryArgs obj)
        {
            if (obj is null) return false;
            return obj.NameQuery == NameQuery && obj.NameExactly == NameExactly && obj.Offset == Offset && obj.Count == Count && obj.State == State && obj.Order == Order;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Converts to query argumentse.
        /// </summary>
        /// <param name="q">The query data to convert.</param>
        /// <returns>The query arguments.</returns>
        public static implicit operator QueryArgs(QueryData q)
        {
            if (q == null) return null;
            var result = new QueryArgs
            {
                NameQuery = q.GetValue("q") ?? q.GetValue("name"),
                NameExactly = q.GetValue("eq_name")?.ToLowerInvariant() == JsonBoolean.TrueString,
                Offset = q.TryGetInt32Value("offset") ?? 0,
                Count = q.TryGetInt32Value("count") ?? ResourceEntityExtensions.PageSize
            };
            var stateNum = q.TryGetInt32Value("state");
            if (stateNum.HasValue)
            {
                result.State = (ResourceEntityStates)stateNum.Value;
            }
            else
            {
                var stateStr = q.GetValue("state");
                if (!string.IsNullOrWhiteSpace(stateStr) && Enum.TryParse<ResourceEntityStates>(stateStr, true, out var stateResult))
                    result.State = stateResult;
            }

            stateNum = q.TryGetInt32Value("order");
            if (stateNum.HasValue)
            {
                result.Order = (ResourceEntityOrders)stateNum.Value;
            }
            else
            {
                var stateStr = q.GetValue("order");
                if (!string.IsNullOrWhiteSpace(stateStr) && Enum.TryParse<ResourceEntityOrders>(stateStr, true, out var stateResult))
                    result.Order = stateResult;
            }

            return result;
        }

        /// <summary>
        /// Converts an instance of query arguments to query data.
        /// </summary>
        /// <param name="q">The query arguments to convert.</param>
        /// <returns>The query data.</returns>
        public static explicit operator QueryData(QueryArgs q)
        {
            if (q == null) return null;
            return new QueryData
            {
                { "q", q.NameQuery },
                { "eq_name", q.NameExactly },
                { "offset", q.Offset.ToString("g") },
                { "count", q.Count.ToString("g") },
                { "state", ((int)q.State).ToString("g") },
                { "order", ((int)q.Order).ToString("g") }
            };
        }

        /// <summary>
        /// Converts an instance of query arguments to JSON object.
        /// </summary>
        /// <param name="q">The query arguments to convert.</param>
        /// <returns>The JSON object.</returns>
        public static explicit operator JsonObject(QueryArgs q)
        {
            if (q == null) return null;
            return new JsonObject
            {
                { "q", q.NameQuery },
                { "eq_name", q.NameExactly },
                { "offset", q.Offset.ToString("g") },
                { "count", q.Count.ToString("g") },
                { "state", ((int)q.State).ToString("g") },
                { "order", ((int)q.Order).ToString("g") }
            };
        }

        /// <summary>
        /// Compares two instances to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator ==(QueryArgs leftValue, QueryArgs rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return true;
            if (rightValue is null || leftValue is null) return false;
            return leftValue.NameQuery == rightValue.NameQuery && leftValue.NameExactly == rightValue.NameExactly && leftValue.Offset == rightValue.Offset && leftValue.Count == rightValue.Count && leftValue.State == rightValue.State && leftValue.Order == rightValue.Order;
        }

        /// <summary>
        /// Compares two instances to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator !=(QueryArgs leftValue, QueryArgs rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return false;
            if (rightValue is null || leftValue is null) return true;
            return leftValue.NameQuery != rightValue.NameQuery || leftValue.NameExactly != rightValue.NameExactly || leftValue.Offset != rightValue.Offset || leftValue.Count != rightValue.Count || leftValue.State != rightValue.State || leftValue.Order != rightValue.Order;
        }

        /// <summary>
        /// Parses query arguments.
        /// </summary>
        /// <param name="s">The input string to parse.</param>
        /// <returns>An instance of query arguments</returns>
        public static QueryArgs Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (!s.StartsWith("{"))
            {
                if (s.IndexOf("=") < 0 && s.IndexOf(":") < 0) return new QueryArgs { NameQuery = s };
                try
                {
                    return QueryData.Parse(s);
                }
                catch (ArgumentException)
                {
                }
                catch (FormatException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }

            var json = JsonObject.Parse(s);
            return Parse(json);
        }

        /// <summary>
        /// Parses query arguments.
        /// </summary>
        /// <param name="json">The JSON object.</param>
        /// <returns>An instance of query arguments</returns>
        public static QueryArgs Parse(JsonObject json)
        {
            return new QueryArgs
            {
                NameQuery = json.GetStringValue("q") ?? json.GetStringValue("name"),
                NameExactly = json.TryGetBooleanValue("eq_name") ?? false,
                Offset = json.TryGetInt32Value("offset") ?? 0,
                Count = json.TryGetInt32Value("count") ?? ResourceEntityExtensions.PageSize,
                State = json.TryGetEnumValue<ResourceEntityStates>("state", true) ?? ResourceEntityStates.Normal,
                Order = json.TryGetEnumValue<ResourceEntityOrders>("order", true) ?? ResourceEntityOrders.Default
            };
        }
    }

    /// <summary>
    /// Json number converter with number string fallback.
    /// </summary>
    public sealed class QueryArgsConverter : JsonConverter<QueryArgs>
    {
        /// <inheritdoc />
        public override QueryArgs Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.Number => reader.TryGetUInt16(out var num) ? new QueryArgs { Count = num } : null,
                JsonTokenType.String => QueryArgs.Parse(reader.GetString()),
                JsonTokenType.False => null,
                JsonTokenType.True => new QueryArgs(),
                JsonTokenType.StartObject => ParseJson(ref reader),
                _ => throw new JsonException($"The token type is {reader.TokenType} but JSON object.")
            };
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, QueryArgs value, JsonSerializerOptions options)
        {
            if (value == null) return;
            writer.WriteStartObject();
            if (!string.IsNullOrWhiteSpace(value.NameQuery))
            {
                writer.WriteString("q", value.NameQuery);
                writer.WriteBoolean("eq_name", value.NameExactly);
            }

            writer.WriteNumber("offset", value.Offset);
            writer.WriteNumber("count", value.Count);
            writer.WriteNumber("state", (int)value.State);
            writer.WriteNumber("order", (int)value.Order);
            writer.WriteEndObject();
        }

        private static QueryArgs ParseJson(ref Utf8JsonReader reader)
        {
            var json = JsonObject.ParseValue(ref reader);
            return QueryArgs.Parse(json);
        }
    }
}
