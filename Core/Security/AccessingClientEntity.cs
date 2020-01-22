using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Text;

namespace NuScien.Security
{
    /// <summary>
    /// The client entity.
    /// </summary>
    [DataContract]
    public class AccessingClientEntity : BaseResourceEntity
    {
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
    }
}
