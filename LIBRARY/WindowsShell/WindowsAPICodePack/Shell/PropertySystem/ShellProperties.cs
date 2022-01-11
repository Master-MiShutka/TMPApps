// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using Microsoft.WindowsAPICodePack.Shell.Resources;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Defines a partial class that implements helper methods for retrieving Shell properties
    /// using a canonical name, property key, or a strongly-typed property. Also provides
    /// access to all the strongly-typed system properties and default properties collections.
    /// </summary>
    public partial class ShellProperties : IDisposable
    {
        private ShellObject ParentShellObject { get; set; }

        private ShellPropertyCollection defaultPropertyCollection;

        internal ShellProperties(ShellObject parent)
        {
            this.ParentShellObject = parent;
        }

        /// <summary>
        /// Returns a property available in the default property collection using
        /// the given property key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>An IShellProperty.</returns>
        public IShellProperty GetProperty(PropertyKey key)
        {
            return this.CreateTypedProperty(key);
        }

        /// <summary>
        /// Returns a property available in the default property collection using
        /// the given canonical name.
        /// </summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <returns>An IShellProperty.</returns>
        public IShellProperty GetProperty(string canonicalName)
        {
            return this.CreateTypedProperty(canonicalName);
        }

        /// <summary>
        /// Returns a strongly typed property available in the default property collection using
        /// the given property key.
        /// </summary>
        /// <typeparam name="T">The type of property to retrieve.</typeparam>
        /// <param name="key">The property key.</param>
        /// <returns>A strongly-typed ShellProperty for the given property key.</returns>
        public ShellProperty<T> GetProperty<T>(PropertyKey key)
        {
            return this.CreateTypedProperty(key) as ShellProperty<T>;
        }

        /// <summary>
        /// Returns a strongly typed property available in the default property collection using
        /// the given canonical name.
        /// </summary>
        /// <typeparam name="T">The type of property to retrieve.</typeparam>
        /// <param name="canonicalName">The canonical name.</param>
        /// <returns>A strongly-typed ShellProperty for the given canonical name.</returns>
        public ShellProperty<T> GetProperty<T>(string canonicalName)
        {
            return this.CreateTypedProperty(canonicalName) as ShellProperty<T>;
        }

        private PropertySystem propertySystem;

        /// <summary>
        /// Gets all the properties for the system through an accessor.
        /// </summary>
        public PropertySystem System
        {
            get
            {
                if (this.propertySystem == null)
                {
                    this.propertySystem = new PropertySystem(this.ParentShellObject);
                }

                return this.propertySystem;
            }
        }

        /// <summary>
        /// Gets the collection of all the default properties for this item.
        /// </summary>
        public ShellPropertyCollection DefaultPropertyCollection
        {
            get
            {
                if (this.defaultPropertyCollection == null)
                {
                    this.defaultPropertyCollection = new ShellPropertyCollection(this.ParentShellObject);
                }

                return this.defaultPropertyCollection;
            }
        }

        /// <summary>
        /// Returns the shell property writer used when writing multiple properties.
        /// </summary>
        /// <returns>A ShellPropertyWriter.</returns>
        /// <remarks>Use the Using pattern with the returned ShellPropertyWriter or
        /// manually call the Close method on the writer to commit the changes
        /// and dispose the writer</remarks>
        public ShellPropertyWriter GetPropertyWriter()
        {
            return new ShellPropertyWriter(this.ParentShellObject);
        }

        internal IShellProperty CreateTypedProperty<T>(PropertyKey propKey)
        {
            ShellPropertyDescription desc = ShellPropertyDescriptionsCache.Cache.GetPropertyDescription(propKey);
            return new ShellProperty<T>(propKey, desc, this.ParentShellObject);
        }

        internal IShellProperty CreateTypedProperty(PropertyKey propKey)
        {
            return ShellPropertyFactory.CreateShellProperty(propKey, this.ParentShellObject);
        }

        internal IShellProperty CreateTypedProperty(string canonicalName)
        {
            // Otherwise, call the native PropertyStore method
            PropertyKey propKey;

            int result = PropertySystemNativeMethods.PSGetPropertyKeyFromName(canonicalName, out propKey);

            if (!CoreErrorHelper.Succeeded(result))
            {
                throw new ArgumentException(
                    LocalizedMessages.ShellInvalidCanonicalName,
                    Marshal.GetExceptionForHR(result));
            }

            return this.CreateTypedProperty(propKey);
        }

        #region IDisposable Members

        /// <summary>
        /// Cleans up memory
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up memory
        /// </summary>
        protected virtual void Dispose(bool disposed)
        {
            if (disposed && this.defaultPropertyCollection != null)
            {
                this.defaultPropertyCollection.Dispose();
            }
        }

        #endregion
    }
}