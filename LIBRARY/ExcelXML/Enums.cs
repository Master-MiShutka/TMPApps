using System;
using System.Diagnostics.CodeAnalysis;

namespace TMP.ExcelXml
{
	#region ParseArgumentType
	internal enum ParseArgumentType
	{
		None,
		Function,
		Range,
		AbsoluteRange
	}
	#endregion

	#region ParameterType
	/// <summary>
	/// Formula parameter types
	/// </summary>
	public enum ParameterType
	{
		/// <summary>
		/// A unknown string parameter
		/// </summary>
		String,
		/// <summary>
		/// A Range
		/// </summary>
		Range,
		/// <summary>
		/// A Formula
		/// </summary>
		Formula
	}
	#endregion 

	#region Cell Content & Display Enums
	/// <summary>
	/// The default display format of the cell in excel
	/// </summary>
	public enum DisplayFormatType
	{
		/// <summary>
		/// General format
		/// </summary>
		None,
		/// <summary>
		/// Displays anything as text (i.e. Left aligned without formatting)
		/// </summary>
		Text,
		/// <summary>
		/// Displays numeric values with two fixed decimals
		/// </summary>
		Fixed,
		/// <summary>
		/// Displays numeric values with two fixed decimals and digit grouping
		/// </summary>
		Standard,
		/// <summary>
		/// Displays numeric values as percentage values
		/// </summary>
		Percent,
		/// <summary>
		/// Displays numeric values in scientific notation
		/// </summary>
		Scientific,
		/// <summary>
		/// Displays numeric or date values as formatted date values
		/// </summary>
		GeneralDate,
		/// <summary>
		/// Displays numeric or date values as short date format
		/// </summary>
		ShortDate,
		/// <summary>
		/// Displays numeric or date values as long date format
		/// </summary>
		LongDate,
		/// <summary>
		/// Displays numeric or date values in time format
		/// </summary>
		Time,
		/// <summary>
		/// Custom defined format
		/// </summary>
		Custom
	}

	/// <summary>
	/// The cell content type
	/// </summary>
	public enum ContentType
	{
		/// <summary>
		/// Cell does not contain anything
		/// </summary>
		None,
		/// <summary>
		/// Cell contains a string
		/// </summary>
		String,
		/// <summary>
		/// Cell contains a number
		/// </summary>
		Number,
		/// <summary>
		/// Cell contains a DateTime value
		/// </summary>
		DateTime,
		/// <summary>
		/// Cell contains a bool value
		/// </summary>
		Boolean,
		/// <summary>
		/// Cell contains a formula
		/// </summary>
		Formula,
		/// <summary>
		/// Cell contains a formula which cannot be resolved
		/// </summary>
		UnresolvedValue
	}
	#endregion

	#region Cell Style Enums
	/// <summary>
	/// Cell's vertical alignment values
	/// </summary>
	public enum VerticalAlignment
	{
		/// <summary>
		/// None
		/// </summary>
		None,
		/// <summary>
		/// Top aligned
		/// </summary>
		Top,
		/// <summary>
		/// Centered
		/// </summary>
		Center,
		/// <summary>
		/// Bottom aligned
		/// </summary>
		Bottom,
		/// <summary>
		/// Justified
		/// </summary>
		Justify,
		/// <summary>
		/// Distributed
		/// </summary>
		Distributed
	}

	/// <summary>
	/// Cell's horizontal alignment values
	/// </summary>
	public enum HorizontalAlignment
	{
		/// <summary>
		/// None
		/// </summary>
		None,
		/// <summary>
		/// Left aligned
		/// </summary>
		Left,
		/// <summary>
		/// Centered
		/// </summary>
		Center,
		/// <summary>
		/// Right aligned
		/// </summary>
		Right,
		/// <summary>
		/// Streched to fill
		/// </summary>
		Fill,
		/// <summary>
		/// Justified
		/// </summary>
		Justify,
		/// <summary>
		/// Distributed
		/// </summary>
		Distributed
	}

