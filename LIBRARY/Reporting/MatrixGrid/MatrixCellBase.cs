namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Базовый класс для ячеек матрицы
    /// </summary>
    public abstract class MatrixCellBase : IMatrixCell
    {
        public int GridRow { get; internal set; }
        public int GridRowSpan { get; internal set; } = 1;
        public int GridColumn { get; internal set; }
        public int GridColumnSpan { get; internal set; } = 1;

        public string ToolTip { get; set; }
        /// <summary>
        /// Тип ячейки
        /// </summary>
        public virtual MatrixCellType CellType { get; internal set; }

        public void SetGridProperties(int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            this.GridRow = row;
            this.GridRowSpan = rowSpan;
            this.GridColumn = column;
            this.GridColumnSpan = columnSpan;

#if DEBUG
            this.ToolTip = ToString();
#endif
        }
    }
}
