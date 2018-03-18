using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    public interface IMatrix
    {
        event PropertyChangedEventHandler Builded;
        IList<IMatrixCell> Items { get; }
        IMatrixCell[,] Cells { get; }

        int RowHeadersCount { get; }
        int ColumnHeadersCount { get; }
        System.Windows.Size Size { get; }

        string Header { get; set; }
        string Description { get; set; }
        ICommand CommandCopyToClipboard { get; set; }
        bool? ShowColumnsTotal { get; set; }
        bool? ShowRowsTotal { get; set; }
    }
}
