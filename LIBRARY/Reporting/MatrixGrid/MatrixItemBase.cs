using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Базовый класс для ячеек матрицы
    /// </summary>
    public abstract class MatrixItemBase : IMatrixItem
    {
        public int GridRow { get; internal set; }
        public int GridRowSpan { get; internal set; } = 1;
        public int GridColumn { get; internal set; }
        public int GridColumnSpan { get; internal set; } = 1;

        public string ToolTip { get; internal set; }
    }
}
