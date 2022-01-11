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
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;

    public abstract class ClipboardExporterBase
    {
        protected ClipboardExporterBase()
        {
            this.UseFieldNamesInHeader = false;
        }

        #region IncludeColumnHeaders Property

        public bool IncludeColumnHeaders
        {
            get;
            set;
        }

        #endregion

        #region UseFieldNamesInHeader Property

        public bool UseFieldNamesInHeader
        {
            get;
            set;
        }

        #endregion

        #region ClipboardData Property

        protected abstract object ClipboardData
        {
            get;
        }

        #endregion

        protected virtual void Indent()
        {
        }

        protected virtual void Unindent()
        {
        }

        protected virtual void StartExporter(string dataFormat)
        {
        }

        protected virtual void EndExporter(string dataFormat)
        {
        }

        protected virtual void ResetExporter()
        {
        }

        protected virtual void StartHeader(FilterDataGrid filterDataGrid)
        {
        }

        protected virtual void StartHeaderField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column)
        {
        }

        protected virtual void EndHeaderField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column)
        {
        }

        protected virtual void EndHeader(FilterDataGrid filterDataGrid)
        {
        }

        protected virtual void StartDataItem(FilterDataGrid filterDataGrid, object dataItem)
        {
        }

        protected virtual void StartDataItemField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column, object rawFieldValue, object formattedFieldValue, string dataDisplayFormat, string dataExportFormat)
        {
        }

        protected virtual void EndDataItemField(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel column, object rawFieldValue, object formattedFieldValue, string dataDisplayFormat, string dataExportFormat)
        {
        }

        protected virtual void EndDataItem(FilterDataGrid filterDataGrid, object dataItem)
        {
        }

        internal static IDataObject CreateDataObject(FilterDataGrid dataGridControl)
        {
            if (dataGridControl == null)
            {
                throw new ArgumentNullException(nameof(dataGridControl));
            }

            XceedDataObject dataObject = null;
            bool containsData = false;

            Cursor oldCursor = dataGridControl.ReadLocalValue(FrameworkElement.CursorProperty) as Cursor;
            try
            {
                dataGridControl.Cursor = Cursors.Wait;

                Dictionary<string, ClipboardExporterBase> exporters = dataGridControl.ClipboardExporters;

                foreach (KeyValuePair<string, ClipboardExporterBase> keyPair in exporters)
                {
                    if (keyPair.Value == null)
                    {
                        throw new NullReferenceException("ClipboardExporterBase cannot be null.");
                    }

                    keyPair.Value.StartExporter(keyPair.Key);
                }

                using (ManualExporter exporter = new ManualExporter(exporters.Values as IEnumerable<ClipboardExporterBase>))
                {
                    exporter.Export(dataGridControl);
                }

                foreach (KeyValuePair<string, ClipboardExporterBase> keyPair in exporters)
                {
                    keyPair.Value.EndExporter(keyPair.Key);

                    if (dataObject == null)
                    {
                        dataObject = new XceedDataObject();
                    }

                    object clipboardExporterValue = keyPair.Value.ClipboardData;

                    // For other formats, we directly copy the content to the IDataObject
                    if (clipboardExporterValue != null)
                    {
                        ((IDataObject)dataObject).SetData(keyPair.Key, clipboardExporterValue);
                        containsData = true;
                    }

                    keyPair.Value.ResetExporter();
                }
            }
            finally
            {
                if (oldCursor != null)
                {
                    dataGridControl.Cursor = oldCursor;
                }
                else
                {
                    dataGridControl.ClearValue(FrameworkElement.CursorProperty);
                }
            }

            // Only return dataObject some data was copied
            if (containsData)
            {
                return dataObject as IDataObject;
            }

            return null;
        }

        private class ManualExporter : IDisposable
        {
            public ManualExporter(IEnumerable<ClipboardExporterBase> clipboardExporters)
            {
                this.clipboardExporters = new List<ClipboardExporterBase>(clipboardExporters);
            }

            internal void Export(FilterDataGrid filterDataGrid)
            {
                if (filterDataGrid == null)
                {
                    return;
                }

                // Get informations for this filterDataGrid
                IList<System.Windows.Controls.DataGridCellInfo> selectedCells = filterDataGrid.SelectedCells;
                Debug.Assert(selectedCells != null);

                System.Windows.Controls.DataGridCellInfo[] cells = selectedCells
                    .Where(cellInfo => cellInfo.IsValid && (cellInfo.Column != null) && (cellInfo.Column.Visibility == Visibility.Visible)).ToArray();

                // Get datagrid row objects
                object[] rowItems = cells
                    .Select(cell => cell.Item)
                    .Distinct()
                    .ToArray();

                // objects count
                int objectsCount = rowItems.Length;

                // Get DataGridCells by each row
                var dataGridCells = cells // System.Windows.Controls.DataGridCellInfo[]
                    .GroupBy(cell => cell.Item)
                    .ToDictionary(grp => grp.Key, grp => grp.ToArray());

                // Get DataGridColumn array
                System.Windows.Controls.DataGridColumn[] dataGridColumns = cells
                    .GroupBy(cell => cell.Column)
                    .Where(c => c.Key.Visibility == Visibility.Visible)
                    .OrderBy(c => c.Key.DisplayIndex)
                    .Select(c => c.Key)
                    .ToArray();

                // Get DataGridWpfColumn array
                DataGridWpfColumnViewModel[] dataGridWpfColumns = filterDataGrid.ColumnsViewModels
                    .Where(c => c.IsVisible)
                    .OrderBy(i => i.DisplayIndex)
                    .ToArray();

                // For the first, ensure to export the headers before anything else
                this.ExportHeaders(filterDataGrid, dataGridWpfColumns);

                for (int i = 0; i < objectsCount; i++)
                {
                    object exportedItem = rowItems[i];

                    var itemCells = dataGridCells[exportedItem];

                    this.ExportDataItem(filterDataGrid, exportedItem, itemCells, dataGridWpfColumns, dataGridColumns);
                }
            }

            private void ExportDataItemCore(FilterDataGrid filterDataGrid,
                                            ClipboardExporterBase clipboardExporter,
                                            object item,
                                            System.Windows.Controls.DataGridCellInfo[] cells,
                                            DataGridWpfColumnViewModel[] wpfColumns,
                                            System.Windows.Controls.DataGridColumn[] dataGridColumns)
            {
                clipboardExporter.StartDataItem(filterDataGrid, item);

                // Ensure the count does not exceeds the columns count
                int columnsCount = wpfColumns.Length;

                Type itemType = item.GetType();

                for (int i = 0; i < columnsCount; i++)
                {
                    DataGridWpfColumnViewModel wpfColumn = wpfColumns[i];
                    System.Windows.Controls.DataGridColumn dataGidColumn = dataGridColumns[i];
                    System.Windows.Controls.DataGridCellInfo dataGridCellInfo = cells[i];

                    (object rawValue, object formattedValue) field = (default, default);

                    switch (dataGidColumn)
                    {
                        case System.Windows.Controls.DataGridBoundColumn dbc:
                            field = GetBoundColumnFieldValue(dbc);
                            if (dbc.ClipboardContentBinding != null)
                            {
                                field.formattedValue = dbc.OnCopyingCellClipboardContent(item);
                            }

                            break;
                        case System.Windows.Controls.DataGridComboBoxColumn dcc:
                            field = GetComboBoxColumnFieldValue(dcc);
                            break;
                        default:
                            throw new NotImplementedException(dataGidColumn.GetType().FullName);
                    }

                    if (string.IsNullOrEmpty(wpfColumn.CellContentExportStringFormat) == false)
                    {
                        field.formattedValue = string.Format(wpfColumn.CellContentExportStringFormat, field.rawValue);
                    }

                    clipboardExporter.StartDataItemField(filterDataGrid, wpfColumn, field.rawValue, field.formattedValue, wpfColumn.CellContentStringFormat, wpfColumn.CellContentExportStringFormat);
                    clipboardExporter.EndDataItemField(filterDataGrid, wpfColumn, field.rawValue, field.formattedValue, wpfColumn.CellContentStringFormat, wpfColumn.CellContentExportStringFormat);
                }

                clipboardExporter.EndDataItem(filterDataGrid, item);

                (object rawValue, object formattedValue) GetBoundColumnFieldValue(System.Windows.Controls.DataGridBoundColumn column)
                {
                    (object rawValue, object formattedValue) result = default;

                    // Get the property name from the column's binding
                    BindingBase bb = column.Binding;
                    if (bb != null)
                    {
                        Binding binding = bb as Binding;
                        if (binding != null)
                        {
                            string boundProperty = binding.Path.Path;

                            // Get the property value using reflection
                            PropertyInfo pi = itemType.GetProperty(boundProperty);
                            if (pi != null)
                            {
                                result.rawValue = pi.GetValue(item);
                                if (string.IsNullOrEmpty(binding.StringFormat) == false)
                                {
                                    result.formattedValue = string.Format(binding.StringFormat, result.rawValue);
                                }
                                else
                                {
                                    result.formattedValue = result.rawValue;
                                }

                                // for a DataGridCheckBoxColumn that is bound to a nullable Boolean (null?) property with a value of NULL
                                if (column is WpfDataGridCheckBoxColumn)
                                {
                                    result.rawValue = result.formattedValue = "-";
                                }
                            }
                        }
                    }

                    return result;
                }

                (object rawValue, object formattedValue) GetComboBoxColumnFieldValue(System.Windows.Controls.DataGridComboBoxColumn column)
                {
                    (object rawValue, object formattedValue) result = default;

                    // Get the property name from the column's binding
                    BindingBase bb = column.SelectedValueBinding;
                    if (bb != null)
                    {
                        Binding binding = bb as Binding;
                        if (binding != null)
                        {
                            string boundProperty = binding.Path.Path;

                            /// Get the selected property
                            PropertyInfo pi = itemType.GetProperty(boundProperty);
                            if (pi != null)
                            {
                                object boundProperyValue = pi.GetValue(item);
                                if (boundProperyValue != null)
                                {
                                    Type propertyType = boundProperyValue.GetType();
                                    if (propertyType.IsPrimitive || propertyType.Equals(typeof(string)))
                                    {
                                        if (column.ItemsSource != null)
                                        {
                                            // Find the object in the ItemsSource of the ComboBox with an SelectedValuePath equal to the selected
                                            IEnumerable<object> comboBoxSource = column.ItemsSource.Cast<object>();
                                            object obj = (from oo in comboBoxSource
                                                          let prop = oo.GetType().GetProperty(column.SelectedValuePath)
                                                          where prop != null && prop.GetValue(oo).Equals(boundProperyValue)
                                                          select oo).FirstOrDefault();
                                            if (obj != null)
                                            {
                                                // Get the (DisplayMemberPath) of the object
                                                if (string.IsNullOrEmpty(column.DisplayMemberPath))
                                                {
                                                    result.rawValue = result.formattedValue = obj.GetType();
                                                }
                                                else
                                                {
                                                    PropertyInfo prop = obj.GetType().GetProperty(column.DisplayMemberPath);
                                                    if (prop != null)
                                                    {
                                                        result.rawValue = result.formattedValue = prop.GetValue(obj);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Export the scalar property value of the selected object specified by the SelectedValuePath property of the DataGridComboBoxColumn
                                            result.rawValue = result.formattedValue = boundProperyValue;
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(column.DisplayMemberPath) == false)
                                    {
                                        // Get the Name (DisplayMemberPath) property of the selected object
                                        PropertyInfo pi2 = boundProperyValue.GetType().GetProperty(column.DisplayMemberPath);

                                        if (pi2 != null)
                                        {
                                            object display = pi2.GetValue(boundProperyValue);
                                            if (display != null)
                                            {
                                                result.rawValue = result.formattedValue = display;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result.rawValue = result.formattedValue = itemType.ToString();
                                    }
                                }
                            }
                        }
                    }

                    return result;
                }
            }

            private void ExportDataItem(FilterDataGrid filterDataGrid, object item, System.Windows.Controls.DataGridCellInfo[] cells, DataGridWpfColumnViewModel[] wpfColumns, System.Windows.Controls.DataGridColumn[] dataGridColumns)
            {
                foreach (ClipboardExporterBase clipboardExporter in this.clipboardExporters)
                {
                    this.ExportDataItemCore(filterDataGrid, clipboardExporter, item, cells, wpfColumns, dataGridColumns);
                }
            }

            private void ExportHeaders(FilterDataGrid filterDataGrid, DataGridWpfColumnViewModel[] columnsByVisiblePosition)
            {
                foreach (ClipboardExporterBase clipboardExporter in this.clipboardExporters)
                {
                    // We always add the headers for detail levels every time
                    if (clipboardExporter.IncludeColumnHeaders)
                    {
                        clipboardExporter.StartHeader(filterDataGrid);

                        // Ensure the count does not exceeds the columns count
                        int columnsCount = columnsByVisiblePosition.Length;

                        for (int index = 0; index < columnsCount; index++)
                        {
                            DataGridWpfColumnViewModel column = columnsByVisiblePosition[index];

                            if (column == null)
                            {
                                continue;
                            }

                            clipboardExporter.StartHeaderField(filterDataGrid, column);
                            clipboardExporter.EndHeaderField(filterDataGrid, column);
                        }

                        clipboardExporter.EndHeader(filterDataGrid);
                    }
                }
            }

            #region IDisposable Members

            public void Dispose()
            {
                this.clipboardExporters.Clear();
            }

            #endregion IDisposable Members

            private List<ClipboardExporterBase> clipboardExporters; // = null;
        }
    }
}
