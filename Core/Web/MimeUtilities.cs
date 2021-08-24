using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Trivial.Collection;
using Trivial.Text;

namespace Trivial.Web
{
    /// <summary>
    /// The MIME constants.
    /// </summary>
    public static partial class MimeUtilities
    {
        private static MethodInfo method;
        private static PropertyInfo handlerProp;

        /// <summary>
        /// The name of MIME.
        /// </summary>
        public const string Name = "Multipurpose Internet Mail Extensions";

        /// <summary>
        /// Gets or sets the customized MIME handler.
        /// </summary>
        public static Func<FileInfo, string> GetHandler
        {
            get
            {
                var prop = CustomizedGetHandlerProperty();
                return prop?.CanRead == true && (prop.GetValue(null) is Func<FileInfo, string> h) ? h : null;
            }

            set
            {
                var prop = CustomizedGetHandlerProperty();
                if (prop?.CanWrite != true) return;
                prop.SetValue(null, value);
            }
        }

        /// <summary>
        /// Gets the MIME content type by file extension part.
        /// </summary>
        /// <param name="file">The file information.</param>
        /// <returns>The MIME content type.</returns>
        public static string Get(FileInfo file)
            => Get(file, WebFormat.StreamMIME);

        /// <summary>
        /// Gets the MIME content type by file extension part.
        /// </summary>
        /// <param name="file">The file information.</param>
        /// <param name="returnNullIfUnsupported">true if returns null if not supported; otherwise, false.</param>
        /// <returns>The MIME content type.</returns>
        public static string Get(FileInfo file, bool returnNullIfUnsupported)
            => Get(file, returnNullIfUnsupported ? null : WebFormat.StreamMIME);

        /// <summary>
        /// Gets the MIME content type by file extension part.
        /// </summary>
        /// <param name="file">The file information.</param>
        /// <param name="defaultMime">The default MIME content type.</param>
        /// <returns>The MIME content type.</returns>
        public static string Get(FileInfo file, string defaultMime)
        {
            if (file == null) return null;
            if (string.IsNullOrWhiteSpace(file?.Extension)) return defaultMime;
            if (method == null)
                method = typeof(WebFormat).GetMethod("GetMime", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(FileInfo) }, null);
            if (method == null)
                method = typeof(WebFormat).GetMethod("GetMime", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(FileInfo) }, null);
            if (method == null) return defaultMime;
            var r = method.Invoke(null, new object[] { file });
            if (r == null) return defaultMime;
            try
            {
                return (string)r;
            }
            catch (InvalidCastException)
            {
            }

            return defaultMime;
        }

        private static PropertyInfo CustomizedGetHandlerProperty()
        {
            if (handlerProp != null) return handlerProp;
            handlerProp = typeof(WebFormat).GetProperty("GetMimeHandler", BindingFlags.Static | BindingFlags.NonPublic);
            if (handlerProp != null) return handlerProp;
            handlerProp = typeof(WebFormat).GetProperty("MimeMapping", BindingFlags.Static | BindingFlags.Public);
            return handlerProp;
        }
    }
}
