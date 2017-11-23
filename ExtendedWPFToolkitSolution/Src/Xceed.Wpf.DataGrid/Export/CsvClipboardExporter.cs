/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Export
{
    public class CsvClipboardExporter : ClipboardExporterBase
    {


        #region PUBLIC CONSTRUCTORS

        public CsvClipboardExporter()
          : base()
        {
            this.IncludeColumnHeaders = true;
            this.FormatSettings = new CsvFormatSettings();
            m_indentationString = string.Empty;
            m_sb = new StringBuilder();
        }

        #endregion

        #region PUBLIC PROPERTIES

        public IFormatSettings FormatSettings
        {
            get;
            private set;
        }

        #endregion

        #region PROTECTED OVERRIDES

        protected override object ClipboardData
        {
            get
            {
                return m_sb.ToString();
            }
        }

        protected override void Indent()
        {
            char separator = this.FormatSettings.Separator;
            m_indentationString += separator;
        }

        protected override void Unindent()
        {
            char separator = this.FormatSettings.Separator;

            if (m_indentationString == null)
            {
                Debug.Fail("Indentation must at least be string.empty when unindenting.");
                m_indentationString = string.Empty;
            }
            else
            {
                int separatorLength = 1;
                int indentationLength = m_indentationString.Length;

                // If there are less characters in indentationString than in the empty field, just set indentation
                // as string.empty
                if (indentationLength < separatorLength)
                {
                    m_indentationString = string.Empty;
                }
                else
                {
                    m_indentationString = m_indentationString.Substring(0, indentationLength - separatorLength);
                }
            }
        }

        protected override void ResetExporter()
        {
            m_sb = new StringBuilder();
            m_indentationString = string.Empty;
        }

        protected override void StartHeader(DataGridContext dataGridContext)
        {
            if (string.IsNullOrEmpty(m_indentationString) == false & dataGridContext.DataGridControl.ClipboardCopyMode == System.Windows.Controls.DataGridClipboardCopyMode.IncludeHeader)
                m_sb.Append(m_indentationString);
        }

        protected override void StartHeaderField(DataGridContext dataGridContext, Column column, bool isFirst, bool isLast)
        {
            // We always insert the separator before the value except for the first item
            if (dataGridContext.DataGridControl.ClipboardCopyMode == System.Windows.Controls.DataGridClipboardCopyMode.IncludeHeader)
            {
                object columnHeader = ((this.UseFieldNamesInHeader) || (column.Title == null)) ? column.FieldName : column.Title;

                FormatHelper.CsvFormatCell(columnHeader, isFirst, isLast, m_sb, this.FormatSettings);
            }
        }

        protected override void StartDataItem(DataGridContext dataGridContext, object dataItem)
        {
            if (string.IsNullOrEmpty(m_indentationString) == false)
                m_sb.Append(m_indentationString);
        }

        protected override void StartDataItemField(DataGridContext dataGridContext, Column column, object fieldValue, bool isFirst, bool isLast)
        {
            FormatHelper.CsvFormatCell(fieldValue, isFirst, isLast, m_sb, this.FormatSettings);
        }

        #endregion

        #region PRIVATE FIELDS

        private string m_indentationString; // = null;
        private StringBuilder m_sb;

        #endregion
    }
}
