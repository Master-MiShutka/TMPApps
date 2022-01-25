namespace TMP.WORK.AramisChetchiki.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using NetOffice.ExcelApi.Enums;
    using Excel = NetOffice.ExcelApi;

    public static class ICollectionViewExtension
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Export<T>(
            this ICollectionView collectionView,
            Dictionary<string, Model.DataCellFormats> fieldsAndFormats,
            string reportTitle,
            string reportDescription,
            Func<T, string, string, object> getValueDelegate, Action<string> callBack = null)
        {
            if (getValueDelegate == null || fieldsAndFormats == null || reportTitle == null)
            {
                return;
            }

            IEnumerable<T> collection = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                collection = collectionView.Cast<T>();
            });
            int numberOfRows = collection.Count();

            // +1 т.к. первый столбец номер по порядку
            int numberOfColumns = fieldsAndFormats.Count + 1;

            // +1 т.к. первая строка шапка
            object[,] output = new object[numberOfRows + 1, numberOfColumns];

            // for (int i = 0; i < numberOfRows + 1; i++)
            //    output[i] = new object[numberOfColumns];
            callBack?.Invoke("чтение данных");

            output[0, 0] = "№ п/п";
            int ind = 1;
            foreach (KeyValuePair<string, Model.DataCellFormats> field in fieldsAndFormats)
            {
                output[0, ind++] = Utils.ConvertFromTitleCase(field.Key);
            }

            callBack?.Invoke("заполнение таблицы");

            int rowIndex = 1;
            foreach (T item in collection)
            {
                output[rowIndex, 0] = rowIndex;
                ind = 1; // т.к. первый столбец номер по порядку
                foreach (string field in fieldsAndFormats.Keys)
                {
                    object value = string.Empty;
                    if (string.IsNullOrWhiteSpace(fieldsAndFormats[field].ExcelFormat) == false)
                    {
                        value = getValueDelegate(item, string.Empty, field);
                    }
                    else
                    {
                        value = getValueDelegate(item, fieldsAndFormats[field].ContentDisplayFormat, field);
                    }

                    output[rowIndex, ind++] = value;
                }

                rowIndex++;
            }

            callBack?.Invoke("поиск MS Excel");

            string fileName = System.IO.Path.GetTempFileName();
            fileName = System.IO.Path.ChangeExtension(fileName, "xlsx");

            var defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            // HACK: Workaround for Excel bug on machines which are set up in the English language, but not an English region.
            var enusCultureInfo = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = enusCultureInfo;

            Excel.Application excelApplication = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;

            Exception exception = null;

            try
            {
                callBack?.Invoke("создание книги MS Excel");

                excelApplication = new Excel.Application();
                excelApplication.DisplayAlerts = false;
                excelApplication.ScreenUpdating = false;

                xlWorkbook = excelApplication.Workbooks.Add();
                xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];

                Excel.Range header = xlWorksheet.Range("A1");
                header.WrapText = true;
                header.Resize(1, numberOfColumns).Merge();
                using (Excel.Font font = header.Font)
                {
                    font.Size = 14;
                    font.Bold = true;
                }

                header.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                header.VerticalAlignment = XlVAlign.xlVAlignCenter;

                double oneRowHeight = (double)header.RowHeight;
                int rowsCount = reportTitle.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;
                header.RowHeight = oneRowHeight * rowsCount * 1.1;

                header.Value2 = reportTitle;

                Excel.Range description = xlWorksheet.Range("A2");
                description.Resize(1, numberOfColumns).Merge();
                description.WrapText = true;
                using (Excel.Font font = description.Font)
                {
                    font.Size = 12;
                    font.Italic = true;
                }

                description.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                description.VerticalAlignment = XlVAlign.xlVAlignCenter;

                oneRowHeight = (double)description.RowHeight;
                rowsCount = reportDescription.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;
                description.RowHeight = oneRowHeight * rowsCount * 1.1;

                description.Value2 = reportDescription;

                Excel.Range data = xlWorksheet.Range("A3").Resize(numberOfRows + 1, numberOfColumns);
                data.NumberFormat = "@";

                data.Value = output;

                callBack?.Invoke("установка формата данных");

                rowIndex = 1;
                foreach (T item in collection)
                {
                    data[rowIndex, 0 + 1].NumberFormat = "0";
                    ind = 1; // т.к. первый столбец номер по порядку
                    foreach (string field in fieldsAndFormats.Keys)
                    {
                        object value = string.Empty;
                        if (string.IsNullOrWhiteSpace(fieldsAndFormats[field].ExcelFormat) == false)
                        {
                            data[rowIndex, 0 + 1 + ind].NumberFormat = fieldsAndFormats[field].ExcelFormat;
                        }
                        else if (string.IsNullOrWhiteSpace(fieldsAndFormats[field].ContentDisplayFormat) == false)
                        {
                            data[rowIndex, 0 + 1 + ind].NumberFormat = "General";
                        }

                        ind++;
                    }

                    rowIndex++;
                }

                callBack?.Invoke("настройка книги MS Excel");

                xlWorksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, data,
                    Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name = "DataTable";
                xlWorksheet.ListObjects["DataTable"].TableStyle = "TableStyleMedium6";

                var ps = xlWorksheet.PageSetup;
                ps.PaperSize = XlPaperSize.xlPaperA4;
                ps.Orientation = XlPageOrientation.xlLandscape;
                ps.Zoom = false;
                ps.FitToPagesWide = 1;
                ps.FitToPagesTall = false;

                ps.PrintTitleRows = "$3:$3";

                ps.LeftMargin = excelApplication.CentimetersToPoints(1.0);
                ps.RightMargin = excelApplication.CentimetersToPoints(1.0);
                ps.TopMargin = excelApplication.CentimetersToPoints(2.0);
                ps.BottomMargin = excelApplication.CentimetersToPoints(1.0);

                ps.HeaderMargin = excelApplication.CentimetersToPoints(0.6);
                ps.FooterMargin = excelApplication.CentimetersToPoints(0.6);

                ps.CenterHorizontally = true;
                ps.RightHeader = DateTime.Now.ToLongDateString();
                ps.CenterFooter = "Страница &P / &N";
                ps.PrintArea = header.Resize(numberOfRows + 3, numberOfColumns).Address;

                foreach (var i in Enumerable.Range(1, numberOfColumns))
                {
                    xlWorksheet.Columns[i].AutoFit();
                }

                callBack?.Invoke("сохранение книги MS Excel");

                xlWorkbook.SaveAs(fileName);
                xlWorkbook.Close(false);

                callBack?.Invoke("завершение");

                logger?.Info($"Export >> файл сформирован и сохранен: '{fileName}'");
            }
            catch (Exception e)
            {
                exception = e;
#if DEBUG
                App.ToDebug(e);
#endif
                App.ShowError("Произошла ошибка:\n" + App.GetExceptionDetails(e));
                return;
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = defaultCulture;

                excelApplication.Quit();
                excelApplication.ScreenUpdating = true;
                excelApplication.DisplayAlerts = true;
                if (exception != null)
                {
                    if (excelApplication.Workbooks.Any())
                    {
                        foreach (var workbook in excelApplication.Workbooks.Where(x => !x.IsDisposed))
                        {
                            workbook.Close(false, System.Reflection.Missing.Value, Missing.Value);
                            workbook.Dispose();
                        }
                    }

                    if (excelApplication.IsDisposed == false)
                    {
                        excelApplication.Quit();
                        excelApplication.Dispose();
                    }
                }
            }

            try
            {
                callBack?.Invoke("открытие созданной книги MS Excel");

                using var p = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(fileName)
                    {
                        UseShellExecute = true,
                    },
                };
                p.Start();

                // System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception e)
            {
#if DEBUG
                App.ToDebug(e);
#endif
                App.ShowError("Произошла ошибка при открытии файла:\n" + App.GetExceptionDetails(e));
            }
        }

        public static DataTable ToDataTable<T>(
            this ICollectionView collectionView,
            Dictionary<string, Model.DataCellFormats> fieldsAndFormats,
            Func<T, string, string, object> getValueDelegate)
        {
            if (collectionView == null || getValueDelegate == null || fieldsAndFormats == null)
            {
                return null;
            }

            DataTable table = new ();

            foreach (var column in fieldsAndFormats)
            {
                table.Columns.Add(column.Key);
            }

            foreach (T item in collectionView)
            {
                DataRow row = table.NewRow();
                int index = 0;
                foreach (string field in fieldsAndFormats.Keys)
                {
                    object value = getValueDelegate(item, fieldsAndFormats[field].ContentDisplayFormat, field);
                    row[index++] = value;
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
