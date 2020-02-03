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
    [DataContract]
    [Table("nsclients")]
    public class AccessingClientEntity : BaseSecurityEntity
    {
        /// <summary>
        /// Gets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public override SecurityEntityTypes SecurityEntityType => SecurityEntityTypes.ServiceClient;

        /// <summary>
        /// Gets or sets the credential.
        /// </summary>
        [JsonIgnore]
        [Column("credential")]
        public string CredentialKeyEncrypted
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Renews the credential key.
        /// </summary>
        public void RenewCredentialKey()
        {
            var password = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
            CredentialKeyEncrypted = ComputeCredentialKeyHash(password);
        }

        /// <summary>
        /// Tests if the given credential key is valid.
        /// </summary>
        /// <param name="password">The original password.</param>
        /// <returns>true if the password is correct; otherwise, false.</returns>
        public bool ValidateCredentialKey(string password)
        {
            if (password == null) return false;
            password = password.Trim();
            return ComputeCredentialKeyHash(password).Equals(CredentialKeyEncrypted, StringComparison.Ordinal);
        }

        /// <summary>
        /// Tests if the given password is valid.
        /// </summary>
        /// <param name="password">The original password.</param>
        /// <returns>true if the password is correct; otherwise, false.</returns>
        public bool ValidateCredentialKey(SecureString password)
        {
            return ValidateCredentialKey(password.ToUnsecureString());
        }

        /// <summary>
        /// Computes the credential key hash string.
        /// </summary>
        /// <param name="originalCredentialKey">The original credential key value.</param>
        /// <returns>The hash value.</returns>
        public static string ComputeCredentialKeyHash(string originalCredentialKey)
        {
            return HashUtility.ComputeSHA512String(originalCredentialKey);
        }
    }
}
