using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using TMP.Extensions;

namespace TMP.ExcelXml
{
	/// <summary>
	/// Cell Index Information
	/// </summary>
	public class CellIndexInfo
	{
		/// <summary>
		/// Row index starting from 0
		/// </summary>
		public int RowIndex { get; private set; }
		/// <summary>
		/// Column index starting from 0
		/// </summary>
		public int ColumnIndex { get; private set; }
		/// <summary>
		/// Index in excel format, eg. A1
		/// </summary>
		public string ExcelColumnIndex { get; private set; }

		internal CellIndexInfo(Cell cell)
		{
			ColumnIndex = cell.CellIndex;
			RowIndex = cell.ParentRow.RowIndex;

			SetExcelIndex();
		}

		private void SetExcelIndex()
		{
			ExcelColumnIndex = "";

			int partOne = (ColumnIndex / 26) - 1;
			int partTwo = ColumnIndex % 26;

			if (partOne >= 0)
			{
				char firstHalf = (char)(('A') + partOne);
				ExcelColumnIndex += firstHalf;
			}

			char secondHalf = (char)(('A') + partTwo);
			ExcelColumnIndex += secondHalf;
		}
	}

	/// <summary>
	/// Cell class represents a single cell in a worksheet
	/// </summary>
	/// <remarks>
	/// Cell class represents a single cell in a worksheet.
	/// <para>You cannot directly declare a instance of a cell from your code by using
	/// <c>new</c> keyword. The only way to access a cell is to retrieve it from
	/// a worksheet or a row.</para>
	/// </remarks>
	public class Cell : Styles
	{
		#region Private and Internal fields
		private Formula formula;

		internal ContentType Content;

		internal Row ParentRow;
		internal int CellIndex;
		internal bool MergeStart;
		#endregion

		#region Public Properties
		/// <summary>
		/// Returns the cell content type
		/// </summary>
		public ContentType ContentType
		{
			get
			{
				return Content;
			}
		}

		/// <summary>
		/// Index information of the cell
		/// </summary>
		public CellIndexInfo Index
		{
			get
			{
				return new CellIndexInfo(this);
			}
		}

		/// <summary>
		/// Gets or sets the comment for the cell
		/// </summary>
		/// <remarks>Comment is in raw html format which means you can insert
		/// bold and italics markers just like regular html</remarks>
		public string Comment { get; set; }

		private int columnSpan;
		/// <summary>
		/// Gets the number of columns merged together, starting with this cell
		/// </summary>
		public int ColumnSpan
		{
			get
			{
				if (MergeStart)
					return columnSpan;

				return 1;
			}
			internal set
			{
				columnSpan = value;
			}
		}

		private int rowSpan;
		/// <summary>
		/// Gets the number of rows merged together, starting with this cell
		/// </summary>
		public int RowSpan
		{
			get
			{
				if (MergeStart)
					return rowSpan;

				return 1;
			}
			internal set
			{
				rowSpan = value;
			}
		}
		/// <summary>
		/// Gets or sets the a external reference as a link
		/// </summary>
		/// <remarks>The value of HRef is not verified.</remarks>
		public string HRef { get; set; }
		#endregion

		#region Constructor
		internal Cell(Row parent, int cell)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			ParentRow = parent;

			Content = ContentType.None;
			CellIndex = cell;

			if (parent.Style != null)
				Style = parent.Style;
			else if (parent.ParentSheet.Columns(CellIndex).Style != null)
				Style = parent.ParentSheet.Columns(CellIndex).Style;
			else if (parent.ParentSheet.Style != null)
				Style = parent.ParentSheet.Columns(CellIndex).Style;
		}
		#endregion

		#region Private and Internal methods
		internal override ExcelXmlWorkbook GetParentBook()
		{
			return ParentRow.ParentSheet.ParentBook;
		}

		internal override void IterateAndApply(IterateFunction ifFunc)
		{
		}

		internal override Cell FirstCell()
		{
			return null;
		}

		internal void ResolveReferences()
		{
			if (Content == ContentType.Formula)
			{
				foreach (Parameter p in formula.Parameters)
				{
					if (p.ParameterType == ParameterType.Range)
					{
						Range r = p.Value as Range;

						if (r != null)
						{
							r.ParseUnresolvedReference(this);
						}
					}
				}
			}
		}

		internal void Empty(bool removeContentOnly)
		{
			Content = ContentType.None;

			_value = null;
			formula = null;

			if (!removeContentOnly)
				ParentRow = null;
		}
		#endregion

