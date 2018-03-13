using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Представляет ячейку с данными матрицы
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("R[{GridRow}],C[{GridColumn}],RS[{GridRowSpan}],CS[{GridColumnSpan}], Value: {Value}")]
    public class MatrixCellItem : MatrixItemBase
    {
        public MatrixCellItem(object value)
        {
            this.Value = value;
        }

        public object Value { get; private set; }

        public string ContentFormat { get; set; } = "N0";

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
