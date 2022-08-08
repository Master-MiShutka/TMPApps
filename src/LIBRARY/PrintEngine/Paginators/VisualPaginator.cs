namespace TMP.PrintEngine.Paginators
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TMP.PrintEngine.Utils;

    public class VisualPaginator : DocumentPaginator
    {
        protected const string DrawingVisualNullMessage = "Drawing Visual Source is null";
        public int HorizontalPageCount;

        private readonly Size printSize;
        protected Thickness PageMargins;
        private readonly Thickness originalMargin;
        protected Size ContentSize;
        protected Pen FramePen;
        protected readonly List<double> AdjustedPageWidths = new List<double>();
        protected readonly List<double> AdjustedPageHeights = new List<double>();
        private int _verticalPageCount;
        protected Rect FrameRect;
        public List<DrawingVisual> DrawingVisuals;
        private IDocumentPaginatorSource _document;
        protected double PrintablePageWidth;
        public DrawingVisual DrawingVisual;
        protected double PrintablePageHeight;
        public bool ShowPageMarkers;
        protected double HeaderHeight;

        public event EventHandler<PageEventArgs> PageCreated;

        public void OnPageCreated(PageEventArgs e)
        {
            EventHandler<PageEventArgs> handler = this.PageCreated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public VisualPaginator(DrawingVisual source, Size printSize, Thickness pageMargins, Thickness originalMargin)
        {
            this.DrawingVisual = source;
            this.printSize = printSize;
            this.PageMargins = pageMargins;
            this.originalMargin = originalMargin;
        }

        public void Initialize(bool isMarkPageNumbers)
        {
            this.ShowPageMarkers = isMarkPageNumbers;
            var totalHorizontalMargin = this.PageMargins.Left + this.PageMargins.Right;
            var toltalVerticalMargin = this.PageMargins.Top + this.PageMargins.Bottom;

            this.PrintablePageWidth = this.PageSize.Width - totalHorizontalMargin;
            this.PrintablePageHeight = this.PageSize.Height - toltalVerticalMargin;

            this.ContentSize = new Size(this.printSize.Width - totalHorizontalMargin, this.printSize.Height - toltalVerticalMargin);
            this.FrameRect = new Rect(new Point(this.PageMargins.Left, this.PageMargins.Top), new Size(this.printSize.Width - totalHorizontalMargin, this.printSize.Height - toltalVerticalMargin));
            this.FramePen = new Pen(Brushes.Black, 0);

            this.HorizontalPageCount = this.GetHorizontalPageCount();

            this._verticalPageCount = this.GetVerticalPageCount();

            this.CreateAllPageVisuals();
        }

        private void CreateAllPageVisuals()
        {
            this.DrawingVisuals = new List<DrawingVisual>();

            for (var verticalPageNumber = 0; verticalPageNumber < this._verticalPageCount; verticalPageNumber++)
            {
                for (var horizontalPageNumber = 0; horizontalPageNumber < this.HorizontalPageCount; horizontalPageNumber++)
                {
                    const float horizontalOffset = 0;
                    var verticalOffset = (float)(verticalPageNumber * this.PrintablePageHeight);
                    var pageBounds = this.GetPageBounds(horizontalPageNumber, verticalPageNumber, horizontalOffset, verticalOffset);
                    var visual = new DrawingVisual();
                    using (var dc = visual.RenderOpen())
                    {
                        this.CreatePageVisual(pageBounds, this.DrawingVisual,
                                         IsFooterPage(horizontalPageNumber), dc);
                    }

                    this.DrawingVisuals.Add(visual);
                }
            }
        }

        protected virtual Rect GetPageBounds(int horizontalPageNumber, int verticalPageNumber, float horizontalOffset, float verticalOffset)
        {
            var x = (float)(horizontalPageNumber * this.PrintablePageWidth);
            return new Rect { X = x, Y = verticalOffset, Size = this.ContentSize };
        }

        private static bool IsFooterPage(int horizontalPageNumber)
        {
            return horizontalPageNumber == 0;
        }

        protected virtual int GetVerticalPageCount()
        {
            int count;
            if (this.IsDrawingNotNull())
            {
                count = (int)Math.Ceiling(this.GetDrawingBounds().Height / this.PrintablePageHeight);
            }
            else
            {
                throw new NullReferenceException(DrawingVisualNullMessage);
            }

            return count;
        }

        protected virtual Rect GetDrawingBounds()
        {
            return this.DrawingVisual.Drawing.Bounds;
        }

        protected virtual bool IsDrawingNotNull()
        {
            return this.DrawingVisual.Drawing != null;
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            DrawingVisual pageVisual = this.GetPageVisual(pageNumber);
            var documentPage = new DocumentPage(pageVisual, this.PageSize, this.FrameRect, this.FrameRect);
            if (this.ShowPageMarkers)
            {
                this.InsertPageMarkers(pageNumber + 1, documentPage);
            }

            this.OnPageCreated(new PageEventArgs(pageNumber));
            return documentPage;
        }

        private DrawingVisual GetPageVisual(int pageNumber)
        {
            var totalHorizontalMargin = this.originalMargin.Left + this.originalMargin.Right;
            var toltalVerticalMargin = this.originalMargin.Top + this.originalMargin.Bottom;
            var printablePageWidth = this.PageSize.Width - totalHorizontalMargin;
            var printablePageHeight = this.PageSize.Height - toltalVerticalMargin - 10;

            var xFactor = printablePageWidth / this.PageSize.Width;
            var yFactor = printablePageHeight / this.PageSize.Height;
            var scaleFactor = Math.Max(xFactor, yFactor);
            var pageVisual = this.DrawingVisuals[pageNumber];
            var transformGroup = new TransformGroup();
            var scaleTransform = new ScaleTransform(scaleFactor, scaleFactor);
            var translateTransform = new TranslateTransform(this.originalMargin.Left, this.originalMargin.Top);
            transformGroup.Children.Add(translateTransform);
            transformGroup.Children.Add(scaleTransform);
            pageVisual.Transform = transformGroup;
            return pageVisual;
        }

        protected virtual void InsertPageMarkers(int pageNumber, DocumentPage documentPage)
        {
            var labelDrawingVisual = new DrawingVisual();
            using (var drawingContext = labelDrawingVisual.RenderOpen())
            {
                var pageNumberContent = pageNumber + "/" + this.PageCount;
                var ft_back = new FormattedText(pageNumberContent,
                                           Thread.CurrentThread.CurrentCulture,
                                           FlowDirection.LeftToRight,
                                           new Typeface("Verdana"),
                                           15, Brushes.White);
                var ft = new FormattedText(pageNumberContent,
                                           Thread.CurrentThread.CurrentCulture,
                                           FlowDirection.LeftToRight,
                                           new Typeface("Verdana"),
                                           15, Brushes.Black);

                drawingContext.DrawText(ft_back, new Point(this.PageMargins.Left, this.PageMargins.Top));
                drawingContext.DrawText(ft, new Point(this.PageMargins.Left + 2, this.PageMargins.Top + 2));
            }

            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(((DrawingVisual)documentPage.Visual).Drawing);
            drawingGroup.Children.Add(labelDrawingVisual.Drawing);

            var currentDrawingVisual = (DrawingVisual)documentPage.Visual;
            using (var currentDrawingContext = currentDrawingVisual.RenderOpen())
            {
                currentDrawingContext.DrawDrawing(drawingGroup);
                currentDrawingContext.PushOpacityMask(Brushes.White);
            }
        }

        public override bool IsPageCountValid => true;

        public override int PageCount => this._verticalPageCount * this.HorizontalPageCount;

        public override sealed Size PageSize
        {
            get => this.printSize;
            set { }
        }

        public override IDocumentPaginatorSource Source => this._document;

        public void UpdatePageMarkers(bool showPageMarkers)
        {
            this.ShowPageMarkers = showPageMarkers;
        }

        public IDocumentPaginatorSource CreateDocumentPaginatorSource()
        {
            var document = new FixedDocument();
            for (var i = 0; i < this.PageCount; i++)
            {
                var page = this.GetPage(i);
                var fp = new FixedPage { ContentBox = this.FrameRect, BleedBox = this.FrameRect, Width = page.Size.Width, Height = page.Size.Height };

                var vb = new DrawingBrush(this.DrawingVisuals[i].Drawing)
                {
                    Stretch = Stretch.Uniform,
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(page.Size),
                };
                var totalHorizontalMargin = this.originalMargin.Left + this.originalMargin.Right;
                var toltalVerticalMargin = this.originalMargin.Top + this.originalMargin.Bottom;
                var printablePageWidth = this.PageSize.Width - totalHorizontalMargin;
                var printablePageHeight = this.PageSize.Height - toltalVerticalMargin - 10;

                var rect = new Rect(this.originalMargin.Left, this.originalMargin.Top, printablePageWidth, printablePageHeight);
                fp.Children.Add(CreateContentRectangle(vb, rect));
                var pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fp);
                document.Pages.Add(pageContent);
            }

            this._document = document;
            this._document.DocumentPaginator.PageSize = new Size(this.PageSize.Width, this.PageSize.Height);
            return this._document;
        }

        public List<FixedDocument> CreateFixedDocumentsForEachPage()
        {
            var documents = new List<FixedDocument>();
            for (var i = 0; i < this.PageCount; i++)
            {
                var document = new FixedDocument();
                var page = this.GetPage(i);
                var fp = new FixedPage { ContentBox = this.FrameRect, BleedBox = this.FrameRect, Width = page.Size.Width, Height = page.Size.Height };
                var vb = new DrawingBrush(this.DrawingVisuals[i].Drawing) { Stretch = Stretch.Uniform, ViewboxUnits = BrushMappingMode.Absolute, Viewbox = new Rect(page.Size) };

                var totalHorizontalMargin = this.originalMargin.Left + this.originalMargin.Right;
                var toltalVerticalMargin = this.originalMargin.Top + this.originalMargin.Bottom;
                var printablePageWidth = this.PageSize.Width - totalHorizontalMargin;
                var printablePageHeight = this.PageSize.Height - toltalVerticalMargin - 10;

                var rect = new Rect(this.originalMargin.Left, this.originalMargin.Top, printablePageWidth, printablePageHeight);
                fp.Children.Add(CreateContentRectangle(vb, rect));
                var pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fp);
                document.Pages.Add(pageContent);
                documents.Add(document);
            }

            return documents;
        }

        public List<FixedDocument> CreateFixedDocumentsForEachPageWithPageNumber(int startPageNumber, double height, string slideName)
        {
            slideName = this.GetSlideNameForEntityChartHeader(slideName);
            var documents = new List<FixedDocument>();
            for (var i = 0; i < this.PageCount; i++)
            {
                var document = new FixedDocument();
                var page = this.GetPage(i);
                var fp = new FixedPage { ContentBox = this.FrameRect, BleedBox = this.FrameRect, Width = page.Size.Width, Height = page.Size.Height };
                var vb = new DrawingBrush(this.DrawingVisuals[i].Drawing) { Stretch = Stretch.Uniform, ViewboxUnits = BrushMappingMode.Absolute, Viewbox = new Rect(page.Size) };

                var totalHorizontalMargin = this.originalMargin.Left + this.originalMargin.Right;
                var toltalVerticalMargin = this.originalMargin.Top + this.originalMargin.Bottom;
                var printablePageWidth = this.PageSize.Width - totalHorizontalMargin;
                var printablePageHeight = height - toltalVerticalMargin - 10;

                var rect = new Rect(this.originalMargin.Left, this.originalMargin.Top + Constants.CsBook.EntityChartPageHeaderSize, printablePageWidth, printablePageHeight);
                fp.Children.Add(CreateContentRectangle(vb, rect));

                this.InsertEntityChartPageHeader(fp, startPageNumber++, slideName, i + 1);

                var pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fp);
                document.Pages.Add(pageContent);
                documents.Add(document);
            }

            return documents;
        }

        private string GetSlideNameForEntityChartHeader(string slideName)
        {
            var entityChartSlideNameMaxSize = this.PageSize.Width - Constants.CsBook.PageNumberTextLength - 10;
            if (slideName.Length > entityChartSlideNameMaxSize)
            {
                return string.Format("{0}...", slideName.Substring(0, (int)entityChartSlideNameMaxSize));
            }

            return slideName;
        }

        private void InsertEntityChartPageHeader(FixedPage fp, int pageNumber, string slideName, int pageIndex)
        {
            var slideNameAvailableWidth = this.PageSize.Width - (2 * Constants.CsBook.PageNumberTextLength);

            var pageNumberLabel = new TextBlock { Text = string.Format("{0} {1}", TMP.PrintEngine.Resources.Strings.Page, pageNumber) };
            FixedPage.SetLeft(pageNumberLabel, this.PageSize.Width - Constants.CsBook.PageNumberTextLength);
            FixedPage.SetTop(pageNumberLabel, 10);
            fp.Children.Add(pageNumberLabel);

            var noOfTotal = string.Format("({0} {1} {2})", pageIndex, TMP.PrintEngine.Resources.Strings.of, this.PageCount);
            const int noOfTotalFontSize = 10;
            var ft1 = new FormattedText(noOfTotal, Thread.CurrentThread.CurrentCulture,
                                       FlowDirection.LeftToRight, new Typeface("Verdana"), noOfTotalFontSize, Brushes.Black);
            var pageNumberTextWidth = ft1.Width;
            var availableSlideNameWidth = slideNameAvailableWidth - pageNumberTextWidth;
            ft1 = new FormattedText(slideName, Thread.CurrentThread.CurrentCulture,
                                       FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Black);
            var slideNameTextWidth = ft1.Width;
            var txtBlockSlideNameWidth = (slideNameTextWidth > availableSlideNameWidth)
                                             ? availableSlideNameWidth
                                             : slideNameTextWidth;
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var txtBlockSlideName = new TextBlock
            {
                Text = slideName,
                Width = Math.Max(0, txtBlockSlideNameWidth),
                FontFamily = new FontFamily("Verdana"),
                FontSize = 20,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            var txtBlockPageNo = new TextBlock
            {
                Text = noOfTotal,
                Width = pageNumberTextWidth,
                FontFamily = new FontFamily("Verdana"),
                FontSize = noOfTotalFontSize,
                TextTrimming = TextTrimming.None,
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Thickness(0, 0, 0, 2),
                Margin = new Thickness(0),
            };
            stackPanel.Children.Add(txtBlockSlideName);
            stackPanel.Children.Add(txtBlockPageNo);
            var label = new Label
            {
                Width = slideNameAvailableWidth,
                Content = stackPanel,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
            };

            FixedPage.SetLeft(label, Constants.CsBook.PageNumberTextLength);
            FixedPage.SetTop(label, 10);
            fp.Children.Add(label);
        }

        private static Rectangle CreateContentRectangle(Brush vb, Rect rect)
        {
            var rc = new Rectangle { Width = rect.Width, Height = rect.Height, Fill = vb };
            FixedPage.SetLeft(rc, rect.X);
            FixedPage.SetTop(rc, rect.Y);
            FixedPage.SetRight(rc, rect.Width);
            FixedPage.SetBottom(rc, rect.Height);
            return rc;
        }

        protected virtual void CreatePageVisual(Rect pageBounds, DrawingVisual source, bool isFooterPage, DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(null, this.FramePen, new Rect { X = this.FrameRect.X, Y = this.FrameRect.Y, Width = this.FrameRect.Width, Height = this.FrameRect.Height });
            var offsetX = this.PageMargins.Left - pageBounds.X - 1;
            var offsetY = this.PageMargins.Top - pageBounds.Y;
            drawingContext.PushTransform(new TranslateTransform(offsetX, offsetY + this.HeaderHeight));
            var pg = new Rect(new Point(pageBounds.X, pageBounds.Y), new Size(pageBounds.Width, pageBounds.Height));
            drawingContext.PushClip(new RectangleGeometry(pg));
            drawingContext.PushOpacityMask(Brushes.White);
            drawingContext.DrawDrawing(source.Drawing);
        }

        protected virtual int GetHorizontalPageCount()
        {
            if (this.IsDrawingNotNull())
            {
                return (int)Math.Ceiling(this.GetDrawingBounds().Width / this.PrintablePageWidth);
            }

            throw new NullReferenceException(DrawingVisualNullMessage);
        }

        public FixedDocument GetDocument(int startIndex, int endIndex)
        {
            var document = new FixedDocument();
            for (var i = startIndex; i < endIndex; i++)
            {
                var fp = new FixedPage { ContentBox = this.FrameRect, BleedBox = this.FrameRect };
                var page = this.GetPage(i);
                var vb = new DrawingBrush(this.DrawingVisuals[i].Drawing) { Stretch = Stretch.Uniform, ViewboxUnits = BrushMappingMode.Absolute, Viewbox = new Rect(page.Size) };
                fp.Children.Add(CreateContentRectangle(vb, this.FrameRect));
                var pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fp);
                document.Pages.Add(pageContent);
            }

            document.DocumentPaginator.PageSize = new Size(this.PageSize.Width, this.PageSize.Height);
            return document;
        }
    }

    public class PageEventArgs : EventArgs
    {
        private readonly int pageNumber;

        public int PageNumber => this.pageNumber;

        public PageEventArgs(int pageNumber)
        {
            this.pageNumber = pageNumber;
        }
    }
}