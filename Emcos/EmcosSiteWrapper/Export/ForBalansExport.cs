using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Export
{
    using OfficeOpenXml;
    using TMP.Work.Emcos.Model.Balans;
    public class ForBalansExport : BaseExport
    {
        private OfficeOpenXml.Style.ExcelStyle tableStyle;
        public ForBalansExport(ExportInfo info) : base(info)
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
        private void CreateSheetHeader(string departament)
        {
            CreateCell(1, 1, "#");
            CreateCell(1, 2, "POINT_ID");
            CreateCell(1, 3, "ML_ID");
            CreateCell(1, 4, "VAL");
            CreateCell(1, 5, "DA");

            var s = CreateCell(1, 8, "Внимание! Файл сохранить под именем '1.xls' в папку с программой 'Balans' (тип файла 'Книга Excel 97-2003').").Style;
            s.Font.Bold = true;
            s.Font.Color.SetColor(System.Drawing.Color.Red);

        }

        private void CreateBodyForDepartament(string departament, IList<Substation> list)
        {
            sheet = book.Worksheets.Add(departament);
            CreateSheetHeader(departament);

            int count = list.Count();

            int startRowIndex = 2;
            int rowIndex = startRowIndex;
            int orderIndex = 1;

            for (int index = 0; index < count; index++)
            {
                var substation = list[index];
                if (substation == null || substation.Children == null)
                    continue;
                IList<IBalansItem> sections = substation.Children.Where((c) => c.ElementType == Model.ElementTypes.SECTION && (c as SubstationSection).IsLowVoltage).ToList();
                foreach (IBalansItem section in sections)
                {
                    var bss = section as SubstationSection;
                    if (bss == null)
                        continue;
                    if (bss.Children != null)
                        foreach (IBalansItem item in bss.Children)
                            if (item is PowerTransformer || item is Fider || item is UnitTransformerBus)
                            {
                                // А+ за месяц
                                CreateCell(rowIndex, 1, orderIndex++);
                                CreateCell(rowIndex, 2, Convert.ToInt32(item.Id));
                                CreateCell(rowIndex, 3, 381);
                                CreateCell(rowIndex, 4, item.EnergyIn);
                                CreateCell(rowIndex, 5, exportInfo.StartDate.ToString("dd.MM.yyyy"));
                                CreateCell(rowIndex, 7, item.Code);
                                rowIndex++;
                                // А- за месяц
                                CreateCell(rowIndex, 1, orderIndex++);
                                CreateCell(rowIndex, 2, Convert.ToInt32(item.Id));
                                CreateCell(rowIndex, 3, 382);
                                CreateCell(rowIndex, 4, item.EnergyOut);
                                CreateCell(rowIndex, 5, exportInfo.StartDate.ToString("dd.MM.yyyy"));
                                rowIndex++;
                            }
                }
                // собственные нуды
                IList<IBalansItem> sectionAux = substation.Children.Where((c) => c.ElementType == Model.ElementTypes.AUXILIARY && c.Name == "Собственные нужды").ToList();
                foreach (IBalansGroup aux in sectionAux)
                {
                    if (aux == null || aux.Children == null)
                        continue;
                    foreach (IBalansItem auxChild in aux.Children)
                    {
                        IBalansItem unit = null;
                        if (auxChild is UnitTransformer)
                            unit = auxChild as UnitTransformer;
                        if (unit == null)
                            continue;
                        // А+ за месяц
                        CreateCell(rowIndex, 1, orderIndex++);
                        CreateCell(rowIndex, 2, Convert.ToInt32(unit.Id));
                        CreateCell(rowIndex, 3, 381);
                        CreateCell(rowIndex, 4, unit.EnergyIn);
                        CreateCell(rowIndex, 5, exportInfo.StartDate.ToString("dd.MM.yyyy"));
                        CreateCell(rowIndex, 7, unit.Code);
                        rowIndex++;
                        // А- за месяц
                        CreateCell(rowIndex, 1, orderIndex++);
                        CreateCell(rowIndex, 2, Convert.ToInt32(unit.Id));
                        CreateCell(rowIndex, 3, 382);
                        CreateCell(rowIndex, 4, unit.EnergyOut);
                        CreateCell(rowIndex, 5, exportInfo.StartDate.ToString("dd.MM.yyyy"));
                        rowIndex++;
                    }
                }
            }
            rowIndex--;
            ChangeRangeStyle(1, 1, rowIndex, 5, "style_table");

            // be-BY 423
            // ru-RU 419
            Range(2, 4, rowIndex - 1, 1).Style.Numberformat.Format = @"[$-419]#,##0.00_ ;[Red]\-# ##0.00, ";

            SetColumnWidthAndRowHeight();

            sheet.View.ShowGridLines = false;
            sheet.PrinterSettings.PrintArea = Range(1, 1, rowIndex, 5);
            ChangePageSettings();
        }
        public override void CreateBody()
        {
            book.Worksheets.Delete(sheet);

            CreateBodyForDepartament("ВСЕ", exportInfo.Substations.OrderBy(s => s.Name).ToList());

            foreach (var grouping in exportInfo.Substations.GroupBy(x => x.Departament))
            {
                IList<Substation> list = grouping.Cast<Substation>()
                    .OrderBy(x => x.Name)
                    .ToList<Substation>();
                var departament = list.FirstOrDefault().Departament;
                CreateBodyForDepartament(departament, list);
            }
        }
        private void SetColumnWidthAndRowHeight()
        {
            //sheet.Cells.AutoFitColumns(0);  //Autofit columns for all cells
            sheet.Column(1).Width = 5;
            sheet.Column(2).Width = 10;
            sheet.Column(3).Width = 10;
            sheet.Column(4).Width = 15;
            sheet.Column(5).Width = 15;
            sheet.Column(6).Width = 5;
            sheet.Column(7).Width = 30;
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
            sheet.HeaderFooter.OddFooter.RightAlignedText = String.Format("Стр. {0} из {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);

            sheet.PrinterSettings.PaperSize = ePaperSize.A4;
            // установка параметров страницы
            sheet.PrinterSettings.Orientation = eOrientation.Portrait;
            sheet.PrinterSettings.HorizontalCentered = true;
            sheet.PrinterSettings.LeftMargin = 1.2m / 2.54m;
            sheet.PrinterSettings.TopMargin = 2.0m / 2.54m;
            sheet.PrinterSettings.RightMargin = 1.2m / 2.54m;
            sheet.PrinterSettings.BottomMargin = 1.2m / 2.54m;

            sheet.PrinterSettings.RepeatRows = sheet.Cells["1:1"];

            sheet.View.FreezePanes(2, 1);
            sheet.View.ZoomScale = 130;
            //sheet.PrinterSettings.FitToPage = true;
            sheet.PrinterSettings.FitToWidth = 1;
        }
    }
}
