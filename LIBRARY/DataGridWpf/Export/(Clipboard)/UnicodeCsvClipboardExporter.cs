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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    public class UnicodeCsvClipboardExporter : ClipboardExporterBase
    {
        public UnicodeCsvClipboardExporter()
        {
            this.IncludeColumnHeaders = true;
            this.FormatSettings = new CsvFormatSettings();
            this.indentationString = string.Empty;
            this.baseStream = new ToStringMemoryStream();
        }

        #region PUBLIC PROPERTIES

        public CsvFormatSettings FormatSettings
        {
            get;
            private set;
        }

        #endregion

        #region PROTECTED OVERRIDES

        protected override object ClipboardData => this.baseStream;

        protected override void Indent()
        {
            char separator = this.FormatSettings.Separator;
            this.indentationString += separator;
        }

        protected override void Unindent()
        {
            char separator = this.FormatSettings.Separator;

            if (this.indentationString == null)
            {
                Debug.Fail("Indentation must at least be string.empty when unindenting.");
                this.indentationString = string.Empty;
            }
            else
            {
                int separatorLength = 1;
                int indentationLength = this.indentationString.Length;

                // If there are less characters in indentationString than in the empty field, just set indentation
                // as string.empty
                if (indentationLength < separatorLength)
                {
                    this.indentationString = string.Empty;
                }
                else
                {
                    this.indentationString = this.indentationString.Substring(0, indentationLength - separatorLength);
                }
            }
        }

        protected override void ResetExporter()
        {
            this.baseStream = new ToStringMemoryStream();
            this.indentationString = string.Empty;
        }

        protected override void StartHeader(FilterDataGrid filterDataGrid)
        {
            if (string.IsNullOrEmpty(this.indentationString) == false)
            {
                this.WriteToBaseStream(this.indentationString);
            }

            // The next StartDataItemField will be considered as first column
            this._isFirstColumn = true;
        }

        protected override void StartHeaderField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column)
        {
            // We always insert the separator before the value except for the first item
            if (!this._isFirstColumn)
            {
                this.WriteToBaseStream(this.FormatSettings.Separator);
            }
            else
            {
                this._isFirstColumn = false;
            }

            object columnHeader = (this.UseFieldNamesInHeader || (column.Title == null)) ? column.FieldName : column.Title;
            string fieldValueString = UnicodeCsvClipboardExporter.FormatCsvData(null, columnHeader, this.FormatSettings);

            this.WriteToBaseStream(fieldValueString);
        }

        protected override void EndHeader(FilterDataGrid filterDataGrid)
        {
            this.WriteToBaseStream(this.FormatSettings.NewLine);
        }

        protected override void StartDataItem(FilterDataGrid filterDataGrid, object dataItem)
        {
            if (string.IsNullOrEmpty(this.indentationString) == false)
            {
                this.WriteToBaseStream(this.indentationString);
            }

            // The next StartDataItemField will be considered as first column
            this._isFirstColumn = true;
        }

        protected override void StartDataItemField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column, object rawFieldValue, object formattedFieldValue, string dataDisplayFormat, string dataExportFormat)
        {
            // We always insert the separator before the value except for the first item
            if (!this._isFirstColumn)
            {
                this.WriteToBaseStream(this.FormatSettings.Separator);
            }
            else
            {
                this._isFirstColumn = false;
            }

            string fieldValueString = UnicodeCsvClipboardExporter.FormatCsvData(null, formattedFieldValue, this.FormatSettings);

            this.WriteToBaseStream(fieldValueString);
        }

        protected override void EndDataItem(FilterDataGrid filterDataGrid, object dataItem)
        {
            this.WriteToBaseStream(this.FormatSettings.NewLine);
        }

        #endregion

        #region PRIVATE METHODS

        private void WriteToBaseStream(char value)
        {
            byte[] tempBuffer = Encoding.Unicode.GetBytes(new char[] { value });
            this.baseStream.Write(tempBuffer, 0, tempBuffer.Length);
        }

        private void WriteToBaseStream(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            byte[] tempBuffer = Encoding.Unicode.GetBytes(value);
            this.baseStream.Write(tempBuffer, 0, tempBuffer.Length);
        }

        #endregion

        #region Private Static Methods

        private static string FormatCsvData(Type dataType, object dataValue, CsvFormatSettings formatSettings)
        {
            string outputString = null;
            bool checkWhitespace = true;

            if ((dataValue != null) && (!Convert.IsDBNull(dataValue)) && (!(dataValue is Array)))
            {
                if (dataType == null)
                {
                    dataType = dataValue.GetType();
                }

                if (dataType == typeof(string))
                {
                    if (formatSettings.TextQualifier == '\0')
                    {
                        outputString = (string)dataValue;
                    }
                    else
                    {
                        outputString = formatSettings.TextQualifier + (string)dataValue + formatSettings.TextQualifier;
                    }

                    checkWhitespace = false;
                }
                else if (dataType == typeof(DateTime))
                {
                    if (!string.IsNullOrEmpty(formatSettings.DateTimeFormat))
                    {
                        if (formatSettings.Culture == null)
                        {
                            outputString = ((DateTime)dataValue).ToString(formatSettings.DateTimeFormat, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            outputString = ((DateTime)dataValue).ToString(formatSettings.DateTimeFormat, formatSettings.Culture);
                        }
                    }
                }
                else if ((dataType == typeof(double)) ||
                         (dataType == typeof(decimal)) ||
                         (dataType == typeof(float)) ||
                         (dataType == typeof(int)) ||
                         (dataType == typeof(double)) ||
                         (dataType == typeof(decimal)) ||
                         (dataType == typeof(float)) ||
                         (dataType == typeof(short)) ||
                         (dataType == typeof(float)) ||
                         (dataType == typeof(ushort)) ||
                         (dataType == typeof(uint)) ||
                         (dataType == typeof(ulong)) ||
                         (dataType == typeof(short)) ||
                         (dataType == typeof(long)))
                {
                    string format = formatSettings.NumericFormat;

                    if (((dataType == typeof(double)) ||
                          (dataType == typeof(decimal)) ||
                          (dataType == typeof(float))) &&
                        (!string.IsNullOrEmpty(formatSettings.FloatingPointFormat)))
                    {
                        format = formatSettings.FloatingPointFormat;
                    }

                    if (!string.IsNullOrEmpty(format))
                    {
                        if (formatSettings.Culture == null)
                        {
                            outputString = string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", dataValue);
                        }
                        else
                        {
                            outputString = string.Format(formatSettings.Culture, "{0:" + format + "}", dataValue);
                        }
                    }
                }

                if (outputString == null)
                {
                    if (formatSettings.Culture == null)
                    {
                        outputString = string.Format(CultureInfo.InvariantCulture, "{0}", dataValue);
                    }
                    else
                    {
                        outputString = string.Format(formatSettings.Culture, "{0}", dataValue);
                    }
                }

                // For dates and numbers, as a rule, we never use the TextQualifier. However, the
                // specified format can introduce whitespaces. To better support this case, we add
                // the TextQualifier if needed.
                if (checkWhitespace && (formatSettings.TextQualifier != '\0'))
                {
                    bool useTextQualifier = false;

                    // If the output string contains the character used to separate the fields, we
                    // don't bother checking for whitespace. TextQualifier will be used.
                    if (outputString.IndexOf(formatSettings.Separator) < 0)
                    {
                        for (int i = 0; i < outputString.Length; i++)
                        {
                            if (char.IsWhiteSpace(outputString, i))
                            {
                                useTextQualifier = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        useTextQualifier = true;
                    }

                    if (useTextQualifier)
                    {
                        outputString = formatSettings.TextQualifier + outputString + formatSettings.TextQualifier;
                    }
                }
            }

            return outputString;
        }

        #endregion

        #region PRIVATE FIELDS

        private string indentationString; // = null;
        private MemoryStream baseStream; // = null;
        private bool _isFirstColumn; // = false;

        #endregion

        #region ToStringMemoryStream Private Class

        // This class is used to force the ToString of the
        // MemoryStream to return the content of the Stream
        // instead of the name of the Type.
        private class ToStringMemoryStream : MemoryStream
        {
            public override string ToString()
            {
                if (this.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    try
                    {
                        return Encoding.Unicode.GetString(this.ToArray());
                    }
                    catch (Exception)
                    {
                        return base.ToString();
                    }
                }
            }
        }

        #endregion
    }
}

