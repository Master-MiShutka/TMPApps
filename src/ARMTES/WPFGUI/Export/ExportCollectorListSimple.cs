using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TMP.ExcelXml;

namespace TMP.ARMTES
{
    using Model;
    public class ExportCollectorListSimple : ExportCollectorListAsIs
    {
        private int _columnsCount = 10;

        public ExportCollectorListSimple(ExportInfo exportInfo, IEnumerable<Collector> collectors)
            : base(exportInfo, collectors)
        {
            ;
        }
        public override void CreateHeader()
        {
            CreateRange(1, 1, 1, 1, String.Format("Экспортировано: {0}", DateTime.Now));

            XmlStyle titleStyle = new XmlStyle();
            titleStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center, WrapText = true };
            titleStyle.Font.Size = 14;

            string paramName = string.Empty;
            switch (this.exportInfo.Param)
            {
                case ProfileType.Current:
                    paramName = "параметр: текущие показания";
                    break;
                case ProfileType.Days:
                    paramName = "параметр: показания на начало суток";
                    break;
                case ProfileType.Months:
                    paramName = "параметр: показания на начало месяца";
                    break;
            }

            CreateRange(2, 1, 1, 10,
                String.Format("Отчет по результатам опроса объектов, оборудованных СДСП, в {0}", this.exportInfo.ElementName),
                titleStyle);
            CreateRange(3, 1, 1, 10, paramName, titleStyle);

            XmlStyle tableHeaderStyle = new XmlStyle();
            tableHeaderStyle.Font.Bold = true;
            tableHeaderStyle.Font.Size = 12;
            tableHeaderStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center, WrapText = true };
            tableHeaderStyle.Border.LineStyle = Borderline.Continuous;
            tableHeaderStyle.Border.Sides = BorderSides.All;
            tableHeaderStyle.Border.Weight = 1;
            tableHeaderStyle.Interior.Color = System.Drawing.Color.Azure;

            CreateRange(4, 2, 3, 1, "№ договора");
            CreateRange(4, 3, 3, 1, "Расчётная точка");
            CreateRange(4, 4, 1, 7, "Счётчик");
            CreateRange(5, 8, 1, 3, "Показания");

            CreateRange(4, 1, 3, 1, "№ п/п");
            CreateRange(5, 4, 2, 1, "Присоединение");
            CreateRange(5, 5, 2, 1, "Тип");
            CreateRange(5, 6, 2, 1, "Заводской номер");
            CreateRange(5, 7, 2, 1, "Тариф");
            CreateRange(6, 8, 1, 1, String.Format("предыдущее\r\n({0})", this.exportInfo.StartDate.ToString(this.exportInfo.StartDateFormat)));
            CreateRange(6, 9, 1, 1, String.Format("следующее\r\n({0})", this.exportInfo.EndDate.ToString(this.exportInfo.EndDateFormat)));
            CreateRange(6, 10, 1, 1, "разность");

