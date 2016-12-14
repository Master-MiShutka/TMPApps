using System;

namespace TMP.ExcelXml
{
	/// <summary>
	/// FormulaHelper makes formulae the way previous Formula
	/// constructor made. Only for backward compatibility.
	/// </summary>
	public static class FormulaHelper
	{
		/// <summary>
		/// Constructs a formula without any parameters
		/// </summary>
		/// <param name="function">Function name</param>
		/// <example><code>Formula formula = new Formula("sum");</code></example>
		public static Formula Formula(string function)
		{
			Formula formula = new Formula();
			formula.Add(function).EmptyGroup();

			return formula;
		}

		/// <summary>
		/// Constructs a formula and adds a range as the first parameter
		/// </summary>
		/// <param name="function">Function name</param>
		/// <param name="range">Range to add as parameter</param>
		/// <example><code>Formula formula = new Formula("sum", new Range(cell1, cell2));</code></example>
		public static Formula Formula(string function, Range range)
		{
			Formula formula = new Formula();
			formula.Add(function).StartGroup().Add(range).EndGroup();

			return formula;
		}

		/// <summary>
		/// Constructs a formula and adds a string as the first parameter
		/// </summary>
		/// <param name="function">Function name</param>
		/// <param name="parameter">String to add as parameter</param>
		/// <example><code>Formula formula = new Formula("sum", "0,1");</code></example>
		public static Formula Formula(string function, string parameter)
		{
			Formula formula = new Formula();
			formula.Add(function).StartGroup().Add(parameter).EndGroup();

			return formula;
		}

		/// <summary>
		/// Constructs a formula and adds another formula as the first parameter
		/// </summary>
		/// <param name="function">Function name</param>
		/// <param name="parameter">Another formula to add to this formula's parameter list</param>
		public static Formula Formula(string function, Formula parameter)
		{
			Formula formula = new Formula();
			formula.Add(function).StartGroup().Add(parameter).EndGroup();

			return formula;
		}

		/// <summary>
		/// Constructs a formula and adds a filtered range as the first parameter
		/// </summary>
		/// <param name="function">Function name</param>
		/// <param name="range">Range to add as parameter</param>
		/// <param name="cellCompare">A custom defined to compare the values of the range</param>
		/// <remarks>
		/// Custom delegates can filter all cells and auto add them to the parameter list of a formula 
		/// by passing a System.Predicate&gt;Cell&lt;, i.e. a 
		/// delegate which accepts Cell as its value and returns bool 
		/// to both Formula constructor or Add. All the values accessors (i.e. Value, NumericValue etc.) 
		/// and cell style can be checked.
		/// </remarks>
		/// <example>
		/// Lets assume column 1,2,3,6 and 7 are bold...
		/// <code>
		/// XmlStyle style = new XmlStyle();
		/// style.Font.Bold = true;
		/// 
		/// // VS2008 style
		/// sheet[7, 3].Value = new Formula("sum", new Range(sheet[0, 3], sheet[6, 3]), 
		/// 		cell =&gt; cell.Style == style);
		/// 
		/// // or VS2005 style
		/// sheet[7, 3].Value = new Formula("sum", new Range(sheet[0, 3], sheet[6, 3]), 
		/// 		delegate (Cell cell) { return cell.Style == style; } );
		/// </code>
		/// In the first example of style, the value of the cell will be =SUM(A4:C4, F4:G4).
		/// <para><b>Continuous ranges matching to true will be joined as one parameter, i.e. A4:C4 
		/// and not as seperate parameters, i.e. A4,B4,C4</b></para>
		/// 
		/// Using value accessors...
		/// <code>
		/// sheet[7, 3].Value = new Formula("sum", new Range(sheet[0, 3], sheet[6, 3]), 
		/// 			cell =&gt; cell.NumericValue > 10000 &amp; cell.NumericValue &lt;= 50000);
		/// </code>
		/// </example>
		public static Formula Formula(string function, Range range, Predicate<Cell> cellCompare)
		{
			Formula formula = new Formula();
			formula.Add(function).StartGroup().Add(range, cellCompare).EndGroup();

			return formula;
		}
	}
}
