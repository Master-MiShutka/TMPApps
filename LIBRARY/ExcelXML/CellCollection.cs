using System;
using System.Collections.Generic;

namespace TMP.ExcelXml
{
	/// <summary>
	/// Represents a strongly typed list of cells that can be accessed by index.
	/// </summary>
	public class CellCollection : List<Cell>
	{
		/// <summary>
		/// Adds a range to the collection
		/// </summary>
		/// <param name="range">Range to add</param>
		public void Add(Range range)
		{
			foreach (Cell cell in range)
				Add(cell);
		}

		/// <summary>
		/// Adds a worksheet to the collection
		/// </summary>
		/// <param name="ws">Worksheet to add</param>
		public void Add(Worksheet ws)
		{
			foreach (Cell cell in ws)
				Add(cell);
		}

		/// <summary>
		/// Adds a row to the collection
		/// </summary>
		/// <param name="row">Row to add</param>
		public void Add(Row row)
		{
			foreach (Cell cell in row)
				Add(cell);
		}

		/// <summary>
		/// Adds a single cell to the collection if it matches the filter condition
		/// </summary>
		/// <param name="cell">Cell to add</param>
		/// <param name="filterCondition">Filter predicate</param>
		public void Add(Cell cell, Predicate<Cell> filterCondition)
		{
			if (filterCondition(cell))
				Add(cell);
		}

		/// <summary>
		/// Adds a range to the collection if it matches the filter condition
		/// </summary>
		/// <param name="range">Range to add</param>
		/// <param name="filterCondition">Filter predicate</param>
		public void Add(Range range, Predicate<Cell> filterCondition)
		{
			foreach (Cell cell in range)
			{
				if (filterCondition(cell))
					Add(cell);
			}
		}

		/// <summary>
		/// Adds a worksheet to the collection if it matches the filter condition
		/// </summary>
		/// <param name="ws">Worksheet to add</param>
		/// <param name="filterCondition">Filter predicate</param>
		public void Add(Worksheet ws, Predicate<Cell> filterCondition)
		{
			foreach (Cell cell in ws)
			{
				if (filterCondition(cell))
					Add(cell);
			}
		}

		/// <summary>
		/// Adds a row to the collection if it matches the filter condition
		/// </summary>
		/// <param name="row">Row to add</param>
		/// <param name="filterCondition">Filter predicate</param>
		public void Add(Row row, Predicate<Cell> filterCondition)
		{
			foreach (Cell cell in row)
			{
				if (filterCondition(cell))
					Add(cell);
			}
		}
	}
}
