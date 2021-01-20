﻿using System;
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
    /// The entity states.
    /// </summary>
    public enum ResourceEntityStates
    {
        /// <summary>
        /// The entity does not exist or is removed.
        /// </summary>
        Deleted = 0,

        /// <summary>
        /// The entity is a draft.
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
        [DataMember(Name = "update")]
        [JsonPropertyName("update")]
        [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackConverter))]
        [Column("update")]
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
    /// The base configurable resource entity.
    /// </summary>
    [DataContract]
    public abstract class ConfigurableResourceEntity : BaseResourceEntity
    {
        private string config;
        private JsonObject json;

        /// <summary>
        /// Gets or sets the additional message.
        /// </summary>
        [JsonPropertyName("config")]
        [JsonConverter(typeof(JsonObjectConverter))]
        [NotMapped]
        public JsonObject Config
        {
            get
            {
                var s = config;
                if (s == null) return json;
                try
                {
                    json = ParseJson(s);
                }
                finally
                {
                    config = null;
                }

                return json;
            }

            set
            {
                config = null;
                json = value;
            }
        }

        /// <summary>
        /// Gets or sets the additional message.
        /// </summary>
        [DataMember(Name = "config")]
        [JsonIgnore]
        [Column("config")]
        public string ConfigString
        {
            get
            {
                return config ?? Config?.ToString() ?? string.Empty;
            }

            set
            {
                config = value;
                json = null;
            }
        }

        /*
        private string config;

        /// <summary>
        /// Gets or sets the additional message.
        /// </summary>
        [JsonPropertyName("config")]
        [JsonConverter(typeof(JsonObjectConverter))]
        [NotMapped]
        public JsonObject Config
        {
            get => GetCurrentProperty<JsonObject>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the additional message.
        /// </summary>
        [DataMember(Name = "config")]
        [JsonIgnore]
        [Column("config")]
        public string ConfigString
        {
            get
            {
                return Config?.ToString() ?? string.Empty;
            }

            set
            {
                config = value;
                Config = ParseJson(config);
            }
        }
        */

        /// <summary>
        /// Updates the configuration string from its JSON object type property.
        /// </summary>
        public void SyncConfig()
        {
            if (json != null) config = null;
        }

        private static JsonObject ParseJson(string s)
        {
            try
            {
                return JsonObject.Parse(s);
            }
            catch (JsonException)
            {
                return new JsonObject();
            }
            catch (ArgumentException)
            {
                return new JsonObject();
            }
            catch (InvalidOperationException)
            {
                return new JsonObject();
            }
            catch (FormatException)
            {
                return new JsonObject();
            }
        }

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not ConfigurableResourceEntity e) return;
            ConfigString = e.ConfigString;
        }
    }

    /// <summary>
    /// The base owner resource entity.
    /// </summary>
    [DataContract]
    public class BaseOwnerResourceEntity : ConfigurableResourceEntity
    {
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

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not BaseOwnerResourceEntity e) return;
            OwnerId = e.OwnerId;
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

    /// <summary>
    /// The owner relationship resource entity.
    /// </summary>
    [DataContract]
    public class OwnerResourceEntity : BaseOwnerResourceEntity
    {
        /// <summary>
        /// Gets or sets the target resource identifier.
        /// </summary>
        [DataMember(Name = "res")]
        [JsonPropertyName("res")]
        [Column("res")]
        public string TargetId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets a value indicating whether has a target resource.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool HasTarget => !string.IsNullOrWhiteSpace(TargetId);

        /// <inheritdoc />
        protected override void FillBaseProperties(BaseResourceEntity entity)
        {
            base.FillBaseProperties(entity);
            if (entity is not OwnerResourceEntity e) return;
            TargetId = e.TargetId;
        }
    }

    /// <summary>
    /// The owner relationship resource entity.
    /// </summary>
    /// <typeparam name="T">The type of owner resource.</typeparam>
    [DataContract]
    public class OwnerResourceEntity<T> : OwnerResourceEntity where T : BaseResourceEntity
    {
        /// <summary>
        /// Initializes a new instance of the OwnerResourceEntity class.
        /// </summary>
        public OwnerResourceEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OwnerResourceEntity class.
        /// </summary>
        /// <param name="owner">The owner resource entity.</param>
        /// <param name="targetId">The target resource identifier.</param>
        /// <param name="name">The relationship name.</param>
        /// <param name="config">The configuration.</param>
        public OwnerResourceEntity(T owner, string targetId, string name, JsonObject config = null)
        {
            if (!string.IsNullOrWhiteSpace(targetId)) TargetId = targetId;
            if (config != null) Config = config;
            if (name != null) Name = name;
            if (owner is null) return;
            Owner = owner;
            OwnerId = owner.Id;
        }

        /// <summary>
        /// Initializes a new instance of the OwnerResourceEntity class.
        /// </summary>
        /// <param name="copy">The source to copy.</param>
        /// <param name="owner">The owner resource entity.</param>
        public OwnerResourceEntity(OwnerResourceEntity<T> copy, T owner)
            : this(owner, copy?.TargetId, copy?.Name, copy?.Config)
        {
            FillBaseProperties(copy);
        }

        /// <summary>
        /// Gets or sets the user entity reference.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public T Owner { get; set; }

        /// <summary>
        /// Gets a value indicating whether the owner reference is filled.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool HasOwnerReference => !(Owner is null);
    }

    /// <summary>
    /// The owner relationship resource entity.
    /// </summary>
    /// <typeparam name="TOwner">The type of owner resource.</typeparam>
    /// <typeparam name="TTarget">The type of target resource.</typeparam>
    [DataContract]
    public class OwnerResourceEntity<TOwner, TTarget> : OwnerResourceEntity<TOwner>
        where TOwner : BaseResourceEntity
        where TTarget : BaseResourceEntity
    {
        /// <summary>
        /// Initializes a new instance of the OwnerResourceEntity class.
        /// </summary>
        public OwnerResourceEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OwnerResourceEntity class.
        /// </summary>
        /// <param name="owner">The owner resource entity.</param>
        /// <param name="target">The target resource entity.</param>
        /// <param name="name">The relationship name.</param>
        /// <param name="config">The configuration.</param>
        public OwnerResourceEntity(TOwner owner, TTarget target, string name = null, JsonObject config = null)
        {
            if (!(owner is null))
            {
                Owner = owner;
                OwnerId = owner.Id;
            }

            if (!(target is null))
            {
                Target = target;
                TargetId = target.Id;
                Name = name ?? target.Name;
            }
            else if (name != null)
            {
                Name = name;
            }

            if (config != null) Config = config;
        }

        /// <summary>
        /// Initializes a new instance of the OwnerResourceEntity class.
        /// </summary>
        /// <param name="copy">The source to copy.</param>
        /// <param name="owner">The owner resource entity.</param>
        /// <param name="target">The target resource entity.</param>
        public OwnerResourceEntity(OwnerResourceEntity<TOwner, TTarget> copy, TOwner owner, TTarget target)
            : this(owner, target, copy?.Name, copy?.Config)
        {
            FillBaseProperties(copy);
        }

        /// <summary>
        /// Gets or sets the target resource entity reference.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public TTarget Target { get; set; }

        /// <summary>
        /// Gets a value indicating whether the target resource reference is filled.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool HasTargetReference => !(Owner is null);
    }
}
