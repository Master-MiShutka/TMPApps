// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Microsoft.WindowsAPICodePack.Shell.Resources;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Defines a strongly-typed property object.
    /// All writable property objects must be of this type
    /// to be able to call the value setter.
    /// </summary>
    /// <typeparam name="T">The type of this property's value.
    /// Because a property value can be empty, only nullable types
    /// are allowed.</typeparam>
    public class ShellProperty<T> : IShellProperty
    {
        #region Private Fields

        private PropertyKey propertyKey;
        private string imageReferencePath = null;
        private int? imageReferenceIconIndex;
        private ShellPropertyDescription description = null;

        #endregion

        #region Private Methods

        private ShellObject ParentShellObject { get; set; }

        private IPropertyStore NativePropertyStore { get; set; }

        private void GetImageReference()
        {
            IPropertyStore store = ShellPropertyCollection.CreateDefaultPropertyStore(this.ParentShellObject);

            using (PropVariant propVar = new PropVariant())
            {
                store.GetValue(ref this.propertyKey, propVar);

                Marshal.ReleaseComObject(store);
                store = null;

                string refPath;
                ((IPropertyDescription2)this.Description.NativePropertyDescription).GetImageReferenceForValue(
                    propVar, out refPath);

                if (refPath == null)
                {
                    return;
                }

                int index = ShellNativeMethods.PathParseIconLocation(ref refPath);
                if (refPath != null)
                {
                    this.imageReferencePath = refPath;
                    this.imageReferenceIconIndex = index;
                }
            }
        }

        private void StorePropVariantValue(PropVariant propVar)
        {
            Guid guid = new Guid(ShellIIDGuid.IPropertyStore);
            IPropertyStore writablePropStore = null;
            try
            {
                int hr = this.ParentShellObject.NativeShellItem2.GetPropertyStore(
                        ShellNativeMethods.GetPropertyStoreOptions.ReadWrite,
                        ref guid,
                        out writablePropStore);

                if (!CoreErrorHelper.Succeeded(hr))
                {
                    throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty,
                        Marshal.GetExceptionForHR(hr));
                }

                HResult result = writablePropStore.SetValue(ref this.propertyKey, propVar);

                if (!this.AllowSetTruncatedValue && (int)result == ShellNativeMethods.InPlaceStringTruncated)
                {
                    throw new ArgumentOutOfRangeException(nameof(propVar), LocalizedMessages.ShellPropertyValueTruncated);
                }

                if (!CoreErrorHelper.Succeeded(result))
                {
                    throw new PropertySystemException(LocalizedMessages.ShellPropertySetValue, Marshal.GetExceptionForHR((int)result));
                }

                writablePropStore.Commit();
            }
            catch (InvalidComObjectException e)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty, e);
            }
            catch (InvalidCastException)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty);
            }
            finally
            {
                if (writablePropStore != null)
                {
                    Marshal.ReleaseComObject(writablePropStore);
                    writablePropStore = null;
                }
            }
        }

        #endregion

        #region Internal Constructor

        /// <summary>
        /// Constructs a new Property object
        /// </summary>
        /// <param name="propertyKey"></param>
        /// <param name="description"></param>
        /// <param name="parent"></param>
        internal ShellProperty(
            PropertyKey propertyKey,
            ShellPropertyDescription description,
            ShellObject parent)
        {
            this.propertyKey = propertyKey;
            this.description = description;
            this.ParentShellObject = parent;
            this.AllowSetTruncatedValue = false;
        }

        /// <summary>
        /// Constructs a new Property object
        /// </summary>
        /// <param name="propertyKey"></param>
        /// <param name="description"></param>
        /// <param name="propertyStore"></param>
        internal ShellProperty(
            PropertyKey propertyKey,
            ShellPropertyDescription description,
            IPropertyStore propertyStore)
        {
            this.propertyKey = propertyKey;
            this.description = description;
            this.NativePropertyStore = propertyStore;
            this.AllowSetTruncatedValue = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the strongly-typed value of this property.
        /// The value of the property is cleared if the value is set to null.
        /// </summary>
        /// <exception cref="System.Runtime.InteropServices.COMException">
        /// If the property value cannot be retrieved or updated in the Property System</exception>
        /// <exception cref="NotSupportedException">If the type of this property is not supported; e.g. writing a binary object.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <see cref="AllowSetTruncatedValue"/> is false, and either
        /// a string value was truncated or a numeric value was rounded.</exception>
        public T Value
        {
            get
            {
                // Make sure we load the correct type
                Debug.Assert(this.ValueType == ShellPropertyFactory.VarEnumToSystemType(this.Description.VarEnumType));

                using (PropVariant propVar = new PropVariant())
                {
                    if (this.ParentShellObject.NativePropertyStore != null)
                    {
                        // If there is a valid property store for this shell object, then use it.
                        this.ParentShellObject.NativePropertyStore.GetValue(ref this.propertyKey, propVar);
                    }
                    else if (this.ParentShellObject != null)
                    {
                        // Use IShellItem2.GetProperty instead of creating a new property store
                        // The file might be locked. This is probably quicker, and sufficient for what we need
                        this.ParentShellObject.NativeShellItem2.GetProperty(ref this.propertyKey, propVar);
                    }
                    else if (this.NativePropertyStore != null)
                    {
                        this.NativePropertyStore.GetValue(ref this.propertyKey, propVar);
                    }

                    // Get the value
                    return propVar.Value != null ? (T)propVar.Value : default(T);
                }
            }

            set
            {
                // Make sure we use the correct type
                Debug.Assert(this.ValueType == ShellPropertyFactory.VarEnumToSystemType(this.Description.VarEnumType));

                if (typeof(T) != this.ValueType)
                {
                    throw new NotSupportedException(
                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        LocalizedMessages.ShellPropertyWrongType, this.ValueType.Name));
                }

                if (value is Nullable)
                {
                    Type t = typeof(T);
                    PropertyInfo pi = t.GetProperty("HasValue");
                    if (pi != null)
                    {
                        bool hasValue = (bool)pi.GetValue(value, null);
                        if (!hasValue)
                        {
                            this.ClearValue();
                            return;
                        }
                    }
                }
                else if (value == null)
                {
                    this.ClearValue();
                    return;
                }

                if (this.ParentShellObject != null)
                {
                    using (ShellPropertyWriter propertyWriter = this.ParentShellObject.Properties.GetPropertyWriter())
                    {
                        propertyWriter.WriteProperty<T>(this, value, this.AllowSetTruncatedValue);
                    }
                }
                else if (this.NativePropertyStore != null)
                {
                    throw new InvalidOperationException(LocalizedMessages.ShellPropertyCannotSetProperty);
                }
            }
        }

        #endregion

        #region IProperty Members

        /// <summary>
        /// Gets the property key identifying this property.
        /// </summary>
        public PropertyKey PropertyKey => this.propertyKey;

        /// <summary>
        /// Returns a formatted, Unicode string representation of a property value.
        /// </summary>
        /// <param name="format">One or more of the PropertyDescriptionFormat flags
        /// that indicate the desired format.</param>
        /// <param name="formattedString">The formatted value as a string, or null if this property
        /// cannot be formatted for display.</param>
        /// <returns>True if the method successfully locates the formatted string; otherwise
        /// False.</returns>
        public bool TryFormatForDisplay(PropertyDescriptionFormatOptions format, out string formattedString)
        {

            if (this.Description == null || this.Description.NativePropertyDescription == null)
            {
                // We cannot do anything without a property description
                formattedString = null;
                return false;
            }

            IPropertyStore store = ShellPropertyCollection.CreateDefaultPropertyStore(this.ParentShellObject);

            using (PropVariant propVar = new PropVariant())
            {
                store.GetValue(ref this.propertyKey, propVar);

                // Release the Propertystore
                Marshal.ReleaseComObject(store);
                store = null;

                HResult hr = this.Description.NativePropertyDescription.FormatForDisplay(propVar, ref format, out formattedString);

                // Sometimes, the value cannot be displayed properly, such as for blobs
                // or if we get argument exception
                if (!CoreErrorHelper.Succeeded(hr))
                {
                    formattedString = null;
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Returns a formatted, Unicode string representation of a property value.
        /// </summary>
        /// <param name="format">One or more of the PropertyDescriptionFormat flags
        /// that indicate the desired format.</param>
        /// <returns>The formatted value as a string, or null if this property
        /// cannot be formatted for display.</returns>
        public string FormatForDisplay(PropertyDescriptionFormatOptions format)
        {
            string formattedString;

            if (this.Description == null || this.Description.NativePropertyDescription == null)
            {
                // We cannot do anything without a property description
                return null;
            }

            IPropertyStore store = ShellPropertyCollection.CreateDefaultPropertyStore(this.ParentShellObject);

            using (PropVariant propVar = new PropVariant())
            {
                store.GetValue(ref this.propertyKey, propVar);

                // Release the Propertystore
                Marshal.ReleaseComObject(store);
                store = null;

                HResult hr = this.Description.NativePropertyDescription.FormatForDisplay(propVar, ref format, out formattedString);

                // Sometimes, the value cannot be displayed properly, such as for blobs
                // or if we get argument exception
                if (!CoreErrorHelper.Succeeded(hr))
                {
                    throw new ShellException(hr);
                }

                return formattedString;
            }
        }

        /// <summary>
        /// Get the property description object.
        /// </summary>
        public ShellPropertyDescription Description => this.description;

        /// <summary>
        /// Gets the case-sensitive name of a property as it is known to the system,
        /// regardless of its localized name.
        /// </summary>
        public string CanonicalName => this.Description.CanonicalName;

        /// <summary>
        /// Clears the value of the property.
        /// </summary>
        public void ClearValue()
        {
            using (PropVariant propVar = new PropVariant())
            {
                this.StorePropVariantValue(propVar);
            }
        }

        /// <summary>
        /// Gets the value for this property using the generic Object type.
        /// To obtain a specific type for this value, use the more type strong
        /// Property&lt;T&gt; class.
        /// Also, you can only set a value for this type using Property&lt;T&gt;
        /// </summary>
        public object ValueAsObject
        {
            get
            {
                using (PropVariant propVar = new PropVariant())
                {
                    if (this.ParentShellObject != null)
                    {

                        IPropertyStore store = ShellPropertyCollection.CreateDefaultPropertyStore(this.ParentShellObject);

                        store.GetValue(ref this.propertyKey, propVar);

                        Marshal.ReleaseComObject(store);
                        store = null;
                    }
                    else if (this.NativePropertyStore != null)
                    {
                        this.NativePropertyStore.GetValue(ref this.propertyKey, propVar);
                    }

                    return propVar != null ? propVar.Value : null;
                }
            }
        }

        /// <summary>
        /// Gets the associated runtime type.
        /// </summary>
        public Type ValueType
        {
            get
            {
                // The type for this object need to match that of the description
                Debug.Assert(this.Description.ValueType == typeof(T));

                return this.Description.ValueType;
            }
        }

        /// <summary>
        /// Gets the image reference path and icon index associated with a property value (Windows 7 only).
        /// </summary>
        public IconReference IconReference
        {
            get
            {
                if (!CoreHelpers.RunningOnWin7)
                {
                    throw new PlatformNotSupportedException(LocalizedMessages.ShellPropertyWindows7);
                }

                this.GetImageReference();
                int index = this.imageReferenceIconIndex.HasValue ? this.imageReferenceIconIndex.Value : -1;

                return new IconReference(this.imageReferencePath, index);
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if a value can be truncated. The default for this property is false.
        /// </summary>
        /// <remarks>
        /// An <see cref="ArgumentOutOfRangeException"/> will be thrown if
        /// this property is not set to true, and a property value was set
        /// but later truncated.
        ///
        /// </remarks>
        public bool AllowSetTruncatedValue { get; set; }

        #endregion
    }
}
