using System;
using System.Collections.Generic;
using System.Linq;

using TMP.ExcelXml;

namespace TMP.ARMTES
{
    using Model;
    public class ExportCollectorListAsIs : BaseSmallEngineExport
    {
        public ExportCollectorListAsIs(ExportInfo exportInfo, IEnumerable<Collector> collectors)
            : base(exportInfo, collectors)
        {
            ;
        }

        public override bool Export(string outputFile)
        {
            // изменяем имя
            sheet.Name = "Экспорт";
            // закрепление строк
            sheet.FreezeTopRows = 6;
            return base.Export(outputFile);
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
                case ProfileType.BeginningOfTheDay:
                    paramName = "параметр: показания на начало суток";
                break;
                case ProfileType.BeginningOfTheMonth:
                    paramName = "параметр: показания на начало месяца";
                break;
	        }

            CreateRange(2, 1, 1, 19,
                String.Format("Отчет по результатам опроса объектов, оборудованных СДСП, в {0}", this.exportInfo.ElementName),
                titleStyle);
            CreateRange(3, 1, 1, 19, paramName, titleStyle);

            XmlStyle tableHeaderStyle = new XmlStyle();
            tableHeaderStyle.Font.Bold = true;
            tableHeaderStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center, WrapText = true };
            tableHeaderStyle.Border.LineStyle = Borderline.Continuous;
            tableHeaderStyle.Border.Sides = BorderSides.All;
            tableHeaderStyle.Border.Weight = 1;
            tableHeaderStyle.Interior.Color = System.Drawing.Color.Azure;

            CreateRange(4, 2, 1, 9, "Модем");
            CreateRange(4, 11, 1, 9, "Счётчик");
            CreateRange(5, 17, 1, 3, "Показания");

            CreateRange(4, 1, 3, 1, "№ п/п");
            CreateRange(5, 2, 2, 1, "Статус");
            CreateRange(5, 3, 2, 1, "УСПД");
            CreateRange(5, 4, 2, 1, "Сетевой адрес");
            CreateRange(5, 5, 2, 1, "№ GSM");
            CreateRange(5, 6, 2, 1, "Адрес объекта");
            CreateRange(5, 7, 2, 1, "№ договора");
            CreateRange(5, 8, 2, 1, "Наименование абонента");

            CreateRange(5, 9, 2, 1, "Объект учета");
            CreateRange(5, 10, 2, 1, "Номер ТП");


            CreateRange(5, 11, 2, 1, "Статус счётчика");
            CreateRange(5, 12, 2, 1, "Присоединение");
            CreateRange(5, 13, 2, 1, "Тип");
            CreateRange(5, 14, 2, 1, "Сетевой адрес");
            CreateRange(5, 15, 2, 1, "Заводской номер");
            CreateRange(5, 16, 2, 1, "Тариф");
            CreateRange(6, 17, 1, 1, String.Format("предыдущее\r\n({0})", this.exportInfo.StartDate.ToString(this.exportInfo.StartDateFormat)));
            CreateRange(6, 18, 1, 1, String.Format("следующее\r\n({0})", this.exportInfo.EndDate.ToString(this.exportInfo.EndDateFormat)));
            CreateRange(6, 19, 1, 1, "разность");

