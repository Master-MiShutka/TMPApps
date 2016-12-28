using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TMP.Work.Emcos
{
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
                var item = new Model.ArchData();

                item.MONTH = nvc.Get("MONTH");
                item.DA = nvc.Get("DA");
                item.H = nvc.Get("H");
                item.BT = nvc.Get("BT");
                item.ET = nvc.Get("ET");
                item.I = nvc.Get("I");
                item.DR = nvc.Get("DR");
                item.D = nvc.Get("D");
                item.READ_TIME = nvc.Get("READ_TIME");
                item.HSS = nvc.Get("HSS");
                item.SFS = nvc.Get("SFS");
                item.HAS_ACT = nvc.Get("HAS_ACT");
                item.TFF_ID = nvc.Get("TFF_ID");
                item.E_BT = nvc.Get("E_BT");

                result.Add(item);
            }
            return result;
        }

        /********************************/
        public static string GetData(FrameworkElement sender, EmcosSiteWrapperMethod method, string parametr, Func<string, bool> postAction)
        {
            var stateObj = sender as IStateObject;
            if (stateObj == null)
                throw new ArgumentException("Not a IStateObject");
            var data = String.Empty;
            stateObj.State = State.Busy;
            try
            {
                var response = string.Empty;
                var task = method(parametr);
                if (task == null) return null;

                task.
                ContinueWith<String>((t) =>
                {
                    MessageBox.Show(t.Exception.InnerException.Message);
                    stateObj.State = State.Idle;
                    return null;
                },
                    System.Threading.CancellationToken.None,
                    System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted,
                    System.Threading.Tasks.TaskScheduler.Current
                    );
                task.
                ContinueWith<String>(t =>
                {
                    response = t.Result;

                    if (String.IsNullOrWhiteSpace(response))
                    {
                        sender.Dispatcher.Invoke((Action)(() =>
                                MessageBox.Show("Не удалось получить данные!", "Проблема с доступом", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                    }
                    if (response.Contains("result=1"))
                    {
                        if (EmcosSiteWrapper.Instance.Login(EmcosSiteWrapper.AuthorizationType.Login) == true)
                        {
                            if (EmcosSiteWrapper.Instance.Login(EmcosSiteWrapper.AuthorizationType.GetRights) == false)
                                sender.Dispatcher.Invoke((Action)(() =>
                                MessageBox.Show("Не удалось авторизоваться!", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                        }
                        else
                            response = GetData(sender, method, parametr, postAction);
                    }
                    else
                    {
                        if (response.Contains("result=0") == false)
                            sender.Dispatcher.Invoke((Action)(() =>
                                MessageBox.Show("Данные не получены!\nКод результата:\n" + response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                        else
                        {
                            postAction(response);
                        }
                    }
                    stateObj.State = State.Idle;
                    return response;
                },
                    System.Threading.CancellationToken.None,
                    System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion,
                    System.Threading.Tasks.TaskScheduler.Current
                    );

                if (String.IsNullOrWhiteSpace(response))
                    response = task.Result;

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                stateObj.State = State.Idle;
                return null;
            }
        }

    }
}