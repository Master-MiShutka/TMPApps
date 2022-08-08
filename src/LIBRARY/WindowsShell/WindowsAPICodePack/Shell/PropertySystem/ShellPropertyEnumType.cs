// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
    using System;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Defines the enumeration values for a property type.
    /// </summary>
    public class ShellPropertyEnumType
    {
        #region Private Properties

        private string displayText;
        private PropEnumType? enumType;
        private object minValue;
        private object setValue;
        private object enumerationValue;

        private IPropertyEnumType NativePropertyEnumType
        {
            set;
            get;
        }

        #endregion

        #region Internal Constructor

        internal ShellPropertyEnumType(IPropertyEnumType nativePropertyEnumType)
        {
            this.NativePropertyEnumType = nativePropertyEnumType;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets display text from an enumeration information structure.
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (this.displayText == null)
                {
                    this.NativePropertyEnumType.GetDisplayText(out this.displayText);
                }

                return this.displayText;
            }
        }

        /// <summary>
        /// Gets an enumeration type from an enumeration information structure.
        /// </summary>
        public PropEnumType EnumType
        {
            get
            {
                if (!this.enumType.HasValue)
                {
                    PropEnumType tempEnumType;
                    this.NativePropertyEnumType.GetEnumType(out tempEnumType);
                    this.enumType = tempEnumType;
                }

                return this.enumType.Value;
            }
        }

        /// <summary>
        /// Gets a minimum value from an enumeration information structure.
        /// </summary>
        public object RangeMinValue
        {
            get
            {
                if (this.minValue == null)
                {
                    using (PropVariant propVar = new PropVariant())
                    {
                        this.NativePropertyEnumType.GetRangeMinValue(propVar);
                        this.minValue = propVar.Value;
                    }
                }

                return this.minValue;
            }
        }

        /// <summary>
        /// Gets a set value from an enumeration information structure.
        /// </summary>
        public object RangeSetValue
        {
            get
            {
                if (this.setValue == null)
                {
                    using (PropVariant propVar = new PropVariant())
                    {
                        this.NativePropertyEnumType.GetRangeSetValue(propVar);
                        this.setValue = propVar.Value;
                    }
                }

                return this.setValue;
            }
        }

        /// <summary>
        /// Gets a value from an enumeration information structure.
        /// </summary>
        public object RangeValue
        {
            get
            {
                if (this.enumerationValue == null)
                {
                    using (PropVariant propVar = new PropVariant())
                    {
                        this.NativePropertyEnumType.GetValue(propVar);
                        this.enumerationValue = propVar.Value;
                    }
                }

                return this.enumerationValue;
            }
        }

        #endregion
    }
}
