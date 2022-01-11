namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;

    public interface IMatrix
    {
        bool IsBuilded { get; }

        bool IsBuilding { get; }

        event PropertyChangedEventHandler Builded;

        event PropertyChangedEventHandler Building;

        IList<IMatrixCell> Items { get; }

        IMatrixCell[,] Cells { get; }

        int RowHeadersCount { get; }

        int ColumnHeadersCount { get; }

        System.Windows.Size Size { get; }

        string Header { get; set; }

        string Description { get; set; }

        ICommand CommandCopyToClipboard { get; }

        bool? ShowColumnsTotal { get; set; }

        bool? ShowRowsTotal { get; set; }
    }
}
