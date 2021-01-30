using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Reflection;
using NuScien.Security;
using NuScien.Users;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Sns
{
    /// <summary>
    /// The roles targeted for the contact information.
    /// </summary>
    public enum ContactInfoRoles
    {
        /// <summary>
        /// Personal prime information.
        /// </summary>
        Person = 0,

        /// <summary>
        /// For home.
        /// </summary>
        Home = 1,

        /// <summary>
        /// For business (including company and indivial commercial entity).
        /// </summary>
        Business = 2,

        /// <summary>
        /// For school.
        /// </summary>
        School = 3,

        /// <summary>
        /// For other kinds of organization.
        /// </summary>
        Organization = 4,

        /// <summary>
        /// Used for club joined in.
        /// </summary>
        Club = 5,

        /// <summary>
        /// Virtual information (e.g. online network only).
        /// </summary>
        Virtual = 6,

        /// <summary>
        /// Others.
        /// </summary>
        Other = 7
    }

    /// <summary>
    /// The phone number types.
    /// </summary>
    public enum PhoneNumberTypes
    {
        /// <summary>
        /// Mobile phone.
        /// </summary>
        Mobile = 0,

        /// <summary>
        /// Landline telephone.
        /// </summary>
        Telephone = 1,

        /// <summary>
        /// Internal phone with short number.
        /// </summary>
        Internal = 2,

        /// <summary>
        /// Fax.
        /// </summary>
        Fax = 3,

        /// <summary>
        /// Virtual phone.
        /// </summary>
        Virtual = 4,

        /// <summary>
        /// IoT.
        /// </summary>
        IoT = 5,

        /// <summary>
        /// Voice record or video record only.
        /// </summary>
        Record = 6,

        /// <summary>
        /// Others.
        /// </summary>
        Other = 7
    }

    /// <summary>
    /// The address information.
    /// </summary>
    public class AddressInfo : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the country or region.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the 1st level region, e.g. province, and others levels if the city is not the 2nd.
        /// </summary>
        [JsonPropertyName("province")]
        public string Province
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        [JsonPropertyName("city")]
        public string City
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the address 1.
        /// </summary>
        [JsonPropertyName("addr1")]
        public string Address1
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the address 2.
        /// </summary>
        [JsonPropertyName("addr2")]
        public string Address2
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        [JsonPropertyName("zipcode")]
        public string ZipCode
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// Social account information.
    /// </summary>
    public class SocialAccountInfo : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the social account provider name.
        /// </summary>
        [JsonPropertyName("provider")]
        public string Provider
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        [JsonPropertyName("account")]
        public string Account
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the protocal link used to contact with.
        /// </summary>
        [JsonPropertyName("url")]
        public string Link
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the optional description.
        /// </summary>
        [JsonPropertyName("desc")]
        public string Description
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The organiztion information and title of the person.
    /// </summary>
    public class OrganizationRelationshipInfo : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the name of company, school or other kind of the organization.
        /// </summary>
        [JsonPropertyName("org")]
        public string Organization
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the branch or department name.
        /// </summary>
        [JsonPropertyName("dept")]
        public string Department
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the official website.
        /// </summary>
        [JsonPropertyName("url")]
        public string OfficialWebsite
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The phone number information.
    /// </summary>
    public class PhoneNumber : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the phone number type.
        /// </summary>
        [JsonPropertyName("type")]
        public PhoneNumberTypes Type
        {
            get => GetCurrentProperty<PhoneNumberTypes>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [JsonPropertyName("number")]
        public string Number
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the optional description.
        /// </summary>
        [JsonPropertyName("desc")]
        public string Description
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The contact information details.
    /// </summary>
    public class ContactInfo : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the contact information role.
        /// </summary>
        [JsonPropertyName("role")]
        public ContactInfoRoles Role
        {
            get => GetCurrentProperty<ContactInfoRoles>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the optional description.
        /// </summary>
        [JsonPropertyName("desc")]
        public string Description
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the organization relationship.
        /// </summary>
        [JsonPropertyName("org")]
        public OrganizationRelationshipInfo OrganizationRelationship
        {
            get => GetCurrentProperty<OrganizationRelationshipInfo>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [JsonPropertyName("phone")]
        public IEnumerable<PhoneNumber> Phone
        {
            get => GetCurrentProperty<IEnumerable<PhoneNumber>>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        [JsonPropertyName("email")]
        public IEnumerable<string> Email
        {
            get => GetCurrentProperty<IEnumerable<string>>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the address information.
        /// </summary>
        [JsonPropertyName("addr")]
        public AddressInfo Address
        {
            get => GetCurrentProperty<AddressInfo>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the homepage.
        /// </summary>
        [JsonPropertyName("url")]
        public string Homepage
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the social account information.
        /// </summary>
        [JsonPropertyName("sns")]
        public IEnumerable<SocialAccountInfo> SocialAccounts
        {
            get => GetCurrentProperty<IEnumerable<SocialAccountInfo>>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the other information.
        /// </summary>
        [JsonPropertyName("others")]
        public string OtherInformation
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The moniker information for the contact.
    /// </summary>
    public class UserMonikerInfo : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the moniker template.
        /// </summary>
        [JsonPropertyName("templ")]
        public string Template
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the family name.
        /// </summary>
        [JsonPropertyName("surname")]
        public string Surname
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the middle name.
        /// </summary>
        [JsonPropertyName("middlename")]
        public string MiddleName
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the given name.
        /// </summary>
        [JsonPropertyName("givenname")]
        public string GivenName
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        [JsonPropertyName("prefix")]
        public string Prefix
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        [JsonPropertyName("suffix")]
        public string Suffix
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// The contact model.
    /// </summary>
    public class ContactModel : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonPropertyName("name")]
        public UserMonikerInfo Name
        {
            get => GetCurrentProperty<UserMonikerInfo>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        [JsonPropertyName("nickname")]
        public string Nickname
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the contact information.
        /// </summary>
        [JsonPropertyName("contacts")]
        public IEnumerable<ContactInfo> Contacts
        {
            get => GetCurrentProperty<IEnumerable<ContactInfo>>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the birthday.
        /// </summary>
        [JsonPropertyName("birthday")]
        public DateTime? Birthday
        {
            get => GetCurrentProperty<DateTime?>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the memorial day.
        /// </summary>
        [JsonPropertyName("anniversary")]
        public DateTime? Anniversary
        {
            get => GetCurrentProperty<DateTime?>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the bio.
        /// </summary>
        [JsonPropertyName("bio")]
        public string Bio
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }
    }
}
