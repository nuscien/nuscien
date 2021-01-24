using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Reflection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// Base entity information.
    /// </summary>
    [DataContract]
    public abstract class BaseResourceEntity : ReadonlyObservableProperties
    {
        private string id;
        private bool isRandomId;
        private bool wasRandomId;
        private string revision;
        private string oldRevision;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [DataMember(Name = "id")]
        [JsonPropertyName("id")]
        [Column("id")]
        [Required]
        public string Id
        {
            get
            {
                if (id != null) return id;
                id = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
                isRandomId = true;
                return id;
            }

            set
            {
                if (value != null) value = value.Trim().ToLower();
                if (id == value) return;
                id = value;
                isRandomId = false;
                ForceNotify(nameof(Id));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entity is a new one.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public bool IsNew
        {
            get => string.IsNullOrWhiteSpace(id) || isRandomId;
            internal set => isRandomId = value;
        }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        [DataMember(Name = "name")]
        [JsonPropertyName("name")]
        [Column("name")]
        [Required]
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
        [JsonIgnore]
        [Column("state")]
        [Required]
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
        [Required]
        public DateTime CreationTime
        {
            get => GetCurrentProperty(DateTime.Now);
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the last modification date time.
        /// </summary>
        [DataMember(Name = "lastupdate")]
        [JsonPropertyName("lastupdate")]
        [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackConverter))]
        [Column("lastupdate")]
        [Required]
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
        [Required]
        public string Revision
        {
            get
            {
                return revision;
            }

            set
            {
                if (value != null) value = value.Trim().ToLower();
                oldRevision = value;
                if (revision == value) return;
                revision = value;
                ForceNotify(nameof(Revision));
            }
        }

        /// <summary>
        /// Gets or sets the extension data for JSON serialization.
        /// </summary>
        [JsonExtensionData]
        [NotMapped]
        public Dictionary<string, JsonElement> ExtensionSerializationData { get; set; }

        /// <summary>
        /// Prepares for saving.
        /// </summary>
        public void PrepareForSaving()
        {
            _ = Id;
            wasRandomId = isRandomId;
            isRandomId = false;
            revision = Guid.NewGuid().ToString("n");
            LastModificationTime = DateTime.Now;
            ForceNotify(nameof(Revision));
        }

        /// <summary>
        /// Rollbacks the saving action.
        /// </summary>
        public void RollbackSaving()
        {
            revision = oldRevision;
            isRandomId = wasRandomId;
            wasRandomId = false;
        }

        /// <summary>
        /// Copy base properties into current instance.
        /// </summary>
        /// <param name="entity">The source entity to copy.</param>
        protected virtual void FillBaseProperties(BaseResourceEntity entity)
        {
            if (entity == null) return;
            revision = entity.revision;
            id = entity.id;
            isRandomId = entity.isRandomId;
            oldRevision = entity.oldRevision;
            ForceNotify(nameof(Id));
            Name = entity.Name;
            State = entity.State;
            CreationTime = entity.CreationTime;
            LastModificationTime = entity.LastModificationTime;
        }
    }

    /// <summary>
    /// The base resource entity with site bound.
    /// </summary>
    public abstract class SiteOwnedResourceEntity : BaseResourceEntity
    {
        /// <summary>
        /// Gets or sets the identifier of the owner site.
        /// </summary>
        [JsonPropertyName("site")]
        [Column("site")]
        public string SiteId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not SiteOwnedResourceEntity e) return;
            SiteId = e.SiteId;
        }
    }

    /// <summary>
    /// The base resource entity with security entity bound.
    /// </summary>
    public abstract class SpecificOwnerResourceEntity : BaseResourceEntity
    {
        private Security.BaseSecurityEntity owner;

        /// <summary>
        /// Gets or sets the owner identifier.
        /// </summary>
        [DataMember(Name = "owner")]
        [JsonPropertyName("owner")]
        [Column("owner")]
        [Required]
        public string OwnerId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the security entity type.
        /// </summary>
        [NotMapped]
        [JsonPropertyName("kind")]
        public abstract Security.SecurityEntityTypes OwnerType { get; }

        /// <summary>
        /// Gets the owner security entity; or null, if not applied.
        /// </summary>
        [NotMapped]
#if !NETCOREAPP3_1
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public Security.BaseSecurityEntity Owner
        {
            get => IsOwner(owner) ? owner : null;
            internal set => owner = IsOwner(value) ? value : null;
        }

        /// <summary>
        /// Gets a value indicating whether the owner entity has been filled.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool HasFilledOwnerEntity => IsOwner(owner);

        /// <summary>
        /// Tests if the entity is the owner.
        /// </summary>
        /// <param name="entity">The entity to test.</param>
        /// <returns>true if the specific entity is the owner; otherwise, false.</returns>
        public bool IsOwner(Security.BaseSecurityEntity entity)
        {
            return entity != null && entity.Id == OwnerId && entity.SecurityEntityType == OwnerType;
        }

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not SpecificOwnerResourceEntity e) return;
            OwnerId = e.OwnerId;
            if (e.Owner != null) Owner = e.Owner;
        }
    }
}
