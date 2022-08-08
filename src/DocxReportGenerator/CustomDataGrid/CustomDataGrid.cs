namespace TMP.Work.DocxReportGenerator.CustomControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class CustomDataGrid : DataGrid
    {
        #region Constructors

        static CustomDataGrid()
        {
            CommandManager.RegisterClassCommandBinding(typeof(CustomDataGrid),
                new CommandBinding(ApplicationCommands.Paste,
                new ExecutedRoutedEventHandler(OnExecutedPasteInternal),
                new CanExecuteRoutedEventHandler(OnCanExecutePasteInternal)));
        }

        public CustomDataGrid()
        {
            this.Loaded += this.CustomDataGridLoaded;
        }

        private void CustomDataGridLoaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region Clipboard Paste

        public event ExecutedRoutedEventHandler ExecutePasteEvent;

        public event CanExecuteRoutedEventHandler CanExecutePasteEvent;

        private static void OnCanExecutePasteInternal(object target, CanExecuteRoutedEventArgs args)
        {
            ((CustomDataGrid)target).OnCanExecutePaste(target, args);
        }

        /// <summary>
        /// This virtual method is called when ApplicationCommands.Paste command query its state.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCanExecutePaste(object target, CanExecuteRoutedEventArgs args)
        {
            if (this.CanExecutePasteEvent != null)
            {
                this.CanExecutePasteEvent(target, args);
                if (args.Handled)
                {
                    return;
                }
            }

            args.CanExecute = this.CurrentCell != null;
            args.Handled = true;
        }

        private static void OnExecutedPasteInternal(object target, ExecutedRoutedEventArgs args)
        {
            ((CustomDataGrid)target).OnExecutedPaste(target, args);
        }

        /// <summary>
        /// This virtual method is called when ApplicationCommands.Paste command is executed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExecutedPaste(object target, ExecutedRoutedEventArgs args)
        {
            if (this.ExecutePasteEvent != null)
            {
                this.ExecutePasteEvent(target, args);
                if (args.Handled)
                {
                    return;
                }
            }

            // parse the clipboard data            [row][column]
            List<string[]> clipboardData = ClipboardHelper.ParseClipboardWithCommaSeparatedValueOrText();

            bool hasAddedNewRow = false;

            Debug.Print(">>> DataGrid Paste: >>>");
#if DEBUG
            StringBuilder sb = new StringBuilder();
#endif

            // call OnPastingCellClipboardContent for each cell
            int minRowIndex = Math.Max(this.Items.IndexOf(this.CurrentItem), 0);
            int maxRowIndex = this.Items.Count - 1;
            int minColumnDisplayIndex = (this.SelectionUnit != DataGridSelectionUnit.FullRow) ? this.Columns.IndexOf(this.CurrentColumn) : 0;
            int maxColumnDisplayIndex = this.Columns.Count - 1;

            int clipboardRowIndex = 0;
            for (int i = minRowIndex; i <= maxRowIndex && clipboardRowIndex < clipboardData.Count; i++, clipboardRowIndex++)
            {
                if (this.CanUserAddRows && i == maxRowIndex)
                {
                    // add a new row to be pasted to
                    ICollectionView cv = CollectionViewSource.GetDefaultView(this.Items);
                    IEditableCollectionView iecv = cv as IEditableCollectionView;
                    if (iecv != null)
                    {
                        hasAddedNewRow = true;
                        iecv.AddNew();
                        if (clipboardRowIndex + 1 < clipboardData.Count)
                        {
                            // still has more items to paste, update the maxRowIndex
                            maxRowIndex = this.Items.Count - 1;
                        }
                    }
                }
                else if (i == maxRowIndex)
                {
                    continue;
                }

                int columnDataIndex = 0;
                for (int j = minColumnDisplayIndex; j < maxColumnDisplayIndex && columnDataIndex < clipboardData[clipboardRowIndex].Length; j++, columnDataIndex++)
                {
                    DataGridColumn column = this.ColumnFromDisplayIndex(j);
                    object item = this.Items[i];
                    if (column is DataGridBoundColumn dgbc && dgbc.Binding != null)
                    {
                        string propertyName = (dgbc.Binding as Binding).Path.Path;
                        object value = clipboardData[clipboardRowIndex][columnDataIndex];
                        PropertyInfo pi = item.GetType().GetProperty(propertyName);
                        if (pi != null)
                        {
                            object convertedValue = Convert.ChangeType(value, pi.PropertyType);
                            item.GetType().GetProperty(propertyName).SetValue(item, convertedValue, null);
                        }
                    }
                    else
                    {
                        column.OnPastingCellClipboardContent(item, clipboardData[clipboardRowIndex][columnDataIndex]);
                    }
                }
            }

            // update selection
            if (hasAddedNewRow)
            {
                this.UnselectAll();
                this.UnselectAllCells();
                this.CurrentItem = this.Items[minRowIndex];
                if (this.SelectionUnit == DataGridSelectionUnit.FullRow)
                {
                    this.SelectedItem = this.Items[minRowIndex];
                }
                else
                    if (this.SelectionUnit == DataGridSelectionUnit.CellOrRowHeader || this.SelectionUnit == DataGridSelectionUnit.Cell)
                {
                    this.SelectedCells.Add(new DataGridCellInfo(this.Items[minRowIndex], this.Columns[minColumnDisplayIndex]));
                }
            }
        }

        /// <summary>
        ///     Whether the end-user can add new rows to the ItemsSource.
        /// </summary>
        public bool CanUserPasteToNewRows
        {
            get => (bool)this.GetValue(CanUserPasteToNewRowsProperty);
            set => this.SetValue(CanUserPasteToNewRowsProperty, value);
        }

        /// <summary>
        ///     DependencyProperty for CanUserAddRows.
        /// </summary>
        public static readonly DependencyProperty CanUserPasteToNewRowsProperty =
            DependencyProperty.Register("CanUserPasteToNewRows",
                                        typeof(bool), typeof(CustomDataGrid),
                                        new FrameworkPropertyMetadata(true, null, null));

        #endregion Clipboard Paste

    }
}