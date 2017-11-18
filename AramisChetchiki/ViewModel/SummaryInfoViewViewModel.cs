using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;
using System.Collections.ObjectModel;

using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;

using TMP.UI.Controls.WPF;
using Exp = System.Linq.Expressions;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using Model;
    using Extensions;

    /// <summary>
    /// Модель представления для сводной информации
    /// </summary>
    public class SummaryInfoViewViewModel : BaseDataViewModel<SummaryInfoItem>, IDisposable
    {
        private IMainViewModel _mainViewModel;

        public SummaryInfoViewViewModel()
        {
            CommandShowMeters = new DelegateCommand<string>(field =>
            {
                _mainViewModel.ShowMetersWithGroupingAtField(field);
            }, () => _mainViewModel != null, "Список", "Показать все счётчики, сгруппированные по этому полю");

            CommandToShowList = new DelegateCommand<object>(obj =>
            {
                string[] parameters = obj as string[];
                if (parameters != null && parameters.Length == 2)
                {
                    _mainViewModel.ShowMeterFilteredByFieldValue(parameters[0], parameters[1]);
                }
            }, () => _mainViewModel != null, "Список", "Показать все счётчики, сгруппированные по этому полю");

            CommandExport = new DelegateCommand(() =>
            {
                ;
            }, () => View != null, "Экспорт");
            CommandPrint = new DelegateCommand(() =>
            {
                ;
            }, () => View != null, "Печать");

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Data = new List<SummaryInfoItem>(new SummaryInfoItem[] { new SummaryInfoItem() { Header = "Test" } });
            }
        }

        public SummaryInfoViewViewModel(IMainViewModel mainViewModel, ICollection<Model.SummaryInfoItem> infos) : this()
        {
            _mainViewModel = mainViewModel;
            Data = infos;
            (Properties.Settings.Default.SummaryInfoFields as ObservableCollection<Properties.TableField>).CollectionChanged += SummaryInfoViewViewModel_CollectionChanged;
        }

        private void SummaryInfoViewViewModel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (View != null)
                View.Refresh();
        }

        public void Dispose()
        {
            (Properties.Settings.Default.SummaryInfoFields as ObservableCollection<Properties.TableField>).CollectionChanged -= SummaryInfoViewViewModel_CollectionChanged;
        }

        #region Properties

        private ICollection<SummaryInfoItem> _data;
        /// <summary>
        /// Коллекция сводной информации
        /// </summary>
        public override ICollection<SummaryInfoItem> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                RaisePropertyChanged("Data");
                IsDataLoaded = _data != null;
                RaisePropertyChanged("View");
            }
        }

        private ICollectionView _view;
        public override ICollectionView View
        {
            get
            {
                if (_data == null)
                    return _view = null;
                if (_view == null && Properties.Settings.Default.SummaryInfoFields != null)
                {
                    DateTime start = DateTime.Now;

                    var fields = from f in Properties.Settings.Default.SummaryInfoFields
                                 where f.IsVisible
                                 orderby f.DisplayOrder
                                 select f.Name;
                    if (fields == null) return null;
                    IList<SummaryInfoItem> list = new List<SummaryInfoItem>();
                    foreach (var f in fields)
                    {
                        SummaryInfoItem si = _data.First(i => i.FieldName == f);
                        if (si != null)
                            list.Add(si);
                    }

                    _view = CollectionViewSource.GetDefaultView(list);
                    _view.Filter = Filter;

                    double ms = (DateTime.Now - start).TotalMilliseconds;

                    System.Diagnostics.Debug.WriteLine("*** generate view = " + ms.ToString("N1"));
                }
                return _view;
            }
            set { _view = value; RaisePropertyChanged("View"); }
        }

        private string _textToFind;
        public string TextToFind
        {
            get { return _textToFind; }
            set
            {
                _textToFind = value;
                RaisePropertyChanged("TextToFind");
                if (View != null)
                    View.Refresh();
            }
        }

        private bool _showOnly10Groups = true;
        /// <summary>
        /// Признак, указвающий, что необходимо отображать только первые 10 групп по убыванию количества счётчиков
        /// </summary>
        public bool ShowOnly10Groups
        {
            get { return _showOnly10Groups; }
            set
            {
                _showOnly10Groups = value;
                RaisePropertyChanged("ShowOnly10Groups");
            }
        }

        #region Commands

        public ICommand CommandShowMeters { get;}

        public ICommand CommandToShowList { get; }

        public override ICommand CommandExport { get; }
        public override ICommand CommandPrint { get; }

        #endregion

        #endregion

        #region Private methods

        private bool Filter(object obj)
        {
            if (Data == null) return true;
            if (String.IsNullOrWhiteSpace(TextToFind) == false)
            {
                string s = TextToFind.ToLower();
                SummaryInfoItem item = obj as SummaryInfoItem;
                if (item != null && item.FieldName.ToLower().Contains(s) || item.Header.ToLower().Contains(s))
                    return true;
                return false;
            }
            else
                return true;
        }

        private const string _summaryInfoReportsHeader = "Сводная информация к списку счетчиков";

        /// <summary>
        /// Печать сводной информации
        /// </summary>
        private void PrintSummaryInfo()
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Verdana;Tahoma");
            doc.ColumnWidth = Double.PositiveInfinity;

            Paragraph p = new Paragraph(new Run(_summaryInfoReportsHeader)) { FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
            doc.Blocks.Add(p);

            IEnumerable<Model.SummaryInfoChildItem> childs;

            if (ShowOnly10Groups)
            {
                doc.Blocks.Add(new Paragraph(new Run("* показаны только первые 10 групп, по убыванию количества счётчиков")) { FontSize = 12, FontStyle = FontStyles.Italic });
            }

            Table table = new Table();
            table.CellSpacing = 0;
            table.BorderBrush = new SolidColorBrush(Colors.Black);
            table.BorderThickness = new Thickness(2);

            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());

            foreach (SummaryInfoItem si in Data)
            {
                if (ShowOnly10Groups)
                    childs = si.OnlyFirst10Items;
                else
                    childs = si.AllItems;

                TableRowGroup rg = new TableRowGroup();
                TableRow row = new TableRow();

                TableCell cellHeader = new TableCell(new Paragraph(new Run(si.Header) { FontWeight = FontWeights.Bold }));
                //TableCell cellInfo = new TableCell(new Paragraph(new Run(si.Info) { FontStyle = FontStyles.Italic }) { TextAlignment = TextAlignment.Right });
                row.Cells.Add(cellHeader);
                //row.Cells.Add(cellInfo);
                rg.Rows.Add(row);

                row = new TableRow();

                int index = 1;
                foreach (SummaryInfoChildItem item in childs)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(item.Header)))
                    { BorderBrush = new SolidColorBrush(Colors.Gray), BorderThickness = new Thickness(0.5) };
                    row.Cells.Add(cell);

                    if (index % 2 == 0)
                    {
                        rg.Rows.Add(row);
                        row = new TableRow();
                    }

                    index++;
                }
                if (index % 2 != 0)
                    rg.Rows.Add(row);

                row = new TableRow();
                TableCell line = new TableCell() { Padding = new Thickness(0), BorderBrush = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(0, 0, 0, 1), ColumnSpan = 2 };
                row.Cells.Add(line);
                rg.Rows.Add(row);

                table.RowGroups.Add(rg);
            }

            doc.Blocks.Add(table);

            var window = new PrintPreviewWindow(doc);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
        }
        /// <summary>
        /// Экспорт сводной информации
        /// </summary>
        private void ExportSummaryInfo()
        {
            if (Data == null) return;

            string fileName = System.IO.Path.GetTempFileName();
            fileName = System.IO.Path.ChangeExtension(fileName, "xls");

            Excel.Application excelApplication = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;
            try
            {
                excelApplication = new Excel.Application();
                excelApplication.DisplayAlerts = false;
                Excel.Tools.CommonUtils utils = new Excel.Tools.CommonUtils(excelApplication);

                xlWorkbook = excelApplication.Workbooks.Add();
                xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];

                excelApplication.ScreenUpdating = false;

                int numberOfColumns = 10;

                Excel.Range header = xlWorksheet.Range("A1");
                header.Value = _summaryInfoReportsHeader;
                header.Resize(1, numberOfColumns).Merge();
                using (Excel.Font font = header.Font)
                {
                    font.Size = 14;
                    font.Bold = true;
                }
                header.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                // подсчёт количества строк
                int totalRows = 0;
                foreach (SummaryInfoItem si in Data)
                {
                    totalRows += 1 + (int)Math.Ceiling(ShowOnly10Groups ? si.OnlyFirst10Items.Count / 2d : si.AllItems.Count / 2d);
                }
                object[,] outputArray = new object[totalRows, numberOfColumns];

                int row = 0;
                IEnumerable<Model.SummaryInfoChildItem> childs;
                foreach (SummaryInfoItem si in Data)
                {
                    if (ShowOnly10Groups)
                        childs = si.OnlyFirst10Items;
                    else
                        childs = si.AllItems;

                    outputArray[row, 0] = si.Header;
                    outputArray[row++, numberOfColumns - 1] = si.Info;
                    // делим на 2 колонки, т.е. максимум 5 строк
                    int index = 1;
                    foreach (SummaryInfoChildItem item in childs)
                    {
                        if (index % 2 == 0)
                            outputArray[row++, numberOfColumns / 2] = item.Header;
                        else
                            outputArray[row, 0] = item.Header;
                        index++;
                    }
                    if (index % 2 != 0)
                        row++;
                }

                row = 2;
                int rows = 0;
                int startRow = 0;
                foreach (SummaryInfoItem si in Data)
                {
                    xlWorksheet.Range(String.Format("A" + row.ToString())).Font.Bold = true;
                    xlWorksheet.Range(String.Format("J" + row.ToString())).Font.Italic = true;
                    xlWorksheet.Range(String.Format("J" + row.ToString())).HorizontalAlignment = XlHAlign.xlHAlignRight;
                    row++;
                    startRow = row;
                    rows = (int)Math.Ceiling(ShowOnly10Groups ? si.OnlyFirst10Items.Count / 2d : si.AllItems.Count / 2d);
                    Excel.Range range;
                    for (int rowInd = 0; rowInd < rows; rowInd++)
                    {
                        range = xlWorksheet.Range(String.Format("A{0}:E{0}", row));
                        range.Resize(1, 5).Merge();
                        range.WrapText = true;
                        range = xlWorksheet.Range(String.Format("F{0}:J{0}", row));
                        range.Resize(1, 5).Merge();
                        range.WrapText = true;
                        row++;
                    }

                    range = xlWorksheet.Range(String.Format("A{0}:J{1}", startRow, startRow + rows - 1));
                    range.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin);
                    range.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlDot;
                    range.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlDot;

                    xlWorksheet.Range(String.Format("A{0}:J{1}", startRow - 1, startRow + rows - 1)).BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium);
                }

                Excel.Range data = xlWorksheet.Range("A2").Resize(row - 2, numberOfColumns);
                data.Value = outputArray;
                data.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium);
                //data.Borders[XlBordersIndex.xl].LineStyle = XlLineStyle.xlDouble;

                //xlWorksheet.Columns[2].ColumnWidth = 25;

                var ps = xlWorksheet.PageSetup;
                ps.PaperSize = XlPaperSize.xlPaperA4;
                ps.Orientation = XlPageOrientation.xlPortrait;
                ps.Zoom = false;
                ps.FitToPagesWide = 1;
                ps.FitToPagesTall = false;

                ps.LeftMargin = excelApplication.CentimetersToPoints(2.0);
                ps.RightMargin = excelApplication.CentimetersToPoints(1.0);
                ps.TopMargin = excelApplication.CentimetersToPoints(1.0);
                ps.BottomMargin = excelApplication.CentimetersToPoints(1.0);

                ps.HeaderMargin = excelApplication.CentimetersToPoints(0.6);
                ps.FooterMargin = excelApplication.CentimetersToPoints(0.6);

                ps.CenterHorizontally = true;
                ps.RightHeader = DateTime.Now.ToLongDateString();
                ps.RightFooter = "Страница &P / &N";
                ps.PrintArea = header.get_Resize(totalRows + 1, numberOfColumns).Address;

                xlWorkbook.SaveAs(fileName);
                xlWorkbook.Close(false);

                excelApplication.ScreenUpdating = true;
                excelApplication.Quit();

                System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception ex)
            {
#if DEBUG
                App.ToDebug(ex);
#endif
                MessageBox.Show("Произошла ошибка:\n" + App.GetExceptionDetails(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                xlWorkbook.Dispose();
                xlWorksheet.Dispose();
                excelApplication.Dispose();
            }
        }

        #endregion
    }
}
