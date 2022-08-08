namespace TMP.UI.WPF.Reporting.MatrixGrid
{
    using System.Collections.Generic;
    using System.Windows.Input;

    public interface IMatrix
    {
        string Id { get; set; }

        bool IsBuilded { get; }

        bool IsBuilding { get; }

        event MatrixBuildedEventHandler Builded;

        event MatrixEventHandler Building;

        IList<IMatrixCell> Items { get; }

        IMatrixCell[,] Cells { get; }

        int RowHeadersCount { get; }

        int ColumnHeadersCount { get; }

        System.Windows.Size Size { get; }

        string Header { get; set; }

        string Description { get; set; }

        ICommand CommandCopyToClipboard { get; }

        bool ShowColumnsTotal { get; set; }

        bool ShowRowsTotal { get; set; }
    }
}
