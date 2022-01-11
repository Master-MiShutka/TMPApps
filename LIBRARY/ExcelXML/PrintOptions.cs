namespace TMP.ExcelXml
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using TMP.Extensions;

    /// <summary>
    /// Gets or sets various sheet printing options
    /// </summary>
    public class PrintOptions
    {
        #region Private and Internal fields
        internal double LeftMargin;
        internal double RightMargin;
        internal double TopMargin;
        internal double BottomMargin;
        internal double HeaderMargin;
        internal double FooterMargin;

        internal bool FitToPage;

        internal int Scale;
        internal int FitHeight;
        internal int FitWidth;

        internal int TopPrintRow;
        internal int BottomPrintRow;

        internal int LeftPrintCol;
        internal int RightPrintCol;

        internal bool PrintTitles;
        #endregion

        #region Public Properties
        private PageLayout layout;
        private PaperSize paperSize = PaperSize.A4;

        /// <summary>
        /// Gets or sets page layout
        /// </summary>
        public PageLayout Layout
        {
            get => this.layout;

            set
            {
                this.layout = value;

                if (!this.layout.IsValid())
                {
                    throw new ArgumentException("Invalid page layout defined");
                }
            }
        }

        /// <summary>
        /// Gets or sets paper size
        /// </summary>
        public PaperSize PaperSize
        {
            get => this.paperSize;

            set
            {
                this.paperSize = value;

                if (!this.paperSize.IsValid())
                {
                    throw new ArgumentException("Invalid paper size defined");
                }
            }
        }

        private PageOrientation orientation;

        /// <summary>
        /// Gets or sets page orientation
        /// </summary>
        public PageOrientation Orientation
        {
            get => this.orientation;

            set
            {
                this.orientation = value;

                if (!this.orientation.IsValid())
                {
                    throw new ArgumentException("Invalid page layout defined");
                }
            }
        }
        #endregion

        #region Private and Internal methods
        internal string GetPrintTitleRange(string workSheetName)
        {
            StringBuilder range = new StringBuilder();

            if (this.PrintTitles)
            {
                if (this.LeftPrintCol != 0)
                {
                    range.AppendFormat("'{0}'!C{1}", workSheetName, this.LeftPrintCol);

                    if (this.RightPrintCol != this.LeftPrintCol)
                    {
                        range.AppendFormat(":C{0}", this.RightPrintCol);
                    }
                }

                if (this.TopPrintRow != 0)
                {
                    if (this.LeftPrintCol != 0)
                    {
                        range.Append(',');
                    }

                    range.AppendFormat("'{0}'!R{1}", workSheetName, this.TopPrintRow);

                    if (this.BottomPrintRow != this.TopPrintRow)
                    {
                        range.AppendFormat(":R{0}", this.BottomPrintRow);
                    }
                }
            }

            return range.ToString();
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Sets print header rows which are repeated at top on every page
        /// </summary>
        /// <param name="top">Top print row</param>
        /// <param name="bottom">Bottom print row</param>
        /// <remarks>Important Note: Top and bottom row parameters are <b>NOT</b> zero based like row
        /// and column indexers</remarks>
        public void SetTitleRows(int top, int bottom)
        {
            this.TopPrintRow = Math.Max(1, top);
            this.BottomPrintRow = Math.Max(1, Math.Max(top, bottom));

            this.PrintTitles = true;
        }

        /// <summary>
        /// Sets print header columns which are repeated at left on every page
        /// </summary>
        /// <param name="left">Left print column</param>
        /// <param name="right">Right print column</param>
        /// <remarks>Important Note: Left and right column parameters are <b>NOT</b> zero based like row
        /// and column indexers</remarks>
        public void SetTitleColumns(int left, int right)
        {
            this.LeftPrintCol = Math.Max(1, left);
            this.RightPrintCol = Math.Max(1, Math.Max(left, right));

            this.PrintTitles = true;
        }

        /// <summary>
        /// Resets print margins
        /// </summary>
        public void ResetMargins()
        {
            this.LeftMargin = 0.70;
            this.RightMargin = 0.70;

            this.TopMargin = 0.75;
            this.BottomMargin = 0.75;

            this.HeaderMargin = 0.30;
            this.FooterMargin = 0.30;
        }

        /// <summary>
        /// Resets header rows/columns.
        /// </summary>
        public void ResetHeaders()
        {
            this.TopPrintRow = 0;
            this.BottomPrintRow = 0;

            this.LeftPrintCol = 0;
            this.RightPrintCol = 0;
        }

        /// <summary>
        /// Sets print margins
        /// </summary>
        /// <param name="left">Left margin</param>
        /// <param name="top">Top margin</param>
        /// <param name="right">Right margin</param>
        /// <param name="bottom">Bottom margin</param>
        public void SetMargins(double left, double top, double right, double bottom)
        {
            this.LeftMargin = Math.Max(0, left);
            this.TopMargin = Math.Max(0, top);
            this.RightMargin = Math.Max(0, right);
            this.BottomMargin = Math.Max(0, bottom);
        }

        /// <summary>
        /// Sets print header and footer margins
        /// </summary>
        /// <param name="header">Header margin</param>
        /// <param name="footer">Footer margin</param>
        public void SetHeaderFooterMargins(double header, double footer)
        {
            this.HeaderMargin = Math.Max(0, header);
            this.FooterMargin = Math.Max(0, footer);
        }

        /// <summary>
        /// Sets excel's fit to page property
        /// </summary>
        /// <param name="width">Number of pages to fit the page horizontally</param>
        /// <param name="height">Number of pages to fit the page vertically</param>
        public void SetFitToPage(int width, int height)
        {
            this.FitWidth = width;
            this.FitHeight = height;

            this.FitToPage = true;
        }

        /// <summary>
        /// Sets excel's scale or zoom property
        /// </summary>
        /// <param name="scale">Scale to size</param>
        public void SetScaleToSize(int scale)
        {
            this.Scale = scale;

            this.FitToPage = false;
        }
        #endregion

        #region Export
        internal void Export(XmlWriter writer)
        {
            // PageSetup
            writer.WriteStartElement("PageSetup");

            // Layout
            if (this.Orientation != PageOrientation.None)
            {
                writer.WriteStartElement("Layout");
                writer.WriteAttributeString(string.Empty, "Orientation", null, this.Orientation.ToString());
                writer.WriteEndElement();
            }

            // Header
            writer.WriteStartElement("Header");
            writer.WriteAttributeString(string.Empty, "Margin", null, this.HeaderMargin.ToString(
                CultureInfo.InvariantCulture));
            writer.WriteEndElement();

            // Footer
            writer.WriteStartElement("Footer");
            writer.WriteAttributeString(string.Empty, "Margin", null, this.FooterMargin.ToString(
                CultureInfo.InvariantCulture));
            writer.WriteEndElement();

            // Pagemargins
            writer.WriteStartElement("PageMargins");
            writer.WriteAttributeString(string.Empty, "Bottom", null, this.BottomMargin.ToString(
                CultureInfo.InvariantCulture));
            writer.WriteAttributeString(string.Empty, "Left", null, this.LeftMargin.ToString(
                CultureInfo.InvariantCulture));
            writer.WriteAttributeString(string.Empty, "Right", null, this.RightMargin.ToString(
                CultureInfo.InvariantCulture));
            writer.WriteAttributeString(string.Empty, "Top", null, this.TopMargin.ToString(
                CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Fit to page?
            if (this.FitToPage)
            {
                writer.WriteStartElement("FitToPage");
                writer.WriteEndElement();
            }

            // Print options
            writer.WriteStartElement("Print");

            // Paper size
            writer.WriteElementString("PaperSizeIndex", ((byte)this.PaperSize).ToString());
            writer.WriteElementString("ValidPrinterInfo", string.Empty);

            writer.WriteElementString("FitHeight", this.FitHeight.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("FitWidth", this.FitWidth.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Scale", this.Scale.ToString(CultureInfo.InvariantCulture));

            writer.WriteEndElement();
        }
        #endregion
    }
}