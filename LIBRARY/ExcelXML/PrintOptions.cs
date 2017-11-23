using System;
using System.Globalization;
using System.Text;
using System.Xml;
using TMP.Extensions;

namespace TMP.ExcelXml
{
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
			get
			{
				return layout;
			}
			set
			{
				layout = value;

				if (!layout.IsValid())
					throw new ArgumentException("Invalid page layout defined");
			}
		}
		/// <summary>
		/// Gets or sets paper size
		/// </summary>
		public PaperSize PaperSize
		{
			get
			{
				return paperSize;
			}
			set
			{
				paperSize = value;

				if (!paperSize.IsValid())
					throw new ArgumentException("Invalid paper size defined");
			}
		}

		private PageOrientation orientation;
		/// <summary>
		/// Gets or sets page orientation
		/// </summary>
		public PageOrientation Orientation
		{
			get
			{
				return orientation;
			}
			set
			{
				orientation = value;

				if (!orientation.IsValid())
					throw new ArgumentException("Invalid page layout defined");
			}
		}
		#endregion

		#region Private and Internal methods
		internal string GetPrintTitleRange(string workSheetName)
		{
			StringBuilder range = new StringBuilder();

			if (PrintTitles)
			{
				if (LeftPrintCol != 0)
				{
					range.AppendFormat("'{0}'!C{1}", workSheetName, LeftPrintCol);

					if (RightPrintCol != LeftPrintCol)
					{
						range.AppendFormat(":C{0}", RightPrintCol);
					}
				}

				if (TopPrintRow != 0)
				{
					if (LeftPrintCol != 0)
						range.Append(',');

					range.AppendFormat("'{0}'!R{1}", workSheetName, TopPrintRow);

					if (BottomPrintRow != TopPrintRow)
					{
						range.AppendFormat(":R{0}", BottomPrintRow);
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
			TopPrintRow = Math.Max(1, top);
			BottomPrintRow = Math.Max(1, Math.Max(top, bottom));

			PrintTitles = true;
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
			LeftPrintCol = Math.Max(1, left);
			RightPrintCol = Math.Max(1, Math.Max(left, right));

			PrintTitles = true;
		}

		/// <summary>
		/// Resets print margins
		/// </summary>
		public void ResetMargins()
		{
			LeftMargin = 0.70;
			RightMargin = 0.70;

			TopMargin = 0.75;
			BottomMargin = 0.75;

			HeaderMargin = 0.30;
			FooterMargin = 0.30;
		}

		/// <summary>
		/// Resets header rows/columns.
		/// </summary>
		public void ResetHeaders()
		{
			TopPrintRow = 0;
			BottomPrintRow = 0;

			LeftPrintCol = 0;
			RightPrintCol = 0;
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
			LeftMargin = Math.Max(0, left);
			TopMargin = Math.Max(0, top);
			RightMargin = Math.Max(0, right);
			BottomMargin = Math.Max(0, bottom);
		}

		/// <summary>
		/// Sets print header and footer margins
		/// </summary>
		/// <param name="header">Header margin</param>
		/// <param name="footer">Footer margin</param>
		public void SetHeaderFooterMargins(double header, double footer)
		{
			HeaderMargin = Math.Max(0, header);
			FooterMargin = Math.Max(0, footer);
		}

		/// <summary>
		/// Sets excel's fit to page property
		/// </summary>
		/// <param name="width">Number of pages to fit the page horizontally</param>
		/// <param name="height">Number of pages to fit the page vertically</param>
		public void SetFitToPage(int width, int height)
		{
			FitWidth = width;
			FitHeight = height;

			FitToPage = true;
		}

		/// <summary>
		/// Sets excel's scale or zoom property
		/// </summary>
		/// <param name="scale">Scale to size</param>
		public void SetScaleToSize(int scale)
		{
			Scale = scale;

			FitToPage = false;
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			// PageSetup
			writer.WriteStartElement("PageSetup");
			// Layout
			if (Orientation != PageOrientation.None)
			{
				writer.WriteStartElement("Layout");
				writer.WriteAttributeString("", "Orientation", null, Orientation.ToString());
				writer.WriteEndElement();
			}
			// Header
			writer.WriteStartElement("Header");
			writer.WriteAttributeString("", "Margin", null, HeaderMargin.ToString(
				CultureInfo.InvariantCulture));
			writer.WriteEndElement();
			// Footer
			writer.WriteStartElement("Footer");
			writer.WriteAttributeString("", "Margin", null, FooterMargin.ToString(
				CultureInfo.InvariantCulture));
			writer.WriteEndElement();
			// Pagemargins
			writer.WriteStartElement("PageMargins");
			writer.WriteAttributeString("", "Bottom", null, BottomMargin.ToString(
				CultureInfo.InvariantCulture));
			writer.WriteAttributeString("", "Left", null, LeftMargin.ToString(
				CultureInfo.InvariantCulture));
			writer.WriteAttributeString("", "Right", null, RightMargin.ToString(
				CultureInfo.InvariantCulture));
			writer.WriteAttributeString("", "Top", null, TopMargin.ToString(
				CultureInfo.InvariantCulture));
			writer.WriteEndElement();
			writer.WriteEndElement();

			// Fit to page?
			if (FitToPage)
			{
				writer.WriteStartElement("FitToPage");
				writer.WriteEndElement();
			}

			// Print options
			writer.WriteStartElement("Print");
            // Paper size
            writer.WriteElementString("PaperSizeIndex", ((byte)PaperSize).ToString());
            writer.WriteElementString("ValidPrinterInfo", "");

			writer.WriteElementString("FitHeight", FitHeight.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("FitWidth", FitWidth.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Scale", Scale.ToString(CultureInfo.InvariantCulture));

			writer.WriteEndElement();
		}
		#endregion
	}
}