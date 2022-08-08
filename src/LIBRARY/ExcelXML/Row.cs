namespace TMP.ExcelXml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml;
    using TMP.Extensions;

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
        internal List<Cell> cells;
        internal Worksheet parentSheet;
        internal int rowIndex;
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
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.cells = new List<Cell>();

            this.parentSheet = parent;
            this.Height = 0;
            this.rowIndex = row;

            if (parent.Style != null)
            {
                this.Style = parent.Style;
            }
        }
        #endregion

        #region Private and Internal methods
        private Cell Cells(int colIndex)
        {
            if (colIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(colIndex));
            }

            if (colIndex + 1 > this.cells.Count)
            {
                for (int i = this.cells.Count; i <= colIndex; i++)
                {
                    this.cells.Add(new Cell(this, i));
                }
            }

            this.parentSheet.MaxColumnAddressed = Math.Max(colIndex, this.parentSheet.MaxColumnAddressed);

            return this.cells[colIndex];
        }

        internal override ExcelXmlWorkbook GetParentBook()
        {
            return this.parentSheet.ParentBook;
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
            for (int i = index; i < this.cells.Count; i++)
            {
                this.cells[i].CellIndex = i;
            }
        }

        internal void Empty()
        {
            this.parentSheet = null;

            this.cells.Clear();

            this.cells = null;
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
                return this.Cells(colIndex);
            }
        }

        /// <summary>
        /// Returns the number of cell in a row
        /// </summary>
        public int CellCount => this.cells.Count;

        /// <summary>
        /// Deletes the row from the parent sheet
        /// </summary>
        public void Delete()
        {
            this.parentSheet.DeleteRow(this);
        }
        #endregion

        #region Collection Methods

        /// <summary>
        /// Get a cell enumerator
        /// </summary>
        /// <returns>returns IEnumerator&gt;Cell&lt;</returns>
        public IEnumerator<Cell> GetEnumerator()
        {
            for (int i = 0; i <= this.parentSheet.MaxColumnAddressed; i++)
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
            {
                return;
            }

            if (index < 0 || index >= this.cells.Count)
            {
                return;
            }

            if (index + numberOfCells > this.cells.Count)
            {
                numberOfCells = this.cells.Count - index;
            }

            for (int i = index; i < (index + numberOfCells); i++)
            {
                this.cells[index].Empty(!cascade);

                if (cascade)
                {
                    this.cells.RemoveAt(index);
                }
            }

            if (cascade)
            {
                this.ResetCellNumbersFrom(index);
            }
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
            {
                this.DeleteCells(this.cells.FindIndex(r => r == cell), numberOfCells, cascade);
            }
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
            this.DeleteCells(index, numberOfCells, true);
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
            {
                this.DeleteCells(this.cells.FindIndex(r => r == cell), numberOfCells, true);
            }
        }

        /// <summary>
        /// Deletes a cell
        /// </summary>
        /// <param name="index">Index of cell to delete</param>
        public void DeleteCell(int index)
        {
            this.DeleteCells(index, 1, true);
        }

        /// <summary>
        /// Deletes a cell
        /// </summary>
        /// <param name="cell">Instance of cell to delete</param>
        public void DeleteCell(Cell cell)
        {
            if (cell != null)
            {
                this.DeleteCells(this.cells.FindIndex(r => r == cell), 1, true);
            }
        }

        /// <summary>
        /// Deletes a cell
        /// </summary>
        /// <param name="index">Index of cell to delete</param>
        /// <param name="cascade">if true, the cell is removed and cells to the right
        /// are cascaded leftwards. if false, the cell is only emptied</param>
        public void DeleteCell(int index, bool cascade)
        {
            this.DeleteCells(index, 1, cascade);
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
            {
                this.DeleteCells(this.cells.FindIndex(r => r == cell), 1, cascade);
            }
        }

        /// <summary>
        /// Inserts a specific number of cells before a cell
        /// </summary>
        /// <param name="index">Index of cell before which the cells are to be inserted</param>
        /// <param name="cells">Number of cells to insert</param>
        public void InsertCellsBefore(int index, int cells)
        {
            if (cells < 0)
            {
                return;
            }

            if (index < 0)
            {
                return;
            }

            if (index >= this.cells.Count)
            {
                return;
            }

            for (int i = index; i < (index + cells); i++)
            {
                Cell newCell = new Cell(this, index);
                this.cells.Insert(index, newCell);
            }

            this.ResetCellNumbersFrom(index);
        }

        /// <summary>
        /// Inserts a specific number of cells before a cell
        /// </summary>
        /// <param name="cell">Instance of cell before which the cells are to be inserted</param>
        /// <param name="cells">Number of cells to insert</param>
        public void InsertCellsBefore(Cell cell, int cells)
        {
            this.InsertCellsBefore(this.cells.FindIndex(r => r == cell), cells);
        }

        /// <summary>
        /// Inserts a cell before another cell
        /// </summary>
        /// <param name="index">Index of cell before which the cell is to be inserted</param>
        public Cell InsertCellBefore(int index)
        {
            if (index < 0)
            {
                return this.AddCell();
            }

            if (index >= this.cells.Count)
            {
                return this[index];
            }

            this.InsertCellsBefore(index, 1);

            return this.cells[index];
        }

        /// <summary>
        /// Inserts a cell before another cell
        /// </summary>
        /// <param name="cell">Instance of cell before which the cell is to be inserted</param>
        public Cell InsertCellBefore(Cell cell)
        {
            return this.InsertCellBefore(this.cells.FindIndex(r => r == cell));
        }

        /// <summary>
        /// Inserts a specific number of cells after a cell
        /// </summary>
        /// <param name="index">Index of cell after which the cells are to be inserted</param>
        /// <param name="cells">Number of cells to insert</param>
        public void InsertCellsAfter(int index, int cells)
        {
            if (cells < 0)
            {
                return;
            }

            if (index < 0)
            {
                return;
            }

            if (index >= (this.cells.Count - 1))
            {
                return;
            }

            for (int i = index; i < (index + cells); i++)
            {
                Cell newCell = new Cell(this, index);

                this.cells.Insert(index + 1, newCell);
            }

            this.ResetCellNumbersFrom(index);
        }

        /// <summary>
        /// Inserts a specific number of cells after a cell
        /// </summary>
        /// <param name="cell">Instance of cell after which the cells are to be inserted</param>
        /// <param name="cells">Number of cells to insert</param>
        public void InsertCellsAfter(Cell cell, int cells)
        {
            if (cell != null)
            {
                this.InsertCellsAfter(this.cells.FindIndex(r => r == cell), cells);
            }
        }

        /// <summary>
        /// Inserts a cell after another cell
        /// </summary>
        /// <param name="index">Index of cell after which the cell is to be inserted</param>
        public Cell InsertCellAfter(int index)
        {
            if (index < 0)
            {
                return this.AddCell();
            }

            if (index >= (this.cells.Count - 1))
            {
                return this[index + 1];
            }

            this.InsertCellsAfter(index, 1);

            return this.cells[index];
        }

        /// <summary>
        /// Inserts a cell after another cell
        /// </summary>
        /// <param name="cell">Instance of cell after which the cell is to be inserted</param>
        public Cell InsertCellAfter(Cell cell)
        {
            return this.InsertCellAfter(this.cells.FindIndex(r => r == cell));
        }

        /// <summary>
        /// Adds a cells to the end of the row
        /// </summary>
        /// <returns>Instance of the newly created cell</returns>
        public Cell AddCell()
        {
            return this[this.cells.Count];
        }

        #endregion

        #region Export
        internal void Export(XmlWriter writer)
        {
            // Row
            writer.WriteStartElement("Row");

            if (!this.StyleID.IsNullOrEmpty() && this.parentSheet.StyleID != this.StyleID && this.StyleID != "Default")
            {
                writer.WriteAttributeString("ss", "StyleID", null, this.StyleID);
            }

            if (this.Height != 0)
            {
                writer.WriteAttributeString("ss", "AutoFitHeight", null, "0");
                writer.WriteAttributeString("ss", "Height", null, this.Height.ToString(CultureInfo.InvariantCulture));
            }

            if (this.Hidden)
            {
                writer.WriteAttributeString("ss", "Hidden", null, "1");
            }

            bool printIndex = false;

            // Start Cells
            foreach (Cell cell in this.cells)
            {
                // if (cell.IsEmpty() && !cell.MergeStart)
                {
                    printIndex = true;
                }

                // else
                // {
                cell.Export(writer, printIndex);

                // printIndex = false;
                // }
            }

            // End Cells

            // End Row
            writer.WriteEndElement();
        }
        #endregion
    }
}
