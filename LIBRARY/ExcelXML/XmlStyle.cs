using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Xml;
using TMP.Extensions;

namespace TMP.ExcelXml
{
	/// <summary>
	/// Gets or sets cell's font options
	/// </summary>
	public class FontOptions : IFontOptions
	{
		#region Public Properties
		/// <summary>
		/// Gets or sets the name of the font
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the size of the font
		/// </summary>
		public int Size { get; set; }
		/// <summary>
		/// Gets or sets font's bold property
		/// </summary>
		public bool Bold { get; set; }
		/// <summary>
		/// Gets or sets font's underline property
		/// </summary>
		public bool Underline { get; set; }
		/// <summary>
		/// Gets or sets font's italic property
		/// </summary>
		public bool Italic { get; set; }
		/// <summary>
		/// Gets or sets font's strike-through property
		/// </summary>
		public bool Strikeout { get; set; }
		/// <summary>
		/// Gets or sets font's color
		/// </summary>
		public Color Color { get; set; }
		#endregion

		#region Constructor
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public FontOptions()
		{
			Name = "Calibri";
			Size = 0;
			Bold = false;
			Underline = false;
			Italic = false;
			Strikeout = false;
			Color = Color.Black;
		}

		/// <summary>
		/// Creates a new instance based on another instance
		/// </summary>
		/// <param name="fo">Instance to copy</param>
		public FontOptions(IFontOptions fo)
		{
			Name = fo.Name;
			Size = fo.Size;
			Bold = fo.Bold;
			Underline = fo.Underline;
			Italic = fo.Italic;
			Strikeout = fo.Strikeout;
			Color = fo.Color;
		}
		#endregion

		#region Private and Internal methods
		internal bool CheckForMatch(FontOptions other)
		{
			return (Name == other.Name &&
					Size == other.Size &&
					Bold == other.Bold &&
					Underline == other.Underline &&
					Italic == other.Italic &&
					Strikeout == other.Strikeout &&
					Color == other.Color);
		}
		#endregion

		#region Import
		internal void Import(XmlReader reader)
		{
			foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
			{
				switch (xa.LocalName)
				{
					case "FontName":
						{
							Name = xa.Value;

							break;
						}
					case "Size":
						{
							int i;
							if (xa.Value.ParseToInt(out i))
								Size = i;

							break;
						}
					case "Color":
						{
							Color = XmlStyle.ExcelFormatToColor(xa.Value);

							break;
						}
					case "Bold":
						{
							Bold = xa.Value == "1" ? true : false;

							break;
						}
					case "Italic":
						{
							Italic = xa.Value == "1" ? true : false;

							break;
						}
					case "Underline":
						{
							Underline = xa.Value == "Single" ? true : false;

							break;
						}
					case "Strikeout":
						{
							Strikeout = xa.Value == "1" ? true : false;

							break;
						}
				}
			}
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			// Font
			writer.WriteStartElement("Font");
			writer.WriteAttributeString("ss", "FontName", null, Name);
			if (Size != 0)
				writer.WriteAttributeString("ss", "Size", null, Size.ToString(
					CultureInfo.InvariantCulture));
			// Color
			writer.WriteAttributeString("ss", "Color", null, XmlStyle.ColorToExcelFormat(Color));
			// Bold?
			if (Bold)
				writer.WriteAttributeString("ss", "Bold", null, "1");
			// Italic?
			if (Italic)
				writer.WriteAttributeString("ss", "Italic", null, "1");
			// Underline?
			if (Underline)
				writer.WriteAttributeString("ss", "Underline", null, "Single");
			if (Strikeout)
				writer.WriteAttributeString("ss", "Strikeout", null, "1");
			// Font end
			writer.WriteEndElement();
		}
		#endregion
	}

	/// <summary>
	/// Gets or sets cell's interior options
	/// </summary>
	public class InteriorOptions : IInteriorOptions
	{
		#region Public Properties
		/// <summary>
		/// Gets or sets cell background color
		/// </summary>
		public Color Color { get; set; }
		/// <summary>
		/// Gets or sets cell pattern color
		/// </summary>
		public Color PatternColor { get; set; }
		/// <summary>
		/// Gets or sets cell pattern
		/// </summary>
		public Pattern Pattern { get; set; }
		#endregion

