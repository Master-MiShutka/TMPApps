using System.Windows;
using System.Windows.Media;

using TMP.PrintEngine.Paginators;
using TMP.PrintEngine.Views;

namespace TMP.PrintEngine.ViewModels
{
    public class DataTablePrintControlViewModel : ItemsPrintControlViewModel, IDataTablePrintControlViewModel
    {
        public DataTablePrintControlViewModel(PrintControlView view)
            : base(view)
        {
        }

        protected override void CreatePaginator(DrawingVisual visual, Size printSize, Thickness margin)
        {
            Thickness _margin = margin;
            if (_margin == null)
                _margin = PrintUtility.GetPageMargin(CurrentPrinterName);
            Paginator = new DataTablePaginator(visual, printSize, _margin, PrintTableDefination);
        }
    }
}