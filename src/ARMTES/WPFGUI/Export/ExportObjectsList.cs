using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TMP.ExcelXml;

namespace TMP.ARMTES
{
    using Model;
    public class ExportObjectsList : ExportCollectorListAsIs
    {
        public ExportObjectsList(ExportInfo info, IEnumerable<Collector> collectors)
            : base(info, collectors)
        {
            ;
        }
        public override void CreateHeader()
        {
            CreateRange(1, 1, 1, 1, String.Format("Экспортировано: {0}", DateTime.Now));

            XmlStyle titleStyle = new XmlStyle();
            titleStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center, WrapText = true };
            titleStyle.Font.Size = 14;

            CreateRange(2, 1, 1, 15,
                String.Format("Список объектов, оборудованных СДСП, в {0}", this.exportInfo.ElementName),
                titleStyle);

            XmlStyle tableHeaderStyle = new XmlStyle();
            tableHeaderStyle.Font.Bold = true;
            tableHeaderStyle.Font.Size = 12;
            tableHeaderStyle.Alignment = new AlignmentOptions() { Horizontal = HorizontalAlignment.Center, Vertical = VerticalAlignment.Center, WrapText = true };
            tableHeaderStyle.Border.LineStyle = Borderline.Continuous;
            tableHeaderStyle.Border.Sides = BorderSides.All;
            tableHeaderStyle.Border.Weight = 1;
            tableHeaderStyle.Interior.Color = System.Drawing.Color.Azure;

            CreateRange(4, 2, 1, 4, "Модем");
            CreateRange(4, 6, 1, 5, "Объект");
            CreateRange(4, 11, 1, 5, "Счётчик");

            CreateRange(4, 1, 2, 1, "№ п/п");
            CreateRange(5, 2, 1, 1, "Статус");
            CreateRange(5, 3, 1, 1, "УСПД");
            CreateRange(5, 4, 1, 1, "Сетевой адрес");
            CreateRange(5, 5, 1, 1, "№ GSM");
            CreateRange(5, 6, 1, 1, "Адрес объекта");
            CreateRange(5, 7, 1, 1, "№ договора");
            CreateRange(5, 8, 1, 1, "Наименование абонента");

            CreateRange(5, 9, 1, 1, "Объект учета");
            CreateRange(5, 10, 1, 1, "Номер ТП");


            CreateRange(5, 11, 1, 1, "Статус счётчика");
            CreateRange(5, 12, 1, 1, "Присоединение");
            CreateRange(5, 13, 1, 1, "Тип");
            CreateRange(5, 14, 1, 1, "Сетевой адрес");
            CreateRange(5, 15, 1, 1, "Заводской номер");


            ChangeRangeStyle(4, 1, 5, 15, tableHeaderStyle);
        }

        public override void CreateBody()
        {
            // создание стилей ячеек
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

            const int startRow = 6;
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
            sheet.Columns(1).Width = 40;
            sheet.Columns(2).Width = 35;
            sheet.Columns(3).Width = 40;
            sheet.Columns(4).Width = 95; // № GSM
            sheet.Columns(5).Width = 70; // Адрес объекта
            sheet.Columns(6).Width = 45; // № договора
            sheet.Columns(7).Width = 250; // Наименование абонента
            sheet.Columns(8).Width = 250; // Объект учета
            sheet.Columns(9).Width = 130; // Номер ТП

            sheet.Columns(10).Width = 45; // статус счётчика
            sheet.Columns(11).Width = 80; // присоединение
            sheet.Columns(12).Width = 45; // тип
            sheet.Columns(13).Width = 42; // сетевой адрес
            sheet.Columns(14).Width = 80; // номер
            // установка параметров страницы
            sheet.PrintOptions.Orientation = PageOrientation.Landscape;
            sheet.PrintOptions.SetMargins(0.7, 0.5, 0.5, 0.5);
            sheet.PrintOptions.SetFitToPage(1, 0);
        }
        public override bool Export(string outputFile)
        {
            // изменяем имя
            sheet.Name = "Экспорт";
            // закрепление строк
            sheet.FreezeTopRows = 5;
            return base.Export(outputFile);
        }
    }
}
