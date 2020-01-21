using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Text;

namespace NuScien.Users
{
    /// <summary>
    /// Genders.
    /// </summary>
    public enum Genders
    {
        /// <summary>
        /// Unknown (or secret).
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Male.
        /// </summary>
        Male = 1,

        /// <summary>
        /// Female.
        /// </summary>
        Female = 2,

        /// <summary>
        /// Shemale.
        /// </summary>
        Shemale = 4,

        /// <summary>
        /// Asexual.
        /// </summary>
        Asexual = 5,

        /// <summary>
        /// Other.
        /// </summary>
        Other = 7
    }

    /// <summary>
    /// The user information.
    /// </summary>
    public class UserEntity : BaseResourceEntity
    {
        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        [DataMember(Name = "nickname")]
        [JsonPropertyName("nickname")]
        public string Nickname
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [JsonIgnore]
        public string PasswordEncrypted
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        [DataMember(Name = "avatar")]
        [JsonPropertyName("avatar")]
        public string Avatar
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        [DataMember(Name = "id")]
        [JsonPropertyName("id")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Genders Gender
        {
            get => GetCurrentProperty<Genders>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the birthday.
        /// </summary>
        [DataMember(Name = "birthday", EmitDefaultValue = true)]
        [JsonPropertyName("birthday")]
        [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackNullableConverter))]
        public DateTime? Birthday
        {
            get => GetCurrentProperty<DateTime?>();
            set => SetCurrentProperty(value);
        }
    }
}
