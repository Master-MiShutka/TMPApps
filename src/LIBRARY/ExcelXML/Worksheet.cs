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

        private List<Column> columns;

        #endregion

        #region Internal Properties

        internal int MaxColumnAddressed { get; set; }

        internal bool AutoFilter { get; set; }
        internal bool PrintArea { get; set; }

        internal List<Row> Rows { get; set; }
        internal List<Range> MergedCells { get; set; }

        internal ExcelXmlWorkbook ParentBook { get; set; }

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
            get => this.sheetName;

            set
            {
                if (!value.IsNullOrEmpty())
                {
                    Worksheet ws = this.GetParentBook()[this.sheetName];

                    if (ws == null || ws == this)
                    {
                        this.sheetName = value.Trim();
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
        public bool IsPrintAreaSet => this.PrintArea;
        #endregion

        #region Constructor
        internal Worksheet(ExcelXmlWorkbook parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.ParentBook = parent;

            this.PrintOptions = new PrintOptions();

            this.PrintOptions.Layout = PageLayout.None;
            this.PrintOptions.Orientation = PageOrientation.None;

            this.Rows = new List<Row>();
            this.columns = new List<Column>();
            this.MergedCells = new List<Range>();

            this.TabColor = -1;

            this.PrintOptions.FitHeight = 1;
            this.PrintOptions.FitWidth = 1;
            this.PrintOptions.Scale = 100;

            this.PrintOptions.ResetMargins();
        }
        #endregion

        #region Private and Internal methods
        internal override ExcelXmlWorkbook GetParentBook()
        {
            return this.ParentBook;
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
            {
                throw new ArgumentOutOfRangeException(nameof(colIndex));
            }

            if (rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if (rowIndex + 1 > this.Rows.Count)
            {
                for (int i = this.Rows.Count; i <= rowIndex; i++)
                {
                    this.Rows.Add(new Row(this, i));
                }
            }

            if (colIndex + 1 > this.Rows[rowIndex].cells.Count)
            {
                for (int i = this.Rows[rowIndex].cells.Count; i <= colIndex; i++)
                {
                    this.Rows[rowIndex].cells.Add(new Cell(this.Rows[rowIndex], i));
                }
            }

            this.MaxColumnAddressed = Math.Max(colIndex, this.MaxColumnAddressed);

            return this.Rows[rowIndex].cells[colIndex];
        }

        internal Row GetRowByIndex(int rowIndex)
        {
            if (rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if (rowIndex + 1 > this.Rows.Count)
            {
                for (int i = this.Rows.Count; i <= rowIndex; i++)
                {
                    this.Rows.Add(new Row(this, i));
                }
            }

            return this.Rows[rowIndex];
        }

        internal void ResetRowNumbersFrom(int index)
        {
            for (int i = index; i < this.Rows.Count; i++)
            {
                this.Rows[i].rowIndex = i;
            }
        }

        internal bool IsCellMerged(Cell cell)
        {
            foreach (Range range in this.MergedCells)
            {
                if (range.Contains(cell))
                {
                    return true;
                }
            }

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
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (Range.IsSystemRangeName(name))
            {
                throw new ArgumentException(name + "is a excel internal range name");
            }

            this.GetParentBook().AddNamedRange(range, name, this);
        }

        /// <summary>
        /// Removes the current print area, if set
        /// </summary>
        public void RemovePrintArea()
        {
            this.PrintArea = false;

            this.GetParentBook().RemoveNamedRange("Print_Area", this);
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
                return this.Cells(colIndex, rowIndex);
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
                return this.GetRowByIndex(rowIndex);
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
            {
                throw new ArgumentOutOfRangeException(nameof(colIndex));
            }

            if (colIndex + 1 > this.columns.Count)
            {
                for (int i = this.columns.Count; i <= colIndex; i++)
                {
                    this.columns.Add(new Column(this));
                }
            }

            return this.columns[colIndex];
        }

        /// <summary>
        /// Returns the number of rows present in the sheet
        /// </summary>
        public int RowCount => this.Rows.Count;

        /// <summary>
        /// Number of columns in this worksheet
        /// </summary>
        public int ColumnCount => this.MaxColumnAddressed;
        #endregion

        #region Collection Methods

        /// <summary>
        /// Get a cell enumerator
        /// </summary>
        /// <returns>returns IEnumerator&gt;Cell&lt;</returns>
        public IEnumerator<Cell> GetEnumerator()
        {
            for (int i = 0; i < this.Rows.Count; i++)
            {
                for (int j = 0; j <= this.MaxColumnAddressed; j++)
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
            this.GetParentBook().DeleteSheet(this);
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
            {
                return;
            }

            if (index < 0 || index >= this.Rows.Count)
            {
                return;
            }

            if (index + numberOfRows > this.Rows.Count)
            {
                numberOfRows = this.Rows.Count - index;
            }

            for (int i = index; i < (index + numberOfRows); i++)
            {
                this.Rows[index].Empty();

                this.Rows.RemoveAt(index);
            }

            this.ResetRowNumbersFrom(index);
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
            {
                this.DeleteRows(this.Rows.FindIndex(r => r == row), numberOfRows);
            }
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
                this.DeleteRows(index, numberOfRows);

                return;
            }

            if (index < 0 || index >= this.Rows.Count)
            {
                return;
            }

            if (index + numberOfRows > this.Rows.Count)
            {
                numberOfRows = this.Rows.Count - index;
            }

            for (int i = index; i < (index + numberOfRows); i++)
            {
                foreach (Cell cell in this.Rows[i].cells)
                {
                    cell.Empty();
                }
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
            {
                this.DeleteRows(this.Rows.FindIndex(r => r == row), numberOfRows, cascade);
            }
        }

        /// <summary>
        /// Deletes a row
        /// </summary>
        /// <param name="index">Index of row to delete</param>
        /// <remarks>The row is removed and rows after the row specified
        /// are cascaded upwards.</remarks>
        public void DeleteRow(int index)
        {
            this.DeleteRows(index, 1);
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
            {
                this.DeleteRow(this.Rows.FindIndex(r => r == row));
            }
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
                this.DeleteRow(index);

                return;
            }

            if (index < 0 || index >= this.Rows.Count)
            {
                return;
            }

            foreach (Cell cell in this.Rows[index].cells)
            {
                cell.Empty();
            }
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
            {
                this.DeleteRow(this.Rows.FindIndex(r => r == row), cascade);
            }
        }

        /// <summary>
        /// Inserts a specific number of rows before a row
        /// </summary>
        /// <param name="index">Index of row before which the new rows are inserted</param>
        /// <param name="rows">Number of rows to insert</param>
        public void InsertRowsBefore(int index, int rows)
        {
            if (rows < 0)
            {
                return;
            }

            if (index < 0)
            {
                return;
            }

            if (index >= this.Rows.Count)
            {
                return;
            }

            for (int i = index; i < (index + rows); i++)
            {
                Row newRow = new Row(this, index);
                this.Rows.Insert(index, newRow);
            }

            this.ResetRowNumbersFrom(index);
        }

        /// <summary>
        /// Inserts a specific number of rows before a row
        /// </summary>
        /// <param name="row">Instance of row before which the new rows are inserted</param>
        /// <param name="rows">Number of rows to insert</param>
        public void InsertRowsBefore(Row row, int rows)
        {
            this.InsertRowsBefore(this.Rows.FindIndex(r => r == row), rows);
        }

        /// <summary>
        /// Inserts a row before another row
        /// </summary>
        /// <param name="index">Index of row before which the new row is to be inserted</param>
        public Row InsertRowBefore(int index)
        {
            if (index < 0)
            {
                return this.AddRow();
            }

            if (index >= this.Rows.Count)
            {
                return this[index];
            }

            this.InsertRowsBefore(index, 1);

            return this.Rows[index];
        }

        /// <summary>
        /// Inserts a row before another row
        /// </summary>
        /// <param name="row">Instance of row before which the new row is to be inserted</param>
        public Row InsertRowBefore(Row row)
        {
            return this.InsertRowBefore(this.Rows.FindIndex(r => r == row));
        }

        /// <summary>
        /// Inserts a specific number of rows after a cell
        /// </summary>
        /// <param name="index">Index of row after which the new rows are inserted</param>
        /// <param name="rows">Number of rows to insert</param>
        public void InsertRowsAfter(int index, int rows)
        {
            if (rows < 0)
            {
                return;
            }

            if (index < 0)
            {
                return;
            }

            if (index >= (this.Rows.Count - 1))
            {
                return;
            }

            for (int i = index; i < (index + rows); i++)
            {
                Row newRow = new Row(this, index);

                this.Rows.Insert(index + 1, newRow);
            }

            this.ResetRowNumbersFrom(index);
        }

        /// <summary>
        /// Inserts a specific number of rows after a cell
        /// </summary>
        /// <param name="row">Instance of row after which the new rows are inserted</param>
        /// <param name="rows">Number of rows to insert</param>
        public void InsertRowsAfter(Row row, int rows)
        {
            if (row != null)
            {
                this.InsertRowsAfter(this.Rows.FindIndex(r => r == row), rows);
            }
        }

        /// <summary>
        /// Inserts a row after another row
        /// </summary>
        /// <param name="index">Index of row after which the new row is to be inserted</param>
        public Row InsertRowAfter(int index)
        {
            if (index < 0)
            {
                return this.AddRow();
            }

            if (index >= (this.Rows.Count - 1))
            {
                return this[index + 1];
            }

            this.InsertRowsAfter(index, 1);

            return this.Rows[index];
        }

        /// <summary>
        /// Inserts a row after another row
        /// </summary>
        /// <param name="row">Instance of row after which the new row is to be inserted</param>
        public Row InsertRowAfter(Row row)
        {
            return this.InsertRowAfter(this.Rows.FindIndex(r => r == row));
        }

        /// <summary>
        /// Adds a row at the end of the sheet
        /// </summary>
        /// <returns>The new row instance which is added</returns>
        public Row AddRow()
        {
            return this[this.Rows.Count];
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
            {
                return;
            }

            if (cascade && index < this.columns.Count)
            {
                for (int i = index; i < (index + numberOfColumns); i++)
                {
                    if (index < this.columns.Count)
                    {
                        this.columns.RemoveAt(index);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (index > this.MaxColumnAddressed)
            {
                return;
            }

            foreach (Row row in this.Rows)
            {
                if (index < row.cells.Count)
                {
                    row.DeleteCells(index, numberOfColumns, cascade);
                }
            }
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
            this.DeleteColumns(index, numberOfColumns, true);
        }

        /// <summary>
        /// Completely removes a column at a given index
        /// </summary>
        /// <param name="index">Index of column to delete columns from</param>
        /// <param name="cascade">if true, the columns are removed and columns to the right
        /// are cascaded leftwards. if false, the columns are only emptied</param>
        public void DeleteColumn(int index, bool cascade)
        {
            this.DeleteColumns(index, 1, cascade);
        }

        /// <summary>
        /// Completely removes a column at a given index
        /// </summary>
        /// <param name="index">Index of column to delete columns from</param>
        /// <remarks>The column is removed and columns to the right
        /// are cascaded leftwards</remarks>
        public void DeleteColumn(int index)
        {
            this.DeleteColumns(index, 1, true);
        }

        /// <summary>
        /// Inserts a specified number of columns before a given column index
        /// </summary>
        /// <param name="index">Index of column before which columns should be inserted</param>
        /// <param name="numberOfColumns">Number of columns to insert</param>
        public void InsertColumnsBefore(int index, int numberOfColumns)
        {
            if (index < 0)
            {
                return;
            }

            if (index < this.columns.Count)
            {
                Column column = new Column(this);

                this.columns.Insert(index, column);
            }

            if (index > this.MaxColumnAddressed)
            {
                return;
            }

            foreach (Row row in this.Rows)
            {
                if (index < row.cells.Count)
                {
                    row.InsertCellsBefore(index, numberOfColumns);
                }
            }
        }

        /// <summary>
        /// Inserts a column before a given column index
        /// </summary>
        /// <param name="index">Index of column before which new column should be inserted</param>
        public void InsertColumnBefore(int index)
        {
            this.InsertColumnsBefore(index, 1);
        }

        /// <summary>
        /// Inserts a specified number of columns after a given column index
        /// </summary>
        /// <param name="index">Index of column after which columns should be inserted</param>
        /// <param name="numberOfColumns">Number of columns to insert</param>
        public void InsertColumnsAfter(int index, int numberOfColumns)
        {
            if (index < 0)
            {
                return;
            }

            if (index < (this.columns.Count - 1))
            {
                Column column = new Column(this);

                this.columns.Insert(index + 1, column);
            }

            if (index > (this.MaxColumnAddressed - 1))
            {
                return;
            }

            foreach (Row row in this.Rows)
            {
                if (index < row.cells.Count)
                {
                    row.InsertCellsAfter(index, numberOfColumns);
                }
            }
        }

        /// <summary>
        /// Inserts a column after a given column index
        /// </summary>
        /// <param name="index">Index of column after which new column should be inserted</param>
        public void InsertColumnAfter(int index)
        {
            this.InsertColumnsAfter(index, 1);
        }
        #endregion

        #region Export
        internal void Export(XmlWriter writer)
        {
            // Worksheet
            writer.WriteStartElement("Worksheet");
            writer.WriteAttributeString("ss", "Name", null, this.Name);

            this.ParentBook.ExportNamedRanges(writer, this);

            // Table
            writer.WriteStartElement("Table");
            writer.WriteAttributeString("ss", "FullColumns", null, "1");
            writer.WriteAttributeString("ss", "FullRows", null, "1");

            if (!this.StyleID.IsNullOrEmpty() && this.StyleID != "Default")
            {
                writer.WriteAttributeString("ss", "StyleID", null, this.StyleID);
            }

            // Start Columns
            foreach (Column col in this.columns)
            {
                col.Export(writer);
            }

            // End Columns

            // Start Rows
            foreach (Row row in this.Rows)
            {
                row.Export(writer);
            }

            // End Rows
            // End Table
            writer.WriteEndElement();

            // Write worksheet options
            this.ExportOptions(writer);

            // Write Autofilter options
            if (this.AutoFilter)
            {
                string range = this.GetParentBook().GetAutoFilterRange(this);

                writer.WriteStartElement(string.Empty, "AutoFilter", "urn:schemas-microsoft-com:office:excel");
                writer.WriteAttributeString(string.Empty, "Range", null, range);
                writer.WriteEndElement();
            }

            // End Worksheet
            writer.WriteEndElement();
        }

        private void WritePanes(XmlWriter writer)
        {
            string panes;

            if (this.FreezeLeftColumns > 0 && this.FreezeTopRows > 0)
            {
                panes = "3210";
            }
            else
            {
                panes = this.FreezeLeftColumns > 0 ? "31" : "32";
            }

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
            writer.WriteStartElement(string.Empty, "WorksheetOptions", "urn:schemas-microsoft-com:office:excel");

            this.PrintOptions.Export(writer);

            writer.WriteElementString("Selected", string.Empty);

            if (this.TabColor != -1)
            {
                writer.WriteElementString("TabColor", this.TabColor.ToString(
                        CultureInfo.InvariantCulture));
            }

            // Pane Info
            if (this.FreezeLeftColumns > 0 || this.FreezeTopRows > 0)
            {
                writer.WriteElementString("FreezePanes", string.Empty);
                writer.WriteElementString("FrozenNoSplit", string.Empty);

                if (this.FreezeTopRows > 0)
                {
                    writer.WriteElementString("SplitHorizontal", this.FreezeTopRows.ToString(
                        CultureInfo.InvariantCulture));
                    writer.WriteElementString("TopRowBottomPane", this.FreezeTopRows.ToString(
                        CultureInfo.InvariantCulture));
                }

                if (this.FreezeLeftColumns > 0)
                {
                    writer.WriteElementString("SplitVertical", this.FreezeLeftColumns.ToString(
                        CultureInfo.InvariantCulture));
                    writer.WriteElementString("LeftColumnRightPane", this.FreezeLeftColumns.ToString(
                        CultureInfo.InvariantCulture));
                }

                // Panes
                this.WritePanes(writer);
            }

            // End Worksheet options
            writer.WriteEndElement();
        }
        #endregion
    }
}
