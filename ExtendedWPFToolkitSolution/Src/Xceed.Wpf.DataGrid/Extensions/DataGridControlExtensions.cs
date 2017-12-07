using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid.Extensions
{
    public static class DataGridControlExtensions
    {
        public static IEnumerable<ColumnBase> BuildColumns(IEnumerable<TableField> fields)
        {
            int index = 0;
            foreach (var item in fields)
                item.DisplayOrder = index++;
            List<ColumnBase> result = new List<ColumnBase>();
            foreach (TableField field in fields)
            {
                if (field == null)
                    System.Diagnostics.Debugger.Break();
                TypeCode typecode = Type.GetTypeCode(field.Type);

                if (Nullable.GetUnderlyingType(field.Type) != null)
                {
                    typecode = Type.GetTypeCode(Nullable.GetUnderlyingType(field.Type));
                }

                switch (typecode)
                {
                    /*case TypeCode.Boolean:

                        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(DataGridCheckBox));
                        factory.SetBinding(DataGridCheckBox.IsCheckedProperty, new Binding(field.Name));
                        factory.SetBinding(DataGridCheckBox.ToolTipProperty, new Binding());

                        result.Add(new UnboundColumn()
                        {
                            Title = field.DisplayName,
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible,
                            CellContentTemplate = new DataTemplate() { VisualTree = factory }
                        });
                        break;*/
                    case TypeCode.Double:
                        result.Add(new Column()
                        {
                            Title = field.DisplayName,
                            CellContentStringFormat = "N0",
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible
                        });
                        break;
                    case TypeCode.DateTime:
                        result.Add(new Column()
                        {
                            Title = field.DisplayName,
                            CellContentStringFormat = "dd.MM.yyyy",
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible
                        });
                        break;
                    default:
                        result.Add(new Column()
                        {
                            Title = field.DisplayName,
                            FieldName = field.Name,
                            VisiblePosition = field.DisplayOrder,
                            Visible = field.IsVisible
                        });
                        break;
                }
            }
            return result;
        }

        #region Bindable Columns

        [AttachedPropertyBrowsableForType(typeof(DataGridControl))]
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
                typeof(DataGridControlExtensions),
                new UIPropertyMetadata(
                    null,
                    ColumnsSourceChanged));


        [AttachedPropertyBrowsableForType(typeof(DataGridControl))]
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
            DependencyProperty.RegisterAttached("HeaderTextMember", typeof(string), typeof(DataGridControlExtensions), new UIPropertyMetadata(null));


        [AttachedPropertyBrowsableForType(typeof(DataGridControl))]
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
            DependencyProperty.RegisterAttached("DisplayMemberMember", typeof(string), typeof(DataGridControlExtensions), new UIPropertyMetadata(null));


        private static void ColumnsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DataGridControl dataGridControl = obj as DataGridControl;
            if (dataGridControl != null)
            {
                dataGridControl.Columns.Clear();

                if (e.OldValue != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(e.OldValue);
                    if (view != null)
                        RemoveHandlers(dataGridControl, view);
                }

                if (e.NewValue != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(e.NewValue);
                    if (view != null)
                    {
                        AddHandlers(dataGridControl, view);
                        CreateColumns(dataGridControl, view);
                    }
                }
            }
        }

        private static IDictionary<ICollectionView, List<DataGridControl>> _dataGridControlsByColumnsSource =
            new Dictionary<ICollectionView, List<DataGridControl>>();

        private static List<DataGridControl> GetGridViewsForColumnSource(ICollectionView columnSource)
        {
            List<DataGridControl> dataGridControls;
            if (!_dataGridControlsByColumnsSource.TryGetValue(columnSource, out dataGridControls))
            {
                dataGridControls = new List<DataGridControl>();
                _dataGridControlsByColumnsSource.Add(columnSource, dataGridControls);
            }
            return dataGridControls;
        }

        private static void AddHandlers(DataGridControl dataGridControl, ICollectionView view)
        {
            GetGridViewsForColumnSource(view).Add(dataGridControl);
            view.CollectionChanged += ColumnsSource_CollectionChanged;
        }

        private static void CreateColumns(DataGridControl dataGridControl, ICollectionView view)
        {
            foreach (var item in view)
            {
                if (item is ColumnBase)
                    dataGridControl.Columns.Add(item as ColumnBase);
                else
                {
                    ColumnBase column = CreateColumn(dataGridControl, item);
                    dataGridControl.Columns.Add(column);
                }
            }
        }

        private static void RemoveHandlers(DataGridControl gridView, ICollectionView view)
        {
            view.CollectionChanged -= ColumnsSource_CollectionChanged;
            GetGridViewsForColumnSource(view).Remove(gridView);
        }

        private static void ColumnsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ICollectionView view = sender as ICollectionView;
            var dataGridControls = GetGridViewsForColumnSource(view);
            if (dataGridControls == null || dataGridControls.Count == 0)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var dataGridControl in dataGridControls)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            ColumnBase column = CreateColumn(dataGridControl, e.NewItems[i]);
                            dataGridControl.Columns.Insert(e.NewStartingIndex + i, column);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (var dataGridControl in dataGridControls)
                    {
                        List<ColumnBase> columns = new List<ColumnBase>();
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            ColumnBase column = dataGridControl.Columns[e.OldStartingIndex + i];
                            columns.Add(column);
                        }
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            ColumnBase column = columns[i];
                            dataGridControl.Columns.Insert(e.NewStartingIndex + i, column);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var dataGridControl in dataGridControls)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            dataGridControl.Columns.RemoveAt(e.OldStartingIndex);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var dataGridControl in dataGridControls)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            ColumnBase column = CreateColumn(dataGridControl, e.NewItems[i]);
                            dataGridControl.Columns[e.NewStartingIndex + i] = column;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var dataGridControl in dataGridControls)
                    {
                        dataGridControl.Columns.Clear();
                        CreateColumns(dataGridControl, sender as ICollectionView);
                    }
                    break;
                default:
                    break;
            }
        }

        private static Column CreateColumn(DataGridControl dataGridControl, object columnSource)
        {
            Column column = new Column();
            string headerTextMember = GetHeaderTextMember(dataGridControl);
            string displayMemberMember = GetDisplayMemberMember(dataGridControl);
            if (!string.IsNullOrEmpty(headerTextMember))
            {
                column.Title = GetPropertyValue(columnSource, headerTextMember);
            }
            if (!string.IsNullOrEmpty(displayMemberMember))
            {
                string propertyName = GetPropertyValue(columnSource, displayMemberMember) as string;
                column.DisplayMemberBindingInfo.Path.Path = propertyName;
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

        #endregion
    }
}
