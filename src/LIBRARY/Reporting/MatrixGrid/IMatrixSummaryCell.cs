namespace TMP.UI.WPF.Reporting.MatrixGrid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IMatrixSummaryCell : IMatrixDataCell
    {
        MatrixSummaryType SummaryType { get; }
    }
}
