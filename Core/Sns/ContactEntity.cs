using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Data;
using NuScien.Reflection;
using NuScien.Security;
using NuScien.Users;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Sns
{
    /// <summary>
    /// The contact entity.
    /// </summary>
    [Table("nscontacts")]
    public class ContactEntity : UserResourceEntity
    {
        private string details;
        private ContactModel model;

        /// <summary>
        /// Gets or sets the details string.
        /// </summary>
        [Column("details")]
        public string Details
        {
            get
            {
                return details ?? model?.ToString() ?? string.Empty;
            }

            set
            {
                details = value;
                model = null;
            }
        }

        /// <summary>
        /// Gets or sets the additional message.
        /// </summary>
        [JsonPropertyName("model")]
        [NotMapped]
        public ContactModel Model
        {
            get
            {
                var s = details;
                if (s == null) return model;
                try
                {
                    model = JsonSerializer.Deserialize<ContactModel>(s);
                    SetProperty("Model", model);
                    return model;
                }
                catch (JsonException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (FormatException)
                {
                }
                catch (NotSupportedException)
                {
                }
                catch (NullReferenceException)
                {
                }
                finally
                {
                    details = null;
                }

                model = new ContactModel();
                SetProperty("Model", model);
                return model;
            }

            set
            {
                details = null;
                model = value;
                SetProperty("Model", model);
            }
        }
    }
}
