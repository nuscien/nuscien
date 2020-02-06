using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public bool IsReadonly { get; private set; }

        /// <summary>
        /// Sets the properties read-only to access.
        /// </summary>
        public void SetReadonly()
        {
            IsReadonly = true;
        }

        /// <inheritdoc />
        protected new bool SetCurrentProperty(object value, [CallerMemberName] string key = null)
        {
            if (IsReadonly) return false;
            return base.SetProperty(key, value);
        }

        /// <inheritdoc />
        protected new bool SetProperty(string key, object value)
        {
            if (IsReadonly) return false;
            return base.SetProperty(key, value);
        }
    }
}
