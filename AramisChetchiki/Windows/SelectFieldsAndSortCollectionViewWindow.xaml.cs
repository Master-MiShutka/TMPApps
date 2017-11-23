using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TMP.UI.Controls.WPF;

namespace TMP.WORK.AramisChetchiki.Windows
{
    /// <summary>
    /// Interaction logic for SelectFieldsAndSortCollectionViewWindow.xaml
    /// </summary>
    public partial class SelectFieldsAndSortCollectionViewWindow : Window, INotifyPropertyChanged
    {
        public class Field
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public bool IsChecked { get; set; } = true;
            public int DisplayIndex { get; set; }
        }

        public class SortingField : INotifyPropertyChanged
        {
            public string PropertyName { get; set; }
            ListSortDirection _direction;
            public ListSortDirection Direction
            {
                get { return _direction; }
                set { if (value == _direction) return; _direction = value; RaisePropertyChanged("Direction"); }
            }

            public SortingField(string name, ListSortDirection direction = ListSortDirection.Ascending)
            {
                PropertyName = name;
                Direction = direction;
            }

            #region INotifyPropertyChanged implementation
            [field: NonSerializedAttribute()]
            public event PropertyChangedEventHandler PropertyChanged;
            protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
            {
                if (Equals(field, value)) { return false; }

                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
            protected void RaisePropertyChanged(string propertyName = null)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
            protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

            {
                PropertyChanged?.Invoke(this, e);
            }
            #endregion

        }

        private ICollectionView _collectionView;
        private SortDescriptionCollection _sourceSortDescriptionCollection;
        private ObservableCollection<DataGridColumn> _columns, _sourceColumns;

        private bool _canSelectFields = false;

        public SelectFieldsAndSortCollectionViewWindow()
        {
            CommandOK = new DelegateCommand(() =>
            {
                ;
                _collectionView.SortDescriptions.Clear();
                foreach (SortingField item in _sortingFields)
                    _collectionView.SortDescriptions.Add(new SortDescription(item.PropertyName, item.Direction));

                if (_canSelectFields)
                {
                    for (int index = 0; index < _fields.Count; index++)
                    {
                        Field item = _fields[index];
                        DataGridColumn column = _columns.First(c => ((c as DataGridBoundColumn).Binding as Binding).Path.Path == item.FieldName);
                        column.Visibility = item.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                        if (column.DisplayIndex != item.DisplayIndex)
                        {
                            _columns.Move(column.DisplayIndex, index);
                            _columns[index].DisplayIndex = index;
                        }
                    }
                }

                Close();
            });
            CommandCancel = new DelegateCommand(() =>
            {
                _collectionView.SortDescriptions.Clear();
                foreach (SortDescription item in _sourceSortDescriptionCollection)
                    _collectionView.SortDescriptions.Add(new SortDescription(item.PropertyName, item.Direction));

                if (_canSelectFields)
                {
                    foreach (DataGridBoundColumn item in _sourceColumns)
                    {
                        DataGridColumn column = _columns.First(c => c == item);
                        column.Visibility = item.Visibility;
                        if (column.DisplayIndex != item.DisplayIndex)
                            _columns.Move(column.DisplayIndex, item.DisplayIndex);
                    }
                }

                Close();
            });
            CommandApply = new DelegateCommand(() =>
            {
                _collectionView.SortDescriptions.Clear();
                foreach (SortingField item in _sortingFields)
                    _collectionView.SortDescriptions.Add(new SortDescription(item.PropertyName, item.Direction));

                if (_canSelectFields)
                {
                    foreach (Field item in _fields)
                    {
                        DataGridColumn column = _columns.First(c => ((c as DataGridBoundColumn).Binding as Binding).Path.Path == item.FieldName);
                        column.Visibility = item.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                        if (column.DisplayIndex != item.DisplayIndex)
                            _columns.Move(column.DisplayIndex, item.DisplayIndex);
                    }
                }
            }, () => Fields != null && SortingFields != null);

            CommandAddField = new DelegateCommand(() =>
            {
                if (SortingFields == null)
                    SortingFields = new ObservableCollection<SortingField>();
                SortingFields.Add(new SortingField(SelectedField.Name));
            }, () => Fields != null && Fields.Count > 0 && SelectedField != null);

            CommandClear = new DelegateCommand(() =>
            {
                SortingFields.Clear();
            }, () => SortingFields != null && SortingFields.Count > 0);

            CommandRemoveSortingField = new DelegateCommand<SortingField>((sd) =>
            {
                if (sd != null)
                    SortingFields.Remove(sd);
            }, () => SortingFields != null && SortingFields.Count > 0);

            CommandChangeSortDirection = new DelegateCommand<SortingField>((sd) =>
            {
                sd.Direction = sd.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            });

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                _canSelectFields = true;
                Fields = new ObservableCollection<Field>(new Field[] {
                    new Field() { FieldName = "ttyyU uI", IsChecked = true, Name = "Hyjbj"},
                    new Field() { FieldName = "ttyyU uI", IsChecked = false, Name = "Htud"},
                    new Field() { FieldName = "Data Grid Column", IsChecked = true, Name = "DataGridColumn"}
                });

                SelectedField = Fields[2];

                SortingFields = new ObservableCollection<SortingField>(new SortingField[]
                {
                    new SortingField("Gtyu", ListSortDirection.Ascending),
                    new SortingField("List Sort Direction", ListSortDirection.Descending)
                });

                return;
            }

