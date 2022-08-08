namespace TMP.PrintEngine.Paginators
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Markup;
    using System.Windows.Media;
    using TMP.PrintEngine.Utils;

    public class ItemsPaginator : VisualPaginator
    {
        protected PrintTableDefination PrintTableDefination;
        protected IList<int> ColumnCount;

        public ItemsPaginator(DrawingVisual source, Size printSize, Thickness pageMargins, PrintTableDefination printTableDefination)
            : base(source, printSize, pageMargins, pageMargins)
        {
            this.PrintTableDefination = printTableDefination;
            this.ColumnCount = new List<int>();
            this.CalculateHeaderHeight();
        }

        private void CalculateHeaderHeight()
        {
            var header = XamlReader.Parse(this.PrintTableDefination.HeaderTemplate) as FrameworkElement;
            UiUtil.UpdateSize(header, this.PrintablePageWidth);
            this.HeaderHeight = header.ActualHeight + this.PageMargins.Top + this.PrintTableDefination.ColumnHeaderHeight;
        }

        protected override int GetVerticalPageCount()
        {
            var pageCountY = 0;
            double totalHeight = 0;
            double lastTotalHeight = 0;
            for (var i = 0; i < this.PrintTableDefination.RowHeights.Count; i++)
            {
                lastTotalHeight = totalHeight + this.PrintTableDefination.RowHeights[i];
                if (totalHeight + this.PrintTableDefination.RowHeights[i] <= this.PrintablePageHeight - this.HeaderHeight)
                {
                    totalHeight += this.PrintTableDefination.RowHeights[i];
                }
                else
                {
                    pageCountY++;
                    this.AdjustedPageHeights.Add(totalHeight);
                    totalHeight = 0;
                    i--;
                }
            }

            this.AdjustedPageHeights.Add(lastTotalHeight);
            return pageCountY + 1;
        }

        protected override Rect GetPageBounds(int horizontalPageNumber, int verticalPageNumber, float horizontalOffset, float verticalOffset)
        {
            verticalOffset = 0;
            for (var i = 0; i < horizontalPageNumber; i++)
            {
                horizontalOffset += (float)this.GetPageWidth(i);
            }

            for (var j = 0; j < verticalPageNumber; j++)
            {
                verticalOffset += (float)this.AdjustedPageHeights[j];
            }

            var pageBounds = new Rect
            {
                X = horizontalOffset,
                Y = verticalOffset,
                Size = new Size(this.GetPageWidth(horizontalPageNumber) + 2, this.AdjustedPageHeights[verticalPageNumber] + 2),
            };
            return pageBounds;
        }

        protected virtual double GetPageWidth(int pageNumber)
        {
            return this.PrintablePageWidth;
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            var page = base.GetPage(pageNumber);
            var headerVisual = new DrawingVisual();
            using (var drawingContext = headerVisual.RenderOpen())
            {
                var rowNumber = pageNumber % this.HorizontalPageCount;
                var contentWidth = this.GetPageWidth(rowNumber);

                this.CreateHeader(rowNumber, drawingContext, pageNumber + 1);
                if (this.PrintTableDefination.HasFooter)
                {
                    var text3 = new FormattedText(this.PrintTableDefination.FooterText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Black);
                    drawingContext.DrawText(text3, new Point(this.PageMargins.Left, this.PageSize.Height - this.PageMargins.Bottom - text3.Height));
                }

                var contentTop = this.PageMargins.Top + this.HeaderHeight;
                var gridLineBrush = Brushes.Gray;
                const double gridLineThickness = 0.5;
                var gridLinePen = new Pen(gridLineBrush, gridLineThickness);

                if (this.PrintTableDefination.ColumnNames != null)
                {
                    drawingContext.DrawRectangle(Brushes.Transparent, gridLinePen, new Rect(this.PageMargins.Left - 1, contentTop - this.PrintTableDefination.ColumnHeaderHeight, contentWidth, this.PrintTableDefination.ColumnHeaderHeight));
                    drawingContext.DrawRectangle(gridLineBrush, gridLinePen, new Rect(this.PageMargins.Left - 1, contentTop - 2, contentWidth, 2));

                    var cumilativeColumnNumber = 0;
                    var columnLeft = this.PageMargins.Left - 1;
                    var currentPageColumns = rowNumber == this.HorizontalPageCount - 1 ? this.ColumnCount[rowNumber] - 1 : this.ColumnCount[rowNumber];
                    for (int j = 0; j < rowNumber; j++)
                    {
                        cumilativeColumnNumber += this.ColumnCount[j];
                    }

                    for (int i = cumilativeColumnNumber; i < cumilativeColumnNumber + currentPageColumns; i++)
                    {
                        var columnWidth = this.PrintTableDefination.ColumnWidths[i];
                        var colName = this.PrintTableDefination.ColumnNames[i];
                        if (colName == string.Empty)
                        {
                            drawingContext.DrawRectangle(gridLineBrush, gridLinePen, new Rect(columnLeft, contentTop - this.PrintTableDefination.ColumnHeaderHeight, columnWidth, this.PrintTableDefination.ColumnHeaderHeight));
                        }
                        else
                        {
                            var columnName = new FormattedText(colName, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.PrintTableDefination.ColumnHeaderFontSize, this.PrintTableDefination.ColumnHeaderBrush)
                            {
                                MaxTextWidth = columnWidth,
                                MaxLineCount = 1,
                                Trimming = TextTrimming.CharacterEllipsis,
                            };
                            drawingContext.DrawText(columnName, new Point(columnLeft + 5, contentTop - this.PrintTableDefination.ColumnHeaderHeight));
                        }

                        columnLeft += columnWidth;
                        drawingContext.DrawRectangle(gridLineBrush, gridLinePen, new Rect(columnLeft, contentTop - this.PrintTableDefination.ColumnHeaderHeight, gridLineThickness, this.PrintTableDefination.ColumnHeaderHeight));
                    }

                    if (rowNumber == this.HorizontalPageCount - 1)
                    {
                        var columnWidth =
                            this.PrintTableDefination.ColumnWidths[cumilativeColumnNumber + this.ColumnCount[rowNumber] - 1];
                        var colName =
                            this.PrintTableDefination.ColumnNames[cumilativeColumnNumber + this.ColumnCount[rowNumber] - 1];
                        if (colName == string.Empty)
                        {
                            drawingContext.DrawRectangle(gridLineBrush, gridLinePen, new Rect(columnLeft, contentTop - this.PrintTableDefination.ColumnHeaderHeight - 2, columnWidth, this.PrintTableDefination.ColumnHeaderHeight));
                        }
                        else
                        {
                            var columnName = new FormattedText(colName, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.PrintTableDefination.ColumnHeaderFontSize, Brushes.Black)
                            {
                                MaxTextWidth = columnWidth,
                                MaxLineCount = 1,
                                Trimming = TextTrimming.CharacterEllipsis,
                            };
                            drawingContext.DrawText(columnName, new Point(columnLeft + 5, contentTop - this.PrintTableDefination.ColumnHeaderHeight));
                        }
                    }
                }

                if (this.ShowPageMarkers)
                {
                    var pageNumberText = new FormattedText(string.Format("{0}", pageNumber + 1), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, gridLineBrush);
                    drawingContext.DrawText(pageNumberText, new Point(this.PageMargins.Left + 5, this.PrintablePageHeight - pageNumberText.Height + 15));
                }

                drawingContext.PushOpacityMask(Brushes.White);
            }

            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(((DrawingVisual)page.Visual).Drawing);
            drawingGroup.Children.Add(headerVisual.Drawing);

            var currentDrawingVisual = (DrawingVisual)page.Visual;
            currentDrawingVisual.Transform = new TranslateTransform(this.PageMargins.Left, this.PageMargins.Top);

            var currentDrawingContext = currentDrawingVisual.RenderOpen();
            currentDrawingContext.DrawDrawing(drawingGroup);
            currentDrawingContext.PushOpacityMask(Brushes.White);
            currentDrawingContext.Close();
            var documentPage = new DocumentPage(currentDrawingVisual, this.PageSize, this.FrameRect, this.FrameRect);
            this.OnGetPageCompleted(new GetPageCompletedEventArgs(documentPage, pageNumber, null, false, null));
            return documentPage;
        }

        private void CreateHeader(int index, DrawingContext drawingContext, int pageNumber)
        {
            if (!string.IsNullOrEmpty(this.PrintTableDefination.HeaderTemplate) && index == 0)
            {
                var headerTemplate = this.PrintTableDefination.HeaderTemplate.Replace("@PageNumber", pageNumber.ToString());
                var header = XamlReader.Parse(headerTemplate) as FrameworkElement;
                header.Width = this.PrintablePageWidth;
                UiUtil.UpdateSize(header, this.PrintablePageWidth);
                drawingContext.DrawRectangle(new VisualBrush(header) { Stretch = Stretch.None }, null, new Rect(this.PageMargins.Left, this.PageMargins.Top, this.PrintablePageWidth, header.ActualHeight));
            }
        }

        protected override void InsertPageMarkers(int pageNumber, DocumentPage documentPage)
        {
        }
    }
}