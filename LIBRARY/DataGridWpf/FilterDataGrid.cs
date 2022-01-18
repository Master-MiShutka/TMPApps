#region (c) 2019 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : FilterDataGrid.cs
// Created    : 26/01/2021
#endregion (c) 2019 Gilles Macabies All right reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using SysDataGridColumn = System.Windows.Controls.DataGridColumn;
using TMP.Shared;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InvertIf
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable CheckNamespace

// https://stackoverflow.com/questions/3685566/wpf-using-resizegrip-to-resize-controls
// https://www.c-sharpcorner.com/UploadFile/mahesh/binding-static-properties-in-wpf-4-5/
// https://www.csharp-examples.net/string-format-datetime/
namespace DataGridWpf
{
    /// <summary>
    ///     Implementation of Datagrid
    /// </summary>
    public partial class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        #region Public Constructors

        /// <summary>
        ///  FilterDataGrid constructor
        /// </summary>
        public FilterDataGrid()
        {
            Debug.WriteLineIf(DebugMode, "Constructor");

            // load resources
            this.Resources = new FilterDataGridDictionary();

            // initial popup size
            this.popUpSize = new Point
            {
                X = (double)this.FindResource("PopupWidth"),
                Y = (double)this.FindResource("PopupHeight"),
            };

            // icons
            this.iconFilterSet = (Geometry)this.FindResource("FilterSet");
            this.iconFilter = (Geometry)this.FindResource("Filter");

            this.CommandBindings.Add(new CommandBinding(CommandShowFilter, this.ShowFilterCommand, this.CanShowFilter));
            this.CommandBindings.Add(new CommandBinding(CommandApplyFilter, this.ApplyFilterCommand, this.CanApplyFilter)); // Ok
            this.CommandBindings.Add(new CommandBinding(CommandCancelFilter, this.CancelFilterCommand));
            this.CommandBindings.Add(new CommandBinding(CommandRemoveFilter, this.RemoveFilterCommand, this.CanRemoveFilter));
            this.CommandBindings.Add(new CommandBinding(CommandIsChecked, this.CheckedAllCommand));
            this.CommandBindings.Add(new CommandBinding(CommandClearSearchBox, this.ClearSearchBoxClick));

            this.CommandBindings.Add(new CommandBinding(CommandSelectColumns, this.SelectColumnsClick));

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, this.OnCopyExecuted, this.OnCopyCanExecute));

            this.CommandBindings.Add(new CommandBinding(CommandExport, this.ExecutedExportCommand, this.CanExecuteExportCommand));

            this.CommandBindings.Add(new CommandBinding(CommandShowHideColumn, this.ExecutedShowHideColumnCommand, this.CanExecuteShowHideColumnCommand));

            this.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            this.SelectionUnit = DataGridSelectionUnit.FullRow;

            this.Translate = new Loc() { Language = (int)this.FilterLanguage };

            this.Loaded += this.FilterDataGrid_Loaded;

            this.ColumnDisplayIndexChanged += this.FilterDataGrid_ColumnDisplayIndexChanged;
        }

        #endregion Public Constructors

        #region Command

        public static readonly ICommand CommandApplyFilter = new RoutedCommand(nameof(CommandApplyFilter), typeof(FilterDataGrid));

        public static readonly ICommand CommandCancelFilter = new RoutedCommand(nameof(CommandCancelFilter), typeof(FilterDataGrid));

        public static readonly ICommand CommandClearSearchBox = new RoutedCommand(nameof(CommandClearSearchBox), typeof(FilterDataGrid));

        public static readonly ICommand CommandIsChecked = new RoutedCommand(nameof(CommandIsChecked), typeof(FilterDataGrid));

        public static readonly ICommand CommandRemoveFilter = new RoutedCommand(nameof(CommandRemoveFilter), typeof(FilterDataGrid));

        public static readonly ICommand CommandShowFilter = new RoutedCommand(nameof(CommandShowFilter), typeof(FilterDataGrid));

        public static readonly ICommand CommandSelectColumns = new RoutedCommand(nameof(CommandSelectColumns), typeof(FilterDataGrid));

        public static readonly ICommand CommandShowHideColumn = new RoutedCommand(nameof(CommandShowHideColumn), typeof(FilterDataGrid));

        public static readonly ICommand CommandExport = new RoutedCommand(nameof(CommandExport), typeof(FilterDataGrid));

        #endregion Command

        #region Public DependencyProperty

        /// <summary>
        /// Language displayed
        /// </summary>
        public static readonly DependencyProperty FilterLanguageProperty =
            DependencyProperty.Register("FilterLanguage",
                typeof(Local),
                typeof(FilterDataGrid),
                new PropertyMetadata(Local.Russian));

        /// <summary>
        ///     Show statusbar
        /// </summary>
        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register("ShowStatusBar",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ColumnsViewModelsProperty =
            DependencyProperty.Register("ColumnsViewModels",
                typeof(ObservableCollection<DataGridWpfColumnViewModel>),
                typeof(FilterDataGrid),
                new UIPropertyMetadata(null, ColumnsViewModelsPropertyChanged));

        /// <summary>
        /// Identifies the ColumnViewModel attached property
        /// </summary>
        public static readonly DependencyProperty ColumnViewModelProperty =
            DependencyProperty.RegisterAttached("ColumnViewModel", typeof(DataGridWpfColumnViewModel), typeof(FilterDataGrid));

        /// <summary>
        /// DisplayRowNumber
        /// </summary>
        public static readonly DependencyProperty DisplayRowNumberProperty =
            DependencyProperty.RegisterAttached("DisplayRowNumber",
                typeof(bool),
                typeof(FilterDataGrid),
                new UIPropertyMetadata(false, OnDisplayRowNumberChanged));

        private static void OnDisplayRowNumberChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FilterDataGrid dataGrid = target as FilterDataGrid;
            if ((bool)e.NewValue == true)
            {
                void loadedRowHandler(object sender, DataGridRowEventArgs ea)
                {
                    if (dataGrid.DisplayRowNumber == false)
                    {
                        dataGrid.LoadingRow -= loadedRowHandler;
                        return;
                    }

                    ea.Row.Header = ea.Row.GetIndex() + 1;
                }

                dataGrid.LoadingRow += loadedRowHandler;

                ItemsChangedEventHandler itemsChangedHandler = null;
                itemsChangedHandler = (object sender, ItemsChangedEventArgs ea) =>
                {
                    if (dataGrid.DisplayRowNumber == false)
                    {
                        dataGrid.ItemContainerGenerator.ItemsChanged -= itemsChangedHandler;
                        return;
                    }

                    GetVisualChildCollection<DataGridRow>(dataGrid).
                        ForEach(d => d.Header = d.GetIndex());
                };
                dataGrid.ItemContainerGenerator.ItemsChanged += itemsChangedHandler;
            }
        }

        #endregion Public DependencyProperty

        #region Public Properties

        /// <summary>
        ///     Language
        /// </summary>
        public Local FilterLanguage
        {
            get => (Local)this.GetValue(FilterLanguageProperty);
            set => this.SetValue(FilterLanguageProperty, value);
        }

        /// <summary>
        ///     Display items count
        /// </summary>
        public int ItemsSourceCount { get; set; }

        /// <summary>
        ///     Show status bar
        /// </summary>
        public bool ShowStatusBar
        {
            get => (bool)this.GetValue(ShowStatusBarProperty);
            set => this.SetValue(ShowStatusBarProperty, value);
        }

        /// <summary>
        ///     Instance of Loc
        /// </summary>
        public Loc Translate { get; private set; }

        /// <summary>
        /// DisplayRowNumber
        /// </summary>
        public bool DisplayRowNumber
        {
            get => (bool)this.GetValue(DisplayRowNumberProperty);
            set => this.SetValue(DisplayRowNumberProperty, value);
        }

        /// <summary>
        /// Gets the column ViewModel
        /// </summary>
        /// <param name="column">The data grid column</param>
        /// <returns>The view model</returns>
        [AttachedPropertyBrowsableForType(typeof(SysDataGridColumn))]
        public static DataGridWpfColumnViewModel GetColumnViewModel(SysDataGridColumn column)
        {
            return (DataGridWpfColumnViewModel)column.GetValue(ColumnViewModelProperty);
        }

        /// <summary>
        /// Sets the column ViewModel
        /// </summary>
        /// <param name="column">The data grid column</param>
        /// <param name="value">The view model</param>
        public static void SetColumnViewModel(SysDataGridColumn column, DataGridWpfColumnViewModel? value)
        {
            column.SetValue(ColumnViewModelProperty, value);
        }

        /// <summary>
        /// List of MenuItem to show/hide column
        /// </summary>
        public IList<MenuItem> ColumnsVisibilitySelectMenuItemList { get; private set; }

        #region ColumnsViewModels

        /// <summary>
        /// ColumnsViewModels
        /// </summary>
        public ObservableCollection<DataGridWpfColumnViewModel> ColumnsViewModels
        {
            get => (ObservableCollection<DataGridWpfColumnViewModel>)this.GetValue(ColumnsViewModelsProperty);
            set => this.SetValue(ColumnsViewModelsProperty, value);
        }

        private static void ColumnsViewModelsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            FilterDataGrid dataGrid = source as FilterDataGrid;
            if (dataGrid == null)
            {
                return;
            }

            if (e.NewValue is not ObservableCollection<DataGridWpfColumnViewModel> columns)
            {
                return;
            }

            if (e.OldValue != null)
            {
                dataGrid.Columns.Clear();
            }

            if (dataGrid.Columns.Count == 0)
            {
                foreach (var column in columns)
                {
                    SysDataGridColumn newColumn = Factory.ToDataGridWpfColumn(column) as SysDataGridColumn;
                    dataGrid.Columns.Add(newColumn);
                }
            }
            else
            {
                if (dataGrid.Columns.All(i => i is IDataGridWpfColumn) == false)
                {
                    var notIDataGridWpfColumnList = dataGrid.Columns.Where(i => !(i is IDataGridWpfColumn)).ToList();

                    foreach (var column in notIDataGridWpfColumnList)
                    {
                        var newColumn = Factory.ToDataGridWpfColumn(column);
                        dataGrid.Columns.Remove(column);
                        dataGrid.Columns.Add(newColumn as SysDataGridColumn);
                    }
                }
            }

            if (e.OldValue is ObservableCollection<DataGridWpfColumnViewModel> oldColumns)
            {
                foreach (DataGridWpfColumnViewModel item in oldColumns)
                {
                    item.PropertyChanged -= dataGrid.ColumnsViewModelsItem_PropertyChanged;
                }
            }

            foreach (DataGridWpfColumnViewModel item in columns)
            {
                item.PropertyChanged += dataGrid.ColumnsViewModelsItem_PropertyChanged;
            }

            columns.CollectionChanged += (sender, ne) =>
            {
                foreach (DataGridWpfColumnViewModel item in ne.OldItems)
                {
                    item.PropertyChanged -= dataGrid.ColumnsViewModelsItem_PropertyChanged;
                }

                foreach (DataGridWpfColumnViewModel item in ne.NewItems)
                {
                    item.PropertyChanged += dataGrid.ColumnsViewModelsItem_PropertyChanged;
                }

                if (ne.Action == NotifyCollectionChangedAction.Reset)
                {
                    dataGrid.Columns.Clear();
                    foreach (DataGridWpfColumnViewModel column in ne.NewItems)
                    {
                        dataGrid.Columns.Add(Factory.ToDataGridWpfColumn(column) as SysDataGridColumn);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (DataGridWpfColumnViewModel column in ne.NewItems)
                    {
                        dataGrid.Columns.Add(Factory.ToDataGridWpfColumn(column) as SysDataGridColumn);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Move)
                {
                    dataGrid.Columns.Move(ne.OldStartingIndex, ne.NewStartingIndex);
                }
                else if (ne.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (DataGridWpfColumnViewModel column in ne.OldItems)
                    {
                        dataGrid.Columns.Remove(Factory.ToDataGridWpfColumn(column) as SysDataGridColumn);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Replace)
                {
                    dataGrid.Columns[ne.NewStartingIndex] = Factory.ToDataGridWpfColumn(ne.NewItems[0] as DataGridWpfColumnViewModel) as SysDataGridColumn;
                }
            };
            if (dataGrid.collectionType != null)
            {
                dataGrid.GeneratingCustomsColumn();
            }
        }

        private void ColumnsViewModelsItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DataGridWpfColumnViewModel vm = sender as DataGridWpfColumnViewModel;
            if (vm == null)
            {
                return;
            }

            SysDataGridColumn column = null;
            UiDispatcher.Invoke(() =>
            {
                column = this.Columns.Where(c => string.Equals(c.Header, vm.Title)).FirstOrDefault();

                if (column == null)
                {
                    return;
                }

                if (e.PropertyName == nameof(DataGridWpfColumnViewModel.DisplayIndex))
                {
                    column.DisplayIndex = vm.DisplayIndex;
                }

                if (e.PropertyName == nameof(DataGridWpfColumnViewModel.IsVisible))
                {
                    column.Visibility = vm.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            });
        }

        #endregion

        #endregion Public Properties

        #region Public Event

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Sorted;

        #endregion Public Event

        #region Public methods

        #region Copy/Paste

        public bool PasteFromClipBoard(char textColumnSeparator = '\t')
        {
            return this.PasteCells(Clipboard.GetText().ParseTable(textColumnSeparator));
        }

        /// <summary>
        /// Determines whether the cell selection of the data grid is a rectangular range.
        /// </summary>
        /// <returns><c>true</c> if the cell selection is a rectangular range.</returns>
        public bool HasRectangularCellSelection()
        {
            var selectedCells = this.GetVisibleSelectedCells();

            if (selectedCells == null || !selectedCells.Any())
            {
                return false;
            }

            var visibleColumnIndexes = this.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c => c.DisplayIndex)
                .OrderBy(displayIndex => displayIndex)
                .ToList();

            var rowIndexes = selectedCells
                .Select(cell => cell.Item)
                .Distinct()
                .Select(item => this.Items.IndexOf(item))
                .ToArray();

            var columnIndexes = selectedCells
                .Select(c => c.Column.DisplayIndex)
                .Distinct()
                .Select(i => visibleColumnIndexes.IndexOf(i))
                .ToArray();

            var rows = rowIndexes.Max() - rowIndexes.Min() + 1;
            var columns = columnIndexes.Max() - columnIndexes.Min() + 1;

            return selectedCells.Count == rows * columns;
        }

        /// <summary>
        /// Gets the content of the selected cells as a table of strings.
        /// </summary>
        /// <returns>The selected cell content.</returns>
        public IList<IList<string>> GetCellSelection()
        {
            IList<IList<string>> result = new List<IList<string>>();

            var selectedCells = this.GetVisibleSelectedCells();

            if ((selectedCells == null) || !selectedCells.Any())
            {
                return null;
            }

            var orderedRows = selectedCells
                .GroupBy(i => i.Item)
                .OrderBy(i => this.Items.IndexOf(i.Key));

            result = orderedRows.Select(this.GetRowContent).ToList();

            if (this.ClipboardCopyMode == DataGridClipboardCopyMode.IncludeHeader)
            {
                var r = selectedCells.GroupBy(cell => cell.Column).OrderBy(i => i.Key.DisplayIndex).Select(c => c.Key.Header.ToString()).ToArray();
                result.Insert(0, r);
            }

            return result;
        }

        /// <summary>
        /// Replaces the selected cells with the data. Cell selection and the data table must have matching dimensions, either 1:n, n:1 or n:x*n.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <remarks>The cell selection is assumed to be a rectangular area.</remarks>
        /// <returns><c>true</c> if the dimensions of data and cell selection did match and the cells data has been replaced; otherwise <c>false</c>.</returns>
        public bool PasteCells(IList<IList<string>> data)
        {
            if (data == null)
            {
                return false;
            }

            var numberOfDataRows = data.Count;
            if (data.Count < 1)
            {
                return false;
            }

            var firstRow = data[0];

            var numberOfDataColumns = firstRow.Count;

            var selectedCells = this.GetVisibleSelectedCells();

            if ((selectedCells == null) || !selectedCells.Any())
            {
                return false;
            }

            var selectedColumns = selectedCells
                .Select(cellInfo => cellInfo.Column)
                .Distinct()
                .Where(column => column.Visibility == Visibility.Visible)
                .OrderBy(column => column.DisplayIndex)
                .ToArray();

            var selectedRows = selectedCells
                .Select(cellInfo => cellInfo.Item)
                .Distinct()
                .OrderBy(item => this.Items.IndexOf(item))
                .ToArray();

            if ((selectedColumns.Length == 1) && (selectedRows.Length == 1))
            {
                // n:1 => n:n, extend selection to match data
                var selectedColumn = selectedColumns[0];
                selectedColumns = this.Columns
                    .Where(col => col.DisplayIndex >= selectedColumn.DisplayIndex)
                    .OrderBy(col => col.DisplayIndex)
                    .Where(col => col.Visibility == Visibility.Visible)
                    .Take(numberOfDataColumns)
                    .ToArray();

                var selectedItem = selectedRows[0];
                selectedRows = this.Items
                    .Cast<object>()
                    .Skip(this.Items.IndexOf(selectedItem))
                    .Take(numberOfDataRows)
                    .ToArray();
            }

            var verticalFactor = selectedRows.Length / numberOfDataRows;
            if ((numberOfDataRows * verticalFactor) != selectedRows.Length)
            {
                return false;
            }

            var horizontalFactor = selectedColumns.Length / numberOfDataColumns;
            if ((numberOfDataColumns * horizontalFactor) != selectedColumns.Length)
            {
                return false;
            }

            // n:x*n
            foreach (var row in selectedRows.Zip(Repeat(data, verticalFactor), (row, cellValues) => new { row, cellValues }))
            {
                foreach (var column in selectedColumns.Zip(Repeat(row.cellValues, horizontalFactor), (dataGridColumn, cellValue) => new { dataGridColumn, cellValue }))
                {
                    column.dataGridColumn.OnPastingCellClipboardContent(row.row, column.cellValue);
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the selected cells that are in visible columns.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The selected cells of visible columns.</returns>
        public IList<DataGridCellInfo> GetVisibleSelectedCells()
        {
            var selectedCells = this.SelectedCells;

            return selectedCells?
                .Where(cellInfo => cellInfo.IsValid && (cellInfo.Column != null) && (cellInfo.Column.Visibility == Visibility.Visible))
                .ToArray();
        }

        #endregion Copy/Paste

        #endregion

        #region Private Fields

        private bool pending;
        private bool search;
        private Button button;
        private const bool DebugMode = false;
        private Cursor cursor;
        private double minHeight;
        private double minWidth;
        private double sizableContentHeight;
        private double sizableContentWidth;
        private Grid sizableContentGrid;

        private List<object> sourceObjectList;

        private ListBox listBox;
        private Path pathFilterIcon;
        private Point popUpSize;
        private Popup popup;
        private readonly Dictionary<string, Predicate<object>> criteria = new();
        private readonly Geometry iconFilter;
        private readonly Geometry iconFilterSet;
        private static readonly Dispatcher UiDispatcher = Dispatcher.CurrentDispatcher;
        private string fieldName;
        private string lastFilter;
        private string searchText;
        private TextBox searchTextBox;
        private Thumb thumb;
        private TreeView treeview;
        private Type collectionType;

        private object currentColumn;

        private Predicate<object> existingFilter;

        #endregion Private Fields

        #region Private Properties

        private ICollectionView CollectionViewSource { get; set; }

        private FilterCommon CurrentFilter { get; set; }

        private List<FilterCommon> GlobalFilterList { get; set; } = new List<FilterCommon>();

        private ICollectionView ItemCollectionView { get; set; }

        private IEnumerable<FilterItem> PopupViewItems => this.ItemCollectionView?.Cast<FilterItem>().Skip(1) ?? new List<FilterItem>();

        #endregion Private Properties

        #region Protected Methods

        /// <summary>
        ///     Initialize datagrid
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "OnInitialized");

            base.OnInitialized(e);

            try
            {
                // FilterLanguage : default : 0 (english)
                this.Translate = new Loc { Language = (int)this.FilterLanguage };

                // sorting event
                this.Sorted += OnSorted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnInitialized : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Auto generated column, set templateHeader
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "OnAutoGeneratingColumn");

            base.OnAutoGeneratingColumn(e);
            try
            {
                var column = Factory.CreateWpfColumn(e.Column, e);

                e.Column = column as SysDataGridColumn;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnAutoGeneratingColumn : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     The source of the Datagrid items has been changed (refresh or on loading)
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            Debug.WriteLineIf(DebugMode, "OnItemsSourceChanged");

            // order call :
            // Constructor
            // OnInitialized
            // OnItemsSourceChanged
            base.OnItemsSourceChanged(oldValue, newValue);

            try
            {
                if (newValue == null)
                {
                    return;
                }

                this.GlobalFilterList = new List<FilterCommon>();
                this.criteria.Clear(); // clear criteria

                this.CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(this.ItemsSource);

                // set Filter
                // thank's Stefan Heimel for this contribution
                if (this.CollectionViewSource.CanFilter)
                {
                    if (this.CollectionViewSource.Filter != null)
                    {
                        this.existingFilter = this.CollectionViewSource.Filter;
                    }

                    this.CollectionViewSource.Filter = this.Filter;
                }

                this.ItemsSourceCount = this.Items.Count;
                this.OnPropertyChanged(nameof(this.ItemsSourceCount));

                // get collection type
                if (this.ItemsSourceCount > 0)
                {
                    this.collectionType = this.ItemsSource is ICollectionView collectionView
                        ? (collectionView.SourceCollection?.GetType().GenericTypeArguments?.FirstOrDefault())
                        : (this.ItemsSource?.GetType().GenericTypeArguments?.FirstOrDefault());

                    if (this.collectionType == null)
                    {
                        Debug.WriteLine("ItemsSource must be implement ICollectionView");
                    }
                }

                // generating custom columns
                if (!this.AutoGenerateColumns && this.collectionType != null)
                {
                    this.GeneratingCustomsColumn();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnItemsSourceChanged : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Set the cursor to "Cursors.Wait" during a long sorting operation
        ///     https://stackoverflow.com/questions/8416961/how-can-i-be-notified-if-a-datagrid-column-is-sorted-and-not-sorting
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            if (this.pending || (this.popup?.IsOpen ?? false))
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            base.OnSorting(eventArgs);
            this.Sorted?.Invoke(this, new EventArgs());
        }

        #endregion Protected Methods

        #region Private Methods

        private void FilterDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ContextMenu == null)
            {
                this.ContextMenu = new ContextMenu();
            }

            this.ContextMenu.Loaded += this.ContextMenu_Loaded;
        }

        private void ContextMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem mi = new();
            mi.Header = this.Translate.Export;
            mi.Command = CommandExport;

            if (this.ContextMenu.ItemsSource != null)
            {
                CompositeCollection cc = new();

                CollectionContainer boundCollection = new();
                boundCollection.Collection = this.ContextMenu.ItemsSource;
                cc.Add(boundCollection);

                CollectionContainer exportCollection = new();
                List<Control> exportMenuItems = new(2);
                exportMenuItems.Add(new Separator());
                exportMenuItems.Add(mi);
                exportCollection.Collection = exportMenuItems;
                cc.Add(exportCollection);

                this.ContextMenu.ItemsSource = cc;
            }
            else
            {
                if (this.ContextMenu.HasItems)
                {
                    this.ContextMenu.Items.Add(new Separator());
                }

                this.ContextMenu.Items.Add(mi);
            }

            this.ContextMenu.Loaded -= this.ContextMenu_Loaded;
        }

        private void ExecutedExportCommand(object sender, ExecutedRoutedEventArgs e)
        {
            WaitCursor();
            /* Execute the private DoExportUsingRefection method on a background thread by starting a new task */
            Task task = Task.Factory.StartNew(() => { this.DoExportUsingRefection(); });
            task.ContinueWith(t => ResetCursor());
        }

        private void CanExecuteExportCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Items.Count > 0;
        }

        private void DoExportUsingRefection()
        {
            bool checkAccess = UiDispatcher.CheckAccess();
            ICollectionView collectionView = null;
            IList<DataGridColumn> columns = null;
            if (checkAccess)
            {
                columns = this.Columns.Where(c => c.Visibility == Visibility.Visible).OrderBy(c => c.DisplayIndex).ToList();
                collectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(this.ItemsSource);
            }
            else
            {
                UiDispatcher.Invoke(() => { columns = this.Columns.Where(c => c.Visibility == Visibility.Visible).OrderBy(c => c.DisplayIndex).ToList(); });
                UiDispatcher.Invoke(() => { collectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(this.ItemsSource); });
            }
        }

        /// <summary>
        /// Generate custom columns that can be filtered
        /// </summary>
        private void GeneratingCustomsColumn()
        {
            Debug.WriteLineIf(DebugMode, "GeneratingCustomColumn");

            try
            {
                // get the columns that can be filtered
                var columns = this.Columns
                    .Where(c => c is IDataGridWpfColumn column && column.IsColumnFiltered)
                    .Select(c => c)
                    .ToList();

                // set header template
                foreach (var col in columns)
                {
                    // reset template
                    col.HeaderTemplate = null;
                    col.HeaderTemplate = (DataTemplate)this.FindResource("DataGridHeaderTemplate");

                    var columnType = col.GetType();

                    if (col is System.Windows.Controls.DataGridBoundColumn column)
                    {
                        Type fieldType = null;
                        var fieldProperty = this.collectionType.GetProperty(((Binding)column.Binding).Path.Path);

                        // get type or underlying type if nullable
                        if (fieldProperty != null)
                        {
                            fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
                        }

                        // culture
                        if (((Binding)column.Binding).ConverterCulture == null)
                        {
                            ((Binding)column.Binding).ConverterCulture = this.Translate.Culture;
                        }

                        (col as IDataGridWpfColumn).FieldName = ((Binding)column.Binding).Path.Path;
                    }
                }

                this.ColumnsVisibilitySelectMenuItemList = this.GetColumnsShowHideMenuItems();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.GeneratingCustomColumn : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Reset the cursor at the end of the sort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSorted(object sender, EventArgs e)
        {
            ResetCursor();
        }

        /// <summary>
        /// Reactivate sorting
        /// </summary>
        private void ReactivateSorting()
        {
            if (this.currentColumn == null)
            {
                return;
            }

            if (this.currentColumn is WpfDataGridTextColumn column)
            {
                column.CanUserSort = true;
            }
            else
            {
                ;
            }

            if (this.currentColumn is WpfDataGridTemplateColumn templateColumn)
            {
                templateColumn.CanUserSort = true;
            }
        }

        /// <summary>
        ///     Reset cursor
        /// </summary>
        private static async void ResetCursor()
        {
            // reset cursor
            bool checkAccess = UiDispatcher.CheckAccess();
            if (checkAccess)
            {
                Mouse.OverrideCursor = null;
            }
            else
            {
                await UiDispatcher.BeginInvoke((Action)(() => { Mouse.OverrideCursor = null; }), DispatcherPriority.ContextIdle);
            }
        }

        /// <summary>
        ///     Wait cursor
        /// </summary>
        private static async void WaitCursor()
        {
            // wait cursor
            bool checkAccess = UiDispatcher.CheckAccess();
            if (checkAccess)
            {
                Mouse.OverrideCursor = Cursors.Wait;
            }
            else
            {
                await UiDispatcher.BeginInvoke((Action)(() => { Mouse.OverrideCursor = Cursors.Wait; }), DispatcherPriority.ContextIdle);
            }
        }

        /// <summary>
        ///     Can Apply filter (popup Ok button)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanApplyFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            // CanExecute only when the popup is open
            if ((this.popup?.IsOpen ?? false) == false)
            {
                e.CanExecute = false;
            }
            else
            {
                if (this.search)
                {
                    // in search, at least one article must be checked
                    e.CanExecute = this.CurrentFilter?.FieldType == typeof(DateTime) || this.CurrentFilter?.FieldType == typeof(DateOnly)
                    ? this.CurrentFilter.AnyDateIsChecked()
                    : this.PopupViewItems.Any(f => f?.IsChecked == true);
                }
                else
                {
                    // on change state, at least one item must be checked
                    // and another must have changed status
                    e.CanExecute = this.CurrentFilter?.FieldType == typeof(DateTime) || this.CurrentFilter?.FieldType == typeof(DateOnly)
                        ? this.CurrentFilter.AnyDateChanged()
                        : this.PopupViewItems.Any(f => f.Changed) && this.PopupViewItems.Any(f => f?.IsChecked == true);
                }
            }
        }

        /// <summary>
        ///     Cancel button, close popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.popup == null)
            {
                return;
            }

            this.popup.IsOpen = false; // raise EventArgs PopupClosed
        }

        /// <summary>
        ///     Can remove filter when current column (CurrentFilter) filtered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanRemoveFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CurrentFilter?.IsFiltered ?? false;
        }

        /// <summary>
        ///     Can show filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanShowFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.collectionType != null && this.CollectionViewSource?.CanFilter == true && (!this.popup?.IsOpen ?? true) && !this.pending;
        }

        /// <summary>
        ///     Check/uncheck all item when the action is (select all)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckedAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (FilterItem)e.Parameter;

            // only when the item[0] (select all) is checked or unchecked
            if (item?.Id != 0 || this.ItemCollectionView == null)
            {
                return;
            }

            foreach (var obj in this.PopupViewItems.ToList()
                .Where(f => f.IsChecked != item.IsChecked))
            {
                obj.IsChecked = item.IsChecked;
            }
        }

        /// <summary>
        ///     Clear Search Box text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void ClearSearchBoxClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.search = false;
            this.searchTextBox.Text = string.Empty; // raises TextChangedEventArgs
        }

        /// <summary>
        ///     Aggregate list of predicate as filter
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool Filter(object o)
        {
            bool b1 = this.existingFilter == null || this.existingFilter.Invoke(o);
            bool b2 = this.criteria.Values
                .Aggregate(true, (prevValue, predicate) => prevValue && predicate(o));

            return b1 && b2;
        }

        /// <summary>
        ///     OnPropertyChange
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     On Resize Thumb Drag Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            this.Cursor = this.cursor;
        }

        /// <summary>
        ///     Get delta on drag thumb
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            // initialize the first Actual size Width/Height
            if (this.sizableContentHeight <= 0)
            {
                this.sizableContentHeight = this.sizableContentGrid.ActualHeight;
                this.sizableContentWidth = this.sizableContentGrid.ActualWidth;
            }

            var yAdjust = this.sizableContentGrid.Height + e.VerticalChange;
            var xAdjust = this.sizableContentGrid.Width + e.HorizontalChange;

            // make sure not to resize to negative width or heigth
            xAdjust = this.sizableContentGrid.ActualWidth + xAdjust > this.minWidth ? xAdjust : this.minWidth;
            yAdjust = this.sizableContentGrid.ActualHeight + yAdjust > this.minHeight ? yAdjust : this.minHeight;

            xAdjust = xAdjust < this.minWidth ? this.minWidth : xAdjust;
            yAdjust = yAdjust < this.minHeight ? this.minHeight : yAdjust;

            // set size of grid
            this.sizableContentGrid.Width = xAdjust;
            this.sizableContentGrid.Height = yAdjust;
        }

        /// <summary>
        ///     On Resize Thumb DragStarted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            this.cursor = this.Cursor;
            this.Cursor = Cursors.SizeNWSE;
        }

        /// <summary>
        ///     Reset the size of popup to original size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupClosed(object sender, EventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "PopupClosed");

            var pop = (Popup)sender;

            // free the resources if the popup is closed without filtering
            if (!this.pending)
            {
                // clear resources
                this.sourceObjectList = null;
                this.ItemCollectionView = null;
                this.ReactivateSorting();
            }

            this.sizableContentGrid.Width = this.sizableContentWidth;
            this.sizableContentGrid.Height = this.sizableContentHeight;
            this.Cursor = this.cursor;

            // fix resize grip: unsubscribe event
            if (pop != null)
            {
                pop.Closed -= this.PopupClosed;
            }

            this.thumb.DragCompleted -= this.OnResizeThumbDragCompleted;
            this.thumb.DragDelta -= this.OnResizeThumbDragDelta;
            this.thumb.DragStarted -= this.OnResizeThumbDragStarted;
        }

        /// <summary>
        ///     Remove current filter
        /// </summary>
        private void RemoveCurrentFilter()
        {
            if (this.CurrentFilter == null)
            {
                return;
            }

            this.popup.IsOpen = false;
            this.button.Opacity = 0.5;
            this.pathFilterIcon.Data = this.iconFilter;

            Mouse.OverrideCursor = Cursors.Wait;

            if (this.CurrentFilter.IsFiltered && this.criteria.Remove(this.CurrentFilter.FieldName))
            {
                this.CollectionViewSource.Refresh();
            }

            if (this.GlobalFilterList.Contains(this.CurrentFilter))
            {
                this.GlobalFilterList.Remove(this.CurrentFilter);
            }

            // set the last filter applied
            this.lastFilter = this.GlobalFilterList.LastOrDefault()?.FieldName;

            ResetCursor();
        }

        /// <summary>
        ///     remove current filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.RemoveCurrentFilter();
        }

        /// <summary>
        ///     Filter current list in popup
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SearchFilter(object obj)
        {
            var item = (FilterItem)obj;
            return string.IsNullOrEmpty(this.searchText) || item == null || item.Id == 0
                ? true
                : item.Content?.ToString().IndexOf(this.searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        ///     Search TextBox Text Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            // fix TextChanged event fires twice
            // I did not find another solution
            if (textBox == null || textBox.Text == this.searchText || this.ItemCollectionView == null)
            {
                return;
            }

            this.searchText = textBox.Text;

            this.search = !string.IsNullOrEmpty(this.searchText);

            // apply filter
            this.ItemCollectionView.Refresh();

            if ((this.CurrentFilter.FieldType != typeof(DateTime) || this.CurrentFilter.FieldType != typeof(DateOnly)) || this.treeview == null)
            {
                return;
            }

            // rebuild treeview
            if (string.IsNullOrEmpty(this.searchText))
            {
                // fill the tree with the elements of the list of the original items
                this.treeview.ItemsSource = this.CurrentFilter.BuildTree(this.sourceObjectList, this.lastFilter);
            }
            else
            {
                // fill the tree only with the items found by the search
                var items = this.PopupViewItems.Where(i => i.IsChecked == true)
                    .Select(f => f.Content).ToList();

                // if at least one item is not null, fill in the tree structure
                // otherwise the tree structure contains only the item (select all).
                this.treeview.ItemsSource = this.CurrentFilter.BuildTree(items.Any() ? items : null);
            }
        }

        /// <summary>
        ///    Open a pop-up window, Click on the header button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "\r\nShowFilterCommand");

            // clear search text (important)
            this.searchText = string.Empty;
            this.search = false;

            try
            {
                this.button = (Button)e.OriginalSource;

                if (this.Items.Count == 0 || this.button == null || this.collectionType == null)
                {
                    return;
                }

                // navigate up to the current header and get column type
                var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(this.button);
                var columnType = header.Column.GetType();

                // then down to the current popup
                this.popup = VisualTreeHelpers.FindChild<Popup>(header, "FilterPopup");

                if (this.popup == null)
                {
                    return;
                }

                // popup handle event
                this.popup.Closed -= this.PopupClosed;
                this.popup.Closed += this.PopupClosed;

                // icon filter
                this.pathFilterIcon = VisualTreeHelpers.FindChild<Path>(this.button, "PathFilterIcon");

                // resizable grid
                this.sizableContentGrid = VisualTreeHelpers.FindChild<Grid>(this.popup.Child, "SizableContentGrid");

                // search textbox
                this.searchTextBox = VisualTreeHelpers.FindChild<TextBox>(this.popup.Child, "SearchBox");
                this.searchTextBox.Text = string.Empty;
                this.searchTextBox.TextChanged += this.SearchTextBoxOnTextChanged;

                // clear SearchBox button
                var clearSearchBoxBtn = VisualTreeHelpers.FindChild<Button>(this.popup.Child, "ClearSearchBoxBtn");
                clearSearchBoxBtn.Click += this.ClearSearchBoxClick;

                // thumb resize grip
                this.thumb = VisualTreeHelpers.FindChild<Thumb>(this.sizableContentGrid, "PopupThumb");

                // minimum size of Grid
                this.sizableContentHeight = 0;
                this.sizableContentWidth = 0;

                this.sizableContentGrid.Height = this.popUpSize.Y;
                this.sizableContentGrid.MinHeight = this.popUpSize.Y;

                this.minHeight = this.sizableContentGrid.MinHeight;
                this.minWidth = this.sizableContentGrid.MinWidth;

                // thumb handle event
                this.thumb.DragCompleted += this.OnResizeThumbDragCompleted;
                this.thumb.DragDelta += this.OnResizeThumbDragDelta;
                this.thumb.DragStarted += this.OnResizeThumbDragStarted;

                // get field name from binding Path
                if (columnType == typeof(WpfDataGridTextColumn))
                {
                    var column = (WpfDataGridTextColumn)header.Column;
                    this.fieldName = column.FieldName;
                    column.CanUserSort = false;
                    this.currentColumn = column;
                }

                if (columnType == typeof(WpfDataGridTemplateColumn))
                {
                    var column = (WpfDataGridTemplateColumn)header.Column;
                    this.fieldName = column.FieldName;
                    column.CanUserSort = false;
                    this.currentColumn = column;
                }

                if (columnType == typeof(WpfDataGridCheckBoxColumn))
                {
                    var column = (WpfDataGridCheckBoxColumn)header.Column;
                    this.fieldName = column.FieldName;
                    column.CanUserSort = false;
                    this.currentColumn = column;
                }

                // invalid fieldName
                if (string.IsNullOrEmpty(this.fieldName))
                {
                    return;
                }

                // get type of field
                Type fieldType = null;
                var fieldProperty = this.collectionType.GetProperty(this.fieldName);

                // get type or underlying type if nullable
                if (fieldProperty != null)
                {
                    fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
                }

                // If no filter, add filter to GlobalFilterList list
                this.CurrentFilter = this.GlobalFilterList.FirstOrDefault(f => f.FieldName == this.fieldName) ??
                                new FilterCommon
                                {
                                    FieldName = this.fieldName,
                                    FieldType = fieldType,
                                    Translate = this.Translate,
                                };

                // list of all item values, filtered and unfiltered (previous filtered items)
                this.sourceObjectList = new List<object>();

                // set cursor
                Mouse.OverrideCursor = Cursors.Wait;

                var filterItemList = new List<FilterItem>();

                // get the list of distinct values from the selected column
                // list of raw values of the current column
                await Task.Run(() =>
                {
                    // thank's Stefan Heimel for this contribution
                    UiDispatcher.Invoke(() =>
                    {
                        this.sourceObjectList = this.Items.Cast<object>()
                        .Select(x => x.GetType().GetProperty(this.fieldName)?.GetValue(x, null))
                        .Distinct() // clear duplicate values first
                        .Select(item => item)
                        .ToList();
                    });

                    // adds the previous filtered items to the list of new items (CurrentFilter.PreviouslyFilteredItems)
                    // displays new (checked) and already filtered (unchecked) items
                    // PreviouslyFilteredItems is a HashSet of objects
                    if (this.lastFilter == this.CurrentFilter.FieldName)
                    {
                        this.sourceObjectList.AddRange(this.CurrentFilter?.PreviouslyFilteredItems);
                    }

                    // sorting is a slow operation, using ParallelQuery
                    // TODO : AggregateException when user can add row
                    this.sourceObjectList = this.sourceObjectList.AsParallel().OrderBy(x => x).ToList();

                    // empty item flag
                    var emptyItem = false;

                    // if it exists, remove them from the list
                    if (this.sourceObjectList.Any(l => string.IsNullOrEmpty(l?.ToString())))
                    {
                        // item = null && items = "" => the empty string is not filtered.
                        // the solution is to add an empty string to the element to filter, see ApplyFilterCommand method
                        emptyItem = true;
                        this.sourceObjectList.RemoveAll(v => v == null || string.IsNullOrEmpty(v.ToString()));
                    }

                    // add the first element (select all) at the top of list
                    filterItemList = new List<FilterItem>(this.sourceObjectList.Count + 2)
                    {
                        new FilterItem { Label = this.Translate.All, IsChecked = true },
                    };

                    // add all items to the filterItemList
                    // filterItemList is used only for search and string list, the dates list is computed by FilterCommon.BuildTree
                    for (var i = 0; i < this.sourceObjectList.Count; i++)
                    {
                        var item = this.sourceObjectList[i];
                        var filterItem = new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = item, // raw value
                            Label = item?.ToString(), // Content displayed
                            Level = 1,

                            // check or uncheck if the content of current item exists in the list of previously filtered items
                            // SetState doesn't raise OnpropertyChanged notification
                            SetState = this.CurrentFilter.PreviouslyFilteredItems?.Contains(item) == false,
                        };
                        filterItemList.Add(filterItem);
                    }

                    // add a empty item(if exist) at the bottom of the list
                    if (emptyItem)
                    {
                        this.sourceObjectList.Insert(this.sourceObjectList.Count, null);

                        filterItemList.Add(new FilterItem
                        {
                            Id = filterItemList.Count,
                            FieldType = fieldType,
                            Content = null,
                            Label = this.Translate.Empty,
                            SetState = this.CurrentFilter.PreviouslyFilteredItems?.Contains(null) == false,
                        });
                    }
                }); // and task

                // the current listbox or treeview
                if (fieldType == typeof(DateTime) || fieldType == typeof(DateOnly))
                {
                    this.treeview = VisualTreeHelpers.FindChild<TreeView>(this.popup.Child, "PopupTreeview");

                    if (this.treeview != null)
                    {
                        // fill the treeview with CurrentFilter.BuildTree method
                        // and if it's the last filter, uncheck the items already filtered
                        this.treeview.ItemsSource =
                            this.CurrentFilter?.BuildTree(this.sourceObjectList, this.lastFilter);
                        this.treeview.Visibility = Visibility.Visible;
                    }

                    if (this.listBox != null)
                    {
                        // clear previous data
                        this.listBox.ItemsSource = null;
                        this.listBox.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    this.listBox = VisualTreeHelpers.FindChild<ListBox>(this.popup.Child, "PopupListBox");
                    if (this.listBox != null)
                    {
                        // set filterList as ItemsSource of ListBox
                        this.listBox.Visibility = Visibility.Visible;
                        this.listBox.ItemsSource = filterItemList;
                        this.listBox.UpdateLayout();

                        // scroll to top of view
                        var scrollViewer =
                            VisualTreeHelpers.GetDescendantByType(this.listBox, typeof(ScrollViewer)) as ScrollViewer;
                        scrollViewer?.ScrollToTop();
                    }

                    if (this.treeview != null)
                    {
                        // clear previous data
                        this.treeview.ItemsSource = null;
                        this.treeview.Visibility = Visibility.Collapsed;
                    }
                }

                // Set ICollectionView for filtering in the pop-up window
                this.ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(filterItemList);

                // set filter in popup
                if (this.ItemCollectionView.CanFilter)
                {
                    this.ItemCollectionView.Filter = this.SearchFilter;
                }

                // set the placement and offset of the PopUp in relation to the header and
                // the main window of the application (placement : bottom left or bottom right)
                this.PopupPlacement(this.sizableContentGrid, header);

                // open popup
                this.popup.IsOpen = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ShowFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                // reset cursor
                ResetCursor();
            }
        }

        /// <summary>
        ///     Click OK Button when Popup is Open, apply filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ApplyFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLineIf(DebugMode, "\r\nApplyFilterCommand");

            var start = DateTime.Now;
            this.pending = true;
            this.popup.IsOpen = false; // raise PopupClosed event

            // set cursor wait
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                // items already filtered
                var previousFiltered = new List<object>(this.CurrentFilter.PreviouslyFilteredItems);

                // list of content of items to filter
                var popupItems = this.PopupViewItems.ToList();

                await Task.Run(() =>
                {
                    // list of content of items not to be filtered
                    List<FilterItem> uncheckedItems;
                    List<FilterItem> checkedItems = null;

                    if (this.search)
                    {
                        // search items result displayed
                        var searchResult = popupItems;

                        this.Dispatcher.Invoke(() =>
                        {
                            // remove filter
                            this.ItemCollectionView.Filter = null;
                        });

                        // popup = all items except searchResult
                        uncheckedItems = this.PopupViewItems.Except(searchResult).ToList();
                        uncheckedItems.AddRange(searchResult.Where(c => c.IsChecked == false));

                        previousFiltered = previousFiltered.Except(searchResult
                            .Where(c => c.IsChecked == true)
                            .Select(c => c.Content)).ToList();

                        previousFiltered.AddRange(uncheckedItems.Select(c => c.Content));
                    }
                    else
                    {
                        var viewItems = this.CurrentFilter.FieldType == typeof(DateTime) || this.CurrentFilter.FieldType == typeof(DateOnly)
                            ? this.CurrentFilter.GetAllItemsTree().ToList()
                            : popupItems.Where(v => v.Changed).ToList();

                        checkedItems = viewItems.Where(i => i.IsChecked == true).ToList();
                        uncheckedItems = viewItems.Where(i => i.IsChecked == false).ToList();

                        // previous item except unchecked items checked again
                        previousFiltered = previousFiltered.Except(checkedItems.Select(c => c.Content)).ToList();
                        previousFiltered.AddRange(uncheckedItems.Select(c => c.Content));
                    }

                    // two values, null and string.empty for the list of strings
                    if (this.CurrentFilter.FieldType == typeof(string))
                    {
                        // add string.Empty
                        if (uncheckedItems.Any(v => v.Content == null))
                        {
                            previousFiltered.Add(string.Empty);
                        }

                        // remove string.Empty
                        if (checkedItems != null && checkedItems.Any(i => i.Content == null))
                        {
                            previousFiltered.RemoveAll(item => item?.ToString() == string.Empty);
                        }
                    }

                    // fill the PreviouslyFilteredItems HashSet with unchecked items
                    this.CurrentFilter.PreviouslyFilteredItems = new HashSet<object>(previousFiltered,
                        EqualityComparer<object>.Default);

                    // add a filter if it is not already added previously
                    if (!this.CurrentFilter.IsFiltered)
                    {
                        this.CurrentFilter.AddFilter(this.criteria);
                    }

                    // add current filter to GlobalFilterList
                    if (this.GlobalFilterList.All(f => f.FieldName != this.CurrentFilter.FieldName))
                    {
                        this.GlobalFilterList.Add(this.CurrentFilter);
                    }

                    // set the current field name as the last filter name
                    this.lastFilter = this.CurrentFilter.FieldName;
                });

                // set button opacity
                this.button.Opacity = 1;

                // set icon filter
                this.pathFilterIcon.Data = this.iconFilterSet;

                // apply filter
                this.CollectionViewSource.Refresh();

                // remove the current filter if there is no items to filter
                if (!this.CurrentFilter.PreviouslyFilteredItems.Any())
                {
                    this.RemoveCurrentFilter();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ApplyFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                this.ReactivateSorting();
                this.pending = false;
                ResetCursor();
            }
        }

        /// <summary>
        ///     PopUp placement and offset
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="header"></param>
        private void PopupPlacement(FrameworkElement grid, FrameworkElement header)
        {
            try
            {
                this.popup.PlacementTarget = header;
                this.popup.HorizontalOffset = 0d;
                this.popup.VerticalOffset = -1d;
                this.popup.Placement = PlacementMode.Bottom;

                // get the host window of the datagrid
                // thank's Stefan Heimel for this contribution
                var hostingWindow = Window.GetWindow(this);

                if (hostingWindow != null)
                {
                    const double border = 1d;

                    // get the ContentPresenter from the hostingWindow
                    var contentPresenter = VisualTreeHelpers.FindChild<ContentPresenter>(hostingWindow);

                    var hostSize = new Point
                    {
                        X = contentPresenter.ActualWidth,
                        Y = contentPresenter.ActualHeight,
                    };

                    // get the X, Y position of the header
                    var headerContentOrigin = header.TransformToVisual(contentPresenter).Transform(new Point(0, 0));
                    var headerDataGridOrigin = header.TransformToVisual(this).Transform(new Point(0, 0));

                    var headerSize = new Point { X = header.ActualWidth, Y = header.ActualHeight };
                    var offset = this.popUpSize.X - headerSize.X + border;

                    // the popup must stay in the DataGrid, move it to the left of the header,
                    // because it overflows on the right.
                    if (headerDataGridOrigin.X + headerSize.X > this.popUpSize.X)
                    {
                        this.popup.HorizontalOffset -= offset;
                    }

                    // delta for max size popup
                    var delta = new Point
                    {
                        X = hostSize.X - (headerContentOrigin.X + headerSize.X),
                        Y = Math.Max(hostSize.Y - (headerContentOrigin.Y + headerSize.Y + this.popUpSize.Y), 400),
                    };

                    // max size
                    grid.MaxWidth = this.popUpSize.X + delta.X - border;
                    grid.MaxHeight = this.popUpSize.Y + delta.Y - border;

                    // remove offset
                    if (this.popup.HorizontalOffset == 0)
                    {
                        grid.MaxWidth -= offset;
                    }

                    // the height of popup is too large, reduce it, because it overflows down.
                    if (delta.Y <= 0d)
                    {
                        grid.MaxHeight = this.popUpSize.Y - Math.Abs(delta.Y) - border;
                        grid.Height = grid.MaxHeight;
                        grid.MinHeight = grid.MaxHeight;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.PopupPlacement error : {ex.Message}");
                throw;
            }
        }

        private void OnCopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CopyCanExecute();
        }

        private bool CopyCanExecute()
        {
            try
            {
                new UIPermission(UIPermissionClipboard.AllClipboard).Demand();
                var view = (IEditableCollectionView)this.Items;

                if (view.IsEditingItem || view.IsAddingNew)
                {
                    return false;
                }

                if (this.SelectionUnit == DataGridSelectionUnit.Cell)
                {
                    return this.HasRectangularCellSelection(); // cell selection
                }
                else
                {
                    return this.GetCellSelection() != null;
                }
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        private void OnCopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!this.CopyCanExecute())
            {
                return;
            }

            try
            {
                IDataObject copyDataObject = Export.ClipboardExporterBase.CreateDataObject(this);

                if (copyDataObject != null)
                {
                    Clipboard.SetDataObject(copyDataObject, true);
                }
            }
            catch (SecurityException)
            {
            }
        }

        private IList<string> GetRowContent(IGrouping<object, DataGridCellInfo> row)
        {
            return row
                .OrderBy(i => i.Column.DisplayIndex)
                .Select(i => i.Column.OnCopyingCellClipboardContent(i.Item))
                .Select(i => i?.ToString() ?? string.Empty)
                .ToArray();
        }

        private IEnumerable<T> Repeat<T>(ICollection<T> source, int count)
            where T : class
        {
            for (var i = 0; i < count; i++)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        private static List<T> GetVisualChildCollection<T>(object parent)
            where T : Visual
        {
            List<T> visualCollection = new();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection)
            where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }

                if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }

        /// <summary>
        /// Show/Hide columns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void SelectColumnsClick(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var button = (Button)routedEventArgs.OriginalSource;

                if (this.Columns.Count == 0 || button == null)
                {
                    return;
                }

                button.ContextMenu = new ContextMenu();
                var menuItems = this.GetColumnsShowHideMenuItems();
                if (menuItems != null)
                {
                    foreach (var menuItem in menuItems)
                    {
                        button.ContextMenu.Items.Add(menuItem);
                    }
                }

                button.ContextMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.SelectColumnsClick error : {ex.Message}");
                throw;
            }
        }

        private void ShowHideColumnMenuItem_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            MenuItem menuItem = (MenuItem)routedEventArgs.OriginalSource;
            SysDataGridColumn column = (SysDataGridColumn)menuItem.Tag;

            if (column == null)
            {
                return;
            }

            this.ShowHideColumn(column);
        }

        protected IList<MenuItem> GetColumnsShowHideMenuItems()
        {
            IList<MenuItem> itemCollection = new List<MenuItem>();

            if (this.ColumnsViewModels != null)
            {
                var items = this.ColumnsViewModels
                    .GroupBy(i => string.IsNullOrEmpty(i.GroupName) ? "<нет названия группы>" : i.GroupName)
                    .ToList();
                foreach (var group in items)
                {
                    MenuItem menuItem = new()
                    {
                        StaysOpenOnClick = true,
                        Header = group.Key,
                    };

                    foreach (var child in group)
                    {
                        MenuItem childMenuItem = new()
                        {
                            IsCheckable = true,
                            StaysOpenOnClick = true,
                            IsChecked = child.IsVisible,
                            Header = child.Title,
                            Tag = this.Columns.Where(c => string.Equals(c.Header, child.Title)).FirstOrDefault(),
                        };
                        childMenuItem.Click += this.ShowHideColumnMenuItem_Click;
                        menuItem.Items.Add(childMenuItem);
                    }

                    itemCollection.Add(menuItem);
                }
            }
            else
            {
                foreach (DataGridColumn column in this.Columns)
                {
                    MenuItem menuItem = new()
                    {
                        IsCheckable = true,
                        StaysOpenOnClick = true,
                        IsChecked = column.Visibility == Visibility.Visible,
                        Header = column.Header,
                        Tag = column,
                    };
                    menuItem.Click += this.ShowHideColumnMenuItem_Click;
                    itemCollection.Add(menuItem);
                }
            }

            return itemCollection;
        }

        private void CanExecuteShowHideColumnCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecutedShowHideColumnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var param = e.Parameter as System.Windows.Controls.Primitives.DataGridColumnHeader;

            if (param == null)
            {
                return;
            }

            e.Handled = true;

            SysDataGridColumn column = (SysDataGridColumn)param.Column;

            if (column == null)
            {
                return;
            }

            this.ShowHideColumn(column);
        }

        private void ShowHideColumn(SysDataGridColumn column, bool? isVisible = null)
        {
            if (isVisible != null && isVisible.HasValue)
            {
                column.Visibility = isVisible.Value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                column.Visibility = column.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            }

            DataGridWpfColumnViewModel dataGridWpfColumn = (DataGridWpfColumnViewModel)column.GetValue(FilterDataGrid.ColumnViewModelProperty);
            if (dataGridWpfColumn != null)
            {
                dataGridWpfColumn.SetIsVisible(column.Visibility == Visibility.Visible);
            }

            System.Diagnostics.Debug.WriteLine(column.Visibility);
        }

        private void FilterDataGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            SysDataGridColumn column = e.Column;

            if (column == null)
            {
                return;
            }

            DataGridWpfColumnViewModel dataGridWpfColumn = (DataGridWpfColumnViewModel)column.GetValue(FilterDataGrid.ColumnViewModelProperty);
            if (dataGridWpfColumn != null)
            {
                dataGridWpfColumn.SetDisplayIndex(column.DisplayIndex);
            }
        }

        #endregion Private Methods
    }

    /// <summary>
    ///     ResourceDictionary
    /// </summary>
    public partial class FilterDataGridDictionary
    {
        #region Public Constructors

        /// <summary>
        /// FilterDataGrid Dictionary
        /// </summary>
        public FilterDataGridDictionary()
        {
            this.InitializeComponent();
        }

        #endregion Public Constructors
    }
}