using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    public interface IMatrixSummaryCell : IMatrixDataCell
    {
        MatrixSummaryType SummaryType { get; }
    }
}
