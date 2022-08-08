using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TMP.Work.Emcos
{
    using System.Linq;
    using TMP.Work.Emcos.Model;

    public class Utils
    {
        public static NameValueCollection ParsePairs(string data)
        {
            if (String.IsNullOrWhiteSpace(data)) return null;
            var result = new NameValueCollection();

            // записи вида ключ=значение разделены символом &
            var records = data.Split(new char[] { '&' });
            // по всем записям
            for (int i = 0; i < records.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(records[i])) // пустая запись
                    continue;
                var kvp = ParseKeyValuePair(records[i]);
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }

        public static List<NameValueCollection> ParseRecords(string data)
        {
            if (String.IsNullOrWhiteSpace(data)) return null;

            // записи вида ключ=значение разделены символом &
            var records = data.Split(new char[] { '&' });

            // записи содержат результат выполнения запроса (result=0 или result=?) и количество записей (recordCount=N)
            bool resultok = false;
            // запись с количеством
            var recCount = "";
            foreach (string record in records)
            {
                if (record == "result=0")
                    resultok = true;
                if (record.StartsWith("recordCount"))
                    recCount = record;
            }
            if (resultok == false)
                return null;
            int recordsCount = 0;
            // количество записей
            Int32.TryParse(ParseKeyValuePair(recCount).Value, out recordsCount);

            // ключ каждой записи имеет окончание вида _?, где ? указывает на порядковый номер объекта

            // список объектов, каждый из которых имеет список записей в виде пар ключ-значение
            var result = new List<NameValueCollection>();
            int index = 0;
            // по всем записям
            for (int i = 0; i < recordsCount; i++)
            {
                // список пар ключ-значение текущего объекта
                var list = new NameValueCollection();
                while (index < records.Length)
                {
                    if (String.IsNullOrWhiteSpace(records[index]) // пустая запись
                        || records[index].StartsWith("result")    // результат выполнения
                        || records[index].StartsWith("record"))  // количество записей
                    {
                        index++;
                        continue;
                    }
                    // порядковый номер текущего объекта
                    var currentRecordIndexAsString = i.ToString();
                    // пара ключ-значение из текущей записи
                    var kvp = ParseKeyValuePair(records[index]);
                    // если пара относится к текущему объекту
                    if (kvp.Key.EndsWith(currentRecordIndexAsString) == true)
                    {
                        // убираем окончание _?, где ? число
                        var key = kvp.Key;
                        int lastind = key.LastIndexOf("_");
                        key = key.Remove(lastind);

                        // добавляем в список
                        list.Add(key, kvp.Value);
                        index++;
                    }
                    else
                        break;
                }
                result.Add(list);
            }
            return result;
        }

        public static KeyValuePair<string, string> ParseKeyValuePair(string data)
        {
            var parts = data.Split(new char[] { '=' });
            if (parts.Length != 2) throw new ArgumentException();

            return new KeyValuePair<string, string>(parts[0], parts[1]);
        }
        /// <summary>
        /// Разбор измерений
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ICollection<Model.IParam> Params(string data)
        {
            var list = Utils.ParseRecords(data);

            if (list == null)
                return null;

            ICollection<Model.IParam> result = new List<Model.IParam>();

            foreach (var nvc in list)
            {
                var nodetype = nvc.Get("TYPE");
                if (String.IsNullOrWhiteSpace(nodetype))
                    return null;
                // &MS_TYPE_ID_0=1&MS_TYPE_NAME_0=Электричество&TYPE_0=MS_TYPE&result=0&recordCount=1
                switch (nodetype)
                {
                    case "MS_TYPE":
                        var ms_param = new Model.MS_Param();
                        result.Add(ms_param);
                        break;
                    case "AGGS_TYPE":
                        var aggs_param = new Model.AGGS_Param();

                        aggs_param.Id = nvc.Get("AGGS_TYPE_ID");
                        aggs_param.Name = nvc.Get("AGGS_TYPE_NAME");

                        result.Add(aggs_param);
                        break;
                    case "MSF":
                        var msf_param = new Model.MSF_Param();

                        msf_param.Id = nvc.Get("MSF_ID");
                        msf_param.Name = nvc.Get("MSF_NAME");
                        msf_param.AGGS.Id = nvc.Get("AGGS_TYPE_ID");

                        result.Add(msf_param);
                        break;
                    case "ML":
                        var ml_param = new Model.ML_Param();
                        if (nvc.Get("ML_ID") != null)
                            ml_param.Id = nvc.Get("ML_ID");
                        if (nvc.Get("ID") != null)
                            ml_param.Id = nvc.Get("ID");
                        if (nvc.Get("ML_NAME") != null)
                            ml_param.Name = nvc.Get("ML_NAME");
                        if (nvc.Get("NAME") != null)
                            ml_param.Name = nvc.Get("NAME");

                        ml_param.AGGF.Id = nvc.Get("AGGF_ID");
                        ml_param.AGGF.Name = nvc.Get("AGGF_NAME");

                        ml_param.DIR.Id = nvc.Get("DIR_ID");
                        ml_param.DIR.Name = nvc.Get("DIR_NAME");
                        ml_param.DIR.Code = nvc.Get("DIR_CODE");

                        ml_param.EU.Id = nvc.Get("EU_ID");
                        ml_param.EU.Name = nvc.Get("EU_NAME");
                        ml_param.EU.Code = nvc.Get("EU_CODE");

                        ml_param.TFF.Id = nvc.Get("TFF_ID");
                        ml_param.TFF.Name = nvc.Get("TFF_NAME");

                        ml_param.AGGS.Id = nvc.Get("AGGS_ID");
                        ml_param.AGGS.Name = nvc.Get("AGGS_NAME");
                        ml_param.AGGS.Value = nvc.Get("AGGS_VALUE");
                        ml_param.AGGS.Per_Id = nvc.Get("AGGS_PER_ID");
                        ml_param.AGGS.Per_Code = nvc.Get("AGGS_PER_CODE");
                        ml_param.AGGS.Per_Name = nvc.Get("AGGS_PER_NAME");

                        ml_param.MS.Id = nvc.Get("MS_ID");
                        ml_param.MS.Name = nvc.Get("MS_NAME");
                        ml_param.MS.Code = nvc.Get("MS_CODE");

                        ml_param.MD.Id = nvc.Get("MD_ID");
                        ml_param.MD.Name = nvc.Get("MD_NAME");
                        ml_param.MD.Code = nvc.Get("MD_CODE");
                        ml_param.MD.Per_Id = nvc.Get("MD_PER_ID");
                        ml_param.MD.Per_Code = nvc.Get("MD_PER_CODE");
                        ml_param.MD.Per_Name = nvc.Get("MD_PER_NAME");
                        ml_param.MD.Int_Value = nvc.Get("MD_INT_VALUE");

                        ml_param.MSF.Id = nvc.Get("MSF_ID");
                        ml_param.MSF.Name = nvc.Get("MSF_NAME");
                        ml_param.MSF.Code = nvc.Get("MSF_CODE");

                        ml_param.OBIS = nvc.Get("OBIS");

                        result.Add(ml_param);
                        break;
                }
            }
            return result;
        }
        /// <summary>
        /// Разбор ар
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ICollection<Model.ArchAP> ArchAPs(string data)
        {
            var list = Utils.ParseRecords(data);

            if (list == null)
                return null;

            ICollection<Model.ArchAP> result = new List<Model.ArchAP>();

            foreach (var nvc in list)
            {
                var nodetype = nvc.Get("TYPE");
                if (String.IsNullOrWhiteSpace(nodetype))
                    return null;
                var ap = new Model.ArchAP();

                int intValue = 0;

                ap.Type = nvc.Get("TYPE");
                int.TryParse(nvc.Get("ID"), out intValue);
                ap.Id = intValue;
                ap.Name = nvc.Get("NAME");
                intValue = 0;
                switch (nodetype)
                {
                    case "GROUP":
                        var group = new Model.ArchGroup();
                        int.TryParse(nvc.Get("ID"), out intValue);
                        group.Id = intValue;
                        group.Name = nvc.Get("GR_NAME");
                        group.Code = nvc.Get("GR_CODE");
                        group.Code = nvc.Get("GR_TYPE_NAME");

                        ap.AP = group;
                        break;
                    case "POINT":
                        var point = new Model.ArchPoint();
                        int.TryParse(nvc.Get("ID"), out intValue);
                        point.Id = intValue;
                        point.Name = nvc.Get("POINT_NAME");
                        point.CODE = nvc.Get("POINT_CODE");
                        point.CODE = nvc.Get("POINT_TYPE_NAME");
                        point.Ecp_Name = nvc.Get("ECP_NAME");

                        ap.AP = point;
                        break;
                }
                result.Add(ap);
            }
            return result;
        }
        /// <summary>
        /// Разбор архивных значений
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IEnumerable<Model.ArchData> ArchiveData(string data)
        {
            var list = Utils.ParseRecords(data);

            if (list == null)
                return null;

            IList<Model.ArchData> result = new List<Model.ArchData>();

            foreach (var nvc in list)
            {
                var item = new Model.ArchData
                {
                    MONTH = nvc.Get("MONTH"),
                    DA = nvc.Get("DA"),
                    H = nvc.Get("H"),
                    BT = nvc.Get("BT"),
                    ET = nvc.Get("ET"),
                    I = nvc.Get("I"),
                    DR = nvc.Get("DR"),
                    D = nvc.Get("D"),
                    READ_TIME = nvc.Get("READ_TIME"),
                    HSS = nvc.Get("HSS"),
                    SFS = nvc.Get("SFS"),
                    HAS_ACT = nvc.Get("HAS_ACT"),
                    TFF_ID = nvc.Get("TFF_ID"),
                    E_BT = nvc.Get("E_BT")
                };

                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Определяет содержит ли указанный интервал дат месяц
        /// </summary>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">Конечная дата</param>
        /// <returns>True если это интервал за месяц</returns>
        public static bool HasDateIntervalMonth(DateTime startDate, DateTime endDate)
        {
            var nextMonthOfEndDate = endDate.AddMonths(1);
            var nextMonthOfEndDate2 = new DateTime(nextMonthOfEndDate.Year, nextMonthOfEndDate.Month, 1);
            int lastDayInEndDateMonth = nextMonthOfEndDate2.AddDays(-1).Day;
            return startDate.Day == 1 && endDate.Day == lastDayInEndDateMonth;
        }

        /// <summary>
        /// Получение архивных данных по указанной подстанции
        /// </summary>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">Конечная дата</param>
        /// <param name="substation">Подстанция</param>
        /// <param name="cts">Маркер отмены</param>
        /// <param name="callback">Функция обратного вызова, для информирования о прогрессе</param>
        /// <returns>True если успешно</returns>
        public static bool GetArchiveDataForSubstation(DateTime startDate, DateTime endDate,
            Model.Balance.Substation substation, System.Threading.CancellationTokenSource cts, Action<int, int> callback)
        {
            bool error;

            int current = 0, total = 0;

            bool hasMonthData = HasDateIntervalMonth(startDate, endDate);
            // измерения - А энергия за сутки
            var measurementsDaysPart = @"T2_TYPE_0=MSF&T2_NAME_0=А энергия&T2_MSF_ID_0=14&T2_MSF_NAME_0=А энергия&T2_AGGS_TYPE_ID_0=3&T2_MS_TYPE_ID_0=1";
            // А энергия за месяц
            var measurementsMonthPart = @"&T2_TYPE_1=MSF&T2_NAME_1=А энергия&T2_MSF_ID_1=14&T2_MSF_NAME_1=А энергия&T2_AGGS_TYPE_ID_1=4&T2_MS_TYPE_ID_1=1";
            // 
            var measurementsEndPartIfNotHasMonth = "&T2_COUNT=1&";
            var measurementsEndPartIfHasMonth = "&T2_COUNT=2&";

            substation.Status = Model.DataStatus.Processing;
            error = false;

            string[] wids = null;

            if (substation.Children == null || substation.Children.Count == 0)
                error = true;
            else
            {
                if (substation.Items == null)
                {
                    TMPApplication.TMPApp.LogInfo(String.Format("Подстанция <{0}> не имеет точек.", substation.Name));
                    error = true;
                }
                else
                {
                    try
                    {
                        var items = substation.Items.Cast<Model.IEmcosPoint>();
                        string points = EmcosSiteWrapper.Instance.CreatePointsParam(items);
                        wids = EmcosSiteWrapper.Instance.GetArchiveWIds(
                            hasMonthData
                              ? measurementsDaysPart + measurementsMonthPart + measurementsEndPartIfHasMonth
                              : measurementsDaysPart + measurementsEndPartIfNotHasMonth,
                            points, startDate, endDate, cts.Token);
                    }
                    catch (Exception e)
                    {
                        TMPApplication.TMPApp.LogInfo("GetSubstationsDaylyArchives - ошибка: " + e.Message);
                        error = true;
                    }
                    if (wids == null || wids.Length == 0)
                    {
                        TMPApplication.TMPApp.LogInfo("GetSubstationsDaylyArchives - ошибка получен неверный wid. Подстанция - " + substation.Name);
                        error = true;
                    }
                }
            }
            if (error == false)
            {
                var mls = EmcosSiteWrapper.Instance.GetParamsForArchiveData(wids[0]);

                var m = mls.ToList();

                total = substation.Items.Count;
                // получение архивных данных по каждой из точек
                foreach (Model.Balance.IBalanceItem item in substation.Items)
                {
                    if (item == null)
                        throw new ArgumentNullException();

                    current++;
                    if (callback != null)
                        callback.Invoke(current, total);

                    GetBalanceItemArchiveData(item, startDate, endDate, cts);
                }
            }
            substation.Status = Model.DataStatus.Processed;
            return error == false;
        }
        /// <summary>
        /// Получение архивных данных по указанному элементу
        /// </summary>
        /// <param name="item">Элемент баланса подстанции</param>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">Конечная дата</param>
        /// <param name="cts">Маркер отмены</param>
        public static void GetBalanceItemArchiveData(Model.Balance.IBalanceItem item, DateTime startDate, DateTime endDate, System.Threading.CancellationTokenSource cts)
        {
            bool hasMonthData = HasDateIntervalMonth(startDate, endDate);

            Action<Model.Balance.IDirectedEnergy> getDirectedEnergyValuesForBalanceItem = null;
            getDirectedEnergyValuesForBalanceItem = (Model.Balance.IDirectedEnergy energy) =>
            {
                if (cts.IsCancellationRequested) return;
                IEnumerable<Model.ArchData> data_days, data_month = null;
                // энергия за сутки
                data_days = GetPointArchive(item.Id, item.Name, startDate, endDate, energy.DayMlParam);
                // разбор данных
                energy.DaysValues = ParseArchiveData(data_days);

                // энергия за месяц
                if (hasMonthData)
                {
                    data_month = GetPointArchive(item.Id, item.Name, startDate, endDate, energy.MonthMlParam);
                    if (data_month == null || data_month.Count() != 1)
                        TMPApplication.TMPApp.LogInfo(String.Format("getDirectedEnergyValuesForBalanceItem - данные за месяц. Ошибка в данных (data_month == null || data_month.Count() != 1). Точка: ID={0}, NAME={1}", item.Id, item.Name));
                    // разбор данных
                    energy.MonthValue = ParseArchiveData(data_month).FirstOrDefault();
                }
            };
            try
            {
                item.Status = Model.DataStatus.Processing;
                getDirectedEnergyValuesForBalanceItem(item.ActiveEnergy.Plus);
                getDirectedEnergyValuesForBalanceItem(item.ActiveEnergy.Minus);
                getDirectedEnergyValuesForBalanceItem(item.ReactiveEnergy.Plus);
                getDirectedEnergyValuesForBalanceItem(item.ReactiveEnergy.Minus);
            }
            catch (Exception e)
            {
                item.Status = Model.DataStatus.Processed;
                TMPApplication.TMPApp.LogInfo(String.Format("GetBalanceItemArchiveData. Ошибка получения данных по точке: ID={0}, NAME={1}. Сообщение: {2}",
                            item.Id, item.Name, e.Message));
            }
            finally
            {
                item.Status = Model.DataStatus.Processed;
            }
        }
        /// <summary>
        ///  Получение архивных данных энергии, заданной параметром
        /// </summary>
        /// <param name="pointId">ИД точки</param>
        /// <param name="pointName">Название точки</param>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">Конечная дата</param>
        /// <param name="ml">Параметр, описывающий энергию - тип, направление, а также вид временного интервала</param>
        /// <returns>Список <see cref="Model.ArchData"/> для каждого интервала времени</returns>
        public static IEnumerable<Model.ArchData> GetPointArchive(int pointId, string pointName, DateTime startDate, DateTime endDate, Model.ML_Param ml)
        {
            var point = new Model.ArchAP { Id = pointId, Type = "POINT", Name = pointName };
            IEnumerable<Model.ArchData> list;
            try
            {
                list = EmcosSiteWrapper.Instance.GetArchiveData(ml, point, startDate, endDate).Result;
            }
            catch (Exception e)
            {
                TMPApplication.TMPApp.LogInfo(String.Format("GetPointArchive. Ошибка получения данных по точке: ИД-[{0}], название-[{1}]. Сообщение: {2}",
                    pointId, pointName, e.Message));
                return null;
            }
            return list;
        }
        /// <summary>
        /// Преобразование архивных данных энергии в список значений
        /// </summary>
        /// <param name="values">Список <see cref="Model.ArchData"/></param>
        /// <returns>Спсиок значений</returns>
        public static IList<double?> ParseArchiveData(IEnumerable<Model.ArchData> values)
        {
            return values
                .Select(d =>
                    d.SFS == "Нормальные данные" || d.SFS.Contains("Ручной ввод")
                        ? Double.Parse(d.D, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)
                        : new Nullable<double>()
                        )
                .ToList();
        }
    }
}