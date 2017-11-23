using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid.Export
{
    public class HtmlClipboardExporter : ClipboardExporterBase
    {
        #region PUBLIC CONSTRUCTORS

        public HtmlClipboardExporter()
          : base()
        {
            this.IncludeColumnHeaders = true;
            m_sb = new StringBuilder();
        }

        #endregion

        #region PUBLIC PROPERTIES

        #endregion

        #region PROTECTED OVERRIDES

        protected override object ClipboardData
        {
            get
            {
                FormatHelper.GetClipboardContentForHtml(m_sb);
                return m_sb.ToString();
            }
        }

        protected override void ResetExporter()
        {
            m_sb = new StringBuilder();
        }

        protected override void StartHeaderField(DataGridContext dataGridContext, Column column, bool isFirst, bool isLast)
        {
            if (dataGridContext.DataGridControl.ClipboardCopyMode == System.Windows.Controls.DataGridClipboardCopyMode.IncludeHeader)
            {
                object columnHeader = ((this.UseFieldNamesInHeader) || (column.Title == null)) ? column.FieldName : column.Title;
                FormatHelper.HtmlFormatCell(columnHeader, isFirst, isLast, m_sb);
            }
        }

        protected override void StartDataItemField(DataGridContext dataGridContext, Column column, object fieldValue, bool isFirst, bool isLast)
        {
            FormatHelper.HtmlFormatCell(fieldValue, isFirst, isLast, m_sb);
        }

        #endregion

        #region PRIVATE METHODS
        #endregion

        #region PRIVATE FIELDS
        private StringBuilder m_sb;
        #endregion
    }
}
