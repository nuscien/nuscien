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

namespace NuScien.Security
{
    /// <summary>
    /// The client entity.
    /// </summary>
    [Table("nsauthcodes")]
    public class AuthorizationCodeEntity : BaseOwnerResourceEntity
    {
        /// <summary>
        /// Gets or sets the avatar or icon URL.
        /// </summary>
        [NotMapped]
        [JsonPropertyName("kind")]
        public SecurityEntityTypes OwnerType
        {
            get => GetCurrentProperty<SecurityEntityTypes>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the avatar or icon URL.
        /// </summary>
        [Column("kind")]
        [JsonIgnore]
        public int OwnerTypeCode
        {
            get => (int)OwnerType;
            set => OwnerType = (SecurityEntityTypes)value;
        }

        /// <summary>
        /// Gets or sets the avatar or icon URL.
        /// </summary>
        [Column("avatar")]
        [JsonPropertyName("avatar")]
        public string Avatar
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        [JsonIgnore]
        [Column("code")]
        public string CodeEncrypted
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the service provider name or url.
        /// </summary>
        [Column("provider")]
        [JsonPropertyName("provider")]
        public string ServiceProvider
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Sets a new authorization code.
        /// </summary>
        /// <param name="authorizationCode">The new authorization code.</param>
        /// <param name="old">The optional old authorization code to validate; or null, to ignore validation.</param>
        public bool SetCode(string authorizationCode, string old = null)
        {
            if (!string.IsNullOrEmpty(old) && !ValidateCode(old)) return false;
            if (authorizationCode == null) return false;
            authorizationCode = authorizationCode.Trim();
            if (authorizationCode.Length < 6) return false;
            CodeEncrypted = ComputeCodeHash(authorizationCode);
            return true;
        }

        /// <summary>
        /// Sets a new authorization code.
        /// </summary>
        /// <param name="authorizationCode">The new authorization code.</param>
        /// <param name="old">The optional old authorization code to validate; or null, to ignore validation.</param>
        public bool SetCode(SecureString authorizationCode, string old = null)
        {
            return SetCode(authorizationCode.ToUnsecureString(), old);
        }

        /// <summary>
        /// Tests if the given authorization code is valid.
        /// </summary>
        /// <param name="authorizationCode">The original password.</param>
        /// <returns>true if the password is correct; otherwise, false.</returns>
        public bool ValidateCode(string authorizationCode)
        {
            if (authorizationCode == null) return false;
            authorizationCode = authorizationCode.Trim();
            return ComputeCodeHash(authorizationCode).Equals(CodeEncrypted, StringComparison.Ordinal);
        }

        /// <summary>
        /// Tests if the given authorization code is valid.
        /// </summary>
        /// <param name="authorizationCode">The original authorization code.</param>
        /// <returns>true if the password is correct; otherwise, false.</returns>
        public bool ValidateCode(SecureString authorizationCode)
        {
            return ValidateCode(authorizationCode.ToUnsecureString());
        }

        /// <summary>
        /// Computes the authorization code hash string.
        /// </summary>
        /// <param name="originalAuthorizationCode">The original code value.</param>
        /// <returns>The hash value.</returns>
        public static string ComputeCodeHash(string originalAuthorizationCode)
        {
            return HashUtility.ComputeSHA512String(originalAuthorizationCode);
        }
    }
}
