using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Security;
using Trivial.Text;

namespace NuScien.Security;

/// <summary>
/// Security entity types.
/// </summary>
public enum SecurityEntityTypes
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// User.
    /// </summary>
    User = 1,

    /// <summary>
    /// User group.
    /// </summary>
    UserGroup = 2,

    /// <summary>
    /// Service.
    /// </summary>
    ServiceClient = 3
}

/// <summary>
/// The base security entity.
/// </summary>
public abstract class BaseSecurityEntity : BaseResourceEntity
{
    /// <summary>
    /// Gets the security entity type.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public abstract SecurityEntityTypes SecurityEntityType { get; }

    /// <summary>
    /// Gets the security entity type in string.
    /// This property is only used for JSON serialization.
    /// </summary>
    [NotMapped]
    [DataMember(Name = "identype")]
    [JsonPropertyName("identype")]
    public string SecurityEntityTypeString
    {
        get => SecurityEntityType.ToString();
        set => _ = value;
    }

    /// <summary>
    /// Gets or sets the nickname.
    /// </summary>
    [DataMember(Name = "nickname")]
    [JsonPropertyName("nickname")]
    [Column("nickname")]
    public string Nickname
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    [DataMember(Name = "avatar")]
    [JsonPropertyName("avatar")]
    [Column("avatar")]
    public string Avatar
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Returns a string that represents the current entity object.
    /// </summary>
    /// <returns>A string that represents the current entity object.</returns>
    public override string ToString()
    {
        return $"Security entity ({SecurityEntityType}) {Name ?? Id} - {Nickname ?? "?"}";
    }
}
