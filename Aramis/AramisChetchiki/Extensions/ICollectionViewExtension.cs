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

            const string commentFieldName = nameof(Model.Meter.Коментарий);
            bool hasCommentColumn = fieldsAndFormats.ContainsKey(commentFieldName);

            IEnumerable<T> collection = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                collection = collectionView.Cast<T>();
            });
            int numberOfRows = collection.Count();

            // +1 т.к. первый столбец номер по порядку
            int numberOfColumns = fieldsAndFormats.Count + 1;

            callBack?.Invoke("чтение данных");
            object[,] output = BuildDataArray();
            object[,] outputWithTwoRowPerRecord = null;
            if (hasCommentColumn)
            {
                outputWithTwoRowPerRecord = BuildDataArrayWithTwoRowPerRecord();
            }

            callBack?.Invoke("поиск MS Excel");

            string fileName = System.IO.Path.GetTempFileName();
            fileName = System.IO.Path.ChangeExtension(fileName, "xlsx");

            System.Globalization.CultureInfo defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            // HACK: Workaround for Excel bug on machines which are set up in the English language, but not an English region.
            System.Globalization.CultureInfo enusCultureInfo = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = enusCultureInfo;

            Excel.Application excelApplication = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;
            Excel.Worksheet xlWorksheet2 = null;

            Exception exception = null;

            Process(output, outputWithTwoRowPerRecord);

            OpenCreatedWorkBook();

            // возвращает значение указанного поля записи
            object GetValueOfField(T item, string fieldName)
            {
                object value = string.Empty;
                if (string.IsNullOrWhiteSpace(fieldsAndFormats[fieldName].ExcelFormat) == false)
                {
                    value = getValueDelegate(item, string.Empty, fieldName);
                }
                else
                {
                    value = getValueDelegate(item, fieldsAndFormats[fieldName].ContentDisplayFormat, fieldName);
                }

                if (value is DateOnly dateOnlyValue)
                {
                    value = dateOnlyValue.ToDateTime(TimeOnly.MinValue);
                }

                return value;
            }

            // создание массива данных
            object[,] BuildDataArray()
            {
                // +1 т.к. первый столбец номер по порядку
                int countOfColumns = fieldsAndFormats.Count + 1;

                // +1 т.к. первая строка шапка
                object[,] output = new object[numberOfRows + 1, countOfColumns];

                // for (int i = 0; i < numberOfRows + 1; i++)
                //    output[i] = new object[numberOfColumns];

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
                        output[rowIndex, ind++] = GetValueOfField(item, field);
                    }

                    rowIndex++;
                }

                return output;
            }

            // создание массива данных со второй строкой в каждой записи где указан комментарий
            object[,] BuildDataArrayWithTwoRowPerRecord()
            {
                // -1 т.к. исключен столбец Комментарий
                // +1 т.к. первый столбец номер по порядку
                int countOfColumns = fieldsAndFormats.Count - 1 + 1;

                int numberOfRecords = collection.Count();

                // +1 т.к. первая строка шапка
                object[,] output = new object[(2 * numberOfRecords) + 1, countOfColumns];

                // for (int i = 0; i < numberOfRows + 1; i++)
                //    output[i] = new object[numberOfColumns];

                output[0, 0] = "№ п/п";
                int ind = 1;
                foreach (KeyValuePair<string, Model.DataCellFormats> field in fieldsAndFormats)
                {
                    if (field.Key == commentFieldName)
                        continue;
                    else
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
                        if (field == commentFieldName)
                            continue;

                        output[rowIndex, ind++] = GetValueOfField(item, field);

                        string commentValue = GetValueOfField(item, commentFieldName).ToString().Trim().Replace('\n', '\t');
                        output[rowIndex + 1, 1] = commentValue;
                    }

                    rowIndex += 2;
                }

                return output;
            }

            void ApplyDataFormatForSheet(Excel.Worksheet xlWorksheet, Excel.Range rangeToSetData)
            {
                // -1 т.к. исключен столбец Комментарий
                // +1 т.к. первый столбец номер по порядку
                int countOfColumns = hasCommentColumn ? fieldsAndFormats.Count - 1 + 1 : fieldsAndFormats.Count + 1;

                callBack?.Invoke("установка формата данных");

                int rowIndex = 1;
                int ind = 1;
                foreach (T item in collection)
                {
                    rangeToSetData[rowIndex, 0 + 1].NumberFormat = "0";
                    ind = 1; // т.к. первый столбец номер по порядку
                    foreach (string field in fieldsAndFormats.Keys)
                    {
                        if (field == commentFieldName)
                            continue;

                        try
                        {
                            object value = string.Empty;
                            if (string.IsNullOrWhiteSpace(fieldsAndFormats[field].ExcelFormat) == false)
                            {
                                string format = string.IsNullOrWhiteSpace(fieldsAndFormats[field].ExcelFormat) ? "General" : fieldsAndFormats[field].ExcelFormat;
                                rangeToSetData[rowIndex, 0 + 1 + ind].NumberFormat = format;
                            }
                            else if (string.IsNullOrWhiteSpace(fieldsAndFormats[field].ContentDisplayFormat) == false)
                            {
                                rangeToSetData[rowIndex, 0 + 1 + ind].NumberFormat = "General";
                            }
                        }
                        catch (Exception e)
                        {
#if DEBUG
                            App.ToDebug(e);
#endif
                        }
                        finally
                        {
                            ind++;
                        }
                    }

                    if (hasCommentColumn)
                    {
                        Excel.Range rng = rangeToSetData[rowIndex + 1, 1].Resize(1, countOfColumns);
                        rng.Merge();
                        rng.WrapText = true;
                        rng.NumberFormat = "@";

                        rowIndex += 2;
                    }
                    else
                    {
                        rowIndex++;
                    }
                }
            }

            void Process(object[,] outputData1, object[,] outputData2)
            {
                try
                {
                    callBack?.Invoke("создание книги MS Excel");

                    excelApplication = new Excel.Application
                    {
                        DisplayAlerts = false,
                        ScreenUpdating = false,
                    };

                    xlWorkbook = excelApplication.Workbooks.Add();
                    xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];
                    xlWorksheet.Name = "Данные";

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

                    data.Value = outputData1;

                    ApplyDataFormatForSheet(xlWorksheet, data);

                    callBack?.Invoke("настройка книги MS Excel");

                    if (hasCommentColumn)
                    {
                        // создание копии листа
                        xlWorksheet.Copy(xlWorksheet);
                        xlWorksheet2 = (Excel.Worksheet)xlWorkbook.Sheets[2];
                        xlWorksheet2.Name = "Для печати";
                    }

                    xlWorksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, data,
                        Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name = "DataTable";
                    xlWorksheet.ListObjects["DataTable"].TableStyle = "TableStyleMedium6";

                    SetupWorkSheet(xlWorksheet, numberOfColumns);

                    if (hasCommentColumn)
                    {
                        SetupWorkSheet(xlWorksheet2, numberOfColumns - 1);
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
                            foreach (Excel.Workbook workbook in excelApplication.Workbooks.Where(x => !x.IsDisposed))
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
            }

            void SetupWorkSheet(Excel.Worksheet xlWorksheet, int numberOfColumns)
            {
                foreach (int i in Enumerable.Range(1, numberOfColumns))
                {
                    xlWorksheet.Columns[i].AutoFit();
                }

                Excel.PageSetup ps = xlWorksheet.PageSetup;
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
                ps.PrintArea = xlWorksheet.Range("A1").Resize(numberOfRows + 3, numberOfColumns).Address;
            }

            void OpenCreatedWorkBook()
            {
                try
                {
                    callBack?.Invoke("открытие созданной книги MS Excel");

                    using System.Diagnostics.Process p = new System.Diagnostics.Process
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

            DataTable table = new();

            foreach (KeyValuePair<string, Model.DataCellFormats> column in fieldsAndFormats)
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
