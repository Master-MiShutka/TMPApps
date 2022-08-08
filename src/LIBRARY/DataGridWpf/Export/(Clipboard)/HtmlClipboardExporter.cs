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
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class HtmlClipboardExporter : ClipboardExporterBase
    {
        #region PUBLIC CONSTRUCTORS

        public HtmlClipboardExporter()
            : base()
        {
            this.IncludeColumnHeaders = true;
            this.FormatSettings = new HtmlFormatSettings();
            this.indentationString = string.Empty;

            // We keep a reference to the innerStream to return it when clipboard export is finished
            this.memoryStream = new MemoryStream();
            this.baseStream = new CF_HtmlStream(this.memoryStream);
        }

        #endregion

        #region PUBLIC PROPERTIES

        public HtmlFormatSettings FormatSettings
        {
            get;
            private set;
        }

        #endregion

        #region PROTECTED OVERRIDES

        protected override object ClipboardData =>
                // Return the innerStream of the CF_HtmlStream which contains the CF_HTML formatted data
                this.memoryStream;

        protected override void Indent()
        {
            string startDelimiter = this.FormatSettings.FieldStartDelimiter;
            string endDelimiter = this.FormatSettings.FieldEndDelimiter;

            if (startDelimiter == null)
            {
                startDelimiter = string.Empty;
            }

            if (endDelimiter == null)
            {
                endDelimiter = string.Empty;
            }

            // By default, we suppose indentation as an empty field
            this.indentationString += startDelimiter + endDelimiter;
        }

        protected override void Unindent()
        {
            string startDelimiter = this.FormatSettings.FieldStartDelimiter;
            string endDelimiter = this.FormatSettings.FieldEndDelimiter;

            if (startDelimiter == null)
            {
                startDelimiter = string.Empty;
            }

            if (endDelimiter == null)
            {
                endDelimiter = string.Empty;
            }

            if (this.indentationString == null)
            {
                Debug.Fail("Indentation must at least be string.empty when unindenting.");

                // We initalize the indentation string and return
                this.indentationString = string.Empty;
            }
            else
            {
                int emptyFieldLength = startDelimiter.Length + endDelimiter.Length;
                int indentationLength = this.indentationString.Length;

                // If there are less characters in indentationString than in the empty field, just set indentation
                // as string.empty
                if (indentationLength < emptyFieldLength)
                {
                    this.indentationString = string.Empty;
                }
                else
                {
                    this.indentationString = this.indentationString.Substring(0, indentationLength - emptyFieldLength);
                }
            }
        }

        protected override void ResetExporter()
        {
            this.tempBuffer = null;
            this.indentationString = string.Empty;

            // We must NOT close or dispose the previous MemoryStream since we pass this
            // instance to the Clipboard directly and it becomes responsible of
            // closing/disposing it
            this.memoryStream = new MemoryStream();
            this.baseStream = new CF_HtmlStream(this.memoryStream);
        }

        protected override void StartExporter(string dataFormat)
        {
            this.WriteToBaseStream(this.FormatSettings.ExporterStartDelimiter);
        }

        protected override void EndExporter(string dataFormat)
        {
            this.WriteToBaseStream(this.FormatSettings.ExporterEndDelimiter);

            // Force the header to be updated with length of HTML data and add the footer
            this.baseStream.Close();
        }

        protected override void StartHeader(FilterDataGrid filterDataGrid)
        {
            this.WriteToBaseStream(this.FormatSettings.HeaderDataStartDelimiter);

            if (string.IsNullOrEmpty(this.indentationString) == false)
            {
                this.WriteToBaseStream(this.indentationString);
            }
        }

        protected override void StartHeaderField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column)
        {
            this.WriteToBaseStream(this.FormatSettings.HeaderFieldStartDelimiter);

            object columnHeader = (this.UseFieldNamesInHeader || (column.Title == null)) ? column.FieldName : column.Title;

            string fieldValueString = FormatHelper.FormatHtmlFieldData(null, columnHeader, this.FormatSettings);

            this.WriteToBaseStream(fieldValueString);
            this.WriteToBaseStream(this.FormatSettings.HeaderFieldEndDelimiter);
        }

        protected override void EndHeader(FilterDataGrid filterDataGrid)
        {
            this.WriteToBaseStream(this.FormatSettings.HeaderDataEndDelimiter);
        }

        protected override void StartDataItem(FilterDataGrid filterDataGrid, object dataItem)
        {
            this.WriteToBaseStream(this.FormatSettings.DataStartDelimiter);

            if (string.IsNullOrEmpty(this.indentationString) == false)
            {
                this.WriteToBaseStream(this.indentationString);
            }
        }

        protected override void StartDataItemField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column, object rawFieldValue, object formattedFieldValue, string dataDisplayFormat, string dataExportFormat)
        {
            this.WriteToBaseStream(this.FormatSettings.FieldStartDelimiter);

            string fieldValueString;
            if (string.IsNullOrEmpty(dataExportFormat) == false)
            {
                fieldValueString = string.Format(this.FormatSettings.Culture, "{0:" + dataExportFormat + "}", rawFieldValue);
            }
            else
            {
                fieldValueString = FormatHelper.FormatHtmlFieldData(null, rawFieldValue, this.FormatSettings);
            }

            this.WriteToBaseStream(fieldValueString);
            this.WriteToBaseStream(this.FormatSettings.FieldEndDelimiter);
        }

        protected override void EndDataItem(FilterDataGrid filterDataGrid, object dataItem)
        {
            this.WriteToBaseStream(this.FormatSettings.DataEndDelimiter);
        }

        #endregion

        #region PRIVATE METHODS

        private void WriteToBaseStream(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            this.tempBuffer = Encoding.UTF8.GetBytes(value);

            this.baseStream.Write(this.tempBuffer, 0, this.tempBuffer.Length);
        }

        #endregion

        #region PRIVATE FIELDS

        private string indentationString; // = null;
        private MemoryStream memoryStream; // = null;
        private CF_HtmlStream baseStream; // = null;
        private byte[] tempBuffer; // = null;

        #endregion
    }
}
