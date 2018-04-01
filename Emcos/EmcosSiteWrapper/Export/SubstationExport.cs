using System;
using System.Collections.Generic;
using System.Linq;

namespace TMP.Work.Emcos.Export
{
    using OfficeOpenXml;
    using TMP.Work.Emcos.Model.Balans;

    using TMP.Wpf.Common.Controls.TableView;

    public class SubstationExport : BaseExport
    {
        private OfficeOpenXml.Style.ExcelStyle tableStyle;
        private OfficeOpenXml.Style.ExcelStyle tableHeaderStyle;

        private OfficeOpenXml.Style.ExcelStyle cellDateStyle;
        private OfficeOpenXml.Style.ExcelStyle cellTextStyle;
        private OfficeOpenXml.Style.ExcelStyle cellNumbersStyle;

        private Model.BalansGrop balansGroup;

        public SubstationExport(Model.BalansGrop balansGroup)
        {
            if (balansGroup == null)
                throw new ArgumentNullException("SubstationExport - constructor: argument balansGroup is null");
            this.balansGroup = balansGroup;

            tableStyle = CreateStyle("style_table");
            tableStyle.Border.Left.Style =
                tableStyle.Border.Top.Style =
                tableStyle.Border.Right.Style =
                tableStyle.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            tableStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            tableStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            tableHeaderStyle = CreateStyle("style_table_header");
            tableHeaderStyle.Border.Left.Style = 
                tableHeaderStyle.Border.Top.Style =
                tableHeaderStyle.Border.Right.Style =
                tableHeaderStyle.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            tableHeaderStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            tableHeaderStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            tableHeaderStyle.WrapText = true;
            tableHeaderStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            tableHeaderStyle.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 153));

            cellDateStyle = CreateStyle("cellDateStyle");
            cellDateStyle.Border.Left.Style =
                cellDateStyle.Border.Top.Style =
                cellDateStyle.Border.Right.Style =
                cellDateStyle.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            cellDateStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cellDateStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            cellDateStyle.Numberformat.Format = "dd.MM.yyyy";

            cellTextStyle = CreateStyle("cellTextStyle");
            cellTextStyle.Border.Left.Style =
                cellTextStyle.Border.Top.Style =
                cellTextStyle.Border.Right.Style =
                cellTextStyle.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            cellTextStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cellTextStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            cellTextStyle.Numberformat.Format = "@";