            ChangeRangeStyle(4, 1, 4, 19, tableHeaderStyle);
            ChangeRangeStyle(5, 2, 5, 16, tableHeaderStyle);
            ChangeRangeStyle(5, 17, 5, 19, tableHeaderStyle);
            ChangeRangeStyle(6, 17, 6, 19, tableHeaderStyle);
        }

        public override void CreateBody()
        {
            // создание стилей ячеек
            XmlStyle textStyle = new XmlStyle();
            textStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Left, Vertical = VerticalAlignment.Center, WrapText = true };
            textStyle.Border.LineStyle = Borderline.Continuous;
            textStyle.Border.Sides = BorderSides.All;
            textStyle.Border.Weight = 1;
            textStyle.Font.Size = 12;
            textStyle.Font.Name = "Calibri";

            XmlStyle numbersStyle = new XmlStyle(textStyle);
            numbersStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center, WrapText = true };
            numbersStyle.Font.Name = "Century Gothic";

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
                CreateRange(rowIndex, columnIndex++, collectorRowSpan, 1, collector.IsAnswered == true ? "OK" : "не отвечает", textStyle);
                CreateRange(rowIndex, columnIndex++, collectorRowSpan, 1, collector.IsUSPD == true ? "УСПД" : string.Empty, textStyle);
                CreateRange(rowIndex, columnIndex++, collectorRowSpan, 1, collector.NetworkAddress, numbersStyle);
                CreateRange(rowIndex, columnIndex++, collectorRowSpan, 1, collector.PhoneNumber, numbersStyle);

                int oldObjectColumnIndex = columnIndex;
                for (int objectIndex = 0; objectIndex < collector.Objects.Count; objectIndex++)
                {
                    AccountingObject aobject = collector.Objects[objectIndex];
                    int countersCount = aobject.CountersCount;
                    int objectRowSpan = aobject.Counters.Sum(c => c.TarifsCount == 1 ? 1 : 3);

                    columnIndex = oldObjectColumnIndex;
                    CreateRange(rowIndex, columnIndex++, objectRowSpan, 1, aobject.City, textStyle);
                    CreateRange(rowIndex, columnIndex++, objectRowSpan, 1, aobject.Contract, textStyle);
                    CreateRange(rowIndex, columnIndex++, objectRowSpan, 1, aobject.Subscriber, textStyle);
                    CreateRange(rowIndex, columnIndex++, objectRowSpan, 1, aobject.Name, textStyle);
                    CreateRange(rowIndex, columnIndex++, objectRowSpan, 1, aobject.Tp, textStyle);

                    bool isCounterMultiTariff = false;

                    int oldCounterColumnIndex = columnIndex;
                    for (int counterIndex = 0; counterIndex < countersCount; counterIndex++)
                    {
                        Counter counter = aobject.Counters[counterIndex];

                        isCounterMultiTariff = aobject.Counters[counterIndex].TarifsCount != 1;
                        int counterRowSpan = isCounterMultiTariff ? 3 : 1;

                        columnIndex = oldCounterColumnIndex;
                        CreateRange(rowIndex + counterIndex, columnIndex++, counterRowSpan, 1, counter.IsAnswered == true ? "OK" : "не отвечает", textStyle);
                        CreateRange(rowIndex + counterIndex, columnIndex++, counterRowSpan, 1, counter.AccountingPoint, textStyle);
                        CreateRange(rowIndex + counterIndex, columnIndex++, counterRowSpan, 1, counter.CounterType, textStyle);
                        CreateRange(rowIndex + counterIndex, columnIndex++, counterRowSpan, 1, counter.CounterNetworkAdress, numbersStyle);
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
                            columnIndex = oldCounterColumnIndex + 5;
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, "T1", textStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.PreviousIndications.Tarriff1), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.NextIndications.Tarriff1), valuesStyle);
                            CreateRange(rowIndex + counterIndex, columnIndex++, 1, 1, GetIndication(counter.DifferenceT1), valuesStyle);
                            rowIndex++;
                            // T2
                            columnIndex = oldCounterColumnIndex + 5;
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

        public override void CreateFooter()
        {
            ;
        }

        public override void ChangePageSettings()
        {
            // установка ширины столбцов
            sheet.Columns(0).Width = 30;
            sheet.Columns(1).Width = 30;
            sheet.Columns(2).Width = 35;
            sheet.Columns(3).Width = 40;
            sheet.Columns(4).Width = 85; // № GSM
            sheet.Columns(5).Width = 65; // Адрес объекта
            sheet.Columns(6).Width = 45; // № договора
            sheet.Columns(7).Width = 250; // Наименование абонента
            sheet.Columns(8).Width = 250; // Объект учета
            sheet.Columns(9).Width = 130; // Номер ТП

            sheet.Columns(10).Width = 32; // статус счётчика
            sheet.Columns(11).Width = 80; // присоединение
            sheet.Columns(12).Width = 45; // тип
            sheet.Columns(13).Width = 40; // сетевой адрес
            sheet.Columns(14).Width = 70; // номер
            sheet.Columns(15).Width = 30; // тариф
            sheet.Columns(16).Width = 80;
            sheet.Columns(17).Width = 80;
            sheet.Columns(18).Width = 85;

            // установка параметров страницы
            sheet.PrintOptions.SetFitToPage(1, 0);
            sheet.PrintOptions.Orientation = PageOrientation.Landscape;
            sheet.PrintOptions.SetMargins(0.7, 0.5, 0.5, 0.5);
        }
    }
}