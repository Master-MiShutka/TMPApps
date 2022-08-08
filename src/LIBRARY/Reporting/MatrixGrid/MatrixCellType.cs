namespace TMP.UI.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Тип ячейки матрицы
    /// </summary>
    public enum MatrixCellType
    {
        /// <summary>
        /// пустая, ничего не содержащая
        /// </summary>
        Empty,

        /// <summary>
        /// заголовок строки
        /// </summary>
        RowHeader,

        /// <summary>
        /// заголовок группы строк
        /// </summary>
        RowsGroupHeader,

        /// <summary>
        /// заголовок строки с итогом
        /// </summary>
        RowSummaryHeader,

        /// <summary>
        /// заголовок столбца
        /// </summary>
        ColumnHeader,

        /// <summary>
        /// заголовок группы столбцов
        /// </summary>
        ColumnsGroupHeader,

        /// <summary>
        /// заголовок столца с итогом
        /// </summary>
        ColumnSummaryHeader,

        /// <summary>
        /// ячейка с данными
        /// </summary>
        DataCell,

        /// <summary>
        /// ячейка с итогом
        /// </summary>
        SummaryCell,
    }
}
