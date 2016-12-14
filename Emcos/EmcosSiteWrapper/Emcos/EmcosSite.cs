using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Text;

namespace TMP.Work.Emcos
{
    using TMP.Common.Logger;
    public class EmcosSite
    {
        private Action<string> _checkRightsAction;

        public Exception LastException { get; set; }
        public void CheckRights(Action<string> action)
        {
            _checkRightsAction = action;
            var request = System.Net.WebRequest.Create("http://10.96.18.16/emcos/scripts/autentification.asp") as System.Net.HttpWebRequest;
            request.Method = "POST";
            request.Accept = "*/*";
            request.Headers.Add("Accept-Encoding: gzip, deflate");
            request.ContentType = "application/x-www-form-urlencoded";
            if (String.IsNullOrWhiteSpace(EmcosSiteWrapper.Instance.Cookie.Value) == false)
            {
                request.CookieContainer = new System.Net.CookieContainer(1);
                request.CookieContainer.Add(EmcosSiteWrapper.Instance.Cookie);
            }
            request.BeginGetRequestStream(U.SyncContextCallback(ProcessRequest), request);
        }
        private void ProcessRequest(IAsyncResult result)
        {
            var request = (System.Net.HttpWebRequest)result.AsyncState;
            System.IO.Stream reqStream = null;
            reqStream = request.EndGetRequestStream(result);
            var data = string.Format("action=getrights");
            var buffer = System.Text.Encoding.ASCII.GetBytes(data);
            //request.ContentLength = buffer.Length;

            reqStream.Write(buffer, 0, buffer.Length);
            reqStream.Close();

            request.BeginGetResponse(U.SyncContextCallback(ProcessResponse), request);
        }

        private void ProcessResponse(IAsyncResult result)
        {
            var request = (System.Net.HttpWebRequest)result.AsyncState;
            using (var response = request.EndGetResponse(result))
            {
                var responseStream = response.GetResponseStream();
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(responseStream, new System.Text.UTF8Encoding()))
                {
                    var answer = streamReader.ReadToEnd();
                    if (_checkRightsAction != null)
                        _checkRightsAction(answer);
                    _checkRightsAction = null;
                }
            }
        }

        public enum State
        {
            Online,
            Offline,
            NotAuthorized,
            Error
        }
        public State Status { get; private set; }
        public Task<State> Login(CancellationToken ct)
        {
            var task = new Task<State>((t) =>
            {
                Status = State.Online;
                bool online = false;
                try
                {
                    online = EmcosSiteWrapper.Instance.IsServerOnline();
                    if (online)
                    {
                        if (EmcosSiteWrapper.Instance.Login(EmcosSiteWrapper.AuthorizationType.Login) == false)
                        {
                            if (EmcosSiteWrapper.Instance.IsSiteOnline() == false)
                                Status = State.Offline;
                            else
                                Status = State.NotAuthorized;
                        }
                    }
                    else
                        Status = State.Offline;
                }
                catch (SystemException se)
                {
                    Status = State.Error;
                    App.Log.Log("Ошибка авторизации: " + se.Message, Category.Exception, Priority.High);
                }

                App.Log.Log("Авторизация - " + Status.ToString(), Category.Info, Priority.None);
                return Status;
            }, ct);

            task.Start();

            return task;
        }

