namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DBF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMPApplication;

    internal partial class AramisDBParser
    {
        private const string UNKNOWN_STR = "#Н/Д";

        #region Model parsers

        /// <summary>
        /// получение таблицы с абонентами
        /// </summary>
        /// <returns></returns>
        private IList<Meter> GetMeters()
        {
            int totalRows = 0;
            int processedRows = 0;

            Model.WorkTask workTask = new("обработка картотеки");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);

            // таблица с абонентами
            totalRows = this.collectionKARTAB.Count;

            System.Collections.Concurrent.ConcurrentBag<Meter> metersList = new System.Collections.Concurrent.ConcurrentBag<Meter>();

            string s = string.Empty;

            workTask.UpdateStatus($"количество строк в таблице абонентов: {totalRows:N0}");
            processedRows = 0;
            workTask.StartProcessing();

            Parallel.ForEach(this.collectionKARTAB, (abonent) =>
            {
                ICollection<KARTSCH> abonentMeters = this.GetDictionaryValue(this.dictionaryKARTSCH, abonent.LIC_SCH);

                if (abonentMeters != null && abonentMeters.Count != 0)
                {
                    var rowsGroupedByMeter = abonentMeters
                        .GroupBy(
                            i => i.N_SCH.ToString().Trim(),
                            (meterNumber, list) => new { MeterNumber = meterNumber, List = list.ToArray() });

                    foreach (var group in rowsGroupedByMeter)
                    {
                        KARTSCH meterDataRow = group.List[0];
                        Meter meter = this.ParseDataRowAndGetMeter(meterDataRow, abonent);
                        meter.Тарифов = group.List.Length > 1 ? (byte)group.List.Length : (byte)1;
                        metersList.Add(meter);
                    }
                }
                else
                {
                    Meter meter = this.ParseDataRowAndGetMeter(null, abonent);
                    meter.Тарифов = 0;
                    metersList.Add(meter);
                }

                workTask.UpdateUI(++processedRows, totalRows);
            });

            // fix
            workTask.UpdateUI(totalRows, totalRows);

            return this.SortData(metersList, workTask);
        }

        private Meter ParseDataRowAndGetMeter(KARTSCH meterData, KARTAB abonent)
        {
            Meter meter = new();
            try
            {
                #region Контактные данные

                meter.Удалён = abonent.IsDeleted;

                meter.Фамилия = abonent.FAM;
                meter.Имя = abonent.NAME;
                meter.Отчество = abonent.OTCH;

                meter.SMS = abonent.SMS;
                meter.Телефоны = abonent.TELEF;

                if (this.dictionaryKartTn.ContainsKey(abonent.COD_TN))
                {
                    KartTn town = this.dictionaryKartTn[abonent.COD_TN];
                    string townName = town.TOWN;
                    string province = NOTFOUNDED;
                    if (this.dictionaryKartSs.ContainsKey(town.COD_SS))
                    {
                        province = this.dictionaryKartSs[town.COD_SS].СЕЛЬСОВЕТ;

                        if (string.IsNullOrEmpty(province) && townName != null && townName.StartsWith("г.", AppSettings.StringComparisonMethod))
                        {
                            province = "город";
                        }
                    }
                    else
                    {
                        if (string.Equals(province, NOTFOUNDED) && townName != null && townName.StartsWith("г.", AppSettings.StringComparisonMethod))
                        {
                            province = "город";
                        }
                    }

                    KartSt street = this.GetDictionaryValue(this.dictionaryKartSt, abonent.COD_ST);

                    Address address = new(
                        townName,
                        street?.STREET,
                        abonent.HOME,
                        abonent.KV,
                        province);
                    meter.Адрес = address;
                }
                else
                {
                    string code = abonent.COD_TN;
                    this.errors.Add($"Не найден населенный пункт с кодом {code}");
                }

                #endregion

                #region Абонент

                meter.Категория = this.GetDictionaryValue(this.dictionaryKartKat, abonent.COD_KAT)?.KATEGAB;

                meter.Коментарий = abonent.KOMENT;

                meter.ДатаУведомления = ConvertToDateOnly(abonent.ДАТА_ОТКПЛ);

                meter.ДатаОтключения = ConvertToDateOnly(abonent.ДАТА_ОТКФК);
                meter.ПоказаниеПриОтключении = ConvertToUInt(abonent.ПОКАЗАНИЯ);

                meter.Задолженник = abonent.PR_ZD;

                meter.КолвоЧеловек = ConvertToByte(abonent.ЧЛЕНОВ);

                #endregion

                #region Счётчик

                meter.ШифрТарифа = abonent.ШИФР;
                meter.Контролёр = this.GetDictionaryValue(this.dictionaryASKONTR, abonent.КОД_КОН)?.ФАМИЛИЯ;

                #region Оплата

                meter.ПериодПослОплаты = ConvertToDateOnly(abonent.YEARMON);
                meter.Среднее = abonent.СРЕДНЕЕ;
                meter.Месяц = abonent.МЕСЯЦ;

                meter.ДатаОплаты = ConvertToDateOnly(abonent.DATE_R);
                meter.СуммаОплаты = ConvertToFloat(abonent.SUMMA_KN + abonent.SUMMA_KC);

                meter.ДолгРуб = ConvertToFloat(abonent.ERRSUM);

                meter.ErrSumN = ConvertToFloat(abonent.ERRSUMN);
                meter.ErrSumV = ConvertToFloat(abonent.ERRSUMV);

                meter.ДатаОбхода = ConvertToDateOnly(abonent.DATE_KON);

                #endregion

                #region Счётчик-признаки

                meter.Расположение = this.GetDictionaryValue(this.dictionaryKartTpr, abonent.COD_TPR)?.TPRIEM;
                meter.Использование = this.GetDictionaryValue(this.dictionaryKartIsp, abonent.COD_ISP)?.ISPIEM;

                #endregion

                #region Привязка

                if (this.dictionaryKartfid.ContainsKey(abonent.FIDER10))
                {
                    Kartfid fider10 = this.dictionaryKartfid[abonent.FIDER10];
                    string s = fider10.ФИДЕР;
                    meter.Фидер10 = string.IsNullOrWhiteSpace(s) ? string.Empty : fider10.НАИМЕНОВ + "-" + s;
                    meter.Подстанция = this.GetDictionaryValue(this.dictionaryKartps, fider10.ПОДСТАНЦИЯ)?.НАИМЕНОВ;
                }
                else
                {
                    this.errors.Add($"Не найдена информация по фидеру 10 кВ: л/с {meter.Лицевой}, код фидера {abonent.FIDER10}");
                }

                if (int.TryParse(abonent.НОМЕР_ТП, out int n) == false && abonent.IsDeleted == false)
                {
                    this.errors.Add($"Ошибка в номере ТП (поле НОМЕР_ТП) '{abonent.НОМЕР_ТП}' (Лицевой счет абонента={abonent.LIC_SCH})");
                }

                if (this.dictionaryKartktp.ContainsKey(n))
                {
                    string nameTp = this.GetDictionaryValue(this.dictionaryKartktp, n)?.НАИМ_ТП;
                    int? numberTp = this.GetDictionaryValue(this.dictionaryKartktp, n)?.НОМЕР_ТП;
                    string ss = this.GetDictionaryValue(this.dictionaryKartktp, n)?.PR_GS;
                    meter.ТП = new TransformerSubstation(nameTp, numberTp.GetValueOrDefault(), ss);
                }

                meter.Фидер04 = ConvertToByte(abonent.ФИДЕР);
                meter.Опора = abonent.НОМЕР_ОПОР;

                #endregion

                #region Признаки

                string priznak = string.IsNullOrWhiteSpace(abonent.PRIZNAK) ? string.Empty : abonent.PRIZNAK;
                if (priznak.Length is > 0 and not 15)
                {
                    System.Diagnostics.Debugger.Break();
                }

                BitArray bitArray = new BitArray(15);
                for (byte index = 0; index < priznak.Length; index++)
                {
                    char bit = priznak[index];

                    if (bit == '1')
                    {
                        meter.Signs = (MeterSigns)((int)meter.Signs | (1 << index));
                    }
                }

                meter.Договор = string.IsNullOrWhiteSpace(abonent.DOG) == false && (abonent.DOG == "1" || abonent.DOG == "д");
                meter.ДатаДоговора = abonent.DATE_DOG.HasValue ? ConvertToDateOnly(abonent.DATE_DOG) : null;

                meter.ПринадлежностьРуп = abonent.PR_MO;
                meter.Льгота = (ushort)abonent.ЛЬГОТА;

                meter.РаботникБелЭнерго = abonent.RABOT;

                #endregion

                #region Счёт

                meter.Лицевой = ConvertToULong(abonent.LIC_SCH);
                meter.ЛицевойЖКХ = ConvertToULong(abonent.GKH_L1);

                #endregion

                if (meterData != null)
                {
                    KARTTSCH meterInfo = this.GetDictionaryValue(this.dictionaryKARTTSCH, meterData.COD_TSCH);
                    if (meterInfo != null)
                    {
                        meter.ТипСчетчика = meterInfo.NAME;
                        meter.Ампераж = meterInfo.TOK;
                        meter.ПериодПоверки = ConvertToByte(meterInfo.PERIOD, 0, 1, 0);
                        meter.Фаз = (byte)meterInfo.ФАЗ;
                        meter.Принцип = meterInfo.TIP == "И" ? "индукционный" : (meterInfo.TIP == "Э" ? "электронный" : "неизвестный");
                        meter.Разрядность = ConvertToByte(meterInfo.ЗНАК);
                    }

                    meter.ПоследнееОплаченноеПоказание = ConvertToUInt(meterData.DATA_NEW);
                    meter.ПредыдущеОплаченноеПоказание = ConvertToUInt(meterData.DATA_OLD);

                    meter.РасчПоказание = abonent.RACHPOK;
                    meter.ПослПоказаниеОбхода = ConvertToUInt(abonent.DATA_KON);

                    meter.НомерСчетчика = meterData.N_SCH;
                    string god = meterData.GODVYPUSKA.ToString();
                    meter.ГодВыпуска = ConvertToUShort(god);

                    meter.ДатаУстановки = ConvertToDateOnly(meterData.DUSTAN);
                    meter.ПоказаниеПриУстановке = ConvertToUInt(meterData.PUSTAN);

                    meter.НомераПломб = meterData.N_PLOMB + "; " + meterData.PLOMB_GS;
                    meter.Мощность = ConvertToFloat(meterData.POWERS);

                    #region Счётчик-признаки

                    meter.МестоУстановки = this.GetDictionaryValue(this.dictionaryASVIDYST, meterData.COD_SS)?.MESTO;
                    meter.Аскуэ = abonent.ASKUE;

                    #endregion

                    #region Поверка

                    meter.КварталПоверки = ConvertToByte(meterData.G_PROV, 0, 1, 1);
                    meter.ГодПоверки = ConvertToByte(meterData.G_PROV, 2, 2, 0);

                    #endregion
                }
                else
                {
                    meter.ТипСчетчика = UNKNOWN_STR;
                    meter.Ампераж = UNKNOWN_STR;
                    meter.ПериодПоверки = 0;
                    meter.Фаз = 1;
                    meter.Принцип = UNKNOWN_STR;
                    meter.НомерСчетчика = UNKNOWN_STR;
                    meter.МестоУстановки = UNKNOWN_STR;
                }

                #endregion
            }
            catch (Exception ex)
            {
                logger?.Error($">>> TMP.WORK.AramisChetchiki.Repository>GetSelectedDepartamentData->AbonentsTable\n{meterData}\n>>>: {TMPApp.GetExceptionDetails(ex)}");
            }

            return meter;
        }

        /// <summary>
        /// получение замен счётчиков
        /// </summary>
        /// <returns></returns>
        private IList<ChangeOfMeter> GetChangesOfMeters()
        {
            string taskName = "таблица замен счётчиков";
            Model.WorkTask workTask = new(taskName);
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            string fileName = Path.Combine(this.dataFilesPath, "ChangesOfMeters" + DATA_FILE_EXTENSION);

            workTask.UpdateStatus($"загрузка данных из кэша ...");
            workTask.IsIndeterminate = true;

            ChangeOfMeter[] result;
            result = this.DeserializeDataAsync<ChangeOfMeter>(fileName).Result;
            if (result != null)
            {
                List<ChangeOfMeter> data = new(result);
                return this.SortData(data, workTask);
            }
            else
            {
                int processedRows = 0;
                int totalRows = 0;

                this.dictionaryAssmena = new();

                System.Collections.Concurrent.ConcurrentBag<Assmena> tableAssmena = new System.Collections.Concurrent.ConcurrentBag<Assmena>();
                using DBF.DbfReader dbfReader = new DbfReader(Path.Combine(this.pathDBF, "Assmena.DBF"), System.Text.Encoding.GetEncoding(866), skipDeletedRecords: true);
                {
                    processedRows = 0;
                    totalRows = dbfReader.DbfTable.Header.NumberOfRecords;
                    int currYear = DateTime.Now.Year;
                    workTask.UpdateStatus($"количество строк в таблице: {totalRows:N0}");

                    tableAssmena = new System.Collections.Concurrent.ConcurrentBag<Assmena>();

                    void toParse(DbfRecord record)
                    {
                        if (record != null)
                        {
                            tableAssmena.Add(this.ParseAssmenaRecord(record));

                            workTask.UpdateUI(++processedRows, totalRows, stepNameString: "строка");

                            workTask.SetProgress(totalRows, processedRows);
                        }
                    }

                    dbfReader.ParseRecords(toParse);
                    workTask.UpdateUI(totalRows, totalRows, stepNameString: "строка");
                }

                workTask.UpdateStatus("анализ полученных данных...");
                foreach (Assmena item in tableAssmena)
                {
                    if (this.dictionaryAssmena.ContainsKey(item.ЛИЦ_СЧЕТ))
                    {
                        this.dictionaryAssmena[item.ЛИЦ_СЧЕТ].Add(item);
                    }
                    else
                    {
                        this.dictionaryAssmena.Add(item.ЛИЦ_СЧЕТ, new List<Assmena>(new[] { item }));
                    }
                }

                processedRows = 0;
                totalRows = this.dictionaryAssmena.Count;

                workTask.UpdateStatus($"количество замен: {totalRows:N0}");
                System.Collections.Concurrent.BlockingCollection<ChangeOfMeter> changes = new();

                Parallel.ForEach(this.dictionaryAssmena.Values, changeOfMeterRow =>
                {
                    ChangeOfMeter changesOfMeters;
                    if (changeOfMeterRow.Count == 1)
                    {
                        changesOfMeters = this.ParseChangesOfMetersRecord(changeOfMeterRow.First());
                        changes.Add(changesOfMeters);
                    }
                    else
                    {
                        foreach (Assmena assmena in changeOfMeterRow)
                        {
                            changesOfMeters = this.ParseChangesOfMetersRecord(assmena);
                            if (changesOfMeters != null)
                            {
                                changes.Add(changesOfMeters);
                            }
                        }
                    }

                    workTask.UpdateUI(++processedRows, totalRows);
                });
                changes.CompleteAdding();

                workTask.UpdateStatus($"сохранение в кэш...");
                workTask.IsIndeterminate = true;
                _ = this.SerializeDataAsync<ChangeOfMeter>(changes.ToArray(), fileName);

                // fix
                workTask.UpdateUI(totalRows, totalRows);

                return this.SortData(changes, workTask);
            }
        }

        /// <summary>
        /// Парсинг записи о замене счетчика
        /// </summary>
        /// <param name="assmena"></param>
        /// <returns></returns>
        private ChangeOfMeter ParseChangesOfMetersRecord(Assmena assmena)
        {
            ChangeOfMeter change = new();
            try
            {
                KARTTSCH meterType = this.GetDictionaryValue(this.dictionaryKARTTSCH, assmena.ТИП_СЧЕТЧ);
                ICollection<KARTSCH> meterInfos = this.dictionaryKARTSCH[assmena.ЛИЦ_СЧЕТ];

                KARTAB abonent = new KARTAB();
                IEnumerable<KARTAB> list = this.collectionKARTAB.Where(i => i.LIC_SCH == assmena.ЛИЦ_СЧЕТ && i.IsDeleted == false);
                if (list.Any())
                {
                    if (list.Count() > 1)
                    {
                        ;
                    }

                    abonent = this.collectionKARTAB.Where(i => i.LIC_SCH == assmena.ЛИЦ_СЧЕТ && i.IsDeleted == false).First();
                }
                else
                {
                    return null;
                }

                ICollection<KARTSCH> old_meter = this.GetDictionaryValue(this.dictionaryKARTSCHRemoved, assmena.ЛИЦ_СЧЕТ);

                change.ТипСнятогоСчетчика = meterType == null ? NOTFOUNDED : meterType.NAME;
                change.СнятЭлектронный = meterType == null ? false : meterType.TIP == "Э";

                if (meterInfos != null && meterInfos.Count != 0)
                {
                    KARTSCH meterInfo1 = meterInfos.First();
                    change.ГодВыпускаУстановленного = ConvertToUShort(meterInfo1.GODVYPUSKA);

                    KARTTSCH meterInfos1 = this.GetDictionaryValue(this.dictionaryKARTTSCH, meterInfo1.COD_TSCH);
                    if (meterInfos1 != null)
                    {
                        change.ТипУстановленногоСчетчика = meterInfos1.NAME;
                        change.УстановленЭлектронный = meterInfos1.TIP == "Э";

                        change.РазрядностьУстановленного = (byte)meterInfos1.ЗНАК;
                    }
                }

                if (old_meter != null && old_meter.Count != 0)
                {
                    KARTSCH meterInfo3 = old_meter.First();
                    KARTTSCH meterInfos3 = this.GetDictionaryValue(this.dictionaryKARTTSCH, meterInfo3.COD_TSCH);

                    change.КварталПоверкиСнятого = ConvertToByte(meterInfo3.G_PROV, 0, 1, 1);
                    change.ГодПоверкиСнятого = ConvertToByte(meterInfo3.G_PROV, 2, 2, 0);
                    change.ГодВыпускаСнятого = ConvertToUShort(meterInfo3.GODVYPUSKA);
                    change.ДатаУстановкиСнятого = ConvertToDateOnly(meterInfo3.DATE_UST);

                    if (meterInfos3 != null)
                    {
                        change.РазрядностьСнятого = (byte)meterInfos3.ЗНАК;
                    }
                }

                KartTn town = this.dictionaryKartTn[abonent.COD_TN];
                string townName = town.TOWN;
                string province = NOTFOUNDED;
                if (this.dictionaryKartSs.ContainsKey(town.COD_SS))
                {
                    province = this.dictionaryKartSs[town.COD_SS].СЕЛЬСОВЕТ;

                    if (string.IsNullOrEmpty(province) && townName != null && townName.StartsWith("г.", AppSettings.StringComparisonMethod))
                    {
                        province = "город";
                    }
                }
                else
                {
                    if (string.Equals(province, NOTFOUNDED) && townName != null && townName.StartsWith("г.", AppSettings.StringComparisonMethod))
                    {
                        province = "город";
                    }
                }

                KartSt street = this.GetDictionaryValue(this.dictionaryKartSt, abonent.COD_ST);

                change.Адрес = new Address(
                        townName,
                        street?.STREET,
                        abonent.HOME,
                        abonent.KV,
                        province);

                string name = abonent.NAME;
                string otch = abonent.OTCH;
                change.Фио = abonent.FAM + " " + (string.IsNullOrEmpty(name) ? string.Empty : name[0] + ".") + (string.IsNullOrEmpty(otch) ? string.Empty : otch[0] + ".");
                change.Фио.Trim();

                if (this.dictionaryKartfid.ContainsKey(abonent.FIDER10))
                {
                    Kartfid fider10 = this.dictionaryKartfid[abonent.FIDER10];
                    string s = fider10.ФИДЕР;
                    change.Фидер10 = string.IsNullOrWhiteSpace(s) ? string.Empty : fider10.НАИМЕНОВ + "-" + s;
                    change.Подстанция = this.GetDictionaryValue(this.dictionaryKartps, fider10.ПОДСТАНЦИЯ)?.НАИМЕНОВ;
                }

                if (int.TryParse(abonent.НОМЕР_ТП, out int n) == false)
                {
                    this.errors.Add($"Ошибка в номере ТП (поле НОМЕР_ТП) '{abonent.НОМЕР_ТП}' (Лицевой счет абонента={abonent.LIC_SCH})");
                }

                if (this.dictionaryKartktp.ContainsKey(n) == false)
                {
                    this.errors.Add($"Справочник ТП не содержит записи (поле НОМЕР_ТП) с номером ТП '{n}' (Лицевой счет абонента={abonent.LIC_SCH})");
                }
                else
                {
                    string typeTp = this.dictionaryKartktp[n].НАИМ_ТП;
                    string nameTp = this.dictionaryKartktp[n].PR_GS;
                    int? numberTp = this.dictionaryKartktp[n].НОМЕР_ТП;
                    change.НомерТП = numberTp.GetValueOrDefault().ToString();
                    change.НаименованиеТП = typeTp + " " + nameTp;
                }

                change.Фидер04 = ConvertToByte(abonent.ФИДЕР);
                change.Опора = abonent.НОМЕР_ОПОР;

                change.Лицевой = ConvertToULong(assmena.ЛИЦ_СЧЕТ);
                change.НомерСнятогоСчетчика = assmena.НОМЕР_СНЯТ;
                change.ПоказаниеСнятого = ConvertToUInt(assmena.ПОКАЗ_СНЯТ);

                change.НомерУстановленногоСчетчика = assmena.НОМЕР_УСТ;
                change.LastReading = ConvertToUInt(assmena.ПОКАЗ_УСТ);
                change.ДатаЗамены = ConvertToDateOnly(assmena.ДАТА_ЗАМЕН);
                change.НомерАкта = ConvertToUInt(assmena.НОМЕР_АКТА);
                change.Фамилия = assmena.ФАМИЛИЯ;
                change.Причина = assmena.ПРИЧИНА;
            }
            catch (Exception ex)
            {
                logger?.Error($">>> ChangesOfMeters ParseRecord >>>: {TMPApp.GetExceptionDetails(ex)}");
            }

            return change;
        }

        /// <summary>
        /// получение полезного отпуска (FORMAB71)
        /// </summary>
        /// <returns></returns>
        private ElectricitySupply ParseElectricitySupplyRecord(DbfRecord record)
        {
            ElectricitySupply electricitySupply = new();
            if (record != null)
            {
                int currYear = DateTime.Now.Year;
                DateOnly period = ConvertToDateOnly(record.GetValue<DateTime>("DATE_N"));
                if (period.Year == currYear || period.Year == currYear - 1)
                {
                    try
                    {
                        electricitySupply.ДатаОплаты = ConvertToDateOnly(record.GetValue<DateTime>("DATE_OPL"));
                        electricitySupply.ОплаченныеПоказания = record.GetValue<int>("DATA_OPL");

                        electricitySupply.Период = period.AddMonths(-1); // оплата на месяц позже
                        electricitySupply.Лицевой = ConvertToULong(record.GetString("LIC_SCH"));
                        electricitySupply.Полезный_отпуск = record.GetValue<int>("POLOT");
                        electricitySupply.Задолженность = record.GetValue<int>("KVT_ZADOL");
                        electricitySupply.Тип_населённого_пункта = record.GetString("TIP");
                    }
                    catch (Exception ex)
                    {
                        logger?.Error($">>> TMP.WORK.AramisChetchiki.Repository>GetSelectedDepartamentData->electricitySupplyTable\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                    }

                    /*
                                                 System.Data.DataRow[] abonents = electricitySupplyRow.GetChildRows("ПО_абонент");

                    if (abonents != null && abonents.Length != 0)
                    {
                        DataRow abonent = abonents[0];
                        electricitySupply.Населённый_пункт = _getChildRelationValue(abonent, "населенный_пункт", "TOWN");
                        electricitySupply.Адрес = string.Join(", ", new string[]
                        {
                    _getChildRelationValue(abonent, "улицы", "STREET"),
                    _getString(abonent["HOME"]),
                    _getString(abonent["KV"])
                        });

                        var fider10Rows = abonent.GetChildRows("фидер10");
                        if (fider10Rows.Length > 0)
                        {
                            var fider10 = fider10Rows[0];
                            s = _getString(fider10["ФИДЕР"]);
                            electricitySupply.Фидер10 = String.IsNullOrWhiteSpace(s) ? String.Empty : _getString(fider10["НАИМЕНОВ"]) + "-" + s;
                            electricitySupply.Подстанция = _getChildRelationValue(fider10, "подстанция", "НАИМЕНОВ");
                        }
                        electricitySupply.ТП = _getChildRelationValue(abonent, "тп", "НАИМЕНОВ");
                        electricitySupply.Фидер04 = _getString(abonent["ФИДЕР"]);
                    }
                     * */
                }
            }
            else
            {
            }

            return electricitySupply;
        }

        /// <summary>
        /// получение информации по оплатам
        /// </summary>
        /// <returns></returns>
        private RawPaymentData ParsePaymentRecord(DbfRecord record)
        {
            RawPaymentData payment = new();

            if (record != null)
            {
                try
                {
                    payment.Лицевой = ConvertToULong(record.GetString("LIC_SCH"));
                    payment.ДатаОплаты = ConvertToDateOnly(record.GetValue<DateTime?>("DATE_R"));
                    payment.ПериодОплаты = ConvertToDateOnly(record.GetValue<DateTime?>("YEARMON"));

                    payment.ПредыдущееПоказание = record.GetValue<int>("DATA_OLD");
                    payment.ПоследнееПоказание = record.GetValue<int>("DATA_NEW");

                    payment.РазностьПоказанийПоКвитанции = record.GetValue<int>("РАЗН_КН") + record.GetValue<int>("РАЗН_КС");
                    payment.РазностьПоказанийРасчётная = record.GetValue<int>("РАЗН_РН") + record.GetValue<int>("РАЗН_РС");

                    payment.ВеличинаТарифа = (float)record.GetValue<decimal>("TAR_KN");

                    payment.СуммаОплаты = (float)record.GetValue<decimal>("SUMMA_KN") + (float)record.GetValue<decimal>("SUMMA_KC");

                    payment.СуммаОплатыРасчётная = (float)record.GetValue<decimal>("SUMMA_RN") + (float)record.GetValue<decimal>("SUMMA_RC") + (float)record.GetValue<decimal>("PENYA_R");

                    payment.ПеняВыставленая = (float)record.GetValue<decimal>("PENYA_R");
                    payment.ПеняОплаченная = (float)record.GetValue<decimal>("PENYA_K");
                }
                catch (Exception ex)
                {
                    logger?.Error($">>> AramisDBParser > ParsePaymentRecord\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                }
            }
            else
            {
            }

            return payment;
        }

        /// <summary>
        /// получение контрольных показаний по лицевому счету
        /// </summary>
        /// <returns></returns>
        private ControlData ParseControlData(DbfRecord record)
        {
            ControlData controlData = new();

            if (record != null)
            {
                controlData.Лицевой = ConvertToULong(record.GetString("LIC_SCH"));
                controlData.Оператор = record.GetString("ОПЕРАТОР");

                List<MeterControlData> list = new List<MeterControlData>(12);

                try
                {
                    DateOnly date;
                    int? value;
                    for (int ind = 1; ind <= 12; ind++)
                    {
                        date = ConvertToDateOnly(record.GetValue<DateTime?>($"DATE_{ind}"));
                        value = record.GetValue<int>($"DATA_{ind}");

                        if (date != default && value.HasValue)
                        {
                            list.Add(new MeterControlData(date, ConvertToUInt(value.Value), string.Empty));
                        }
                    }

                    controlData.Data = list;
                }
                catch (Exception ex)
                {
                    logger?.Error($">>> AramisDBParser > ParseControlData\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                }
            }
            else
            {
            }

            return controlData;
        }

        /// <summary>
        /// справочник установленных счётчиков
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private KARTSCH ParseKARTSCHRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("LIC_SCH"),
            record.GetString("N_SCH"),
            record.GetString("COD_TSCH"),
            record.GetString("G_PROV"),
            ConvertToDateTime(record.GetValue<DateTime?>("DUSTAN")),
            record.GetValue<int?>("DATA_OLD"),
            record.GetValue<int?>("DATA_NEW"),
            record.GetString("N_PLOMB"),
            record.GetValue<int?>("GODVYPUSKA"),
            record.GetValue<decimal?>("POWERS"),
            record.GetString("COD_SS"),
            ConvertToDateTime(record.GetValue<DateTime?>("DATE_FAZ")),
            record.GetString("PLOMB_GS"),
            record.GetString("MATER"),
            record.GetValue<int?>("PUSTAN"),
            record.GetValue<int?>("DATA_UST"),
            ConvertToDateTime(record.GetValue<DateTime?>("DATE_UST")));
        }

        private Kartfid ParseKartfidRecord(DbfRecord record)
        {
            return new
            (
            record.GetValue<int>("ПОДСТАНЦИЯ"),
            record.GetString("ФИДЕР"),
            record.GetString("НАИМЕНОВ"),
            record.GetString("НАИМ_ПОД"));
        }

        private ASKONTR ParseASKONTRRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("КОД_КОН"),
            record.GetString("ФАМИЛИЯ"));
        }

        private Kartktp ParseKartktpRecord(DbfRecord record)
        {
            return new
            (
            record.GetValue<int>("КОД_ТП"),
            record.GetString("ФИДЕР"),
            record.GetValue<int?>("НОМЕР_ТП"),
            record.GetString("НАИМ_ТП"),
            record.GetValue<int?>("ПОДСТАНЦИЯ"),
            record.GetValue<int?>("РЭС"),
            record.GetString("НАИМЕНОВ"),
            record.GetString("PR_GS"));
        }

        private Kartps ParseKartpsRecord(DbfRecord record)
        {
            return new
            (
            record.GetValue<int>("ПОДСТАНЦИЯ"),
            record.GetValue<int?>("РЭС"),
            record.GetString("НАИМЕНОВ"));
        }

        private KartKat ParseKartKatRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_KAT"),
            record.GetString("KATEGAB"));
        }

        private KartIsp ParseKartIspRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_ISP"),
            record.GetString("ISPIEM"));
        }

        private KartTpr ParseKartTprRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_TPR"),
            record.GetString("TPRIEM"));
        }

        private KartSt ParseKartStRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_ST"),
            record.GetString("STREET"));
        }

        private KartSs ParseKartSsRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_SS"),
            record.GetString("СЕЛЬСОВЕТ"));
        }

        private KartTn ParseKartTnRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_TN"),
            record.GetString("TOWN"),
            record.GetString("COD_SS"));
        }

        private ASVIDYST ParseASVIDYSTRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_SS"),
            record.GetString("MESTO"));
        }

        private KARTTSCH ParseKARTTSCHRecord(DbfRecord record)
        {
            return new
            (
            record.GetString("COD_TSCH"),
            record.GetString("NAME"),
            record.GetString("TOK"),
            record.GetString("PERIOD"),
            record.GetString("TIP"),
            record.GetValue<int>("ФАЗ"),
            record.GetValue<decimal>("ЗНАК"));
        }

        private KARTAB ParseKARTABRecord(DbfRecord record)
        {
            return new KARTAB
            {
                IsDeleted = record.IsDeleted,
                LIC_SCH = record.GetString("LIC_SCH"),
                FAM = record.GetString("FAM"),
                NAME = record.GetString("NAME"),
                OTCH = record.GetString("OTCH"),
                SMS = record.GetString("SMS"),
                COD_TN = record.GetString("COD_TN"),
                COD_ST = record.GetString("COD_ST"),
                HOME = record.GetString("HOME"),
                KV = record.GetString("KV"),

                RABOT = record.GetValue<bool>("RABOT"),

                ЧЛЕНОВ = record.GetValue<int?>("ЧЛЕНОВ"),
                TELEF = record.GetString("TELEF"),
                COD_KAT = record.GetString("COD_KAT"),
                COD_TPR = record.GetString("COD_TPR"),
                ШИФР = record.GetString("ШИФР"),
                COD_PD = record.GetString("COD_PD"),
                DKONT = record.GetValue<decimal?>("DKONT"),
                KOMENT = record.GetValue<string>("KOMENT"),
                PLIT = record.GetValue<bool>("PLIT"),

                YEARMON = record.GetValue<DateTime?>("YEARMON"),
                ERRSUM = record.GetValue<decimal?>("ERRSUM"),
                ERRSUMN = record.GetValue<decimal?>("ERRSUMN"),
                SUMMA_KN = record.GetValue<decimal?>("SUMMA_KN"),
                SUMMA_KC = record.GetValue<int?>("SUMMA_KC"),
                КОД_КОН = record.GetString("КОД_КОН"),
                СРЕДНЕЕ = record.GetValue<int?>("СРЕДНЕЕ"),
                МЕСЯЦ = record.GetValue<int?>("МЕСЯЦ"),
                ДАТА_ОТКПЛ = record.GetValue<DateTime?>("ДАТА_ОТКПЛ"),
                ДАТА_ОТКФК = record.GetValue<DateTime?>("ДАТА_ОТКФК"),
                НОМЕР_ТП = record.GetString("НОМЕР_ТП"),
                ФИДЕР = record.GetValue<int?>("ФИДЕР"),
                НОМЕР_ОПОР = record.GetString("НОМЕР_ОПОР"),
                СМЕНА = record.GetString("СМЕНА"),
                DATA_KON = record.GetValue<int?>("DATA_KON"),
                DATE_KON = record.GetValue<DateTime?>("DATE_KON"),
                ФАМИЛИЯ = record.GetString("ФАМИЛИЯ"),
                PR_OPL = record.GetString("PR_OPL"),
                COD_PRED = record.GetString("COD_PRED"),
                DATE_ZAP = record.GetValue<DateTime?>("DATE_ZAP"),
                COD_VID = record.GetString("COD_VID"),
                ПОКАЗАНИЯ = record.GetValue<int?>("ПОКАЗАНИЯ"),
                DATE_LGT = record.GetValue<DateTime?>("DATE_LGT"),
                ЛЬГОТА = record.GetValue<int?>("ЛЬГОТА"),
                ПРОЦЕНТ = record.GetValue<int?>("ПРОЦЕНТ"),
                PR_VN = record.GetValue<bool>("PR_VN"),
                PR_VO = record.GetValue<bool>("PR_VO"),
                PR_MO = record.GetValue<bool>("PR_MO"),
                PR_ZD = record.GetValue<bool>("PR_ZD"),
                DATE_R = record.GetValue<DateTime?>("DATE_R"),
                RACHPOK = record.GetValue<int?>("RACHPOK"),
                PENYA_T = record.GetValue<decimal?>("PENYA_T"),
                COD_ISP = record.GetString("COD_ISP"),
                DOG = record.GetString("DOG"),
                DATE_DOG = record.GetValue<DateTime?>("DATE_DOG"),
                FIDER10 = record.GetString("FIDER10"),
                ASKUE = record.GetValue<bool>("ASKUE"),
                ERRSUMV = record.GetValue<decimal?>("ERRSUMV"),
                KVT_LGT = record.GetValue<int?>("KVT_LGT"),
                // field 'PRIZNAK' not allowed to trim
                PRIZNAK = record.GetValue<string>("PRIZNAK"),
                GKH_L = record.GetString("GKH_L"),
                GKH_L1 = record.GetString("GKH_L1"),
            };
        }

        private Assmena ParseAssmenaRecord(DbfRecord record)
        {
            return new
                (
                    record.GetString("ЛИЦ_СЧЕТ"),
                    record.GetString("ТИП_СЧЕТЧ"),
                    record.GetString("НОМЕР_СНЯТ"),
                    record.GetValue<int>("ПОКАЗ_СНЯТ"),
                    record.GetString("НОМЕР_УСТ"),
                    record.GetValue<int>("ПОКАЗ_УСТ"),
                    record.GetValue<DateTime?>("ДАТА_ЗАМЕН"),
                    record.GetValue<int?>("НОМЕР_АКТА"),
                    record.GetString("ФАМИЛИЯ"),
                    record.GetString("ПРИЧИНА"));
        }

        private KartPd ParseKartPdRecord(DbfRecord record)
        {
            return new
                (
                    record.GetString("COD_PD"),
                    record.GetString("SNPD"),
                    record.GetString("PNPD"));
        }

        private RemovAb ParseRemovAbRecord(DbfRecord record)
        {
            return new
                (
                    ConvertToULong(record.GetString("LIC_SCH")),
                    record.GetString("FAM"),
                    record.GetString("NAME"),
                    record.GetString("OTCH"),
                    ConvertToDateOnly(record.GetValue<DateTime?>("DATE_ZAP"))
                    );
        }

        #endregion
    }
}
