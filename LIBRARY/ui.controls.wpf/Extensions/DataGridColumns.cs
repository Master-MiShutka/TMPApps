using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.UI.Controls.WPF.Extensions
{
    public static class DataGridColumns
    {
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static object GetColumnsSource(DependencyObject obj)
        {
            return (object)obj.GetValue(ColumnsSourceProperty);
        }

        public static void SetColumnsSource(DependencyObject obj, object value)
        {
            obj.SetValue(ColumnsSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for ColumnsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.RegisterAttached(
                "ColumnsSource",
                typeof(object),
                typeof(DataGridColumns),
                new UIPropertyMetadata(
                    null,
                    ColumnsSourceChanged));


        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static string GetHeaderTextMember(DependencyObject obj)
        {
            return (string)obj.GetValue(HeaderTextMemberProperty);
        }

        public static void SetHeaderTextMember(DependencyObject obj, string value)
        {
            obj.SetValue(HeaderTextMemberProperty, value);
        }

        // Using a DependencyProperty as the backing store for HeaderTextMember.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTextMemberProperty =
            DependencyProperty.RegisterAttached("HeaderTextMember", typeof(string), typeof(DataGridColumns), new UIPropertyMetadata(null));


        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static string GetDisplayMemberMember(DependencyObject obj)
        {
            return (string)obj.GetValue(DisplayMemberMemberProperty);
        }

        public static void SetDisplayMemberMember(DependencyObject obj, string value)
        {
            obj.SetValue(DisplayMemberMemberProperty, value);
        }

        // Using a DependencyProperty as the backing store for DisplayMember.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemberMemberProperty =
            DependencyProperty.RegisterAttached("DisplayMemberMember", typeof(string), typeof(DataGridColumns), new UIPropertyMetadata(null));


        private static void ColumnsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = obj as DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Columns.Clear();

                if (e.OldValue != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(e.OldValue);
                    if (view != null)
                        RemoveHandlers(dataGrid, view);
                }

                if (e.NewValue != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(e.NewValue);
                    if (view != null)
                    {
                        AddHandlers(dataGrid, view);
                        CreateColumns(dataGrid, view);
                    }
                }
            }
        }

        private static IDictionary<ICollectionView, List<DataGrid>> _dataGridsByColumnsSource =
            new Dictionary<ICollectionView, List<DataGrid>>();

        private static List<DataGrid> GetDataGridsForColumnSource(ICollectionView columnSource)
        {
            List<DataGrid> dataGrids;
            if (!_dataGridsByColumnsSource.TryGetValue(columnSource, out dataGrids))
            {
                dataGrids = new List<DataGrid>();
                _dataGridsByColumnsSource.Add(columnSource, dataGrids);
            }
            return dataGrids;
        }

        private static void AddHandlers(DataGrid dataGrid, ICollectionView view)
        {
            GetDataGridsForColumnSource(view).Add(dataGrid);
            view.CollectionChanged += ColumnsSource_CollectionChanged;
        }

        private static void CreateColumns(DataGrid dataGrid, ICollectionView view)
        {
            foreach (var item in view)
            {
                DataGridColumn column = CreateColumn(dataGrid, item);
                dataGrid.Columns.Add(column);
            }
        }

        private static void RemoveHandlers(DataGrid dataGrid, ICollectionView view)
        {
            view.CollectionChanged -= ColumnsSource_CollectionChanged;
            GetDataGridsForColumnSource(view).Remove(dataGrid);
        }

        private static void ColumnsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ICollectionView view = sender as ICollectionView;
            var dataGrids = GetDataGridsForColumnSource(view);
            if (dataGrids == null || dataGrids.Count == 0)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var dataGrid in dataGrids)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            DataGridColumn column = CreateColumn(dataGrid, e.NewItems[i]);
                            dataGrid.Columns.Insert(e.NewStartingIndex + i, column);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (var dataGrid in dataGrids)
                    {
                        List<DataGridColumn> columns = new List<DataGridColumn>();
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            DataGridColumn column = dataGrid.Columns[e.OldStartingIndex + i];
                            columns.Add(column);
                        }
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            DataGridColumn column = columns[i];
                            dataGrid.Columns.Insert(e.NewStartingIndex + i, column);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var dataGrid in dataGrids)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            dataGrid.Columns.RemoveAt(e.OldStartingIndex);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var dataGrid in dataGrids)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            DataGridColumn column = CreateColumn(dataGrid, e.NewItems[i]);
                            dataGrid.Columns[e.NewStartingIndex + i] = column;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var dataGrid in dataGrids)
                    {
                        dataGrid.Columns.Clear();
                        CreateColumns(dataGrid, sender as ICollectionView);
                    }
                    break;
                default:
                    break;
            }
        }

        private static DataGridColumn CreateColumn(DataGrid dataGrid, object columnSource)
        {
            DataGridTextColumn column = new DataGridTextColumn();
            string headerTextMember = GetHeaderTextMember(dataGrid);
            string displayMemberMember = GetDisplayMemberMember(dataGrid);
            if (!string.IsNullOrEmpty(headerTextMember))
            {
                column.Header = GetPropertyValue(columnSource, headerTextMember);
            }
            if (!string.IsNullOrEmpty(displayMemberMember))
            {
                string propertyName = GetPropertyValue(columnSource, displayMemberMember) as string;
                column.Binding = new Binding(propertyName);
            }
            return column;
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj != null)
            {
                PropertyInfo prop = obj.GetType().GetProperty(propertyName);
                if (prop != null)
                    return prop.GetValue(obj, null);
            }
            return null;
        }
    }
}
