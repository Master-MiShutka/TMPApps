using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using TMP.Extensions;

namespace TMP.ExcelXml
{
	/// <summary>
	/// Worksheet class represents a single sheet in a workbook
	/// </summary>
	/// <remarks>
	/// Worksheet class represents a single sheet in a workbook.
	/// <para>You cannot directly declare a instance of a sheet from your code by using
	/// <c>new</c> keyword. The only way to access a sheet is to retrieve it from
	/// a workbook.</para>
	/// </remarks>
	public partial class Worksheet : Styles, IEnumerable<Cell>
	{
		#region Private and Internal fields
		private List<Column> _Columns;

		internal List<Row> _Rows;
		internal List<Range> _MergedCells;

		internal int maxColumnAddressed;

		internal ExcelXmlWorkbook ParentBook;
		internal bool AutoFilter;
		internal bool PrintArea;
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets or sets various sheet printing options
		/// </summary>
		public PrintOptions PrintOptions { get; set; }

		private string sheetName;
		/// <summary>
		/// Gets or sets the sheet name
		/// </summary>
		public string Name
		{
			get
			{
				return sheetName;
			}
			set
			{
				if (!value.IsNullOrEmpty())
				{
					Worksheet ws = GetParentBook()[sheetName];

					if (ws == null || ws == this)
					{
						sheetName = value.Trim();
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets top freezed row setting
		/// </summary>
		public int FreezeTopRows { get; set; }
		/// <summary>
		/// Gets or sets left freezed column setting
		/// </summary>
		public int FreezeLeftColumns { get; set; }
		/// <summary>
		/// Gets or sets the tab color
		/// </summary>
		public int TabColor { get; set; }

		/// <summary>
		/// Checks if print area is set
		/// </summary>
		public bool IsPrintAreaSet
		{
			get
			{
				return PrintArea;
			}
		}
		#endregion

		#region Constructor
		internal Worksheet(ExcelXmlWorkbook parent)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			ParentBook = parent;

			PrintOptions = new PrintOptions();

			PrintOptions.Layout = PageLayout.None;
			PrintOptions.Orientation = PageOrientation.None;

			_Rows = new List<Row>();
			_Columns = new List<Column>();
			_MergedCells = new List<Range>();

			TabColor = -1;

			PrintOptions.FitHeight = 1;
			PrintOptions.FitWidth = 1;
			PrintOptions.Scale = 100;

			PrintOptions.ResetMargins();
		}
		#endregion

		#region Private and Internal methods
		internal override ExcelXmlWorkbook GetParentBook()
		{
			return ParentBook;
		}


		internal override void IterateAndApply(IterateFunction ifFunc)
		{
		}

		internal override Cell FirstCell()
		{
			return null;
		}

		internal Cell Cells(int colIndex, int rowIndex)
		{
			if (colIndex < 0)
				throw new ArgumentOutOfRangeException("colIndex");
			if (rowIndex < 0)
				throw new ArgumentOutOfRangeException("rowIndex");

			if (rowIndex + 1 > _Rows.Count)
				for (int i = _Rows.Count; i <= rowIndex; i++)
					_Rows.Add(new Row(this, i));

			if (colIndex + 1 > _Rows[rowIndex]._Cells.Count)
				for (int i = _Rows[rowIndex]._Cells.Count; i <= colIndex; i++)
					_Rows[rowIndex]._Cells.Add(new Cell(_Rows[rowIndex], i));

			maxColumnAddressed = Math.Max(colIndex, maxColumnAddressed);

			return _Rows[rowIndex]._Cells[colIndex];
		}

		internal Row GetRowByIndex(int rowIndex)
		{
			if (rowIndex < 0)
				throw new ArgumentOutOfRangeException("rowIndex");

			if (rowIndex + 1 > _Rows.Count)
				for (int i = _Rows.Count; i <= rowIndex; i++)
					_Rows.Add(new Row(this, i));

			return _Rows[rowIndex];
		}

		internal void ResetRowNumbersFrom(int index)
		{
			for (int i = index; i < _Rows.Count; i++)
				_Rows[i].RowIndex = i;
		}

		internal bool IsCellMerged(Cell cell)
		{
			foreach (Range range in _MergedCells)
				if (range.Contains(cell))
					return true;

			return false;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Add a named range to the book with limited scope with this sheet
		/// </summary>
		/// <param name="range">Range to be named</param>
		/// <param name="name">Name of the range</param>
		/// <remarks>This property always adds sheet level named ranges. To add globally valid 
		/// ranges, use <see cref="TMP.ExcelXml.Range.Name"/> property in
		/// <see cref="TMP.ExcelXml.Range"/>.</remarks>
		/// <remarks>Range may not necessarily reside in this sheet</remarks>
		public void AddNamedRange(Range range, string name)
		{
			if (name.IsNullOrEmpty())
				throw new ArgumentNullException("name");

			if (Range.IsSystemRangeName(name))
				throw new ArgumentException(name + "is a excel internal range name");

			GetParentBook().AddNamedRange(range, name, this);
		}

		/// <summary>
		/// Removes the current print area, if set
		/// </summary>
		public void RemovePrintArea()
		{
			PrintArea = false;

			GetParentBook().RemoveNamedRange("Print_Area", this);
		}
		#endregion

		#region Public Sheet Information methods
		/// <summary>
		/// Returns the cell at a given position
		/// </summary>
		/// <param name="colIndex">Index of the <see cref="TMP.ExcelXml.Cell"/> starting from 0</param>
		/// <param name="rowIndex">Index of the <see cref="TMP.ExcelXml.Row"/> starting from 0</param>
		/// <returns><see cref="TMP.ExcelXml.Cell"/> reference to the requested cell</returns>
		public Cell this[int colIndex, int rowIndex]
		{
			get
			{
				return Cells(colIndex, rowIndex);
			}
		}

		/// <summary>
		/// Returns the row at a given position
		/// </summary>
		/// <param name="rowIndex">Index of the <see cref="TMP.ExcelXml.Row"/> starting from 0</param>
		/// <returns><see cref="TMP.ExcelXml.Row"/> reference to the requested row</returns>
		public Row this[int rowIndex]
		{
			get
			{
				return GetRowByIndex(rowIndex);
			}
		}

		/// <summary>
		/// Returns the column at a given position
		/// </summary>
		/// <param name="colIndex">Index of the <see cref="TMP.ExcelXml.Column"/> starting from 0</param>
		/// <returns><see cref="TMP.ExcelXml.Column"/> reference to the requested column</returns>
		public Column Columns(int colIndex)
		{
			if (colIndex < 0)
				throw new ArgumentOutOfRangeException("colIndex");

			if (colIndex + 1 > _Columns.Count)
				for (int i = _Columns.Count; i <= colIndex; i++)
					_Columns.Add(new Column(this));

			return _Columns[colIndex];
		}

		/// <summary>
		/// Returns the number of rows present in the sheet
		/// </summary>
		public int RowCount
		{
			get
			{
				return _Rows.Count;
			}
		}

		/// <summary>
		/// Number of columns in this worksheet
		/// </summary>
		public int ColumnCount
		{
			get
			{
				return maxColumnAddressed;
			}
		}
		#endregion

		#region Collection Methods
		/// <summary>
		/// Get a cell enumerator
		/// </summary>
		/// <returns>returns IEnumerator&gt;Cell&lt;</returns>
		public IEnumerator<Cell> GetEnumerator()
		{
			for (int i = 0; i < _Rows.Count; i++)
			{
				for (int j = 0; j <= maxColumnAddressed; j++)
				{
					yield return this[j, i];
				}
			}
		}

		/// <summary>
		/// Get a object enumerator
		/// </summary>
		/// <returns>returns IEnumerator&gt;Cell&lt;</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Row Addition, Insertion & Deletion methods
		/// <summary>
		/// Delete this sheet from the workbook
		/// </summary>
		public void Delete()
		{
			GetParentBook().DeleteSheet(this);
		}

		/// <summary>
		/// Delete a specific number of rows starting from a row index
		/// </summary>
		/// <param name="index">Index of row from which the rows are deleted</param>
		/// <param name="numberOfRows">Number of rows to delete</param>
		/// <remarks>The rows are removed and rows after the row specified 
		/// are cascaded upwards.</remarks>
		public void DeleteRows(int index, int numberOfRows)
		{
			if (numberOfRows < 0)
				return;

			if (index < 0 || index >= _Rows.Count)
				return;

			if (index + numberOfRows > _Rows.Count)
			{
				numberOfRows = _Rows.Count - index;
			}

			for (int i = index; i < (index + numberOfRows); i++)
			{
				_Rows[index].Empty();

				_Rows.RemoveAt(index);
			}

			ResetRowNumbersFrom(index);
		}

		/// <summary>
		/// Delete a specific number of rows starting from a row instance
		/// </summary>
		/// <param name="row">Instance of row from which the rows are deleted</param>
		/// <param name="numberOfRows">Number of rows to delete</param>
		/// <remarks>The rows are removed and rows after the row specified 
		/// are cascaded upwards.</remarks>
		public void DeleteRows(Row row, int numberOfRows)
		{
			if (row != null)
				DeleteRows(_Rows.FindIndex(r => r == row), numberOfRows);
		}

		/// <summary>
		/// Delete a specific number of rows starting from a row index
		/// </summary>
		/// <param name="index">Index of row from which the rows are deleted</param>
		/// <param name="numberOfRows">Number of rows to delete</param>
		/// <param name="cascade">if true, the rows are removed and rows after the row
		/// specified are cascaded upwards. if false, the rows are only emptied</param>
		public void DeleteRows(int index, int numberOfRows, bool cascade)
		{
			if (cascade)
			{
				DeleteRows(index, numberOfRows);

				return;
			}

			if (index < 0 || index >= _Rows.Count)
				return;

			if (index + numberOfRows > _Rows.Count)
				numberOfRows = _Rows.Count - index;

			for (int i = index; i < (index + numberOfRows); i++)
			{
				foreach (Cell cell in _Rows[i]._Cells)
					cell.Empty();
			}
		}

		/// <summary>
		/// Delete a specific number of rows starting from a row instance
		/// </summary>
		/// <param name="row">Instance of row from which the rows are deleted</param>
		/// <param name="numberOfRows">Number of rows to delete</param>
		/// <param name="cascade">if true, the rows are removed and rows after the row
		/// specified are cascaded upwards. if false, the rows are only emptied</param>
		public void DeleteRows(Row row, int numberOfRows, bool cascade)
		{
			if (row != null)
				DeleteRows(_Rows.FindIndex(r => r == row), numberOfRows, cascade);
		}

		/// <summary>
		/// Deletes a row
		/// </summary>
		/// <param name="index">Index of row to delete</param>
		/// <remarks>The row is removed and rows after the row specified 
		/// are cascaded upwards.</remarks>
		public void DeleteRow(int index)
		{
			DeleteRows(index, 1);
		}

		/// <summary>
		/// Deletes a row
		/// </summary>
		/// <param name="row">Instance of row to delete</param>
		/// <remarks>The row is removed and rows after the row specified 
		/// are cascaded upwards.</remarks>
		public void DeleteRow(Row row)
		{
			if (row != null)
				DeleteRow(_Rows.FindIndex(r => r == row));
		}

		/// <summary>
		/// Deletes a row
		/// </summary>
		/// <param name="index">Index of row to delete</param>
		/// <param name="cascade">if true, the row is removed and rows after the row
		/// specified are cascaded upwards. if false, the rows are only emptied</param>
		public void DeleteRow(int index, bool cascade)
		{
			if (cascade)
			{
				DeleteRow(index);

				return;
			}

			if (index < 0 || index >= _Rows.Count)
				return;

			foreach (Cell cell in _Rows[index]._Cells)
				cell.Empty();
		}

		/// <summary>
		/// Deletes a row
		/// </summary>
		/// <param name="row">Instance of row to delete</param>
		/// <param name="cascade">if true, the row is removed and rows after the row
		/// specified are cascaded upwards. if false, the rows are only emptied</param>
		public void DeleteRow(Row row, bool cascade)
		{
			if (row != null)
				DeleteRow(_Rows.FindIndex(r => r == row), cascade);
		}

		/// <summary>
		/// Inserts a specific number of rows before a row
		/// </summary>
		/// <param name="index">Index of row before which the new rows are inserted</param>
		/// <param name="rows">Number of rows to insert</param>
		public void InsertRowsBefore(int index, int rows)
		{
			if (rows < 0)
				return;

			if (index < 0)
				return;

			if (index >= _Rows.Count)
				return;

			for (int i = index; i < (index + rows); i++)
			{
				Row newRow = new Row(this, index);
				_Rows.Insert(index, newRow);
			}

			ResetRowNumbersFrom(index);
		}

		/// <summary>
		/// Inserts a specific number of rows before a row
		/// </summary>
		/// <param name="row">Instance of row before which the new rows are inserted</param>
		/// <param name="rows">Number of rows to insert</param>
		public void InsertRowsBefore(Row row, int rows)
		{
			InsertRowsBefore(_Rows.FindIndex(r => r == row), rows);
		}

		/// <summary>
		/// Inserts a row before another row
		/// </summary>
		/// <param name="index">Index of row before which the new row is to be inserted</param>
		public Row InsertRowBefore(int index)
		{
			if (index < 0)
				return AddRow();

			if (index >= _Rows.Count)
				return this[index];

			InsertRowsBefore(index, 1);

			return _Rows[index];
		}

		/// <summary>
		/// Inserts a row before another row
		/// </summary>
		/// <param name="row">Instance of row before which the new row is to be inserted</param>
		public Row InsertRowBefore(Row row)
		{
			return InsertRowBefore(_Rows.FindIndex(r => r == row));
		}

		/// <summary>
		/// Inserts a specific number of rows after a cell
		/// </summary>
		/// <param name="index">Index of row after which the new rows are inserted</param>
		/// <param name="rows">Number of rows to insert</param>
		public void InsertRowsAfter(int index, int rows)
		{
			if (rows < 0)
				return;

			if (index < 0)
				return;

			if (index >= (_Rows.Count - 1))
				return;

			for (int i = index; i < (index + rows); i++)
			{
				Row newRow = new Row(this, index);

				_Rows.Insert(index + 1, newRow);
			}

			ResetRowNumbersFrom(index);
		}

		/// <summary>
		/// Inserts a specific number of rows after a cell
		/// </summary>
		/// <param name="row">Instance of row after which the new rows are inserted</param>
		/// <param name="rows">Number of rows to insert</param>
		public void InsertRowsAfter(Row row, int rows)
		{
			if (row != null)
				InsertRowsAfter(_Rows.FindIndex(r => r == row), rows);
		}

		/// <summary>
		/// Inserts a row after another row
		/// </summary>
		/// <param name="index">Index of row after which the new row is to be inserted</param>
		public Row InsertRowAfter(int index)
		{
			if (index < 0)
				return AddRow();

			if (index >= (_Rows.Count - 1))
				return this[index + 1];

			InsertRowsAfter(index, 1);

			return _Rows[index];
		}

		/// <summary>
		/// Inserts a row after another row
		/// </summary>
		/// <param name="row">Instance of row after which the new row is to be inserted</param>
		public Row InsertRowAfter(Row row)
		{
			return InsertRowAfter(_Rows.FindIndex(r => r == row));
		}

		/// <summary>
		/// Adds a row at the end of the sheet
		/// </summary>
		/// <returns>The new row instance which is added</returns>
		public Row AddRow()
		{
			return this[_Rows.Count];
		}
		#endregion

		#region Public Column Addition, Insertion & Deletion methods
		/// <summary>
		/// Completely removes a specified a number of columns from a given index
		/// </summary>
		/// <param name="index">Index of column to delete columns from</param>
		/// <param name="numberOfColumns">Number of columns to delete</param>
		/// <param name="cascade">if true, the columns are removed and columns to the right
		/// are cascaded leftwards. if false, the columns are only emptied</param>
		public void DeleteColumns(int index, int numberOfColumns, bool cascade)
		{
			if (index < 0)
				return;

			if (cascade && index < _Columns.Count)
			{
				for (int i = index; i < (index + numberOfColumns); i++)
				{
					if (index < _Columns.Count)
						_Columns.RemoveAt(index);
					else
						break;
				}
			}

			if (index > maxColumnAddressed)
			{
				return;
			}

			foreach (Row row in _Rows)
				if (index < row._Cells.Count)
					row.DeleteCells(index, numberOfColumns, cascade);
		}

		/// <summary>
		/// Completely removes a specified a number of columns from a given index
		/// </summary>
		/// <param name="index">Index of column to delete columns from</param>
		/// <param name="numberOfColumns">Number of columns to delete</param>
		/// <remarks>The columns are removed and columns to the right
		/// are cascaded leftwards</remarks>
		public void DeleteColumns(int index, int numberOfColumns)
		{
			DeleteColumns(index, numberOfColumns, true);
		}

		/// <summary>
		/// Completely removes a column at a given index
		/// </summary>
		/// <param name="index">Index of column to delete columns from</param>
		/// <param name="cascade">if true, the columns are removed and columns to the right
		/// are cascaded leftwards. if false, the columns are only emptied</param>
		public void DeleteColumn(int index, bool cascade)
		{
			DeleteColumns(index, 1, cascade);
		}

		/// <summary>
		/// Completely removes a column at a given index
		/// </summary>
		/// <param name="index">Index of column to delete columns from</param>
		/// <remarks>The column is removed and columns to the right
		/// are cascaded leftwards</remarks>
		public void DeleteColumn(int index)
		{
			DeleteColumns(index, 1, true);
		}

		/// <summary>
		/// Inserts a specified number of columns before a given column index
		/// </summary>
		/// <param name="index">Index of column before which columns should be inserted</param>
		/// <param name="numberOfColumns">Number of columns to insert</param>
		public void InsertColumnsBefore(int index, int numberOfColumns)
		{
			if (index < 0)
				return;

			if (index < _Columns.Count)
			{
				Column column = new Column(this);

				_Columns.Insert(index, column);
			}

			if (index > maxColumnAddressed)
				return;

			foreach (Row row in _Rows)
				if (index < row._Cells.Count)
					row.InsertCellsBefore(index, numberOfColumns);
		}

		/// <summary>
		/// Inserts a column before a given column index
		/// </summary>
		/// <param name="index">Index of column before which new column should be inserted</param>
		public void InsertColumnBefore(int index)
		{
			InsertColumnsBefore(index, 1);
		}

		/// <summary>
		/// Inserts a specified number of columns after a given column index
		/// </summary>
		/// <param name="index">Index of column after which columns should be inserted</param>
		/// <param name="numberOfColumns">Number of columns to insert</param>
		public void InsertColumnsAfter(int index, int numberOfColumns)
		{
			if (index < 0)
				return;

			if (index < (_Columns.Count - 1))
			{
				Column column = new Column(this);

				_Columns.Insert(index + 1, column);
			}

			if (index > (maxColumnAddressed - 1))
				return;

			foreach (Row row in _Rows)
				if (index < row._Cells.Count)
					row.InsertCellsAfter(index, numberOfColumns);
		}

		/// <summary>
		/// Inserts a column after a given column index
		/// </summary>
		/// <param name="index">Index of column after which new column should be inserted</param>
		public void InsertColumnAfter(int index)
		{
			InsertColumnsAfter(index, 1);
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			// Worksheet
			writer.WriteStartElement("Worksheet");
			writer.WriteAttributeString("ss", "Name", null, Name);

			ParentBook.ExportNamedRanges(writer, this);

			// Table
			writer.WriteStartElement("Table");
			writer.WriteAttributeString("ss", "FullColumns", null, "1");
			writer.WriteAttributeString("ss", "FullRows", null, "1");

			if (!StyleID.IsNullOrEmpty() && StyleID != "Default")
				writer.WriteAttributeString("ss", "StyleID", null, StyleID);

			// Start Columns
			foreach (Column col in _Columns)
				col.Export(writer);
			// End Columns

			// Start Rows
			foreach (Row row in _Rows)
				row.Export(writer);
			// End Rows
			// End Table
			writer.WriteEndElement();

			// Write worksheet options
			ExportOptions(writer);

			// Write Autofilter options
			if (AutoFilter)
			{
				string range = GetParentBook().GetAutoFilterRange(this);

				writer.WriteStartElement("", "AutoFilter", "urn:schemas-microsoft-com:office:excel");
				writer.WriteAttributeString("", "Range", null, range);
				writer.WriteEndElement();
			}

			// End Worksheet
			writer.WriteEndElement();
		}

		private void WritePanes(XmlWriter writer)
		{
			string panes;

			if (FreezeLeftColumns > 0 && FreezeTopRows > 0)
				panes = "3210";
			else 
				panes = FreezeLeftColumns > 0 ? "31" : "32";

			// Active pane
			writer.WriteElementString("ActivePane", panes[panes.Length - 1].ToString());

			// All panes recide in Panes 
			writer.WriteStartElement("Panes");
			// Write all panes one by one
			foreach (char c in panes)
			{
				writer.WriteStartElement("Pane");
				writer.WriteElementString("Number", c.ToString());
				writer.WriteEndElement();
			}
			// End Panes
			writer.WriteEndElement();
		}

		private void ExportOptions(XmlWriter writer)
		{
			// Start Worksheet options
			writer.WriteStartElement("", "WorksheetOptions", "urn:schemas-microsoft-com:office:excel");

			PrintOptions.Export(writer);

			writer.WriteElementString("Selected", "");

			if (TabColor != -1)
				writer.WriteElementString("TabColor", TabColor.ToString(
						CultureInfo.InvariantCulture));

			// Pane Info
			if (FreezeLeftColumns > 0 || FreezeTopRows > 0)
			{
				writer.WriteElementString("FreezePanes", "");
				writer.WriteElementString("FrozenNoSplit", "");

				if (FreezeTopRows > 0)
				{
					writer.WriteElementString("SplitHorizontal", FreezeTopRows.ToString(
						CultureInfo.InvariantCulture));
					writer.WriteElementString("TopRowBottomPane", FreezeTopRows.ToString(
						CultureInfo.InvariantCulture));
				}
				if (FreezeLeftColumns > 0)
				{
					writer.WriteElementString("SplitVertical", FreezeLeftColumns.ToString(
						CultureInfo.InvariantCulture));
					writer.WriteElementString("LeftColumnRightPane", FreezeLeftColumns.ToString(
						CultureInfo.InvariantCulture));
				}

				// Panes
				WritePanes(writer);
			}

			// End Worksheet options
			writer.WriteEndElement();
		}
		#endregion
	}
}
