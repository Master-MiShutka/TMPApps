using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Export
{
    using OfficeOpenXml;
    using TMP.Work.Emcos.Model.Balance;
    public class SubstationsBalanceExport : BaseExport
    {
        private OfficeOpenXml.Style.ExcelStyle tableStyle;
        OfficeOpenXml.Style.ExcelStyle titleStyle;
        OfficeOpenXml.Style.ExcelStyle dottedTableCell;

        public SubstationsBalanceExport(ExportInfo info) : base(info)
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


            dottedTableCell = CreateStyle("style_dottedTableCell");
            dottedTableCell.Border.Left.Style = 
                dottedTableCell.Border.Right.Style = 
                dottedTableCell.Border.Top.Style = 
                dottedTableCell.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Dotted;
            dottedTableCell.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            dottedTableCell.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            titleStyle = base.CreateStyle("style_title");
            titleStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            titleStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            titleStyle.Font.Bold = true;
            titleStyle.Font.Name = "Segoe UI";
            titleStyle.Font.Size = 14f;
            titleStyle.Font.Color.SetColor(System.Drawing.Color.Navy);
            titleStyle.WrapText = true;
        }
        public override void CreateHeader()
        {
            ;
        }
        private int _columnsCount = 13;
        private void CreateSheetHeader(string departament)
        {            
            CreateRange(1, 4, 2, _columnsCount - 3, exportInfo.Title + "\r\n" + departament, "style_title");
            
            CreateCell(2, 1, String.Format("период: {0} - {1}", exportInfo.StartDate.ToString("dd.MM.yyyy"), exportInfo.EndDate.ToString("dd.MM.yyyy")))
                .Style.Font.Italic = true;
        }

        private void CreateSheet(string departament, IList<Substation> list, bool createNames = false)
        {
            if (list == null)
                return;

            sheet = book.Worksheets.Add(departament);
            CreateSheetHeader(departament);

            CreateTableHeader();

            int count = list.Count();

            int startRowIndex = 5;
            int rowIndex = startRowIndex;               

            for (int index = 0; index < count; index++) 
            {
                var substation = list[index];
                #region * Шапка *
                int column = 1;

                CreateCell(rowIndex, column++, index + 1);
                CreateRange(rowIndex, column++, 1, 2, "ПС " + substation.Name);
                column++;
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.EnergyIn);
                if (createNames) book.Names.Add("Postuplenie_vvoda_i_fidera_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.VvodaIn);
                if (createNames) book.Names.Add("Postuplenie_vvoda_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.FideraIn);
                if (createNames) book.Names.Add("Postuplenie_fidera_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.EnergyOut);
                if (createNames) book.Names.Add("Otpusk_vvoda_i_fidera_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.VvodaOut);
                if (createNames) book.Names.Add("Otpusk_vvoda_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.FideraOut);
                if (createNames) book.Names.Add("Otpusk_fidera_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.TsnIn);
                if (createNames) book.Names.Add("Tsn_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.Unbalance);
                if (createNames) book.Names.Add("NeBalance_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column++, substation.ActiveEnergyBalance.PercentageOfUnbalance);
                if (createNames) book.Names.Add("NeBalance_procent_" + substation.Name.ToUpper(), Range(rowIndex, column - 1));
                CreateCell(rowIndex, column, substation.ActiveEnergyBalance.MaximumAllowableUnbalance);
                if (createNames) book.Names.Add("MaxNeBalance_procent_" + substation.Name.ToUpper(), Range(rowIndex, column));

                _columnsCount = column;

                ChangeRangeStyle(rowIndex, 1, 1, column, "style_table");

                if (substation.ActiveEnergy.Correction != null)
                {
                    Range(rowIndex, 2, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Range(rowIndex, 2, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
                    Range(rowIndex, 2, 1, 2).Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
                if (substation.ActiveEnergy.Plus.CorrectionValue != null)
                {
                    Range(rowIndex, 4).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Range(rowIndex, 4).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                }
                if (substation.ActiveEnergy.Minus.CorrectionValue != null)
                {
                    Range(rowIndex, 7).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Range(rowIndex, 7).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                }
                rowIndex++;
                #endregion
                #region * ТСН ввода *
                IList<IBalanceItem> auxilary = substation.Items.Where((c) => c.ElementType == Model.ElementTypes.UNITTRANSFORMER).ToList();
                foreach (UnitTransformer aux in auxilary)
                {
                    CreateCell(rowIndex, 3, aux.Name);
                    CreateCell(rowIndex, 4, aux.ActiveEnergy.Plus.Value);
                    CreateRange(rowIndex, 5, 1, 2, String.IsNullOrWhiteSpace(aux.ActiveEnergy.Plus.DaysValuesStatus) ? null : "нет данных: " + aux.ActiveEnergy.Plus.DaysValuesStatus);
                    CreateCell(rowIndex, 7, aux.ActiveEnergy.Minus.Value);
                    CreateRange(rowIndex, 8, 1, 2, String.IsNullOrWhiteSpace(aux.ActiveEnergy.Minus.DaysValuesStatus) ? null : "нет данных: " + aux.ActiveEnergy.Minus.DaysValuesStatus);
                    CreateCell(rowIndex, 10, aux.ActiveEnergy.Correction);
                    CreateRange(rowIndex, 11, 1, 3, null);
                    ChangeRangeStyle(rowIndex, 3, 1, 11, "style_dottedTableCell");

                    CreateCell(rowIndex, 15, aux.ActiveEnergy.Plus.Value / 1000.0);
                    CreateCell(rowIndex, 16, aux.ActiveEnergy.Minus.Value / 1000.0);
                    CreateCell(rowIndex, 17, aux.Code);
                    ChangeRangeStyle(rowIndex, 15, 1, 3, "style_dottedTableCell");

                    if (String.IsNullOrWhiteSpace(aux.ActiveEnergy.Correction) == false)
                    {
                        Range(rowIndex, 3).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Range(rowIndex, 3).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
                        Range(rowIndex, 3).Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }
                    if (aux.ActiveEnergy.Plus.CorrectionValue != null)
                    {
                        Range(rowIndex, 4).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Range(rowIndex, 4).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    }
                    if (aux.ActiveEnergy.Plus.DaysValuesStatus != null)
                    {
                        Range(rowIndex, 5, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Range(rowIndex, 5, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                    }
                    if (aux.ActiveEnergy.Minus.CorrectionValue != null)
                    {
                        Range(rowIndex, 7).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Range(rowIndex, 7).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    }
                    if (aux.ActiveEnergy.Minus.DaysValuesStatus != null)
                    {
                        Range(rowIndex, 8, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Range(rowIndex, 8, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                    }
                    sheet.Row(rowIndex).OutlineLevel = 1;
                    sheet.Row(rowIndex).Collapsed = true;
                    rowIndex++;
                }
                #endregion

                #region * Секции *
                IList<Model.IHierarchicalEmcosPoint> sections = substation.Children.Where((c) => c.ElementType == Model.ElementTypes.SECTION && (c as SubstationSection).IsLowVoltage).ToList();

                foreach (IBalanceItem section in sections)
                {
                    int sectionRowIndex = rowIndex;

                    var bss = section as SubstationSection;
                    if (bss == null)
                        System.Diagnostics.Debugger.Break();
                    CreateRange(rowIndex, 2, 1, 2, bss.Name);
                    CreateCell(rowIndex, 4, bss.ActiveEnergyBalance.EnergyIn);
                    CreateCell(rowIndex, 7, bss.ActiveEnergyBalance.EnergyOut);
                    CreateCell(rowIndex, 11, bss.ActiveEnergyBalance.Unbalance);
                    CreateCell(rowIndex, 12, bss.ActiveEnergyBalance.PercentageOfUnbalance);

                    ChangeRangeStyle(rowIndex, 2, 1, column - 1, "style_table");
                    Range(rowIndex, 2, 1, column - 1).Style.Font.Italic = true;

                    if (bss.ActiveEnergy.Correction != null)
                    {
                        Range(rowIndex, 2, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Range(rowIndex, 2, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
                        Range(rowIndex, 2, 1, 2).Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }
                    sheet.Row(rowIndex).OutlineLevel = 1;
                    sheet.Row(rowIndex).Collapsed = true;
                    rowIndex++;

                    int topRowIndex = rowIndex;

                    #region * ТРАСНФОРМАТОРЫ *
                    IList<Model.IHierarchicalEmcosPoint> transformers = bss.Children.Where((c) => c.ElementType == Model.ElementTypes.POWERTRANSFORMER).ToList();
                    foreach (PowerTransformer transformer in transformers)
                    {
                        CreateCell(rowIndex, 3, transformer.Name);
                        CreateCell(rowIndex, 4, transformer.ActiveEnergy.Plus.Value);
                        CreateRange(rowIndex, 5, 1, 2, String.IsNullOrWhiteSpace(transformer.ActiveEnergy.Plus.DaysValuesStatus) ? null : "нет данных: " + transformer.ActiveEnergy.Plus.DaysValuesStatus);
                        CreateCell(rowIndex, 7, transformer.ActiveEnergy.Minus.Value);
                        CreateRange(rowIndex, 8, 1, 2, String.IsNullOrWhiteSpace(transformer.ActiveEnergy.Minus.DaysValuesStatus) ? null : "нет данных: " + transformer.ActiveEnergy.Minus.DaysValuesStatus);
                        CreateCell(rowIndex, 10, transformer.ActiveEnergy.Correction);
                        CreateRange(rowIndex, 11, 1, 3, transformer.Description);
                        ChangeRangeStyle(rowIndex, 3, 1, 11, "style_dottedTableCell");

                        CreateCell(rowIndex, 15, transformer.ActiveEnergy.Plus.Value / 1000.0);
                        CreateCell(rowIndex, 16, transformer.ActiveEnergy.Minus.Value / 1000.0);
                        CreateCell(rowIndex, 17, transformer.Code);
                        ChangeRangeStyle(rowIndex, 15, 1, 3, "style_dottedTableCell");

                        if (String.IsNullOrWhiteSpace(transformer.ActiveEnergy.Correction) == false)
                        {
                            Range(rowIndex, 3).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 3).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
                            Range(rowIndex, 3).Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }
                        if (transformer.ActiveEnergy.Plus.CorrectionValue != null)
                        {
                            Range(rowIndex, 4).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 4).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (transformer.ActiveEnergy.Plus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 5, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 5, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        if (transformer.ActiveEnergy.Minus.CorrectionValue != null)
                        {
                            Range(rowIndex, 7).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 7).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (transformer.ActiveEnergy.Minus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 8, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 8, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        sheet.Row(rowIndex).OutlineLevel = 2;
                        sheet.Row(rowIndex).Collapsed = true;
                        rowIndex++;
                    }

                    #endregion
                    #region * ТСН ш *
                    IList<Model.IHierarchicalEmcosPoint> auxilaryBus = bss.Children.Where((c) => c.ElementType == Model.ElementTypes.UNITTRANSFORMERBUS).ToList();
                    foreach (UnitTransformerBus aux in auxilaryBus)
                    {
                        CreateCell(rowIndex, 3, aux.Name);
                        CreateCell(rowIndex, 4, aux.ActiveEnergy.Plus.Value);
                        CreateRange(rowIndex, 5, 1, 2, String.IsNullOrWhiteSpace(aux.ActiveEnergy.Plus.DaysValuesStatus) ? null : "нет данных: " + aux.ActiveEnergy.Plus.DaysValuesStatus);
                        CreateCell(rowIndex, 7, aux.ActiveEnergy.Minus.Value);
                        CreateRange(rowIndex, 8, 1, 2, String.IsNullOrWhiteSpace(aux.ActiveEnergy.Minus.DaysValuesStatus) ? null : "нет данных: " + aux.ActiveEnergy.Minus.DaysValuesStatus);
                        CreateCell(rowIndex, 10, aux.ActiveEnergy.Correction);
                        CreateRange(rowIndex, 11, 1, 3, null);
                        ChangeRangeStyle(rowIndex, 3, 1, 11, "style_dottedTableCell");

                        CreateCell(rowIndex, 15, aux.ActiveEnergy.Plus.Value / 1000.0);
                        CreateCell(rowIndex, 16, aux.ActiveEnergy.Minus.Value / 1000.0);
                        CreateCell(rowIndex, 17, aux.Code);
                        ChangeRangeStyle(rowIndex, 15, 1, 3, "style_dottedTableCell");

                        if (String.IsNullOrWhiteSpace(aux.ActiveEnergy.Correction) == false)
                        {
                            Range(rowIndex, 3).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 3).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
                            Range(rowIndex, 3).Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }
                        if (aux.ActiveEnergy.Plus.CorrectionValue != null)
                        {
                            Range(rowIndex, 4).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 4).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (aux.ActiveEnergy.Plus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 5, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 5, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        if (aux.ActiveEnergy.Minus.CorrectionValue != null)
                        {
                            Range(rowIndex, 7).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 7).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (aux.ActiveEnergy.Minus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 8, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 8, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        sheet.Row(rowIndex).OutlineLevel = 2;
                        sheet.Row(rowIndex).Collapsed = true;
                        rowIndex++;
                    }
                    foreach (UnitTransformerBus aux in auxilaryBus)
                    {
                        CreateCell(rowIndex, 3, aux.Name);
                        CreateCell(rowIndex, 4, aux.ActiveEnergy.Plus.Value);
                        CreateRange(rowIndex, 5, 1, 2, String.IsNullOrWhiteSpace(aux.ActiveEnergy.Plus.DaysValuesStatus) ? null : "нет данных: " + aux.ActiveEnergy.Plus.DaysValuesStatus);
                        CreateCell(rowIndex, 7, aux.ActiveEnergy.Minus.Value);
                        CreateRange(rowIndex, 8, 1, 2, String.IsNullOrWhiteSpace(aux.ActiveEnergy.Minus.DaysValuesStatus) ? null : "нет данных: " + aux.ActiveEnergy.Minus.DaysValuesStatus);
                        CreateCell(rowIndex, 10, aux.ActiveEnergy.Correction);
                        CreateRange(rowIndex, 11, 1, 3, null);
                        ChangeRangeStyle(rowIndex, 3, 1, 11, "style_dottedTableCell");

                        CreateCell(rowIndex, 15, aux.ActiveEnergy.Plus.Value / 1000.0);
                        CreateCell(rowIndex, 16, aux.ActiveEnergy.Minus.Value / 1000.0);
                        CreateCell(rowIndex, 17, aux.Code);
                        ChangeRangeStyle(rowIndex, 15, 1, 3, "style_dottedTableCell");

                        if (String.IsNullOrWhiteSpace(aux.ActiveEnergy.Correction) == false)
                        {
                            Range(rowIndex, 3).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 3).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
                            Range(rowIndex, 3).Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }
                        if (aux.ActiveEnergy.Plus.CorrectionValue != null)
                        {
                            Range(rowIndex, 4).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 4).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (aux.ActiveEnergy.Plus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 5, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 5, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        if (aux.ActiveEnergy.Minus.CorrectionValue != null)
                        {
                            Range(rowIndex, 7).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 7).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (aux.ActiveEnergy.Minus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 8, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 8, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        sheet.Row(rowIndex).OutlineLevel = 2;
                        sheet.Row(rowIndex).Collapsed = true;
                        rowIndex++;
                    }

                    #endregion
                    #region * ФИДЕРА *
                    IList<Model.IHierarchicalEmcosPoint> fiders = bss.Children.Where((c) => c.ElementType == Model.ElementTypes.FIDER).ToList();
                    foreach (Fider fider in fiders)
                    {
                        CreateCell(rowIndex, 3, fider.Name);
                        CreateCell(rowIndex, 4, fider.ActiveEnergy.Plus.Value);

                        CreateRange(rowIndex, 5, 1, 2, String.IsNullOrWhiteSpace(fider.ActiveEnergy.Plus.DaysValuesStatus) ? null : "нет данных: " + fider.ActiveEnergy.Plus.DaysValuesStatus);

                        CreateCell(rowIndex, 7, fider.ActiveEnergy.Minus.Value);

                        CreateRange(rowIndex, 8, 1, 2, String.IsNullOrWhiteSpace(fider.ActiveEnergy.Minus.DaysValuesStatus) ? null : "нет данных: " + fider.ActiveEnergy.Minus.DaysValuesStatus);

                        CreateCell(rowIndex, 10, fider.ActiveEnergy.Correction);

                        CreateRange(rowIndex, 11, 1, 3, null);

                        ChangeRangeStyle(rowIndex, 3, 1, 11, "style_dottedTableCell");

                        CreateCell(rowIndex, 15, fider.ActiveEnergy.Plus.Value / 1000.0);
                        CreateCell(rowIndex, 16, fider.ActiveEnergy.Minus.Value / 1000.0);
                        CreateCell(rowIndex, 17, fider.Code);
                        ChangeRangeStyle(rowIndex, 15, 1, 3, "style_dottedTableCell");

                        if (String.IsNullOrWhiteSpace(fider.ActiveEnergy.Correction) == false)
                        {
                            Range(rowIndex, 3).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 3).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Brown);
                            Range(rowIndex, 3).Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }
                        if (fider.ActiveEnergy.Plus.CorrectionValue != null)
                        {
                            Range(rowIndex, 4).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 4).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (fider.ActiveEnergy.Plus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 5, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 5, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        if (fider.ActiveEnergy.Minus.CorrectionValue != null)
                        {
                            Range(rowIndex, 7).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 7).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        if (fider.ActiveEnergy.Minus.DaysValuesStatus != null)
                        {
                            Range(rowIndex, 8, 1, 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Range(rowIndex, 8, 1, 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Aqua);
                        }
                        sheet.Row(rowIndex).OutlineLevel = 2;
                        sheet.Row(rowIndex).Collapsed = true;
                        rowIndex++;
                    }

                    #endregion

                    Range(sectionRowIndex, 10, rowIndex - sectionRowIndex, 1).Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
                #endregion
                // пустая строка вконце, добавлена чтобы работала группировка строк
                rowIndex++;
                sheet.Row(rowIndex - 1).OutlineLevel = 1;
                sheet.Row(rowIndex - 1).Collapsed = true;
            }
            rowIndex--;
            // толстая граница вокруг таблицы
            Range(startRowIndex, 1, rowIndex - startRowIndex, _columnsCount).Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

            // be-BY 423
            // ru-RU 419
            Range(startRowIndex, 4, rowIndex - startRowIndex, 7).Style.Numberformat.Format = @"[$-419]#,##0_ ;[Red]\-#,##0";
            // небаланс % и допустимый %
            Range(startRowIndex, 12, rowIndex - startRowIndex, 2).Style.Numberformat.Format = @"[$-419]#,##0.00_ ;[Red]\-#,##0.00";
            // для программы Balance
            Range(startRowIndex, 15, rowIndex - startRowIndex, 2).Style.Numberformat.Format = @"[$-419]#,##0.00_ ;[Red]\-#,##0.00";

            Range(startRowIndex, 2, rowIndex - startRowIndex, 2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Range(startRowIndex, 4, rowIndex - startRowIndex, 1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            Range(startRowIndex, 7, rowIndex - startRowIndex, 1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            Range(startRowIndex, 10, rowIndex - startRowIndex, 1).Style.WrapText = true;
            Range(startRowIndex, 10, rowIndex - startRowIndex, 1).Style.Font.Italic = true;                

            SetColumnWidthAndRowHeight();

            sheet.View.ShowGridLines = false;
            sheet.PrinterSettings.PrintArea = Range(1, 1, rowIndex, 13);
            ChangePageSettings();
        }

        public override void CreateBody()
        {
            book.Worksheets.Delete(sheet);

            CreateSheet("ВСЕ РЭСы", exportInfo.Substations
                .OrderBy(x => x.Departament)
                .ThenBy(x => x.Voltage)
                .ThenBy(x => x.Name)
                .ToList<Substation>());

            foreach (var grouping in exportInfo.Substations.GroupBy(x => x.Departament))
            {
                IList<Substation> list = grouping.Cast<Substation>()
                    .OrderBy(x => x.Voltage)
                    .ToList<Substation>();
                if (list == null)
                    continue;
                var departament = list.FirstOrDefault().Departament;
                CreateSheet(departament, list, true);
            }
        }
        /// <summary>
        /// шапка таблицы
        /// </summary>
        private void CreateTableHeader()
        {            
            CreateRange(3, 1, 2, 1, "№ п/п");
            CreateRange(3, 2, 2, 2, "Наименование");
            CreateRange(3, 4, 2, 1, "Поступление по вводам и фидерам, кВт∙ч");
            CreateRange(3, 5, 2, 1, "Поступление по вводам, кВт∙ч");
            CreateRange(3, 6, 2, 1, "Поступление по фидерам, кВт∙ч");
            CreateRange(3, 7, 2, 1, "Отпуск по фидерам и вводам, кВт∙ч");
            CreateRange(3, 8, 2, 1, "Отпуск по вводам, кВт∙ч");
            CreateRange(3, 9, 2, 1, "Отпуск по фидерам, кВт∙ч");
            CreateCell(3, 10, "ТСНш, кВт∙ч");
            CreateCell(4, 10, "Корректировка");
            CreateRange(3, 11, 1, 2, "Небаланс");
            CreateCell(4, 11, "кВт∙ч");
            CreateCell(4, 12, "%");
            CreateRange(3, 13, 2, 1, "Максимально допустимый, %");

            CreateRange(3, 15, 2, 1, "Приём (для Balance), тыс. кВт∙ч");
            CreateRange(3, 16, 2, 1, "Отдача (для Balance), тыс. кВт∙ч");
            CreateRange(3, 17, 2, 1, "Точка");
            ChangeRangeStyle(3, 15, 2, 3, "style_table");

            ChangeRangeStyle(3, 1, 2, _columnsCount, "style_table");

            Range(3, 5, 1, 1).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Dotted;
            Range(3, 5, 1, 2).Style.Font.Bold = false;

            Range(3, 8, 1, 1).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Dotted;
            Range(3, 8, 1, 2).Style.Font.Bold = false;

            Range(3, 1, 2, _columnsCount).Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
        }

        private void SetColumnWidthAndRowHeight()
        {
            int i = 1;
            sheet.Column(i++).Width = 4.5; // №
            sheet.Column(i++).Width = 4.5;
            sheet.Column(i++).Width = 25; // Наименование
            sheet.Column(i++).Width = 20; // Е+
            sheet.Column(i).Width = 15; // Е+ ввода
            sheet.Column(i).Collapsed = false;
            sheet.Column(i++).OutlineLevel = 1;
            sheet.Column(i).Width = 16; // Е+ фидера
            sheet.Column(i).Collapsed = false;
            sheet.Column(i++).OutlineLevel = 1;
            sheet.Column(i++).Width = 20; // Е-
            sheet.Column(i).Width = 15; // Е- ввода
            sheet.Column(i).Collapsed = false;
            sheet.Column(i++).OutlineLevel = 1;
            sheet.Column(i).Width = 16; // Е- фидера
            sheet.Column(i).Collapsed = false;
            sheet.Column(i++).OutlineLevel = 1;
            sheet.Column(i++).Width = 20; // тснш
            sheet.Column(i++).Width = 8.5; // Небаланс
            sheet.Column(i++).Width = 8.5; // Небаланс %
            sheet.Column(i++).Width = 15; // Допустимый

            sheet.Column(i++).Width = 5; // 
            sheet.Column(i++).Width = 20; // Приём для Balance
            sheet.Column(i++).Width = 20; // Отдача для Balance
            sheet.Column(i++).Width = 30; // Описание

            sheet.Row(1).Height = 23;
            sheet.Row(1).CustomHeight = true;

            sheet.Row(4).CustomHeight = true;
            sheet.Row(4).Height = 30.8;
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
            sheet.HeaderFooter.OddHeader.CenteredText = exportInfo.Title;
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

            sheet.PrinterSettings.RepeatRows = sheet.Cells["3:4"];

            sheet.View.FreezePanes(5, 1);
            sheet.View.ZoomScale = 115;
            sheet.PrinterSettings.FitToPage = true;
            sheet.PrinterSettings.FitToWidth = 1;
            sheet.PrinterSettings.FitToHeight = 0;
        }
    }
}
