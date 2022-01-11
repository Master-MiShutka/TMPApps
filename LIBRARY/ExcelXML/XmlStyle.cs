namespace TMP.ExcelXml
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Xml;
    using TMP.Extensions;

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
            this.Name = "Calibri";
            this.Size = 0;
            this.Bold = false;
            this.Underline = false;
            this.Italic = false;
            this.Strikeout = false;
            this.Color = Color.Black;
        }

        /// <summary>
        /// Creates a new instance based on another instance
        /// </summary>
        /// <param name="fo">Instance to copy</param>
        public FontOptions(IFontOptions fo)
        {
            this.Name = fo.Name;
            this.Size = fo.Size;
            this.Bold = fo.Bold;
            this.Underline = fo.Underline;
            this.Italic = fo.Italic;
            this.Strikeout = fo.Strikeout;
            this.Color = fo.Color;
        }
        #endregion

        #region Private and Internal methods
        internal bool CheckForMatch(FontOptions other)
        {
            return this.Name == other.Name &&
                    this.Size == other.Size &&
                    this.Bold == other.Bold &&
                    this.Underline == other.Underline &&
                    this.Italic == other.Italic &&
                    this.Strikeout == other.Strikeout &&
                    this.Color == other.Color;
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
                            this.Name = xa.Value;

                            break;
                        }

                    case "Size":
                        {
                            int i;
                            if (xa.Value.ParseToInt(out i))
                            {
                                this.Size = i;
                            }

                            break;
                        }

                    case "Color":
                        {
                            this.Color = XmlStyle.ExcelFormatToColor(xa.Value);

                            break;
                        }

                    case "Bold":
                        {
                            this.Bold = xa.Value == "1" ? true : false;

                            break;
                        }

                    case "Italic":
                        {
                            this.Italic = xa.Value == "1" ? true : false;

                            break;
                        }

                    case "Underline":
                        {
                            this.Underline = xa.Value == "Single" ? true : false;

                            break;
                        }

                    case "Strikeout":
                        {
                            this.Strikeout = xa.Value == "1" ? true : false;

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
            writer.WriteAttributeString("ss", "FontName", null, this.Name);
            if (this.Size != 0)
            {
                writer.WriteAttributeString("ss", "Size", null, this.Size.ToString(
                    CultureInfo.InvariantCulture));
            }

            // Color
            writer.WriteAttributeString("ss", "Color", null, XmlStyle.ColorToExcelFormat(this.Color));

            // Bold?
            if (this.Bold)
            {
                writer.WriteAttributeString("ss", "Bold", null, "1");
            }

            // Italic?
            if (this.Italic)
            {
                writer.WriteAttributeString("ss", "Italic", null, "1");
            }

            // Underline?
            if (this.Underline)
            {
                writer.WriteAttributeString("ss", "Underline", null, "Single");
            }

            if (this.Strikeout)
            {
                writer.WriteAttributeString("ss", "Strikeout", null, "1");
            }

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
            this.Color = Color.Empty;
            this.PatternColor = Color.Empty;
            this.Pattern = Pattern.Solid;
        }

        /// <summary>
        /// Creates a new instance based on another instance
        /// </summary>
        /// <param name="io">Instance to copy</param>
        public InteriorOptions(IInteriorOptions io)
        {
            this.Color = io.Color;
            this.PatternColor = io.PatternColor;
            this.Pattern = io.Pattern;
        }
        #endregion

        #region Private and Internal methods
        internal bool CheckForMatch(InteriorOptions other)
        {
            return this.Color == other.Color &&
                    this.PatternColor == other.PatternColor &&
                    this.Pattern == other.Pattern;
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
                            this.Color = XmlStyle.ExcelFormatToColor(xa.Value);

                            break;
                        }

                    case "PatternColor":
                        {
                            this.PatternColor = XmlStyle.ExcelFormatToColor(xa.Value);

                            break;
                        }

                    case "Pattern":
                        {
                            this.Pattern = ObjectExtensions.ParseEnum<Pattern>(xa.Value);

                            break;
                        }
                }
            }
        }
        #endregion

        #region Export
        internal void Export(XmlWriter writer)
        {
            if (this.Color != Color.Empty || this.PatternColor != Color.Empty)
            {
                // Interior start
                writer.WriteStartElement("Interior");

                if (this.Color != Color.Empty)
                {
                    writer.WriteAttributeString("ss", "Color", null, XmlStyle.ColorToExcelFormat(this.Color));
                }

                if (this.PatternColor != Color.Empty)
                {
                    writer.WriteAttributeString("ss", "PatternColor", null, XmlStyle.ColorToExcelFormat(this.PatternColor));
                }

                writer.WriteAttributeString("ss", "Pattern", null, this.Pattern.ToString());

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
            get => this.vertical;

            set
            {
                this.vertical = value;

                if (!this.vertical.IsValid())
                {
                    throw new ArgumentException("Invalid vertical alignment value encountered");
                }
            }
        }

        private HorizontalAlignment horizontal;

        /// <summary>
        /// Gets or sets horizontal alignment of the cell
        /// </summary>
        public HorizontalAlignment Horizontal
        {
            get => this.horizontal;

            set
            {
                this.horizontal = value;

                if (!this.horizontal.IsValid())
                {
                    throw new ArgumentException("Invalid horizontal alignment value encountered");
                }
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
            this.horizontal = HorizontalAlignment.None;
            this.vertical = VerticalAlignment.None;

            this.Indent = 0;
            this.Rotate = 0;

            this.WrapText = false;
            this.ShrinkToFit = false;
        }

        /// <summary>
        /// Creates a new instance based on another instance
        /// </summary>
        /// <param name="ao">Instance to copy</param>
        public AlignmentOptions(IAlignmentOptions ao)
        {
            this.horizontal = ao.Horizontal;
            this.vertical = ao.Vertical;

            this.Indent = ao.Indent;
            this.Rotate = ao.Rotate;

            this.WrapText = ao.WrapText;
            this.ShrinkToFit = ao.ShrinkToFit;
        }
        #endregion

        #region Private and Internal methods
        internal bool CheckForMatch(AlignmentOptions other)
        {
            return this.Vertical == other.Vertical &&
                    this.Horizontal == other.Horizontal &&
                    this.Indent == other.Indent &&
                    this.Rotate == other.Rotate &&
                    this.WrapText == other.WrapText &&
                    this.ShrinkToFit == other.ShrinkToFit;
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
                            this.Vertical = ObjectExtensions.ParseEnum<VerticalAlignment>(xa.Value);

                            break;
                        }

                    case "Horizontal":
                        {
                            this.Horizontal = ObjectExtensions.ParseEnum<HorizontalAlignment>(xa.Value);

                            break;
                        }

                    case "WrapText":
                        {
                            this.WrapText = xa.Value == "1" ? true : false;

                            break;
                        }

                    case "ShrinkToFit":
                        {
                            this.ShrinkToFit = xa.Value == "1" ? true : false;

                            break;
                        }

                    case "Indent":
                        {
                            int i;
                            if (xa.Value.ParseToInt(out i))
                            {
                                this.Indent = i;
                            }

                            break;
                        }

                    case "Rotate":
                        {
                            int i;
                            if (xa.Value.ParseToInt(out i))
                            {
                                this.Rotate = i;
                            }

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

            if (this.Vertical != VerticalAlignment.None)
            {
                writer.WriteAttributeString("ss", "Vertical", null, this.Vertical.ToString());
            }

            if (this.Horizontal != HorizontalAlignment.None)
            {
                writer.WriteAttributeString("ss", "Horizontal", null, this.Horizontal.ToString());
            }

            if (this.WrapText)
            {
                writer.WriteAttributeString("ss", "WrapText", null, "1");
            }

            if (this.ShrinkToFit)
            {
                writer.WriteAttributeString("ss", "ShrinkToFit", null, "1");
            }

            if (this.Indent > 0)
            {
                writer.WriteAttributeString("ss", "Indent", null, this.Indent.ToString(
                    CultureInfo.InvariantCulture));
            }

            if (this.Rotate > 0)
            {
                writer.WriteAttributeString("ss", "Rotate", null, this.Rotate.ToString(
                    CultureInfo.InvariantCulture));
            }

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
            get => this.sides;

            set
            {
                this.sides = value;

                if (!this.sides.IsValid())
                {
                    throw new ArgumentException("Invalid Border side value encountered");
                }
            }
        }

        private Borderline lineStyle;

        /// <summary>
        /// Gets or sets the border line style
        /// </summary>
        public Borderline LineStyle
        {
            get => this.lineStyle;

            set
            {
                this.lineStyle = value;

                if (!this.lineStyle.IsValid())
                {
                    throw new ArgumentException("Invalid line style value encountered");
                }
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
            this.sides = BorderSides.None;
            this.lineStyle = Borderline.Continuous;
            this.Weight = 1;
            this.Color = Color.Black;
        }

        /// <summary>
        /// Creates a new instance based on another instance
        /// </summary>
        /// <param name="borderOptions">Instance to copy</param>
        public BorderOptions(IBorderOptions borderOptions)
        {
            this.sides = borderOptions.Sides;
            this.lineStyle = borderOptions.LineStyle;
            this.Weight = borderOptions.Weight;
            this.Color = borderOptions.Color;
        }
        #endregion

        #region Private and Internal methods
        internal bool CheckForMatch(BorderOptions other)
        {
            return this.Sides == other.Sides &&
                    this.LineStyle == other.LineStyle &&
                    this.Weight == other.Weight &&
                    this.Color == other.Color;
        }
        #endregion

        #region Import
        internal void Import(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }

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
                                        this.Sides |= ObjectExtensions.ParseEnum<BorderSides>(xa.Value);

                                        break;
                                    }

                                case "LineStyle":
                                    {
                                        this.LineStyle = ObjectExtensions.ParseEnum<Borderline>(xa.Value);

                                        break;
                                    }

                                case "Weight":
                                    {
                                        int i;
                                        if (xa.Value.ParseToInt(out i))
                                        {
                                            this.Weight = i;
                                        }

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
            writer.WriteAttributeString("ss", "LineStyle", null, this.LineStyle.ToString());
            writer.WriteAttributeString("ss", "Weight", null, this.Weight.ToString(
                    CultureInfo.InvariantCulture));

            if (this.Color != Color.Black)
            {
                writer.WriteAttributeString("ss", "Color", null, XmlStyle.ColorToExcelFormat(this.Color));
            }

            writer.WriteEndElement();
        }

        internal void Export(XmlWriter writer)
        {
            if (this.Sides != BorderSides.None)
            {
                // Border start
                writer.WriteStartElement("Borders");
                if (this.Sides.IsFlagSet(BorderSides.Left))
                {
                    this.ExportBorder(writer, "Left");
                }

                if (this.Sides.IsFlagSet(BorderSides.Top))
                {
                    this.ExportBorder(writer, "Top");
                }

                if (this.Sides.IsFlagSet(BorderSides.Right))
                {
                    this.ExportBorder(writer, "Right");
                }

                if (this.Sides.IsFlagSet(BorderSides.Bottom))
                {
                    this.ExportBorder(writer, "Bottom");
                }

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
            get => this._Font;

            set => this._Font = (FontOptions)value;
        }

        private AlignmentOptions _Alignment { get; set; }

        /// <summary>
        /// Gets or sets cell alignment options
        /// </summary>
        public IAlignmentOptions Alignment
        {
            get => this._Alignment;

            set => this._Alignment = (AlignmentOptions)value;
        }

        private InteriorOptions _Interior { get; set; }

        /// <summary>
        /// Gets or sets interior options
        /// </summary>
        public IInteriorOptions Interior
        {
            get => this._Interior;

            set => this._Interior = (InteriorOptions)value;
        }

        private BorderOptions _Border { get; set; }

        /// <summary>
        /// Gets or sets border settings
        /// </summary>
        public IBorderOptions Border
        {
            get => this._Border;

            set => this._Border = (BorderOptions)value;
        }

        private DisplayFormatType displayFormat;

        /// <summary>
        /// Gets or sets the cell display format
        /// </summary>
        public DisplayFormatType DisplayFormat
        {
            get => this.displayFormat;

            set
            {
                this.displayFormat = value;

                if (!this.displayFormat.IsValid())
                {
                    throw new ArgumentException("Invalid display format value encountered");
                }

                if (this.displayFormat == DisplayFormatType.Custom && this.CustomFormatString.IsNullOrEmpty())
                {
                    this.displayFormat = DisplayFormatType.None;
                }
            }
        }

        private string customFormatString;

        /// <summary>
        /// Gets or sets a custom display string
        /// </summary>
        public string CustomFormatString
        {
            get => this.customFormatString;

            set
            {
                this.customFormatString = value;

                this.DisplayFormat = this.customFormatString.IsNullOrEmpty() ? DisplayFormatType.None : DisplayFormatType.Custom;
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public XmlStyle()
        {
            this.Initialize();
            this.SetDefaults();
        }

        /// <summary>
        /// Creates a new instance from another instance of XmlStyle
        /// </summary>
        /// <param name="style">Instance to copy</param>
        public XmlStyle(XmlStyle style)
        {
            if (style == null)
            {
                this.Initialize();
                this.SetDefaults();
                return;
            }

            this.ID = string.Empty;

            this._Font = new FontOptions(style._Font);
            this._Interior = new InteriorOptions(style._Interior);
            this._Alignment = new AlignmentOptions(style._Alignment);
            this._Border = new BorderOptions(style._Border);

            this.DisplayFormat = style.DisplayFormat;
        }
        #endregion

        #region Private and Internal methods
        private void Initialize()
        {
            this._Font = new FontOptions();
            this._Interior = new InteriorOptions();
            this._Alignment = new AlignmentOptions();
            this._Border = new BorderOptions();
        }

        private void SetDefaults()
        {
            this.ID = string.Empty;

            this.DisplayFormat = DisplayFormatType.None;
        }

        internal bool CheckForMatch(XmlStyle style)
        {
            if (style == null)
            {
                return false;
            }

            if (this._Font.CheckForMatch(style._Font) &&
                this._Alignment.CheckForMatch(style._Alignment) &&
                this._Interior.CheckForMatch(style._Interior) &&
                this._Border.CheckForMatch(style._Border) &&
                this.DisplayFormat == style.DisplayFormat)
            {
                return true;
            }

            return false;
        }

        internal static string ColorToExcelFormat(Color color)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        internal static Color ExcelFormatToColor(string str)
        {
            int r;

            if (int.TryParse(str.Substring(1, 2), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out r))
            {
                int g;

                if (int.TryParse(str.Substring(3, 2), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out g))
                {
                    int b;

                    if (int.TryParse(str.Substring(5, 2), NumberStyles.HexNumber,
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
                return cellTwo is XmlStyle;
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
                return this.CheckForMatch((XmlStyle)obj);
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
            {
                style.ID = xa.Value;
            }

            if (isEmpty)
            {
                return xa == null ? null : style;
            }

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
                                                {
                                                    style.DisplayFormat = DisplayFormatType.LongDate;
                                                }

                                                string timeFormat = DateTimeFormatInfo.CurrentInfo.LongTimePattern;
                                                if (timeFormat.Contains("t"))
                                                {
                                                    timeFormat = timeFormat.Replace("t", "AM/PM");
                                                }

                                                if (timeFormat.Contains("tt"))
                                                {
                                                    timeFormat = timeFormat.Replace("tt", "AM/PM");
                                                }

                                                if (format == timeFormat)
                                                {
                                                    style.DisplayFormat = DisplayFormatType.Time;
                                                }

                                                try
                                                {
                                                    style.DisplayFormat = ObjectExtensions.ParseEnum<DisplayFormatType>(format);
                                                }
                                                catch (ArgumentException)
                                                {
                                                    if (format.IsNullOrEmpty())
                                                    {
                                                        style.DisplayFormat = DisplayFormatType.None;
                                                    }
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
            writer.WriteAttributeString("ss", "ID", null, this.ID);

            this._Font.Export(writer);

            this._Alignment.Export(writer);

            this._Border.Export(writer);

            this._Interior.Export(writer);

            if (this.DisplayFormat != DisplayFormatType.None)
            {
                string format = string.Empty;

                switch (this.DisplayFormat)
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
                            {
                                format = format.Replace("t", "AM/PM");
                            }

                            if (format.Contains("tt"))
                            {
                                format = format.Replace("tt", "AM/PM");
                            }

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
                            format = this.DisplayFormat.ToString();
                            break;
                        }

                    case DisplayFormatType.Custom:
                        {
                            format = this.CustomFormatString;
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
