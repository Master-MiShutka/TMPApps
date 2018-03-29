using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Data;
using System.Threading;
using System.Windows.Threading;

using TMP.UI.Controls.WPF;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Extensions;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using Model;
    using Extensions;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;

    public class ChangesOfMetersViewModel : BaseDataViewModel<ChangesOfMeters>
    {
        public ChangesOfMetersViewModel()
        {
            DateTime today = DateTime.Today;
            DateTime beginOfMonth = today.AddDays(1 - today.Day);
            _fromDate = beginOfMonth.AddMonths(-2);
            _toDate = beginOfMonth.AddMonths(-1).AddDays(-1);

            CommandExport = new DelegateCommand(() =>
            {
                IsBusy = true;
                App.DoEvents();

                Status = "Экспорт данных";
                DetailedStatus = "подготовка ...";

                var task = System.Threading.Tasks.Task.Factory.StartNew(() => Export());
                task.ContinueWith(t =>
                {
                    MessageBox.Show(String.Format("Произошла ошибка при формировании отчёта.\nОписание: {0}", App.GetExceptionDetails(t.Exception)),
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

                task.ContinueWith(t =>
                {
                    IsBusy = false;
                    Status = null;
                    DetailedStatus = null;
                });


            }, () => View != null, "Экспорт");
            CommandPrint = new DelegateCommand(() =>
            {
                IsBusy = true;
                App.DoEvents();

                Status = "Печать данных";
                DetailedStatus = "подготовка ...";
                App.DoEvents();
                try
                {
                    FlowDocument doc = GenerateFlowDocumentFromPrint();
                    if (doc == null) return;
                    var window = new PrintPreviewWindow(doc);
                    window.Owner = App.Current.MainWindow;
                    window.ShowDialog();
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Произошла ошибка при формировании отчёта.\nОписание: {0}",
                        App.GetExceptionDetails(e)),
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsAnalizingData = false;
                    IsBusy = false;
                    Status = null;
                    DetailedStatus = null;
                }
            }, () => View != null, "Печать");

            CommandSetSorting = new DelegateCommand(() =>
            {
                Windows.SelectFieldsAndSortCollectionViewWindow window = new Windows.SelectFieldsAndSortCollectionViewWindow(this.TableColumns, this.View);
                window.ShowDialog();
            }, () => Data != null, "Сортировка");

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                IsBusy = false;
                Status = "подготовка";
                Data = new List<ChangesOfMeters>();
                //CreateView();
                return;
            }
            IsAnalizingData = true;
        }

        public ChangesOfMetersViewModel(ICollection<Model.ChangesOfMeters> data) : this()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Data = data;
                CreateView();
            });
        }

        #region Properties

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; RaisePropertyChanged("FromDate"); OnDateChanged(); }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get { return _toDate; }
            set { _toDate = value; RaisePropertyChanged("ToDate"); OnDateChanged(); }
        }

        private ICollection<ChangesOfMeters> _data;
        public override ICollection<ChangesOfMeters> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                RaisePropertyChanged("Data");
                if (_data == null)
                    return;
                else
                    IsAnalizingData = true;
            }
        }

        private ICollectionView _view;
        public override ICollectionView View
        {
            get
            {
                if (_data == null)
                    return _view = null;
                return _view;
            }
            set { _view = value; RaisePropertyChanged("View"); }
        }

        private ObservableCollection<ColumnBase> _tableColumns;
        public ObservableCollection<ColumnBase> TableColumns
        {
            get
            {
                return _tableColumns ??
                    (_tableColumns = new ObservableCollection<ColumnBase>(DataGridControlExtensions.BuildColumns(
                        Properties.Settings.Default.GetChangesOfMetersColumnsNames())));
            }
            private set { _tableColumns = value; RaisePropertyChanged("TableColumns"); }
        }

        #region Commands

        public ICommand CommandSetSorting { get; }

        public override ICommand CommandExport { get; }
        public override ICommand CommandPrint { get; }

        #endregion

        #endregion

        #region Private methods

        private void CreateView()
        {
            IsBusy = true;
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                System.Diagnostics.Debug.WriteLine("*** start generating view ChangesOfMetersViewModel");
                DateTime start = DateTime.Now;

                _view = new TMP.UI.Controls.WPF.PagingCollectionView(_data.ToList());
                _view.SortDescriptions.Add(new SortDescription("Дата_замены", ListSortDirection.Ascending));
                _view.SortDescriptions.Add(new SortDescription("Номер_акта", ListSortDirection.Ascending));
                _view.SortDescriptions.Add(new SortDescription("Населённый_пункт", ListSortDirection.Ascending));
                _view.SortDescriptions.Add(new SortDescription("ФИО", ListSortDirection.Ascending));
                _view.Filter = Filter;

                double ms = (DateTime.Now - start).TotalMilliseconds;
                System.Diagnostics.Debug.WriteLine("*** generated view ChangesOfMetersViewModel = " + ms.ToString("N1"));

                RaisePropertyChanged("View");
                IsBusy = false;
            }));
        }

        private bool Filter(object obj)
        {
            if (FromDate.HasValue && ToDate.HasValue)
            {
                if (obj is ChangesOfMeters item)
                {
                    return item.Дата_замены >= FromDate && item.Дата_замены <= ToDate;
                }
                else
                    return false;
            }
            return false;
        }

        private void OnDateChanged()
        {
            if (View != null)
                View.Refresh();
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

        private FlowDocument GenerateFlowDocumentFromPrint()
        {
            DataTable dataTable = View.ToDataTable<Model.ChangesOfMeters>(GetFieldsAndFormats(), ModelHelper.ChangesOfMetersGetPropertyValue);

            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(System.Printing.PageMediaSizeName.ISOA4);
            printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

            FlowDocument doc = new FlowDocument();
            doc.PageHeight = printDialog.PrintableAreaHeight;
            doc.PageWidth = printDialog.PrintableAreaWidth;

            doc.FontFamily = new FontFamily("Verdana;Tahoma");
            doc.FontSize = 10d;
            doc.ColumnWidth = Double.PositiveInfinity;

            Paragraph p = new Paragraph(new Run(String.Format("Сведения по заменам счётчиков за период с {0} по {1}", FromDate.Value.ToShortDateString(), ToDate.Value.ToShortDateString())))
                { FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
            doc.Blocks.Add(p);

            Table table = new Table();
            table.CellSpacing = 0;
            table.BorderBrush = new SolidColorBrush(Colors.Black);
            table.BorderThickness = new Thickness(1);

            foreach (var c in dataTable.Columns)
                table.Columns.Add(new TableColumn());
            int columnsCount = dataTable.Columns.Count;
            foreach (System.Data.DataRow dataRow in dataTable.Rows)
            {
               App.DoEvents();
                TableRowGroup rg = new TableRowGroup();
                TableRow row = new TableRow();

                foreach (DataColumn column in dataTable.Columns)
                {
                    App.DoEvents();
                    TableCell cell = new TableCell(new Paragraph(new Run(dataRow[column].ToString())))
                    { BorderBrush = new SolidColorBrush(Colors.Gray), BorderThickness = new Thickness(0.5) };
                    row.Cells.Add(cell);
                }
                rg.Rows.Add(row);

                row = new TableRow();
                TableCell line = new TableCell() { Padding = new Thickness(0), BorderBrush = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(0, 0, 0, 1), ColumnSpan = columnsCount };
                row.Cells.Add(line);
                rg.Rows.Add(row);

                table.RowGroups.Add(rg);
            }

            doc.Blocks.Add(table);

            return doc;
        }

        private void Export()
        {
            View.Export<Model.ChangesOfMeters>(
                GetFieldsAndFormats(),
                String.Format("Сведения по заменам счётчиков за период с {0} по {1}", FromDate.Value.ToShortDateString(), ToDate.Value.ToShortDateString()),
                ModelHelper.ChangesOfMetersGetPropertyValue);
        }

        #endregion
    }
}
