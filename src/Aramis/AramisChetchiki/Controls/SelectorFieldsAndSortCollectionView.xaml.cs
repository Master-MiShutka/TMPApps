namespace TMP.WORK.AramisChetchiki.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using DataGridWpf;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;

    /// <summary>
    /// Interaction logic for SelectorFieldsAndSortCollectionView.xaml
    /// </summary>
    public partial class SelectorFieldsAndSortCollectionView : UserControl, INotifyPropertyChanged
    {
        private readonly ICollectionView collectionView;
        private readonly SortDescriptionCollection sourceSortDescriptionCollection;
        private readonly IEnumerable<DataGridWpfColumnViewModel> sourceColumns;
        private bool canSelectFields = false;
        private ObservableCollection<Field> fields;
        private Field selectedField;
        private ObservableCollection<Field> sortingFields;
        private Field selectedSortingField;
        private Action closeAction;

        public SelectorFieldsAndSortCollectionView()
        {
            this.CommandOK = new DelegateCommand(() =>
            {
                /*
                _collectionView.SortDescriptions.Clear();
                foreach (Field item in _sortingFields)
                    _collectionView.SortDescriptions.Add(new SortDescription(item.FieldName, item.SortDirection));

                if (_canSelectFields)
                {
                    for (int index = 0; index < _fields.Count; index++)
                    {
                        Field item = _fields[index];
                        var column = _columns.First(c => c.FieldName == item.FieldName);
                        column.Visible = item.IsChecked;
                        if (column.VisiblePosition != item.DisplayIndex)
                        {
                            _columns.Move(column.VisiblePosition, index);
                            _columns[index].VisiblePosition = index;
                        }
                    }
                }*/

                this.CloseAction();
            });
            this.CommandCancel = new DelegateCommand(() =>
            {
                /*_collectionView.SortDescriptions.Clear();
                foreach (SortDescription item in _sourceSortDescriptionCollection)
                    _collectionView.SortDescriptions.Add(new SortDescription(item.PropertyName, item.Direction));

                if (_canSelectFields)
                {
                    foreach (ColumnBase item in _sourceColumns)
                    {
                        ColumnBase column = _columns.First((ColumnBase c) => c == item);
                        column.Visible = item.Visible;
                        if (column.VisiblePosition != item.VisiblePosition)
                            _columns.Move(column.VisiblePosition, item.VisiblePosition);
                    }
                }
                */

                this.CloseAction();
            });
            this.CommandApply = new DelegateCommand(
                () =>
            {
                /*_collectionView.SortDescriptions.Clear();
                foreach (Field item in _sortingFields)
                    _collectionView.SortDescriptions.Add(new SortDescription(item.FieldName, item.SortDirection));

                if (_canSelectFields)
                {
                    foreach (Field item in _fields)
                    {
                        ColumnBase column = _columns.First((ColumnBase c) => c.FieldName == item.FieldName);
                        column.Visible = item.IsChecked;
                        if (column.VisiblePosition != item.DisplayIndex)
                            _columns.Move(column.VisiblePosition, item.DisplayIndex);
                    }
                }*/
            }, () => this.Fields != null && this.SortingFields != null);

            this.CommandAddField = new DelegateCommand(
                () =>
            {
                if (this.SortingFields == null)
                {
                    this.SortingFields = new ObservableCollection<Field>();
                }

                this.SortingFields.Add(new Field(this.SelectedField.FieldName));
            }, () => this.Fields != null && this.Fields.Count > 0 && this.SelectedField != null);

            this.CommandClear = new DelegateCommand(
                () =>
            {
                this.SortingFields.Clear();
            }, () => this.SortingFields != null && this.SortingFields.Count > 0);

            this.CommandRemoveField = new DelegateCommand<Field>(
                (sd) =>
            {
                if (sd != null)
                {
                    this.SortingFields.Remove(sd);
                }
            }, (o) => this.SortingFields != null && this.SortingFields.Count > 0);

            this.CommandChangeSortDirection = new DelegateCommand<Field>((sd) =>
            {
                sd.SortDirection = sd.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            });

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.canSelectFields = true;
                this.fields = new ObservableCollection<Field>(new Field[]
                {
                    new Field() { FieldName = "ttyyU uI", IsChecked = true, Header = "Hyjbj" },
                    new Field() { FieldName = "ttyyU uI", IsChecked = false, Header = "Htud" },
                    new Field() { FieldName = "Data Grid Column", IsChecked = true, Header = "DataGridColumn" },
                });

                this.selectedField = this.Fields[2];

                this.sortingFields = new ObservableCollection<Field>(new Field[]
                {
                    new Field("Gtyu") { SortDirection = ListSortDirection.Ascending },
                    new Field("List Sort Direction") { SortDirection = ListSortDirection.Descending },
                });

                return;
            }

            this.InitializeComponent();
            this.DataContext = this;
        }

        public SelectorFieldsAndSortCollectionView(ObservableCollection<Field> fields, ICollectionView collectionView) : this()
        {
            this.canSelectFields = false;
            this.collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));
            this.sourceSortDescriptionCollection = this.collectionView.SortDescriptions;
            this.sortingFields = new ObservableCollection<Field>(this.sourceSortDescriptionCollection.ToFields());
            this.fields = fields;
        }

        public SelectorFieldsAndSortCollectionView(IEnumerable<DataGridWpfColumnViewModel> columns, ICollectionView collectionView) : this()
        {
            this.canSelectFields = true;
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            this.collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));

            this.sourceColumns = columns;
            this.sourceSortDescriptionCollection = this.collectionView.SortDescriptions;
            this.sortingFields = new ObservableCollection<Field>(this.sourceSortDescriptionCollection.ToFields());

            this.fields = new ObservableCollection<Field>(columns.ToFields());
        }

        public bool CanSelectFields => this.canSelectFields;

        public ObservableCollection<Field> Fields
        {
            get => this.fields;
            private set => this.SetProperty(ref this.fields, value);
        }

        public Field SelectedField
        {
            get => this.selectedField;
            set => this.SetProperty(ref this.selectedField, value);
        }

        public ObservableCollection<Field> SortingFields
        {
            get => this.sortingFields;
            private set => this.SetProperty(ref this.sortingFields, value);
        }

        public Field SelectedSortingField
        {
            get => this.selectedSortingField;
            set => this.SetProperty(ref this.selectedSortingField, value);
        }

        public Action CloseAction
        {
            get => this.closeAction;
            set => this.SetProperty(ref this.closeAction, value);
        }

        public ICommand CommandAddField { get; }

        public ICommand CommandClear { get; }

        public ICommand CommandRemoveField { get; }

        public ICommand CommandChangeSortDirection { get; }

        public ICommand CommandOK { get; }

        public ICommand CommandCancel { get; }

        public ICommand CommandApply { get; }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
