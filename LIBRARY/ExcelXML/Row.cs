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
	/// Row class represents a single row in a worksheet
	/// </summary>
	/// <remarks>
	/// Row class represents a single row in a worksheet.
	/// <para>You cannot directly declare a instance of a row from your code by using
	/// <c>new</c> keyword. The only way to access a row is to retrieve it from
	/// a worksheet.</para>
	/// </remarks>
	public class Row : Styles, IEnumerable<Cell>
	{
		#region Private and Internal fields
		internal List<Cell> _Cells;
		internal Worksheet ParentSheet;
		internal int RowIndex;
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets or sets the row height
		/// </summary>
		public double Height { get; set; }

		/// <summary>
		/// Row is hidden?
		/// </summary>
		public bool Hidden { get; set; }
		#endregion

		#region Constructor
		internal Row(Worksheet parent, int row)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			_Cells = new List<Cell>();

			ParentSheet = parent;
			Height = 0;
			RowIndex = row;

			if (parent.Style != null)
				Style = parent.Style;
		}
		#endregion

		#region Private and Internal methods
		private Cell Cells(int colIndex)
		{
			if (colIndex < 0)
				throw new ArgumentOutOfRangeException("colIndex");

			if (colIndex + 1 > _Cells.Count)
				for (int i = _Cells.Count; i <= colIndex; i++)
					_Cells.Add(new Cell(this, i));

			ParentSheet.maxColumnAddressed = Math.Max(colIndex, ParentSheet.maxColumnAddressed);

			return _Cells[colIndex];
		}

		internal override ExcelXmlWorkbook GetParentBook()
		{
			return ParentSheet.ParentBook;
		}

		internal override void IterateAndApply(IterateFunction ifFunc)
		{
		}

		internal override Cell FirstCell()
		{
			return null;
		}

		internal void ResetCellNumbersFrom(int index)
		{
			for (int i = index; i < _Cells.Count; i++)
				_Cells[i].CellIndex = i;
		}

		internal void Empty()
		{
			ParentSheet = null;

			_Cells.Clear();

			_Cells = null;
		}
		#endregion

		#region Public Row Information methods
		/// <summary>
		/// Returns the cell at a given position
		/// </summary>
		/// <param name="colIndex">Index of the <see cref="TMP.ExcelXml.Cell"/> starting from 0</param>
		/// <returns><see cref="TMP.ExcelXml.Cell"/> reference to the requested cell</returns>
		public Cell this[int colIndex]
		{
			get
			{
				return Cells(colIndex);
			}
		}

		/// <summary>
		/// Returns the number of cell in a row
		/// </summary>
		public int CellCount
		{
			get
			{
				return _Cells.Count;
			}
		}

		/// <summary>
		/// Deletes the row from the parent sheet
		/// </summary>
		public void Delete()
		{
			ParentSheet.DeleteRow(this);
		}
		#endregion

		#region Collection Methods
		/// <summary>
		/// Get a cell enumerator
		/// </summary>
		/// <returns>returns IEnumerator&gt;Cell&lt;</returns>
		public IEnumerator<Cell> GetEnumerator()
		{
			for (int i = 0; i <= ParentSheet.maxColumnAddressed; i++)
			{
				yield return this[i];
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

		#region Public Cell Addition, Insertion & Deletion methods
		/// <summary>
		/// Delete a specific number of cells starting from a cell index
		/// </summary>
		/// <param name="index">Index of cell from which the cells are deleted</param>
		/// <param name="numberOfCells">Number of cells to delete</param>
		/// <param name="cascade">if true, the cells are removed and cells to the right
		/// are cascaded leftwards. if false, the cells are only emptied</param>
		public void DeleteCells(int index, int numberOfCells, bool cascade)
		{
			if (numberOfCells < 0)
				return;

			if (index < 0 || index >= _Cells.Count)
				return;

			if (index + numberOfCells > _Cells.Count)
			{
				numberOfCells = _Cells.Count - index;
			}

			for (int i = index; i < (index + numberOfCells); i++)
			{
				_Cells[index].Empty(!cascade);

				if (cascade)
					_Cells.RemoveAt(index);
			}

			if (cascade)
				ResetCellNumbersFrom(index);
		}

		/// <summary>
		/// Delete a specific number of cells starting from a cell instance
		/// </summary>
		/// <param name="cell">Instance of cell from which the cells are deleted</param>
		/// <param name="numberOfCells">Number of cells to delete</param>
		/// <param name="cascade">if true, the cells are removed and cells to the right
		/// are cascaded leftwards. if false, the cells are only emptied</param>
		public void DeleteCells(Cell cell, int numberOfCells, bool cascade)
		{
			if (cell != null)
				DeleteCells(_Cells.FindIndex(r => r == cell), numberOfCells, cascade);
		}

		/// <summary>
		/// Delete a specific number of cells starting from a cell index
		/// </summary>
		/// <param name="index">Index of cell from which the cells are deleted</param>
		/// <param name="numberOfCells">Number of cells to delete</param>
		/// <remarks>The cells are removed and cells to the right are cascaded
		/// leftwards.</remarks>
		public void DeleteCells(int index, int numberOfCells)
		{
			DeleteCells(index, numberOfCells, true);
		}

		/// <summary>
		/// Delete a specific number of cells starting from a cell instance
		/// </summary>
		/// <param name="cell">Instance of cell from which the cells are deleted</param>
		/// <param name="numberOfCells">Number of cells to delete</param>
		/// <remarks>The cells are removed and cells to the right are cascaded
		/// leftwards.</remarks>
		public void DeleteCells(Cell cell, int numberOfCells)
		{
			if (cell != null)
				DeleteCells(_Cells.FindIndex(r => r == cell), numberOfCells, true);
		}

		/// <summary>
		/// Deletes a cell
		/// </summary>
		/// <param name="index">Index of cell to delete</param>
		public void DeleteCell(int index)
		{
			DeleteCells(index, 1, true);
		}

		/// <summary>
		/// Deletes a cell
		/// </summary>
		/// <param name="cell">Instance of cell to delete</param>
		public void DeleteCell(Cell cell)
		{
			if (cell != null)
				DeleteCells(_Cells.FindIndex(r => r == cell), 1, true);
		}

		/// <summary>
		/// Deletes a cell
		/// </summary>
		/// <param name="index">Index of cell to delete</param>
		/// <param name="cascade">if true, the cell is removed and cells to the right
		/// are cascaded leftwards. if false, the cell is only emptied</param>
		public void DeleteCell(int index, bool cascade)
		{
			DeleteCells(index, 1, cascade);
		}

		/// <summary>
		/// Deletes a cell
		/// </summary>
		/// <param name="cell">Instance of cell to delete</param>
		/// <param name="cascade">if true, the cell is removed and cells to the right
		/// are cascaded leftwards. if false, the cell is only emptied</param>
		public void DeleteCell(Cell cell, bool cascade)
		{
			if (cell != null)
				DeleteCells(_Cells.FindIndex(r => r == cell), 1, cascade);
		}

		/// <summary>
		/// Inserts a specific number of cells before a cell
		/// </summary>
		/// <param name="index">Index of cell before which the cells are to be inserted</param>
		/// <param name="cells">Number of cells to insert</param>
		public void InsertCellsBefore(int index, int cells)
		{
			if (cells < 0)
				return;

			if (index < 0)
				return;

			if (index >= _Cells.Count)
				return;

			for (int i = index; i < (index + cells); i++)
			{
				Cell newCell = new Cell(this, index);
				_Cells.Insert(index, newCell);
			}

			ResetCellNumbersFrom(index);
		}

		/// <summary>
		/// Inserts a specific number of cells before a cell
		/// </summary>
		/// <param name="cell">Instance of cell before which the cells are to be inserted</param>
		/// <param name="cells">Number of cells to insert</param>
		public void InsertCellsBefore(Cell cell, int cells)
		{
			InsertCellsBefore(_Cells.FindIndex(r => r == cell), cells);
		}

		/// <summary>
		/// Inserts a cell before another cell
		/// </summary>
		/// <param name="index">Index of cell before which the cell is to be inserted</param>
		public Cell InsertCellBefore(int index)
		{
			if (index < 0)
				return AddCell();

			if (index >= _Cells.Count)
				return this[index];

			InsertCellsBefore(index, 1);

			return _Cells[index];
		}

		/// <summary>
		/// Inserts a cell before another cell
		/// </summary>
		/// <param name="cell">Instance of cell before which the cell is to be inserted</param>
		public Cell InsertCellBefore(Cell cell)
		{
			return InsertCellBefore(_Cells.FindIndex(r => r == cell));
		}

		/// <summary>
		/// Inserts a specific number of cells after a cell
		/// </summary>
		/// <param name="index">Index of cell after which the cells are to be inserted</param>
		/// <param name="cells">Number of cells to insert</param>
		public void InsertCellsAfter(int index, int cells)
		{
			if (cells < 0)
				return;

			if (index < 0)
				return;

			if (index >= (_Cells.Count - 1))
				return;

			for (int i = index; i < (index + cells); i++)
			{
				Cell newCell = new Cell(this, index);

				_Cells.Insert(index + 1, newCell);
			}

			ResetCellNumbersFrom(index);
		}

		/// <summary>
		/// Inserts a specific number of cells after a cell
		/// </summary>
		/// <param name="cell">Instance of cell after which the cells are to be inserted</param>
		/// <param name="cells">Number of cells to insert</param>
		public void InsertCellsAfter(Cell cell, int cells)
		{
			if (cell != null)
				InsertCellsAfter(_Cells.FindIndex(r => r == cell), cells);
		}

		/// <summary>
		/// Inserts a cell after another cell
		/// </summary>
		/// <param name="index">Index of cell after which the cell is to be inserted</param>
		public Cell InsertCellAfter(int index)
		{
			if (index < 0)
				return AddCell();

			if (index >= (_Cells.Count - 1))
				return this[index + 1];

			InsertCellsAfter(index, 1);

			return _Cells[index];
		}

		/// <summary>
		/// Inserts a cell after another cell
		/// </summary>
		/// <param name="cell">Instance of cell after which the cell is to be inserted</param>
		public Cell InsertCellAfter(Cell cell)
		{
			return InsertCellAfter(_Cells.FindIndex(r => r == cell));
		}

		/// <summary>
		/// Adds a cells to the end of the row
		/// </summary>
		/// <returns>Instance of the newly created cell</returns>
		public Cell AddCell()
		{
			return this[_Cells.Count];
		}

		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			// Row
			writer.WriteStartElement("Row");

			if (!StyleID.IsNullOrEmpty() && ParentSheet.StyleID != StyleID && StyleID != "Default")
				writer.WriteAttributeString("ss", "StyleID", null, StyleID);

			if (Height != 0)
			{
				writer.WriteAttributeString("ss", "AutoFitHeight", null, "0");
				writer.WriteAttributeString("ss", "Height", null, Height.ToString(CultureInfo.InvariantCulture));
			}

			if (Hidden)
				writer.WriteAttributeString("ss", "Hidden", null, "1");

			bool printIndex = false;

			// Start Cells
			foreach (Cell cell in _Cells)
			{
				//if (cell.IsEmpty() && !cell.MergeStart)
				{
					printIndex = true;
				}
				//else
				//{
					cell.Export(writer, printIndex);

//					printIndex = false;
				//}
			}
			// End Cells

			// End Row
			writer.WriteEndElement();
		}
		#endregion
	}
}