		#region Constructor
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public InteriorOptions()
		{
			Color = Color.Empty;
			PatternColor = Color.Empty;
			Pattern = Pattern.Solid;
		}

		/// <summary>
		/// Creates a new instance based on another instance
		/// </summary>
		/// <param name="io">Instance to copy</param>
		public InteriorOptions(IInteriorOptions io)
		{
			Color = io.Color;
			PatternColor = io.PatternColor;
			Pattern = io.Pattern;
		}
		#endregion

		#region Private and Internal methods
		internal bool CheckForMatch(InteriorOptions other)
		{
			return (Color == other.Color &&
					PatternColor == other.PatternColor &&
					Pattern == other.Pattern);					
		}
		#endregion

		#region Import
		internal void Import(XmlReader reader)
		{
			foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
			{
				switch (xa.LocalName)
				{
					case "Color":
						{
							Color = XmlStyle.ExcelFormatToColor(xa.Value);

							break;
						}
					case "PatternColor":
						{
							PatternColor = XmlStyle.ExcelFormatToColor(xa.Value);

							break;
						}
					case "Pattern":
						{
							Pattern = ObjectExtensions.ParseEnum<Pattern>(xa.Value);

							break;
						}
				}
			}
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			if (Color != Color.Empty || PatternColor != Color.Empty)
			{
				// Interior start
				writer.WriteStartElement("Interior");

				if (Color != Color.Empty)
					writer.WriteAttributeString("ss", "Color", null, XmlStyle.ColorToExcelFormat(Color));

				if (PatternColor != Color.Empty)
					writer.WriteAttributeString("ss", "PatternColor", null, XmlStyle.ColorToExcelFormat(PatternColor));

				writer.WriteAttributeString("ss", "Pattern", null, Pattern.ToString());

				// Interior end
				writer.WriteEndElement();
			}
		}
		#endregion
	}

	/// <summary>
	/// Gets or sets cell's alignment options
	/// </summary>
	public class AlignmentOptions : IAlignmentOptions
	{
		#region Public Properties
		private VerticalAlignment vertical;
		/// <summary>
		/// Gets or sets vertical alignment of the cell
		/// </summary>
		public VerticalAlignment Vertical
		{
			get
			{
				return vertical;
			}
			set
			{
				vertical = value;

				if (!vertical.IsValid())
					throw new ArgumentException("Invalid vertical alignment value encountered");
			}
		}

		private HorizontalAlignment horizontal;
		/// <summary>
		/// Gets or sets horizontal alignment of the cell
		/// </summary>
		public HorizontalAlignment Horizontal
		{
			get
			{
				return horizontal;
			}
			set
			{
				horizontal = value;

				if (!horizontal.IsValid())
					throw new ArgumentException("Invalid horizontal alignment value encountered");
			}
		}

		/// <summary>
		/// Gets or sets the indent
		/// </summary>
		public int Indent { get; set; }
		/// <summary>
		/// Gets or sets the text rotation
		/// </summary>
		public int Rotate { get; set; }

		/// <summary>
		/// Gets or sets text wrap setting
		/// </summary>
		public bool WrapText { get; set; }
		/// <summary>
		/// Gets or sets cell's shrink to cell setting
		/// </summary>
		public bool ShrinkToFit { get; set; }
		#endregion

		#region Constructor
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public AlignmentOptions()
		{
			horizontal = HorizontalAlignment.None;
			vertical = VerticalAlignment.None;

			Indent = 0;
			Rotate = 0;

			WrapText = false;
			ShrinkToFit = false;
		}

		/// <summary>
		/// Creates a new instance based on another instance
		/// </summary>
		/// <param name="ao">Instance to copy</param>
		public AlignmentOptions(IAlignmentOptions ao)
		{
			horizontal = ao.Horizontal;
			vertical = ao.Vertical;

			Indent = ao.Indent;
			Rotate = ao.Rotate;

			WrapText = ao.WrapText;
			ShrinkToFit = ao.ShrinkToFit;
		}
		#endregion