            InitializeComponent();
            DataContext = this;
        }

        public SelectFieldsAndSortCollectionViewWindow(ref ObservableCollection<string> fields, ICollectionView collectionView) : this()
        {
            _canSelectFields = false;
            _collectionView = collectionView ?? throw new ArgumentNullException("ICollectionView");
            _sourceSortDescriptionCollection = _collectionView.SortDescriptions;

            _sortingFields = new ObservableCollection<SortingField>();
            if (_sourceSortDescriptionCollection != null)
                foreach (SortDescription item in _sourceSortDescriptionCollection)
                    _sortingFields.Add(new SortingField(item.PropertyName, item.Direction));

            if (fields == null)
                throw new ArgumentNullException("Fields");
            _fields = new ObservableCollection<Field>();
            foreach (string field in fields)
                _fields.Add(new Field() { Name = field });
        }

        public SelectFieldsAndSortCollectionViewWindow(ObservableCollection<DataGridColumn> columns, ICollectionView collectionView) : this()
        {
            _canSelectFields = true;







            if (columns == null)
                throw new ArgumentNullException("Columns");
            _columns = columns;

            DataGridColumn[] copyOfColumnsArray = new DataGridColumn[columns.Count];
            columns.CopyTo(copyOfColumnsArray, 0);
            _sourceColumns = new ObservableCollection<DataGridColumn>(copyOfColumnsArray);

            _collectionView = collectionView ?? throw new ArgumentNullException("ICollectionView");
            _sourceSortDescriptionCollection = _collectionView.SortDescriptions;

            _sortingFields = new ObservableCollection<SortingField>();
            if (_sourceSortDescriptionCollection != null)
                foreach (SortDescription item in _sourceSortDescriptionCollection)
                    _sortingFields.Add(new SortingField(item.PropertyName, item.Direction));

            if (columns == null)
                throw new ArgumentNullException("Fields");
            _fields = new ObservableCollection<Field>();
            foreach (DataGridColumn column in columns)
                _fields.Add(new Field() 
                {
                    Name = column.Header.ToString(),
                    FieldName = ((column as DataGridBoundColumn).Binding as Binding).Path.Path,
                    IsChecked = column.Visibility == Visibility.Visible,
                    DisplayIndex = column.DisplayIndex
                });
        }

        public bool CanSelectFields => _canSelectFields;

        private ObservableCollection<Field> _fields;
        public ObservableCollection<Field> Fields
        {
            get { return _fields; }
            set { _fields = value; RaisePropertyChanged("Fields"); }
        }
        public Field SelectedField { get; set; }

        private ObservableCollection<SortingField> _sortingFields;
        public ObservableCollection<SortingField> SortingFields
        {
            get { return _sortingFields; }
            set { _sortingFields = value; RaisePropertyChanged("SortingFields"); }
        }
        public SortingField SelectedSortingField { get; set; }

        public ICommand CommandAddField { get; }
        public ICommand CommandClear { get; }
        public ICommand CommandRemoveSortingField { get; }
        public ICommand CommandChangeSortDirection { get; }

        public ICommand CommandOK { get; }
        public ICommand CommandCancel { get; }
        public ICommand CommandApply { get; }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
