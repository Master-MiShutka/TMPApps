using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Security;

namespace TMP.UI.Controls.WPF.Reporting.Helpers
{
    [SecurityCritical]
    [Serializable]
    internal class DataObject : IDataObject
    {
        public DataObject()
        {
            m_formatToValue = new Dictionary<string, object>();
        }

        #region PRIVATE FIELDS

        private Dictionary<string, object> m_formatToValue; // = null;

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

            m_formatToValue.TryGetValue(format, out value);

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
            return m_formatToValue.ContainsKey(format);
        }

        string[] IDataObject.GetFormats(bool autoConvert)
        {
            return ((IDataObject)this).GetFormats();
        }

        string[] IDataObject.GetFormats()
        {
            return m_formatToValue.Keys.ToArray<string>();
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
            if (m_formatToValue.ContainsKey(format))
            {
                m_formatToValue[format] = data;
            }
            else
            {
                m_formatToValue.Add(format, data);
            }
        }

        void IDataObject.SetData(object data)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
