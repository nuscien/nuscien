﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseResourceEntity.cs" company="Nanchang Jinchen Software Co., Ltd.">
//   Copyright (c) 2010 Nanchang Jinchen Software Co., Ltd. All rights reserved.
// </copyright>
// <summary>
//   The base resource entities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
/// Base entity information.
/// </summary>
[DataContract]
public abstract class BaseResourceEntity : ReadonlyObservableProperties
{
    private string id;
    private bool isRandomId;
    private bool wasRandomId;
    private string revision;
    private string oldRevision;
    private Dictionary<string, PropertyInfo> propertyMapping;

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    [Column("id")]
    [Required]
    public string Id
    {
        get
        {
            if (id != null) return id;
            id = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
            isRandomId = true;
            return id;
        }

        set
        {
            if (value != null) value = value.Trim().ToLower();
            if (id == value) return;
            id = value;
            isRandomId = false;
            ForceNotify(nameof(Id));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the entity is a new one.
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public bool IsNew
    {
        get => string.IsNullOrWhiteSpace(id) || isRandomId;
        internal set => isRandomId = value;
    }

    /// <summary>
    /// Gets or sets the group name.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    [Column("name")]
    [Required]
    public string Name
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [DataMember(Name = "state")]
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonIntegerEnumCompatibleConverter<ResourceEntityStates>))]
    [NotMapped]
    public ResourceEntityStates State
    {
        get => GetCurrentProperty<ResourceEntityStates>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the code of the entity state.
    /// </summary>
    [JsonIgnore]
    [Column("state")]
    [Required]
    public int StateCode
    {
        get => (int)State;
        set => State = (ResourceEntityStates)value;
    }

    /// <summary>
    /// Gets or sets the creation date time.
    /// </summary>
    [DataMember(Name = "creation")]
    [JsonPropertyName("creation")]
    [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackConverter))]
    [Column("creation")]
    [Required]
    public DateTime CreationTime
    {
        get => GetCurrentProperty(DateTime.Now);
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the last modification date time.
    /// </summary>
    [DataMember(Name = "lastupdate")]
    [JsonPropertyName("lastupdate")]
    [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackConverter))]
    [Column("lastupdate")]
    [Required]
    public DateTime LastModificationTime
    {
        get => GetCurrentProperty(DateTime.Now);
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets a random string of revision.
    /// </summary>
    [DataMember(Name = "rev")]
    [JsonPropertyName("rev")]
    [Column("revision")]
    [Required]
    public string Revision
    {
        get
        {
            return revision;
        }

        set
        {
            if (value != null) value = value.Trim().ToLower();
            oldRevision = value;
            if (revision == value) return;
            revision = value;
            ForceNotify(nameof(Revision));
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the properties are slim.
    /// </summary>
    [NotMapped]
    [JsonPropertyName("slim")]
    public new bool IsSlim
    {
        get => base.IsSlim;
        set => base.IsSlim = value;
    }

    /// <summary>
    /// Gets or sets the extension data for JSON serialization.
    /// </summary>
    [JsonExtensionData]
    [NotMapped]
    public Dictionary<string, JsonElement> ExtensionSerializationData { get; set; }

    /// <summary>
    /// Sets properites.
    /// </summary>
    /// <param name="value">The JSON object to fill the member of this instance.</param>
    public void SetProperties(JsonObjectNode value)
    {
        SetProperties(value, null);
    }

    /// <summary>
    /// Sets properites.
    /// </summary>
    /// <param name="value">The JSON object to fill the member of this instance.</param>
    /// <param name="blacklist">The blacklist of the entity property name.</param>
    public virtual void SetProperties(JsonObjectNode value, IEnumerable<string> blacklist)
    {
        if (value == null) return;
        if (value.TryGetStringValue("id", out var id) && !string.IsNullOrWhiteSpace(id) && id.Trim().ToLowerInvariant() != Id?.ToLowerInvariant()) return;
        var arr = blacklist?.ToList() ?? new List<string>();
        if (value.TryGetEnumValue<ResourceEntityStates>("state", true, out var state) && !arr.Contains(nameof(State)) && !arr.Contains(nameof(StateCode))) State = state;
        if (value.TryGetStringValue("name", out var name) && !string.IsNullOrWhiteSpace(name) && !arr.Contains(nameof(Name))) Name = name;
        if (value.TryGetStringValue("rev", out var rev) && !string.IsNullOrWhiteSpace(rev) && !arr.Contains(nameof(Revision))) Revision = rev;
        var dict = propertyMapping;
        if (dict == null)
        {
            dict = new Dictionary<string, PropertyInfo>();
            foreach (var prop in GetType().GetProperties())
            {
                if (prop == null || !prop.CanWrite || string.IsNullOrWhiteSpace(prop.Name)) continue;
                switch (prop.Name)
                {
                    case nameof(Id):
                    case nameof(Name):
                    case nameof(CreationTime):
                    case nameof(LastModificationTime):
                    case nameof(State):
                    case nameof(StateCode):
                    case nameof(Revision):
                        break;
                    default:
                        var attrs = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false);
                        if (attrs == null || attrs.Length < 1) continue;
                        if (attrs[0] is not JsonPropertyNameAttribute propName || string.IsNullOrWhiteSpace(propName.Name)) continue;
                        attrs = prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), false);
                        if (attrs != null && attrs.Length >= 1) continue;
                        attrs = prop.GetCustomAttributes(typeof(JsonConverterAttribute), false);
                        if (attrs != null && attrs.Length >= 1) continue;
                        dict[propName.Name] = prop;
                        break;
                }
            }

            propertyMapping = dict;
        }

        foreach (var prop in value)
        {
            var key = prop.Key?.Trim();
            if (string.IsNullOrEmpty(key)) continue;
            switch (key.ToLowerInvariant())
            {
                case "id":
                case "creation":
                case "lastupdate":
                case "state":
                case "name":
                case "rev":
                    break;
                default:
                    if (!dict.TryGetValue(key, out var p) || arr.Contains(p.Name)) break;
                    try
                    {
                        var v = prop.Value.As(p.PropertyType);
                        p.SetValue(this, v);
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (MemberAccessException)
                    {
                    }
                    catch (ApplicationException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (InvalidCastException)
                    {
                    }
                    catch (FormatException)
                    {
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Returns a string that represents the current entity object.
    /// </summary>
    /// <returns>A string that represents the current entity object.</returns>
    public override string ToString()
        => $"Entity ({base.ToString()}) {Name ?? Id}";

    /// <summary>
    /// Prepares for saving.
    /// </summary>
    protected internal void PrepareForSaving()
    {
        _ = Id;
        wasRandomId = isRandomId;
        isRandomId = false;
        IsSlim = false;
        revision = Guid.NewGuid().ToString("n");
        LastModificationTime = DateTime.Now;
        ForceNotify(nameof(Revision));
    }

    /// <summary>
    /// Rollbacks the saving action.
    /// </summary>
    protected internal void RollbackSaving()
    {
        revision = oldRevision;
        isRandomId = wasRandomId;
        wasRandomId = false;
    }

    /// <summary>
    /// Copy base properties into current instance.
    /// </summary>
    /// <param name="entity">The source entity to copy.</param>
    protected virtual void FillBaseProperties(BaseResourceEntity entity)
    {
        if (entity == null) return;
        revision = entity.revision;
        id = entity.id;
        isRandomId = entity.isRandomId;
        oldRevision = entity.oldRevision;
        ForceNotify(nameof(Id));
        Name = entity.Name;
        State = entity.State;
        CreationTime = entity.CreationTime;
        LastModificationTime = entity.LastModificationTime;
    }
}

/// <summary>
/// The base resource entity with site bound.
/// </summary>
public abstract class SiteOwnedResourceEntity : BaseResourceEntity
{
    /// <summary>
    /// Gets or sets the identifier of the owner site.
    /// </summary>
    [JsonPropertyName("site")]
    [Column("site")]
    public string OwnerSiteId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not SiteOwnedResourceEntity e) return;
        OwnerSiteId = e.OwnerSiteId;
    }
}

/// <summary>
/// The base resource entity with security entity bound.
/// </summary>
public abstract class SpecificOwnerResourceEntity : BaseResourceEntity
{
    private Security.BaseSecurityEntity owner;

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

    /// <summary>
    /// Gets or sets the security entity type.
    /// </summary>
    [NotMapped]
    [JsonPropertyName("kind")]
    public abstract Security.SecurityEntityTypes OwnerType { get; }

    /// <summary>
    /// Gets the owner security entity; or null, if not applied.
    /// </summary>
    [NotMapped]
#if !NETCOREAPP3_1
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Security.BaseSecurityEntity Owner
    {
        get => IsOwner(owner) ? owner : null;
        internal set => owner = IsOwner(value) ? value : null;
    }

    /// <summary>
    /// Gets a value indicating whether the owner entity has been filled.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public bool HasFilledOwnerEntity => IsOwner(owner);

    /// <summary>
    /// Tests if the entity is the owner.
    /// </summary>
    /// <param name="entity">The entity to test.</param>
    /// <returns>true if the specific entity is the owner; otherwise, false.</returns>
    public bool IsOwner(Security.BaseSecurityEntity entity)
    {
        return entity != null && entity.Id == OwnerId && entity.SecurityEntityType == OwnerType;
    }

    /// <inheritdoc />
    protected override void FillBaseProperties(BaseResourceEntity entity)
    {
        base.FillBaseProperties(entity);
        if (entity is not SpecificOwnerResourceEntity e) return;
        OwnerId = e.OwnerId;
        if (e.Owner != null) Owner = e.Owner;
    }
}
