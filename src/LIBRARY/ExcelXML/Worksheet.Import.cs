namespace TMP.ExcelXml
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml;
    using TMP.Extensions;

    public partial class Worksheet
    {
        #region Import Work sheet
        internal void Import(XmlReader reader)
        {
            foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
            {
                if (xa.LocalName == "Name" && xa.HasValue)
                {
                    this.Name = xa.Value;
                }

                if (xa.LocalName == "StyleID" && xa.HasValue)
                {
                    this.Style = this.ParentBook.GetStyleByID(xa.Value);
                }
            }

            while (reader.Read() && !(reader.Name == "Worksheet" && reader.NodeType == XmlNodeType.EndElement))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Names":
                            {
                                ExcelXmlWorkbook.ImportNamedRanges(reader, this.GetParentBook(), this);

                                break;
                            }

                        case "Table":
                            {
                                this.ImportTable(reader);

                                break;
                            }

                        case "WorksheetOptions":
                            {
                                this.ImportOptions(reader);

                                break;
                            }
                    }
                }
            }
        }
        #endregion

        #region Import Book Options
        private void ImportOptions(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }

            while (reader.Read() && !(reader.Name == "WorksheetOptions" && reader.NodeType == XmlNodeType.EndElement))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "PageSetup":
                            {
                                this.ImportPageSetup(reader);

                                break;
                            }

                        case "FitToPage":
                            {
                                this.PrintOptions.FitToPage = true;

                                break;
                            }

                        case "TabColorIndex":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    int i;
                                    if (reader.Value.ParseToInt(out i))
                                    {
                                        this.TabColor = i;
                                    }
                                }

                                break;
                            }

                        case "Print":
                            {
                                this.ImportPrintOptions(reader);

                                break;
                            }

                        case "SplitHorizontal":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    int i;
                                    if (reader.Value.ParseToInt(out i))
                                    {
                                        this.FreezeTopRows = i;
                                    }
                                }

                                break;
                            }

                        case "SplitVertical":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    int i;
                                    if (reader.Value.ParseToInt(out i))
                                    {
                                        this.FreezeLeftColumns = i;
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }

        private void ImportPrintOptions(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }

            while (reader.Read() && !(reader.Name == "Print" && reader.NodeType == XmlNodeType.EndElement))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "FitHeight":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    int i;
                                    if (reader.Value.ParseToInt(out i))
                                    {
                                        this.PrintOptions.FitHeight = i;
                                    }
                                }

                                break;
                            }

                        case "FitWidth":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    int i;
                                    if (reader.Value.ParseToInt(out i))
                                    {
                                        this.PrintOptions.FitWidth = i;
                                    }
                                }

                                break;
                            }

                        case "Scale":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    int i;
                                    if (reader.Value.ParseToInt(out i))
                                    {
                                        this.PrintOptions.Scale = i;
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }

        private void ImportPageSetup(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }

            while (reader.Read() && !(reader.Name == "PageSetup" && reader.NodeType == XmlNodeType.EndElement))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Layout":
                            {
                                XmlReaderAttributeItem xa = reader.GetSingleAttribute("Orientation", true);
                                if (xa != null)
                                {
                                    this.PrintOptions.Orientation = ObjectExtensions.ParseEnum<PageOrientation>(xa.Value);
                                }

                                break;
                            }

                        case "Header":
                            {
                                XmlReaderAttributeItem xa = reader.GetSingleAttribute("Margin", true);
                                if (xa != null)
                                {
                                    double d;
                                    if (xa.Value.ParseToInt(out d))
                                    {
                                        this.PrintOptions.HeaderMargin = d;
                                    }
                                }

                                break;
                            }

                        case "Footer":
                            {
                                XmlReaderAttributeItem xa = reader.GetSingleAttribute("Margin", true);
                                if (xa != null)
                                {
                                    double d;
                                    if (xa.Value.ParseToInt(out d))
                                    {
                                        this.PrintOptions.FooterMargin = d;
                                    }
                                }

                                break;
                            }

                        case "PageMargins":
                            {
                                foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
                                {
                                    double d;
                                    if (xa.Value.ParseToInt(out d))
                                    {
                                        switch (xa.LocalName)
                                        {
                                            case "Bottom":
                                                this.PrintOptions.BottomMargin = d;
                                                break;

                                            case "Left":
                                                this.PrintOptions.LeftMargin = d;
                                                break;

                                            case "Right":
                                                this.PrintOptions.RightMargin = d;
                                                break;

                                            case "Top":
                                                this.PrintOptions.TopMargin = d;
                                                break;
                                        }
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }
        #endregion

        #region Import Table
        private void ImportTable(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }

            int column = 0;

            while (reader.Read() && !(reader.Name == "Table" && reader.NodeType == XmlNodeType.EndElement))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Column":
                            {
                                double width = 0;
                                bool hidden = false;
                                int span = 1;
                                XmlStyle style = null;

                                foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
                                {
                                    if (xa.LocalName == "Width" && xa.HasValue)
                                    {
                                        double d;
                                        if (xa.Value.ParseToInt(out d))
                                        {
                                            width = d;
                                        }
                                    }

                                    if (xa.LocalName == "Hidden" && xa.HasValue)
                                    {
                                        hidden = xa.Value == "1";
                                    }

                                    if (xa.LocalName == "Index" && xa.HasValue)
                                    {
                                        xa.Value.ParseToInt(out column);
                                    }

                                    if (xa.LocalName == "Span" && xa.HasValue)
                                    {
                                        xa.Value.ParseToInt(out span);
                                    }

                                    if (xa.LocalName == "StyleID" && xa.HasValue)
                                    {
                                        style = this.ParentBook.GetStyleByID(xa.Value);
                                    }
                                }

                                for (int i = 1; i <= span; i++)
                                {
                                    this.Columns(column).Width = width;
                                    this.Columns(column).Hidden = hidden;

                                    if (style != null)
                                    {
                                        this.Columns(column).Style = style;
                                    }

                                    column++;
                                }

                                break;
                            }

                        case "Row":
                            {
                                this.ImportRow(reader);

                                break;
                            }
                    }
                }
            }
        }
        #endregion

        #region Import Row
        private void ImportRow(XmlReader reader)
        {
            bool isEmpty = reader.IsEmptyElement;

            int rowIndex = this.Rows.Count;

            double height = -1;
            XmlStyle style = null;
            bool hidden = false;
            foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
            {
                if (xa.LocalName == "Height" && xa.HasValue)
                {
                    xa.Value.ParseToInt(out height);
                }

                if (xa.LocalName == "Index" && xa.HasValue)
                {
                    xa.Value.ParseToInt(out rowIndex);

                    rowIndex--;
                }

                if (xa.LocalName == "StyleID" && xa.HasValue)
                {
                    style = this.ParentBook.GetStyleByID(xa.Value);
                }

                if (xa.LocalName == "Hidden" && xa.HasValue)
                {
                    hidden = xa.Value == "1";
                }
            }

            Row row = this.GetRowByIndex(rowIndex);
            row.Hidden = hidden;
            if (height != -1)
            {
                row.Height = height;
            }

            if (style != null)
            {
                row.Style = style;
            }

            if (isEmpty)
            {
                return;
            }

            while (reader.Read() && !(reader.Name == "Row" && reader.NodeType == XmlNodeType.EndElement))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "Cell")
                    {
                        this.ImportCell(reader, row);
                    }
                }
            }
        }
        #endregion

        #region Import Cell
        private void ImportCell(XmlReader reader, Row row)
        {
            bool isEmpty = reader.IsEmptyElement;

            int cellIndex = row.cells.Count;

            int mergeDown = 0;
            int mergeAcross = 0;

            XmlStyle style = null;
            string formula = string.Empty;
            string reference = string.Empty;
            foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
            {
                if (xa.LocalName == "Index" && xa.HasValue)
                {
                    xa.Value.ParseToInt(out cellIndex);

                    cellIndex--;
                }

                if (xa.LocalName == "StyleID" && xa.HasValue)
                {
                    style = this.ParentBook.GetStyleByID(xa.Value);
                }

                if (xa.LocalName == "HRef" && xa.HasValue)
                {
                    reference = xa.Value;
                }

                if (xa.LocalName == "Formula" && xa.HasValue)
                {
                    formula = xa.Value;
                }

                if (xa.LocalName == "MergeAcross" && xa.HasValue)
                {
                    xa.Value.ParseToInt(out mergeAcross);
                }

                if (xa.LocalName == "MergeDown" && xa.HasValue)
                {
                    xa.Value.ParseToInt(out mergeDown);
                }
            }

            Cell cell = this.Cells(cellIndex, row.rowIndex);
            if (style != null)
            {
                cell.Style = style;
            }

            if (!reference.IsNullOrEmpty())
            {
                cell.HRef = reference;
            }

            if (!formula.IsNullOrEmpty())
            {
                FormulaParser.Parse(cell, formula);

                return;
            }

            if (isEmpty)
            {
                return;
            }

            if (mergeDown > 0 || mergeAcross > 0)
            {
                cell.MergeStart = true;

                Range range = new Range(cell, this.Cells(cellIndex + mergeAcross, row.rowIndex + mergeDown));

                this.MergedCells.Add(range);

                cell.ColumnSpan = range.ColumnCount;
                cell.RowSpan = range.RowCount;
            }

            while (reader.Read() && !(reader.Name == "Cell" && reader.NodeType == XmlNodeType.EndElement))
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "Data")
                    {
                        ImportCellData(reader, cell);
                    }
                    else if (reader.Name == "Comment")
                    {
                        ImportCellComment(reader, cell);
                    }
                }
            }
        }

        internal static void ImportCellData(XmlReader reader, Cell cell)
        {
            if (reader.IsEmptyElement)
            {
                cell.Value = string.Empty;

                return;
            }

            XmlReaderAttributeItem xa = reader.GetSingleAttribute("Type");
            if (xa != null)
            {
                reader.Read();

                if (reader.NodeType != XmlNodeType.Text)
                {
                    cell.Value = string.Empty;

                    return;
                }

                switch (xa.Value)
                {
                    case "String":
                        {
                            cell.Value = reader.Value;

                            break;
                        }

                    case "Number":
                        {
                            decimal d;
                            if (reader.Value.ParseToInt(out d))
                            {
                                cell.Value = d;
                            }
                            else
                            {
                                cell.Value = reader.Value;
                            }

                            break;
                        }

                    case "DateTime":
                        {
                            DateTime date;
                            if (DateTime.TryParseExact(reader.Value, "yyyy-MM-dd\\Thh:mm:ss.fff",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                            {
                                cell.Value = date;
                            }
                            else
                            {
                                cell.Value = reader.Value;
                            }

                            break;
                        }

                    case "Boolean":
                        {
                            cell.Value = reader.Value == "1";
                            break;
                        }
                }
            }
        }

        internal static void ImportCellComment(XmlReader reader, Cell cell)
        {
            reader.Read();

            if (reader.LocalName == "Data")
            {
                string comment = reader.ReadInnerXml();

                if (!comment.IsNullOrEmpty())
                {
                    cell.Comment = comment;
                }
            }
        }
        #endregion
    }
}