	/// <summary>
	/// Different style of border lines
	/// </summary>
	public enum Borderline
	{
		/// <summary>
		/// Continuous line
		/// </summary>
		Continuous,
		/// <summary>
		/// Dash line
		/// </summary>
		Dash,
		/// <summary>
		/// DashDot line
		/// </summary>
		DashDot,
		/// <summary>
		/// DashDotDot line
		/// </summary>
		DashDotDot,
		/// <summary>
		/// Double line
		/// </summary>
		Double,
		/// <summary>
		/// Dot line
		/// </summary>
		Dot,
		/// <summary>
		/// SlantDashDot line
		/// </summary>
		SlantDashDot
	}

	/// <summary>
	/// Different type of border sides.
	/// </summary>
	/// <remarks>Multiple values can be combined by an or (i.e. "|") operation.</remarks>
	[Flags]
	public enum BorderSides
	{
		/// <summary>
		/// No border
		/// </summary>
		None = 0,
		/// <summary>
		/// Cell has a top border
		/// </summary>
		Top = 1,
		/// <summary>
		/// Cell has a left border
		/// </summary>
		Left = 2,
		/// <summary>
		/// Cell has a botom border
		/// </summary>
		Bottom = 4,
		/// <summary>
		/// Cell has a right border
		/// </summary>
		Right = 8,
		/// <summary>
		/// Cell has full border on all sides
		/// </summary>
		All = 15
	}

	#region Pattern
	/// <summary>
	/// Different types of cell background patterns
	/// </summary>
	public enum Pattern
	{
		/// <summary>
		/// Solid
		/// </summary>
		Solid,
		/// <summary>
		/// Gray25
		/// </summary>
		Gray25,
		/// <summary>
		/// Gray50
		/// </summary>
		Gray50,
		/// <summary>
		/// Gray75
		/// </summary>
		Gray75,
		/// <summary>
		/// Gray125
		/// </summary>
		Gray125,
		/// <summary>
		/// Gray0625
		/// </summary>
		Gray0625,
		/// <summary>
		/// HorzStripe
		/// </summary>
		HorzStripe,
		/// <summary>
		/// VertStripe
		/// </summary>
		VertStripe,
		/// <summary>
		/// ReverseDiagStripe
		/// </summary>
		ReverseDiagStripe,
		/// <summary>
		/// DiagStripe
		/// </summary>
		DiagStripe,
		/// <summary>
		/// DiagCross
		/// </summary>
		DiagCross,
		/// <summary>
		/// ThickDiagCross
		/// </summary>
		ThickDiagCross,
		/// <summary>
		/// ThinHorzStripe
		/// </summary>
		ThinHorzStripe,
		/// <summary>
		/// ThinVertStripe
		/// </summary>
		ThinVertStripe,
		/// <summary>
		/// ThinReverseDiagStripe
		/// </summary>
		ThinReverseDiagStripe,
		/// <summary>
		/// ThinDiagStripe
		/// </summary>
		ThinDiagStripe,
		/// <summary>
		/// ThinHorzCross
		/// </summary>
		ThinHorzCross,
		/// <summary>
		/// ThinDiagCross
		/// </summary>
		ThinDiagCross
	}
	#endregion
	#endregion

	#region Page Properties
	/// <summary>
	/// Page layout
	/// </summary>
	public enum PageLayout
	{
		/// <summary>
		/// None
		/// </summary>
		None,
		/// <summary>
		/// Centers the page horizontally
		/// </summary>
		CenterHorizontal,
		/// <summary>
		/// Centers the page vertically
		/// </summary>
		CenterVertical,
		/// <summary>
		/// Centers the page vertically and horizontally
		/// </summary>
		CenterVerticalAndHorizontal
	}

	/// <summary>
	/// Orientation mode
	/// </summary>
	public enum PageOrientation
	{
		/// <summary>
		/// None.
		/// </summary>
		None,
		/// <summary>
		/// Landscape orientation
		/// </summary>
		Landscape,
		/// <summary>
		/// Portrait orientation
		/// </summary>
		Portrait
	}
	#endregion
}
