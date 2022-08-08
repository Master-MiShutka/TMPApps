namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using NetOffice.ExcelApi.Enums;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;
    using Excel = NetOffice.ExcelApi;

    /// <summary>
    /// Модель представления для сводной информации
    /// </summary>
    public class SummaryInfoViewViewModel : BaseDataViewModel<SummaryInfoItem>
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool showOnly10Groups = true;
        private InfoViewType selectedInfoViewType = AppSettings.Default.SelectedSummaryView;
        private const string SUMMARYINFOREPORTHEADER = "Сводная информация к списку счетчиков";
        private const string HEADERFORWINDOWSHOWMETERS = "Список";

        public SummaryInfoViewViewModel()
        {
            this.CommandChangeViewKind = new DelegateCommand<Model.InfoViewType>(
                kind =>
                {
                    this.SelectedInfoViewType = kind;
                }, o => this.View != null);

            this.CommandShowMeters = new DelegateCommand<string>(
                field =>
            {
                MainViewModel.ShowMetersWithGroupingAtField(field);
            }, (o) => MainViewModel != null);

            this.CommandToShowList = new DelegateCommand<object>(
                obj =>
            {
                if (obj is string[] parameters && parameters.Length == 2)
                {
                    MainViewModel.ShowMeterFilteredByFieldValue(parameters[0], parameters[1]);
                }
            }, (o) => MainViewModel != null);

            this.CommandExport = new DelegateCommand(
                () =>
            {
                this.ShowDialogInfo("Ещё не реализовано..");
            }, () => this.View != null);
            this.CommandPrint = new DelegateCommand(
                () =>
            {
                this.ShowDialogInfo("Ещё не реализовано.");
            }, () => this.View != null);

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            this.Data = MainViewModel?.Data?.SummaryInfos;
        }

        #region Properties

        /// <summary>
        /// Признак, указывающий, что данные загружены
        /// </summary>
        public override bool IsDataLoaded => this.Data != null;

        /// <summary>
        /// Признак, указвающий, что необходимо отображать только первые 10 групп по убыванию количества счётчиков
        /// </summary>
        public bool ShowOnly10Groups
        {
            get => this.showOnly10Groups;
            set => this.SetProperty(ref this.showOnly10Groups, value);
        }

        /// <summary>
        /// Выбранный вид отображения
        /// </summary>
        public InfoViewType SelectedInfoViewType
        {
            get => this.selectedInfoViewType;
            set => this.SetProperty(ref this.selectedInfoViewType, value);
        }

        #region Commands

        public ICommand CommandShowMeters { get; }

        public ICommand CommandToShowList { get; }

        #endregion

        #endregion

        #region Private methods

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();

            this.RaisePropertyChanged(nameof(this.Data));
            this.RaisePropertyChanged(nameof(this.IsDataLoaded));
            this.RaisePropertyChanged(nameof(this.View));
        }

        protected override ICollectionView BuildAndGetView()
        {
            if (AppSettings.Default.SummaryInfoFields != null)
            {
                DateTime start = DateTime.Now;

                IEnumerable<Shared.PlusPropertyDescriptor> fields = ModelHelper.GetFields(ModelHelper.MeterSummaryInfoPropertiesCollection, AppSettings.Default.SummaryInfoFields);

                if (fields == null)
                {
                    return null;
                }

                IList<SummaryInfoItem> list = new List<SummaryInfoItem>();
                foreach (Shared.PlusPropertyDescriptor f in fields)
                {
                    SummaryInfoItem si = this.Data.FirstOrDefault(i => i?.FieldName == f.Name);
                    if (si != null)
                    {
                        list.Add(si);
                    }
                }

                ICollectionView view = CollectionViewSource.GetDefaultView(list);
                view.Filter = this.Filter;

                double ms = (DateTime.Now - start).TotalMilliseconds;

                System.Diagnostics.Debug.WriteLine("*** generate view = " + ms.ToString("N1", AppSettings.CurrentCulture));

                return view;
            }
            else
            {
                return null;
            }
        }

        protected override bool Filter(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this.Data == null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(this.TextToFind) == false)
            {
                string s = this.TextToFind.ToLower(AppSettings.CurrentCulture);
                SummaryInfoItem item = obj as SummaryInfoItem;
                return item != null && item.FieldName.ToLower(AppSettings.CurrentCulture).Contains(s, AppSettings.StringComparisonMethod)
                    || item.Header.ToLower(AppSettings.CurrentCulture).Contains(s, AppSettings.StringComparisonMethod);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Печать сводной информации
        /// </summary>
        private void PrintSummaryInfo()
        {
            FlowDocument doc = new()
            {
                FontFamily = new FontFamily("Verdana;Tahoma"),
                ColumnWidth = double.PositiveInfinity,
            };

            Paragraph p = new(new Run(SUMMARYINFOREPORTHEADER)) { FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
            doc.Blocks.Add(p);

            IEnumerable<Model.SummaryInfoChildItem> childs;

            if (this.ShowOnly10Groups)
            {
                doc.Blocks.Add(new Paragraph(new Run("* показаны только первые 10 групп, по убыванию количества счётчиков")) { FontSize = 12, FontStyle = FontStyles.Italic });
            }

            Table table = new()
            {
                CellSpacing = 0,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
            };

            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());

            foreach (SummaryInfoItem si in this.Data)
            {
                childs = this.ShowOnly10Groups ? si.OnlyFirst10Items : si.AllItems;

                TableRowGroup rg = new();
                TableRow row = new();

                TableCell cellHeader = new(new Paragraph(new Run(si.Header) { FontWeight = FontWeights.Bold }));

                // TableCell cellInfo = new TableCell(new Paragraph(new Run(si.Info) { FontStyle = FontStyles.Italic }) { TextAlignment = TextAlignment.Right });
                row.Cells.Add(cellHeader);

                // row.Cells.Add(cellInfo);
                rg.Rows.Add(row);

                row = new TableRow();

                int index = 1;
                foreach (SummaryInfoChildItem item in childs)
                {
                    TableCell cell = new(new Paragraph(new Run(item.Header)))
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
                {
                    rg.Rows.Add(row);
                }

                row = new TableRow();
                TableCell line = new() { Padding = new Thickness(0), BorderBrush = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(0, 0, 0, 1), ColumnSpan = 2 };
                row.Cells.Add(line);
                rg.Rows.Add(row);

                table.RowGroups.Add(rg);
            }

            doc.Blocks.Add(table);

            PrintPreviewWindow window = new PrintPreviewWindow(doc)
            {
                Owner = App.Current.MainWindow,
            };
            window.ShowDialog();
        }

        /// <summary>
        /// Экспорт сводной информации
        /// </summary>
        private void ExportSummaryInfo()
        {
            if (this.Data == null)
            {
                return;
            }

            string fileName = System.IO.Path.GetTempFileName();
            fileName = System.IO.Path.ChangeExtension(fileName, "xls");

            Excel.Application excelApplication = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;
            try
            {
                excelApplication = new Excel.Application
                {
                    DisplayAlerts = false,
                };
                using Excel.Tools.Contribution.CommonUtils utils = new(excelApplication);

                xlWorkbook = excelApplication.Workbooks.Add();
                xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];

                excelApplication.ScreenUpdating = false;

                int numberOfColumns = 10;

                Excel.Range header = xlWorksheet.Range("A1");
                header.Value = SUMMARYINFOREPORTHEADER;
                header.Resize(1, numberOfColumns).Merge();
                using (Excel.Font font = header.Font)
                {
                    font.Size = 14;
                    font.Bold = true;
                }

                header.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                // подсчёт количества строк
                int totalRows = 0;
                foreach (SummaryInfoItem si in this.Data)
                {
                    totalRows += 1 + (int)Math.Ceiling(this.ShowOnly10Groups ? si.OnlyFirst10Items.Count / 2d : si.AllItems.Count / 2d);
                }

                object[][] outputArray = new object[totalRows][];
                for (int i = 0; i < totalRows; i++)
                {
                    outputArray[i] = new object[numberOfColumns];
                }

                int row = 0;
                IEnumerable<Model.SummaryInfoChildItem> childs;
                foreach (SummaryInfoItem si in this.Data)
                {
                    childs = this.ShowOnly10Groups ? si.OnlyFirst10Items : si.AllItems;

                    outputArray[row][0] = si.Header;
                    outputArray[row++][numberOfColumns - 1] = si.Info;

                    // делим на 2 колонки, т.е. максимум 5 строк
                    int index = 1;
                    foreach (SummaryInfoChildItem item in childs)
                    {
                        if (index % 2 == 0)
                        {
                            outputArray[row++][numberOfColumns / 2] = item.Header;
                        }
                        else
                        {
                            outputArray[row][0] = item.Header;
                        }

                        index++;
                    }

                    if (index % 2 != 0)
                    {
                        row++;
                    }
                }

                row = 2;
                int rows = 0;
                int startRow = 0;
                foreach (SummaryInfoItem si in this.Data)
                {
                    xlWorksheet.Range($"A{row}").Font.Bold = true;
                    xlWorksheet.Range($"J{row}").Font.Italic = true;
                    xlWorksheet.Range($"J{row}").HorizontalAlignment = XlHAlign.xlHAlignRight;
                    row++;
                    startRow = row;
                    rows = (int)Math.Ceiling(this.ShowOnly10Groups ? si.OnlyFirst10Items.Count / 2d : si.AllItems.Count / 2d);
                    Excel.Range range;
                    for (int rowInd = 0; rowInd < rows; rowInd++)
                    {
                        range = xlWorksheet.Range($"A{row}:E{row}");
                        range.Resize(1, 5).Merge();
                        range.WrapText = true;
                        range = xlWorksheet.Range($"F{row}:J{row}");
                        range.Resize(1, 5).Merge();
                        range.WrapText = true;
                        row++;
                    }

                    range = xlWorksheet.Range($"A{startRow}:J{startRow + rows - 1}");
                    range.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin);
                    range.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlDot;
                    range.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlDot;

                    xlWorksheet.Range($"A{startRow - 1}:J{startRow + rows - 1}").BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium);
                }

                Excel.Range data = xlWorksheet.Range("A2").Resize(row - 2, numberOfColumns);
                data.Value = outputArray;
                data.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium);

                // data.Borders[XlBordersIndex.xl].LineStyle = XlLineStyle.xlDouble;

                // xlWorksheet.Columns[2].ColumnWidth = 25;
                Excel.PageSetup ps = xlWorksheet.PageSetup;
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

                this.logger?.Info($"Export >> файл сформирован и сохранен: '{fileName}'");
            }
            catch (Exception ex)
            {
#if DEBUG
                App.ToDebug(ex);
#endif
                this.ShowDialogError("Произошла ошибка:\n" + App.GetExceptionDetails(ex));
            }
            finally
            {
                xlWorkbook.Dispose();
                xlWorksheet.Dispose();
                excelApplication.Dispose();
            }

            try
            {
                using System.Diagnostics.Process p = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(fileName)
                    {
                        UseShellExecute = true,
                    },
                };
                p.Start();

                // System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception e)
            {
#if DEBUG
                App.ToDebug(e);
#endif
                App.ShowError("Произошла ошибка при открытии файла:\n" + App.GetExceptionDetails(e));
            }
        }

        #endregion

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0461");
            return guid.GetHashCode();
        }
    }
}