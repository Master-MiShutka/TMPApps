using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Threading.Tasks;
using System.Threading;

using TMP.ExcelXml;
using TMP.ARMTES.Model;
using TMP.Shared;


namespace TMP.ARMTES.Editor
{
    public class XMLExcelParser
    {
        public static Task<RegistrySDSP> Parse(string fileName)
        {
            RegistrySDSP sdsp = new RegistrySDSP();
            TaskCompletionSource<RegistrySDSP> tcs = new TaskCompletionSource<RegistrySDSP>();

            ThreadStart listenThread = delegate()
            {
                try
                {
                    ExcelXmlWorkbook book = ExcelXmlWorkbook.Import("data.xml");
                    var r = book.SheetCount;
                    Worksheet sheet = book[0];

                    int columnCount = sheet.ColumnCount;
                    if (columnCount <= 0) throw new InvalidDataException("В рабочем листе недопустимое количество колонок.");

                    int rowCount = sheet.RowCount;
                    if (rowCount <= 3) throw new InvalidDataException("В рабочем листе недопустимое количество строк.");

                    String departament = null;
                    RegistryCollector collector = null;

                    // количество счётчиков очередной системы
                    int collectorMetersCount = 0;
                    // количество счётчиков для обработки
                    int countMetersToParse = 0;

                    #region парсер

                    // пропускаем название таблицы и шапку таблицы
                    for (int rowIndex = 2; rowIndex < sheet.RowCount; rowIndex++)
                    {
                        Row row = sheet[rowIndex];
                        int cellCount = row.CellCount;

                        // имеются ли данные в строке
                        bool hasData = false;
                        for (int colIndex = 0; colIndex < row.CellCount; colIndex++)
                            if (!row[colIndex].IsEmpty()) 
                            {
                                hasData = true;
                                break;
                            }

                        if (hasData == false) continue;

                        // первая ячейка в строке, это не первая ячейка в строке листа EXCEL !
                        Cell firstCell = row[0];

                        ContentType meterNameCellContentType = row[4].ContentType;

                        // если это какая-то категория
                        if (firstCell.ContentType == ContentType.None & meterNameCellContentType == ContentType.None)
                        {
                            // если это новая - добавляем предыдущую в список
                            if (departament != null)
                                sdsp.Departaments.Add(departament);
                            departament = String.Empty;

                            StringBuilder data = new StringBuilder();
                            for (int i = 0; i < row.CellCount; i++)
                                if (row[i].ContentType == ContentType.String) data.Append(row[i].Value);
                            departament = data.ToString();
                        }
                        // тогда если это данные
                        else
                        {
                                if (meterNameCellContentType == ContentType.UnresolvedValue)
                                {
                                    U.L(LogLevel.Error, "EDITOR PARSER",
                                        String.Format("Не обнаружены данные:\nНомер строки таблицы {0}", rowIndex));
                                    throw new InvalidDataException("Не обнаружены данные.");
                                }

                            if (meterNameCellContentType == ContentType.String)
                            {
                                RegistryCounter counter = new RegistryCounter();

                                // имеется более одного счётчика?
                                collectorMetersCount = firstCell.RowSpan;

                                if (collectorMetersCount > 1) countMetersToParse = collectorMetersCount;

                                // если значение в первом столбце имеется и является числовым значением
                                if (firstCell.ContentType == ContentType.Number)
                                {                                   
                                    collector = new RegistryCollector();

                                    try
                                    {

                                        // номер по-порядку
                                        collector.NumberOfOrder = row[0].GetValue<uint>();
                                        // фидер
                                        collector.House = row[1].Value == null ? "<???>" : row[1].GetValue<string>();
                                        // тип модема
                                        collector.ModemType = row[2].Value == null ? "<???>" : row[2].GetValue<string>();
                                        // номер gsm
                                        collector.GsmNumber = row[3].Value == null ? "<???>" : row[3].Value.ToString();
                                        // место установки
                                        collector.Street = row[10].Value == null ? "<???>" : row[10].GetValue<string>();
                                        // примечание и номер договора
                                        if (row[11].ContentType == ContentType.String)
                                            collector.Description += "Договор №" + Environment.NewLine + row[11].GetValue<string>();
                                        collector.Description += row[12].GetValue<string>();

                                        collector.Departament = departament;

                                        //collector.CreationDate = row[13].Value == null ? "<?>" : row[13].GetValue<string>();
                                    }
                                    catch (Exception ex)
                                    {
                                        var s = ex.Message;
                                    }

                                }
                                try
                                {
                                    // присоединение
                                    counter.Name = row[4].Value == null ? "<???>" : row[4].GetValue<string>();
                                    // состояние
                                    // сетевой адрес                                    
                                    counter.NetworkAddress = row[6].Value == null ? "<???>" : row[6].Value.ToString();
                                    // тип счётчика
                                    counter.CounterType = row[7].Value == null ? "<???>" : row[7].GetValue<string>();
                                    // номер счётчика
                                    counter.Number = row[8].Value == null ? "<???>" : row[8].Value.ToString();
                                    // количество тарифов
                                    counter.TarifsCount = row[9].Value == null ? (byte)0 : row[9].GetValue<byte>();
                                }
                                catch (Exception ex)
                                {
                                    var s = ex.Message;
                                }

                                //
                                countMetersToParse--;

                                if (collector == null)
                                    System.Diagnostics.Debugger.Break();

                                if (collector.Counters == null)
                                    System.Diagnostics.Debugger.Break();

                                if (counter == null)
                                    System.Diagnostics.Debugger.Break();

                                collector.Counters.Add(counter);

                                if (countMetersToParse == 0)
                                {
                                    sdsp.Collectors.Add(collector);
                                    collector = null;
                                }
                            }
                        }
                    }
                    #endregion

                    tcs.TrySetResult(sdsp);
                }
                catch (Exception e)
                {
                    U.L(LogLevel.Error, "EDITOR", "Ошибка при импорте данных.");
                    U.L(LogLevel.Error, "EDITOR", e.Message);
                }                
            };            

            Thread l_thread = new Thread(listenThread);
            l_thread.Name = "Import data thread";
            l_thread.Priority = ThreadPriority.Highest;
            l_thread.Start();            

            return tcs.Task;
        }

        
    }
}
