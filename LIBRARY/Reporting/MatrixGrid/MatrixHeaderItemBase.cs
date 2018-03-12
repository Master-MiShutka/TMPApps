namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Базовый класс заголовка матрицы <see cref="IMatrix"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("R[{GridRow}],C[{GridColumn}],RS[{GridRowSpan}],CS[{GridColumnSpan}], Header: {Header}, ChildrenCount: {ChildrenCount}")]
    public class MatrixHeaderItemBase : MatrixItemBase, IMatrixHeader
    {
        public string Header { get; internal set; }
        public object Tag { get; internal set; }
        public System.Collections.Generic.IList<IMatrixHeader> Children { get; internal set; }
        public int ChildrenCount => Children == null ? 0 : Children.Count;
        public IMatrixHeader Parent { get; internal set; }
    }
}