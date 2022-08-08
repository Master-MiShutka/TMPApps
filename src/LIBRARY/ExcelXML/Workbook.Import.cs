namespace TMP.ExcelXml
{
    using System;
    using System.IO;
    using System.Security;
    using System.Xml;
    using TMP.Extensions;

    partial class ExcelXmlWorkbook
    {
        #region Import Work book

        /// <summary>
        /// Imports a excel xml workbook into a ExcelXmlWorkbook instance
        /// </summary>
        /// <param name="importFile">File to import</param>
        /// <returns>If import was successful, the ExcelXmlWorkbook instance, null otherwise</returns>
        public static ExcelXmlWorkbook Import(string importFile)
        {
            if (!File.Exists(importFile))
            {
                return null;
            }

            Stream stream;

            try
            {
                stream = new FileStream(importFile, FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                return null;
            }
            catch (SecurityException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            ExcelXmlWorkbook book = Import(stream);

            stream.Close();
            stream.Dispose();

            return book;
        }

        /// <summary>
        /// Imports a excel xml workbook into a ExcelXmlWorkbook instance
        /// </summary>
        /// <param name="stream">Stream to import</param>
        /// <returns>If import was successful, the ExcelXmlWorkbook instance, null otherwise</returns>
        public static ExcelXmlWorkbook Import(Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CloseInput = false;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;

            if (!stream.CanRead)
            {
                return null;
            }

            XmlReader reader = XmlReader.Create(stream, settings);

            ExcelXmlWorkbook book = new ExcelXmlWorkbook();
            book.Styles.Clear();

            int workSheet = 0;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        // Document Properties
                        case "DocumentProperties":
                            {
                                if (!reader.IsEmptyElement)
                                {
                                    book.Properties.Import(reader);
                                }

                                break;
                            }

                        case "Styles":
                            {
                                if (!reader.IsEmptyElement)
                                {
                                    book.ImportStyles(reader);
                                }

                                break;
                            }

                        case "Names":
                            {
                                ImportNamedRanges(reader, book, null);

                                break;
                            }

                        case "Worksheet":
                            {
                                if (!reader.IsEmptyElement)
                                {
                                    book[workSheet++].Import(reader);
                                }

                                break;
                            }
                    }
                }
            }

            book.ResolveNamedRangeReferences();

            book.ResolveCellReferences();

            reader.Close();

            stream.Close();
            stream.Dispose();

            return book;
        }
        #endregion

        #region Import Named Ranges
        internal static void ImportNamedRanges(XmlReader reader, ExcelXmlWorkbook book, Worksheet ws)
        {
            if (!reader.IsEmptyElement)
            {
                while (reader.Read() && !(reader.Name == "Names" && reader.NodeType == XmlNodeType.EndElement))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "NamedRange")
                        {
                            Range range = null;
                            string name = string.Empty;

                            foreach (XmlReaderAttributeItem xa in reader.GetAttributes())
                            {
                                if (xa.LocalName == "Name" && xa.HasValue)
                                {
                                    name = xa.Value;
                                }

                                if (xa.LocalName == "RefersTo" && xa.HasValue)
                                {
                                    range = new Range(xa.Value);
                                }
                            }

                            NamedRange nr = new NamedRange(range, name, ws);
                            book.NamedRanges.Add(nr);
                        }
                    }
                }
            }
        }
        #endregion

        #region Import Styles
        private void ImportStyles(XmlReader reader)
        {
            while (reader.Read() && !(reader.Name == "Styles" && reader.NodeType == XmlNodeType.EndElement))
            {
                XmlStyle style = XmlStyle.Import(reader);

                if (style != null)
                {
                    this.Styles.Add(style);
                }
            }
        }
        #endregion

        #region Resolve Ranges
        private void ResolveNamedRangeReferences()
        {
            int printTitleIndex = -1;
            int i = -1;

            foreach (NamedRange nr in this.NamedRanges)
            {
                i++;

                if (nr.Name == "Print_Titles")
                {
                    FormulaParser.ParsePrintHeaders(nr.Worksheet, nr.Range.UnresolvedRangeReference);

                    printTitleIndex = i;
                }
                else
                {
                    Worksheet ws = nr.Worksheet ?? this[0];

                    nr.Range.ParseUnresolvedReference(ws[0, 0]);

                    if (nr.Name == "_FilterDatabase")
                    {
                        ws.AutoFilter = true;
                    }

                    if (nr.Name == "Print_Area")
                    {
                        ws.PrintArea = true;
                    }
                }
            }

            if (printTitleIndex != -1)
            {
                this.NamedRanges.RemoveAt(printTitleIndex);
            }
        }

        private void ResolveCellReferences()
        {
            for (int sheetIndex = 0; sheetIndex < this.Worksheets.Count; sheetIndex++)
            {
                Worksheet sheet = this[sheetIndex];

                foreach (Row row in sheet.Rows)
                {
                    foreach (Cell cell in row.cells)
                    {
                        cell.ResolveReferences();
                    }
                }
            }
        }
        #endregion
    }
}