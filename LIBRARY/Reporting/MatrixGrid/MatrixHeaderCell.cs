namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Класс заголовка матрицы <see cref="IMatrix"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{CellType}, R[{GridRow}],C[{GridColumn}],RS[{GridRowSpan}],CS[{GridColumnSpan}], Header: {Header}, ChildrenCount: {ChildrenCount}")]
    public class MatrixHeaderCell : MatrixCellBase, IMatrixHeader
    {
        protected MatrixHeaderCell(MatrixCellType cellType, string header = null)
        {
            CellType = cellType;
            Header = header;
        }
        public MatrixHeaderCell(IMatrixHeader header, MatrixCellType newCellType, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            CellType = newCellType;
            Children = children;
            Header = header.Header;
            ToolTip = header.ToolTip;
            Tag = header.Tag;
        }
        public static MatrixHeaderCell CreateHeaderCell(string header, System.Collections.Generic.IList<IMatrixHeader> children = null, string toolTip = null, object tag = null)
        {
            return new MatrixHeaderCell(MatrixCellType.Empty, header)
            {
                Children = children,
                ToolTip = toolTip,
                Tag = tag
            };
        }

        public static MatrixHeaderCell CreateEmptyHeader(int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.Empty);
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static MatrixHeaderCell CreateColumnHeader(string header, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            return new MatrixHeaderCell(MatrixCellType.ColumnHeader, header)
            {
                Children = children
            };
        }
        public static MatrixHeaderCell CreateColumnHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.ColumnHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }
        public static MatrixHeaderCell CreateColumnsGroupHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.ColumnsGroupHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }
        public static MatrixHeaderCell CreateRowHeader(string header, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            return new MatrixHeaderCell(MatrixCellType.RowHeader, header)
            {
                Children = children
            };
        }
        public static MatrixHeaderCell CreateRowHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.RowHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }
        public static MatrixHeaderCell CreateRowsGroupHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.RowsGroupHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }
        public static MatrixHeaderCell CreateRowSummaryHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.RowSummaryHeader, header);
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }
        public static MatrixHeaderCell CreateColumnSummaryHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.ColumnSummaryHeader, header);
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Header { get; internal set; }
        /// <summary>
        /// Объект, хранящий дополнительную информацию
        /// </summary>
        public object Tag { get; internal set; }
        /// <summary>
        /// Список подзаголовков
        /// </summary>
        public System.Collections.Generic.IList<IMatrixHeader> Children { get; internal set; }
        /// <summary>
        /// Количество подзаголовков
        /// </summary>
        public int ChildrenCount => Children == null ? 0 : Children.Count;
        /// <summary>
        /// Ссылка на родительский заголовок
        /// </summary>
        public IMatrixHeader Parent { get; internal set; }

        public override string ToString()
        {
            return string.Format("MatrixHeaderCell - {0} - {1}, childs: {2}, R[{3}], C[{4}], RS[{5}, CS[{6}]", Header, CellType, ChildrenCount, GridRow, GridColumn, GridRowSpan, GridColumnSpan);
        }
    }
}