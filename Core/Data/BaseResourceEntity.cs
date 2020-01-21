using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The entity states.
    /// </summary>
    public enum ResourceEntityStates
    {
        /// <summary>
        /// The entity does not exist or is removed.
        /// </summary>
        Deleted = 0,

        /// <summary>
        /// The entity is applying for to approval.
        /// </summary>
        Draft = 1,

        /// <summary>
        /// The entity is applying for to approval.
        /// </summary>
        Request = 2,

        /// <summary>
        /// The entity is in service.
        /// </summary>
        Normal = 3
    }

    /// <summary>
    /// Base entity information.
    /// </summary>
    [DataContract]
    public abstract class BaseResourceEntity : BaseObservableProperties
    {
        private string id;
        private string revision;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [DataMember(Name = "id")]
        [JsonPropertyName("id")]
        [Column("id")]
        public string Id
        {
            get
            {
                if (id != null) return id;
                id = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
                return id;
            }

            set
            {
                if (value != null) value = value.Trim().ToLower();
                if (id == value) return;
                if (string.IsNullOrEmpty(value))
                {
                    id = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
                    IsNew = true;
                }
                else
                {
                    id = value;
                    IsNew = false;
                }

                var isRevChanged = string.IsNullOrEmpty(revision);
                revision = null;
                ForceNotify(nameof(Id));
                if (isRevChanged) ForceNotify(nameof(Revision));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entity is a new one.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public bool IsNew { get; set; } = true;

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        [DataMember(Name = "name")]
        [JsonPropertyName("name")]
        [Column("name")]
        public string Name
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [DataMember(Name = "state")]
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [NotMapped]
        public ResourceEntityStates State
        {
            get => GetCurrentProperty<ResourceEntityStates>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the code of the entity state.
        /// </summary>
        [Column("state")]
        public int StateCode
        {
            get => (int)State;
            set => State = (ResourceEntityStates)value;
        }

        /// <summary>
        /// Gets or sets the creation date time.
        /// </summary>
        [DataMember(Name = "creation")]
        [JsonPropertyName("creation")]
        [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackConverter))]
        [Column("creation")]
        public DateTime CreationTime
        {
            get => GetCurrentProperty(DateTime.Now);
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the last modification date time.
        /// </summary>
        [DataMember(Name = "update")]
        [JsonPropertyName("update")]
        [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackConverter))]
        [Column("update")]
        public DateTime LastModificationTime
        {
            get => GetCurrentProperty(DateTime.Now);
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets a random string of revision.
        /// </summary>
        [DataMember(Name = "revision")]
        [JsonPropertyName("revision")]
        [Column("revision")]
        public string Revision
        {
            get
            {
                return revision;
            }

            set
            {
                if (value != null) value = value.Trim().ToLower();
                if (revision == value) return;
                if (string.IsNullOrEmpty(value)) IsNew = true;
                revision = value;
                ForceNotify(nameof(Revision));
            }
        }

        /// <summary>
        /// Renews a revision.
        /// </summary>
        public void RenewRevision()
        {
            revision = Guid.NewGuid().ToString("n");
            LastModificationTime = DateTime.Now;
            ForceNotify(nameof(Revision));
        }
    }
}
