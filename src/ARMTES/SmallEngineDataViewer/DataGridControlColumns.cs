using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

using Xceed.Wpf.DataGrid;

namespace TMP.ARMTES
{
    public static class DataGridControlColumns
    {
        [AttachedPropertyBrowsableForType(typeof(Xceed.Wpf.DataGrid.DataGridControl))]
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
                typeof(DataGridControlColumns),
                new UIPropertyMetadata(
                    null,
                    ColumnsSourceChanged));

        private static void ColumnsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Xceed.Wpf.DataGrid.DataGridControl dataGrid = obj as Xceed.Wpf.DataGrid.DataGridControl;
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

        private static IDictionary<ICollectionView, List<Xceed.Wpf.DataGrid.DataGridControl>> _dataGridsByColumnsSource =
            new Dictionary<ICollectionView, List<Xceed.Wpf.DataGrid.DataGridControl>>();

        private static List<Xceed.Wpf.DataGrid.DataGridControl> GetDataGridControlsForColumnSource(ICollectionView columnSource)
        {
            List<Xceed.Wpf.DataGrid.DataGridControl> dataGrids;
            if (!_dataGridsByColumnsSource.TryGetValue(columnSource, out dataGrids))
            {
                dataGrids = new List<Xceed.Wpf.DataGrid.DataGridControl>();
                _dataGridsByColumnsSource.Add(columnSource, dataGrids);
            }
            return dataGrids;
        }

        private static void AddHandlers(Xceed.Wpf.DataGrid.DataGridControl dataGrid, ICollectionView view)
        {
            GetDataGridControlsForColumnSource(view).Add(dataGrid);
            view.CollectionChanged += ColumnsSource_CollectionChanged;
        }

        private static void CreateColumns(Xceed.Wpf.DataGrid.DataGridControl dataGrid, ICollectionView view)
        {
            foreach (var item in view)
            {
                Xceed.Wpf.DataGrid.Column column = CreateColumn(dataGrid, item);
                dataGrid.Columns.Add(column);
            }
        }

        private static void RemoveHandlers(Xceed.Wpf.DataGrid.DataGridControl gridView, ICollectionView view)
        {
            view.CollectionChanged -= ColumnsSource_CollectionChanged;
            GetDataGridControlsForColumnSource(view).Remove(gridView);
        }

        private static void ColumnsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ICollectionView view = sender as ICollectionView;
            var dataGrids = GetDataGridControlsForColumnSource(view);
            if (dataGrids == null || dataGrids.Count == 0)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var dataGrid in dataGrids)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            Xceed.Wpf.DataGrid.Column column = CreateColumn(dataGrid, e.NewItems[i]);
                            dataGrid.Columns.Insert(e.NewStartingIndex + i, column);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (var dataGrid in dataGrids)
                    {
                        List<Xceed.Wpf.DataGrid.Column> columns = new List<Xceed.Wpf.DataGrid.Column>();
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            Xceed.Wpf.DataGrid.Column column = (Xceed.Wpf.DataGrid.Column)dataGrid.Columns[e.OldStartingIndex + i];
                            columns.Add(column);
                        }
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            Xceed.Wpf.DataGrid.Column column = columns[i];
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
                            Xceed.Wpf.DataGrid.Column column = CreateColumn(dataGrid, e.NewItems[i]);
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

        private static Xceed.Wpf.DataGrid.Column CreateColumn(Xceed.Wpf.DataGrid.DataGridControl dataGrid, object columnSource)
        {
            Xceed.Wpf.DataGrid.Column column = new Xceed.Wpf.DataGrid.Column();
            column.Title = GetPropertyValue(columnSource, "HeaderText");
            column.FieldName = GetPropertyValue(columnSource, "DisplayMember") as string;
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
