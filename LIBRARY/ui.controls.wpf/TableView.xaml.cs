using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace TMP.UI.Controls.WPF
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : DataGrid
    {
        private DataGridColumnHeader[] _dataGridColumnHeaders = null;

        public TableView()
        {
            InitializeComponent();

            CanUserHideColumns = true;

            table.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            table.Unloaded += DataGrid_Unloaded;
            table.LoadingRow += DataGrid_LoadingRow;
            table.MouseDoubleClick += Table_MouseDoubleClick;
        }

        #region DataGrid

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyDescriptor is System.ComponentModel.PropertyDescriptor pd)
                e.Column.Header = pd.DisplayName;
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            table.LoadingRow -= DataGrid_LoadingRow;
        }

        private void Table_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            var cellinfo = grid.CurrentCell;
            if (cellinfo != null)
            {
                var column = cellinfo.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var element = new FrameworkElement() { DataContext = cellinfo.Item };
                    BindingOperations.SetBinding(element, FrameworkElement.TagProperty, column.Binding);
                    var cellValue = element.Tag;
                    Clipboard.SetText(cellValue.ToString());
                }
            }
        }

        #endregion

        #region BindableColumns
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.Register("BindableColumns",
                                                typeof(ObservableCollection<DataGridColumn>),
                                                typeof(TableView),
                                                new UIPropertyMetadata(null, BindableColumnsPropertyChanged));
        private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            TableView dataGrid = source as TableView;
            ObservableCollection<DataGridColumn> columns = e.NewValue as ObservableCollection<DataGridColumn>;
            dataGrid.Columns.Clear();
            if (columns == null)
            {
                return;
            }
            foreach (DataGridColumn column in columns)
            {
                dataGrid.Columns.Add(column);
            }
            columns.CollectionChanged += (sender, e2) =>
            {
                NotifyCollectionChangedEventArgs ne = e2 as NotifyCollectionChangedEventArgs;
                if (ne.Action == NotifyCollectionChangedAction.Reset)
                {
                    dataGrid.Columns.Clear();
                    foreach (DataGridColumn column in ne.NewItems)
                    {
                        dataGrid.Columns.Add(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (DataGridColumn column in ne.NewItems)
                    {
                        dataGrid.Columns.Add(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Move)
                {
                    dataGrid.Columns.Move(ne.OldStartingIndex, ne.NewStartingIndex);
                }
                else if (ne.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (DataGridColumn column in ne.OldItems)
                    {
                        dataGrid.Columns.Remove(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Replace)
                {
                    dataGrid.Columns[ne.NewStartingIndex] = ne.NewItems[0] as DataGridColumn;
                }
            };
        }
        public static void SetBindableColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
        {
            element.SetValue(BindableColumnsProperty, value);
        }
        public static ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject element)
        {
            return (ObservableCollection<DataGridColumn>)element.GetValue(BindableColumnsProperty);
        }
        #endregion

        #region HideColumnsHeader
        public static readonly DependencyProperty HideColumnsHeaderProperty = DependencyProperty.Register("HideColumnsHeader", typeof(object), typeof(TableView));

        public static object GetHideColumnsHeader(TableView obj)
        {
            return obj.GetValue(HideColumnsHeaderProperty);
        }

        public static void SetHideColumnsHeader(TableView obj, object value)
        {
            obj.SetValue(HideColumnsHeaderProperty, value);
        }
        #endregion HideColumnsHeader

        #region HideColumnsHeaderTemplate
        public static readonly DependencyProperty HideColumnsHeaderTemplateProperty =
            DependencyProperty.Register("HideColumnsHeaderTemplate",typeof(DataTemplate), typeof(TableView));

        public static DataTemplate GetHideColumnsHeaderTemplate(TableView obj)
        {
            return (DataTemplate)obj.GetValue(HideColumnsHeaderTemplateProperty);
        }

        public static void SetHideColumnsHeaderTemplate(TableView obj, DataTemplate value)
        {
            obj.SetValue(HideColumnsHeaderTemplateProperty, value);
        }
        #endregion HideColumnsHeaderTemplate

        #region HideColumnsIcon
        public static readonly DependencyProperty HideColumnsIconProperty =
            DependencyProperty.Register("HideColumnsIcon",typeof(object), typeof(TableView));

        public static object GetHideColumnsIcon(TableView obj)
        {
            return obj.GetValue(HideColumnsIconProperty);
        }

        public static void SetHideColumnsIcon(TableView obj, object value)
        {
            obj.SetValue(HideColumnsIconProperty, value);
        }
        #endregion HideColumnsIcon

        #region CanUserHideColumns
        public static readonly DependencyProperty CanUserHideColumnsProperty =
            DependencyProperty.Register("CanUserHideColumns",typeof(bool), typeof(TableView),
            new UIPropertyMetadata(false, OnCanUserHideColumnsChanged));

        public bool CanUserHideColumns
        {
            get { return (bool)GetValue(CanUserHideColumnsProperty); }
            set
            {
                SetValue(CanUserHideColumnsProperty, value);
            }
        }

        public static bool GetCanUserHideColumns(TableView obj)
        {
            return (bool)obj.GetValue(CanUserHideColumnsProperty);
        }

        public static void SetCanUserHideColumns(TableView obj, bool value)
        {
            obj.SetValue(CanUserHideColumnsProperty, value);
        }

        private static void OnCanUserHideColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TableView dataGrid = d as TableView;
            if (dataGrid == null)
                return;

            if ((bool)e.NewValue == false)
            {
                dataGrid.Loaded -= new RoutedEventHandler(dataGrid.DataGrid_Loaded);
                dataGrid.RemoveAllItems();
                return;
            }

            if (!dataGrid.IsLoaded)
            {
                dataGrid.Loaded -= new RoutedEventHandler(dataGrid.DataGrid_Loaded);
                dataGrid.Loaded += new RoutedEventHandler(dataGrid.DataGrid_Loaded);
            }
            else
                dataGrid.SetupColumnHeaders();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            TableView dataGrid = sender as TableView;
            if (dataGrid == null)
                return;

            if (BindingOperations.IsDataBound(dataGrid, DataGrid.ItemsSourceProperty))
            {
                Binding b = BindingOperations.GetBinding(dataGrid, DataGrid.ItemsSourceProperty);
                dataGrid.TargetUpdated += new EventHandler<DataTransferEventArgs>(DataGrid_TargetUpdated);

                string xaml = XamlWriter.Save(b);
                Binding b2 = XamlReader.Parse(xaml) as Binding;
                if (b2 != null)
                {
                    b2.NotifyOnTargetUpdated = true;
                    BindingOperations.ClearBinding(dataGrid, DataGrid.ItemsSourceProperty);
                    BindingOperations.SetBinding(dataGrid, DataGrid.ItemsSourceProperty, b2);
                }
            }
            else
                SetupColumnHeaders();
        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property != DataGrid.ItemsSourceProperty)
                return;

            TableView dataGrid = sender as TableView;
            if (dataGrid == null)
                return;

            EventHandler handler = null;
            handler = delegate
            {
                RemoveAllItems();
                if (SetupColumnHeaders())
                    dataGrid.LayoutUpdated -= handler;
            };

            dataGrid.LayoutUpdated += handler;
        }

        private DataGridColumnHeader[] GetColumnHeaders()
        {
            if (_dataGridColumnHeaders == null)
            {//dataGrid.UpdateLayout();
                DataGridColumnHeader[] columnHeaders = CustomVisualTreeHelper<DataGridColumnHeader>.FindChildrenRecursive(this);


                _dataGridColumnHeaders = (from DataGridColumnHeader columnHeader in columnHeaders
                        where columnHeader != null && columnHeader.Column != null
                        select columnHeader).ToArray();
            }
            return _dataGridColumnHeaders;
        }

        private string GetColumnName(DataGridColumn column)
        {
            if (column == null)
                return string.Empty;

            if (column.Header != null)
                return column.Header.ToString().Replace("\r", " ");
            else
                return string.Format("Колонка {0}", column.DisplayIndex);
        }

        private MenuItem GenerateItem(DataGridColumn column)
        {
            if (column == null)
                return null;

            MenuItem item = new MenuItem();
            item.Tag = column;

            item.Header = GetColumnName(column);
            if (string.IsNullOrEmpty(item.Header as string))
                return null;

            item.ToolTip = string.Format("Сменить видимость столбца '{0}'.", item.Header);

            item.IsCheckable = true;
            item.IsChecked = column.Visibility == Visibility.Visible;

            item.Checked += delegate
            {
                SetItemIsChecked(column, true);
            };

            item.Unchecked += delegate
            {
                SetItemIsChecked(column, false);
            };

            return item;
        }

        public MenuItem[] GetAttachedItems(DataGridColumnHeader columnHeader)
        {
            if (columnHeader == null || columnHeader.ContextMenu == null)
                return null;

            ItemsControl itemsContainer = (from object i in columnHeader.ContextMenu.Items
                                           where i is MenuItem && ((MenuItem)i).Tag != null && ((MenuItem)i).Tag.ToString() == "ItemsContainer"
                                           select i).FirstOrDefault() as MenuItem;

            if (itemsContainer == null)
                itemsContainer = columnHeader.ContextMenu;

            return (from object i in itemsContainer.Items
                    where i is MenuItem && ((MenuItem)i).Tag is DataGridColumn
                    select i).Cast<MenuItem>().ToArray();
        }

        private DataGridColumn GetColumnFromName(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return null;

            foreach (DataGridColumn column in Columns)
            {
                if (GetColumnName(column) == columnName)
                    return column;
            }

            return null;
        }

        private DataGridColumnHeader GetColumnHeaderFromColumn(DataGridColumn column)
        {
            if (column == null)
                return null;

            DataGridColumnHeader[] columnHeaders = GetColumnHeaders();
            return (from DataGridColumnHeader columnHeader in columnHeaders
                    where columnHeader.Column == column
                    select columnHeader).FirstOrDefault();
        }

        public void RemoveAllItems()
        {
            foreach (DataGridColumn column in Columns)
            {
                RemoveAllItems(column);
            }
        }

        public void RemoveAllItems(DataGridColumn column)
        {
            if (column == null)
                return;

            DataGridColumnHeader columnHeader = GetColumnHeaderFromColumn(column);
            List<MenuItem> itemsToRemove = new List<MenuItem>();

            if (columnHeader == null)
                return;

            // Mark items and/or items container for removal.
            if (columnHeader.ContextMenu != null)
            {
                foreach (object item in columnHeader.ContextMenu.Items)
                {
                    if (item is MenuItem && ((MenuItem)item).Tag != null
                        && (((MenuItem)item).Tag.ToString() == "ItemsContainer" || ((MenuItem)item).Tag is DataGridColumn))
                        itemsToRemove.Add((MenuItem)item);
                }
            }

            // Remove items and/or items container.
            foreach (MenuItem item in itemsToRemove)
            {
                columnHeader.ContextMenu.Items.Remove(item);
            }
        }

        public void ResetupColumnHeaders()
        {
            RemoveAllItems();
            SetupColumnHeaders();
        }

        private void SetItemIsChecked(DataGridColumn column, bool isChecked)
        {
            if (column == null)
                return;

            // Deny request if there are no other columns visible. Otherwise,
            // they'd have no way of changing the visibility of any columns
            // again.
            //if (!isChecked && (from DataGridColumn c in dataGrid.Columns
            //                   where c.Visibility == Visibility.Visible
            //                   select c).Count() < 2)
            //    return;

            if (isChecked && column.Visibility != Visibility.Visible)
            {
                ShowColumn(column);
            }
            else if (!isChecked)
                column.Visibility = Visibility.Hidden;

            DataGridColumnHeader[] columnHeaders = GetColumnHeaders();
            ItemsControl itemsContainer = null;
            object containerHeader = GetHideColumnsHeader(this);

            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                itemsContainer = null;
                if (columnHeader != null)
                {
                    if (columnHeader.ContextMenu == null)
                        continue;

                    itemsContainer = (from object i in columnHeader.ContextMenu.Items
                                      where i is MenuItem && ((MenuItem)i).Header == containerHeader
                                      select i).FirstOrDefault() as MenuItem;
                }

                if (itemsContainer == null)
                    itemsContainer = columnHeader.ContextMenu;

                foreach (object item in itemsContainer.Items)
                {
                    if (item is MenuItem && ((MenuItem)item).Tag != null && ((MenuItem)item).Tag is DataGridColumn
                        && ((MenuItem)item).Header.ToString() == GetColumnName(column))
                    {
                        ((MenuItem)item).IsChecked = isChecked;
                    }
                }
            }
        }

        private void SetupColumnHeader(DataGridColumnHeader columnHeader)
        {
            if (columnHeader == null)
                return;

            DataGrid dataGrid = CustomVisualTreeHelper<DataGrid>.FindAncestor(columnHeader);
            if (dataGrid == null)
                return;

            DataGridColumnHeader[] columnHeaders = GetColumnHeaders();
            if (columnHeaders == null)
                return;

            SetupColumnHeader(columnHeaders, columnHeader);
        }

        private void SetupColumnHeader(DataGridColumnHeader[] columnHeaders, DataGridColumnHeader columnHeader)
        {
            if (columnHeader.ContextMenu == null)
                columnHeader.ContextMenu = new ContextMenu();

            ItemsControl itemsContainer = null;
            itemsContainer = columnHeader.ContextMenu;

            object containerHeader = GetHideColumnsHeader(this);
            if (containerHeader != null)
            {
                MenuItem ic = (from object i in columnHeader.ContextMenu.Items
                               where i is MenuItem && ((MenuItem)i).Tag != null && ((MenuItem)i).Tag.ToString() == "ItemsContainer"
                               select i).FirstOrDefault() as MenuItem;

                if (ic == null)
                {
                    itemsContainer = new MenuItem()
                    {
                        Header = containerHeader,
                        HeaderTemplate = GetHideColumnsHeaderTemplate(this) as DataTemplate,
                        Icon = GetHideColumnsIcon(this),
                        Tag = "ItemsContainer"
                    };
                    columnHeader.ContextMenu.Items.Add(itemsContainer);
                }
                else
                    return;
            }

            foreach (DataGridColumnHeader columnHeader2 in columnHeaders)
            {
                if (columnHeader2 != columnHeader
                    && itemsContainer is ContextMenu
                    && columnHeader2.ContextMenu == itemsContainer)
                {
                    continue;
                }
                itemsContainer.Items.Add(GenerateItem(columnHeader2.Column));
            }
        }

        public bool SetupColumnHeaders()
        {
            DataGridColumnHeader[] columnHeaders = GetColumnHeaders();
            if (columnHeaders == null || columnHeaders.Count() == 0)
                return false;

            RemoveAllItems();
            columnHeaders = GetColumnHeaders();
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                SetupColumnHeader(columnHeaders, columnHeader);
            }

            return true;
        }

        /// <summary>
        /// Shows a column within the datagrid, which is not straightforward
        /// because the datagrid not only hides a column when you tell it to
        /// do so, but it also completely destroys its associated column
        /// header. Meaning we need to set it up again. Before we can do
        /// so we have to turn all columns back on again so we can get a
        /// complete list of their column headers, then turn them back off
        /// again.
        /// </summary>
        /// <param name="column"></param>
        private void ShowColumn(DataGridColumn column)
        {
            if (column == null)
                return;

            column.Visibility = Visibility.Visible;

            // Turn all columns on, but store their original visibility so we
            // can restore it after we're done.
            Dictionary<DataGridColumn, Visibility> vis = new Dictionary<DataGridColumn, Visibility>();
            foreach (DataGridColumn c in Columns)
            {
                vis.Add(c, c.Visibility);
                c.Visibility = Visibility.Visible;
            }
            UpdateLayout();

            DataGridColumnHeader columnHeader = GetColumnHeaderFromColumn(column);
            SetupColumnHeader(columnHeader);

            foreach (DataGridColumn c in vis.Keys)
            {
                if ((Visibility)vis[c] != Visibility.Visible)
                {
                    c.Visibility = (Visibility)vis[c];
                }
            }
            UpdateLayout();

            // Now we need to uncheck items that are associated with hidden
            // columns.
            SyncItemsOnColumnHeader(columnHeader);
        }

        private void SyncItemsOnColumnHeader(DataGridColumnHeader columnHeader)
        {
            bool isVisible;
            foreach (MenuItem item in GetAttachedItems(columnHeader))
            {
                if (item.Tag is DataGridColumn)
                {
                    isVisible = ((DataGridColumn)item.Tag).Visibility == Visibility.Visible ? true : false;
                    if (item.IsChecked != isVisible)
                    {
                        item.IsChecked = isVisible;
                    }
                }
            }
        }
        #endregion CanUserHideColumns

        #region CustomVisualTreeHelper
        private static class CustomVisualTreeHelper<TReturn> where TReturn : DependencyObject
        {
            public static TReturn FindAncestor(DependencyObject descendant)
            {
                DependencyObject parent = descendant;
                while (parent != null && !(parent is TReturn))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent != null)
                {
                    return (TReturn)parent;
                }
                return default(TReturn);
            }

            public static TReturn FindChild(DependencyObject parent)
            {
                int childCount = VisualTreeHelper.GetChildrenCount(parent);
                DependencyObject child = null;

                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    child = VisualTreeHelper.GetChild(parent, childIndex);
                    if (child is TReturn)
                    {
                        return (TReturn)(object)child;
                    }
                }
                return default(TReturn);
            }

            public static TReturn FindChildRecursive(DependencyObject parent)
            {
                int childCount = VisualTreeHelper.GetChildrenCount(parent);
                DependencyObject child = null;

                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    child = VisualTreeHelper.GetChild(parent, childIndex);
                    if (child is TReturn)
                    {
                        return (TReturn)(object)child;
                    }
                    else
                    {
                        child = CustomVisualTreeHelper<TReturn>.FindChildRecursive(child);
                        if (child is TReturn)
                        {
                            return (TReturn)(object)child;
                        }
                    }
                }
                return default(TReturn);
            }

            public static TReturn[] FindChildren(DependencyObject parent)
            {
                int childCount = VisualTreeHelper.GetChildrenCount(parent);
                DependencyObject child = null;
                List<TReturn> children = new List<TReturn>(childCount);

                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    child = VisualTreeHelper.GetChild(parent, childIndex);
                    if (child is TReturn)
                    {
                        children[childIndex] = (TReturn)(object)child;
                    }
                }
                return children.ToArray();
            }

            public static TReturn[] FindChildrenRecursive(DependencyObject parent)
            {
                DataGridCellsPanel panel = CustomVisualTreeHelper<DataGridCellsPanel>.FindChildRecursive(parent);
                if (panel != null)
                    parent = panel;

                int childCount = VisualTreeHelper.GetChildrenCount(parent);
                DependencyObject child = null;
                List<TReturn> children = new List<TReturn>();

                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    child = VisualTreeHelper.GetChild(parent, childIndex);
                    if (child is TReturn)
                    {
                        children.Add((TReturn)(object)child);
                    }

                    children.AddRange(CustomVisualTreeHelper<TReturn>.FindChildrenRecursive(child));
                }
                return children.ToArray();
            }
        }
        #endregion CustomVisualTreeHelper
    }
}
