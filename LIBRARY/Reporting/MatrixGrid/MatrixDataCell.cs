namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Представляет ячейку с данными матрицы
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("R[{GridRow}],C[{GridColumn}],RS[{GridRowSpan}],CS[{GridColumnSpan}], Value: {Value}")]
    public class MatrixDataCell : MatrixCellBase, IMatrixDataCell
    {
        public MatrixDataCell(object value, string contentFormat = null)
        {
            this.CellType = MatrixCellType.DataCell;
            this.Value = value;
            if (string.IsNullOrWhiteSpace(contentFormat) == false)
            {
                this.ContentFormat = contentFormat;
            }
        }

        /// <summary>
        /// Значение ячейки
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Формат для отображения содержимого
        /// </summary>
        public string ContentFormat { get; set; } = "N0";

        /// <summary>
        /// Преобразование значения в числе, если неудачно возврат 0
        /// </summary>
        /// <returns></returns>
        public int ValueToInt()
        {
            int result = 0;
            int.TryParse(this.Value.ToString(), out result);
            return result;
        }

        public override string ToString()
        {
            return string.Format(string.Format("MatrixDataCell - {{0:{0}}}", this.ContentFormat), this.Value);
        }
    }
}
