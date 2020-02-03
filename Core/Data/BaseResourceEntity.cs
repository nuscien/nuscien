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

                ForceNotify(nameof(Id));
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
                if (string.IsNullOrEmpty(value)) IsNew = true;
                revision = value;
                ForceNotify(nameof(Revision));
            }
        }

        /// <summary>
        /// Prepares for saving.
        /// </summary>
        public void PrepareForSaving()
        {
            revision = Guid.NewGuid().ToString("n");
            LastModificationTime = DateTime.Now;
            IsNew = false;
            ForceNotify(nameof(Revision));
        }

        /// <summary>
        /// Rollbacks the saving action.
        /// </summary>
        public void RollbackSaving()
        {
            IsNew = string.IsNullOrEmpty(oldRevision);
            revision = oldRevision;
        }

        /// <summary>
        /// Copy base properties into current instance.
        /// </summary>
        /// <param name="entity">The source entity to copy.</param>
        protected void FillBaseProperties(BaseResourceEntity entity)
        {
            revision = entity.revision;
            id = entity.id;
            IsNew = entity.IsNew;
            oldRevision = entity.oldRevision;
            ForceNotify(nameof(Id));
            Name = entity.Name;
            State = entity.State;
            CreationTime = entity.CreationTime;
            LastModificationTime = entity.LastModificationTime;
        }
    }

    /// <summary>
    /// The base owner resource entity.
    /// </summary>
    [DataContract]
    public class BaseOwnerResourceEntity : BaseResourceEntity
    {
        private string config;

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
                try
                {
                    Config = JsonObject.Parse(config);
                }
                catch (JsonException)
                {
                    Config = new JsonObject();
                }
                catch (ArgumentException)
                {
                    Config = new JsonObject();
                }
                catch (InvalidOperationException)
                {
                    Config = new JsonObject();
                }
                catch (FormatException)
                {
                    Config = new JsonObject();
                }
            }
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
