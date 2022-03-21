namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DBF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMPApplication;

    internal partial class AramisDBParser
    {
        /// <summary>
        /// чтение оплат за электроэнергию
        /// </summary>
        /// <returns></returns>
        private async Task<IList<RawPaymentData>> GetPaymentDatasAsync()
        {
            int totalRecords = 0, processedRecords = 0;

            WorkTask workTask = new("чтение таблиц с данными по оплате")
            {
                Status = "подготовка ...",
            };
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);

            void _(string msg)
            {
                workTask.UpdateUI(++processedRecords, totalRecords, message: "таблица " + msg + $"\nзапись {processedRecords:N0} из {totalRecords:N0}", stepNameString: "обработка файла");
            }

            System.Collections.Concurrent.ConcurrentBag<RawPaymentData> list = new();
            try
            {
                // KARTKVGD - годовая база
                // KARTKVMN - месячная база

                string tableFileName = Path.Combine(this.pathDBF, "KARTKVGD.DBF");

                void toParse(DbfRecord record)
                {
                    if (record != null)
                    {
                        list.Add(this.ParsePaymentRecord(record));

                        _(System.IO.Path.GetFileName(tableFileName));
                    }
                }

                IList<RawPaymentData> result = await this.CheckAndLoadFromCacheAsync<RawPaymentData>(tableFileName, workTask);
                if (result != null)
                {
                    workTask.IsIndeterminate = false;
                    workTask.ChildProgress = 0;
                    int total = result.Count;
                    for (int i = 0; i < total; i++)
                    {
                        workTask.ChildProgress = 100d * i / total;
                        list.Add(result[i]);
                    }
                }
                else
                {
                    _(System.IO.Path.GetFileName(tableFileName));

                    using DBF.DbfReader dbfReader = new DbfReader(tableFileName, System.Text.Encoding.GetEncoding(866), true);
                    {
                        processedRecords = 0;
                        totalRecords = dbfReader.DbfTable.Header.NumberOfRecords;
                        _(System.IO.Path.GetFileName(tableFileName));

                        dbfReader.ParseRecords(toParse);

                        this.StoreHashAndSaveData(dbfReader.DataBaseFileInfo, workTask, list.ToArray());
                    }
                }
            }
            catch (IOException ioex)
            {
                logger?.Error(ioex, TMPApp.GetExceptionDetails(ioex));
                return null;
            }
            catch (Exception ex)
            {
                logger?.Error($">>> read PaymentData\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                return null;
            }

            return this.SortData(list, workTask);
        }

        /// <summary>
        /// Заполнение недостающих полей в таблице полезного отпуска электроэнергии
        /// </summary>
        /// <param name="electricitySupplies"></param>
        /// <param name="meters"></param>
        private void AddAdditionalInfoToElectricitySupply(IEnumerable<ElectricitySupply> electricitySupplies, IEnumerable<Meter> meters)
        {
            if (electricitySupplies == null || meters == null)
            {
                return;
            }

            Model.WorkTask workTask = new("заполнение недостающих полей в таблице полезного отпуска электроэнергии");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            int totalRows = (electricitySupplies as ICollection).Count;
            int processedRows = 0;

            try
            {
                List<Meter> metersList = new List<Meter>(meters);

                Parallel.ForEach(electricitySupplies, electricitySupply =>
                {
                    if (electricitySupply.Лицевой == 0)
                    {
                        return;
                    }

                    // используем бинарный поиск в заранее отсортированном списке
                    int index = metersList.BinarySearch(new Meter { Лицевой = electricitySupply.Лицевой }, this.meterComparerByPersonalID);

                    Meter meter = null;
                    if (index > 0)
                    {
                        meter = metersList[index];
                    }
                    else
                    {
                        ;
                    }

                    if (meter != null)
                    {
                        electricitySupply.Адрес = meter.Адрес?.StreetWithHouseNumberAndApartment;
                        electricitySupply.Населённый_пункт = meter.Адрес?.City;
                        electricitySupply.Подстанция = meter.Подстанция;
                        electricitySupply.Фидер10 = meter.Фидер10;

                        if (meter.НомерСчетчика == null)
                        {
                            logger?.Warn($"AddAdditionalInfoToElectricitySupply - Номер_счетчика is null, Лицевой: '{electricitySupply.Лицевой}'");
                        }

                        if (meter.ТП == null)
                        {
                            logger?.Warn($"AddAdditionalInfoToElectricitySupply - ТП is null, Лицевой: '{electricitySupply.Лицевой}'");
                        }
                        else
                        {
                            electricitySupply.НомерТП = meter.ТП.Number;
                            electricitySupply.ТипТП = meter.ТП.Type;
                            electricitySupply.НаименованиеТП = meter.ТП.Name;
                        }

                        electricitySupply.Фидер04 = meter.Фидер04;
                    }

                    workTask.UpdateUI(++processedRows, totalRows, stepNameString: "лицевой счет");
                });
            }
            catch (Exception ex)
            {
                logger?.Error($">>> TMP.WORK.AramisChetchiki.AramisDBParser>AddAdditionalInfoToElectricitySupply\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                return;
            }

            // fix
            workTask.UpdateUI(totalRows, totalRows, stepNameString: "лицевой счет");

            workTask.IsCompleted = true;
        }

        private void GroupPaymentDatasAsync(IList<RawPaymentData> paymentDatas)
        {
            WorkTask workTask = new("группировка произведенных оплат абонентом")
            {
                Progress = 0d,
                ChildProgress = 0d,
            };
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            int processedRecords = 0;

            // группировка оплат по лицевому счету, затем по периоду оплаты (в одном периоде может быть несколько оплат)
            Dictionary<ulong, Dictionary<DateOnly, List<RawPaymentData>>> result = paymentDatas
                .GroupBy(i => i.Лицевой)
                .ToDictionary(i => i.Key, e => e.GroupBy(element => element.ПериодОплаты).ToDictionary(i => i.Key, j => j.ToList()));

            this.data.Payments = new();

            int totalRecords = result.Count;
            int childProcessed = 0, childCount = 0;
            SortedList<DateOnly, Payment> sortedList = new SortedList<DateOnly, Payment>(this.dateOnlyComparerByAscending);

            // по всем лицевым счетам
            foreach (KeyValuePair<ulong, Dictionary<DateOnly, List<RawPaymentData>>> item in result)
            {
                sortedList.Clear();

                childProcessed = 0;
                childCount = item.Value.Count;

                // по всем временным периодам
                foreach (KeyValuePair<DateOnly, List<RawPaymentData>> p in item.Value)
                {
                    Payment payment = new Payment
                    {
                        Лицевой = item.Key,
                        ПериодОплаты = p.Key,

                        // массив оплат в этом периоде
                        Payments = new PaymentData[p.Value.Count],
                    };

                    int index = 0;

                    // по всем оплатам за этот период
                    IOrderedEnumerable<RawPaymentData> pp = p.Value.OrderBy(i => i.ПредыдущееПоказание);
                    foreach (RawPaymentData rawPayment in pp)
                    {
                        payment.Payments[index++] = PaymentData.FromRawData(rawPayment);
                    }

                    payment.ПредыдущееПоказание = payment.Payments.Min(i => i.ПредыдущееПоказание);
                    payment.ПоследнееПоказание = payment.Payments.Max(i => i.ПоследнееПоказание);
                    payment.СуммаОплаты = payment.Payments.Sum(i => i.СуммаОплатыРасчётная ?? 0f);

                    sortedList.Add(payment.ПериодОплаты, payment);

                    workTask.UpdateUI(processedRecords, totalRecords, ++childProcessed, childCount);
                }

                if (this.data.Payments.ContainsKey(item.Key))
                {
                    foreach (KeyValuePair<DateOnly, Payment> element in sortedList)
                    {
                        this.data.Payments[item.Key].Add(element.Value);
                    }
                }
                else
                {
                    this.data.Payments.Add(item.Key, sortedList.Values.ToList());
                }

                workTask.UpdateUI(++processedRecords, totalRecords);
            }

            workTask.IsCompleted = true;
            this.workTasksProgressViewModel.WorkTasks.Remove(workTask);
        }
    }
}
