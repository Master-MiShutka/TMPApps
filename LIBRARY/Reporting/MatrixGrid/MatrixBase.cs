using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Базовый класс для отображения данных в виде матрицы в MatrixControl
    /// </summary>
    public abstract class MatrixBase : IMatrix, INotifyPropertyChanged
    {
        #region Constructor

        protected MatrixBase()
        {
            CommandCopyToClipboard = new RelayCommand(
                CopyToClipboard, 
                () => {
                    return HasData;
                    });

            CommandPrint = new RelayCommand(
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
                                Document = fds as System.Windows.Documents.IDocumentPaginatorSource
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
                    return HasData;
                });
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
        public bool HasData
        {
            get { return _matrixItems != null; }
        }

        /// <summary>
        /// Возвращает доступную только для чтения коллекцию всех ячеек матрицы
        /// </summary>
        public IList<IMatrixCell> Items
        {
            get
            {
                if (_matrixItems == null)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        _matrixItems = new ReadOnlyCollection<IMatrixCell>(this.BuildMatrix());
                        RaisePropertyChanged("Items");
                        RaisePropertyChanged("RowHeadersCount");
                        RaisePropertyChanged("ColumnHeadersCount");
                        RaisePropertyChanged("Size");
                        RaisePropertyChanged("HasData");
                    });
                }
                return _matrixItems;
            }
        }
        /// <summary>
        /// Количество полных строк заголовка - количество строк с данными
        /// </summary>
        public int RowHeadersCount
        {
            get
            {
                if (_matrixItems == null) return 0;
                return _rowHeaders != null ? _rowHeaders.Count : 0;
            }
        }
        /// <summary>
        /// Количество полных столбцов заголовка - количество столбцов с данными
        /// </summary>
        public int ColumnHeadersCount
        {
            get
            {
                if (_matrixItems == null) return 0;
                return _columnHeaders != null ? _columnHeaders.Count : 0;
            }
        }
        /// <summary>
        /// Размер матрицы
        /// </summary>
        public Size Size
        {
            get
            {
                if (_matrixItems == null)
                    return Size.Empty;
                return _size;
            }
        }
        /// <summary>
        /// Двумерный массив ячеек матрицы
        /// </summary>
        public IMatrixCell[,] Cells  => _cells;

        /// <summary>
        /// Заголовок матрицы
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Команда копирования данных в виде таблицы в буфер обмена
        /// </summary>
        public ICommand CommandCopyToClipboard { get; set; }
        /// <summary>
        /// Команда печати данных
        /// </summary>
        public ICommand CommandPrint { get; set; }
        /// <summary>
        /// Событие для оповещения о завершении построения матрицы
        /// </summary>
        public event PropertyChangedEventHandler Builded;
        /// <summary>
        /// Отображать ли итоги по столбцам
        /// </summary>
        public bool? ShowColumnsTotal { get; set; }
        /// <summary>
        /// Отображать ли итоги по строкам
        /// </summary>
        public bool? ShowRowsTotal { get; set; }

        #endregion // Properties

        #region Matrix Construction

        List<IMatrixCell> BuildMatrix()
        {
            List<IMatrixCell> matrixItems = new List<IMatrixCell>();

            // Получение значений заголовков строк и столбцов
            List<IMatrixHeader> rowHeaderValues = this.GetRowHeaderValues().ToList();
            List<IMatrixHeader> columnHeaderValues = this.GetColumnHeaderValues().ToList();

            SetParentForChildren(rowHeaderValues);
            SetParentForChildren(columnHeaderValues);

            // подсчёт высоты заголовков
            _rowHeadersColumnSpan = MaxHeaderDepth(rowHeaderValues);
            _columnHeadersRowSpan = MaxHeaderDepth(columnHeaderValues);

            _rowHeaders = BuildFlatHeadersList(rowHeaderValues);
            _columnHeaders = BuildFlatHeadersList(columnHeaderValues);

            // высота и ширина блока с данными
            int dataBlockHeight = _rowHeaders.Count(),
                dataBlockWidth = _columnHeaders.Count();

            int matrixHeight = _columnHeadersRowSpan + dataBlockHeight;
            int matrixWidth = _rowHeadersColumnSpan + dataBlockWidth;

            if (ShowColumnsTotal.GetValueOrDefault())
                matrixHeight++;
            if (ShowRowsTotal.GetValueOrDefault())
                matrixWidth++;

            _size = new Size(matrixWidth, matrixHeight);

            _cells = new IMatrixCell[(int)_size.Height, (int)_size.Width];

            // проседура для добавления ячейки и в список и в массив
            Action<IMatrixCell> addCell = (cell) =>
            {
                matrixItems.Add(cell);
                _cells[cell.GridRow, cell.GridColumn] = cell;
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
                    return result;
                int.TryParse(value.ToString(), out result);
                return result;
            };

            if (ShowRowsTotal.GetValueOrDefault())
            {
                addCell(MatrixHeaderCell.CreateColumnSummaryHeader("Итого", row: 0, rowSpan: _columnHeadersRowSpan, column: _rowHeadersColumnSpan + _columnHeaders.Count));

                int summ = 0;
                for (int rowIndex = _columnHeadersRowSpan; rowIndex < _rowHeaders.Count + _columnHeadersRowSpan; rowIndex ++)
                {
                    summ = matrixItems.
                    Where(i => i.GridRow == rowIndex)
                    .Where(i => i.GridColumn >= _rowHeadersColumnSpan)
                    .Where(i => i.CellType == MatrixCellType.DataCell)
                    .Cast<IMatrixDataCell>()
                    .Sum(i => getInt(i.Value));

                    addCell(MatrixSummaryCell.CreateRowSummary(summ, row: rowIndex, column: _rowHeadersColumnSpan + _columnHeaders.Count));
                }
            }
            if (ShowColumnsTotal.GetValueOrDefault())
            {
                addCell(MatrixHeaderCell.CreateRowSummaryHeader("Итого", row : _columnHeadersRowSpan + _rowHeaders.Count, column : 0, columnSpan : _rowHeadersColumnSpan ));

                int summ = 0;
                for (int columnIndex = _rowHeadersColumnSpan; columnIndex < _columnHeaders.Count + _rowHeadersColumnSpan; columnIndex++)
                {
                    summ = matrixItems.
                    Where(i => i.GridColumn == columnIndex)
                    .Where(i => i.GridRow >= _columnHeadersRowSpan)
                    .Where(i => i.CellType == MatrixCellType.DataCell)
                    .Cast<IMatrixDataCell>()
                    .Sum(i => getInt(i.Value));

                    addCell(MatrixSummaryCell.CreateColumnSummary(summ, row: _columnHeadersRowSpan + _rowHeaders.Count, column: columnIndex));                    
                }
            }

            if (ShowRowsTotal.GetValueOrDefault() & ShowColumnsTotal.GetValueOrDefault())
            {
                int summ = matrixItems.
                Where(i => i.GridColumn == _rowHeadersColumnSpan + _columnHeaders.Count)
                .Where(i => (i.GridRow >= _columnHeadersRowSpan) & (i.GridRow != (_columnHeadersRowSpan + _rowHeaders.Count)))
                .Where(i => i.CellType == MatrixCellType.SummaryCell)
                .Cast<IMatrixSummaryCell>()
                .Sum(i => i.ValueToInt());

                addCell(MatrixSummaryCell.CreateTotalSummary(summ, row: _columnHeadersRowSpan + _rowHeaders.Count, column: _rowHeadersColumnSpan + _columnHeaders.Count));
            }

            // оповещение о готовности
            Builded?.Invoke(this, null);

            return matrixItems;
        }

        void CreateEmptyHeader(ICollection<IMatrixCell> matrixItems)
        {
            // Вставка пустой ячейки
            var cell = MatrixHeaderCell.CreateEmptyHeader(row: 0, column: 0, rowSpan: _columnHeadersRowSpan, columnSpan: _rowHeadersColumnSpan);
            matrixItems.Add(cell);
            _cells[0, 0] = cell;
        }

        enum HeaderType { row, column}

        /// <summary>
        /// Добавление в матрицу заголовков столбцов
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="columnHeaderValues">Список заголовков столбцов</param>
        void CreateColumnHeaders(ICollection<IMatrixCell> matrixItems, IList<IMatrixHeader> columnHeaderValues)
        {
            // необходимо учесть высоту заголовков строк
            Tuple<IList<IMatrixHeader>, int, int, int, int> childs = CreateChildHeaders(columnHeaderValues, 0, _rowHeadersColumnSpan, HeaderType.column);
            foreach (IMatrixHeader item in childs.Item1)
            {
                matrixItems.Add(item);
                _cells[item.GridRow, item.GridColumn] = item;
            }
        }
        /// <summary>
        /// Добавление в матрицу заголовков строк
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="rowHeaderValues">Список заголовков строк</param>
        void CreateRowHeaders(ICollection<IMatrixCell> matrixItems, IList<IMatrixHeader> rowHeaderValues)
        {
            // необходимо учесть высоту заголовков столбцов
            Tuple<IList<IMatrixHeader>, int, int, int, int> childs = CreateChildHeaders(rowHeaderValues, _columnHeadersRowSpan, 0, HeaderType.row);
            foreach (IMatrixHeader item in childs.Item1)
            {
                matrixItems.Add(item);
                _cells[item.GridRow, item.GridColumn] = item;
            }
        }
        /// <summary>
        /// Добавление в матрицу ячеек с данными
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="rowHeaderValues">Список заголовков строк</param>
        /// <param name="columnHeaderValues">Список заголовков столбцов</param>
        void CreateCells(ICollection<IMatrixCell> matrixItems, List<IMatrixHeader> rowHeaderValues, IList<IMatrixHeader> columnHeaderValues)
        {
            // Добавление ячеек с данными
            int row = _columnHeadersRowSpan, column = _rowHeadersColumnSpan;

            foreach (IMatrixHeader rowHeader in _rowHeaders)
            {
                column = _rowHeadersColumnSpan;
                foreach (IMatrixHeader columnHeader in _columnHeaders)
                {
                    IMatrixDataCell dataCell = this.GetDataCell(rowHeader, columnHeader);
                    dataCell.SetGridProperties(row, column++);
                    matrixItems.Add(dataCell);
                    _cells[dataCell.GridRow, dataCell.GridColumn] = dataCell;
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
        Tuple<IList<IMatrixHeader>, int, int, int, int> CreateChildHeaders(IList<IMatrixHeader> headerValues, int rowIndex, int colIndex, HeaderType headerType)
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
                        childs = CreateChildHeaders(header.Children, rowIndex, colIndex + 1, headerType);
                    else
                        childs = CreateChildHeaders(header.Children, rowIndex + 1, colIndex, headerType);

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
                        matrixItems.Add(item);
                }

                return new Tuple<IList<IMatrixHeader>, int, int, int, int>(matrixItems, rowIndex, colIndex,
                    headerType == HeaderType.row ? span : 1,
                    headerType == HeaderType.column ? span : 1);
            }
            else
                return new Tuple<IList<IMatrixHeader>, int, int, int, int>(matrixItems, rowIndex, colIndex,
                    rowIndex < _columnHeadersRowSpan ? _columnHeadersRowSpan - rowIndex + 1 : 1,
                    colIndex < _rowHeadersColumnSpan ? _rowHeadersColumnSpan - colIndex + 1 : 1);

        }
        /// <summary>
        /// Подсчёт высоты дерева
        /// </summary>
        /// <param name="headerValues"></param>
        /// <returns></returns>
        int MaxHeaderDepth(IList<IMatrixHeader> headerValues)
        {
            if (headerValues == null || headerValues.Count == 0)
                return 0;

            int maxDepth = 0;
            foreach (IMatrixHeader header in headerValues)
            {
                int childrenDepth = 1;

                if (header.Children != null && header.Children.Count > 0)
                    childrenDepth += MaxHeaderDepth(header.Children);
                maxDepth = Math.Max(maxDepth, childrenDepth);
            }
            return maxDepth;
        }

        void SetParentForChildren(IList<IMatrixHeader> headers, IMatrixHeader parent = null)
        {
            if (headers == null || headers.Count == 0) return; 
            foreach (MatrixHeaderCell header in headers)
            {
                header.Parent = parent;
                SetParentForChildren(header.Children, header);
            }
        }

        IList<IMatrixHeader> BuildFlatHeadersList(IList<IMatrixHeader> headers)
        {
            List<IMatrixHeader> result = new List<IMatrixHeader>();
            foreach (MatrixHeaderCell header in headers)
            {
                if (header.Children == null || header.Children.Count == 0)
                    result.Add(header);
                else
                    result.AddRange(BuildFlatHeadersList(header.Children));
            }
            return result;
        }

        #endregion // Matrix Construction

        #region Default commands implementation

        void CopyToClipboard()
        {
            Helpers.FormatHelper.MatrixToClipboard(this);
        }

        #endregion

        #region Fields
        /// <summary>
        /// Коллекция ячеек матрицы
        /// </summary>
        ReadOnlyCollection<IMatrixCell> _matrixItems;

        IMatrixCell[,] _cells;

        /// <summary>
        /// Ширина заголовков строк и столбцов
        /// </summary>
        int _columnHeadersRowSpan = 1, _rowHeadersColumnSpan = 1;

        IList<IMatrixHeader> _rowHeaders, _columnHeaders;
        Size _size = Size.Empty;

        #endregion // Fields


        public override string ToString()
        {
            if (_matrixItems == null || _matrixItems.Count == 0)
                return "Matrix is empty.";
            else
                return String.Format("Matrix size {0}x{1}", Size.Height, Size.Width);
        }

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

        class RelayCommand : ICommand
        {
            Action _action;
            Func<bool> _canExecute;

            public RelayCommand(Action action, Func<bool> canExecute)
            {
                if (action == null)
                    throw new ArgumentNullException("Action");
                if (canExecute == null)
                    throw new ArgumentNullException("CanExecute");
                _action = action;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { System.Windows.Input.CommandManager.RequerySuggested += value; }
                remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
            }
            public bool CanExecute(object parameter)
            {
                return _canExecute == null ? true : _canExecute();
            }

            public void Execute(object parameter)
            {
                _action();
            }
            public void OnCanExecuteChanged()
            {
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
