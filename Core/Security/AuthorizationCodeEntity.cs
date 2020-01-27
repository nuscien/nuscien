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
        /// Sets a new password.
        /// </summary>
        /// <param name="password">The new password.</param>
        /// <param name="old">The optional old password to validate; or null, to ignore validation.</param>
        public bool SetCode(string password, string old = null)
        {
            if (!string.IsNullOrEmpty(old) && !ValidateCode(old)) return false;
            if (password == null) return false;
            password = password.Trim();
            if (password.Length < 6) return false;
            CodeEncrypted = ComputeCodeHash(password);
            return true;
        }

        /// <summary>
        /// Sets a new password.
        /// </summary>
        /// <param name="password">The new password.</param>
        /// <param name="old">The optional old password to validate; or null, to ignore validation.</param>
        public bool SetCode(SecureString password, string old = null)
        {
            return SetCode(password.ToUnsecureString(), old);
        }

        /// <summary>
        /// Tests if the given password is valid.
        /// </summary>
        /// <param name="password">The original password.</param>
        /// <returns>true if the password is correct; otherwise, false.</returns>
        public bool ValidateCode(string password)
        {
            if (password == null) return false;
            password = password.Trim();
            return ComputeCodeHash(password).Equals(CodeEncrypted, StringComparison.Ordinal);
        }

        /// <summary>
        /// Tests if the given password is valid.
        /// </summary>
        /// <param name="password">The original password.</param>
        /// <returns>true if the password is correct; otherwise, false.</returns>
        public bool ValidateCode(SecureString password)
        {
            return ValidateCode(password.ToUnsecureString());
        }

        /// <summary>
        /// Computes the code hash string.
        /// </summary>
        /// <param name="originalCode">The original code value.</param>
        /// <returns>The hash value.</returns>
        public static string ComputeCodeHash(string originalCode)
        {
            return HashUtility.ComputeSHA512String(originalCode);
        }
    }
}
