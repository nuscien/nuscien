using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

using Trivial.Reflection;

namespace NuScien.Data
{
    /// <summary>
    /// The entity states.
    /// </summary>
    public enum EntityStates
    {
        /// <summary>
        /// The entity does not exist or is removed.
        /// </summary>
        Deleted = 0,

        /// <summary>
        /// The entity is applying for to approval.
        /// </summary>
        Request = 1,

        /// <summary>
        /// The entity is in service.
        /// </summary>
        InService = 2
    }

    /// <summary>
    /// Base entity information.
    /// </summary>
    [DataContract]
    public abstract class BaseEntityInfo : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [DataMember(Name = "id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets a value indicating whether the entity is a new one.
        /// </summary>
        [JsonIgnore]
        public bool IsNew => string.IsNullOrWhiteSpace(Id);

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        [DataMember(Name = "name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [DataMember(Name = "state")]
        [JsonPropertyName("state")]
        public EntityStates State { get; set; }
    }
}
