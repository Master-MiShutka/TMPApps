namespace TMP.PrintEngine.ViewModels
{
    using System.Collections.Generic;
    using TMP.PrintEngine.Utils;

    public interface IItemsPrintControlViewModel : IPrintControlViewModel
    {
        List<double> ColumnsWidths { get; set; }

        List<double> RowHeights { get; set; }

        PrintTableDefination PrintTableDefination { get; set; }
    }
}