using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;

using DataGridColumn = Xceed.Wpf.DataGrid.ColumnBase;

using TMP.UI.Controls.WPF;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using Model;
    using Extensions;
    using ViewModel;

    public class ViewCollectionViewModel : BaseViewModel
    {
        private FiltersWindow _fw;
        private FilterViewModel _filterViewModel = new FilterViewModel();
        private ListCollectionView _collectionOfMeters;

        private string _fieldDisplayName = null;
        private string _fieldName = null;
        private string _value = null;

        public ViewCollectionViewModel()
        {
            string _none = "(нет)";

            CommandShowFilters = new DelegateCommand(() =>
            {
                if (_fw.IsVisible == false)
                    _fw.Show();
            }, "Фильтры");

            CommandDoSort = new DelegateCommand<HierarchicalItem>((field) =>
            {
                if (field == null) return;

                if (CollectionOfMeters == null) return;

                if (CollectionOfMeters.CanSort == false) return;

                SortingFields = String.Empty;
                CollectionOfMeters.SortDescriptions.Clear();
                if (field.Name == _none)
                    return;
                
                Stack<string> stack = new Stack<string>();
                var item = field;
                while (item != null)
                {
                    stack.Push(item.Name);
                    item = item.Parent;
                }

                string[] values = stack.ToArray();
                SortingFields = String.Join(" > ", values.Select(s => s.Replace("_", " ").AsParallel()));
                foreach (string value in values)
                    CollectionOfMeters.SortDescriptions.Add(new SortDescription(value, ListSortDirection.Ascending));           
            });
            CommandDoGroup = new DelegateCommand<HierarchicalItem>((field) =>
            {
                if (field == null) return;

                if (CollectionOfMeters == null) return;

                if (CollectionOfMeters.CanGroup == false) return;

                GroupingFields = String.Empty;
                CollectionOfMeters.GroupDescriptions.Clear();
                if (field.Name == _none)
                    return;
                
                Stack<string> stack = new Stack<string>();
                var item = field;
                while (item != null)
                {
                    stack.Push(item.Name);
                    item = item.Parent;
                }

                string[] values = stack.ToArray();
                GroupingFields = String.Join(" > ", values.Select(s => s.Replace("_", " ").AsParallel()));
                foreach (string value in values)
                    CollectionOfMeters.GroupDescriptions.Add(new PropertyGroupDescription(value));
            });

            SortFields = new List<HierarchicalItem>
            {
                new HierarchicalItem() { Name = _none, Command = CommandDoSort }
            };
            var l1 = ModelHelper.MeterPropertiesNames.Select(g => new HierarchicalItem
            {
                Name = g,
                Command = CommandDoSort,
                Items = g == _none ? null : ModelHelper.MeterPropertiesNames
                    .Where(i => i != _none && i != g).Select(c => new HierarchicalItem(c, CommandDoSort, true))
            });
            foreach (var item in l1) SortFields.Add(item);

            GroupFields = new List<HierarchicalItem>
            {
                new HierarchicalItem() { Name = _none, Command = CommandDoGroup }
            };
            var l2 = ModelHelper.MeterPropertiesNames.Select(a => new HierarchicalItem
            {
                Name = a,
                Command = CommandDoGroup,
                Items = a == _none ? null : ModelHelper.MeterPropertiesNames.Where(b => b != _none && b != a)
                    .Select(c => new HierarchicalItem(c, CommandDoGroup, true)
                    {
                        Items = c == _none ? null : ModelHelper.MeterPropertiesNames.Where(e => e != _none && e != c)
                            .Select(d => new HierarchicalItem(d, CommandDoGroup, true))
                    })
            });
            foreach (var item in l2) GroupFields.Add(item);


            CommandPrint = new DelegateCommand(() =>
            {
                ;
            },
            () => CollectionOfMeters != null && false,
            "Печать");
            CommandExport = new DelegateCommand(() =>
            {
                IsBusy = true;
                Status = "Экспорт данных";
                DetailedStatus = "подготовка ...";

                var task = System.Threading.Tasks.Task.Factory.StartNew(() => Export());
                task.ContinueWith(t =>
                {
                    IsBusy = false;
                    Status = null;
                    DetailedStatus = null;

                    MessageBox.Show(String.Format("Произошла ошибка при формировании отчёта.\nОписание: {0}", App.GetExceptionDetails(t.Exception)),
    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

                task.ContinueWith(t =>
                {
                    IsBusy = false;
                    Status = null;
                    DetailedStatus = null;
                }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);
            },
            () => CollectionOfMeters != null,
            "Экспорт");
        }

        /// <summary>
        /// Модель представления для окна просмотра коллекции счётчиков
        /// </summary>
        /// <param name="meters">Коллекция счётчиков</param>
        /// <param name="fieldDisplayName">Отображаемое имя поля, по которому сгруппирована или отфильтрована коллекция</param>
        /// <param name="fieldName">Ммя поля, по которому сгруппирована или отфильтрована коллекция</param>
        /// <param name="value">Значение, по которому отфильтрована коллекция</param>
        public ViewCollectionViewModel(System.Collections.IList meters, string fieldDisplayName = null, string fieldName = null, string value = null) : this()
        {
            if (meters == null)
                throw new ArgumentNullException("Meters collection");

            _collectionOfMeters = new ListCollectionView(meters);

            //SummaryInfoViewModel.Meters = meters as IList<Model.Meter>;

            _filterViewModel.Collection = _collectionOfMeters;
            _filterViewModel.Meters = meters as IList<Model.Meter>;

            _fieldDisplayName = fieldDisplayName;
            _fieldName = fieldName;
            _value = value;

            _fw = new FiltersWindow() { Owner = App.Current.MainWindow, DataContext = _filterViewModel };

            if (String.IsNullOrWhiteSpace(_value) == true && String.IsNullOrWhiteSpace(fieldName) == false)
            {
                _collectionOfMeters.GroupDescriptions.Add(new PropertyGroupDescription(fieldName));
                _collectionOfMeters.SortDescriptions.Add(new SortDescription(fieldName, ListSortDirection.Ascending));
                GroupingFields = _fieldDisplayName;
            }
        }
        ~ViewCollectionViewModel()
        {
            if (_fw != null && _fw.IsVisible)
                _fw.Close();
        }

        #region Properties

        public ViewModel.SummaryInfoViewViewModel SummaryInfoViewModel { get; private set; } = new SummaryInfoViewViewModel();

        public string WindowTitle
        {
            get
            {
                string title = "Просмотр списка";
                if (_collectionOfMeters != null && _collectionOfMeters.IsEmpty == false)
                {
                    // не указаны поля - просто просмотр коллекции
                    if (String.IsNullOrWhiteSpace(_value) && String.IsNullOrWhiteSpace(_fieldDisplayName))
                        title += " : всего: " + _collectionOfMeters.Count.ToString("N0");
                    if (String.IsNullOrWhiteSpace(_value) == false && String.IsNullOrWhiteSpace(_fieldDisplayName) == false)
                        title += " : отфильтровано по '" + _fieldDisplayName + "' по значению '" + _value + "'";
                    else
                        title += " : группировка по '" + _fieldDisplayName + "', всего: " + _collectionOfMeters.Count.ToString("N0");
                }
                return title;
            }
        }

        public ListCollectionView CollectionOfMeters
        {
            get { return _collectionOfMeters; }
            set { _collectionOfMeters = value; RaisePropertyChanged("CollectionOfMeters"); }
        }

        public IEnumerable<Meter> Meters => CollectionOfMeters?.SourceCollection.Cast<Meter>();

        public ICommand CommandDoSort { get; private set; }
        public ICommand CommandDoGroup { get; private set; }
        public ICollection<HierarchicalItem> GroupFields { get; private set; }
        public ICollection<HierarchicalItem> SortFields { get; private set; }

        private string _sortingFields = String.Empty;
        public string SortingFields
        {
            get { return _sortingFields; }
            private set { _sortingFields = value; RaisePropertyChanged("SortingFields"); }
        }
        private string _groupingFields = string.Empty;
        public string GroupingFields
        {
            get { return _groupingFields; }
            private set { _groupingFields = value; RaisePropertyChanged("GroupingFields"); }
        }
        public override ICommand CommandExport { get; }
        public override ICommand CommandPrint { get; }

        public ICommand CommandShowFilters { get; private set; }

        TableView _selectedView = TableView.ShortView;

        /// <summary>
        /// Режим просмотра таблицы - набор колонок
        /// </summary>
        public TableView SelectedView
        {
            get { return _selectedView; }
            set
            {
                _selectedView = value;
                RaisePropertyChanged("SelectedView");
                ChangeView();
            }
        }

        private ObservableCollection<DataGridColumn> _tableColumns;
        /// <summary>
        /// Коллекция колонок
        /// </summary>
        public ObservableCollection<DataGridColumn> TableColumns
        {
            get {
                if (_tableColumns == null)
                    ChangeView();
                  return _tableColumns;
                }
            private set { _tableColumns = value; RaisePropertyChanged("TableColumns"); }
        }

        #endregion 

        private void ChangeView()
        {
            Func<IList<string>, IEnumerable<Xceed.Wpf.DataGrid.TableField>> getFields = (names) =>
            {
                return names.Select(name => new Xceed.Wpf.DataGrid.TableField()
                {
                    Type = ModelHelper.MeterPropertiesCollection[name].PropertyType,
                    DisplayOrder = ModelHelper.MeterPropertiesCollection[name].Order,
                    Name = name,
                    DisplayName = ModelHelper.MeterPropertiesCollection[name].DisplayName,
                    GroupName = ModelHelper.MeterPropertiesCollection[name].GroupName,
                    IsVisible = ModelHelper.MeterPropertiesCollection[name].IsVisible
                });
            };

            IEnumerable < Xceed.Wpf.DataGrid.TableField> fields = null;
            switch (SelectedView)
            {
                case TableView.BaseView:
                    fields = getFields(Meter.ShortViewColumns);
                    break;
                case TableView.DetailedView:
                    fields = getFields(Meter.DetailedViewColumns);
                    break;
                case TableView.ShortView:
                    fields = getFields(Meter.ShortViewColumns);
                    break;
                case TableView.ОплатаView:
                    fields = getFields(Meter.ОплатаViewColumns);
                    break;
                case TableView.ПривязкаView:
                    fields = getFields(Meter.ПривязкаViewColumns);
                    break;
                default:
                    throw new NotImplementedException("Unknown TableView");
            }
            _tableColumns = new ObservableCollection<Xceed.Wpf.DataGrid.ColumnBase>(Xceed.Wpf.DataGrid.Extensions.DataGridControlExtensions.BuildColumns(fields));
        }

        private Dictionary<string, string> GetFieldsAndFormats()
        {
            var fieldsAndFormats = new Dictionary<string, string>();
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                var columns = TableColumns
                    .Where(c => c.Visible)
                    .OrderBy(c => c.VisiblePosition);
                var columnsFields = columns.Select(c => new
                {
                    Key = c.FieldName,
                    Format = c.CellContentStringFormat
                });
                foreach (var item in columnsFields)
                    fieldsAndFormats.Add(item.Key, item.Format);
            }));
            return fieldsAndFormats;
        }

        private void Export()
        {
            CollectionOfMeters.Export<Model.Meter>(
                GetFieldsAndFormats(), "Список счётчиков",
                ModelHelper.MeterGetPropertyValue);
        }
    }
}
