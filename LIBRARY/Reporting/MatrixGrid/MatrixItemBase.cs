namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Базовый класс для ячеек матрицы
    /// </summary>
    public abstract class MatrixItemBase : IMatrixItem
    {
        public int GridRow { get;  set; }
        public int GridRowSpan { get;  set; } = 1;
        public int GridColumn { get;  set; }
        public int GridColumnSpan { get;  set; } = 1;

        public string ToolTip { get; set; }
    }
}
