﻿// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.WindowsAPICodePack.Shell.Resources;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Creates a property writer capable of setting multiple properties for a given ShellObject.
    /// </summary>
    public class ShellPropertyWriter : IDisposable
    {

        private ShellObject parentShellObject;

        // Reference to our writable PropertyStore
        internal IPropertyStore writablePropStore;

        internal ShellPropertyWriter(ShellObject parent)
        {
            this.ParentShellObject = parent;

            // Open the property store for this shell object...
            Guid guid = new Guid(ShellIIDGuid.IPropertyStore);

            try
            {
                int hr = this.ParentShellObject.NativeShellItem2.GetPropertyStore(
                        ShellNativeMethods.GetPropertyStoreOptions.ReadWrite,
                        ref guid,
                        out this.writablePropStore);

                if (!CoreErrorHelper.Succeeded(hr))
                {
                    throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty,
                        Marshal.GetExceptionForHR(hr));
                }
                else
                {
                    // If we succeed in creating a valid property store for this ShellObject,
                    // then set it on the parent shell object for others to use.
                    // Once this writer is closed/commited, we will set the
                    if (this.ParentShellObject.NativePropertyStore == null)
                    {
                        this.ParentShellObject.NativePropertyStore = this.writablePropStore;
                    }
                }
            }
            catch (InvalidComObjectException e)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty, e);
            }
            catch (InvalidCastException)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty);
            }
        }

        /// <summary>
        /// Reference to parent ShellObject (associated with this writer)
        /// </summary>
        protected ShellObject ParentShellObject
        {
            get => this.parentShellObject;
            private set => this.parentShellObject = value;
        }

        /// <summary>
        /// Writes the given property key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The value associated with the key.</param>
        public void WriteProperty(PropertyKey key, object value)
        {
            this.WriteProperty(key, value, true);
        }

        /// <summary>
        /// Writes the given property key and value. To allow truncation of the given value, set allowTruncatedValue
        /// to true.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        /// <exception cref="System.InvalidOperationException">If the writable property store is already
        /// closed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">If AllowTruncatedValue is set to false
        /// and while setting the value on the property it had to be truncated in a string or rounded in
        /// a numeric value.</exception>
        public void WriteProperty(PropertyKey key, object value, bool allowTruncatedValue)
        {
            if (this.writablePropStore == null)
            {
                throw new InvalidOperationException("Writeable store has been closed.");
            }

            using (PropVariant propVar = PropVariant.FromObject(value))
            {
                HResult result = this.writablePropStore.SetValue(ref key, propVar);

                if (!allowTruncatedValue && ((int)result == ShellNativeMethods.InPlaceStringTruncated))
                {
                    // At this point we can't revert back the commit
                    // so don't commit, close the property store and throw an exception
                    // to let the user know.
                    Marshal.ReleaseComObject(this.writablePropStore);
                    this.writablePropStore = null;

                    throw new ArgumentOutOfRangeException(nameof(value), LocalizedMessages.ShellPropertyValueTruncated);
                }

                if (!CoreErrorHelper.Succeeded(result))
                {
                    throw new PropertySystemException(LocalizedMessages.ShellPropertySetValue, Marshal.GetExceptionForHR((int)result));
                }
            }
        }

        /// <summary>
        /// Writes the specified property given the canonical name and a value.
        /// </summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <param name="value">The property value.</param>
        public void WriteProperty(string canonicalName, object value)
        {
            this.WriteProperty(canonicalName, value, true);
        }

        /// <summary>
        /// Writes the specified property given the canonical name and a value. To allow truncation of the given value, set allowTruncatedValue
        /// to true.
        /// </summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        /// <exception cref="System.ArgumentException">If the given canonical name is not valid.</exception>
        public void WriteProperty(string canonicalName, object value, bool allowTruncatedValue)
        {
            // Get the PropertyKey using the canonicalName passed in
            PropertyKey propKey;

            int result = PropertySystemNativeMethods.PSGetPropertyKeyFromName(canonicalName, out propKey);

            if (!CoreErrorHelper.Succeeded(result))
            {
                throw new ArgumentException(
                    LocalizedMessages.ShellInvalidCanonicalName,
                    Marshal.GetExceptionForHR(result));
            }

            this.WriteProperty(propKey, value, allowTruncatedValue);
        }

        /// <summary>
        /// Writes the specified property using an IShellProperty and a value.
        /// </summary>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        public void WriteProperty(IShellProperty shellProperty, object value)
        {
            this.WriteProperty(shellProperty, value, true);
        }

        /// <summary>
        /// Writes the specified property given an IShellProperty and a value. To allow truncation of the given value, set allowTruncatedValue
        /// to true.
        /// </summary>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        public void WriteProperty(IShellProperty shellProperty, object value, bool allowTruncatedValue)
        {
            if (shellProperty == null)
            {
                throw new ArgumentNullException(nameof(shellProperty));
            }

            this.WriteProperty(shellProperty.PropertyKey, value, allowTruncatedValue);
        }

        /// <summary>
        /// Writes the specified property using a strongly-typed ShellProperty and a value.
        /// </summary>
        /// <typeparam name="T">The type of the property name.</typeparam>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        public void WriteProperty<T>(ShellProperty<T> shellProperty, T value)
        {
            this.WriteProperty<T>(shellProperty, value, true);
        }

        /// <summary>
        /// Writes the specified property given a strongly-typed ShellProperty and a value. To allow truncation of the given value, set allowTruncatedValue
        /// to true.
        /// </summary>
        /// <typeparam name="T">The type of the property name.</typeparam>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        public void WriteProperty<T>(ShellProperty<T> shellProperty, T value, bool allowTruncatedValue)
        {
            if (shellProperty == null)
            {
                throw new ArgumentNullException(nameof(shellProperty));
            }

            this.WriteProperty(shellProperty.PropertyKey, value, allowTruncatedValue);
        }

        #region IDisposable Members

        /// <summary>
        /// Release the native objects.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        ~ShellPropertyWriter()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Release the native and managed objects.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            this.Close();
        }

        /// <summary>
        /// Call this method to commit the writes (calls to WriteProperty method)
        /// and dispose off the writer.
        /// </summary>
        public void Close()
        {
            // Close the property writer (commit, etc)
            if (this.writablePropStore != null)
            {
                this.writablePropStore.Commit();

                Marshal.ReleaseComObject(this.writablePropStore);
                this.writablePropStore = null;
            }

            this.ParentShellObject.NativePropertyStore = null;
        }

        #endregion
    }
}