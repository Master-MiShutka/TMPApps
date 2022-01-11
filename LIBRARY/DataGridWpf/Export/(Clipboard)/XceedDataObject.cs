/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

namespace DataGridWpf.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Windows;

    [SecurityCritical]
    [Serializable]
    internal class XceedDataObject : IDataObject
    {
        public XceedDataObject()
        {
            this.formatToValue = new Dictionary<string, object>();
        }

        #region PRIVATE FIELDS

        private Dictionary<string, object> formatToValue; // = null;

        #endregion

        #region IDataObject Members

        object IDataObject.GetData(string format, bool autoConvert)
        {
            return ((IDataObject)this).GetData(format);
        }

        object IDataObject.GetData(Type format)
        {
            throw new NotSupportedException();
        }

        object IDataObject.GetData(string format)
        {
            object value = null;

            this.formatToValue.TryGetValue(format, out value);

            return value;
        }

        bool IDataObject.GetDataPresent(string format, bool autoConvert)
        {
            return ((IDataObject)this).GetDataPresent(format);
        }

        bool IDataObject.GetDataPresent(Type format)
        {
            throw new NotSupportedException();
        }

        bool IDataObject.GetDataPresent(string format)
        {
            return this.formatToValue.ContainsKey(format);
        }

        string[] IDataObject.GetFormats(bool autoConvert)
        {
            return ((IDataObject)this).GetFormats();
        }

        string[] IDataObject.GetFormats()
        {
            return this.formatToValue.Keys.ToArray<string>();
        }

        void IDataObject.SetData(string format, object data, bool autoConvert)
        {
            ((IDataObject)this).SetData(format, data);
        }

        void IDataObject.SetData(Type format, object data)
        {
            throw new NotSupportedException();
        }

        void IDataObject.SetData(string format, object data)
        {
            if (this.formatToValue.ContainsKey(format))
            {
                this.formatToValue[format] = data;
            }
            else
            {
                this.formatToValue.Add(format, data);
            }
        }

        void IDataObject.SetData(object data)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
