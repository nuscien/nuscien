using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Trivial.Reflection;
using Trivial.Text;

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
        /// Gets or sets a value indicating whether the properties are slim.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        protected bool IsSlim { get; set; }

        /// <summary>
        /// Sets the properties read-only to access.
        /// </summary>
        public void SetPropertiesReadonly()
        {
            PropertiesSettingPolicy = preferThrowExceptionWhenSetPropertyFailed ? PropertySettingPolicies.Forbidden : PropertySettingPolicies.Skip;
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="key">The additional key.</param>
        /// <returns>A property value; or null if slim.</returns>
        protected T GetCurrentPropertyWhenNotSlim<T>(T defaultValue = default, [CallerMemberName] string key = null) where T : class
        {
            return GetCurrentProperty(defaultValue, key);
        }

        /// <summary>
        /// Writes this instance to the specified writer as a JSON value.
        /// </summary>
        /// <param name="writer">The writer to which to write this instance.</param>
        public new virtual void WriteTo(Utf8JsonWriter writer) => JsonObject.ConvertFrom(this).WriteTo(writer);

        internal T GetPropertySerialized<T>(string key, string s, Func<T> maker) where T : class
        {
            var model = GetProperty<T>(key);
            if (string.IsNullOrEmpty(s)) return model;
            try
            {
                model = JsonSerializer.Deserialize<T>(s);
                SetProperty(key, model);
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

            try
            {
                model = maker?.Invoke() ?? Activator.CreateInstance<T>();
            }
            catch (MemberAccessException)
            {
                model = null;
            }

            SetProperty(key, model);
            return model;

        }

        /// <summary>
        /// Converts to JSON object.
        /// </summary>
        /// <param name="value">The entity to convert.</param>
        /// <returns>A JSON object instance.</returns>
        public static explicit operator JsonObject(ReadonlyObservableProperties value)
        {
            try
            {
                return JsonObject.ConvertFrom(value);
            }
            catch (JsonException ex)
            {
                throw new InvalidCastException(ex.Message, ex);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCastException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidCastException(ex.Message, ex);
            }
            catch (NullReferenceException ex)
            {
                throw new InvalidCastException(ex.Message, ex);
            }
        }
    }
}
