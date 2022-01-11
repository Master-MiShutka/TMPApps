namespace TMP.PrintEngine.Paginators
{
    using System.Windows;
    using System.Windows.Media;
    using TMP.PrintEngine.Utils;

    public class DataTablePaginator : ItemsPaginator
    {
        public DataTablePaginator(DrawingVisual source, Size printSize, Thickness pageMargins, PrintTableDefination printTableDefination)
            : base(source, printSize, pageMargins, printTableDefination)
        {
        }

        protected override int GetHorizontalPageCount()
        {
            var pageCountX = 0;
            double totalWidth = 0;
            double lastTotalWidth = 0;
            int columnCount = 0;
            for (var i = 0; i < this.PrintTableDefination.ColumnWidths.Count; i++)
            {
                lastTotalWidth = totalWidth + this.PrintTableDefination.ColumnWidths[i];
                if (totalWidth + this.PrintTableDefination.ColumnWidths[i] <= this.PrintablePageWidth)
                {
                    totalWidth += this.PrintTableDefination.ColumnWidths[i];
                    columnCount++;
                }
                else
                {
                    pageCountX++;
                    this.ColumnCount.Add(columnCount);
                    this.AdjustedPageWidths.Add(totalWidth);
                    columnCount = 0;
                    totalWidth = 0;
                    i--;
                }
            }

            this.ColumnCount.Add(columnCount);
            this.AdjustedPageWidths.Add(lastTotalWidth);
            return pageCountX + 1;
        }

        protected override double GetPageWidth(int pageNumber)
        {
            return this.AdjustedPageWidths[pageNumber];
        }
    }
}