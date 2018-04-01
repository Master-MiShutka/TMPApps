using System;
using System.Collections.Generic;
using System.Text;

namespace TMP.Work.Emcos.Export
{
    using OfficeOpenXml;
    public abstract class BaseExport : IDisposable
    {
        internal ExcelPackage package;
        internal ExcelWorkbook book;
        internal ExcelWorksheet sheet;

        internal ExportInfo exportInfo;
        public BaseExport()
        {
            Init();
        }
        public BaseExport(ExportInfo info) : this()
        {
            exportInfo = info;            
        }
        ~BaseExport()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (sheet != null)
                sheet.Dispose();
            if (book != null)
                book.Dispose();
            if (package != null)
                package.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        internal void Init()
        {
            // создание экземпляра
            package = new ExcelPackage();
            book = package.Workbook;

            // добавление свойств
            book.Properties.Author = "MiShutka";

            // получение первого рабочего листа
            sheet = book.Worksheets.Count == 0 ? book.Worksheets.Add("Лист 1") : book.Worksheets[0];
        }
        public virtual bool Export(string outputFile)
        {
            CreateHeader();
            CreateBody();
            CreateFooter();
            ChangePageSettings();
            try
            {
                package.SaveAs(new System.IO.FileInfo(outputFile));
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ошибка экспорта данных.\n{0}", e.Message), 
                    "Экспорт", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                App.LogException(e);
                return false;
            }
        }
        public abstract void CreateHeader();

        public abstract void CreateBody();

        public abstract void CreateFooter();
        public abstract void ChangePageSettings();

        public OfficeOpenXml.Style.ExcelStyle CreateStyle(string styleName)
        {
            var style = book.Styles.CreateNamedStyle(styleName, book.Styles.NamedStyles[0].Style);
            var result = style.Style;
            return result;
        }
        public OfficeOpenXml.Style.ExcelStyle CreateStyle(string styleName, OfficeOpenXml.Style.ExcelStyle template)
        {
            var style = book.Styles.CreateNamedStyle(styleName, template);
            var result = style.Style;
            return result;
        }

        public OfficeOpenXml.Style.ExcelStyle GetCellStyle(int row, int column)
        {
            return sheet.Cells[row, column].Style;
        }

        internal ExcelRange CreateCell(int row, int column, object value)
        {
            var range = sheet.Cells[row, column];
            range.Value = value;
            return range;
        }
        internal ExcelRange CreateCell(int row, int column, object value, string styleName)
        {
            var range = sheet.Cells[row, column];
            range.Value = value;
            range.StyleName = styleName;
            return range;
        }
        internal ExcelRange CreateRange(int row, int column, int rows, int cols, object value)
        {
            sheet.Cells[row, column].Value = value;
            var range = sheet.Cells[row, column, (row + rows - 1), (column + cols - 1)];
            range.Merge = true;
            return range;
        }
        internal ExcelRange CreateRange(int row, int column, int rows, int cols, object value, string styleName)
        {
            sheet.Cells[row, column].Value = value;
            var range = sheet.Cells[row, column, (row + rows - 1), (column + cols - 1)];
            range.Merge = true;
            range.StyleName = styleName;
            return range;
        }

        internal ExcelRange Range(int row, int column)
        {
            return sheet.Cells[row, column];
        }
        internal ExcelRange Range(int row, int column, int rowSpan, int colSpan)
        {
            return sheet.Cells[row, column, row + rowSpan - 1, column + colSpan - 1];
        }

        internal void ChangeRangeStyle(int row_start, int column_start, int rows, int columns, string styleName)
        {
            sheet.Cells[row_start, column_start, row_start + rows - 1, column_start + columns - 1].StyleName = styleName;
        }

        internal void MakeCellAlignment(int row, int column, OfficeOpenXml.Style.ExcelHorizontalAlignment horizontal, OfficeOpenXml.Style.ExcelVerticalAlignment vertical)
        {
            sheet.Cells[row, column].Style.HorizontalAlignment = horizontal;
            sheet.Cells[row, column].Style.VerticalAlignment = vertical;
        }
    }
}