		#region Private and Internal methods
		internal bool CheckForMatch(AlignmentOptions other)
		{
			return (Vertical == other.Vertical &&
					Horizontal == other.Horizontal &&
					Indent == other.Indent &&
					Rotate == other.Rotate &&
					WrapText == other.WrapText &&
					ShrinkToFit == other.ShrinkToFit);
		}
		#endregion

		#region Import
		internal void Import(XmlReader reader)
		{
			foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
			{
				switch (xa.LocalName)
				{
					case "Vertical":
						{
							Vertical = ObjectExtensions.ParseEnum<VerticalAlignment>(xa.Value);

							break;
						}
					case "Horizontal":
						{
							Horizontal = ObjectExtensions.ParseEnum<HorizontalAlignment>(xa.Value);

							break;
						}
					case "WrapText":
						{
							WrapText = xa.Value == "1" ? true : false;

							break;
						}
					case "ShrinkToFit":
						{
							ShrinkToFit = xa.Value == "1" ? true : false;

							break;
						}
					case "Indent":
						{
							int i;
							if (xa.Value.ParseToInt(out i))
								Indent = i;

							break;
						}
					case "Rotate":
						{
							int i;
							if (xa.Value.ParseToInt(out i))
								Rotate = i;

							break;
						}
				}
			}
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			// Alignment
			writer.WriteStartElement("Alignment");

			if (Vertical != VerticalAlignment.None)
				writer.WriteAttributeString("ss", "Vertical", null, Vertical.ToString());
			if (Horizontal != HorizontalAlignment.None)
				writer.WriteAttributeString("ss", "Horizontal", null, Horizontal.ToString());

			if (WrapText)
				writer.WriteAttributeString("ss", "WrapText", null, "1");
			if (ShrinkToFit)
				writer.WriteAttributeString("ss", "ShrinkToFit", null, "1");
			if (Indent > 0)
				writer.WriteAttributeString("ss", "Indent", null, Indent.ToString(
					CultureInfo.InvariantCulture));
			if (Rotate > 0)
				writer.WriteAttributeString("ss", "Rotate", null, Rotate.ToString(
					CultureInfo.InvariantCulture));

			// End Alignment
			writer.WriteEndElement();
		}
		#endregion
	}

	/// <summary>
	/// Gets or sets the border options
	/// </summary>
	public class BorderOptions : IBorderOptions
	{
		#region Public Properties
		private BorderSides sides;
		/// <summary>
		/// Gets or sets the border side flags
		/// </summary>
		public BorderSides Sides
		{
			get
			{
				return sides;
			}
			set
			{
				sides = value;

				if (!sides.IsValid())
					throw new ArgumentException("Invalid Border side value encountered");
			}
		}

		private Borderline lineStyle;
		/// <summary>
		/// Gets or sets the border line style
		/// </summary>
		public Borderline LineStyle
		{
			get
			{
				return lineStyle;
			}
			set
			{
				lineStyle = value;

				if (!lineStyle.IsValid())
					throw new ArgumentException("Invalid line style value encountered");
			}
		}

		/// <summary>
		/// Gets or sets the width of the border
		/// </summary>
		public int Weight { get; set; }
		/// <summary>
		/// Gets or sets border color
		/// </summary>
		public Color Color { get; set; }
		#endregion

		#region Constructor
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public BorderOptions()
		{
			sides = BorderSides.None;
			lineStyle = Borderline.Continuous;
			Weight = 1;
			Color = Color.Black;
		}

		/// <summary>
		/// Creates a new instance based on another instance
		/// </summary>
		/// <param name="borderOptions">Instance to copy</param>
		public BorderOptions(IBorderOptions borderOptions)
		{
			sides = borderOptions.Sides;
			lineStyle = borderOptions.LineStyle;
			Weight = borderOptions.Weight;
			Color = borderOptions.Color;
		}
		#endregion

