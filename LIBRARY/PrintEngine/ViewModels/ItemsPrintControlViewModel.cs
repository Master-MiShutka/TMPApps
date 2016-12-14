using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using TMP.PrintEngine.Paginators;
using TMP.PrintEngine.Utils;
using TMP.PrintEngine.Views;

namespace TMP.PrintEngine.ViewModels
{
    public class ItemsPrintControlViewModel : PrintControlViewModel, IItemsPrintControlViewModel
    {
        public ItemsPrintControlViewModel(PrintControlView view)
            : base(view)
        {
        }

        public List<double> ColumnsWidths { get; set; }
        public List<double> RowHeights { get; set; }
        public PrintTableDefination PrintTableDefination { get; set; }

        protected override void CreatePaginator(DrawingVisual visual, Size printSize, Thickness margin)
        {
            Thickness _margin = margin;
            if (_margin == null)
                _margin = PrintUtility.GetPageMargin(CurrentPrinterName);
            Paginator = new ItemsPaginator(visual, printSize, _margin, PrintTableDefination);
        }
    }
}