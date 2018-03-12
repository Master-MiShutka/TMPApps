using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Xceed.Wpf.DataGrid;

namespace TMP.UI.Controls.WPF
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : DataGridControl
    {

        public TableView()
        {
            InitializeComponent();

            foreach (var item in this.ClipboardExporters)
                System.Diagnostics.Debug.WriteLine("exporter - " + item.Key);
        }


        #region BindableColumns
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.Register("BindableColumns",
                                                typeof(ObservableCollection<ColumnBase>),
                                                typeof(TableView),
                                                new UIPropertyMetadata(null, BindableColumnsPropertyChanged));
        private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            TableView dataGrid = source as TableView;
            ObservableCollection<ColumnBase> columns = e.NewValue as ObservableCollection<ColumnBase>;
            dataGrid.Columns.Clear();
            if (columns == null)
            {
                return;
            }
            foreach (ColumnBase column in columns)
            {
                dataGrid.Columns.Add(column);
            }
            columns.CollectionChanged += (sender, e2) =>
            {
                NotifyCollectionChangedEventArgs ne = e2 as NotifyCollectionChangedEventArgs;
                if (ne.Action == NotifyCollectionChangedAction.Reset)
                {
                    dataGrid.Columns.Clear();
                    foreach (ColumnBase column in ne.NewItems)
                    {
                        dataGrid.Columns.Add(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (ColumnBase column in ne.NewItems)
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
                    foreach (ColumnBase column in ne.OldItems)
                    {
                        dataGrid.Columns.Remove(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Replace)
                {
                    dataGrid.Columns[ne.NewStartingIndex] = ne.NewItems[0] as ColumnBase;
                }
            };
        }
        public static void SetBindableColumns(DependencyObject element, ObservableCollection<ColumnBase> value)
        {
            element.SetValue(BindableColumnsProperty, value);
        }
        public static ObservableCollection<ColumnBase> GetBindableColumns(DependencyObject element)
        {
            return (ObservableCollection<ColumnBase>)element.GetValue(BindableColumnsProperty);
        }
        #endregion
    }
}
