﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Reflection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data;

/// <summary>
/// The base configurable resource entity.
/// </summary>
[DataContract]
public abstract class ConfigurableResourceEntity : BaseResourceEntity
{
    private string config;
    private JsonObjectNode json;

    /// <summary>
    /// Gets or sets the additional message.
    /// </summary>
    [JsonPropertyName("config")]
    [NotMapped]
    public JsonObjectNode Config
    {
        get
        {
            var s = config;
            if (string.IsNullOrEmpty(s)) return json;
            try
            {
                json = ParseJson(s);
                SetProperty("Config", json);
            }
            finally
            {
                config = null;
            }

            return json;
        }

        set
        {
            config = null;
            json = value;
            SetProperty("Config", json);
        }
    }

    /// <summary>
    /// Gets or sets the additional message.
    /// </summary>
    [DataMember(Name = "config")]
    [JsonIgnore]
    [Column("config")]
    public string ConfigString
    {
        get
        {
            return config ?? Config?.ToString() ?? string.Empty;
        }

        set
        {
            config = value;
            json = null;
        }
    }

    /*
    private string config;

    /// <summary>
    /// Gets or sets the additional message.
    /// </summary>
    [JsonPropertyName("config")]
    [JsonConverter(typeof(JsonObjectConverter))]
    [NotMapped]
    public JsonObject Config
    {
        get => GetCurrentProperty<JsonObject>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the additional message.
    /// </summary>
    [DataMember(Name = "config")]
    [JsonIgnore]
    [Column("config")]
    public string ConfigString
    {
        get
        {
            return Config?.ToString() ?? string.Empty;
        }

        set
        {
            config = value;
            Config = ParseJson(config);
        }
    }
    */

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public string TryGetStringConfigValue(string key)
    {
        return Config?.TryGetStringValue(key);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public int? TryGetInt32ConfigValue(string key)
    {
        return Config?.TryGetInt32Value(key);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>The value of the property.</returns>
    public bool TryGetInt32ConfigValue(string key, out int result)
    {
        var json = Config;
        if (json == null)
        {
            result = default;
            return false;
        }

        return json.TryGetInt32Value(key, out result);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public long? TryGetInt64ConfigValue(string key)
    {
        return Config?.TryGetInt64Value(key);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>The value of the property.</returns>
    public bool TryGetInt64ConfigValue(string key, out long result)
    {
        var json = Config;
        if (json == null)
        {
            result = default;
            return false;
        }

        return json.TryGetInt64Value(key, out result);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public double? TryGetDoubleConfigValue(string key)
    {
        return Config?.TryGetDoubleValue(key);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>The value of the property.</returns>
    public bool TryGetDoubleConfigValue(string key, out double result)
    {
        var json = Config;
        if (json == null)
        {
            result = default;
            return false;
        }

        return json.TryGetDoubleValue(key, out result);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public bool? TryGetBooleanConfigValue(string key)
    {
        return Config?.TryGetBooleanValue(key);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>The value of the property.</returns>
    public bool TryGetBooleanConfigValue(string key, out bool result)
    {
        var json = Config;
        if (json == null)
        {
            result = default;
            return false;
        }

        return json.TryGetBooleanValue(key, out result);
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public JsonObjectNode TryGetJsonConfigValue(string key)
    {
        var json = Config;
        if (json != null) return json.TryGetObjectValue(key);
        return default;
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public JsonArrayNode TryGetJsonArrayConfigValue(string key)
    {
        var json = Config;
        if (json != null) return json.TryGetArrayValue(key);
        return default;
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public JsonValueKind GetConfigValueKind(string key)
    {
        var json = Config;
        try
        {
            if (json != null) return json.GetValueKind(key);
        }
        catch (ArgumentException)
        {
        }

        return JsonValueKind.Undefined;
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public IJsonValueNode TryGetConfigValue(string key)
    {
        var json = Config;
        try
        {
            if (json != null) return json.GetValue(key);
        }
        catch (ArgumentException)
        {
        }

        return JsonValues.Undefined;
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>
    public T TryDeserializeConfigValue<T>(string key)
    {
        var json = Config;
        try
        {
            if (json != null) return json.DeserializeValue<T>(key);
        }
        catch (ArgumentException)
        {
        }
        catch (JsonException)
        {
        }

        return default;
    }

    /// <summary>
    /// Gets the specific configuration property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if get and deserialize succeeded; otherwise, false.</returns>
    public bool TryDeserializeConfigValue<T>(string key, out T result)
    {
        var json = Config;
        try
        {
            if (json != null)
            {
                result = json.DeserializeValue<T>(key);
                return true;
            }
        }
        catch (ArgumentException)
        {
        }
        catch (JsonException)
        {
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Sets the configuration propery value.
    /// </summary>
    /// <param name="key">The propery key.</param>
    /// <param name="value">The value to set.</param>
    public void SetConfigValue(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        var json = Config;
        if (json == null) Config = new JsonObjectNode();
        json = Config;
        if (json == null)
        {
            Config = new JsonObjectNode();
            json = Config;
            if (json == null) json = Config = new JsonObjectNode();
        }

        var cont = false;
        if (value is null)
        {
            json.SetNullValue(key);
        }
        else if (value is string s)
        {
            json.SetValue(key, s);
        }
        else if (value is IJsonValueNode j)
        {
            if (j.ValueKind == JsonValueKind.Null || j.ValueKind == JsonValueKind.Undefined)
            {
                json.SetNullValue(key);
            }
            else if (j is IJsonValueNode<string> js2)
            {
                json.SetValue(key, js2.Value);
            }
            else if (j is IJsonNumberNode jn)
            {
                if (jn.IsInteger) json.SetValue(key, jn.GetInt64());
                else json.SetValue(key, jn.GetDouble());
            }
            else if (j is IJsonValueNode<bool> jb)
            {
                json.SetValue(key, jb.Value);
            }
            else if (j is JsonObjectNode jo)
            {
                json.SetValue(key, jo);
            }
            else if (j is JsonArrayNode ja)
            {
                json.SetValue(key, ja);
            }
            else
            {
                cont = true;
            }

            if (!cont) return;
        }
        else if (value.GetType().IsValueType)
        {
            if (value is int i)
            {
                json.SetValue(key, i);
            }
            else if (value is long l)
            {
                json.SetValue(key, l);
            }
            else if (value is double d)
            {
                json.SetValue(key, d);
            }
            else if (value is float f)
            {
                json.SetValue(key, f);
            }
            else if (value is bool b)
            {
                json.SetValue(key, b);
            }
            else if (value is DateTime t)
            {
                json.SetValue(key, t);
            }
            else if (value is Guid g)
            {
                json.SetValue(key, g);
            }
            else if (value is uint ui)
            {
                json.SetValue(key, ui);
            }
            else if (value is ulong ul)
            {
                json.SetValue(key, (double)ul);
            }
            else
            {
                cont = true;
            }

            if (!cont) return;
        }
        else if (value is StringBuilder sb)
        {
            json.SetValue(key, sb.ToString());
        }
        else if (value is System.Security.SecureString ss)
        {
            json.SetValue(key, ss);
        }
        
        try
        {
            var v = JsonObjectNode.ConvertFrom(value);
            json.SetValue(key, v);
        }
        catch (JsonException)
        {
            var a = JsonArrayNode.ConvertFrom(value);
            json.SetValue(key, a);
        }
    }

    /// <summary>
    /// Writes the configuration content to the specified writer as a JSON value.
    /// </summary>
    /// <param name="writer">The writer to which to write this instance.</param>
    public void WriteConfigTo(Utf8JsonWriter writer)
    {
        var json = Config;
        if (json != null) json.WriteTo(writer);
        else writer.WriteNullValue();
    }

    /// <summary>
    /// Sets properites.
    /// </summary>
    /// <param name="value">The JSON object to fill the member of this instance.</param>
    /// <param name="blacklist">The blacklist of the entity property name.</param>
    public override void SetProperties(JsonObjectNode value, IEnumerable<string> blacklist)
    {
        if (value == null) return;
        base.SetProperties(value, blacklist);
        foreach (var prop in value)
        {
            if (prop.Key == null || !prop.Key.StartsWith("config.")) continue;
            var key = prop.Key.Range(0, 7)?.Trim();
            if (string.IsNullOrEmpty(key)) continue;
            var config = Config;
            if (config == null) config = Config = new JsonObjectNode();
            config[key] = prop.Value;
        }
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not ConfigurableResourceEntity e) return;
        ConfigString = e.ConfigString;
    }

    /// <summary>
    /// Copy the config object to itself.
    /// </summary>
    /// <param name="propertiesToSkip">The optional properties that need skip to copy.</param>
    protected void CopyConfigItself(params string[] propertiesToSkip)
    {
        var c = Config;
        if (c == null) return;
        c = c.Clone();
        c.Remove("private");
        Config = c;
        if (propertiesToSkip == null) return;
        foreach (var p in propertiesToSkip)
        {
            if (!string.IsNullOrWhiteSpace(p)) c.Remove(p);
        }
    }

    private static JsonObjectNode ParseJson(string s)
    {
        try
        {
            return JsonObjectNode.Parse(s);
        }
        catch (JsonException)
        {
            return new JsonObjectNode();
        }
        catch (ArgumentException)
        {
            return new JsonObjectNode();
        }
        catch (InvalidOperationException)
        {
            return new JsonObjectNode();
        }
        catch (FormatException)
        {
            return new JsonObjectNode();
        }
    }
}

/// <summary>
/// The base owner resource entity.
/// </summary>
[DataContract]
public class BaseSiteOwnerResourceEntity : ConfigurableResourceEntity
{
    /// <summary>
    /// Gets or sets the owner site identifier.
    /// </summary>
    [Column("site")]
    [DataMember(Name = "site")]
    [JsonPropertyName("site")]
    public string OwnerSiteId
    {
        get
        {
            return GetCurrentProperty<string>();
        }

        set
        {
            SetCurrentProperty(value);
        }
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not BaseSiteOwnerResourceEntity e) return;
        OwnerSiteId = e.OwnerSiteId;
    }
}

/// <summary>
/// The base owner resource entity.
/// </summary>
[DataContract]
public class BaseOwnerResourceEntity : ConfigurableResourceEntity
{
    /// <summary>
    /// Gets or sets the owner identifier.
    /// </summary>
    [DataMember(Name = "owner")]
    [JsonPropertyName("owner")]
    [Column("owner")]
    [Required]
    public string OwnerId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not BaseOwnerResourceEntity e) return;
        OwnerId = e.OwnerId;
    }
}

/// <summary>
/// The owner relationship resource entity.
/// </summary>
[DataContract]
public class OwnerResourceEntity : BaseOwnerResourceEntity
{
    /// <summary>
    /// Gets or sets the target resource identifier.
    /// </summary>
    [DataMember(Name = "res")]
    [JsonPropertyName("res")]
    [Column("res")]
    public string TargetId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets a value indicating whether has a target resource.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public bool HasTarget => !string.IsNullOrWhiteSpace(TargetId);

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not OwnerResourceEntity e) return;
        TargetId = e.TargetId;
    }
}

/// <summary>
/// The owner relationship resource entity.
/// </summary>
/// <typeparam name="T">The type of owner resource.</typeparam>
[DataContract]
public class OwnerResourceEntity<T> : OwnerResourceEntity where T : BaseResourceEntity
{
    /// <summary>
    /// Initializes a new instance of the OwnerResourceEntity class.
    /// </summary>
    public OwnerResourceEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the OwnerResourceEntity class.
    /// </summary>
    /// <param name="owner">The owner resource entity.</param>
    /// <param name="targetId">The target resource identifier.</param>
    /// <param name="name">The relationship name.</param>
    /// <param name="config">The configuration.</param>
    public OwnerResourceEntity(T owner, string targetId, string name, JsonObjectNode config = null)
    {
        if (!string.IsNullOrWhiteSpace(targetId)) TargetId = targetId;
        if (config != null) Config = config;
        if (name != null) Name = name;
        if (owner is null) return;
        Owner = owner;
        OwnerId = owner.Id;
    }

    /// <summary>
    /// Initializes a new instance of the OwnerResourceEntity class.
    /// </summary>
    /// <param name="copy">The source to copy.</param>
    /// <param name="owner">The owner resource entity.</param>
    public OwnerResourceEntity(OwnerResourceEntity<T> copy, T owner)
        : this(owner, copy?.TargetId, copy?.Name, copy?.Config)
    {
        FillBaseProperties(copy);
    }

    /// <summary>
    /// Gets or sets the user entity reference.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public T Owner { get; set; }

    /// <summary>
    /// Gets a value indicating whether the owner reference is filled.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public bool HasOwnerReference => !(Owner is null);
}

/// <summary>
/// The owner relationship resource entity.
/// </summary>
/// <typeparam name="TOwner">The type of owner resource.</typeparam>
/// <typeparam name="TTarget">The type of target resource.</typeparam>
[DataContract]
public class OwnerResourceEntity<TOwner, TTarget> : OwnerResourceEntity<TOwner>
    where TOwner : BaseResourceEntity
    where TTarget : BaseResourceEntity
{
    /// <summary>
    /// Initializes a new instance of the OwnerResourceEntity class.
    /// </summary>
    public OwnerResourceEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the OwnerResourceEntity class.
    /// </summary>
    /// <param name="owner">The owner resource entity.</param>
    /// <param name="target">The target resource entity.</param>
    /// <param name="name">The relationship name.</param>
    /// <param name="config">The configuration.</param>
    public OwnerResourceEntity(TOwner owner, TTarget target, string name = null, JsonObjectNode config = null)
    {
        if (owner is not null)
        {
            Owner = owner;
            OwnerId = owner.Id;
        }

        if (target is not null)
        {
            Target = target;
            TargetId = target.Id;
            Name = name ?? target.Name;
        }
        else if (name != null)
        {
            Name = name;
        }

        if (config != null) Config = config;
    }

    /// <summary>
    /// Initializes a new instance of the OwnerResourceEntity class.
    /// </summary>
    /// <param name="copy">The source to copy.</param>
    /// <param name="owner">The owner resource entity.</param>
    /// <param name="target">The target resource entity.</param>
    public OwnerResourceEntity(OwnerResourceEntity<TOwner, TTarget> copy, TOwner owner, TTarget target)
        : this(owner, target, copy?.Name, copy?.Config)
    {
        FillBaseProperties(copy);
    }

    /// <summary>
    /// Gets or sets the target resource entity reference.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public TTarget Target { get; set; }

    /// <summary>
    /// Gets a value indicating whether the target resource reference is filled.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public bool HasTargetReference => !(Owner is null);
}
