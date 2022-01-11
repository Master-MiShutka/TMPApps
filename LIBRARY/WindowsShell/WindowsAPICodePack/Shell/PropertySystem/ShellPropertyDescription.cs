﻿// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Defines the shell property description information for a property.
    /// </summary>
    public class ShellPropertyDescription : IDisposable
    {
        #region Private Fields

        private IPropertyDescription nativePropertyDescription;
        private string canonicalName;
        private PropertyKey propertyKey;
        private string displayName;
        private string editInvitation;
        private VarEnum? varEnumType = null;
        private PropertyDisplayType? displayType;
        private PropertyAggregationType? aggregationTypes;
        private uint? defaultColumWidth;
        private PropertyTypeOptions? propertyTypeFlags;
        private PropertyViewOptions? propertyViewFlags;
        private Type valueType;
        private ReadOnlyCollection<ShellPropertyEnumType> propertyEnumTypes;
        private PropertyColumnStateOptions? columnState;
        private PropertyConditionType? conditionType;
        private PropertyConditionOperation? conditionOperation;
        private PropertyGroupingRange? groupingRange;
        private PropertySortDescription? sortDescription;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the case-sensitive name of a property as it is known to the system,
        /// regardless of its localized name.
        /// </summary>
        public string CanonicalName
        {
            get
            {
                if (this.canonicalName == null)
                {
                    PropertySystemNativeMethods.PSGetNameFromPropertyKey(ref this.propertyKey, out this.canonicalName);
                }

                return this.canonicalName;
            }
        }

        /// <summary>
        /// Gets the property key identifying the underlying property.
        /// </summary>
        public PropertyKey PropertyKey => this.propertyKey;

        /// <summary>
        /// Gets the display name of the property as it is shown in any user interface (UI).
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (this.NativePropertyDescription != null && this.displayName == null)
                {
                    IntPtr dispNameptr = IntPtr.Zero;

                    HResult hr = this.NativePropertyDescription.GetDisplayName(out dispNameptr);

                    if (CoreErrorHelper.Succeeded(hr) && dispNameptr != IntPtr.Zero)
                    {
                        this.displayName = Marshal.PtrToStringUni(dispNameptr);

                        // Free the string
                        Marshal.FreeCoTaskMem(dispNameptr);
                    }
                }

                return this.displayName;
            }
        }

        /// <summary>
        /// Gets the text used in edit controls hosted in various dialog boxes.
        /// </summary>
        public string EditInvitation
        {
            get
            {
                if (this.NativePropertyDescription != null && this.editInvitation == null)
                {
                    // EditInvitation can be empty, so ignore the HR value, but don't throw an exception
                    IntPtr ptr = IntPtr.Zero;

                    HResult hr = this.NativePropertyDescription.GetEditInvitation(out ptr);

                    if (CoreErrorHelper.Succeeded(hr) && ptr != IntPtr.Zero)
                    {
                        this.editInvitation = Marshal.PtrToStringUni(ptr);

                        // Free the string
                        Marshal.FreeCoTaskMem(ptr);
                    }
                }

                return this.editInvitation;
            }
        }

        /// <summary>
        /// Gets the VarEnum OLE type for this property.
        /// </summary>
        public VarEnum VarEnumType
        {
            get
            {
                if (this.NativePropertyDescription != null && this.varEnumType == null)
                {
                    VarEnum tempType;

                    HResult hr = this.NativePropertyDescription.GetPropertyType(out tempType);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.varEnumType = tempType;
                    }
                }

                return this.varEnumType.HasValue ? this.varEnumType.Value : default(VarEnum);
            }
        }

        /// <summary>
        /// Gets the .NET system type for a value of this property, or
        /// null if the value is empty.
        /// </summary>
        public Type ValueType
        {
            get
            {
                if (this.valueType == null)
                {
                    this.valueType = ShellPropertyFactory.VarEnumToSystemType(this.VarEnumType);
                }

                return this.valueType;
            }
        }

        /// <summary>
        /// Gets the current data type used to display the property.
        /// </summary>
        public PropertyDisplayType DisplayType
        {
            get
            {
                if (this.NativePropertyDescription != null && this.displayType == null)
                {
                    PropertyDisplayType tempDisplayType;

                    HResult hr = this.NativePropertyDescription.GetDisplayType(out tempDisplayType);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.displayType = tempDisplayType;
                    }
                }

                return this.displayType.HasValue ? this.displayType.Value : default(PropertyDisplayType);
            }
        }

        /// <summary>
        /// Gets the default user interface (UI) column width for this property.
        /// </summary>
        public uint DefaultColumWidth
        {
            get
            {
                if (this.NativePropertyDescription != null && !this.defaultColumWidth.HasValue)
                {
                    uint tempDefaultColumWidth;

                    HResult hr = this.NativePropertyDescription.GetDefaultColumnWidth(out tempDefaultColumWidth);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.defaultColumWidth = tempDefaultColumWidth;
                    }
                }

                return this.defaultColumWidth.HasValue ? this.defaultColumWidth.Value : default(uint);
            }
        }

        /// <summary>
        /// Gets a value that describes how the property values are displayed when
        /// multiple items are selected in the user interface (UI).
        /// </summary>
        public PropertyAggregationType AggregationTypes
        {
            get
            {
                if (this.NativePropertyDescription != null && this.aggregationTypes == null)
                {
                    PropertyAggregationType tempAggregationTypes;

                    HResult hr = this.NativePropertyDescription.GetAggregationType(out tempAggregationTypes);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.aggregationTypes = tempAggregationTypes;
                    }
                }

                return this.aggregationTypes.HasValue ? this.aggregationTypes.Value : default(PropertyAggregationType);
            }
        }

        /// <summary>
        /// Gets a list of the possible values for this property.
        /// </summary>
        public ReadOnlyCollection<ShellPropertyEnumType> PropertyEnumTypes
        {
            get
            {
                if (this.NativePropertyDescription != null && this.propertyEnumTypes == null)
                {
                    List<ShellPropertyEnumType> propEnumTypeList = new List<ShellPropertyEnumType>();

                    Guid guid = new Guid(ShellIIDGuid.IPropertyEnumTypeList);
                    IPropertyEnumTypeList nativeList;
                    HResult hr = this.NativePropertyDescription.GetEnumTypeList(ref guid, out nativeList);

                    if (nativeList != null && CoreErrorHelper.Succeeded(hr))
                    {

                        uint count;
                        nativeList.GetCount(out count);
                        guid = new Guid(ShellIIDGuid.IPropertyEnumType);

                        for (uint i = 0; i < count; i++)
                        {
                            IPropertyEnumType nativeEnumType;
                            nativeList.GetAt(i, ref guid, out nativeEnumType);
                            propEnumTypeList.Add(new ShellPropertyEnumType(nativeEnumType));
                        }
                    }

                    this.propertyEnumTypes = new ReadOnlyCollection<ShellPropertyEnumType>(propEnumTypeList);
                }

                return this.propertyEnumTypes;
            }
        }

        /// <summary>
        /// Gets the column state flag, which describes how the property
        /// should be treated by interfaces or APIs that use this flag.
        /// </summary>
        public PropertyColumnStateOptions ColumnState
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (this.NativePropertyDescription != null && this.columnState == null)
                {
                    PropertyColumnStateOptions state;

                    HResult hr = this.NativePropertyDescription.GetColumnState(out state);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.columnState = state;
                    }
                }

                return this.columnState.HasValue ? this.columnState.Value : default(PropertyColumnStateOptions);
            }
        }

        /// <summary>
        /// Gets the condition type to use when displaying the property in
        /// the query builder user interface (UI). This influences the list
        /// of predicate conditions (for example, equals, less than, and
        /// contains) that are shown for this property.
        /// </summary>
        /// <remarks>For more information, see the <c>conditionType</c> attribute
        /// of the <c>typeInfo</c> element in the property's .propdesc file.</remarks>
        public PropertyConditionType ConditionType
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (this.NativePropertyDescription != null && this.conditionType == null)
                {
                    PropertyConditionType tempConditionType;
                    PropertyConditionOperation tempConditionOperation;

                    HResult hr = this.NativePropertyDescription.GetConditionType(out tempConditionType, out tempConditionOperation);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.conditionOperation = tempConditionOperation;
                        this.conditionType = tempConditionType;
                    }
                }

                return this.conditionType.HasValue ? this.conditionType.Value : default(PropertyConditionType);
            }
        }

        /// <summary>
        /// Gets the default condition operation to use
        /// when displaying the property in the query builder user
        /// interface (UI). This influences the list of predicate conditions
        /// (for example, equals, less than, and contains) that are shown
        /// for this property.
        /// </summary>
        /// <remarks>For more information, see the <c>conditionType</c> attribute of the
        /// <c>typeInfo</c> element in the property's .propdesc file.</remarks>
        public PropertyConditionOperation ConditionOperation
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (this.NativePropertyDescription != null && this.conditionOperation == null)
                {
                    PropertyConditionType tempConditionType;
                    PropertyConditionOperation tempConditionOperation;

                    HResult hr = this.NativePropertyDescription.GetConditionType(out tempConditionType, out tempConditionOperation);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.conditionOperation = tempConditionOperation;
                        this.conditionType = tempConditionType;
                    }
                }

                return this.conditionOperation.HasValue ? this.conditionOperation.Value : default(PropertyConditionOperation);
            }
        }

        /// <summary>
        /// Gets the method used when a view is grouped by this property.
        /// </summary>
        /// <remarks>The information retrieved by this method comes from
        /// the <c>groupingRange</c> attribute of the <c>typeInfo</c> element in the
        /// property's .propdesc file.</remarks>
        public PropertyGroupingRange GroupingRange
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (this.NativePropertyDescription != null && this.groupingRange == null)
                {
                    PropertyGroupingRange tempGroupingRange;

                    HResult hr = this.NativePropertyDescription.GetGroupingRange(out tempGroupingRange);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.groupingRange = tempGroupingRange;
                    }
                }

                return this.groupingRange.HasValue ? this.groupingRange.Value : default(PropertyGroupingRange);
            }
        }

        /// <summary>
        /// Gets the current sort description flags for the property,
        /// which indicate the particular wordings of sort offerings.
        /// </summary>
        /// <remarks>The settings retrieved by this method are set
        /// through the <c>sortDescription</c> attribute of the <c>labelInfo</c>
        /// element in the property's .propdesc file.</remarks>
        public PropertySortDescription SortDescription
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (this.NativePropertyDescription != null && this.sortDescription == null)
                {
                    PropertySortDescription tempSortDescription;

                    HResult hr = this.NativePropertyDescription.GetSortDescription(out tempSortDescription);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        this.sortDescription = tempSortDescription;
                    }
                }

                return this.sortDescription.HasValue ? this.sortDescription.Value : default(PropertySortDescription);
            }
        }

        /// <summary>
        /// Gets the localized display string that describes the current sort order.
        /// </summary>
        /// <param name="descending">Indicates the sort order should
        /// reference the string "Z on top"; otherwise, the sort order should reference the string "A on top".</param>
        /// <returns>The sort description for this property.</returns>
        /// <remarks>The string retrieved by this method is determined by flags set in the
        /// <c>sortDescription</c> attribute of the <c>labelInfo</c> element in the property's .propdesc file.</remarks>
        public string GetSortDescriptionLabel(bool descending)
        {
            IntPtr ptr = IntPtr.Zero;
            string label = string.Empty;

            if (this.NativePropertyDescription != null)
            {
                HResult hr = this.NativePropertyDescription.GetSortDescriptionLabel(descending, out ptr);

                if (CoreErrorHelper.Succeeded(hr) && ptr != IntPtr.Zero)
                {
                    label = Marshal.PtrToStringUni(ptr);

                    // Free the string
                    Marshal.FreeCoTaskMem(ptr);
                }
            }

            return label;
        }

        /// <summary>
        /// Gets a set of flags that describe the uses and capabilities of the property.
        /// </summary>
        public PropertyTypeOptions TypeFlags
        {
            get
            {
                if (this.NativePropertyDescription != null && this.propertyTypeFlags == null)
                {
                    PropertyTypeOptions tempFlags;

                    HResult hr = this.NativePropertyDescription.GetTypeFlags(PropertyTypeOptions.MaskAll, out tempFlags);

                    this.propertyTypeFlags = CoreErrorHelper.Succeeded(hr) ? tempFlags : default(PropertyTypeOptions);
                }

                return this.propertyTypeFlags.HasValue ? this.propertyTypeFlags.Value : default(PropertyTypeOptions);
            }
        }

        /// <summary>
        /// Gets the current set of flags governing the property's view.
        /// </summary>
        public PropertyViewOptions ViewFlags
        {
            get
            {
                if (this.NativePropertyDescription != null && this.propertyViewFlags == null)
                {
                    PropertyViewOptions tempFlags;
                    HResult hr = this.NativePropertyDescription.GetViewFlags(out tempFlags);

                    this.propertyViewFlags = CoreErrorHelper.Succeeded(hr) ? tempFlags : default(PropertyViewOptions);
                }

                return this.propertyViewFlags.HasValue ? this.propertyViewFlags.Value : default(PropertyViewOptions);
            }
        }

        /// <summary>
        /// Gets a value that determines if the native property description is present on the system.
        /// </summary>
        public bool HasSystemDescription => this.NativePropertyDescription != null;

        #endregion

        #region Internal Constructor

        internal ShellPropertyDescription(PropertyKey key)
        {
            this.propertyKey = key;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Get the native property description COM interface
        /// </summary>
        internal IPropertyDescription NativePropertyDescription
        {
            get
            {
                if (this.nativePropertyDescription == null)
                {
                    Guid guid = new Guid(ShellIIDGuid.IPropertyDescription);
                    PropertySystemNativeMethods.PSGetPropertyDescription(ref this.propertyKey, ref guid, out this.nativePropertyDescription);
                }

                return this.nativePropertyDescription;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Release the native objects
        /// </summary>
        /// <param name="disposing">Indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.nativePropertyDescription != null)
            {
                Marshal.ReleaseComObject(this.nativePropertyDescription);
                this.nativePropertyDescription = null;
            }

            if (disposing)
            {
                // and the managed ones
                this.canonicalName = null;
                this.displayName = null;
                this.editInvitation = null;
                this.defaultColumWidth = null;
                this.valueType = null;
                this.propertyEnumTypes = null;
            }
        }

        /// <summary>
        /// Release the native objects
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release the native objects
        /// </summary>
        ~ShellPropertyDescription()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
