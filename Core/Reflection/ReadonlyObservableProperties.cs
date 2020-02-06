using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

using Trivial.Reflection;

namespace NuScien.Reflection
{
    /// <summary>
    /// The read-only observable properties.
    /// </summary>
    public class ReadonlyObservableProperties : BaseObservableProperties
    {
        /// <summary>
        /// Gets a value indicating whether the properites are read-only.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool IsPropertyReadonly { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether need throw an exception when set property failed.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool NeedThrowExceptionWhenSetPropertyFailed { get; set; }

        /// <summary>
        /// Sets the properties read-only to access.
        /// </summary>
        public void SetPropertiesReadonly()
        {
            IsPropertyReadonly = true;
        }

        /// <inheritdoc />
        protected new bool SetCurrentProperty(object value, [CallerMemberName] string key = null)
        {
            if (IsPropertyReadonly)
            {
                if (NeedThrowExceptionWhenSetPropertyFailed)
                    throw new InvalidOperationException("Forbidden to set property.", new ArgumentException("The property is forbidden to set.", key));
                return false;
            }

            var r = base.SetProperty(key, value);
            if (!r && NeedThrowExceptionWhenSetPropertyFailed)
                throw new InvalidOperationException("Set property failed.", new ArgumentException("Set property failed.", key));
            return r;
        }

        /// <inheritdoc />
        protected new bool SetProperty(string key, object value)
        {
            if (IsPropertyReadonly)
            {
                if (NeedThrowExceptionWhenSetPropertyFailed)
                    throw new InvalidOperationException("Forbidden to set property.", new ArgumentException("The property is forbidden to set.", key));
                return false;
            }

            var r = base.SetProperty(key, value);
            if (!r && NeedThrowExceptionWhenSetPropertyFailed)
                throw new InvalidOperationException("Set property failed.", new ArgumentException("Set property failed.", key));
            return r;
        }
    }
}
