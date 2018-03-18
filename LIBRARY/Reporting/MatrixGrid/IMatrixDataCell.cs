﻿namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    public interface IMatrixDataCell : IMatrixCell
    {
        object Value { get; set; }
        string ContentFormat { get; set; }
        int ValueToInt();
    }
}
