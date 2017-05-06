using System.IO;
using Microsoft.VisualStudio.Settings;
using GitHub.Extensions;
using System;

namespace GitHub.Helpers
{
    public class SettingsStore
    {
        readonly WritableSettingsStore store;
        readonly string root;
        public SettingsStore(WritableSettingsStore store, string root)
        {
            Guard.ArgumentNotNull(store, nameof(store));
            Guard.ArgumentNotNull(root, nameof(root));
            Guard.ArgumentNotEmptyString(root, nameof(root));
            this.store = store;
            this.root = root;
        }

        public object Read(string property, object defaultValue)
        {
            return Read(null, property, defaultValue);
        }

        /// <summary>
        /// Read from a settings store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subpath">The subcollection path (appended to the path passed to the constructor)</param>
        /// <param name="property">The property name to read</param>
        /// <param name="defaultValue">The default value to use in case the property doesn't exist.
        /// The type of the default value will be used to figure out the proper way to read the property, so if pass null,
        /// the property will be read as a string (which may or may not be what you want)</param>
        /// <returns></returns>
        public object Read(string subpath, string property, object defaultValue)
        {
            Guard.ArgumentNotNull(property, nameof(property));
            Guard.ArgumentNotEmptyString(property, nameof(property));

            var collection = subpath != null ? Path.Combine(root, subpath) : root;
            store.CreateCollection(collection);
            DateTimeOffset offset;

            if (defaultValue is bool)
                return store.GetBoolean(collection, property, (bool)defaultValue);
            else if (defaultValue is int)
                return store.GetInt32(collection, property, (int)defaultValue);
            else if (defaultValue is uint)
                return store.GetUInt32(collection, property, (uint)defaultValue);
            else if (defaultValue is long)
                return store.GetInt64(collection, property, (long)defaultValue);
            else if (defaultValue is ulong)
                return store.GetUInt64(collection, property, (ulong)defaultValue);
            else if (defaultValue is DateTimeOffset)
                return DateTimeOffset.TryParse(defaultValue?.ToString(), out offset) ? offset : defaultValue;
            return store.GetString(collection, property, defaultValue?.ToString() ?? "");
        }

        public void Write(string property, object value)
        {
            Write(null, property, value);
        }

        public void Write(string subpath, string property, object value)
        {
            Guard.ArgumentNotNull(property, nameof(property));
            Guard.ArgumentNotEmptyString(property, nameof(property));

            var collection = subpath != null ? Path.Combine(root, subpath) : root;
            store.CreateCollection(collection);

            if (value is bool)
                store.SetBoolean(collection, property, (bool)value);
            else if (value is int)
                store.SetInt32(collection, property, (int)value);
            else if (value is uint)
                store.SetUInt32(collection, property, (uint)value);
            else if (value is long)
                store.SetInt64(collection, property, (long)value);
            else if (value is ulong)
                store.SetUInt64(collection, property, (ulong)value);
            else
                store.SetString(collection, property, value?.ToString() ?? "");
        }
    }
}