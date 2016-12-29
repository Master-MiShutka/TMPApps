using System;
using System.Collections.Generic;
using System.Linq;

namespace TMP.Work.Emcos.Export
{
    using OfficeOpenXml;
    using TMP.Work.Emcos.Model.Balans;

    public class FiderAnaliz : BaseExport
    {
        private OfficeOpenXml.Style.ExcelStyle tableStyle;

        public FiderAnaliz(ExportInfo info) : base(info)
        {
            tableStyle = CreateStyle("style_table");
            tableStyle.Border.Left.Style =
                tableStyle.Border.Top.Style =
                tableStyle.Border.Right.Style =
                tableStyle.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            tableStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            tableStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            tableStyle.WrapText = true;
            tableStyle.Font.Bold = true;
        }

        public override void CreateHeader()
        {
            ;
        }

        private int _columnsCount = 4;

        private void CreateSheetHeader(string departament)
        {
            var s = CreateRange(1, 1, 1, _columnsCount, exportInfo.Title + "\r\n" + departament, "style_title").Style;
            s.WrapText = true;
            s.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            CreateCell(2, 1, "месяц");
            CreateCell(2, 2, exportInfo.StartDate.ToString("yyyy.MM"))
                .Style.Numberformat.Format = "@";
        }

        public override void CreateBody()
        {
            book.Worksheets.Delete(sheet);

            foreach (var grouping in exportInfo.Substations.GroupBy(x => x.Departament))
            {
                IList<Substation> list = grouping.Cast<Substation>()
                    .OrderBy(x => x.Voltage)
                    .ToList<Substation>();
                if (list == null)
                    continue;
                var departament = list.FirstOrDefault().Departament;
                sheet = book.Worksheets.Add(departament);
                CreateSheetHeader(departament);

                int count = list.Count();

                int startRowIndex = 3;
                int rowIndex = startRowIndex;

                for (int index = 0; index < count; index++)
                {
                    var substation = list[index];

                    IList<IBalansItem> sections = substation.Children.Where((c) => c.Type == Model.ElementTypes.SECTION && (c as SubstationSection).IsLowVoltage).ToList();
                    if (sections == null)
                        continue;
                    foreach (IBalansItem section in sections)
                    {
                        var bss = section as SubstationSection;
                        if (bss == null)
                            continue;
                        if (bss.Children != null)
                            foreach (IBalansItem item in bss.Children)
                                if (item is Fider)
                                {
                                    var name = item.Title;
                                    int commaPos = item.Title.IndexOf(',');
                                    if (commaPos > 0)
                                        name = "яч." + name.Substring(commaPos + 1, name.Length - commaPos - 1);
                                    CreateCell(rowIndex, 1, name);
                                    CreateCell(rowIndex, 2, item.EnergyOut);
                                    CreateCell(rowIndex, 4, 0);
                                    rowIndex++;
                                }
                    }
                }
                rowIndex--;
                ChangeRangeStyle(startRowIndex, 1, rowIndex - startRowIndex + 1, 4, "style_table");

                // be-BY 423
                // ru-RU 419
                Range(startRowIndex, 2, rowIndex - startRowIndex, 1).Style.Numberformat.Format = @"[$-419]#,##0.00_ ;[Red]\-#,##0.00, ";

                SetColumnWidthAndRowHeight();

                sheet.View.ShowGridLines = false;
                sheet.PrinterSettings.PrintArea = Range(1, 1, rowIndex, 4);
                ChangePageSettings();
            }
        }

        private void SetColumnWidthAndRowHeight()
        {
            sheet.Column(1).Width = 35;
            sheet.Column(2).Width = 20;
            sheet.Column(3).Width = 10;
            sheet.Column(4).Width = 10;

            sheet.Row(1).Height = 28;
            sheet.Row(1).CustomHeight = true;
        }

        public override void CreateFooter()
        {
            book.Properties.Title = exportInfo.Title;
            package.Workbook.Properties.Company = "филиал РУП 'Гродноэнерго' Ошмянские электрические сети";

            // set some custom property values
            package.Workbook.Properties.SetCustomPropertyValue("Разработано и проверено", "Трус Михаил Петрович");
            package.Workbook.Properties.SetCustomPropertyValue("AssemblyName", "EmcosSiteWrapper");
        }

        public override void ChangePageSettings()
        {
            // верхний колонтитул
            sheet.HeaderFooter.differentOddEven = false;
            sheet.HeaderFooter.OddHeader.LeftAlignedText = exportInfo.Title;
            sheet.HeaderFooter.OddHeader.RightAlignedText = string.Format("Экспортировано: {0}", DateTime.Now);
            // нижний колонтитул
            sheet.HeaderFooter.OddHeader.RightAlignedText = String.Format("Стр. {0} из {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);

            sheet.PrinterSettings.PaperSize = ePaperSize.A4;
            // установка параметров страницы
            sheet.PrinterSettings.Orientation = eOrientation.Portrait;
            sheet.PrinterSettings.HorizontalCentered = true;
            sheet.PrinterSettings.LeftMargin = 1.2m / 2.54m;
            sheet.PrinterSettings.TopMargin = 2.0m / 2.54m;
            sheet.PrinterSettings.RightMargin = 1.2m / 2.54m;
            sheet.PrinterSettings.BottomMargin = 1.2m / 2.54m;

            sheet.View.ZoomScale = 150;
            //sheet.PrinterSettings.FitToPage = true;
            sheet.PrinterSettings.FitToWidth = 1;
        }
    }
}