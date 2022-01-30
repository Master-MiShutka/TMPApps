namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
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
        private IList<RawPaymentData> GetPaymentDatas()
        {
            int filesCount = 0, processedFiles = 0;
            int totalRecords = 0, processedRecords = 0;

            WorkTask workTask = new("чтение таблиц с данными по оплате")
            {
                Status = "подготовка ...",
            };
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);

            void _(string msg)
            {
                workTask.UpdateUI(processedFiles, filesCount, ++processedRecords, totalRecords, message: "таблица " + msg + $"\nзапись {processedRecords:N0} из {totalRecords:N0}", stepNameString: "обработка файла");
            }

            System.Collections.Concurrent.ConcurrentBag<RawPaymentData> list = new();
            try
            {
                string[] files = System.IO.Directory.GetFiles(this.pathDBF, "KARTKV*.DBF");
                filesCount = files.Length;

                string tableFileName = string.Empty;

                void toParse(DbfRecord record)
                {
                    if (record != null)
                    {
                        list.Add(this.ParsePaymentRecord(record));

                        _(System.IO.Path.GetFileName(tableFileName));
                    }
                }

                foreach (string file in files)
                {
                    IList<RawPaymentData> result = this.CheckAndLoadFromCache<RawPaymentData>(file, ref workTask);
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
                        tableFileName = file;

                        string digits = tableFileName.Substring(tableFileName.Length - 6, 2);

                        if (/*byte.TryParse(digits, out byte n) ||*/ string.Equals(digits, "GD", StringComparison.OrdinalIgnoreCase) || string.Equals(digits, "MN", StringComparison.OrdinalIgnoreCase) || string.Equals(digits, "VW", StringComparison.OrdinalIgnoreCase))
                        {
                            _(System.IO.Path.GetFileName(tableFileName));

                            using DBF.DbfReader dbfReader = new DbfReader(tableFileName, System.Text.Encoding.GetEncoding(866), true);
                            {
                                processedRecords = 0;
                                totalRecords = dbfReader.DbfTable.Header.NumberOfRecords;
                                _(System.IO.Path.GetFileName(tableFileName));

                                dbfReader.ParseRecords(toParse);
                            }

                            this.StoreHashAndSaveData(tableFileName, ref workTask, list.ToArray());
                        }
                    }

                    processedFiles++;
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
                        electricitySupply.Адрес = meter.Адрес?.УлицаСДомомИКв;
                        electricitySupply.Населённый_пункт = meter.Адрес?.НаселённыйПункт;
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
    }
}
