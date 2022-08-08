namespace TMP.UI.WPF.Reporting.MatrixGrid
{
    public class MatrixSummaryCell : MatrixDataCell, IMatrixSummaryCell
    {
        private MatrixSummaryCell(object value, string contentFormat = null)
            : base(value, contentFormat)
        {
            this.CellType = MatrixCellType.SummaryCell;
        }

        public static IMatrixSummaryCell CreateRowSummary(object value, int row, int column, int rowSpan = 1, int columnSpan = 1, string contentFormat = null)
        {
            MatrixSummaryCell cell = new MatrixSummaryCell(value, contentFormat) { SummaryType = MatrixSummaryType.RowSummary };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixSummaryCell CreateColumnSummary(object value, int row, int column, int rowSpan = 1, int columnSpan = 1, string contentFormat = null)
        {
            MatrixSummaryCell cell = new MatrixSummaryCell(value, contentFormat) { SummaryType = MatrixSummaryType.ColumnSummary };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixSummaryCell CreateTotalSummary(object value, int row, int column, int rowSpan = 1, int columnSpan = 1, string contentFormat = null)
        {
            MatrixSummaryCell cell = new MatrixSummaryCell(value, contentFormat) { SummaryType = MatrixSummaryType.TotalSummary };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixSummaryCell CreateRowsGroupSummary(object value, int row, int column, int rowSpan = 1, int columnSpan = 1, string contentFormat = null)
        {
            MatrixSummaryCell cell = new MatrixSummaryCell(value, contentFormat) { SummaryType = MatrixSummaryType.RowsGroupSummary };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixSummaryCell CreateColumnsGroupSummary(object value, int row, int column, int rowSpan = 1, int columnSpan = 1, string contentFormat = null)
        {
            MatrixSummaryCell cell = new MatrixSummaryCell(value, contentFormat) { SummaryType = MatrixSummaryType.ColumnsGroupSummary };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public static IMatrixSummaryCell CreateTotalGroupSummary(object value, int row, int column, int rowSpan = 1, int columnSpan = 1, string contentFormat = null)
        {
            MatrixSummaryCell cell = new MatrixSummaryCell(value, contentFormat) { SummaryType = MatrixSummaryType.TotalGroupSummary };
            cell.SetGridProperties(row, column, rowSpan, columnSpan);
            return cell;
        }

        public MatrixSummaryType SummaryType { get; internal set; }

        public override string ToString()
        {
            return string.Format(string.Format("MatrixSummary - {{0}} [{{1:{0}}}], R[{{2}}], C[{{3}}], RS[{{4}}, CS[{{5}}]", this.ContentFormat), this.SummaryType, this.Value, this.GridRow, this.GridColumn, this.GridRowSpan, this.GridColumnSpan);
        }
    }
}
