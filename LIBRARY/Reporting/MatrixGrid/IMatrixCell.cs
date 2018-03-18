namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Интерфейс для ячеек матрицы
    /// </summary>
    public interface IMatrixCell
    {
        MatrixCellType CellType { get; }

        int GridRow { get; }
        int GridRowSpan { get; }
        int GridColumn { get; }
        int GridColumnSpan { get; }

        string ToolTip { get; set; }

        void SetGridProperties(int row, int column, int rowSpan = 1, int columnSpan = 1);
    }
}
