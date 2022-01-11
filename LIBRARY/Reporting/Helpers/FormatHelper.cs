namespace TMP.UI.Controls.WPF.Reporting.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;

    /// <summary>
    /// Тип разделителя в Csv файле
    /// </summary>
    public enum CsvValueSeparatorType
    {
        Comma,
        Tab,
        Semicolon,
        Custom,
    }

    /// <summary>
    /// Интерфейс, определяющий параметры форматирования данных
    /// </summary>
    public interface IFormatSettings
    {
        string DateTimeFormat { get; set; }

        string NumericFormat { get; set; }

        string FloatingPointFormat { get; set; }

        CsvValueSeparatorType SeparatorType { get; set; }

        char Separator { get; set; }

        char TextQualifier { get; set; }

        string NewLine { get; set; }

        CultureInfo Culture { get; set; }
    }

    /// <summary>
    /// Ячейка HTML таблицы
    /// </summary>
    public struct HtmlTableCell
    {
        public enum ThScope
        {
            none,

            /// <summary>
            /// Specifies that the cell is a header for a column
            /// </summary>
            col,

            /// <summary>
            /// Specifies that the cell is a header for a row
            /// </summary>
            row,

            /// <summary>
            /// Specifies that the cell is a header for a group of columns
            /// </summary>
            colgroup,

            /// <summary>
            /// Specifies that the cell is a header for a group of rows
            /// </summary>
            rowgroup,
        }

        public object Value { get; set; }

        public int RowSpan { get; set; }

        public int ColSpan { get; set; }

        public ThScope Scope { get; set; }

        public HtmlTableCell(object o)
        {
            this.Value = string.Empty;
            this.RowSpan = 1;
            this.ColSpan = 1;
            this.Scope = ThScope.none;
        }
    }

    /// <summary>
    /// Вспомогательный класс для преобразования данных и экспорта в другие форматы
    /// </summary>
    public static class FormatHelper
    {
        public static string GetMatrixCellValueAsString(MatrixGrid.IMatrixCell cell)
        {
            if (cell == null)
            {
                return string.Empty;
            }
            else
            {
                return (cell is MatrixGrid.IMatrixHeader header)
                        ? header.Header
                        : ((cell is MatrixGrid.IMatrixDataCell data) ? string.Format(string.Format("{{0:{0}}}", data.ContentFormat), data.Value) : null);
            }
        }

        /// <summary>
        /// Преобразует матрицу в HTML таблицу
        /// </summary>
        /// <param name="matrix">Матрица для преобразования</param>
        /// <returns>HTML таблица</returns>
        public static string MatrixToHtmlTable(MatrixGrid.IMatrix matrix)
        {
            Func<MatrixGrid.IMatrixCell, HtmlTableCell.ThScope> setCellScope = (matrixCell) =>
            {
                if (matrixCell is MatrixGrid.IMatrixHeader headerCell)
                {
                    if (headerCell.CellType == MatrixGrid.MatrixCellType.ColumnHeader || headerCell.CellType == MatrixGrid.MatrixCellType.ColumnsGroupHeader || headerCell.CellType == MatrixGrid.MatrixCellType.ColumnSummaryHeader)
                    {
                        if (headerCell.ChildrenCount > 0)
                        {
                            return HtmlTableCell.ThScope.colgroup;
                        }
                        else
                        {
                            return HtmlTableCell.ThScope.col;
                        }
                    }
                    else
                    {
                        if (headerCell.CellType == MatrixGrid.MatrixCellType.RowHeader || headerCell.CellType == MatrixGrid.MatrixCellType.RowsGroupHeader || headerCell.CellType == MatrixGrid.MatrixCellType.RowSummaryHeader)
                        {
                            if (headerCell.ChildrenCount > 0)
                            {
                                return HtmlTableCell.ThScope.rowgroup;
                            }
                            else
                            {
                                return HtmlTableCell.ThScope.row;
                            }
                        }
                        else
                        {
                            return HtmlTableCell.ThScope.none;
                        }
                    }
                }
                else
                {
                    return HtmlTableCell.ThScope.none;
                }
            };

            Func<MatrixGrid.IMatrixCell, HtmlTableCell> matrixCellToHtmlCell = (matrixCell) => new HtmlTableCell()
            {
                Value = GetMatrixCellValueAsString(matrixCell),
                RowSpan = matrixCell.GridRowSpan,
                ColSpan = matrixCell.GridColumnSpan,

                Scope = setCellScope(matrixCell),
            };

            Func<MatrixGrid.IMatrixCell, string> getMatrixCellStyleForHtml = (matrixCell) =>
           {
               Thickness thickness = (Thickness)Converters.HeaderBorderThicknessConverter.Singleton.Convert(matrixCell, null);

               string borderColor = "gray";

               return string.Format("border-top: {1}pt solid {0}; border-bottom: {2}pt solid {0}; border-left: {3}pt solid {0}; border-right: {4}pt solid {0}; {5};",
                   borderColor,
                   (thickness.Top / 2).ToString("N1", System.Globalization.CultureInfo.InvariantCulture),
                   (thickness.Bottom / 2).ToString("N1", System.Globalization.CultureInfo.InvariantCulture),
                   (thickness.Left / 2).ToString("N1", System.Globalization.CultureInfo.InvariantCulture),
                   (thickness.Right / 2).ToString("N1", System.Globalization.CultureInfo.InvariantCulture),
                   matrixCell is MatrixGrid.IMatrixSummaryCell summaryCell ? (summaryCell.SummaryType == MatrixGrid.MatrixSummaryType.TotalSummary ? "font-weight: bolder; background-color: Gray; color: White" : "font-weight: bold; background-color: LightGray")
                    : (matrixCell is MatrixGrid.IMatrixHeader headerCell ? (headerCell.CellType is MatrixGrid.MatrixCellType.RowHeader or MatrixGrid.MatrixCellType.RowsGroupHeader or MatrixGrid.MatrixCellType.RowSummaryHeader ? "font-weight: bold" : string.Empty) : string.Empty));
           };

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<TABLE CELLSPACING=0 border=0 style='border-collapse: collapse;'>");

            // пустая ячейка в верхнем левом углу должна быть одна
            // и за ней ожидается строка(и) заголовков столбцов
            var emptyCell = matrix.Items.Cast<MatrixGrid.IMatrixHeader>().Where(i => i.CellType == MatrixGrid.MatrixCellType.Empty).FirstOrDefault();
            if (emptyCell != null)
            {
                sb.AppendLine("<THEAD>");
                FormatHelper.HtmlFormatTableCell(matrixCellToHtmlCell(emptyCell), true, false, sb, cellTag: "TH", cellStyle: getMatrixCellStyleForHtml(emptyCell));

                // список строк матрицы, упорядоченный по номеру строки
                var rowsWithColumnHeaders = matrix.Items
                    .Where(i =>
                        i.CellType == MatrixGrid.MatrixCellType.ColumnHeader ||
                        i.CellType == MatrixGrid.MatrixCellType.ColumnsGroupHeader ||
                        i.CellType == MatrixGrid.MatrixCellType.ColumnSummaryHeader)
                    .Where(i => i.GridRow < emptyCell.GridRowSpan)
                    .GroupBy(i => i.GridRow)
                    .OrderBy(i => i.Key)
                    .ToList();

                bool flag = false;
                foreach (var row in rowsWithColumnHeaders)
                {
                    // список столбцов матрицы, упорядоченный по номеру столбца
                    var columns = row
                        .GroupBy(i => i.GridColumn)
                        .OrderBy(i => i.Key);
                    int columnsCount = columns.Count();
                    int index = 0;
                    foreach (var column in columns)
                    {
                        MatrixGrid.IMatrixCell cell = column.FirstOrDefault();
                        FormatHelper.HtmlFormatTableCell(matrixCellToHtmlCell(cell), index == 0 && flag, index == columnsCount - 1, sb, cellTag: "TH", cellStyle: getMatrixCellStyleForHtml(cell));
                        index++;
                    }

                    if (flag == false)
                    {
                        flag = true;
                    }
                }

                sb.AppendLine("</THEAD>");
            }

            sb.AppendLine("<TBODY>");
            var rowsWithRowHeadersAndDataCells = matrix.Items
                    .Where(i =>
                        i.CellType == MatrixGrid.MatrixCellType.RowHeader ||
                        i.CellType == MatrixGrid.MatrixCellType.RowsGroupHeader ||
                        i.CellType == MatrixGrid.MatrixCellType.DataCell ||
                        i.CellType == MatrixGrid.MatrixCellType.RowSummaryHeader ||
                        i.CellType == MatrixGrid.MatrixCellType.SummaryCell)
                    .Where(i => i.GridRow >= emptyCell.GridRowSpan)
                    .GroupBy(i => i.GridRow)
                    .OrderBy(i => i.Key)
                    .ToList();
            foreach (var row in rowsWithRowHeadersAndDataCells)
            {
                // список столбцов матрицы, упорядоченный по номеру столбца
                var columns = row
                    .GroupBy(i => i.GridColumn)
                    .OrderBy(i => i.Key);
                int columnsCount = columns.Count();
                int index = 0;
                foreach (var column in columns)
                {
                    MatrixGrid.IMatrixCell cell = column.FirstOrDefault();
                    FormatHelper.HtmlFormatTableCell(matrixCellToHtmlCell(cell), index == 0, index == columnsCount - 1, sb, cellStyle: getMatrixCellStyleForHtml(cell));
                    index++;
                }
            }

            sb.AppendLine("</TBODY>");
            sb.Append("</TABLE>");

            return sb.ToString().Replace("\r", string.Empty).Replace("\n", Environment.NewLine).Replace("\r\n", Environment.NewLine);
        }

        /// <summary>
        /// Преобразует матрицу в текст
        /// </summary>
        /// <param name="matrix">Матрица для преобразования</param>
        /// <returns>Текстовое представление</returns>
        public static string MatrixToPlainText(MatrixGrid.IMatrix matrix)
        {
            if (matrix == null || matrix.Cells == null)
            {
                return string.Empty;
            }

            int matrixHeight = (int)matrix.Size.Height, matrixWidth = (int)matrix.Size.Width;
            string[,] data = new string[matrixHeight, matrixWidth];

            // заполнение всего пространства, которое занимает ячейка, кроме верхнего левого угла, символом табуляции
            Action<MatrixGrid.IMatrixCell> fillSpanningCells = (cell) =>
            {
                for (int rowIndex = cell.GridRow + 1; rowIndex < cell.GridRow + cell.GridRowSpan; rowIndex++)
                {
                    for (int columnIndex = cell.GridColumn + 1; columnIndex < cell.GridColumn + cell.GridColumnSpan; columnIndex++)
                    {
                        data[rowIndex, columnIndex] = "\t";
                    }
                }
            };

            for (int rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    MatrixGrid.IMatrixCell cell = matrix.Cells[rowIndex, columnIndex];
                    if (cell == null)
                    {
                        continue;
                    }

                    data[rowIndex, columnIndex] = GetMatrixCellValueAsString(cell);
                    if (columnIndex < matrixWidth - 1)
                    {
                        data[rowIndex, columnIndex] += "\t";
                    }

                    fillSpanningCells(cell);
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int rowIndex = 0; rowIndex < matrixHeight; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    sb.Append(data[rowIndex, columnIndex]);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Преобразует матрицу во System.Windows.Documents.Section, содержащий Table
        /// </summary>
        /// <param name="matrix">Матрица для преобразования</param>
        /// <returns>System.Windows.Documents.Section</returns>
        public static System.Windows.Documents.Section MatrixToFlowDocumentSectionWithTable(MatrixGrid.IMatrix matrix)
        {
            if (matrix == null || matrix.Cells == null)
            {
                return new System.Windows.Documents.Section();
            }

            int matrixHeight = (int)matrix.Size.Height, matrixWidth = (int)matrix.Size.Width;
            int matrixColumnHeadersHeight = matrix.Cells[0, 0].GridRowSpan;
            int matrixRowsHeadersWidth = matrix.Cells[0, 0].GridColumnSpan;

            Action<MatrixGrid.IMatrixCell, System.Windows.Documents.TableCell> setBorders = (matrixCell, tableCell) =>
            {
                Thickness thickness = (Thickness)Converters.HeaderBorderThicknessConverter.Singleton.Convert(matrixCell, null);
                tableCell.BorderThickness = thickness;
            };

            System.Windows.Documents.Table table = new System.Windows.Documents.Table();
            #region resources
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("/ui.controls.wpf.reporting;component/Themes/FlowDocumentTableStyles.xaml", UriKind.RelativeOrAbsolute);
            table.Resources = rd;
            #endregion

            // добавление колонок
            // for (int columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
            //    table.Columns.Add(new System.Windows.Documents.TableColumn());
            #region шапка таблицы
            System.Windows.Documents.TableRowGroup headerRowGroup = new System.Windows.Documents.TableRowGroup();
            for (int rowIndex = 0; rowIndex < matrixColumnHeadersHeight; rowIndex++)
            {
                System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();

                for (int columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    MatrixGrid.IMatrixCell cell = matrix.Cells[rowIndex, columnIndex];
                    if (cell == null)
                    {
                        continue;
                    }

                    System.Windows.Documents.TableCell tableCell = new System.Windows.Documents.TableCell();
                    setBorders(cell, tableCell);

                    if (cell is MatrixGrid.IMatrixHeader headerCell)
                    {
                        if (headerCell.CellType != MatrixGrid.MatrixCellType.Empty)
                        {
                            tableCell.Background = System.Windows.Media.Brushes.LightGray;
                        }
                        else
                        {
                            if (headerCell.CellType == MatrixGrid.MatrixCellType.ColumnsGroupHeader || headerCell.CellType == MatrixGrid.MatrixCellType.ColumnSummaryHeader)
                            {
                                tableCell.Background = System.Windows.Media.Brushes.DarkGray;
                                tableCell.FontWeight = FontWeights.SemiBold;
                            }
                        }
                    }

                    tableCell.RowSpan = cell.GridRowSpan;
                    tableCell.ColumnSpan = cell.GridColumnSpan;
                    tableCell.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(GetMatrixCellValueAsString(cell))));

                    row.Cells.Add(tableCell);
                }

                headerRowGroup.Rows.Add(row);
            }

            table.RowGroups.Add(headerRowGroup);
            #endregion

            #region данные
            int dataRowsMaxGridRow = matrix.ShowColumnsTotal.GetValueOrDefault() ? matrixHeight - 1 : matrixHeight;
            System.Windows.Documents.TableRowGroup dataRowGroup = new System.Windows.Documents.TableRowGroup();
            for (int rowIndex = matrixColumnHeadersHeight; rowIndex < dataRowsMaxGridRow; rowIndex++)
            {
                System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();

                for (int columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    MatrixGrid.IMatrixCell cell = matrix.Cells[rowIndex, columnIndex];
                    if (cell == null)
                    {
                        continue;
                    }

                    System.Windows.Documents.TableCell tableCell = new System.Windows.Documents.TableCell();
                    setBorders(cell, tableCell);

                    if (cell is MatrixGrid.IMatrixHeader headerCell)
                    {
                        tableCell.Background = System.Windows.Media.Brushes.LightGray;
                    }
                    else
                    {
                        if (cell is MatrixGrid.IMatrixSummaryCell summaryCell)
                        {
                            if (summaryCell.SummaryType == MatrixGrid.MatrixSummaryType.RowSummary || summaryCell.SummaryType == MatrixGrid.MatrixSummaryType.RowsGroupSummary)
                            {
                                tableCell.Background = System.Windows.Media.Brushes.DarkGray;
                                tableCell.FontWeight = FontWeights.SemiBold;
                            }
                        }
                    }

                    tableCell.RowSpan = cell.GridRowSpan;
                    tableCell.ColumnSpan = cell.GridColumnSpan;
                    tableCell.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(GetMatrixCellValueAsString(cell))));

                    row.Cells.Add(tableCell);
                }

                dataRowGroup.Rows.Add(row);
            }

            table.RowGroups.Add(dataRowGroup);
            #endregion

            #region строка итогов

            if (matrix.ShowColumnsTotal.GetValueOrDefault())
            {
                System.Windows.Documents.TableRowGroup totalRowGroup = new System.Windows.Documents.TableRowGroup();
                System.Windows.Documents.TableRow row = new System.Windows.Documents.TableRow();

                for (int columnIndex = 0; columnIndex < matrixWidth; columnIndex++)
                {
                    MatrixGrid.IMatrixCell cell = matrix.Cells[matrixHeight - 1, columnIndex];
                    if (cell == null)
                    {
                        continue;
                    }

                    System.Windows.Documents.TableCell tableCell = new System.Windows.Documents.TableCell();
                    setBorders(cell, tableCell);
                    tableCell.Background = System.Windows.Media.Brushes.DarkGray;
                    tableCell.FontWeight = FontWeights.Bold;

                    if (cell is MatrixGrid.IMatrixSummaryCell summaryCell)
                    {
                        if (summaryCell.CellType == MatrixGrid.MatrixCellType.RowSummaryHeader)
                        {
                            tableCell.FontWeight = FontWeights.SemiBold;
                        }
                        else
                            if (summaryCell.SummaryType == MatrixGrid.MatrixSummaryType.TotalSummary)
                        {
                            tableCell.Background = System.Windows.Media.Brushes.Gray;
                            tableCell.Foreground = System.Windows.Media.Brushes.White;
                            tableCell.FontWeight = FontWeights.ExtraBold;
                        }
                    }
                    else
                    {
                        tableCell.Background = System.Windows.Media.Brushes.LightGray;
                    }

                    tableCell.RowSpan = cell.GridRowSpan;
                    tableCell.ColumnSpan = cell.GridColumnSpan;
                    tableCell.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(GetMatrixCellValueAsString(cell))));

                    row.Cells.Add(tableCell);
                }

                totalRowGroup.Rows.Add(row);
                table.RowGroups.Add(totalRowGroup);
            }
            #endregion

            System.Windows.Documents.Section section = new System.Windows.Documents.Section();
            section.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(matrix.Header) { FontWeight = FontWeights.Bold }) { TextAlignment = TextAlignment.Center });
            section.Blocks.Add(table);
            section.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(matrix.Description) { FontWeight = FontWeights.Thin, FontStyle = FontStyles.Italic }) { TextAlignment = TextAlignment.Left });

            return section;
        }

        /// <summary>
        /// Преобразование матрицы в различные форматы и отправка их в буфер обмена
        /// </summary>
        /// <param name="matrix">Матрица</param>
        public static void MatrixToClipboard(MatrixGrid.IMatrix matrix)
        {
            IDataObject dataObject = new DataObject();

            ((IDataObject)dataObject).SetData(DataFormats.Html, CreateHtmlDataFormatFromMatrix(matrix));
            ((IDataObject)dataObject).SetData(DataFormats.UnicodeText, MatrixToPlainText(matrix));
            string xaml = FrameworkContentElementToXaml(MatrixToFlowDocumentSectionWithTable(matrix));
            ((IDataObject)dataObject).SetData(DataFormats.Rtf, XamlToRtf(xaml));
            ((IDataObject)dataObject).SetData(DataFormats.Xaml, xaml);

            Clipboard.SetDataObject(dataObject, true);
        }

        public static void HtmlFormatTableCell(HtmlTableCell cell, bool firstCell, bool lastCell, StringBuilder sb, string cellClassname = null, string cellStyle = null, string cellTag = "TD")
        {
            if (firstCell)
            {
                // First cell - append start of row
                sb.Append("<TR>\n");
            }

            sb.AppendFormat("\t<{0}", cellTag);
            sb.Append(" align=center valign=middle");
            if (cellClassname != null)
            {
                sb.AppendFormat(" class={0}", cellClassname);
            }

            if (cellStyle != null)
            {
                sb.AppendFormat(" style='{0}'", cellStyle);
            }

            if (cell.RowSpan > 1)
            {
                sb.AppendFormat(" rowspan='{0}'", cell.RowSpan);
            }

            if (cell.ColSpan > 1)
            {
                sb.AppendFormat(" colspan='{0}'", cell.ColSpan);
            }

            if (cell.Scope != HtmlTableCell.ThScope.none)
            {
                sb.AppendFormat(" scope='{0}'", cell.Scope);
            }

            sb.Append(">");

            if (cell.Value != null)
            {
                FormatPlainTextAsHtml(cell.Value == null ? string.Empty : cell.Value.ToString(), new StringWriter(sb, CultureInfo.CurrentCulture));
            }
            else
            {
                sb.Append("&nbsp;");
            }

            sb.AppendFormat("</{0}>\n", cellTag); // End cell
            if (lastCell)
            {
                // Last cell - append end of row
                sb.Append("</TR>\n");
            }
        }

        public static void HtmlFormatTableCell(object cellValue, bool firstCell, bool lastCell, StringBuilder sb, string cellClassname = null, string cellTag = "TD")
        {
            string style = string.Empty;
            if (cellValue != null && cellValue.GetType() == typeof(string) && string.IsNullOrWhiteSpace(cellClassname) == false)
            {
                style = " class=text";
            }
            else
            if (string.IsNullOrWhiteSpace(cellClassname) == false)
            {
                style = string.Format(" class={0}", cellClassname);
            }

            if (firstCell)
            {
                // First cell - append start of row
                sb.Append("<TR>");
            }

            sb.AppendFormat("<{0}{1}>", cellTag, style); // Start cell
            if (cellValue != null)
            {
                FormatPlainTextAsHtml(cellValue == null ? " " : cellValue.ToString(), new StringWriter(sb, CultureInfo.CurrentCulture));
            }
            else
            {
                sb.Append("&nbsp;");
            }

            sb.AppendFormat("</{0}>", cellTag); // End cell
            if (lastCell)
            {
                // Last cell - append end of row
                sb.Append("</TR>");
            }
        }

        public static void FormatPlainTextAsHtml(string s, TextWriter output)
        {
            if (s == null)
            {
                return;
            }

            int cb = s.Length;
            char prevCh = '\0';

            for (int i = 0; i < cb; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '<':
                        output.Write("&lt;");
                        break;
                    case '>':
                        output.Write("&gt;");
                        break;
                    case '"':
                        output.Write("&quot;");
                        break;
                    case '&':
                        output.Write("&amp;");
                        break;
                    case ' ':
                        if (prevCh == ' ')
                        {
                            output.Write("&nbsp;");
                        }
                        else
                        {
                            output.Write(ch);
                        }

                        break;
                    case '\r':
                        // Ignore \r, only handle \n
                        break;
                    case '\n':
                        output.Write("<br>");
                        break;

                    default:
                        // The seemingly arbitrary 160 comes from RFC
                        if (ch == 160)
                        {
                            output.Write(' ');
                        }
                        else
                        if (ch > 160 && ch < 256)
                        {
                            output.Write("&#");
                            output.Write(((int)ch).ToString(NumberFormatInfo.InvariantInfo));
                            output.Write(';');
                        }
                        else
                        {
                            output.Write(ch);
                        }

                        break;
                }

                prevCh = ch;
            }
        }

        public static string FrameworkContentElementToXaml(FrameworkContentElement frameworkContentElement)
        {
            return System.Windows.Markup.XamlWriter.Save(frameworkContentElement);
        }

        public static string XamlToRtf(string xamlText)
        {
            var richTextBox = new System.Windows.Controls.RichTextBox();
            var textRange = new System.Windows.Documents.TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            // Create a MemoryStream of the xaml content
            using (var xamlMemoryStream = new MemoryStream())
            {
                using (var xamlStreamWriter = new StreamWriter(xamlMemoryStream))
                {
                    xamlStreamWriter.Write(xamlText);
                    xamlStreamWriter.Flush();
                    xamlMemoryStream.Seek(0, SeekOrigin.Begin);

                    // Load the MemoryStream into TextRange ranging from start to end of RichTextBox.
                    textRange.Load(xamlMemoryStream, DataFormats.Xaml);
                }
            }

            using (var rtfMemoryStream = new MemoryStream())
            {
                textRange = new System.Windows.Documents.TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                textRange.Save(rtfMemoryStream, DataFormats.Rtf);
                rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                using (var rtfStreamReader = new StreamReader(rtfMemoryStream))
                {
                    return rtfStreamReader.ReadToEnd();
                }
            }
        }

        private static string CreateHtmlDataFormatFromMatrix(MatrixGrid.IMatrix matrix, string style = null)
        {
            // Структура Html заголовка
            //      Version:1.0
            //      StartHTML:000000000
            //      EndHTML:000000000
            //      StartFragment:000000000
            //      EndFragment:000000000
            //      StartSelection:000000000
            //      EndSelection:000000000
            const string HtmlHeader = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\n";
            const string HtmlStartFragmentComment = @"<!--StartFragment-->";
            const string HtmlEndFragmentComment = @"<!--EndFragment-->";

            // каждое из шести чисел заголовка представлено как "{0:D10}" в строке формата
            // т.е. число представляется 10-ю разрядами ("0123456789")
            int startHTML = HtmlHeader.Length + (4 * ("0123456789".Length - "{0:D10}".Length));

            StringBuilder sbHtml = new StringBuilder();
            sbHtml.AppendLine(@"<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40"">");
            sbHtml.AppendLine("<head>");
            sbHtml.AppendLine(@"<meta http-equiv=Content-Type content=""text/html; charset=utf-8"" >");
            sbHtml.AppendLine("<title></title>");
            sbHtml.AppendFormat("<style type=\"text/css\">\r\n{0}\r\n</style>\r\n",
                style
                ??
@"table {mso-displayed-decimal-separator:""\,"";mso-displayed-thousand-separator:"" ""; width:100%;}
br {mso-data-placement:same-cell;}
th {font-size:11.0pt; font-family:Calibri, sans-serif; border: 1.0pt solid windowtext; mso-protection:locked visible;}
td {font-size:11.0pt; font-family:Calibri, sans-serif; border: 0.5pt solid windowtext; mso-number-format:General; mso-protection:locked visible; }");
            sbHtml.AppendLine("</head>");
            sbHtml.AppendLine("<body>");
            sbHtml.AppendLine(HtmlStartFragmentComment);

            int fragmentStart = startHTML + GetByteCount(sbHtml);

            string htmlTable = MatrixToHtmlTable(matrix) ?? string.Empty;

            // re-encode the string so it will work  correctly (fixed in CLR 4.0)
            if (Environment.Version.Major < 4 && htmlTable.Length != Encoding.UTF8.GetByteCount(htmlTable))
            {
                htmlTable = Encoding.Default.GetString(Encoding.UTF8.GetBytes(htmlTable));
            }

            sbHtml.AppendLine(htmlTable);

            int fragmentEnd = startHTML + GetByteCount(sbHtml);

            sbHtml.AppendLine(HtmlEndFragmentComment);
            sbHtml.Append("\r\n</body>\r\n</html>");

            int endHTML = startHTML + GetByteCount(sbHtml);

            sbHtml.Insert(0, string.Format(HtmlHeader, startHTML, endHTML, fragmentStart, fragmentEnd));

            string result = sbHtml.ToString();

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(result);

            // result = System.Text.Encoding.UTF8.GetString(bytes);
            var b = bytes.Length == System.Text.Encoding.UTF8.GetByteCount(result);

            return result;
        }

        /// <summary>
        /// Used to calculate characters byte count  in UTF-8
        /// </summary>
        private static readonly char[] byteCount = new char[1];

        /// <summary>
        /// Calculates the number of bytes produced  by encoding the string in the string builder in UTF-8 and not .NET default string encoding.
        /// </summary>
        /// <param name="sb">the  string builder to count its string</param>
        /// <param  name="start">optional: the start index to calculate from (default  - start of string)</param>
        /// <param  name="end">optional: the end index to calculate to (default - end  of string)</param>
        /// <returns>the number of bytes  required to encode the string in UTF-8</returns>
        private static int GetByteCount(StringBuilder sb, int start = 0, int end = -1)
        {
            int count = 0;
            end = end > -1 ? end : sb.Length;
            for (int i = start; i < end; i++)
            {
                byteCount[0] = sb[i];
                count += Encoding.UTF8.GetByteCount(byteCount);
            }

            return count;
        }
    }
}