        public System.Threading.Tasks.Task<EmcosSite.State> ExecuteAction(System.Threading.CancellationTokenSource cts, Action action)
        {
            try
            {
                var task = Login(cts.Token);

                var actionTask =

                task.ContinueWith((t) =>
                    {
                        var state = t.Result;
                        switch (state)
                        {
                            case EmcosSite.State.Online:
                                action();
                                break;

                            case EmcosSite.State.Offline:
                                App.Log.Log("Web-сервис Emcos Corporate не доступен!", Category.Warn, Priority.Medium);
                                MessageBox.Show("Web-сервис Emcos Corporate не доступен!\nПроверьте сетевые настройки.",
                                    "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                break;

                            case EmcosSite.State.NotAuthorized:
                                App.Log.Log("Не удалось авторизоваться.", Category.Warn, Priority.High);
                                MessageBox.Show("Не удалось авторизоваться!",
                                    "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                break;
                        }
                        return state;
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);

                task.ContinueWith((s) =>
                {
                    App.Log.Log("Получение данных отменено.", Category.Info, Priority.None);
                    MessageBox.Show("Отменено.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return State.Online;
                },
                  System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled);

                task.ContinueWith((s) =>
                {
                    App.Log.Log("Получение данных. Ошибка авторизации - " + s.Exception.Message, Category.Exception, Priority.High);
                    MessageBox.Show("Произошла ошибка.\n" + s.Exception.Message, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return State.NotAuthorized;
                }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

                return actionTask;
            }
            catch (Exception ex)
            {
                LastException = ex;
                App.Log.Log("Получение данных. Ошибка - " + ex.Message, Category.Exception, Priority.High);
                MessageBox.Show("Произошла ошибка.\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }
        }

        public string[] GetArchiveWIds(string measurements, string points, Model.DatePeriod period, CancellationToken ct, bool procent = false, bool showGroup = false)
        {
            if (EmcosSiteWrapper.Instance.Login(EmcosSiteWrapper.AuthorizationType.GetRights) == false)
            {
                if (EmcosSiteWrapper.Instance.IsSiteOnline() == false)
                    Status = State.Offline;
                else
                    Status = State.NotAuthorized;
                return null;
            }
            else
                Status = State.Online;

            var wids = new List<string>();

            var footer = new StringBuilder();
            footer.AppendFormat("SHOW_VERGES=0&");
            footer.AppendFormat("FROM={0}&", period.StartDate.ToString("yyyy.MM.dd"));
            footer.AppendFormat("TO={0}&", period.EndDate.ToString("yyyy.MM.dd"));
            // для получения значений группы GR_VALUE=VAL
            // для получения процента группы GR_VALUE=VAL_PRC
            footer.AppendFormat("GR_VALUE={0}&", procent ? "VAL_PRC" : "VAL");
            footer.AppendFormat("GMOD_ID=&");
            footer.AppendFormat("GMOD_NAME=Нетрансформировано&");
            footer.AppendFormat("GMOD_EU_CODE=&");
            footer.AppendFormat("GMOD_EU_CODE_USAGE=&");
            footer.AppendFormat("FREEZED=0&");
            footer.AppendFormat("WO_BYP=0&");
            footer.AppendFormat("WO_ACTS=0&");
            footer.AppendFormat("SHOW_MAP_DATA=0&");
            footer.AppendFormat("BILLING_HOUR=0&");
            footer.AppendFormat("VIEWRAW_DATA=0&");
            // если выбрана секция СШ 6-10 кВ wType=groups и запрос просмотра группы значений
            // иначе wType=global
            footer.AppendFormat("wType={0}&", showGroup ? "groups" : "global");
            footer.AppendFormat("dataBlock=ARCHIVES&");
            footer.AppendFormat("action=set");

            var sb = new StringBuilder();
            sb.Append(measurements);
            sb.Append(points);
            sb.Append(footer.ToString());

            var senddata = sb.ToString();

            var response = EmcosSiteWrapper.Instance.GetViewAsync(senddata).Result;
            if (String.IsNullOrWhiteSpace(response))
            {
                Status = State.Error;
                App.Log.Log("Ошибка при получении ИД архива. Нет получен ответ сервера.", Category.Exception, Priority.High);
                return null;
            }
            var list = AnswerParser.ParsePairs(response);
            bool ok = list.Get("result") == "0";
            if (ok == false)
            {
                Status = State.Error;
                App.Log.Log("Ошибка при получении ИД архива. Ответ сервера: " + response, Category.Exception, Priority.High);
                return null;
            }

            //result=0&wID_0=22&wCount=1&specForm=
            //result=0&wID_0=1&wID_1=2&wCount=2&specForm=
            foreach (string key in list.Keys)
                if (key.StartsWith("wID"))
                    wids.Add(list.Get(key));
            int count = 0;
            Int32.TryParse(list.Get("wCount"), out count);
            if (count != wids.Count)
                App.Log.Log("Разное количество ИД архива: " + wids.Count + " и " + count, Category.Info, Priority.None);
            App.Log.Log("Получен ИД архива: " + String.Join(", ", wids), Category.Info, Priority.None);


            Status = State.Online;
            return wids.ToArray();
        }
        public ICollection<Model.ArchData> GetArchiveData(Model.ML_Param param, Model.ArchAP point, Model.DatePeriod period, CancellationToken ct)
        {
            var md = param.MD.Id;
            var aggs = param.AGGS.Id;
            var datacode = param.Id;
            var type = point.Type;
            var id = point.Id;

            var sendData = String.Format(
                "action=GET&dataBlock=DW&BILLING_HOUR=0&FREEZED=0&SHOW_MAP_DATA=0&GR_VALUE={0}&" +
                "EC_ID=&MD_ID={1}&WO_ACTS=0&WO_BYP=0&GMOD_ID=&TimeEnd={3}&TimeBegin={2}&AGGS_ID={4}&DATA_CODE={5}&TYPE={6}&ID={7}",
                "VAL",
                md,
                period.StartDate.ToString("yyyy.MM.dd"),
                period.EndDate.ToString("yyyy.MM.dd"),
                aggs,
                datacode,
                type,
                id);
            sendData = System.Web.HttpUtility.UrlPathEncode(sendData).Replace("_", "%5F").Replace("+", "%2B").ToUpper();
            var data = EmcosSiteWrapper.Instance.ArchiveData.Get(sendData);
            if (String.IsNullOrWhiteSpace(data))
            {
                App.Log.Log("Архивные данные отсутствуют. Точка: " + point.Name, Category.Exception, Priority.High);
                return null;
            }
            var list = AnswerParser.ArchiveData(data);

            return list;
        }

        public string CreatePointsParam(IList<Model.Balans.IBalansItem> points)
        {
            if (points == null || points.Count == 0)
                return null;
            var sb = new StringBuilder();
            int index;
            for (index = 0; index < points.Count; index++)
            {
                var point = points[index];

                bool isGroup = point.Type == Model.ElementTypes.Section && (point as Model.Balans.SubstationSection).IsLowVoltage == true;

                sb.AppendFormat("T1_TYPE_{0}={1}&", index, isGroup ? "GROUP" : "POINT");
                if (isGroup)
                {
                    sb.AppendFormat("T1_NAME_{0}={1}&", index, point.Title);
                    sb.AppendFormat("T1_GR_CODE_{0}={1}&", index, point.Code);
                    sb.AppendFormat("T1_GR_NAME_{0}={1}&", index, point.Title);
                    sb.AppendFormat("T1_GR_TYPE_ID_{0}=25&", index);
                    sb.AppendFormat("T1_GR_ID_{0}={1}&", index, point.Id);
                    sb.AppendFormat("T1_GR_TYPE_NAME_{0}=Напряжение&", index);
                    sb.AppendFormat("T1_GR_TYPE_CODE_{0}=VOLTAGE&", index);
                }
                else
                {
                    sb.AppendFormat("T1_ID_{0}={1}&", index, point.Id);
                    sb.AppendFormat("T1_NAME_{0}={1} [{2}]&", index, point.Title, point.Code);
                    sb.AppendFormat("T1_POINT_ID_{0}={1}&", index, point.Id);
                    sb.AppendFormat("T1_POINT_NAME_{0}={1}&", index, point.Title);
                    sb.AppendFormat("T1_POINT_CODE_{0}={1}&", index, point.Code);
                    sb.AppendFormat("T1_POINT_ENABLED_{0}=1&", index);
                    sb.AppendFormat("T1_POINT_ENABLED_TXT_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_COMMERCIAL_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_INTERNAL_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_AUTO_READ_ENABLED_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_TYPE_NAME_{0}=Учет электричества&", index);
                    sb.AppendFormat("T1_POINT_TYPE_CODE_{0}=ELECTRICITY&", index);
                    sb.AppendFormat("T1_MOU_BT_{0}=&", index);
                    sb.AppendFormat("T1_MOU_ET_{0}=&", index);
                    sb.AppendFormat("T1_METER_NUMBER_{0}=&", index);
                    sb.AppendFormat("T1_METER_TYPE_NAME_{0}=&", index);
                    sb.AppendFormat("T1_GRP_BT_{0}=&", index);
                    sb.AppendFormat("T1_GRP_DESC_{0}=&", index);
                }
                switch (point.Type)
                {
                    case Model.ElementTypes.Fider:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Линии");
                        break;
                    case Model.ElementTypes.PowerTransformer:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Трансформаторы");
                        break;
                    case Model.ElementTypes.UnitTransformer:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Свои нужды");
                        break;
                    case Model.ElementTypes.UnitTransformerBus:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Свои нужды");
                        break;
                }
                
            }
            sb.AppendFormat("T1_COUNT={0}&", index);
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает параметры измерений для кода архива
        /// </summary>
        /// <param name="wid"></param>
        /// <returns></returns>
        public IEnumerable<Model.ML_Param> GetParamsForArchiveData(string wid)
        {
            var sendData = String.Format("dataBlock=PARAM&action=GET&wID={0}", wid);
            var data = EmcosSiteWrapper.Instance.GetViewAsync(sendData).Result;
            if (String.IsNullOrWhiteSpace(data))
                return null;
            var list = AnswerParser.Params(data).Cast<Model.ML_Param>();

            return list;
        }
        public ICollection<Model.ArchAP> GetPointsForArchiveData(string wid)
        {
            var sendData = String.Format("dataBlock=AP&action=GET&wID={0}", wid);
            var data = EmcosSiteWrapper.Instance.GetViewAsync(sendData).Result;
            if (String.IsNullOrWhiteSpace(data))
                return null;

            var list = AnswerParser.ArchAPs(data);
            return list.ToArray();
        }

        public bool GetDaylyArchiveDataForSubstation(Model.DatePeriod period, Model.Balans.Substation substation, System.Threading.CancellationTokenSource cts, Action<int, int> callback)
        {
            bool error;

            int current = 0, total = 0;

            bool hasMonthData;
            var nextMonthOfEndDate = period.EndDate.AddMonths(1);
            var nextMonthOfEndDate2 = new DateTime(nextMonthOfEndDate.Year, nextMonthOfEndDate.Month, 1);
            int lastDayInEndDateMonth = nextMonthOfEndDate2.AddDays(-1).Day;
            if (period.StartDate.Day == 1 && period.EndDate.Day == lastDayInEndDateMonth)
                hasMonthData = true;
            else
                hasMonthData = false;
            // измерения - А энергия за сутки
            var measurementsDaysPart = @"T2_TYPE_0=MSF&T2_NAME_0=А энергия&T2_MSF_ID_0=14&T2_MSF_NAME_0=А энергия&T2_AGGS_TYPE_ID_0=3&T2_MS_TYPE_ID_0=1";
            // А энергия за месяц
            var measurementsMonthPart = @"&T2_TYPE_1=MSF&T2_NAME_1=А энергия&T2_MSF_ID_1=14&T2_MSF_NAME_1=А энергия&T2_AGGS_TYPE_ID_1=4&T2_MS_TYPE_ID_1=1";
            // 
            var measurementsEndPartIfNotHasMonth = "&T2_COUNT=1&";
            var measurementsEndPartIfHasMonth = "&T2_COUNT=2&";

            substation.Status = Model.DataStatus.Processing;
            substation.Clear();
            error = false;

            string[] wids = null;

            if (substation.Children == null || substation.Children.Count == 0)
                error = true;
            else
            {
                if (substation.Items == null)
                {
                    App.Log.Log(String.Format("Подстанция <{0}> не имеет точек.", substation.Title), Category.Warn, Priority.None);
                    error = true;
                }
                else
                {                   
                    try
                    {
                        // 
                        var points = CreatePointsParam(substation.Items);
                        wids = GetArchiveWIds(
                            hasMonthData 
                              ? measurementsDaysPart + measurementsMonthPart + measurementsEndPartIfHasMonth
                              : measurementsDaysPart + measurementsEndPartIfNotHasMonth,
                            points, period, cts.Token);
                    }
                    catch (Exception e)
                    {
                        LastException = e;
                        App.Log.Log("GetSubstationsDaylyArchives - ошибка: " + e.Message, Category.Exception, Priority.High);
                        error = true;
                    }
                    if (wids == null || wids.Length == 0)
                    {
                        App.Log.Log("GetSubstationsDaylyArchives - ошибка получен неверный wid. Подстанция - " + substation.Title,
                            Category.Exception, Priority.High);
                        error = true;
                    }
                }
            }
            if (error == false)
            {
                var mls = GetParamsForArchiveData(wids[0]);

                #region | получение архивных данных по каждой из точек |

                total = substation.Items.Count;

                foreach (Model.Balans.IBalansItem item in substation.Items)
                {
                    current++;
                    if (callback!= null)
                        callback(current, total);
                    if (cts.IsCancellationRequested)
                    {
                        break;
                    }
                    item.Status = Model.DataStatus.Processing;
                    try
                    {
                        Func<Model.ML_Param, ICollection<Model.ArchData>> getPointArchive = (ml) =>
                        {
                            var point = new Model.ArchAP { Id = item.Id, Type = "POINT", Name = item.Title };
                            ICollection<Model.ArchData> list;
                            try
                            {
                                list = GetArchiveData(ml, point, period, cts.Token);
                            }
                            catch (Exception e)
                            {
                                App.Log.Log(String.Format("GetSubstationsDaylyArchives-getPointArchive. Ошибка получения данных по точке {0}. Сообщение: {1}",
                                    item.Id, e.Message), Category.Exception, Priority.High);
                                return null;
                            }
                            return list;
                        };

                        ICollection<Model.ArchData> data_days_minus, data_days_plus, data_month_minus = null, data_month_plus = null;
                        // А+ энергия  за  сутки
                        data_days_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_DAYS);
                        // А- энергия  за  сутки
                        data_days_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_DAYS);

                        if (hasMonthData)
                        {// А+ энергия  за  месяц
                            data_month_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_MONTH);
                            // А- энергия  за  месяц
                            data_month_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_MONTH);
                        }
                        // разбор данных
                        IList<double?> days_plus = new List<double?>();
                        IList<double?> days_minus = new List<double?>();
                        double? summ;

                        ParseData(out days_plus, out summ, ref data_days_plus, item.Title);
                        item.DailyEplus = days_plus;
                        item.DayEplusValue = summ;

                        ParseData(out days_minus, out summ, ref data_days_minus, item.Title);
                        item.DailyEminus = days_minus;
                        item.DayEminusValue = summ;

                        double? month_plus, month_minus;
                        if (hasMonthData)
                        {
                            ParseData(out month_plus, ref data_month_plus, item.Title);
                            item.MonthEplus = month_plus;

                            ParseData(out month_minus, ref data_month_minus, item.Title);
                            item.MonthEminus = month_minus;
                        }
                    }
                    catch (Exception e)
                    {
                        App.Log.Log(String.Format("GetSubstationsDaylyArchives. Ошибка получения данных по точке: ID={0}, NAME={1}. Сообщение: {2}",
                                    item.Id, item.Title, e.Message), Category.Exception, Priority.High);
                        continue;
                    }
                    item.Status = Model.DataStatus.Processed;
                }

                #endregion | получение архивных данных по каждой из точек |
            }
            substation.Status = Model.DataStatus.Processed;
            return error == false;
        }

        public void GetDaylyArchiveDataForItem(Model.DatePeriod period, Model.Balans.IBalansItem item, System.Threading.CancellationTokenSource cts)
        {
            bool error = false;

            bool hasMonthData;
            var nextMonthOfEndDate = period.EndDate.AddMonths(1);
            var nextMonthOfEndDate2 = new DateTime(nextMonthOfEndDate.Year, nextMonthOfEndDate.Month, 1);
            int lastDayInEndDateMonth = nextMonthOfEndDate2.AddDays(-1).Day;
            if (period.StartDate.Day == 1 && period.EndDate.Day == lastDayInEndDateMonth)
                hasMonthData = true;
            else
                hasMonthData = false;
            // измерения - А энергия за сутки
            var measurementsDaysPart = @"T2_TYPE_0=MSF&T2_NAME_0=А энергия&T2_MSF_ID_0=14&T2_MSF_NAME_0=А энергия&T2_AGGS_TYPE_ID_0=3&T2_MS_TYPE_ID_0=1";
            // А энергия за месяц
            var measurementsMonthPart = @"&T2_TYPE_1=MSF&T2_NAME_1=А энергия&T2_MSF_ID_1=14&T2_MSF_NAME_1=А энергия&T2_AGGS_TYPE_ID_1=4&T2_MS_TYPE_ID_1=1";
            // 
            var measurementsEndPartIfNotHasMonth = "&T2_COUNT=1&";
            var measurementsEndPartIfHasMonth = "&T2_COUNT=2&";

            item.Status = Model.DataStatus.Processing;
            error = false;

            string[] wids = null;

            try
            {
                // 
                var points = CreatePointsParam(new List<Model.Balans.IBalansItem> { item });
                wids = GetArchiveWIds(
                    hasMonthData
                      ? measurementsDaysPart + measurementsMonthPart + measurementsEndPartIfHasMonth
                      : measurementsDaysPart + measurementsEndPartIfNotHasMonth,
                    points, period, cts.Token);
            }
            catch (Exception e)
            {
                App.Log.Log("GetDaylyArchiveDataForItem - GetArchiveWIds - ошибка: " + e.Message, Category.Exception, Priority.High);
                error = true;
            }
            if (wids == null || wids.Length == 0)
            {
                App.Log.Log("GetDaylyArchiveDataForItem - ошибка получен неверный wid. Элемент - " + item.Title,
                    Category.Exception, Priority.High);
                error = true;
            }

            if (error == false)
            {
                var mls = GetParamsForArchiveData(wids[0]);

                try
                {
                    Func<Model.ML_Param, ICollection<Model.ArchData>> getPointArchive = (ml) =>
                    {
                        var point = new Model.ArchAP { Id = item.Id, Type = "POINT", Name = item.Title };
                        ICollection<Model.ArchData> list;
                        try
                        {
                            list = GetArchiveData(ml, point, period, cts.Token);
                        }
                        catch (Exception e)
                        {
                            App.Log.Log(String.Format("GetDaylyArchiveDataForItem-getPointArchive. Ошибка получения данных по точке {0}. Сообщение: {1}",
                                item.Id, e.Message), Category.Exception, Priority.High);
                            return null;
                        }
                        return list;
                    };

                    ICollection<Model.ArchData> data_days_minus, data_days_plus, data_month_minus = null, data_month_plus = null;
                    // А+ энергия  за  сутки
                    data_days_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_DAYS);
                    // А- энергия  за  сутки
                    data_days_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_DAYS);

                    if (hasMonthData)
                    {// А+ энергия  за  месяц
                        data_month_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_MONTH);
                        // А- энергия  за  месяц
                        data_month_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_MONTH);
                    }
                    // разбор данных
                    IList<double?> days_plus = new List<double?>();
                    IList<double?> days_minus = new List<double?>();
                    double? summ;

                    ParseData(out days_plus, out summ, ref data_days_plus, item.Title);
                    item.DailyEplus = days_plus;
                    item.Eplus = summ;

                    ParseData(out days_minus, out summ, ref data_days_minus, item.Title);
                    item.DailyEminus = days_minus;
                    item.Eminus = summ;

                    double? month_plus, month_minus;
                    if (hasMonthData)
                    {
                        ParseData(out month_plus, ref data_month_plus, item.Title);
                        item.MonthEplus = month_plus;

                        ParseData(out month_minus, ref data_month_minus, item.Title);
                        item.MonthEminus = month_minus;
                    }
                }
                catch (Exception e)
                {
                    App.Log.Log(String.Format("GetDaylyArchiveDataForItem. Ошибка получения данных по точке: ID={0}, NAME={1}. Сообщение: {2}",
                                item.Id, item.Title, e.Message), Category.Exception, Priority.High);
                }
            }

            item.Status = Model.DataStatus.Processed;
        }

        /////////

        internal void ParseData(out IList<double?> list, out double? summ, ref ICollection<Model.ArchData> values, string name)
        {
            list = new List<double?>();
            summ = new Nullable<double>();
            if (values != null)
            {
                list = new List<double?>();
                int normalDataCount = 0;
                summ = 0.0;
                foreach (Model.ArchData d in values)
                    if (d.SFS == "Нормальные данные" || d.SFS.Contains("Ручной ввод"))
                    {
                        double value = 0;
                        Double.TryParse(d.D, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
                        list.Add(value);
                        normalDataCount++;
                        summ += value;
                    }
                    else
                    {
                        list.Add(new Nullable<double>());
                        normalDataCount++;
                    }
            }
            else
                App.Log.Log(String.Format("ParseData данные за сутки. Ошибка в данных. Точка: {0}", name), Category.Exception, Priority.High);
        }
        private void ParseData(out double? result, ref ICollection<Model.ArchData> values, string name)
        {
            result = new Nullable<double>();
            if (values != null && values.Count == 1)
            {
                var d = values.FirstOrDefault();
                if (d.SFS == "Нормальные данные" || d.SFS.Contains("Ручной ввод"))
                {
                    double value = 0;
                    Double.TryParse(d.D, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
                    result = value;
                }
            }
            else
                App.Log.Log(String.Format("ParseData данные за месяц. Ошибка в данных. Точка: {0}", name), Category.Exception, Priority.High);
        }
    }
}
