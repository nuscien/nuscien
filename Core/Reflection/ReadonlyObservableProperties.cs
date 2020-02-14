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
        private bool preferThrowExceptionWhenSetPropertyFailed;

        /// <summary>
        /// Gets a value indicating whether the properites are read-only.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool IsPropertyReadonly => PropertiesSettingPolicy != PropertySettingPolicies.Allow;

        /// <summary>
        /// Gets or sets a value indicating whether need throw an exception when set property failed.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public bool NeedThrowExceptionWhenSetPropertyFailed
        {
            get
            {
                if (PropertiesSettingPolicy == PropertySettingPolicies.Allow) return preferThrowExceptionWhenSetPropertyFailed;
                return PropertiesSettingPolicy == PropertySettingPolicies.Forbidden;
            }

            set
            {
                preferThrowExceptionWhenSetPropertyFailed = value;
                if (PropertiesSettingPolicy == PropertySettingPolicies.Allow) return;
                PropertiesSettingPolicy = value ? PropertySettingPolicies.Forbidden : PropertySettingPolicies.Skip;
            }
        }

        /// <summary>
        /// Sets the properties read-only to access.
        /// </summary>
        public void SetPropertiesReadonly()
        {
            PropertiesSettingPolicy = preferThrowExceptionWhenSetPropertyFailed ? PropertySettingPolicies.Forbidden : PropertySettingPolicies.Skip;
        }
    }
}