		#region Cell Get And Set
		/// <summary>
		/// Gets the value of a cell converted to a system type
		/// </summary>
		/// <typeparam name="T">Type to convert to</typeparam>
		/// <returns>Cell value converted to system type</returns>
		public T GetValue<T>()
		{
			string typeName = typeof(T).FullName;

			if (typeName == "System.Object")
				return (T)_value;

			if (!typeof(T).IsPrimitive &&
				typeName != "System.DateTime" &&
				typeName != "System.String" &&
				typeName != "TMP.ExcelXml.Formula")
			{
				throw new ArgumentException("T must be of a primitive or Formula type");
			}

			switch (Content)
			{
				case ContentType.Boolean:
					{
						if (typeName == "System.Boolean")
							return (T)Convert.ChangeType(_value, typeof(T), 
								CultureInfo.InvariantCulture);

						return default(T);
					}

				case ContentType.DateTime:
					{
						if (typeName == "System.DateTime")
							return (T)Convert.ChangeType(_value, typeof(T),
								CultureInfo.InvariantCulture);

						return default(T);
					}

				case ContentType.Number:
					{
						if (ObjectExtensions.IsNumericType(typeof(T)))
						{
							return (T)Convert.ChangeType(_value, typeof(T),
								CultureInfo.InvariantCulture);
						}						

						return default(T);
					}

				case ContentType.Formula:
					{
						if (typeName == "TMP.ExcelXml.Formula")
							return (T)_value;

						return default(T);
					}

				case ContentType.UnresolvedValue:
				case ContentType.String:
					{
						if (typeName == "System.String")
							return (T)Convert.ChangeType(_value, typeof(T),
								CultureInfo.InvariantCulture);

						return default(T);
					}
			}

			return default(T);
		}

