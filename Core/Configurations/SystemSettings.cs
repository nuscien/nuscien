﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Trivial.Text;
using Trivial.Reflection;
using NuScien.Reflection;

namespace NuScien.Configurations;

/// <summary>
/// The system global settings.
/// </summary>
public class SystemGlobalSettings : ReadonlyObservableProperties
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    [JsonPropertyName("owner")]
    public string Owner
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the logo URL.
    /// </summary>
    [JsonPropertyName("logo")]
    public string Logo
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }


    /// <summary>
    /// Gets or sets the wordmark URL.
    /// </summary>
    [JsonPropertyName("wordmark")]
    public string Wordmark
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the group identifier of the group administrators.
    /// </summary>
    [JsonPropertyName("admin_current")]
    public string CurrentSettingsAdminGroupId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the group identifier of the user administrators.
    /// </summary>
    [JsonPropertyName("admin_user")]
    public string UserAdminGroupId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the group identifier of the site administrators.
    /// </summary>
    [JsonPropertyName("admin_site")]
    public string SiteAdminGroupId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the group identifier of the group administrators.
    /// </summary>
    [JsonPropertyName("admin_group")]
    public string GroupAdminGroupId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the group identifier of the CMS administrators.
    /// </summary>
    [JsonPropertyName("admin_cms")]
    public string CmsAdminGroupId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether need forbid user register.
    /// </summary>
    [JsonPropertyName("user_reg_forbid")]
    public bool ForbidUserRegister
    {
        get => GetCurrentProperty<bool>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the extension data for JSON serialization.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionSerializationData { get; set; }

    /// <summary>
    /// Converts an instance of the settings model to JSON object.
    /// </summary>
    /// <param name="model">The model to convert.</param>
    /// <returns>The JSON object.</returns>
    public static explicit operator JsonObjectNode(SystemGlobalSettings model)
    {
        if (model == null) return null;
        return JsonObjectNode.ConvertFrom(model);
    }
}

/// <summary>
/// The system site settings.
/// </summary>
public class SystemSiteSettings : ReadonlyObservableProperties
{
    /// <summary>
    /// Gets or sets the site name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the site owner.
    /// </summary>
    [JsonPropertyName("owner")]
    public string Owner
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the logo URL.
    /// </summary>
    [JsonPropertyName("logo")]
    public string Logo
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the extension data for JSON serialization.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionSerializationData { get; set; }

    /// <summary>
    /// Converts an instance of the settings model to JSON object.
    /// </summary>
    /// <param name="model">The model to convert.</param>
    /// <returns>The JSON object.</returns>
    public static explicit operator JsonObjectNode(SystemSiteSettings model)
    {
        if (model == null) return null;
        return JsonObjectNode.ConvertFrom(model);
    }
}
