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
        /// Gets or sets the group identifier of the administrators for this client.
        /// </summary>
        [JsonIgnore]
        [Column("group")]
        public string AdminGroupId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Renews the credential key.
        /// </summary>
        /// <returns>The app accessing key.</returns>
        public AppAccessingKey RenewCredentialKey()
        {
            var password = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
            var key = new AppAccessingKey
            {
                Id = Name,
                Secret = password.ToSecure()
            };
            CredentialKeyEncrypted = ComputeCredentialKeyHash(password);
            return key;
        }

        /// <summary>
        /// Sets the app accessing key.
        /// </summary>
        /// <param name="key">The app accessing key.</param>
        public void SetKey(AppAccessingKey key)
        {
            Name = key.Id;
            CredentialKeyEncrypted = ComputeCredentialKeyHash(key.Secret.ToUnsecureString());
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
            return ComputeCredentialKeyHash(password).Equals(CredentialKeyEncrypted?.Trim(), StringComparison.Ordinal);
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
        /// Sets the properties writable.
        /// </summary>
        internal void UnlockPropertiesReadonly()
        {
            PropertiesSettingPolicy = Trivial.Reflection.PropertySettingPolicies.Allow;
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