		private object _value;
		/// <summary>
		/// Gets or sets the value of the cell
		/// </summary>
		/// <remarks>
		/// Value returns a boxed <see cref="System.String"/> value of the cell or sets the value of the cell to...
		/// <list type="number">
		/// <item>
		/// <term><see cref="System.String"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.Boolean"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.Byte"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.Int16"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.Int32"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.Int64"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.Double"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.Decimal"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="System.DateTime"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="TMP.ExcelXml.Cell"/></term><description></description>
		/// </item>
		/// <item>
		/// <term><see cref="TMP.ExcelXml.Formula"/></term><description></description>
		/// </item>
		/// </list>
		/// <para>If the type is not any of the above, cell value is set to null.</para></remarks>
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				switch (value.GetType().FullName)
				{
					case "System.DateTime":
						{
							_value = value;

							Content = ContentType.DateTime;

							break;
						}
					case "System.Byte":
					case "System.SByte":
					case "System.Int16":
					case "System.Int32":
					case "System.Int64":
					case "System.UInt16":
					case "System.UInt32":
					case "System.UInt64":
					case "System.Single":
					case "System.Double":
					case "System.Decimal":
						{
							_value = value;

							Content = ContentType.Number;

							break;
						}
					case "System.Boolean":
						{
							_value = value;

							Content = ContentType.Boolean;

							break;
						}
					case "System.String":
						{
							_value = value;

							Content = ContentType.String;

							break;
						}
					case "TMP.ExcelXml.Cell":
						{
							Cell from = value as Cell;
							if (from != null)
							{
								if (formula != null)
									formula = null;

								formula = new Formula();
								_value = null;
								formula.Add(new Range(from));

								Content = ContentType.Formula;
							}
							else
							{
								formula = null;
								_value = null;
								Content = ContentType.None;
							}

							break;
						}
					case "TMP.ExcelXml.Formula":
						{
							Formula from = value as Formula;

							if (from != null)
							{
								formula = from;
								_value = null;
								Content = ContentType.Formula;
							}
							else
							{
								formula = null;
								_value = null;
								Content = ContentType.None;
							}

							break;
						}
					default:
						{
							throw new NotImplementedException();
						}
				}
			}
		}
		#endregion

		#region Public Cell Addition, Insertion & Deletion methods
		/// <summary>
		/// Checks whether the cell has no content and no comment
		/// </summary>
		/// <returns>true if empty, false otherwise</returns>
		public bool IsEmpty()
		{
			if (Content == ContentType.None && Comment.IsNullOrEmpty() && HasDefaultStyle())
				return true;

			return false;
		}

		/// <summary>
		/// Empties the content of a cell
		/// </summary>
		public void Empty()
		{
			Empty(true);
		}

		/// <summary>
		/// Unmerges a cell
		/// </summary>
		public void Unmerge()
		{
			if (!MergeStart)
				return;

			Worksheet ws = ParentRow.ParentSheet;
			ws._MergedCells.RemoveAll(range => range.CellFrom == this);

			MergeStart = false;
		}

		/// <summary>
		/// Deletes a cell from the parent row
		/// </summary>
		public void Delete()
		{
			ParentRow.DeleteCell(this);
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer, bool printIndex)
		{
			if (IsEmpty())
				return;

			// If no merge starts from this cell, and this cells is in 
			// a merged range, no output should be done...
			if (!MergeStart && ParentRow.ParentSheet.IsCellMerged(this))
				return;

			// Start Cell
			writer.WriteStartElement("Cell");

			// Has style? If yes, we only need to write the style if default line
			// style is not same as this one...
			if (!StyleID.IsNullOrEmpty() && ParentRow.StyleID != StyleID && StyleID != "Default")
				writer.WriteAttributeString("ss", "StyleID", null, StyleID);

			if (printIndex)
				writer.WriteAttributeString("ss", "Index", null, 
					(CellIndex + 1).ToString(CultureInfo.InvariantCulture));

			if (!HRef.IsNullOrEmpty())
				writer.WriteAttributeString("ss", "HRef", null, HRef.XmlEncode());

			if (MergeStart)
			{
				Worksheet ws = ParentRow.ParentSheet;
				Range range = ws._MergedCells.Find(rangeToFind => rangeToFind.CellFrom == this);

				if (range != null)
				{
					int rangeCols = range.ColumnCount - 1;
					int rangeRows = range.RowCount - 1;

					if (rangeCols > 0)
						writer.WriteAttributeString("ss", "MergeAcross", null,
							rangeCols.ToString(CultureInfo.InvariantCulture));

					if (rangeRows > 0)
						writer.WriteAttributeString("ss", "MergeDown", null,
							rangeRows.ToString(CultureInfo.InvariantCulture));
				}
			}

			// Export content
			ExportContent(writer);

			// Export comment
			ExportComment(writer);

			// Write named ranges
			List<string> namedRanges = GetParentBook().CellInNamedRanges(this);

			foreach (string range in namedRanges)
			{
				writer.WriteStartElement("NamedCell");
				writer.WriteAttributeString("ss", "Name", null, range);
				writer.WriteEndElement();
			}

			// End Cell
			writer.WriteEndElement();
		}

		void ExportContent(XmlWriter writer)
		{
			// Has formula?
			if (Content == ContentType.Formula)
			{
				writer.WriteAttributeString("ss", "Formula", null, "=" + formula.ToString(this));
			}
			else if (Content == ContentType.UnresolvedValue)
			{
				writer.WriteAttributeString("ss", "Formula", null, (string)_value);
			}
			else if (Content != ContentType.None)
			{
				// Write Data
				writer.WriteStartElement("Data");
				writer.WriteAttributeString("ss", "Type", null, Content.ToString());

				switch (Content)
				{
					case ContentType.Boolean:
						{
							if ((bool)_value)
								writer.WriteValue("1");
							else
								writer.WriteValue("0");
							break;
						}
					case ContentType.DateTime:
						{
							writer.WriteValue(((DateTime)_value).ToString("yyyy-MM-dd\\Thh:mm:ss.fff",
								CultureInfo.InvariantCulture));
							break;
						}
					case ContentType.Number:
						{
							decimal d = Convert.ToDecimal(_value, CultureInfo.InvariantCulture);
							writer.WriteValue(d.ToString(new CultureInfo("en-US")));
							break;
						}
					case ContentType.String:
						{
							writer.WriteValue((string)_value);
							break;
						}
				}

				writer.WriteEndElement();
			}
		}

		void ExportComment(XmlWriter writer)
		{
			// Write comment
			if (!Comment.IsNullOrEmpty())
			{
				string author = GetParentBook().Properties.Author;

				// Start comment
				writer.WriteStartElement("Comment");

				if (!author.IsNullOrEmpty())
					writer.WriteAttributeString("ss", "Author", null, author);

				// Comment data section
				writer.WriteStartElement("ss", "Data", null);
				writer.WriteAttributeString("xmlns", "http://www.w3.org/TR/REC-html40");
				writer.WriteRaw(Comment);
				writer.WriteEndElement();

				// End comment
				writer.WriteEndElement();
			}
		}
		#endregion
	}
}