		#region Private and Internal methods
		internal bool CheckForMatch(BorderOptions other)
		{
			return (Sides == other.Sides &&
					LineStyle == other.LineStyle &&
					Weight == other.Weight &&
					Color == other.Color);

		}
		#endregion

		#region Import
		internal void Import(XmlReader reader)
		{
			if (reader.IsEmptyElement)
				return;

			while (reader.Read() && !(reader.Name == "Borders" && reader.NodeType == XmlNodeType.EndElement))
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.Name == "Border")
					{
						foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
						{
							switch (xa.LocalName)
							{
								case "Position":
									{
										Sides |= ObjectExtensions.ParseEnum<BorderSides>(xa.Value);

										break;
									}
								case "LineStyle":
									{
										LineStyle = ObjectExtensions.ParseEnum<Borderline>(xa.Value);

										break;
									}
								case "Weight":
									{
										int i;
										if (xa.Value.ParseToInt(out i))
											Weight = i;

										break;
									}
							}
						}
					}
				}
			}
		}
		#endregion

		#region Export
		private void ExportBorder(XmlWriter writer, string border)
		{
			writer.WriteStartElement("Border");
			writer.WriteAttributeString("ss", "Position", null, border);
			writer.WriteAttributeString("ss", "LineStyle", null, LineStyle.ToString());
			writer.WriteAttributeString("ss", "Weight", null, Weight.ToString(
					CultureInfo.InvariantCulture));

			if (Color != Color.Black)
				writer.WriteAttributeString("ss", "Color", null, XmlStyle.ColorToExcelFormat(Color));

			writer.WriteEndElement();
		}

		internal void Export(XmlWriter writer)
		{
			if (Sides != BorderSides.None)
			{
				// Border start
				writer.WriteStartElement("Borders");
				if (Sides.IsFlagSet(BorderSides.Left))
					ExportBorder(writer, "Left");
				if (Sides.IsFlagSet(BorderSides.Top))
					ExportBorder(writer, "Top");
				if (Sides.IsFlagSet(BorderSides.Right))
					ExportBorder(writer, "Right");
				if (Sides.IsFlagSet(BorderSides.Bottom))
					ExportBorder(writer, "Bottom");
				// Border end
				writer.WriteEndElement();
			}
		}
		#endregion
	}

	/// <summary>
	/// Style class for cells, rows and worksheets
	/// </summary>
	public class XmlStyle : IStyle
	{
		#region Private and Internal fields
		internal string ID { get; set; }
		#endregion

		#region Public Properties
		private FontOptions _Font { get; set; }
		/// <summary>
		/// Gets or sets the font options
		/// </summary>
		public IFontOptions Font
		{
			get
			{
				return _Font;
			}
			set
			{
				_Font = (FontOptions) value;
			}
		}

		private AlignmentOptions _Alignment { get; set; }
		/// <summary>
		/// Gets or sets cell alignment options
		/// </summary>
		public IAlignmentOptions Alignment
		{ 
			get
			{
				return _Alignment;
			}
			set
			{
				_Alignment = (AlignmentOptions) value;
			}
		}

		private InteriorOptions _Interior { get; set; }
		/// <summary>
		/// Gets or sets interior options
		/// </summary>
		public IInteriorOptions Interior
		{
			get
			{
				return _Interior;
			}
			set
			{
				_Interior = (InteriorOptions) value;
			}
		}

		private BorderOptions _Border { get; set; }
		/// <summary>
		/// Gets or sets border settings
		/// </summary>
		public IBorderOptions Border
		{
			get
			{
				return _Border;
			}
			set
			{
				_Border = (BorderOptions)value;
			}
		}

		private DisplayFormatType displayFormat;
		/// <summary>
		/// Gets or sets the cell display format
		/// </summary>
		public DisplayFormatType DisplayFormat
		{
			get
			{
				return displayFormat;
			}
			set
			{
				displayFormat = value;

				if (!displayFormat.IsValid())
					throw new ArgumentException("Invalid display format value encountered");

				if (displayFormat == DisplayFormatType.Custom && CustomFormatString.IsNullOrEmpty())
					displayFormat = DisplayFormatType.None;
			}
		}

		private string customFormatString;
		/// <summary>
		/// Gets or sets a custom display string
		/// </summary>
		public string CustomFormatString
		{
			get
			{
				return customFormatString;
			}
			set
			{
				customFormatString = value;

				DisplayFormat = customFormatString.IsNullOrEmpty() ? DisplayFormatType.None : DisplayFormatType.Custom;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public XmlStyle()
		{
			Initialize();
			SetDefaults();
		}

		/// <summary>
		/// Creates a new instance from another instance of XmlStyle
		/// </summary>
		/// <param name="style">Instance to copy</param>
		public XmlStyle(XmlStyle style)
		{
			if (style == null)
			{
				Initialize();
				SetDefaults();
				return;
			}

			ID = "";

			_Font = new FontOptions(style._Font);
			_Interior = new InteriorOptions(style._Interior);
			_Alignment = new AlignmentOptions(style._Alignment);
			_Border = new BorderOptions(style._Border);

			DisplayFormat = style.DisplayFormat;
		}
		#endregion

		#region Private and Internal methods
		private void Initialize()
		{
			_Font = new FontOptions();
			_Interior = new InteriorOptions();
			_Alignment = new AlignmentOptions();
			_Border = new BorderOptions();
		}

		private void SetDefaults()
		{
			ID = "";

			DisplayFormat = DisplayFormatType.None;
		}

		internal bool CheckForMatch(XmlStyle style)
		{
			if (style == null)
				return false;

			if (_Font.CheckForMatch(style._Font) &&
				_Alignment.CheckForMatch(style._Alignment) &&
				_Interior.CheckForMatch(style._Interior) &&
				_Border.CheckForMatch(style._Border) &&
				DisplayFormat == style.DisplayFormat)
			{
				return true;
			}

			return false;
		}

		internal static string ColorToExcelFormat(Color color)
		{
			return String.Format(CultureInfo.InvariantCulture,
				"#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
		}

		internal static Color ExcelFormatToColor(string str)
		{
			int r;

			if (Int32.TryParse(str.Substring(1, 2), NumberStyles.HexNumber,
				CultureInfo.InvariantCulture, out r))
			{
				int g;

				if (Int32.TryParse(str.Substring(3, 2), NumberStyles.HexNumber,
					CultureInfo.InvariantCulture, out g))
				{
					int b;

					if (Int32.TryParse(str.Substring(5, 2), NumberStyles.HexNumber,
						CultureInfo.InvariantCulture, out b))
					{

						return Color.FromArgb(r, g, b);
					}
				}
			}

			return Color.Black;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Equality operator
		/// </summary>
		/// <param name="cellOne">Instance one to compare</param>
		/// <param name="cellTwo">Instance two to compare</param>
		/// <returns>true if all the values of the styles match, false otherwise</returns>
		public static bool operator ==(XmlStyle cellOne, XmlStyle cellTwo)
		{
			if (!(cellOne is XmlStyle))
			{
				return !(cellTwo is XmlStyle);
			}

			return cellOne.Equals(cellTwo);
		}

		/// <summary>
		/// Inequality operator
		/// </summary>
		/// <param name="cellOne">Instance one to compare</param>
		/// <param name="cellTwo">Instance two to compare</param>
		/// <returns>true if the values of the styles dont match, false otherwise</returns>
		public static bool operator !=(XmlStyle cellOne, XmlStyle cellTwo)
		{
			if (!(cellOne is XmlStyle))
			{
				return (cellTwo is XmlStyle);
			}

			return !cellOne.Equals(cellTwo);
		}

		/// <summary>
		/// Equality operator
		/// </summary>
		/// <param name="obj">Instance to compare</param>
		/// <returns>true if all the values of the styles match, false otherwise</returns>
		public override bool Equals(object obj)
		{
			if (obj is XmlStyle)
			{
				// do compare logic here
				return CheckForMatch((XmlStyle) obj);
			}

			return false;
		}

		/// <summary>
		/// Returns the hash code of the class
		/// </summary>
		/// <returns>Hash code of the class</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion

		#region Import
		internal static XmlStyle Import(XmlReader reader)
		{
			XmlStyle style = new XmlStyle();

			bool isEmpty = reader.IsEmptyElement;

			XmlReaderAttributeItem xa = reader.GetSingleAttribute("ID");
			if (xa != null)
				style.ID = xa.Value;

			if (isEmpty)
				return xa == null ? null : style;

			while (reader.Read() && !(reader.Name == "Style" && reader.NodeType == XmlNodeType.EndElement))
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						case "Font":
							{
								style._Font.Import(reader);

								break;
							}
						case "Alignment":
							{
								style._Alignment.Import(reader);

								break;
							}
						case "Interior":
							{
								style._Interior.Import(reader);

								break;
							}
						case "Borders":
							{
								style._Border.Import(reader);

								break;
							}
						case "NumberFormat":
							{
								XmlReaderAttributeItem nfAttr = reader.GetSingleAttribute("Format");
								if (nfAttr != null)
								{
									string format = nfAttr.Value;

									switch (format)
									{
										case "Short Date":
											{
												style.DisplayFormat = DisplayFormatType.ShortDate;
												break;
											}
										case "General Date":
											{
												style.DisplayFormat = DisplayFormatType.GeneralDate;
												break;
											}
										case "@":
											{
												style.DisplayFormat = DisplayFormatType.Text;
												break;
											}
										default:
											{
												if (format == DateTimeFormatInfo.CurrentInfo.LongDatePattern)
													style.DisplayFormat = DisplayFormatType.LongDate;

												string timeFormat = DateTimeFormatInfo.CurrentInfo.LongTimePattern;
												if (timeFormat.Contains("t"))
													timeFormat = timeFormat.Replace("t", "AM/PM");
												if (timeFormat.Contains("tt"))
													timeFormat = timeFormat.Replace("tt", "AM/PM");

												if (format == timeFormat)
													style.DisplayFormat = DisplayFormatType.Time;

												try
												{
													style.DisplayFormat = ObjectExtensions.ParseEnum<DisplayFormatType>(format);
												}
												catch (ArgumentException)
												{
													if (format.IsNullOrEmpty())
														style.DisplayFormat = DisplayFormatType.None;
													else
													{
														style.DisplayFormat = DisplayFormatType.Custom;
														style.CustomFormatString = format;
													}
												}

												break;
											}
									}

								}

								break;
							}
					}
				}
			}

			return style;
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			// Start Style tag
			writer.WriteStartElement("Style");
			// ID...
			writer.WriteAttributeString("ss", "ID", null, ID);

			_Font.Export(writer);

			_Alignment.Export(writer);

			_Border.Export(writer);

			_Interior.Export(writer);

			if (DisplayFormat != DisplayFormatType.None)
			{
				string format = "";

				switch (DisplayFormat)
				{
					case DisplayFormatType.ShortDate:
						{
							format = "Short Date";
							break;
						}
					case DisplayFormatType.GeneralDate:
						{
							format = "General Date";
							break;
						}
					case DisplayFormatType.LongDate:
						{
							format = DateTimeFormatInfo.CurrentInfo.LongDatePattern;
							break;
						}
					case DisplayFormatType.Time:
						{
							format = DateTimeFormatInfo.CurrentInfo.LongTimePattern;

							if (format.Contains("t"))
								format = format.Replace("t", "AM/PM");
							if (format.Contains("tt"))
								format = format.Replace("tt", "AM/PM");

							break;
						}
					case DisplayFormatType.Text:
						{
							format = "@";
							break;
						}
					case DisplayFormatType.Fixed:
					case DisplayFormatType.Percent:
					case DisplayFormatType.Scientific:
					case DisplayFormatType.Standard:
						{
							format = DisplayFormat.ToString();
							break;
						}
					case DisplayFormatType.Custom:
						{
							format = CustomFormatString;
							break;
						}
				}

				writer.WriteStartElement("NumberFormat");
				writer.WriteAttributeString("ss", "Format", null, format);
				writer.WriteEndElement();
			}

			// Style end
			writer.WriteEndElement();
		}
		#endregion
	}
}
