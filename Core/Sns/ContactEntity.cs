using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Reflection;
using NuScien.Security;
using NuScien.Users;
using Trivial.Reflection;
using Trivial.SocialNetwork;
using Trivial.Text;
using Trivial.Users;

namespace NuScien.Sns;

/// <summary>
/// The contact entity.
/// </summary>
[Table("nscontacts")]
public class ContactEntity : UserResourceEntity
{
    //private string details;

    ///// <summary>
    ///// Gets or sets the details string.
    ///// </summary>
    //[Column("details")]
    //public string Details
    //{
    //    get
    //    {
    //        return details ?? GetPropertyJson<ContactModel>("Model") ?? string.Empty;
    //    }

    //    set
    //    {
    //        details = value;
    //        RemoveProperty("Model");
    //    }
    //}

    ///// <summary>
    ///// Gets or sets the additional message.
    ///// </summary>
    //[JsonPropertyName("model")]
    //[NotMapped]
    //public ContactModel Model
    //{
    //    get
    //    {
    //        try
    //        {
    //            return GetPropertySerialized<ContactModel>("Model", details, null);
    //        }
    //        finally
    //        {
    //            details = null;
    //        }
    //    }

    //    set
    //    {
    //        details = null;
    //        SetProperty("Model", value);
    //    }
    //}

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public UserMonikerInfo Moniker
    {
        get => TryDeserializeConfigValue<UserMonikerInfo>("name");
        set => SetConfigValue("name", value);
    }

    /// <summary>
    /// Gets or sets the contact information.
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public IEnumerable<ContactInfo> Contacts
    {
        get => TryDeserializeConfigValue<IEnumerable<ContactInfo>>("contacts");
        set => SetConfigValue("contacts", value);
    }

    /// <summary>
    /// Gets or sets the birthday.
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public ContactDatesInfo Dates
    {
        get => TryDeserializeConfigValue<ContactDatesInfo>("dates");
        set => SetConfigValue("dates", value);
    }

    /// <summary>
    /// Gets or sets the bio.
    /// </summary>
    [JsonIgnore]
    [NotMapped]
    public string Bio
    {
        get => TryDeserializeConfigValue<string>("bio");
        set => SetConfigValue("bio", value);
    }

    /// <summary>
    /// Converts an instance of contact model from the entity.
    /// </summary>
    /// <param name="e">The entity to convert.</param>
    /// <returns>The model.</returns>
    public static explicit operator ContactModel(ContactEntity e)
    {
        if (e == null) return null;
        return new ContactModel
        {
            Name = e.Moniker,
            ContactMethods = new(e.Contacts),
            Dates = e.Dates,
            Bio = e.Bio
        };
    }
}
