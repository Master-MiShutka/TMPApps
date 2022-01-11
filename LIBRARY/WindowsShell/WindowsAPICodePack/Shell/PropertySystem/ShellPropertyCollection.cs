// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using Microsoft.WindowsAPICodePack.Shell.Resources;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Creates a readonly collection of IProperty objects.
    /// </summary>
    public class ShellPropertyCollection : ReadOnlyCollection<IShellProperty>, IDisposable
    {
        #region Internal Constructor

        /// <summary>
        /// Creates a new Property collection given an IPropertyStore object
        /// </summary>
        /// <param name="nativePropertyStore">IPropertyStore</param>
        internal ShellPropertyCollection(IPropertyStore nativePropertyStore)
            : base(new List<IShellProperty>())
        {
            this.NativePropertyStore = nativePropertyStore;
            this.AddProperties(nativePropertyStore);
        }

        #endregion

        #region Public Constructor

        /// <summary>
        /// Creates a new Property collection given an IShellItem2 native interface
        /// </summary>
        /// <param name="parent">Parent ShellObject</param>
        public ShellPropertyCollection(ShellObject parent)
            : base(new List<IShellProperty>())
        {
            this.ParentShellObject = parent;
            IPropertyStore nativePropertyStore = null;
            try
            {
                nativePropertyStore = CreateDefaultPropertyStore(this.ParentShellObject);
                this.AddProperties(nativePropertyStore);
            }
            catch
            {
                if (parent != null)
                {
                    parent.Dispose();
                }

                throw;
            }
            finally
            {
                if (nativePropertyStore != null)
                {
                    Marshal.ReleaseComObject(nativePropertyStore);
                    nativePropertyStore = null;
                }
            }
        }

        /// <summary>
        /// Creates a new <c>ShellPropertyCollection</c> object with the specified file or folder path.
        /// </summary>
        /// <param name="path">The path to the file or folder.</param>
        public ShellPropertyCollection(string path) : this(ShellObjectFactory.Create(path))
        {
        }

        #endregion

        #region Private Methods

        private ShellObject ParentShellObject { get; set; }

        private IPropertyStore NativePropertyStore { get; set; }

        private void AddProperties(IPropertyStore nativePropertyStore)
        {
            uint propertyCount;
            PropertyKey propKey;

            // Populate the property collection
            nativePropertyStore.GetCount(out propertyCount);
            for (uint i = 0; i < propertyCount; i++)
            {
                nativePropertyStore.GetAt(i, out propKey);

                if (this.ParentShellObject != null)
                {
                    this.Items.Add(this.ParentShellObject.Properties.CreateTypedProperty(propKey));
                }
                else
                {
                    this.Items.Add(CreateTypedProperty(propKey, this.NativePropertyStore));
                }
            }
        }

        internal static IPropertyStore CreateDefaultPropertyStore(ShellObject shellObj)
        {
            IPropertyStore nativePropertyStore = null;

            Guid guid = new Guid(ShellIIDGuid.IPropertyStore);
            int hr = shellObj.NativeShellItem2.GetPropertyStore(
                   ShellNativeMethods.GetPropertyStoreOptions.BestEffort,
                   ref guid,
                   out nativePropertyStore);

            // throw on failure
            if (nativePropertyStore == null || !CoreErrorHelper.Succeeded(hr))
            {
                throw new ShellException(hr);
            }

            return nativePropertyStore;
        }

        #endregion

        #region Collection Public Methods

        /// <summary>
        /// Checks if a property with the given canonical name is available.
        /// </summary>
        /// <param name="canonicalName">The canonical name of the property.</param>
        /// <returns><B>True</B> if available, <B>false</B> otherwise.</returns>
        public bool Contains(string canonicalName)
        {
            if (string.IsNullOrEmpty(canonicalName))
            {
                throw new ArgumentException(LocalizedMessages.PropertyCollectionNullCanonicalName, nameof(canonicalName));
            }

            return this.Items.Any(p => p.CanonicalName == canonicalName);
        }

        /// <summary>
        /// Checks if a property with the given property key is available.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns><B>True</B> if available, <B>false</B> otherwise.</returns>
        public bool Contains(PropertyKey key)
        {
            return this.Items.Any(p => p.PropertyKey == key);
        }

        /// <summary>
        /// Gets the property associated with the supplied canonical name string.
        /// The canonical name property is case-sensitive.
        ///
        /// </summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <returns>The property associated with the canonical name, if found.</returns>
        /// <exception cref="IndexOutOfRangeException">Throws IndexOutOfRangeException
        /// if no matching property is found.</exception>
        public IShellProperty this[string canonicalName]
        {
            get
            {
                if (string.IsNullOrEmpty(canonicalName))
                {
                    throw new ArgumentException(LocalizedMessages.PropertyCollectionNullCanonicalName, nameof(canonicalName));
                }

                IShellProperty prop = this.Items.FirstOrDefault(p => p.CanonicalName == canonicalName);
                if (prop == null)
                {
                    throw new IndexOutOfRangeException(LocalizedMessages.PropertyCollectionCanonicalInvalidIndex);
                }

                return prop;
            }
        }

        /// <summary>
        /// Gets a property associated with the supplied property key.
        ///
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The property associated with the property key, if found.</returns>
        /// <exception cref="IndexOutOfRangeException">Throws IndexOutOfRangeException
        /// if no matching property is found.</exception>
        public IShellProperty this[PropertyKey key]
        {
            get
            {
                IShellProperty prop = this.Items.FirstOrDefault(p => p.PropertyKey == key);
                if (prop != null)
                {
                    return prop;
                }

                throw new IndexOutOfRangeException(LocalizedMessages.PropertyCollectionInvalidIndex);
            }
        }

        #endregion

        // TODO - ShellProperties.cs also has a similar class that is used for creating
        // a ShellObject specific IShellProperty. These 2 methods should be combined or moved to a
        // common location.
        internal static IShellProperty CreateTypedProperty(PropertyKey propKey, IPropertyStore nativePropertyStore)
        {
            return ShellPropertyFactory.CreateShellProperty(propKey, nativePropertyStore);
        }

        #region IDisposable Members

        /// <summary>
        /// Release the native and managed objects
        /// </summary>
        /// <param name="disposing">Indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.NativePropertyStore != null)
            {
                Marshal.ReleaseComObject(this.NativePropertyStore);
                this.NativePropertyStore = null;
            }
        }

        /// <summary>
        /// Release the native objects.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement the finalizer.
        /// </summary>
        ~ShellPropertyCollection()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
