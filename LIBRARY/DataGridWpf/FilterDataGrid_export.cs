namespace DataGridWpf
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using DataGridWpf.Export;
    using TMP.Shared;

    public partial class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        #region ClipboardExporters Property

        public Dictionary<string, ClipboardExporterBase> ClipboardExporters
        {
            get
            {
                if (this.clipboardExporters == null)
                {
                    this.clipboardExporters = new Dictionary<string, ClipboardExporterBase>();

                    if (!DesignerProperties.GetIsInDesignMode(this))
                    {
                        // Configure CSV ClipboardExporter
                        CsvClipboardExporter csvClipboardExporter = new CsvClipboardExporter();
                        csvClipboardExporter.FormatSettings.Separator = ',';
                        csvClipboardExporter.FormatSettings.TextQualifier = '"';
                        this.clipboardExporters.Add(DataFormats.CommaSeparatedValue, csvClipboardExporter);

                        // Configure tab separated value ClipboardExporter
                        csvClipboardExporter = new CsvClipboardExporter();
                        csvClipboardExporter.FormatSettings.Separator = '\t';
                        csvClipboardExporter.FormatSettings.TextQualifier = '"';
                        this.clipboardExporters.Add(DataFormats.Text, csvClipboardExporter);

                        // Configure HTML exporter
                        this.clipboardExporters.Add(DataFormats.Html, new HtmlClipboardExporter());

                        // Configure Unicode ClipboardExporter
                        this.clipboardExporters.Add(DataFormats.UnicodeText, new UnicodeCsvClipboardExporter());
                    }
                }

                return this.clipboardExporters;
            }
        }

        private Dictionary<string, ClipboardExporterBase> clipboardExporters; // = null;

        #endregion
    }
}
