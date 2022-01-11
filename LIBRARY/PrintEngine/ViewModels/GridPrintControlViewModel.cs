namespace TMP.PrintEngine.ViewModels
{
    using System.Windows;
    using System.Windows.Media;
    using TMP.PrintEngine.Paginators;
    using TMP.PrintEngine.Views;

    public class GridPrintControlViewModel : ItemsPrintControlViewModel, IGridPrintControlViewModel
    {
        public GridPrintControlViewModel(PrintControlView view)
            : base(view)
        {
        }

        protected override void CreatePaginator(DrawingVisual visual, Size printSize, Thickness margin)
        {
            Thickness _margin = margin;
            if (_margin == null)
            {
                _margin = this.PrintUtility.GetPageMargin(this.CurrentPrinterName);
            }

            this.Paginator = new DataGridPaginator(visual, printSize, _margin, this.PrintTableDefination);
        }
    }
}