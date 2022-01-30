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
            this.CellType = cellType;
            this.Header = header;
        }

        public MatrixHeaderCell(IMatrixHeader header, MatrixCellType newCellType, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            this.CellType = newCellType;
            this.Children = children;
            this.Header = header.Header;
            this.ToolTip = header.ToolTip;
            this.Tag = header.Tag;
        }

        public static IMatrixHeader CreateHeaderCell(string header, System.Collections.Generic.IList<IMatrixHeader> children = null, string toolTip = null, object tag = null)
        {
            return new MatrixHeaderCell(MatrixCellType.Empty, header)
            {
                Children = children,
                ToolTip = toolTip,
                Tag = tag,
            };
        }

        public static IMatrixHeader CreateEmptyHeader(int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.Empty);
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixHeader CreateColumnHeader(string header, object tag = null, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            return new MatrixHeaderCell(MatrixCellType.ColumnHeader, header)
            {
                Tag = tag,
                Children = children,
            };
        }

        public static IMatrixHeader CreateColumnHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.ColumnHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixHeader CreateColumnsGroupHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.ColumnsGroupHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixHeader CreateRowHeader(string header, object tag = null, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            return new MatrixHeaderCell(MatrixCellType.RowHeader, header)
            {
                Tag = tag,
                Children = children,
            };
        }

        public static IMatrixHeader CreateRowHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.RowHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixHeader CreateRowsGroupHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1, System.Collections.Generic.IList<IMatrixHeader> children = null)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.RowsGroupHeader, header) { Children = children };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixHeader CreateRowSummaryHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            MatrixHeaderCell cell = new MatrixHeaderCell(MatrixCellType.RowSummaryHeader, header);
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixHeader CreateColumnSummaryHeader(string header, int row, int column, int rowSpan = 1, int columnSpan = 1)
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
        public int ChildrenCount => this.Children == null ? 0 : this.Children.Count;

        /// <summary>
        /// Ссылка на родительский заголовок
        /// </summary>
        public IMatrixHeader Parent { get; set; }

        public override string ToString()
        {
            return string.Format("MatrixHeaderCell - {0} - {1}, childs: {2}, R[{3}], C[{4}], RS[{5}, CS[{6}]", this.Header, this.CellType, this.ChildrenCount, this.GridRow, this.GridColumn, this.GridRowSpan, this.GridColumnSpan);
        }
    }
}
