using System;
using System.Collections.Generic;

using TMP.ExcelXml;
using Range = TMP.ExcelXml.Range;

namespace TMP.ARMTES
{
    using Model;
    public abstract class BaseSmallEngineExport : IExport
    {
        internal IEnumerable<Collector> collectors;

        internal ExcelXmlWorkbook book;
        internal Worksheet sheet;

        internal ExportInfo exportInfo;

        public BaseSmallEngineExport(ExportInfo exportInfo, IEnumerable<Collector> collectors)
        {
            if (collectors == null) throw new ArgumentNullException("Collectors is null");

            this.collectors = collectors;
            this.exportInfo = exportInfo;

            // создание экземпляра
            book = new ExcelXmlWorkbook();

            // добавление свойств
            book.Properties.Author = "MiShutka";

            // получение первого рабочего листа
            sheet = book[0];
        }
        public virtual bool Export(string outputFile)
        {

            CreateHeader();
            CreateBody();
            CreateFooter();
            ChangePageSettings();
            
            return book.Export(outputFile);
        }

        public abstract void CreateHeader();

        public abstract void CreateBody();

        public abstract void CreateFooter();
        public abstract void ChangePageSettings();

        internal void CreateCell(int row, int column, object value)
        {
            sheet[column - 1, row - 1].Value = value;
        }
        internal void CreateCell(int row, int column, object value, XmlStyle style)
        {
            sheet[column - 1, row - 1].Value = value;
            sheet[column - 1, row - 1].Style = style;
        }
        internal void CreateRange(int row, int column, object value)
        {
            sheet[column - 1, row - 1].Value = value;
            new Range(sheet[column - 1, row - 1], sheet[column - 1 , row - 1]).Merge();
        }

        internal void CreateRange(int row, int column, object value, XmlStyle style)
        {
            sheet[column - 1, row - 1].Value = value;
            Range r = new Range(sheet[column - 1, row - 1], sheet[column - 1, row - 1]);
            r.Style = style;
        }

        internal void CreateRange(int row, int column, int rowSpan, int colSpan, object value)
        {
            sheet[column - 1, row - 1].Value = value;
            Range r = new Range(sheet[column - 1, row - 1], sheet[column - 1 + colSpan - 1, row - 1 + rowSpan - 1]);
            if (rowSpan > 1 || colSpan > 1)
                r.Merge();
        }
        internal void CreateRange(int row, int column, int rowSpan, int colSpan, object value, XmlStyle style)
        {
            sheet[column - 1, row - 1].Value = value;
            Range r = new Range(sheet[column - 1, row - 1], sheet[column - 1 + colSpan - 1, row - 1 + rowSpan - 1]);
            if (rowSpan > 1 || colSpan>1) 
                r.Merge();
            r.Style = style;
        }

        internal void ChangeRangeStyle(int row_start, int column_start, int row_end, int column_end,  XmlStyle style)
        {
            new Range(sheet[column_start - 1, row_start - 1], sheet[column_end - 1, row_end - 1]).Style = style;
        }

        internal void MakeCellAlignment(int row, int column, HorizontalAlignment horizontal, VerticalAlignment vertical)
        {
            AlignmentOptionsBase ao = sheet[column - 1, row - 1].Alignment as AlignmentOptionsBase;
            ao.Horizontal = horizontal;
            ao.Vertical = vertical;
        }

        internal void MakeRangeAlignment(int row_start, int column_start, int row_end, int column_end, HorizontalAlignment horizontal, VerticalAlignment vertical)
        {
            AlignmentOptionsBase ao = new Range(sheet[column_start - 1, row_start - 1], sheet[column_end - 1, row_end - 1]).Alignment as AlignmentOptionsBase;
            ao.Horizontal = horizontal;
            ao.Vertical = vertical;
        }

        internal object GetIndication(double? value)
        {
            if (value.HasValue)
                return value.Value;
            else
                return "нет";
        }
        internal object GetIndication(long? value)
        {
            if (value.HasValue)
                return value.Value;
            else
                return "нет";
        }

    }
}
