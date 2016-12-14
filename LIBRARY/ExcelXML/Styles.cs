using System;
using System.Drawing;

namespace TMP.ExcelXml
{
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
				throw new ArgumentNullException("parent");

			Parent = parent;
		}

		internal virtual ExcelXmlWorkbook GetParentBook()
		{
			return Parent.GetParentBook();
		}

		internal object GetCellStyleProperty(StylePropertyAccessor getDelegate)
		{
			if (GetParentBook() == null)
			{
				return getDelegate(Parent.FirstCell());
			}

			XmlStyle style = GetParentBook().GetStyleByID(Parent.StyleID);

			return getDelegate(style);
		}

		internal void SetCellStyleProperty(StylePropertyAccessor setDelegate)
		{
			if (GetParentBook() == null)
			{
				Parent.IterateAndApply(cell => setDelegate(cell));
			}
			else
			{
				XmlStyle style = new XmlStyle(GetParentBook().GetStyleByID(Parent.StyleID));
				setDelegate(style);

				Parent.StyleID = GetParentBook().AddStyle(style);
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
			get
			{
				return (string)GetCellStyleProperty(style => style.Font.Name);
			}
			set
			{
				SetCellStyleProperty(style => style.Font.Name = value);
			}
		}

		/// <summary>
		/// Gets or sets the size of the font
		/// </summary>
		public int Size
		{
			get
			{
				return (int)GetCellStyleProperty(style => style.Font.Size);
			}
			set
			{
				SetCellStyleProperty(style => style.Font.Size = value);
			}
		}

		/// <summary>
		/// Gets or sets font's bold property
		/// </summary>
		public bool Bold
		{
			get
			{
				return (bool)GetCellStyleProperty(style => style.Font.Bold);
			}
			set
			{
				SetCellStyleProperty(style => style.Font.Bold = value);
			}
		}

		/// <summary>
		/// Gets or sets font's underline property
		/// </summary>
		public bool Underline
		{
			get
			{
				return (bool)GetCellStyleProperty(style => style.Font.Underline);
			}
			set
			{
				SetCellStyleProperty(style => style.Font.Underline = value);
			}
		}

		/// <summary>
		/// Gets or sets font's italic property
		/// </summary>
		public bool Italic
		{
			get
			{
				return (bool)GetCellStyleProperty(style => style.Font.Italic);
			}
			set
			{
				SetCellStyleProperty(style => style.Font.Italic = value);
			}
		}

		/// <summary>
		/// Gets or sets font's strike-through property
		/// </summary>
		public bool Strikeout
		{
			get
			{
				return (bool)GetCellStyleProperty(style => style.Font.Strikeout);
			}
			set
			{
				SetCellStyleProperty(style => style.Font.Strikeout = value);
			}
		}

		/// <summary>
		/// Gets or sets font's color
		/// </summary>
		public Color Color
		{
			get
			{
				return (Color)GetCellStyleProperty(style => style.Font.Color);
			}
			set
			{
				SetCellStyleProperty(style => style.Font.Color = value);
			}
		}

		internal FontOptionsBase(Styles parent) : base(parent) { }
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
			get
			{
				return (Color)GetCellStyleProperty(style => style.Interior.Color);
			}
			set
			{
				SetCellStyleProperty(style => style.Interior.Color = value);
			}
		}

		/// <summary>
		/// Gets or sets cell pattern color
		/// </summary>
		public Color PatternColor
		{
			get
			{
				return (Color)GetCellStyleProperty(style => style.Interior.PatternColor);
			}
			set
			{
				SetCellStyleProperty(style => style.Interior.PatternColor = value);
			}
		}

		/// <summary>
		/// Gets or sets cell background color
		/// </summary>
		public Pattern Pattern
		{
			get
			{
				return (Pattern)GetCellStyleProperty(style => style.Interior.Pattern);
			}
			set
			{
				SetCellStyleProperty(style => style.Interior.Pattern = value);
			}
		}

		internal InteriorOptionsBase(Styles parent) : base(parent) { }
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
			get
			{
				return (VerticalAlignment)GetCellStyleProperty(style => style.Alignment.Vertical);
			}
			set
			{
				SetCellStyleProperty(style => style.Alignment.Vertical = value);
			}
		}

		/// <summary>
		/// Gets or sets horizontal alignment of the cell
		/// </summary>
		public HorizontalAlignment Horizontal
		{
			get
			{
				return (HorizontalAlignment)GetCellStyleProperty(style => style.Alignment.Horizontal);
			}
			set
			{
				SetCellStyleProperty(style => style.Alignment.Horizontal = value);
			}
		}

		/// <summary>
		/// Gets or sets text wrap setting
		/// </summary>
		public bool WrapText
		{
			get
			{
				return (bool)GetCellStyleProperty(style => style.Alignment.WrapText);
			}
			set
			{
				SetCellStyleProperty(style => style.Alignment.WrapText = value);
			}
		}

		/// <summary>
		/// Gets or sets the indent
		/// </summary>
		public int Indent
		{
			get
			{
				return (int)GetCellStyleProperty(style => style.Alignment.Indent);
			}
			set
			{
				SetCellStyleProperty(style => style.Alignment.Indent = value);
			}
		}

		/// <summary>
		/// Gets or sets the text rotation
		/// </summary>
		public int Rotate
		{
			get
			{
				return (int)GetCellStyleProperty(style => style.Alignment.Rotate);
			}
			set
			{
				SetCellStyleProperty(style => style.Alignment.Rotate = value);
			}
		}

		/// <summary>
		/// Gets or sets cell's shrink to cell setting
		/// </summary>
		public bool ShrinkToFit
		{
			get
			{
				return (bool)GetCellStyleProperty(style => style.Alignment.ShrinkToFit);
			}
			set
			{
				SetCellStyleProperty(style => style.Alignment.ShrinkToFit = value);
			}
		}

		internal AlignmentOptionsBase(Styles parent) : base(parent) { }
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
			get
			{
				return (BorderSides)GetCellStyleProperty(style => style.Border.Sides);
			}
			set
			{
				SetCellStyleProperty(style => style.Border.Sides = value);
			}
		}

		/// <summary>
		/// Gets or sets border color
		/// </summary>
		public Color Color
		{
			get
			{
				return (Color)GetCellStyleProperty(style => style.Border.Color);
			}
			set
			{
				SetCellStyleProperty(style => style.Border.Color = value);
			}
		}

		/// <summary>
		/// Gets or sets the width of the border
		/// </summary>
		public int Weight
		{
			get
			{
				return (int)GetCellStyleProperty(style => style.Border.Weight);
			}
			set
			{
				SetCellStyleProperty(style => style.Border.Weight = value);
			}
		}

		/// <summary>
		/// Gets or sets the border line style
		/// </summary>
		public Borderline LineStyle
		{
			get
			{
				return (Borderline)GetCellStyleProperty(style => style.Border.LineStyle);
			}
			set
			{
				SetCellStyleProperty(style => style.Border.LineStyle = value);
			}
		}

		internal BorderOptionsBase(Styles parent) : base(parent) { }
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
			return StyleID == "Default";
		}
		#endregion

		#region Public Properties
		private FontOptionsBase _Font;
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
				_Font = (FontOptionsBase)value;
			}
		}


		private AlignmentOptionsBase _Alignment;
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
				_Alignment = (AlignmentOptionsBase)value;
			}
		}


		private InteriorOptionsBase _Interior;
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
				_Interior = (InteriorOptionsBase)value;
			}
		}

		private BorderOptionsBase _Border { get; set; }
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
				_Border = (BorderOptionsBase)value;
			}
		}

		/// <summary>
		/// Gets or sets the cell display format
		/// </summary>
		public DisplayFormatType DisplayFormat
		{
			get
			{
				return (DisplayFormatType)GetCellStyleProperty(style => style.DisplayFormat);
			}
			set
			{
				SetCellStyleProperty(style => style.DisplayFormat = value);
			}
		}

		/// <summary>
		/// Gets or sets custom dispkay format string
		/// </summary>
		public string CustomFormatString
		{
			get
			{
				return (string)GetCellStyleProperty(style => style.CustomFormatString);
			}
			set
			{
				SetCellStyleProperty(style => style.CustomFormatString = value);
			}
		}

		/// <summary>
		/// Returns the <see cref="TMP.ExcelXml.XmlStyle"/> reference of the cell
		/// </summary>
		public XmlStyle Style
		{
			get
			{
				if (GetParentBook() == null)
				{
					return FirstCell().GetParentBook().GetStyleByID(StyleID);
				}

				return GetParentBook().GetStyleByID(StyleID);
			}
			set
			{
				if (value != null)
				{
					if (GetParentBook() == null)
					{
						IterateAndApply(cell => cell.Style = value);
					}
					else
					{
						StyleID = GetParentBook().AddStyle(value);
					}
				}
				else
					throw new ArgumentNullException("value");
			}
		}
		#endregion

		#region Constructor
		internal Styles()
		{
			StyleID = "";

			_Font = new FontOptionsBase(this);
			_Alignment = new AlignmentOptionsBase(this);
			_Interior = new InteriorOptionsBase(this);
			_Border = new BorderOptionsBase(this);

			Parent = this;
		}
		#endregion
	}
}