            cellNumbersStyle = CreateStyle("cellNumbersStyle");
            cellNumbersStyle.Border.Left.Style =
                cellNumbersStyle.Border.Top.Style =
                cellNumbersStyle.Border.Right.Style =
                cellNumbersStyle.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            cellNumbersStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            cellNumbersStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            cellNumbersStyle.Numberformat.Format = @"[$-419]#,#0.0_ ;[Red]\-#,#0.0, ";
        }

        public override void CreateHeader()
        {
            CreateCell(1, 1, "Экспорт из");
            CreateCell(1, 2, "Архивов");
            CreateCell(1, 3, "Баланс по ПС " + balansGroup.SubstationTitle);
            Range(1, 1, 1, 3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            CreateCell(2, 1, "Экспортировано: " + DateTime.Now);
        }
        
        public override void CreateBody()
        {
            int index = 1;
            int startRowIndex = 4;
            int rowIndex = startRowIndex;
            foreach (Model.BalansGrop.HeaderElement header in balansGroup.Headers)
            {
                CreateCell(3, index++, header.Title, "style_table_header");
            }

            foreach (Model.BalansGroupItem item in balansGroup.Items)
            {
                index = 1;
                CreateCell(rowIndex, index++, item.Date);
                CreateCell(rowIndex, index++, item.VvodaIn);
                CreateCell(rowIndex, index++, item.VvodaOut);
                if (balansGroup.AuxCount != 0)
                    CreateCell(rowIndex, index++, item.Tsn);
                CreateCell(rowIndex, index++, item.FideraIn);
                CreateCell(rowIndex, index++, item.FideraOut);
                CreateCell(rowIndex, index++, item.Unbalance);
                CreateCell(rowIndex, index++, item.PercentageOfUnbalance);
                foreach(var t in item.Transformers)
                    CreateCell(rowIndex, index++, t);
                if (balansGroup.AuxCount != 0)
                    foreach (var a in item.Auxiliary)
                        CreateCell(rowIndex, index++, a);
                foreach (var f in item.Fiders)
                    CreateCell(rowIndex, index++, f);
                // применение стиля ячеек
                ChangeRangeStyle(rowIndex, 1, 1, 1, "cellDateStyle");
                ChangeRangeStyle(rowIndex, 2, 1, index - 2, "cellNumbersStyle");
                Range(rowIndex, 7).Style.Numberformat.Format = @"[$-419]#,##0.00_ ;[Red]\-#,##0.00, ";

                rowIndex++;
            }
            // гистограммы
            index = 2;
            var groupColor = System.Drawing.ColorTranslator.FromHtml("#FF63C384");
            sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), groupColor);
            sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), groupColor);
            if (balansGroup.AuxCount != 0)
                sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), groupColor);
            sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), groupColor);
            sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), groupColor);
            var balanceColor = System.Drawing.ColorTranslator.FromHtml("#FFD6007B");
            sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), balanceColor);
            sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), balanceColor);
            var elementPlusColor = System.Drawing.ColorTranslator.FromHtml("#FFFFB628");
            var elementMinusColor = System.Drawing.ColorTranslator.FromHtml("#FF638EC6");
            for (int i = 0; i < balansGroup.TransformersCount * 2; i++)
                if (i % 2 == 0)
                    sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), elementMinusColor);
                else
                    sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), elementPlusColor);
            if (balansGroup.AuxCount != 0)
                for (int i= 0; i < balansGroup.AuxCount; i++)
                    sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), elementPlusColor);
            for (int i = 0; i < balansGroup.FidersCount * 2; i++)
                if (i % 2 == 0)
                    sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), elementPlusColor);
                else
                    sheet.ConditionalFormatting.AddDatabar(Range(startRowIndex, index++, rowIndex - startRowIndex, 1), elementMinusColor);

            #region | минимумы, максимумы, среднее и сумма |
            rowIndex++;
            startRowIndex = rowIndex;
            var firstItem = balansGroup.Items[0];

            var lists = new List<IList<double?>>();
            lists.Add(balansGroup.Min);
            lists.Add(balansGroup.Max);
            lists.Add(balansGroup.Average);
            lists.Add(balansGroup.Sum);

            CreateCell(rowIndex, 1, "Минимум");
            CreateCell(rowIndex + 1, 1, "Максимум");
            CreateCell(rowIndex + 2, 1, "Среднее");
            CreateCell(rowIndex + 3, 1, "Сумма");

            foreach (IList<double?> list in lists)
            {
                index = 2;
                CreateCell(rowIndex, index++, list[index - 2]);
                CreateCell(rowIndex, index++, list[index - 2]);
                if (balansGroup.AuxCount != 0)
                    CreateCell(rowIndex, index++, list[index - 2]);
                CreateCell(rowIndex, index++, list[index - 2]);
                CreateCell(rowIndex, index++, list[index - 2]);
                CreateCell(rowIndex, index++, list[index - 2]);
                CreateCell(rowIndex, index++, list[index - 2]);
                for (int i = 0; i < balansGroup.TransformersCount * 2; i++)
                    CreateCell(rowIndex, index++, list[index - 2]);
                if (balansGroup.AuxCount != 0)
                    for (int i = 0; i < balansGroup.AuxCount; i++)
                        CreateCell(rowIndex, index++, list[index - 2]);
                for (int i = 0; i < balansGroup.FidersCount * 2; i++)
                    CreateCell(rowIndex, index++, list[index - 2]);
                rowIndex++;
            }
            // применение стиля ячеек
            ChangeRangeStyle(startRowIndex, 1, 4, index - 1, "cellDateStyle");
            Range(startRowIndex, 2, 4, index - 1).Style.Numberformat.Format = @"[$-419]#,##0.00_ ;[Red]\-#,##0.00, ";
            #endregion

            SetColumnWidthAndRowHeight();

            sheet.View.ShowGridLines = false;
            sheet.PrinterSettings.PrintArea = Range(1, 1, rowIndex, index - 1);
            ChangePageSettings();
        }

        private void SetColumnWidthAndRowHeight()
        {
            sheet.Column(1).Width = 10;
            sheet.Column(2).Width = 12;
            sheet.Column(4).Width = 12;
            sheet.Column(5).Width = 12;
            sheet.Column(6).Width = 11;
            sheet.Column(7).Width = 10;
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
            sheet.HeaderFooter.OddHeader.LeftAlignedText = "ПС " + balansGroup.SubstationTitle;
            sheet.HeaderFooter.OddHeader.RightAlignedText = string.Format("Экспортировано: {0}", DateTime.Now);
            // нижний колонтитул
            sheet.HeaderFooter.OddHeader.CenteredText = String.Format("Стр. {0} из {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);

            sheet.PrinterSettings.PaperSize = ePaperSize.A4;
            // установка параметров страницы
            sheet.PrinterSettings.Orientation = eOrientation.Landscape;
            sheet.PrinterSettings.HorizontalCentered = true;
            sheet.PrinterSettings.LeftMargin = 1.2m / 2.54m;
            sheet.PrinterSettings.TopMargin = 2.0m / 2.54m;
            sheet.PrinterSettings.RightMargin = 1.2m / 2.54m;
            sheet.PrinterSettings.BottomMargin = 1.2m / 2.54m;

            sheet.View.ZoomScale = 80;
            sheet.PrinterSettings.FitToPage = true;
            sheet.PrinterSettings.FitToWidth = 1;
        }
    }
}