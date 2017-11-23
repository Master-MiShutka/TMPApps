using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;
using TMP.WORK.AramisChetchiki.Model;
using System.Linq;
using System.Windows;
using System.Data;
using System;

namespace TMP.WORK.AramisChetchiki.Extensions
{
    public static class ICollectionViewExtension
    {

        public static void Export<T>(this ICollectionView collectionView, 
            Dictionary<string, string> fieldsAndFormats,
            string reportTitle, 
            Func<T, string, string, object> getValueDelegate)
        {
            IEnumerable<T> collection = null;
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                collection = collectionView.Cast<T>();
            }));
            int numberOfRows = collection.Count();
            // +1 т.к. первый столбец номер по порядку

            int numberOfColumns = fieldsAndFormats.Count + 1;
            // +1 т.к. первая строка шапка
            object[,] output = new object[numberOfRows + 1, numberOfColumns];

            output[0, 0] = "№ п/п";
            int ind = 1;
            foreach (var field in fieldsAndFormats)
                output[0, ind++] = field.Key;

            int rowIndex = 1;
            foreach (T item in collection)
            {
                output[rowIndex, 0] = rowIndex;
                ind = 1; // т.к. первый столбец номер по порядку
                foreach (string field in fieldsAndFormats.Keys)
                {
                    object value = getValueDelegate(item, fieldsAndFormats[field], field);
                    output[rowIndex, ind++] = value;
                }
                rowIndex++;
            }

            string fileName = System.IO.Path.GetTempFileName();
            fileName = System.IO.Path.ChangeExtension(fileName, "xls");

            Excel.Application excelApplication = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;
            try
            {
                excelApplication = new Excel.Application();
                excelApplication.DisplayAlerts = false;
                excelApplication.ScreenUpdating = false;
                Excel.Tools.CommonUtils utils = new Excel.Tools.CommonUtils(excelApplication);

                xlWorkbook = excelApplication.Workbooks.Add();
                xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];

                Excel.Range header = xlWorksheet.Range("A1");
                header.Value = reportTitle;
                header.Resize(1, numberOfColumns).Merge();
                using (Excel.Font font = header.Font)
                {
                    font.Size = 14;
                    font.Bold = true;
                }
                header.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                Excel.Range data = xlWorksheet.Range("A2").Resize(numberOfRows + 1, numberOfColumns);
                data.NumberFormat = "@";

                data.Value = output;

                xlWorksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, data,
                    Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name = "DataTable";
                xlWorksheet.ListObjects["DataTable"].TableStyle = "TableStyleMedium1";


                var ps = xlWorksheet.PageSetup;
                ps.PaperSize = XlPaperSize.xlPaperA4;
                ps.Orientation = XlPageOrientation.xlLandscape;
                ps.Zoom = false;
                ps.FitToPagesWide = 1;
                ps.FitToPagesTall = false;

                ps.LeftMargin = excelApplication.CentimetersToPoints(1.0);
                ps.RightMargin = excelApplication.CentimetersToPoints(1.0);
                ps.TopMargin = excelApplication.CentimetersToPoints(2.0);
                ps.BottomMargin = excelApplication.CentimetersToPoints(1.0);

                ps.HeaderMargin = excelApplication.CentimetersToPoints(0.6);
                ps.FooterMargin = excelApplication.CentimetersToPoints(0.6);

                ps.CenterHorizontally = true;
                ps.RightHeader = DateTime.Now.ToLongDateString();
                ps.CenterFooter = "Страница &P / &N";
                ps.PrintArea = header.Resize(numberOfRows + 2, numberOfColumns).Address;

                xlWorkbook.SaveAs(fileName);
                xlWorkbook.Close(false);

                excelApplication.ScreenUpdating = true;
                excelApplication.Quit();

                System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception e)
            {
#if DEBUG
                App.ToDebug(e);
#endif
                MessageBox.Show("Произошла ошибка:\n" + App.GetExceptionDetails(e), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                xlWorkbook.Dispose();
                xlWorksheet.Dispose();
                excelApplication.Dispose();
            }
        }

        public static DataTable ToDataTable<T>(this ICollectionView collectionView,
            Dictionary<string, string> fieldsAndFormats,
            Func<T, string, string, object> getValueDelegate)
        {
            DataTable table = new DataTable();

            foreach (var column in fieldsAndFormats)
                table.Columns.Add(column.Key);

            foreach (T item in collectionView)
            {
                DataRow row = table.NewRow();
                int index = 0;
                foreach (string field in fieldsAndFormats.Keys)
                {
                    object value = getValueDelegate(item, fieldsAndFormats[field], field);
                    row[index++] = value;
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
