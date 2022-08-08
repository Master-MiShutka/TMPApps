namespace TMP.ExcelXml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using TMP.Extensions;

    /// <summary>
    /// Defines a range of cells
    /// </summary>
    public class Range : Styles, IEnumerable<Cell>
    {
        #region Private and Internal fields
        internal Cell CellFrom;
        internal Cell CellTo;

        internal string UnresolvedRangeReference;
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the range's flag to return a absolute reference or otherwise
        /// </summary>
        public bool Absolute { get; set; }

        private string name;

        /// <summary>
        /// Gets or sets the name of the range
        /// </summary>
        /// <remarks>This property always adds global (i.e. Workbook level)
        /// named ranges. To add sheet limited ranges, use Worksheet's
        /// AddNamedRange method of <see cref="TMP.ExcelXml.Worksheet"/>
        /// class.</remarks>
        public string Name
        {
            get => this.name;

            set
            {
                if (this.name != value)
                {
                    if (value.IsNullOrEmpty() || IsSystemRangeName(value))
                    {
                        throw new ArgumentException("name");
                    }

                    this.name = value;

                    this.CellFrom.GetParentBook().AddNamedRange(this, this.name);
                }
            }
        }

        /// <summary>
        /// Gets the number of rows in a range
        /// </summary>
        /// <returns>Number of rows in a range</returns>
        public int RowCount
        {
            get
            {
                if (this.CellFrom == null)
                {
                    return 0;
                }

                if (this.CellTo == null)
                {
                    return 1;
                }

                int rowFrom = this.CellFrom.ParentRow.rowIndex;
                int rowTo = this.CellTo.ParentRow.rowIndex;

                return rowTo - rowFrom + 1;
            }
        }

        /// <summary>
        /// Gets the number of columns in a range
        /// </summary>
        /// <returns>Number of columns in a range</returns>
        public int ColumnCount
        {
            get
            {
                if (this.CellFrom == null)
                {
                    return 0;
                }

                if (this.CellTo == null)
                {
                    return 1;
                }

                int cellIndexFrom = this.CellFrom.CellIndex;
                int cellIndexTo = this.CellTo.CellIndex;

                return cellIndexTo - cellIndexFrom + 1;
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Defines a unresolved range
        /// </summary>
        /// <param name="range">Unresolved range address</param>
        internal Range(string range)
        {
            if (range[0] == '=')
            {
                range = range.Substring(1);
            }

            this.UnresolvedRangeReference = range;
        }

        /// <summary>
        /// Defines a range
        /// </summary>
        /// <param name="cell">A single cell as a range</param>
        public Range(Cell cell)
        {
            this.CellFrom = cell;

            this.UnresolvedRangeReference = string.Empty;
        }

        /// <summary>
        /// Defines a range
        /// </summary>
        /// <param name="cellFrom">Starting cell</param>
        /// <param name="cellTo">Ending cell</param>
        /// <remarks>Defines a rectangular area of a sheet with a starting cell and a ending cell</remarks>
        public Range(Cell cellFrom, Cell cellTo)
        {
            this.UnresolvedRangeReference = string.Empty;

            if (cellTo == null)
            {
                this.CellFrom = cellFrom;

                return;
            }

            if (cellFrom.ParentRow.parentSheet != cellTo.ParentRow.parentSheet)
            {
                throw new ArgumentException("cellFrom and cellTo's parent worksheets should be same");
            }

            if (cellFrom == cellTo)
            {
                this.CellFrom = cellFrom;

                return;
            }

            // Swap the cells if the range inverted
            int rowFrom = cellFrom.ParentRow.rowIndex;
            int rowTo = cellTo.ParentRow.rowIndex;

            int cellIndexFrom = cellFrom.CellIndex;
            int cellIndexTo = cellTo.CellIndex;

            if (rowFrom > rowTo || cellIndexFrom > cellIndexTo)
            {
                this.CellFrom = cellTo;
                this.CellTo = cellFrom;
            }
            else
            {
                this.CellFrom = cellFrom;
                this.CellTo = cellTo;
            }
        }
        #endregion

        #region Private and Internal methods
        private string AbsoluteReference()
        {
            string range = string.Format(CultureInfo.InvariantCulture, "R{0}C{1}",
                    this.CellFrom.ParentRow.rowIndex + 1, this.CellFrom.CellIndex + 1);

            if (this.CellFrom != null)
            {
                range += string.Format(CultureInfo.InvariantCulture, ":R{0}C{1}",
                    this.CellTo.ParentRow.rowIndex + 1, this.CellTo.CellIndex + 1);
            }

            return range;
        }

        internal bool Match(Range range)
        {
            if (range.CellFrom == this.CellFrom && range.CellTo == this.CellTo)
            {
                return true;
            }

            return false;
        }

        internal override ExcelXmlWorkbook GetParentBook()
        {
            return null;
        }

        internal override Cell FirstCell()
        {
            return this.CellFrom;
        }

        internal override void IterateAndApply(IterateFunction applyStyleFunction)
        {
            if (this.CellFrom == null)
            {
                return;
            }

            if (this.CellTo == null)
            {
                applyStyleFunction(this.CellFrom);

                return;
            }

            int rowFrom = this.CellFrom.ParentRow.rowIndex;
            int rowTo = this.CellTo.ParentRow.rowIndex;

            int cellIndexFrom = this.CellFrom.CellIndex;
            int cellIndexTo = this.CellTo.CellIndex;

            Worksheet ws = this.CellFrom.ParentRow.parentSheet;

            for (int i = rowFrom; i <= rowTo; i++)
            {
                for (int j = cellIndexFrom; j <= cellIndexTo; j++)
                {
                    applyStyleFunction(ws[j, i]);
                }
            }
        }

        internal string NamedRangeReference(bool sheetReference)
        {
            if (this.CellFrom == null)
            {
                return this.UnresolvedRangeReference;
            }

            string range = string.Empty;

            if (sheetReference)
            {
                range = "'" + this.CellFrom.ParentRow.parentSheet.Name + "'!";
            }

            range += this.AbsoluteReference();

            return range;
        }

        internal string RangeReference(Cell cell)
        {
            if (this.CellFrom == null)
            {
                return this.UnresolvedRangeReference;
            }

            if (this.CellFrom.ParentRow == null)
            {
                return "#N/A";
            }

            if (this.CellTo != null && this.CellTo.ParentRow == null)
            {
                return "#N/A";
            }

            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            string range;

            if (this.Absolute)
            {
                range = this.AbsoluteReference();
            }
            else
            {
                if (this.CellTo != null)
                {
                    range = string.Format(CultureInfo.InvariantCulture, "R[{0}]C[{1}]:R[{2}]C[{3}]",
                        this.CellFrom.ParentRow.rowIndex - cell.ParentRow.rowIndex,
                        this.CellFrom.CellIndex - cell.CellIndex,
                        this.CellTo.ParentRow.rowIndex - cell.ParentRow.rowIndex,
                        this.CellTo.CellIndex - cell.CellIndex);
                }
                else
                {
                    range = string.Format(CultureInfo.InvariantCulture, "R[{0}]C[{1}]",
                        this.CellFrom.ParentRow.rowIndex - cell.ParentRow.rowIndex,
                        this.CellFrom.CellIndex - cell.CellIndex);
                }
            }

            string sheetReference = string.Empty;

            if (this.CellFrom.ParentRow.parentSheet != cell.ParentRow.parentSheet)
            {
                sheetReference = this.CellFrom.ParentRow.parentSheet.Name;
                ExcelXmlWorkbook workBook = this.CellFrom.GetParentBook();

                if (workBook != cell.GetParentBook())
                {
                    throw new ArgumentException("External workbook references are not supported");
                }
            }

            if (!sheetReference.IsNullOrEmpty())
            {
                range = "'" + sheetReference + "'!" + range;
            }

            return range;
        }

        internal void ParseUnresolvedReference(Cell cell)
        {
            if (this.UnresolvedRangeReference.IsNullOrEmpty())
            {
                return;
            }

            Match match;
            ParseArgumentType pat = FormulaParser.GetArgumentType(this.UnresolvedRangeReference, out match);

            Range range;

            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            bool parsed = FormulaParser.ParseRange(cell, match, out range, pat == ParseArgumentType.AbsoluteRange);

            if (parsed)
            {
                this.UnresolvedRangeReference = string.Empty;

                this.CellFrom = range.CellFrom;
                this.CellTo = range.CellTo;
            }
        }

        internal static bool IsSystemRangeName(string name)
        {
            if (name == "Print_Titles" || name == "_FilterDatabase" || name == "Print_Area")
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Collection Methods

        /// <summary>
        /// Get a cell enumerator
        /// </summary>
        /// <returns>returns IEnumerator&gt;Cell&lt;</returns>
        public IEnumerator<Cell> GetEnumerator()
        {
            if (this.CellFrom != null)
            {
                if (this.CellTo == null)
                {
                    yield return this.CellFrom;
                }
                else
                {
                    int rowFrom = this.CellFrom.ParentRow.rowIndex;
                    int rowTo = this.CellTo.ParentRow.rowIndex;

                    int cellIndexFrom = this.CellFrom.CellIndex;
                    int cellIndexTo = this.CellTo.CellIndex;

                    Worksheet ws = this.CellFrom.ParentRow.parentSheet;

                    for (int i = rowFrom; i <= rowTo; i++)
                    {
                        for (int j = cellIndexFrom; j <= cellIndexTo; j++)
                        {
                            yield return ws[j, i];
                        }
                    }
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

        #region Public Information methods

        /// <summary>
        /// Checks if a particular cell is present in a range or not
        /// </summary>
        /// <param name="cell">Cell to check</param>
        /// <returns>true if cell is present, false otherwise</returns>
        public bool Contains(Cell cell)
        {
            if (this.CellFrom == null)
            {
                return false;
            }

            if (this.CellFrom.ParentRow.parentSheet != cell.ParentRow.parentSheet)
            {
                return false;
            }

            if (this.CellTo == null)
            {
                return this.CellFrom == cell;
            }

            int rowFrom = this.CellFrom.ParentRow.rowIndex;
            int rowTo = this.CellTo.ParentRow.rowIndex;

            int cellIndexFrom = this.CellFrom.CellIndex;
            int cellIndexTo = this.CellTo.CellIndex;

            return cell.ParentRow.rowIndex >= rowFrom &&
                    cell.ParentRow.rowIndex <= rowTo &&
                    cell.CellIndex >= cellIndexFrom &&
                    cell.CellIndex <= cellIndexTo;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Sets this range as a auto-filter range in the sheet
        /// </summary>
        public void AutoFilter()
        {
            this.CellFrom.ParentRow.parentSheet.AutoFilter = true;

            this.CellFrom.GetParentBook().AddNamedRange(this, "_FilterDatabase",
                this.CellFrom.ParentRow.parentSheet);
        }

        /// <summary>
        /// Sets this range as the current print area in the sheet
        /// </summary>
        public void SetAsPrintArea()
        {
            this.CellFrom.ParentRow.parentSheet.PrintArea = true;

            this.CellFrom.GetParentBook().AddNamedRange(this, "Print_Area",
                this.CellFrom.ParentRow.parentSheet);
        }

        /// <summary>
        /// Merges a range into one cell
        /// </summary>
        /// <returns>true if merge was successful, false otherwise</returns>
        public bool Merge()
        {
            if (this.CellFrom.MergeStart)
            {
                return true;
            }

            bool rangeHasMergedCells = false;

            // if any cell in this range is merged, this operation will fail!!
            this.IterateAndApply(cell => rangeHasMergedCells = cell.MergeStart);

            if (rangeHasMergedCells)
            {
                return false;
            }

            Worksheet ws = this.CellFrom.ParentRow.parentSheet;
            ws.MergedCells.Add(this);

            this.CellFrom.MergeStart = true;
            this.CellFrom.ColumnSpan = this.ColumnCount;
            this.CellFrom.RowSpan = this.RowCount;

            return true;
        }

        /// <summary>
        /// Unmerges a merged range
        /// </summary>
        public void Unmerge()
        {
            if (!this.CellFrom.MergeStart)
            {
                return;
            }

            Worksheet ws = this.CellFrom.ParentRow.parentSheet;
            ws.MergedCells.Remove(this);

            this.CellFrom.MergeStart = false;
            this.CellFrom.ColumnSpan = 1;
            this.CellFrom.RowSpan = 1;
        }
        #endregion
    }
}
