namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using TMP.Shared.Commands;

    /// <summary>
    /// Базовый класс для отображения данных в виде матрицы в MatrixControl
    /// </summary>
    public abstract class MatrixBase : TMP.Shared.PropertyChangedBase, IMatrix, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Коллекция ячеек матрицы
        /// </summary>
        private IList<IMatrixCell> items;
        private IMatrixCell[,] cells;
        private Size size = Size.Empty;

        private string header;
        private string description;

        /// <summary>
        /// Ширина заголовков строк и столбцов
        /// </summary>
        private int columnHeadersRowSpan = 1;

        /// <summary>
        /// Ширина заголовков строк и столбцов
        /// </summary>
        private int rowHeadersColumnSpan = 1;

        private IList<IMatrixHeader> rowHeaders;
        private IList<IMatrixHeader> columnHeaders;

        private bool isBuilded = false;
        private bool isBuilding;
        private bool? showColumnsTotal;
        private bool? showRowsTotal;

        #endregion // Fields

        #region Constructor

        protected MatrixBase()
        {
            this.CommandCopyToClipboard = new DelegateCommand(
                this.CopyToClipboard,
                () =>
                {
                    return this.HasData;
                });

            this.CommandPrint = new DelegateCommand(
                () =>
                {
                    var section = Helpers.FormatHelper.MatrixToFlowDocumentSectionWithTable(this);
                    var flowDocument = new System.Windows.Documents.FlowDocument(section);
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
                            wnd.Title = string.Format("Предварительный просмотр :: {0}", this.Header);
                            wnd.Owner = Application.Current.MainWindow;
                            System.Windows.Media.TextOptions.SetTextFormattingMode(wnd, System.Windows.Media.TextFormattingMode.Display);
                            wnd.Padding = new Thickness(2);
                            wnd.Content = new System.Windows.Controls.DocumentViewer()
                            {
                                Document = fds,
                            };
                            wnd.ShowDialog();
                        }
                    }
                    finally
                    {
                        System.IO.Packaging.PackageStore.RemovePackage(url);
                    }
                },
                () =>
                {
                    return this.HasData;
                });

            this.CommandRefresh = new DelegateCommand(this.Build, () => this.HasData);
        }

        #endregion // Constructor

        #region Abstract Methods

        /// <summary>
        /// Наследуемые классы должны переопределить этот метод для возврата списка
        /// всех записей, которые будут отображены в заголовках столбцов
        /// </summary>
        protected abstract IEnumerable<IMatrixHeader> GetColumnHeaderValues();

        /// <summary>
        /// Наследуемые классы должны переопределить этот метод для возврата списка
        /// всех записей, которые будут отображены в заголовках строк
        /// </summary>
        protected abstract IEnumerable<IMatrixHeader> GetRowHeaderValues();

        /// <summary>
        /// Наследуемые классы должны переопределить этот метод для возврата
        /// каждой ячейки данных в матрице, где ячейка определяется пересечением
        /// строки и столбцы
        /// </summary>
        /// <param name="rowHeaderValue">Строка</param>
        /// <param name="columnHeaderValue">Столбец</param>
        protected abstract IMatrixDataCell GetDataCell(IMatrixHeader rowHeaderValue, IMatrixHeader columnHeaderValue);

        #endregion // Abstract Methods

        #region Properties

        /// <summary>
        /// Построена ли матрица
        /// </summary>
        public bool HasData => this.items != null;

        /// <summary>
        /// Возвращает доступную только для чтения коллекцию всех ячеек матрицы
        /// </summary>
        public IList<IMatrixCell> Items
        {
            get
            {
                if (this.items == null)
                {
                    this.Build();
                }

                return this.items;
            }

            protected set
            {
                if (this.SetProperty(ref this.items, value))
                {
                    this.size = new Size(this.items.Max(i => i.GridRow), this.items.Max(i => i.GridColumn));

                    this.cells = new IMatrixCell[(int)this.size.Height, (int)this.size.Width];
                    foreach (var cell in this.items)
                    {
                        this.cells[cell.GridRow, cell.GridColumn] = cell;
                    }

                    this.RaisePropertyChanged(nameof(this.Size));
                    this.RaisePropertyChanged(nameof(this.HasData));
                    this.RaisePropertyChanged(nameof(this.RowHeadersCount));
                    this.RaisePropertyChanged(nameof(this.ColumnHeadersCount));
                    this.RaisePropertyChanged(nameof(this.Cells));
                }
            }
        }

        /// <summary>
        /// Количество полных строк заголовка - количество строк с данными
        /// </summary>
        public int RowHeadersCount => this.items == null ? 0 : this.rowHeaders != null ? this.rowHeaders.Count : 0;

        /// <summary>
        /// Количество полных столбцов заголовка - количество столбцов с данными
        /// </summary>
        public int ColumnHeadersCount => this.items == null ? 0 : this.columnHeaders != null ? this.columnHeaders.Count : 0;

        /// <summary>
        /// Размер матрицы
        /// </summary>
        public Size Size => this.items == null ? Size.Empty : this.size;

        /// <summary>
        /// Двумерный массив ячеек матрицы
        /// </summary>
        public IMatrixCell[,] Cells { get => this.cells; }

        /// <summary>
        /// Заголовок матрицы
        /// </summary>
        public string Header { get => this.header; set => this.SetProperty(ref this.header, value); }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        /// <summary>
        /// Команда копирования данных в виде таблицы в буфер обмена
        /// </summary>
        public ICommand CommandCopyToClipboard { get; }

        /// <summary>
        /// Команда печати данных
        /// </summary>
        public ICommand CommandPrint { get; }

        /// <summary>
        /// Команда обновления данных
        /// </summary>
        public ICommand CommandRefresh { get; }

        /// <summary>
        /// Событие для оповещения о завершении построения матрицы
        /// </summary>
        public event PropertyChangedEventHandler Builded;

        /// <summary>
        /// Событие для оповещения о начале построения матрицы
        /// </summary>
        public event PropertyChangedEventHandler Building;

        /// <summary>
        /// Флаг, указывающий готова ли матрица к отображению
        /// </summary>
        public bool IsBuilded { get => this.isBuilded; private set => this.SetProperty(ref this.isBuilded, value); }

        public bool IsBuilding { get => this.isBuilding; private set => this.SetProperty(ref this.isBuilding, value); }

        /// <summary>
        /// Отображать ли итоги по столбцам
        /// </summary>
        public bool? ShowColumnsTotal { get => this.showColumnsTotal; set => this.SetProperty(ref this.showColumnsTotal, value); }

        /// <summary>
        /// Отображать ли итоги по строкам
        /// </summary>
        public bool? ShowRowsTotal { get => this.showRowsTotal; set => this.SetProperty(ref this.showRowsTotal, value); }

        #endregion // Properties

        #region Matrix Construction

        private void RaisePropertiesChanged()
        {
            this.RaisePropertyChanged(nameof(this.HasData));
            this.RaisePropertyChanged(nameof(this.Items));
            this.RaisePropertyChanged(nameof(this.Cells));
            this.RaisePropertyChanged(nameof(this.RowHeadersCount));
            this.RaisePropertyChanged(nameof(this.ColumnHeadersCount));
            this.RaisePropertyChanged(nameof(this.Size));
        }

        /// <summary>
        /// Построение матрицы
        /// </summary>
        protected void Build()
        {
            var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.items = this.GetMatrixCells();
            })
                .ContinueWith(t =>
                {
                    this.RaisePropertiesChanged();
                });
        }

        /// <summary>
        /// Возвращаетя список ячеек матрицы
        /// </summary>
        /// <returns></returns>
        private List<IMatrixCell> GetMatrixCells()
        {
            this.IsBuilding = true;
            this.Building?.Invoke(this, null);
            List<IMatrixCell> matrixItems = new List<IMatrixCell>();

            // Получение значений заголовков строк и столбцов
            List<IMatrixHeader> rowHeaderValues = this.GetRowHeaderValues().ToList();
            List<IMatrixHeader> columnHeaderValues = this.GetColumnHeaderValues().ToList();

            IMatrixHeader nul = null;
            this.SetParentForChildren(rowHeaderValues, ref nul);
            this.SetParentForChildren(columnHeaderValues, ref nul);

            // подсчёт высоты заголовков
            this.rowHeadersColumnSpan = this.MaxHeaderDepth(rowHeaderValues);
            this.columnHeadersRowSpan = this.MaxHeaderDepth(columnHeaderValues);

            this.rowHeaders = this.BuildFlatHeadersList(rowHeaderValues);
            this.columnHeaders = this.BuildFlatHeadersList(columnHeaderValues);
            this.RaisePropertyChanged(nameof(this.RowHeadersCount));
            this.RaisePropertyChanged(nameof(this.ColumnHeadersCount));

            // высота и ширина блока с данными
            int dataBlockHeight = this.rowHeaders.Count(),
                dataBlockWidth = this.columnHeaders.Count();

            int matrixHeight = this.columnHeadersRowSpan + dataBlockHeight;
            int matrixWidth = this.rowHeadersColumnSpan + dataBlockWidth;

            if (this.ShowColumnsTotal.GetValueOrDefault())
            {
                matrixHeight++;
            }

            if (this.ShowRowsTotal.GetValueOrDefault())
            {
                matrixWidth++;
            }

            this.size = new Size(matrixWidth, matrixHeight);
            this.cells = new IMatrixCell[(int)this.size.Height, (int)this.size.Width];
            this.RaisePropertyChanged(nameof(this.Size));
            this.RaisePropertyChanged(nameof(this.Cells));

            // проседура для добавления ячейки и в список и в массив
            Action<IMatrixCell> addCell = (cell) =>
            {
                matrixItems.Add(cell);
                this.cells[cell.GridRow, cell.GridColumn] = cell;
            };

            // учитывая высоту заголовков добавляем в верхний левый угол пустую ячейку
            this.CreateEmptyHeader(matrixItems);

            // добавление в матрицу заголовков
            this.CreateRowHeaders(matrixItems, rowHeaderValues);
            this.CreateColumnHeaders(matrixItems, columnHeaderValues);

            // добавление ячеек с данными
            this.CreateCells(matrixItems, rowHeaderValues, columnHeaderValues);

            Func<object, int> getInt = (value) =>
            {
                int result = 0;
                if (value == null)
                {
                    return result;
                }

                int.TryParse(value.ToString(), out result);
                return result;
            };

            if (this.ShowRowsTotal.GetValueOrDefault())
            {
                addCell(MatrixHeaderCell.CreateColumnSummaryHeader("Итого", row: 0, rowSpan: this.columnHeadersRowSpan, column: this.rowHeadersColumnSpan + this.columnHeaders.Count));

                int summ = 0;
                for (int rowIndex = this.columnHeadersRowSpan; rowIndex < this.rowHeaders.Count + this.columnHeadersRowSpan; rowIndex++)
                {
                    summ = matrixItems.
                    Where(i => i.GridRow == rowIndex)
                    .Where(i => i.GridColumn >= this.rowHeadersColumnSpan)
                    .Where(i => i.CellType == MatrixCellType.DataCell)
                    .Cast<IMatrixDataCell>()
                    .Sum(i => getInt(i.Value));

                    addCell(MatrixSummaryCell.CreateRowSummary(summ, row: rowIndex, column: this.rowHeadersColumnSpan + this.columnHeaders.Count));
                }
            }

            if (this.ShowColumnsTotal.GetValueOrDefault())
            {
                addCell(MatrixHeaderCell.CreateRowSummaryHeader("Итого", row: this.columnHeadersRowSpan + this.rowHeaders.Count, column: 0, columnSpan: this.rowHeadersColumnSpan));

                int summ = 0;
                for (int columnIndex = this.rowHeadersColumnSpan; columnIndex < this.columnHeaders.Count + this.rowHeadersColumnSpan; columnIndex++)
                {
                    summ = matrixItems.
                    Where(i => i.GridColumn == columnIndex)
                    .Where(i => i.GridRow >= this.columnHeadersRowSpan)
                    .Where(i => i.CellType == MatrixCellType.DataCell)
                    .Cast<IMatrixDataCell>()
                    .Sum(i => getInt(i.Value));

                    addCell(MatrixSummaryCell.CreateColumnSummary(summ, row: this.columnHeadersRowSpan + this.rowHeaders.Count, column: columnIndex));
                }
            }

            if (this.ShowRowsTotal.GetValueOrDefault() & this.ShowColumnsTotal.GetValueOrDefault())
            {
                int summ = matrixItems.
                Where(i => i.GridColumn == this.rowHeadersColumnSpan + this.columnHeaders.Count)
                .Where(i => (i.GridRow >= this.columnHeadersRowSpan) & (i.GridRow != (this.columnHeadersRowSpan + this.rowHeaders.Count)))
                .Where(i => i.CellType == MatrixCellType.SummaryCell)
                .Cast<IMatrixSummaryCell>()
                .Sum(i => i.ValueToInt());

                addCell(MatrixSummaryCell.CreateTotalSummary(summ, row: this.columnHeadersRowSpan + this.rowHeaders.Count, column: this.rowHeadersColumnSpan + this.columnHeaders.Count));
            }

            // оповещение о готовности
            this.IsBuilded = true;
            this.IsBuilding = false;
            this.Builded?.Invoke(this, null);

            return matrixItems;
        }

        private void CreateEmptyHeader(ICollection<IMatrixCell> matrixItems)
        {
            // Вставка пустой ячейки
            var cell = MatrixHeaderCell.CreateEmptyHeader(row: 0, column: 0, rowSpan: this.columnHeadersRowSpan, columnSpan: this.rowHeadersColumnSpan);
            matrixItems.Add(cell);
            this.cells[0, 0] = cell;
        }

        private enum HeaderType
        {
            row, column,
        }

        /// <summary>
        /// Добавление в матрицу заголовков столбцов
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="columnHeaderValues">Список заголовков столбцов</param>
        private void CreateColumnHeaders(ICollection<IMatrixCell> matrixItems, IList<IMatrixHeader> columnHeaderValues)
        {
            // необходимо учесть высоту заголовков строк
            Tuple<IList<IMatrixHeader>, int, int, int, int> childs = this.CreateChildHeaders(columnHeaderValues, 0, this.rowHeadersColumnSpan, HeaderType.column);
            foreach (IMatrixHeader item in childs.Item1)
            {
                matrixItems.Add(item);
                this.cells[item.GridRow, item.GridColumn] = item;
            }
        }

        /// <summary>
        /// Добавление в матрицу заголовков строк
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="rowHeaderValues">Список заголовков строк</param>
        private void CreateRowHeaders(ICollection<IMatrixCell> matrixItems, IList<IMatrixHeader> rowHeaderValues)
        {
            // необходимо учесть высоту заголовков столбцов
            Tuple<IList<IMatrixHeader>, int, int, int, int> childs = this.CreateChildHeaders(rowHeaderValues, this.columnHeadersRowSpan, 0, HeaderType.row);
            foreach (IMatrixHeader item in childs.Item1)
            {
                matrixItems.Add(item);
                this.cells[item.GridRow, item.GridColumn] = item;
            }
        }

        /// <summary>
        /// Добавление в матрицу ячеек с данными
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="rowHeaderValues">Список заголовков строк</param>
        /// <param name="columnHeaderValues">Список заголовков столбцов</param>
        private void CreateCells(ICollection<IMatrixCell> matrixItems, List<IMatrixHeader> rowHeaderValues, IList<IMatrixHeader> columnHeaderValues)
        {
            // Добавление ячеек с данными
            int row = this.columnHeadersRowSpan, column = this.rowHeadersColumnSpan;

            foreach (IMatrixHeader rowHeader in this.rowHeaders)
            {
                column = this.rowHeadersColumnSpan;
                foreach (IMatrixHeader columnHeader in this.columnHeaders)
                {
                    IMatrixDataCell dataCell = this.GetDataCell(rowHeader, columnHeader);
                    dataCell.SetGridProperties(row, column++);
                    matrixItems.Add(dataCell);
                    this.cells[dataCell.GridRow, dataCell.GridColumn] = dataCell;
                }

                row++;
            }
        }

        /// <summary>
        ///  Рекурсивное создание списка подзаголовков
        /// </summary>
        /// <param name="headerValues">Список заголовков</param>
        /// <param name="rowIndex">Строка матрицы</param>
        /// <param name="colIndex">Колонка матрицы</param>
        /// <param name="headerType">Тип заголовка</param>
        /// <returns></returns>
        private Tuple<IList<IMatrixHeader>, int, int, int, int> CreateChildHeaders(IList<IMatrixHeader> headerValues, int rowIndex, int colIndex, HeaderType headerType)
        {
            IList<IMatrixHeader> matrixItems = new List<IMatrixHeader>();
            Tuple<IList<IMatrixHeader>, int, int, int, int> childs;
            int count = (headerValues != null && headerValues.Count > 0) ? headerValues.Count : 0;
            if (count >= 1)
            {
                int span = 0;
                foreach (IMatrixHeader header in headerValues)
                {
                    if (headerType == HeaderType.row)
                    {
                        childs = this.CreateChildHeaders(header.Children, rowIndex, colIndex + 1, headerType);
                    }
                    else
                    {
                        childs = this.CreateChildHeaders(header.Children, rowIndex + 1, colIndex, headerType);
                    }

                    MatrixHeaderCell headerItem;
                    if (headerType == HeaderType.row)
                    {
                        headerItem = new MatrixHeaderCell(header, MatrixCellType.RowHeader, childs.Item1);
                        headerItem.SetGridProperties(rowIndex, colIndex, childs.Item4, childs.Item5);

                        span += childs.Item4;
                        rowIndex += childs.Item4;
                    }
                    else
                    {
                        headerItem = new MatrixHeaderCell(header, MatrixCellType.ColumnHeader, childs.Item1);
                        headerItem.SetGridProperties(rowIndex, colIndex, childs.Item4, childs.Item5);

                        span += childs.Item5;
                        colIndex += childs.Item5;
                    }

                    matrixItems.Add(headerItem);

                    foreach (IMatrixHeader item in childs.Item1)
                    {
                        matrixItems.Add(item);
                    }
                }

                return new Tuple<IList<IMatrixHeader>, int, int, int, int>(matrixItems, rowIndex, colIndex,
                    headerType == HeaderType.row ? span : 1,
                    headerType == HeaderType.column ? span : 1);
            }
            else
            {
                return new Tuple<IList<IMatrixHeader>, int, int, int, int>(matrixItems, rowIndex, colIndex,
                    rowIndex < this.columnHeadersRowSpan ? this.columnHeadersRowSpan - rowIndex + 1 : 1,
                    colIndex < this.rowHeadersColumnSpan ? this.rowHeadersColumnSpan - colIndex + 1 : 1);
            }
        }

        /// <summary>
        /// Подсчёт высоты дерева
        /// </summary>
        /// <param name="headerValues"></param>
        /// <returns></returns>
        private int MaxHeaderDepth(IList<IMatrixHeader> headerValues)
        {
            if (headerValues == null || headerValues.Count == 0)
            {
                return 0;
            }

            int maxDepth = 0;
            foreach (IMatrixHeader header in headerValues)
            {
                int childrenDepth = 1;

                if (header.Children != null && header.Children.Count > 0)
                {
                    childrenDepth += this.MaxHeaderDepth(header.Children);
                }

                maxDepth = Math.Max(maxDepth, childrenDepth);
            }

            return maxDepth;
        }

        private void SetParentForChildren(IList<IMatrixHeader> headers, ref IMatrixHeader parent)
        {
            if (headers == null || headers.Count == 0)
            {
                return;
            }

            for (int index = 0; index < headers.Count; index++)
            {
                IMatrixHeader header = headers[index];
                header.Parent = parent;
                if (header.Children != null)
                {
                    this.SetParentForChildren(header.Children, ref header);
                }
            }
        }

        private IList<IMatrixHeader> BuildFlatHeadersList(IList<IMatrixHeader> headers)
        {
            List<IMatrixHeader> result = new List<IMatrixHeader>();
            for (int index = 0; index < headers.Count; index++)
            {
                IMatrixHeader header = headers[index];
                if (header.Children == null || header.Children.Count == 0)
                {
                    result.Add(header);
                }
                else
                {
                    result.AddRange(this.BuildFlatHeadersList(header.Children));
                }
            }

            return result;
        }

        #endregion // Matrix Construction

        #region Default commands implementation

        private void CopyToClipboard()
        {
            Helpers.FormatHelper.MatrixToClipboard(this);
        }

        #endregion

        public override string ToString()
        {
            return this.items == null || this.items.Count == 0
                ? "Matrix is empty."
                : string.Format("Matrix size {0}x{1}", this.Size.Height, this.Size.Width);
        }
    }
}
