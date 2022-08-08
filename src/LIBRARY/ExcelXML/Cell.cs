namespace TMP.ExcelXml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml;
    using TMP.Extensions;

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
            this.ColumnIndex = cell.CellIndex;
            this.RowIndex = cell.ParentRow.rowIndex;

            this.SetExcelIndex();
        }

        private void SetExcelIndex()
        {
            this.ExcelColumnIndex = string.Empty;

            int partOne = (this.ColumnIndex / 26) - 1;
            int partTwo = this.ColumnIndex % 26;

            if (partOne >= 0)
            {
                char firstHalf = (char)('A' + partOne);
                this.ExcelColumnIndex += firstHalf;
            }

            char secondHalf = (char)('A' + partTwo);
            this.ExcelColumnIndex += secondHalf;
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
        public ContentType ContentType => this.Content;

        /// <summary>
        /// Index information of the cell
        /// </summary>
        public CellIndexInfo Index => new CellIndexInfo(this);

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
                if (this.MergeStart)
                {
                    return this.columnSpan;
                }

                return 1;
            }

            internal set => this.columnSpan = value;
        }

        private int rowSpan;

        /// <summary>
        /// Gets the number of rows merged together, starting with this cell
        /// </summary>
        public int RowSpan
        {
            get
            {
                if (this.MergeStart)
                {
                    return this.rowSpan;
                }

                return 1;
            }

            internal set => this.rowSpan = value;
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
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.ParentRow = parent;

            this.Content = ContentType.None;
            this.CellIndex = cell;

            if (parent.Style != null)
            {
                this.Style = parent.Style;
            }
            else if (parent.parentSheet.Columns(this.CellIndex).Style != null)
            {
                this.Style = parent.parentSheet.Columns(this.CellIndex).Style;
            }
            else if (parent.parentSheet.Style != null)
            {
                this.Style = parent.parentSheet.Columns(this.CellIndex).Style;
            }
        }
        #endregion

        #region Private and Internal methods
        internal override ExcelXmlWorkbook GetParentBook()
        {
            return this.ParentRow.parentSheet.ParentBook;
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
            if (this.Content == ContentType.Formula)
            {
                foreach (Parameter p in this.formula.Parameters)
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
            this.Content = ContentType.None;

            this.value = null;
            this.formula = null;

            if (!removeContentOnly)
            {
                this.ParentRow = null;
            }
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
            {
                return (T)this.value;
            }

            if (!typeof(T).IsPrimitive &&
                typeName != "System.DateTime" &&
                typeName != "System.String" &&
                typeName != "TMP.ExcelXml.Formula")
            {
                throw new ArgumentException("T must be of a primitive or Formula type");
            }

            switch (this.Content)
            {
                case ContentType.Boolean:
                    {
                        if (typeName == "System.Boolean")
                        {
                            return (T)Convert.ChangeType(this.value, typeof(T),
                                CultureInfo.InvariantCulture);
                        }

                        return default(T);
                    }

                case ContentType.DateTime:
                    {
                        if (typeName == "System.DateTime")
                        {
                            return (T)Convert.ChangeType(this.value, typeof(T),
                                CultureInfo.InvariantCulture);
                        }

                        return default(T);
                    }

                case ContentType.Number:
                    {
                        if (ObjectExtensions.IsNumericType(typeof(T)))
                        {
                            return (T)Convert.ChangeType(this.value, typeof(T),
                                CultureInfo.InvariantCulture);
                        }

                        return default(T);
                    }

                case ContentType.Formula:
                    {
                        if (typeName == "TMP.ExcelXml.Formula")
                        {
                            return (T)this.value;
                        }

                        return default(T);
                    }

                case ContentType.UnresolvedValue:
                case ContentType.String:
                    {
                        if (typeName == "System.String")
                        {
                            return (T)Convert.ChangeType(this.value, typeof(T),
                                CultureInfo.InvariantCulture);
                        }

                        return default(T);
                    }
            }

            return default(T);
        }

        private object value;

        /// <summary>
        /// Gets or sets the value of the cell
        /// </summary>
        /// <remarks>
        /// Value returns a boxed <see cref="string"/> value of the cell or sets the value of the cell to...
        /// <list type="number">
        /// <item>
        /// <term><see cref="string"/></term><description></description>
        /// </item>
        /// <item>
        /// <term><see cref="bool"/></term><description></description>
        /// </item>
        /// <item>
        /// <term><see cref="byte"/></term><description></description>
        /// </item>
        /// <item>
        /// <term><see cref="short"/></term><description></description>
        /// </item>
        /// <item>
        /// <term><see cref="int"/></term><description></description>
        /// </item>
        /// <item>
        /// <term><see cref="long"/></term><description></description>
        /// </item>
        /// <item>
        /// <term><see cref="double"/></term><description></description>
        /// </item>
        /// <item>
        /// <term><see cref="decimal"/></term><description></description>
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
            get => this.value;

            set
            {
                switch (value.GetType().FullName)
                {
                    case "System.DateTime":
                        {
                            this.value = value;

                            this.Content = ContentType.DateTime;

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
                            this.value = value;

                            this.Content = ContentType.Number;

                            break;
                        }

                    case "System.Boolean":
                        {
                            this.value = value;

                            this.Content = ContentType.Boolean;

                            break;
                        }

                    case "System.String":
                        {
                            this.value = value;

                            this.Content = ContentType.String;

                            break;
                        }

                    case "TMP.ExcelXml.Cell":
                        {
                            Cell from = value as Cell;
                            if (from != null)
                            {
                                if (this.formula != null)
                                {
                                    this.formula = null;
                                }

                                this.formula = new Formula();
                                this.value = null;
                                this.formula.Add(new Range(from));

                                this.Content = ContentType.Formula;
                            }
                            else
                            {
                                this.formula = null;
                                this.value = null;
                                this.Content = ContentType.None;
                            }

                            break;
                        }

                    case "TMP.ExcelXml.Formula":
                        {
                            Formula from = value as Formula;

                            if (from != null)
                            {
                                this.formula = from;
                                this.value = null;
                                this.Content = ContentType.Formula;
                            }
                            else
                            {
                                this.formula = null;
                                this.value = null;
                                this.Content = ContentType.None;
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
            if (this.Content == ContentType.None && this.Comment.IsNullOrEmpty() && this.HasDefaultStyle())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Empties the content of a cell
        /// </summary>
        public void Empty()
        {
            this.Empty(true);
        }

        /// <summary>
        /// Unmerges a cell
        /// </summary>
        public void Unmerge()
        {
            if (!this.MergeStart)
            {
                return;
            }

            Worksheet ws = this.ParentRow.parentSheet;
            ws.MergedCells.RemoveAll(range => range.CellFrom == this);

            this.MergeStart = false;
        }

        /// <summary>
        /// Deletes a cell from the parent row
        /// </summary>
        public void Delete()
        {
            this.ParentRow.DeleteCell(this);
        }
        #endregion

        #region Export
        internal void Export(XmlWriter writer, bool printIndex)
        {
            if (this.IsEmpty())
            {
                return;
            }

            // If no merge starts from this cell, and this cells is in
            // a merged range, no output should be done...
            if (!this.MergeStart && this.ParentRow.parentSheet.IsCellMerged(this))
            {
                return;
            }

            // Start Cell
            writer.WriteStartElement("Cell");

            // Has style? If yes, we only need to write the style if default line
            // style is not same as this one...
            if (!this.StyleID.IsNullOrEmpty() && this.ParentRow.StyleID != this.StyleID && this.StyleID != "Default")
            {
                writer.WriteAttributeString("ss", "StyleID", null, this.StyleID);
            }

            if (printIndex)
            {
                writer.WriteAttributeString("ss", "Index", null,
                    (this.CellIndex + 1).ToString(CultureInfo.InvariantCulture));
            }

            if (!this.HRef.IsNullOrEmpty())
            {
                writer.WriteAttributeString("ss", "HRef", null, this.HRef.XmlEncode());
            }

            if (this.MergeStart)
            {
                Worksheet ws = this.ParentRow.parentSheet;
                Range range = ws.MergedCells.Find(rangeToFind => rangeToFind.CellFrom == this);

                if (range != null)
                {
                    int rangeCols = range.ColumnCount - 1;
                    int rangeRows = range.RowCount - 1;

                    if (rangeCols > 0)
                    {
                        writer.WriteAttributeString("ss", "MergeAcross", null,
                            rangeCols.ToString(CultureInfo.InvariantCulture));
                    }

                    if (rangeRows > 0)
                    {
                        writer.WriteAttributeString("ss", "MergeDown", null,
                            rangeRows.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            // Export content
            this.ExportContent(writer);

            // Export comment
            this.ExportComment(writer);

            // Write named ranges
            List<string> namedRanges = this.GetParentBook().CellInNamedRanges(this);

            foreach (string range in namedRanges)
            {
                writer.WriteStartElement("NamedCell");
                writer.WriteAttributeString("ss", "Name", null, range);
                writer.WriteEndElement();
            }

            // End Cell
            writer.WriteEndElement();
        }

        private void ExportContent(XmlWriter writer)
        {
            // Has formula?
            if (this.Content == ContentType.Formula)
            {
                writer.WriteAttributeString("ss", "Formula", null, "=" + this.formula.ToString(this));
            }
            else if (this.Content == ContentType.UnresolvedValue)
            {
                writer.WriteAttributeString("ss", "Formula", null, (string)this.value);
            }
            else if (this.Content != ContentType.None)
            {
                // Write Data
                writer.WriteStartElement("Data");
                writer.WriteAttributeString("ss", "Type", null, this.Content.ToString());

                switch (this.Content)
                {
                    case ContentType.Boolean:
                        {
                            if ((bool)this.value)
                            {
                                writer.WriteValue("1");
                            }
                            else
                            {
                                writer.WriteValue("0");
                            }

                            break;
                        }

                    case ContentType.DateTime:
                        {
                            writer.WriteValue(((DateTime)this.value).ToString("yyyy-MM-dd\\Thh:mm:ss.fff",
                                CultureInfo.InvariantCulture));
                            break;
                        }

                    case ContentType.Number:
                        {
                            decimal d = Convert.ToDecimal(this.value, CultureInfo.InvariantCulture);
                            writer.WriteValue(d.ToString(new CultureInfo("en-US")));
                            break;
                        }

                    case ContentType.String:
                        {
                            writer.WriteValue((string)this.value);
                            break;
                        }
                }

                writer.WriteEndElement();
            }
        }

        private void ExportComment(XmlWriter writer)
        {
            // Write comment
            if (!this.Comment.IsNullOrEmpty())
            {
                string author = this.GetParentBook().Properties.Author;

                // Start comment
                writer.WriteStartElement("Comment");

                if (!author.IsNullOrEmpty())
                {
                    writer.WriteAttributeString("ss", "Author", null, author);
                }

                // Comment data section
                writer.WriteStartElement("ss", "Data", null);
                writer.WriteAttributeString("xmlns", "http://www.w3.org/TR/REC-html40");
                writer.WriteRaw(this.Comment);
                writer.WriteEndElement();

                // End comment
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
