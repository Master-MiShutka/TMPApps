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
            });

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

            CommandChangeView = new DelegateCommand<object>((o) =>
           {
               object[] values = (object[])o;
               if (values == null) return;
               string viewName = values[0] as string;
               DataGrid table = values[1] as DataGrid;
               if (String.IsNullOrWhiteSpace(viewName) == false && table != null)
               {
                   table.BeginInit();
                   table.AutoGenerateColumns = false;
                   table.Columns.Clear();

                   ChangeView();

                   table.EndInit();
               }
           },
           () => CollectionOfMeters != null,
           "Вид");

            CommandPrint = new DelegateCommand(() =>
            {
                ;
            },
            () => CollectionOfMeters != null && false,
            "Печать");
            CommandExport = new DelegateCommand<DataGrid>((table) =>
            {
                IsBusy = true;
                Status = "Экспорт данных";
                DetailedStatus = "подготовка ...";

                var task = System.Threading.Tasks.Task.Factory.StartNew(() => Export(table));
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

        public ICommand CommandChangeView { get; private set; }

        public override ICommand CommandExport { get; }
        public override ICommand CommandPrint { get; }

        public ICommand CommandShowFilters { get; private set; }

        TableView _selectedView = TableView.ShortView;
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

        private IList<string> _tableColumnsFields = null;
        private ObservableCollection<DataGridColumn> _tableColumns;
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
            Func<IList<string>, IEnumerable<Properties.TableField>> getFields = (names) =>
            {
                return names.Select(name => new Properties.TableField()
                {
                    Type = ModelHelper.MeterPropertiesCollection[name].PropertyType,
                    DisplayOrder = ModelHelper.MeterPropertiesCollection[name].Order,
                    Name = name,
                    DisplayName = ModelHelper.MeterPropertiesCollection[name].DisplayName,
                    GroupName = ModelHelper.MeterPropertiesCollection[name].GroupName,
                    IsVisible = ModelHelper.MeterPropertiesCollection[name].IsVisible
                });
            };

            IEnumerable<Properties.TableField> fields = null;
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
            _tableColumns = new ObservableCollection<DataGridColumn>(DataGridExtensions.BuildColumns(fields));
        }

        private void Export(DataGrid dataGrid)
        {
            IEnumerable<Meter> collection = CollectionOfMeters.SourceCollection.Cast<Meter>();
            int numberOfRows = collection.Count();
            // +1 т.к. первый столбец номер по порядку
            int numberOfColumns = _tableColumnsFields.Count() + 1;
            // +1 т.к. первая строка шапка
            object[,] output = new object[numberOfRows + 1, numberOfColumns];

            output[0, 0] = "№ п/п";
            int ind = 1;
            foreach (var column in _tableColumnsFields)
                output[0, ind++] = column.Replace("_", " ");

            Type meterType = typeof(Model.Meter);
            int rowIndex = 1;
            foreach (Model.Meter meter in collection)
            {
                output[rowIndex, 0] = rowIndex;
                ind = 1; // т.к. первый столбец номер по порядку
                foreach (string column in _tableColumnsFields)
                {
                    object value = ModelHelper.MeterGetPropertyValue(meter, column);
                    output[rowIndex, ind++] = value;
                }
                rowIndex++;
            }

            string fileName = System.IO.Path.GetTempFileName();
            fileName = System.IO.Path.ChangeExtension(fileName, "xls");

            Excel.Application excelApplication = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;
            try
            {
                excelApplication = new Excel.Application();
                excelApplication.DisplayAlerts = false;
                excelApplication.ScreenUpdating = false;
                Excel.Tools.CommonUtils utils = new Excel.Tools.CommonUtils(excelApplication);

                xlWorkbook = excelApplication.Workbooks.Add();
                xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];

                Excel.Range header = xlWorksheet.Range("A1");
                header.Value = "Перечень счетчиков";
                header.Resize(1, numberOfColumns).Merge();
                using (Excel.Font font = header.Font)
                {
                    font.Size = 14;
                    font.Bold = true;
                }
                header.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                Excel.Range data = xlWorksheet.Range("A2").Resize(numberOfRows + 1, numberOfColumns);
                data.NumberFormat = "@";

                data.Value = output;

                xlWorksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, data,
                    Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name = "DataTable";
                xlWorksheet.ListObjects["DataTable"].TableStyle = "TableStyleMedium1";


                var ps = xlWorksheet.PageSetup;
                ps.PaperSize = XlPaperSize.xlPaperA4;
                ps.Orientation = XlPageOrientation.xlLandscape;
                ps.Zoom = false;
                ps.FitToPagesWide = 1;
                ps.FitToPagesTall = false;

                ps.LeftMargin = excelApplication.CentimetersToPoints(1.0);
                ps.RightMargin = excelApplication.CentimetersToPoints(1.0);
                ps.TopMargin = excelApplication.CentimetersToPoints(2.0);
                ps.BottomMargin = excelApplication.CentimetersToPoints(1.0);

                ps.HeaderMargin = excelApplication.CentimetersToPoints(0.6);
                ps.FooterMargin = excelApplication.CentimetersToPoints(0.6);

                ps.CenterHorizontally = true;
                ps.RightHeader = DateTime.Now.ToLongDateString();
                ps.CenterFooter = "Страница &P / &N";
                ps.PrintArea = header.Resize(numberOfRows + 2, numberOfColumns).Address;

                xlWorkbook.SaveAs(fileName);
                xlWorkbook.Close(false);

                excelApplication.ScreenUpdating = true;
                excelApplication.Quit();

                System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception e)
            {
#if DEBUG
                App.ToDebug(e);
#endif
                MessageBox.Show("Произошла ошибка:\n" + App.GetExceptionDetails(e), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                xlWorkbook.Dispose();
                xlWorksheet.Dispose();
                excelApplication.Dispose();
            }
        }
    }
}
