namespace TMP.ExcelXml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using TMP.Extensions;

    /// <summary>
    /// This class represents a excel workbook
    /// </summary>
    public partial class ExcelXmlWorkbook
    {
        #region Private and Internal fields
        private List<XmlStyle> Styles;
        private List<Worksheet> Worksheets;

        internal List<NamedRange> NamedRanges;
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets document properties
        /// </summary>
        public DocumentProperties Properties { get; set; }

        /// <summary>
        /// Gets or sets default font options of the sheet
        /// </summary>
        public XmlStyle DefaultStyle
        {
            get => this.Styles[0];

            set
            {
                if (value != null && value.ID == "Default")
                {
                    this.Styles[0] = value;
                }
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public ExcelXmlWorkbook()
        {
            this.Initialize();
        }
        #endregion

        #region Private and Internal methods
        private void Initialize()
        {
            this.Properties = new DocumentProperties();

            this.Styles = new List<XmlStyle>();
            this.Worksheets = new List<Worksheet>();
            this.NamedRanges = new List<NamedRange>();

            XmlStyle style = new XmlStyle();
            style.ID = "Default";
            style.Alignment.Vertical = VerticalAlignment.Bottom;

            this.Styles.Add(style);
        }

        private void SetSheetNameByIndex(Worksheet ws, int index)
        {
            index++;

            string name = "Sheet" + index.ToString(CultureInfo.InvariantCulture);

            while (this.GetSheetIDByName(name) != -1)
            {
                name = "Sheet" + (++index).ToString(CultureInfo.InvariantCulture);
            }

            ws.Name = name;
        }

        private int GetSheetIDByName(string sheetName)
        {
            return this.Worksheets.FindIndex(
                sheet => sheet.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
        }

        internal XmlStyle GetStyleByID(string iD)
        {
            if (iD.IsNullOrEmpty())
            {
                return this.Styles[0];
            }

            return this.Styles.Find(xs => xs.ID == iD);
        }

        internal bool HasStyleID(string iD)
        {
            return this.Styles.Exists(xs => xs.ID == iD);
        }

        internal string AddStyle(XmlStyle style)
        {
            XmlStyle oldStyle = this.FindStyle(style);

            if (oldStyle == null)
            {
                int iterator = this.Styles.Count;
                style.ID = string.Format(CultureInfo.InvariantCulture, "S{0:00}", iterator++);

                while (this.HasStyleID(style.ID))
                {
                    style.ID = string.Format(CultureInfo.InvariantCulture, "S{0:00}", iterator++);
                }

                this.Styles.Add(style);

                return style.ID;
            }

            return oldStyle.ID;
        }

        internal XmlStyle FindStyle(XmlStyle style)
        {
            return this.Styles.Find(xs => xs.CheckForMatch(style));
        }

        internal void AddNamedRange(Range range, string name, Worksheet ws)
        {
            if (range.FirstCell() != null && range.FirstCell().GetParentBook() != this)
            {
                throw new InvalidOperationException("Named range parent book should be same");
            }

            NamedRange namedRange = this.NamedRanges.Find(nr => nr.Name == name && nr.Worksheet == ws);

            if (namedRange == null)
            {
                namedRange = this.NamedRanges.Find(nr => nr.Range.Match(range));

                if (namedRange == null)
                {
                    if (name == "_FilterDatabase")
                    {
                        this.NamedRanges.Insert(0, new NamedRange(range, name, ws));
                    }
                    else
                    {
                        this.NamedRanges.Add(new NamedRange(range, name, ws));
                    }
                }
                else
                {
                    namedRange.Name = name;
                }
            }
            else
            {
                namedRange.Range = range;
            }
        }

        internal void RemoveNamedRange(string name, Worksheet ws)
        {
            this.NamedRanges.RemoveAll(nr => nr.Name == name && nr.Worksheet == ws);
        }

        internal string GetAutoFilterRange(Worksheet ws)
        {
            NamedRange namedRange = this.NamedRanges.Find(
                nr => nr.Name == "_FilterDatabase" && nr.Worksheet == ws);

            if (namedRange == null)
            {
                return string.Empty;
            }

            return namedRange.Range.NamedRangeReference(false);
        }

        internal List<string> CellInNamedRanges(Cell cell)
        {
            List<string> ranges = new List<string>();

            PrintOptions po = cell.ParentRow.parentSheet.PrintOptions;

            if (po.PrintTitles)
            {
                int cellRowIndex = cell.ParentRow.rowIndex + 1;
                int cellColIndex = cell.CellIndex + 1;

                if (cellRowIndex >= po.TopPrintRow && cellRowIndex <= po.BottomPrintRow)
                {
                    ranges.Add("Print_Titles");
                }
                else
                {
                    if (cellColIndex >= po.LeftPrintCol && cellColIndex <= po.RightPrintCol)
                    {
                        ranges.Add("Print_Titles");
                    }
                }
            }

            foreach (NamedRange nr in this.NamedRanges)
            {
                if (nr.Range.Contains(cell))
                {
                    ranges.Add(nr.Name);
                }
            }

            return ranges;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Add a named range to the book with a book scope
        /// </summary>
        /// <param name="range">Range to be named</param>
        /// <param name="name">Name of the range</param>
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

            this.AddNamedRange(range, name, null);
        }
        #endregion

        #region Public Book Information methods

        /// <summary>
        /// Returns the sheet at a given position
        /// </summary>
        /// <param name="index">Index of the <see cref="Worksheet"/> starting from 0</param>
        /// <returns><see cref="Worksheet"/> reference to the requested sheet</returns>
        public Worksheet this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if ((index + 1) > this.Worksheets.Count)
                {
                    for (int i = this.Worksheets.Count; i <= index; i++)
                    {
                        Worksheet ws = new Worksheet(this);
                        this.SetSheetNameByIndex(ws, i);
                        this.Worksheets.Add(ws);
                    }
                }

                return this.Worksheets[index];
            }
        }

        /// <summary>
        /// Returns the sheet by sheet name
        /// </summary>
        /// <param name="sheetName">Name of <see cref="Worksheet"/></param>
        /// <returns><see cref="TMP.ExcelXml.Worksheet"/> reference to the requested sheet</returns>
        public Worksheet this[string sheetName]
        {
            get
            {
                return this.GetSheetByName(sheetName);
            }
        }

        /// <summary>
        /// Gets a Worksheet reference matching a particular name
        /// </summary>
        /// <param name="sheetName">Name to find</param>
        /// <returns>returns instance of matching sheet, null otherwise</returns>
        public Worksheet GetSheetByName(string sheetName)
        {
            return this.Worksheets.Find(
                sheet => sheet.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// The number of sheets in this boook
        /// </summary>
        public int SheetCount => this.Worksheets.Count;
        #endregion

        #region Public Sheet Addition, Insertion & Deletion methods

        /// <summary>
        /// Delete a sheet by index
        /// </summary>
        /// <param name="index">Index number of sheet to delete</param>
        public void DeleteSheet(int index)
        {
            if (index < 0 || index >= this.Worksheets.Count)
            {
                return;
            }

            this.Worksheets.RemoveAt(index);
        }

        /// <summary>
        /// Delete a sheet by name
        /// </summary>
        /// <param name="sheetName">Name of sheet to delete</param>
        public void DeleteSheet(string sheetName)
        {
            this.DeleteSheet(this.GetSheetIDByName(sheetName));
        }

        /// <summary>
        /// Delete a sheet by instance
        /// </summary>
        /// <param name="ws">Instance of sheet to delete</param>
        public void DeleteSheet(Worksheet ws)
        {
            if (ws != null)
            {
                this.DeleteSheet(this.Worksheets.FindIndex(s => s == ws));
            }
        }

        /// <summary>
        /// Insert a new sheet before another sheet
        /// </summary>
        /// <param name="index">Index of sheet before which new sheet will be added</param>
        /// <returns>New worksheet instance</returns>
        /// <remarks>If index is less than 0, the new sheet is added to the end of all sheets</remarks>
        public Worksheet InsertBefore(int index)
        {
            if (index < 0)
            {
                return this.Add();
            }

            if (index >= this.Worksheets.Count)
            {
                return this[index];
            }

            Worksheet newSheet = new Worksheet(this);
            this.SetSheetNameByIndex(newSheet, index);

            this.Worksheets.Insert(index, newSheet);

            return newSheet;
        }

        /// <summary>
        /// Insert a new sheet before another sheet
        /// </summary>
        /// <param name="sheetName">Name of sheet before which new sheet will be added</param>
        /// <returns>New worksheet instance</returns>
        /// <remarks>If sheet is not found, the new sheet is added to the end of all sheets</remarks>
        public Worksheet InsertBefore(string sheetName)
        {
            return this.InsertBefore(this.GetSheetIDByName(sheetName));
        }

        /// <summary>
        /// Insert a new sheet before another sheet
        /// </summary>
        /// <param name="ws">Instance of sheet before which new sheet will be added</param>
        /// <returns>New worksheet instance</returns>
        /// <remarks>If sheet is not found, the new sheet is added to the end of all sheets</remarks>
        public Worksheet InsertBefore(Worksheet ws)
        {
            return this.InsertBefore(this.Worksheets.FindIndex(s => s == ws));
        }

        /// <summary>
        /// Insert a new sheet after another sheet
        /// </summary>
        /// <param name="index">Index of sheet after which new sheet will be added</param>
        /// <returns>New worksheet instance</returns>
        /// <remarks>If index is not in bounds, the new sheet is added to the end of all sheets</remarks>
        public Worksheet InsertAfter(int index)
        {
            if (index < 0)
            {
                return this.Add();
            }

            if (index >= this.Worksheets.Count)
            {
                return this[index];
            }

            Worksheet newSheet = new Worksheet(this);
            this.SetSheetNameByIndex(newSheet, index + 1);

            this.Worksheets.Insert(index + 1, newSheet);

            return newSheet;
        }

        /// <summary>
        /// Insert a new sheet after another sheet
        /// </summary>
        /// <param name="sheetName">Name of sheet after which new sheet will be added</param>
        /// <returns>New worksheet instance</returns>
        /// <remarks>If sheet is not found, the new sheet is added to the end of all sheets</remarks>
        public Worksheet InsertAfter(string sheetName)
        {
            return this.InsertAfter(this.GetSheetIDByName(sheetName));
        }

        /// <summary>
        /// Insert a new sheet after another sheet
        /// </summary>
        /// <param name="ws">Instance of sheet after which new sheet will be added</param>
        /// <returns>New worksheet instance</returns>
        /// <remarks>If sheet is not found, the new sheet is added to the end of all sheets</remarks>
        public Worksheet InsertAfter(Worksheet ws)
        {
            return this.InsertAfter(this.Worksheets.FindIndex(s => s == ws));
        }

        /// <summary>
        /// Adds a new sheet at the end of all sheets
        /// </summary>
        /// <returns>New Worksheet instance</returns>
        public Worksheet Add()
        {
            return this[this.Worksheets.Count];
        }

        /// <summary>
        /// Adds a new sheet at the end of all sheets
        /// </summary>
        /// <param name="sheetName">Sheet name</param>
        /// <returns>New Worksheet instance</returns>
        public Worksheet Add(string sheetName)
        {
            this[this.Worksheets.Count].Name = sheetName;

            return this[this.Worksheets.Count];
        }
        #endregion

        #region Export
        internal void ExportNamedRanges(XmlWriter writer, Worksheet ws)
        {
            bool started = false;

            if (ws != null && ws.PrintOptions.PrintTitles)
            {
                started = true;

                writer.WriteStartElement("Names");
                writer.WriteStartElement("NamedRange");
                writer.WriteAttributeString("ss", "Name", null, "Print_Titles");
                writer.WriteAttributeString("ss", "RefersTo", null,
                    ws.PrintOptions.GetPrintTitleRange(ws.Name));
                writer.WriteEndElement();
            }

            foreach (NamedRange nr in this.NamedRanges)
            {
                if (nr.Worksheet == ws)
                {
                    if (!started)
                    {
                        started = true;

                        writer.WriteStartElement("Names");
                    }

                    writer.WriteStartElement("NamedRange");
                    writer.WriteAttributeString("ss", "Name", null, nr.Name);

                    writer.WriteAttributeString("ss", "RefersTo", null,
                        nr.Range.NamedRangeReference(true));

                    if (nr.Name == "_FilterDatabase")
                    {
                        writer.WriteAttributeString("ss", "Hidden", null, "1");
                    }

                    writer.WriteEndElement();
                }
            }

            if (started)
            {
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Export the workbook to a file
        /// </summary>
        /// <param name="fileName">Output file name</param>
        /// <returns>true if the export was successful, false otherwise</returns>
        public bool Export(string fileName)
        {
            FileStream fs;

            try
            {
                fs = new FileStream(fileName, FileMode.Create);
            }
            catch
            {
                return false;
            }

            bool returnValue = this.Export(fs);

            fs.Close();
            fs.Dispose();

            return returnValue;
        }

        /// <summary>
        /// Export the workbook to a stream
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <returns>true if the export was successful, false otherwise</returns>
        public bool Export(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";

            if (!stream.CanWrite)
            {
                return false;
            }

            XmlWriter writer = XmlWriter.Create(stream, settings);

            if (writer == null)
            {
                return false;
            }

            // Write Xml header
            writer.WriteStartDocument();

            // Write instruction that these file is a excel sheet
            writer.WriteProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
            writer.WriteWhitespace("\n");

            // Everything is embedded in a Workbook node...
            writer.WriteStartElement("Workbook", "urn:schemas-microsoft-com:office:spreadsheet");

            // Write Workbook attributes...
            writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
            writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
            writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
            writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");

            this.Properties.Export(writer);

            // Styles
            writer.WriteStartElement("Styles");

            // Write styles...
            foreach (XmlStyle xs in this.Styles)
            {
                xs.Export(writer);
            }

            // End styles declaration
            writer.WriteEndElement();

            this.ExportNamedRanges(writer, null);

            // Start worksheets
            foreach (Worksheet ws in this.Worksheets)
            {
                ws.Export(writer);
            }

            // End worksheets

            // End Workbook
            writer.WriteEndElement();

            // End Document
            writer.WriteEndDocument();

            // Clean up
            writer.Flush();
            writer.Close();

            return true;
        }
        #endregion
    }
}
