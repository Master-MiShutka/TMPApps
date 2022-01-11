namespace TMP.ExcelXml
{
    using System;
    using System.Drawing;

    #region CellSettingsApplier

    /// <summary>
    /// Gets and sets various cell and range properties
    /// </summary>
    public abstract class CellSettingsApplier
    {
        internal delegate object StylePropertyAccessor(IStyle styles);

        internal Styles Parent;

        internal CellSettingsApplier()
        {
        }

        internal CellSettingsApplier(Styles parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.Parent = parent;
        }

        internal virtual ExcelXmlWorkbook GetParentBook()
        {
            return this.Parent.GetParentBook();
        }

        internal object GetCellStyleProperty(StylePropertyAccessor getDelegate)
        {
            if (this.GetParentBook() == null)
            {
                return getDelegate(this.Parent.FirstCell());
            }

            XmlStyle style = this.GetParentBook().GetStyleByID(this.Parent.StyleID);

            return getDelegate(style);
        }

        internal void SetCellStyleProperty(StylePropertyAccessor setDelegate)
        {
            if (this.GetParentBook() == null)
            {
                this.Parent.IterateAndApply(cell => setDelegate(cell));
            }
            else
            {
                XmlStyle style = new XmlStyle(this.GetParentBook().GetStyleByID(this.Parent.StyleID));
                setDelegate(style);

                this.Parent.StyleID = this.GetParentBook().AddStyle(style);
            }
        }
    }
    #endregion

    #region FontOptions

    /// <summary>
    /// Gets or sets cell's font options
    /// </summary>
    public class FontOptionsBase : CellSettingsApplier, IFontOptions
    {
        /// <summary>
        /// Gets or sets the name of the font
        /// </summary>
        public string Name
        {
            get => (string)this.GetCellStyleProperty(style => style.Font.Name);

            set => this.SetCellStyleProperty(style => style.Font.Name = value);
        }

        /// <summary>
        /// Gets or sets the size of the font
        /// </summary>
        public int Size
        {
            get => (int)this.GetCellStyleProperty(style => style.Font.Size);

            set => this.SetCellStyleProperty(style => style.Font.Size = value);
        }

        /// <summary>
        /// Gets or sets font's bold property
        /// </summary>
        public bool Bold
        {
            get => (bool)this.GetCellStyleProperty(style => style.Font.Bold);

            set => this.SetCellStyleProperty(style => style.Font.Bold = value);
        }

        /// <summary>
        /// Gets or sets font's underline property
        /// </summary>
        public bool Underline
        {
            get => (bool)this.GetCellStyleProperty(style => style.Font.Underline);

            set => this.SetCellStyleProperty(style => style.Font.Underline = value);
        }

        /// <summary>
        /// Gets or sets font's italic property
        /// </summary>
        public bool Italic
        {
            get => (bool)this.GetCellStyleProperty(style => style.Font.Italic);

            set => this.SetCellStyleProperty(style => style.Font.Italic = value);
        }

        /// <summary>
        /// Gets or sets font's strike-through property
        /// </summary>
        public bool Strikeout
        {
            get => (bool)this.GetCellStyleProperty(style => style.Font.Strikeout);

            set => this.SetCellStyleProperty(style => style.Font.Strikeout = value);
        }

        /// <summary>
        /// Gets or sets font's color
        /// </summary>
        public Color Color
        {
            get => (Color)this.GetCellStyleProperty(style => style.Font.Color);

            set => this.SetCellStyleProperty(style => style.Font.Color = value);
        }

        internal FontOptionsBase(Styles parent) : base(parent)
        {
        }
    }
    #endregion

    #region InteriorOptions

    /// <summary>
    /// Gets or sets cell's interior options
    /// </summary>
    public class InteriorOptionsBase : CellSettingsApplier, IInteriorOptions
    {
        /// <summary>
        /// Gets or sets cell background color
        /// </summary>
        public Color Color
        {
            get => (Color)this.GetCellStyleProperty(style => style.Interior.Color);

            set => this.SetCellStyleProperty(style => style.Interior.Color = value);
        }

        /// <summary>
        /// Gets or sets cell pattern color
        /// </summary>
        public Color PatternColor
        {
            get => (Color)this.GetCellStyleProperty(style => style.Interior.PatternColor);

            set => this.SetCellStyleProperty(style => style.Interior.PatternColor = value);
        }

        /// <summary>
        /// Gets or sets cell background color
        /// </summary>
        public Pattern Pattern
        {
            get => (Pattern)this.GetCellStyleProperty(style => style.Interior.Pattern);

            set => this.SetCellStyleProperty(style => style.Interior.Pattern = value);
        }

        internal InteriorOptionsBase(Styles parent) : base(parent)
        {
        }
    }
    #endregion

    #region AlignmentOptions

    /// <summary>
    /// Gets or sets cell's alignment options
    /// </summary>
    public class AlignmentOptionsBase : CellSettingsApplier, IAlignmentOptions
    {
        /// <summary>
        /// Gets or sets vertical alignment of the cell
        /// </summary>
        public VerticalAlignment Vertical
        {
            get => (VerticalAlignment)this.GetCellStyleProperty(style => style.Alignment.Vertical);

            set => this.SetCellStyleProperty(style => style.Alignment.Vertical = value);
        }

        /// <summary>
        /// Gets or sets horizontal alignment of the cell
        /// </summary>
        public HorizontalAlignment Horizontal
        {
            get => (HorizontalAlignment)this.GetCellStyleProperty(style => style.Alignment.Horizontal);

            set => this.SetCellStyleProperty(style => style.Alignment.Horizontal = value);
        }

        /// <summary>
        /// Gets or sets text wrap setting
        /// </summary>
        public bool WrapText
        {
            get => (bool)this.GetCellStyleProperty(style => style.Alignment.WrapText);

            set => this.SetCellStyleProperty(style => style.Alignment.WrapText = value);
        }

        /// <summary>
        /// Gets or sets the indent
        /// </summary>
        public int Indent
        {
            get => (int)this.GetCellStyleProperty(style => style.Alignment.Indent);

            set => this.SetCellStyleProperty(style => style.Alignment.Indent = value);
        }

        /// <summary>
        /// Gets or sets the text rotation
        /// </summary>
        public int Rotate
        {
            get => (int)this.GetCellStyleProperty(style => style.Alignment.Rotate);

            set => this.SetCellStyleProperty(style => style.Alignment.Rotate = value);
        }

        /// <summary>
        /// Gets or sets cell's shrink to cell setting
        /// </summary>
        public bool ShrinkToFit
        {
            get => (bool)this.GetCellStyleProperty(style => style.Alignment.ShrinkToFit);

            set => this.SetCellStyleProperty(style => style.Alignment.ShrinkToFit = value);
        }

        internal AlignmentOptionsBase(Styles parent) : base(parent)
        {
        }
    }
    #endregion

    #region BorderOptions

    /// <summary>
    /// Gets or sets the border options
    /// </summary>
    public class BorderOptionsBase : CellSettingsApplier, IBorderOptions
    {
        /// <summary>
        /// Gets or sets the border side flags
        /// </summary>
        public BorderSides Sides
        {
            get => (BorderSides)this.GetCellStyleProperty(style => style.Border.Sides);

            set => this.SetCellStyleProperty(style => style.Border.Sides = value);
        }

        /// <summary>
        /// Gets or sets border color
        /// </summary>
        public Color Color
        {
            get => (Color)this.GetCellStyleProperty(style => style.Border.Color);

            set => this.SetCellStyleProperty(style => style.Border.Color = value);
        }

        /// <summary>
        /// Gets or sets the width of the border
        /// </summary>
        public int Weight
        {
            get => (int)this.GetCellStyleProperty(style => style.Border.Weight);

            set => this.SetCellStyleProperty(style => style.Border.Weight = value);
        }

        /// <summary>
        /// Gets or sets the border line style
        /// </summary>
        public Borderline LineStyle
        {
            get => (Borderline)this.GetCellStyleProperty(style => style.Border.LineStyle);

            set => this.SetCellStyleProperty(style => style.Border.LineStyle = value);
        }

        internal BorderOptionsBase(Styles parent) : base(parent)
        {
        }
    }
    #endregion

    /// <summary>
    /// Style class for cells, rows and worksheets
    /// </summary>
    public abstract class Styles : CellSettingsApplier, IStyle
    {
        internal delegate void IterateFunction(Cell cell);

        internal abstract void IterateAndApply(IterateFunction ifFunc);

        internal abstract Cell FirstCell();

        #region Private and Internal fields
        internal string StyleID { get; set; }
        #endregion

        #region Private and Internal methods
        internal bool HasDefaultStyle()
        {
            return this.StyleID == "Default";
        }
        #endregion

        #region Public Properties
        private FontOptionsBase Font;

        /// <summary>
        /// Gets or sets the font options
        /// </summary>
        public IFontOptions Font
        {
            get => this.Font;

            set => this.Font = (FontOptionsBase)value;
        }

        private AlignmentOptionsBase _Alignment;

        /// <summary>
        /// Gets or sets cell alignment options
        /// </summary>
        public IAlignmentOptions Alignment
        {
            get => this._Alignment;

            set => this._Alignment = (AlignmentOptionsBase)value;
        }

        private InteriorOptionsBase Interior;

        /// <summary>
        /// Gets or sets interior options
        /// </summary>
        public IInteriorOptions Interior
        {
            get => this.Interior;

            set => this.Interior = (InteriorOptionsBase)value;
        }

        private BorderOptionsBase _Border { get; set; }

        /// <summary>
        /// Gets or sets border settings
        /// </summary>
        public IBorderOptions Border
        {
            get => this._Border;

            set => this._Border = (BorderOptionsBase)value;
        }

        /// <summary>
        /// Gets or sets the cell display format
        /// </summary>
        public DisplayFormatType DisplayFormat
        {
            get => (DisplayFormatType)this.GetCellStyleProperty(style => style.DisplayFormat);

            set => this.SetCellStyleProperty(style => style.DisplayFormat = value);
        }

        /// <summary>
        /// Gets or sets custom dispkay format string
        /// </summary>
        public string CustomFormatString
        {
            get => (string)this.GetCellStyleProperty(style => style.CustomFormatString);

            set => this.SetCellStyleProperty(style => style.CustomFormatString = value);
        }

        /// <summary>
        /// Returns the <see cref="TMP.ExcelXml.XmlStyle"/> reference of the cell
        /// </summary>
        public XmlStyle Style
        {
            get
            {
                if (this.GetParentBook() == null)
                {
                    return this.FirstCell().GetParentBook().GetStyleByID(this.StyleID);
                }

                return this.GetParentBook().GetStyleByID(this.StyleID);
            }

            set
            {
                if (value != null)
                {
                    if (this.GetParentBook() == null)
                    {
                        this.IterateAndApply(cell => cell.Style = value);
                    }
                    else
                    {
                        this.StyleID = this.GetParentBook().AddStyle(value);
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }
        #endregion

        #region Constructor
        internal Styles()
        {
            this.StyleID = string.Empty;

            this.Font = new FontOptionsBase(this);
            this._Alignment = new AlignmentOptionsBase(this);
            this.Interior = new InteriorOptionsBase(this);
            this._Border = new BorderOptionsBase(this);

            this.Parent = this;
        }
        #endregion
    }
}