            ChangeRangeStyle(4, 1, 6, 10, tableHeaderStyle);
            XmlStyle externalBordersStyle = new XmlStyle();
            externalBordersStyle.Border.LineStyle = Borderline.Continuous;
            externalBordersStyle.Border.Sides = BorderSides.All;
            externalBordersStyle.Border.Weight = 1;
            ChangeRangeStyle(4, 1, 6, 10, externalBordersStyle);
        }

        public override void CreateBody()
        {
            // создание стилей ячеек
            XmlStyle objectStyle = new XmlStyle();
            objectStyle.Border.LineStyle = Borderline.Continuous;
            objectStyle.Border.Sides = BorderSides.Bottom;
            objectStyle.Border.Weight = 2;

            XmlStyle textStyle = new XmlStyle();
            textStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Left, Vertical = VerticalAlignment.Center, WrapText = true };
            textStyle.Border.LineStyle = Borderline.Continuous;
            textStyle.Border.Sides = BorderSides.All;
            textStyle.Border.Weight = 1;
            textStyle.Font.Size = 13;
            textStyle.Font.Name = "Calibri";

            XmlStyle numbersStyle = new XmlStyle(textStyle);
            numbersStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center, WrapText = true };
            numbersStyle.Font.Name = "Century Gothic";
            numbersStyle.Font.Size = 14;

            XmlStyle valuesStyle = new XmlStyle(numbersStyle);
            valuesStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Right, Vertical = VerticalAlignment.Center, WrapText = false };
            valuesStyle.DisplayFormat = DisplayFormatType.Custom;
            valuesStyle.CustomFormatString = "# ##0";

            int rowCount = 0;
            foreach (var item in this.collectors)
            {
                rowCount += item.CountersCount;
            }

            const int startRow = 7;
            int rowIndex = startRow;

            int collectorIndex = 0;
            foreach (Collector collector in this.collectors)
            {
                int columnIndex = 1;

                int collectorRowSpan = collector.Objects.Sum(o => o.Counters.Sum(c => c.TarifsCount == 1 ? 1 : 3));


                CreateRange(rowIndex, columnIndex++, collectorRowSpan, 1, collectorIndex + 1, numbersStyle);
                ChangeRangeStyle(rowIndex, columnIndex - 1, collectorRowSpan, _columnsCount, objectStyle);

                int oldObjectColumnIndex = columnIndex;
                for (int objectIndex = 0; objectIndex < collector.Objects.Count; objectIndex++)
                {
                    AccountingObject aobject = collector.Objects[objectIndex];
                    int countersCount = aobject.CountersCount;
                    int objectRowSpan = aobject.Counters.Sum(c => c.TarifsCount == 1 ? 1 : 3);

                    columnIndex = oldObjectColumnIndex;
                    CreateRange(rowIndex, columnIndex++, objectRowSpan, 1, aobject.Contract, textStyle);
                    CreateRange(rowIndex, columnIndex++, objectRowSpan, 1, String.Format("{0} / {1}", aobject.Subscriber, aobject.Name), textStyle);

                    bool isCounterMultiTariff = false;

                    int oldCounterColumnIndex = columnIndex;
                    for (int counterIndex = 0; counterIndex < countersCount; counterIndex++)
                    {
                        Counter counter = aobject.Counters[counterIndex];

                        isCounterMultiTariff = aobject.Counters[counterIndex].TarifsCount != 1;
                        int counterRowSpan = isCounterMultiTariff ? 3 : 1;

                        columnIndex = oldCounterColumnIndex;
                        CreateRange(rowIndex + counterIndex, columnIndex++, counterRowSpan, 1, counter.AccountingPoint, textStyle);
                        CreateRange(rowIndex + counterIndex, columnIndex++, counterRowSpan, 1, counter.CounterType, textStyle);
                        CreateRange(rowIndex + counterIndex, columnIndex++, counterRowSpan, 1, counter.SerialNumber, numbersStyle);

                        // тариф
                        if (isCounterMultiTariff)
                        // счётчик многотарифный
                        {
                            // TƩ
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, "TƩ", textStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.PreviousIndications.Tarriff0), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.NextIndications.Tarriff0), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.Difference), valuesStyle);
                            rowIndex++;
                            // T1
                            columnIndex = oldCounterColumnIndex + 3;
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, "T1", textStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.PreviousIndications.Tarriff1), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.NextIndications.Tarriff1), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.DifferenceT1), valuesStyle);
                            rowIndex++;
                            // T2
                            columnIndex = oldCounterColumnIndex + 3;
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, "T2", textStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.PreviousIndications.Tarriff2), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.NextIndications.Tarriff2), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.DifferenceT2), valuesStyle);
                        }
                        // однотарифный
                        else
                        {
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, "TƩ", textStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.PreviousIndication), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.NextIndication), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.Difference), valuesStyle);
                        }
                    }

                    rowIndex += countersCount;
                }
                collectorIndex++;
            }
        }
        public override void ChangePageSettings()
        {
            // установка ширины столбцов
            sheet.Columns(0).Width = 30;
            sheet.Columns(1).Width = 35; // № договора
            sheet.Columns(2).Width = 250; // расчётная точка
            sheet.Columns(3).Width = 110; // присоединение
            sheet.Columns(4).Width = 50;  // тип
            sheet.Columns(5).Width = 120;  // заводской номер
            sheet.Columns(6).Width = 40;  // тариф
            sheet.Columns(7).Width = 80;
            sheet.Columns(8).Width = 80;
            sheet.Columns(9).Width = 85;
            // установка параметров страницы
            sheet.PrintOptions.Orientation = PageOrientation.Landscape;
            sheet.PrintOptions.Layout = PageLayout.CenterHorizontal;
            sheet.PrintOptions.SetMargins(0.7, 0.5, 0.5, 0.5);
            sheet.PrintOptions.SetFitToPage(1, 0);
        }
    }
}
