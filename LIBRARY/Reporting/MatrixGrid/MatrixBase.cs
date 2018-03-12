using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        /// Наследуемые классы должны переопределить этот метод для возврата значения
        /// каждой ячейки данных в матрице, где ячейка определяется пересечением  
        /// строки и столбцы
        /// </summary>
        /// <param name="rowHeaderValue">Строка</param>
        /// <param name="columnHeaderValue">Столбец</param>
        protected abstract object GetCellValue(IMatrixHeader rowHeaderValue, IMatrixHeader columnHeaderValue);

        #endregion // Abstract Methods

        #region Properties

        /// <summary>
        /// Возвращает доступную только для чтения коллекцию всех ячеек матрицы
        /// </summary>
        public ReadOnlyCollection<MatrixItemBase> Items
        {
            get
            {
                if (_matrixItems == null)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        _matrixItems = new ReadOnlyCollection<MatrixItemBase>(this.BuildMatrix());
                        RaisePropertyChanged("Items");
                    });
                }
                return _matrixItems;
            }
        }

        public string Header { get; set; }
        public string Description { get; set; }

        public ICommand CommandExport { get; set; }
        public ICommand CommandCopyToClipboard { get; set; }

        public event PropertyChangedEventHandler Builded;

        public bool ShowColumnsTotal { get; set; }
        public bool ShowRowsTotal { get; set; }

        #endregion // Properties

        #region Matrix Construction

        List<MatrixItemBase> BuildMatrix()
        {
            List<MatrixItemBase> matrixItems = new List<MatrixItemBase>();

            // Получение значений заголовков строк и столбцов
            List<IMatrixHeader> rowHeaderValues = this.GetRowHeaderValues().ToList();
            List<IMatrixHeader> columnHeaderValues = this.GetColumnHeaderValues().ToList();

            SetParentForChildren(rowHeaderValues);
            SetParentForChildren(columnHeaderValues);

            _rowHeaders = BuildFlatHeadersList(rowHeaderValues);
            _columnHeaders = BuildFlatHeadersList(columnHeaderValues);

            // подсчёт высоты заголовков
            _rowHeadersColumnSpan = MaxHeaderDepth(rowHeaderValues);
            _columnHeadersRowSpan = MaxHeaderDepth(columnHeaderValues);
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

            if (ShowRowsTotal)
            {
                matrixItems.Add(new MatrixSummaryHeaderItem("Итого") { GridRow = 0, GridRowSpan = _columnHeadersRowSpan, GridColumn = _rowHeadersColumnSpan + _columnHeaders.Count });

                int summ = 0;
                for (int rowIndex = _columnHeadersRowSpan; rowIndex < _rowHeaders.Count + _columnHeadersRowSpan; rowIndex ++)
                {
                    summ = matrixItems.
                    Where(i => i.GridRow == rowIndex)
                    .Where(i => i.GridColumn >= _rowHeadersColumnSpan)
                    .Cast<MatrixCellItem>()
                    .Sum(i => getInt(i.Value));

                    matrixItems.Add(new MatrixSummaryColumnItem(summ) { GridRow = rowIndex, GridColumn = _rowHeadersColumnSpan + _columnHeaders.Count });
                }
            }
            if (ShowColumnsTotal)
            {
                matrixItems.Add(new MatrixSummaryHeaderItem("Итого") { GridRow = _columnHeadersRowSpan + _rowHeaders.Count, GridColumn = 0, GridColumnSpan = _rowHeadersColumnSpan });

                int summ = 0;
                for (int columnIndex = _rowHeadersColumnSpan; columnIndex < _columnHeaders.Count + _rowHeadersColumnSpan; columnIndex++)
                {
                    summ = matrixItems.
                    Where(i => i.GridColumn == columnIndex)
                    .Where(i => i.GridRow >= _columnHeadersRowSpan)
                    .Cast<MatrixCellItem>()
                    .Sum(i => getInt(i.Value));

                    matrixItems.Add(new MatrixSummaryRowItem(summ) { GridRow = _columnHeadersRowSpan + _rowHeaders.Count, GridColumn = columnIndex });
                }
            }

            if (ShowRowsTotal & ShowColumnsTotal)
            {
                int summ = matrixItems.
                Where(i => i.GridColumn == _rowHeadersColumnSpan + _columnHeaders.Count)
                .Where(i => (i.GridRow >= _columnHeadersRowSpan) & (i.GridRow != (_columnHeadersRowSpan + _rowHeaders.Count)))
                .Cast<MatrixSummaryItem>()
                .Sum(i => i.Value);

                matrixItems.Add(new MatrixSummaryItem(summ) { GridRow = _columnHeadersRowSpan + _rowHeaders.Count, GridColumn = _rowHeadersColumnSpan + _columnHeaders.Count });
            }

                // оповещение о готовности
                Builded?.Invoke(this, null);

            return matrixItems;
        }

        void CreateEmptyHeader(ICollection<MatrixItemBase> matrixItems)
        {
            // Вставка пустой ячейки
            matrixItems.Add(new MatrixEmptyHeaderItem
            {
                GridRow = 0,
                GridColumn = 0,
                GridRowSpan = _columnHeadersRowSpan,
                GridColumnSpan = _rowHeadersColumnSpan
            });
        }

        enum HeaderType { row, column}

        /// <summary>
        /// Добавление в матрицу заголовков столбцов
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="columnHeaderValues">Список заголовков столбцов</param>
        void CreateColumnHeaders(ICollection<MatrixItemBase> matrixItems, IList<IMatrixHeader> columnHeaderValues)
        {
            // необходимо учесть высоту заголовков строк
            Tuple<ICollection<MatrixItemBase>, int, int, int, int> childs = CreateChildHeaders(columnHeaderValues, 0, _rowHeadersColumnSpan, HeaderType.column);
            foreach (MatrixItemBase item in childs.Item1)
                matrixItems.Add(item);
        }
        /// <summary>
        /// Добавление в матрицу заголовков строк
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="rowHeaderValues">Список заголовков строк</param>
        void CreateRowHeaders(ICollection<MatrixItemBase> matrixItems, IList<IMatrixHeader> rowHeaderValues)
        {
            // необходимо учесть высоту заголовков столбцов
            Tuple<ICollection<MatrixItemBase>, int, int, int, int> childs = CreateChildHeaders(rowHeaderValues, _columnHeadersRowSpan, 0, HeaderType.row);
            foreach (MatrixItemBase item in childs.Item1)
                matrixItems.Add(item);
        }
        /// <summary>
        /// Добавление в матрицу ячеек с данными
        /// </summary>
        /// <param name="matrixItems">Коллекция ячеек матрицы</param>
        /// <param name="rowHeaderValues">Список заголовков строк</param>
        /// <param name="columnHeaderValues">Список заголовков столбцов</param>
        void CreateCells(ICollection<MatrixItemBase> matrixItems, List<IMatrixHeader> rowHeaderValues, IList<IMatrixHeader> columnHeaderValues)
        {
            // Добавление ячеек с данными
            int row = _columnHeadersRowSpan, column = _rowHeadersColumnSpan;

            foreach (IMatrixHeader rowHeader in _rowHeaders)
            {
                column = _rowHeadersColumnSpan;
                foreach (IMatrixHeader columnHeader in _columnHeaders)
                {
                    object cellValue = this.GetCellValue(rowHeader, columnHeader);
                    matrixItems.Add(new MatrixCellItem(cellValue)
                    {
                        GridRow = row,
                        GridColumn = column++
                    });
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
        Tuple<ICollection<MatrixItemBase>, int, int, int, int> CreateChildHeaders(IList<IMatrixHeader> headerValues, int rowIndex, int colIndex, HeaderType headerType)
        {
            ICollection<MatrixItemBase> matrixItems = new List<MatrixItemBase>();
            Tuple<ICollection<MatrixItemBase>, int, int, int, int> childs;
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

                    MatrixHeaderItemBase headerItem;
                    if (headerType == HeaderType.row)
                    {
                        headerItem = new MatrixRowHeaderItem(header)
                        {
                            GridRow = rowIndex,
                            GridRowSpan = childs.Item4,
                            GridColumn = colIndex,
                            GridColumnSpan = childs.Item5
                        };
                        span += childs.Item4;
                        rowIndex += childs.Item4;
                    }
                    else
                    {
                        headerItem = new MatrixColumnHeaderItem(header)
                        {
                            GridRow = rowIndex,
                            GridRowSpan = childs.Item4,
                            GridColumn = colIndex,
                            GridColumnSpan = childs.Item5
                        };
                        span += childs.Item5;
                        colIndex += childs.Item5;
                    }
                    matrixItems.Add(headerItem);

                    foreach (MatrixItemBase item in childs.Item1)
                        matrixItems.Add(item);
                }

                return new Tuple<ICollection<MatrixItemBase>, int, int, int, int>(matrixItems, rowIndex, colIndex,
                    headerType == HeaderType.row ? span : 1,
                    headerType == HeaderType.column ? span : 1);
            }
            else
                return new Tuple<ICollection<MatrixItemBase>, int, int, int, int>(matrixItems, rowIndex, colIndex,
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
            int result = 1;
            foreach (IMatrixHeader header in headerValues)
            {
                if (header.Children != null)
                    result = Math.Max(result, MaxHeaderDepth(header.Children) + 1);
            }
            return result;
        }

        void SetParentForChildren(IList<IMatrixHeader> headers, IMatrixHeader parent = null)
        {
            if (headers == null || headers.Count == 0) return; 
            foreach (MatrixHeaderItemBase header in headers)
            {
                header.Parent = parent;
                SetParentForChildren(header.Children, header);
            }
        }
        IList<IMatrixHeader> BuildFlatHeadersList(IList<IMatrixHeader> headers)
        {
            List<IMatrixHeader> result = new List<IMatrixHeader>();
            foreach (MatrixHeaderItemBase header in headers)
            {
                if (header.Children == null || header.Children.Count == 0)
                    result.Add(header);
                else
                    result.AddRange(BuildFlatHeadersList(header.Children));
            }
            return result;
        }

        #endregion // Matrix Construction

        #region Fields
        /// <summary>
        /// Коллекция ячеек матрицы
        /// </summary>
        ReadOnlyCollection<MatrixItemBase> _matrixItems;
        /// <summary>
        /// Ширина заголовков строк и столбцов
        /// </summary>
        int _columnHeadersRowSpan = 1, _rowHeadersColumnSpan = 1;

        IList<IMatrixHeader> _rowHeaders, _columnHeaders;

        #endregion // Fields

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
