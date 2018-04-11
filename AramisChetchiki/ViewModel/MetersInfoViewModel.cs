using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using TMP.Extensions;
    using TMP.UI.Controls.WPF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.UI.Controls.WPF.Reporting.Helpers;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    using System.Collections.ObjectModel;

    public class MetersInfoViewModel : BaseViewModel
    {
        private Data _data;

        ICollection<IMatrix> _pivotCollection;

        #region Constructors

        public MetersInfoViewModel() : base(null)
        {
            this.CommandUpdate = new DelegateCommand(delegate
               {
                   this.PrepareData();
               }, () => this._data != null, "Обновление");

            CommandPrint = new DelegateCommand(delegate
            {
                System.Windows.Documents.Section rootSection = new System.Windows.Documents.Section();
                rootSection.Blocks.Add(new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run(DateTime.Now.ToLongDateString()) { FontStyle = FontStyles.Italic }
                    )
                { TextAlignment = TextAlignment.Right }
                );
                rootSection.Blocks.Add(new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run(_data.Departament.Name) { FontWeight = FontWeights.Bold }
                    )
                { TextAlignment = TextAlignment.Center }
                );

                foreach (var pivot in PivotCollection)
                {
                    var section = FormatHelper.MatrixToFlowDocumentSectionWithTable(pivot);
                    rootSection.Blocks.Add(section);
                }


                var flowDocument = new System.Windows.Documents.FlowDocument(rootSection);
                flowDocument.FontSize = 11d;

                System.Windows.Controls.PrintDialog pd = new System.Windows.Controls.PrintDialog();
                System.Printing.PrintTicket pt = new System.Printing.PrintTicket();
                pt.PageOrientation = System.Printing.PageOrientation.Landscape;
                pd.PrintTicket = pd.PrintQueue.MergeAndValidatePrintTicket(pd.PrintQueue.DefaultPrintTicket, pt).ValidatedPrintTicket;

                System.Windows.Documents.IDocumentPaginatorSource fdd = flowDocument;
                flowDocument.PageWidth = pd.PrintableAreaWidth;
                flowDocument.PageHeight = pd.PrintableAreaHeight;
                flowDocument.ColumnWidth = pd.PrintableAreaWidth;
                flowDocument.PagePadding = new Thickness(30.0, 50.0, 20.0, 30.0);
                flowDocument.IsOptimalParagraphEnabled = true;
                flowDocument.IsHyphenationEnabled = true;

                var ms = new System.IO.MemoryStream();
                using (var pkg = System.IO.Packaging.Package.Open(ms, System.IO.FileMode.Create))
                {
                    using (System.Windows.Xps.Packaging.XpsDocument doc = new System.Windows.Xps.Packaging.XpsDocument(pkg))
                    {
                        System.Windows.Xps.XpsDocumentWriter writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(doc);
                        writer.Write(fdd.DocumentPaginator);
                    }
                }
                ms.Position = 0;
                var pkg2 = System.IO.Packaging.Package.Open(ms);
                // Read the XPS document into a dynamically generated
                // preview Window 
                var url = new Uri("memorystream://printstream");
                System.IO.Packaging.PackageStore.AddPackage(url, pkg2);
                try
                {
                    using (System.Windows.Xps.Packaging.XpsDocument doc = new System.Windows.Xps.Packaging.XpsDocument(pkg2, System.IO.Packaging.CompressionOption.SuperFast, url.AbsoluteUri))
                    {
                        System.Windows.Documents.FixedDocumentSequence fds = doc.GetFixedDocumentSequence();

                        Window wnd = new Window();
                        wnd.Title = "Предварительный просмотр: сводная информация по счётчикам";
                        wnd.Owner = Application.Current.MainWindow;
                        System.Windows.Media.TextOptions.SetTextFormattingMode(wnd, System.Windows.Media.TextFormattingMode.Display);
                        wnd.Padding = new Thickness(2);
                        wnd.Content = new System.Windows.Controls.DocumentViewer()
                        {
                            Document = fds as System.Windows.Documents.IDocumentPaginatorSource
                        };
                        wnd.ShowDialog();
                    }
                }
                finally
                {
                    System.IO.Packaging.PackageStore.RemovePackage(url);
                }

            }, () => _data != null, "Печать");
        }

        public MetersInfoViewModel(Data data) : this()
        {
            bool flag = data == null;
            bool flag2 = flag;
            if (flag2)
            {
                throw new ArgumentNullException("Data is null!");
            }
            this._data = data;
            this.PrepareData();
        }

        #endregion

        #region Propperties

        public ICommand CommandUpdate { get; private set; }
        public ICommand CommandPrint { get; private set; }

        public ICollection<IMatrix> PivotCollection
        {
            get
            {
                return this._pivotCollection;
            }
            private set
            {
                this._pivotCollection = value;
                base.RaisePropertyChanged("PivotCollection");
            }
        }

        #endregion

        #region Private Methods

        private void PrepareData()
        {
            IsBusy = true;
            base.Status = "Подготовка данных";
            base.DetailedStatus = "подготовка ...";
            Task task = Task.Factory.StartNew(delegate
            {
                if (!(this._data == null || this._data.Meters == null || this._data.Meters.Count == 0))
                {
                    _pivotCollection = new List<IMatrix>();


                    int curYear = DateTime.Now.Year;
                    const int yearsCount = 3;
                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по установке или замене счётчиков за последние три года помесячно",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => Enumerable.Range(curYear - yearsCount + 1, yearsCount).Reverse().Select(i => MatrixHeaderCell.CreateRowHeader(i.ToString())),
                        GetColumnHeaderValuesFunc = () => System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12)
                            .Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var l = this._data.ChangesOfMeters
                                .Where(i => i.Дата_замены.HasValue)
                                .Select(i => i.Дата_замены.Value)
                                .ToList();
                            var l1 = l
                                .Where(i => i.Year.ToString() == row.Header)
                                .ToList();

                            var values = l1
                                .Where(i => string.Equals(i.ToString("MMMM"), column.Header))
                                .ToList();
                            if (values == null || values.Count() == 0)
                                return new MatrixDataCell(string.Empty);
                            else
                                return new MatrixDataCell(values.Count());
                        }
                    });

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по установке или замене на электронный счётчик за последние три года помесячно",
                        Description = "* количество электронных счётчиков",
                        GetRowHeaderValuesFunc = () => Enumerable.Range(curYear - yearsCount + 1, yearsCount).Reverse().Select(i => MatrixHeaderCell.CreateRowHeader(i.ToString())),
                        GetColumnHeaderValuesFunc = () => System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12)
                            .Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var l = this._data.ChangesOfMeters
                                .Where(i => i.Дата_замены.HasValue)
                                .Where(i => i.ЭтоЭлектронный)
                                .Select(i => i.Дата_замены.Value)
                                .ToList();
                            var l1 = l
                                .Where(i => i.Year.ToString() == row.Header)
                                .ToList();

                            var values = l1
                                .Where(i => string.Equals(i.ToString("MMMM"), column.Header))
                                .ToList();
                            if (values == null || values.Count() == 0)
                                return new MatrixDataCell(string.Empty);
                            else
                                return new MatrixDataCell(values.Count());
                        }
                    });


                    int metersCount = this._data.Meters.Count;

                    IEnumerable<SummaryInfoGroupItem> allMeterTypes = SummaryInfoHelper.BuildFirst10LargeGroups(this._data.Meters, "Тип_счетчика");
                    IEnumerable<SummaryInfoGroupItem> electronicMeterTypes = SummaryInfoHelper.BuildFirst10LargeGroups(this._data.Meters.Where(meter => meter.Принцип == "Э").ToList(), "Тип_счетчика");

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по типам счётчиков",
                        Description = "* количество счётчиков",
                        ShowRowsTotal = false,
                        GetRowHeaderValuesFunc = () => allMeterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => new string[] { "Количество", "%" }.Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = allMeterTypes.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            if (column.Header == "%")
                                return new MatrixDataCell(group.Percent);
                            else
                                return new MatrixDataCell(group.Count);
                        }
                    });

                    // Свод по типу счётчика и году поверки
                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по типу счётчика и году поверки",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => allMeterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Год_поверки_для_отчётов).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = allMeterTypes.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            var list = group.Value.Where(i => i.Год_поверки_для_отчётов == column.Header).ToList();
                            if (list != null)
                                return new MatrixDataCell(list.Count());
                            else
                                return new MatrixDataCell(0);
                        }
                    });

                    // Свод по типу электронного счётчика и году поверки
                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по типам электронных счётчиков и году поверки",
                        Description = "* количество электронных счётчиков",
                        GetRowHeaderValuesFunc = () => electronicMeterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Год_поверки_для_отчётов).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = electronicMeterTypes.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            var list = group.Value.Where(i => i.Год_поверки_для_отчётов == column.Header).ToList();
                            if (list != null)
                                return new MatrixDataCell(list.Count());
                            else
                                return new MatrixDataCell(0);
                        }
                    });

                    // Свод по категории счётчика и типу населённого пункта
                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по категории счётчика и типу населённого пункта",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => this._data.Meters.Select(i => i.Группа_счётчика_для_отчётов).Distinct().Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Тип_населённого_пункта).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var list = this._data.Meters
                                .Where(i => i.Группа_счётчика_для_отчётов == row.Header)
                                .Where(i => i.Тип_населённого_пункта == column.Header)
                                .ToList();
                            if (list != null)
                            {
                                int count = list.Count();
                                return new MatrixDataCell(count) { ToolTip = String.Format("{0:N1}%", 100 * count / metersCount) };
                            }
                            else
                                return new MatrixDataCell(0);
                        }
                    });

                    // Свод по населенному пункту и принципу действия счётчика
                    IEnumerable<SummaryInfoGroupItem> meterPerLocality = SummaryInfoHelper.BuildFirst10LargeGroups(this._data.Meters, "Населённый_пункт");

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по населенному пункту и принципу действия счётчика",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => meterPerLocality.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Принцип).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = meterPerLocality.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            var list = group.Value.Where(i => i.Принцип == column.Header).ToList();
                            if (list != null)
                            {
                                int count = list.Count();
                                return new MatrixDataCell(count) { ToolTip = String.Format("{0:N1}%", 100 * count / metersCount) };
                            }
                            else
                                return new MatrixDataCell(0);
                        }
                    });

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по населенному пункту и количеству просроченных",
                        Description = "* количество счётчиков",
                        ShowRowsTotal = false,
                        GetRowHeaderValuesFunc = () => meterPerLocality.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => new string[] { "Количество\nнеповеренных", "% от количества в н.п." }.Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = meterPerLocality.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            int countPerLoacality = group.Value.Count;
                            var list = group.Value.Where(i => i.Поверен == false).ToList();
                            if (list == null)
                                return new MatrixDataCell(0);

                            int count = list.Count();
                            if (column.Header == "% от количества в н.п.")
                            {
                                return new MatrixDataCell(String.Format("{0:N1}%", 100 * count / countPerLoacality));
                            }
                            else
                            {
                                
                                return new MatrixDataCell(count) { ToolTip = String.Format("{0:N1}% от общего количества счётчиков", 100 * count / metersCount) };
                            }
                        }
                    });

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по подстанции и принципу действия",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => this._data.Meters.Select(i => i.Подстанция).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                        GetColumnHeaderValuesFunc = () => this._data.Meters
                            .Select(i => i.Принцип).Distinct()
                            .Select(i => MatrixHeaderCell.CreateHeaderCell(i, this._data.Meters.Select(ii => ii.Фаз).Distinct().OrderBy(ii => ii).Select(ii => MatrixHeaderCell.CreateHeaderCell(ii.ToString())).ToList<IMatrixHeader>())),
                        GetDataCellFunc = (row, column) =>
                        {
                            byte фаз = 0;
                            byte.TryParse(column.Header, out фаз);

                            var list = this._data.Meters
                                .Where(i => i.Подстанция == row.Header)
                                .Where(i => i.Принцип == column.Parent.Header)
                                .Where(i => i.Фаз == фаз)
                                .ToList();
                            if (list == null)
                                return new MatrixDataCell(0);
                            return new MatrixDataCell(list.Count);
                        }
                    });
                }
            });
            task.ContinueWith(delegate (Task t)
            {
                base.IsBusy = false;
                base.Status = null;
                base.DetailedStatus = null;
                base.RaisePropertyChanged("PivotCollection");
            });
            task.ContinueWith(delegate (Task t)
            {
                MessageBox.Show(
                    string.Format(
                        "Произошла ошибка при подготовке данных.\nОписание: {0}", 
                        App.GetExceptionDetails(t.Exception)), 
                    "Ошибка", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Hand);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion

    }
}
