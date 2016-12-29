using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

using TMP.Common.Logger;
using TMP.Common.NetHelper;

namespace TMP.Work.Emcos
{
    using ServiceLocator;
    using MsgBox;
    public class EmcosSiteWrapper : BaseSiteWrapper, IDisposable
    {
        #region Singleton

        private static volatile EmcosSiteWrapper instance;
        private static readonly object syncRoot = new Object();
        public static EmcosSiteWrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new EmcosSiteWrapper();
                    }
                }

                return instance;
            }
        }

        #endregion
        /// <summary>
        /// Сервис отображения сообщений
        /// </summary>
        private IMessageBoxService _messageBoxService;
        /// <summary>
        /// Настройки
        /// </summary>
        private static EmcosSettings _settings;
        public static void SetSettings(EmcosSettings settings)
        {
            _settings = settings;
            if (_settings == null)
                throw new ArgumentNullException("EmcosSettings must be not null");
            //_settings.PropertyChanged += _settings_PropertyChanged;
        }

        #region Constructor
        /// <summary>
        /// Создание экземпляра класса
        /// </summary>
        private EmcosSiteWrapper()
        {
            if (_settings == null)
                throw new ArgumentNullException("EmcosSettings must be not null");

            _messageBoxService = ServiceContainer.Instance.GetService<IMessageBoxService>();
            if (_messageBoxService == null)
                throw new ArgumentNullException("Not found IMessageBoxService");

            Cookie = new Cookie("ASPSESSIONIDQSATBDCD", string.Empty);

            UserName = _settings.UserName;
            Password = _settings.Password;
            ServerAddress = _settings.ServerAddress;
            ServiceName = _settings.ServiceName;
            //_serverAddress = "localhost:1000";
            // в секундах
            TimeOut = _settings.NetTimeOutInSeconds;

            Status = State.Offline;
        }
        #endregion

        #region IDisposable
        ~EmcosSiteWrapper()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_allDone != null)
                _allDone.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private and Internal methods
        private void ToLogError(string message)
        {
            if (Log != null)
                Log.Log(message, Category.Exception, Priority.High);
        }
        private void ToLogInfo(string message)
        {
            if (Log != null)
                Log.Log(message, Category.Info, Priority.None);
        }
        private void ToLogWarning(string message)
        {
            if (Log != null)
                Log.Log(message, Category.Warn, Priority.Medium);
        }

        private void SetOKResult(State state = State.Online)
        {
            LastException = null;
            Status = state;
            ErrorMessage = string.Empty;
        }
        private void SetFailResult(Exception e, string message)
        {
            LastException = e;
            Status = State.Error;
            ErrorMessage = message;
            ToLogError(message);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Состояние сервиса
        /// </summary>
        public enum State
        {
            Online,
            Offline,
            NotAuthorized,
            AccessDenied,
            Error
        }
        /// <summary>
        /// Тип авторизации
        /// </summary>
        public enum AuthorizationType
        {
            Login,
            Logout,
            GetRights
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="method">Метод</param>
        /// <returns></returns>
        public bool Login(AuthorizationType method)
        {
            /*if (isSiteOnline() == false)
                return false;            */

            var url = @"{0}scripts/autentification.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (String.IsNullOrWhiteSpace(Cookie.Value) == false)
                TryAddCookie(ref httpWebRequest);

            var data = String.Empty;
            switch (method)
            {
                case AuthorizationType.Login:
                    data = string.Format("user={0}&password={1}&action=login", this.UserName, this.Password);
                    break;
                case AuthorizationType.Logout:
                    data = string.Format("action=logout");
                    break;
                case AuthorizationType.GetRights:
                    data = string.Format("action=getrights");
                    break;
                default:
                    throw new InvalidOperationException("Неизвестный тип операции для авторизации");
            }

            var buffer = Encoding.ASCII.GetBytes(data);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse httpWebResponse = null;
            try
            {
                var reqStream = httpWebRequest.GetRequestStream();
                reqStream.Write(buffer, 0, buffer.Length);
                reqStream.Close();

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType.ToLower() == "text/html; charset=utf-8")
                    {
                        var responseStream = httpWebResponse.GetResponseStream();
                        using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                        {
                            var answer = streamReader.ReadToEnd();
                            if (answer.Contains("result=0"))
                            {
                                switch (method)
                                {
                                    case AuthorizationType.Login:
                                        // получаем куки
                                        this.Cookie.Value = string.Empty;

                                        for (int i = 0; i < httpWebResponse.Headers.Count; i++)
                                        {
                                            var name = httpWebResponse.Headers.GetKey(i);
                                            if (name != "Set-Cookie")
                                                continue;
                                            var value = httpWebResponse.Headers.Get(i);
                                            // Set-Cookie: ASPSESSIONIDQSATBDCD=OMEJJIGBLEAOINHJMCOELNGH; path=/
                                            var parts = value.Split(new char[] { ';' });
                                            if (parts.Length > 0)
                                            {
                                                foreach (string part in parts)
                                                {
                                                    if (part.StartsWith("ASPSESSIONID"))
                                                    {
                                                        var kvp = Utils.ParseKeyValuePair(part);

                                                        this.Cookie.Name = kvp.Key;
                                                        this.Cookie.Value = kvp.Value;
                                                        this.Cookie.Domain = GetDomain();
                                                    }
                                                }
                                            }
                                        }

                                        // проверка
                                        if (String.IsNullOrEmpty(this.Cookie.Value))
                                        {
                                            ToLogError("[EmcosSiteWrapper] Попытка авторизации - не получены куки ASPSESSIONID");
                                            bool attemp2 = Login(method);
                                            if (attemp2 == false)
                                            {
                                                ToLogError("[EmcosSiteWrapper] Попытка авторизации #2- не получены куки ASPSESSIONID");
                                                throw new InvalidOperationException("Cookie ASPSESSIONID not exist.");
                                            }
                                        }
                                        SetOKResult();
                                        return true;
                                    case AuthorizationType.Logout:
                                        LastException = null;
                                        Status = State.NotAuthorized;
                                        return true;
                                    case AuthorizationType.GetRights:
                                        /* &result=0&DB_TIME=2016.01.06 15:18:45&user=sbyt */
                                        if (answer.Contains("user=" + this.Cookie))
                                        {
                                            SetOKResult();
                                            return true;
                                        }
                                        else
                                        {
                                            if (Login(AuthorizationType.Login))
                                            {
                                                SetOKResult();
                                                return true;
                                            }
                                            else
                                            {
                                                ToLogError("[EmcosSiteWrapper] Проверка прав - отсутствуют. Ответ сервера: " + answer);
                                                LastException = null;
                                                Status = State.AccessDenied;
                                                return false;
                                            }
                                        }
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                SetFailResult(null, "[EmcosSiteWrapper] Функция Login - неверный ответ сервера. Ответ : " + answer);
                                return false;
                            }
                            /* result=0&DB_TIME=2016.01.06 15:18:45&CONFIG_XML=<data><supportedEnergy type="object"><OTHER>0</OTHER><ELECTRICITY>1</ELECTRICITY><WEATHER>2</WEATHER><COLD>5</COLD><TERMO_WATER>6</TERMO_WATER><GAS>7</GAS><WATER>8</WATER><HOT_WATER>11</HOT_WATER></supportedEnergy></data> */
                        }
                    }
                }
                SetFailResult(null, "[EmcosSiteWrapper] Функция Login - статус: " + httpWebResponse.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                SetFailResult(ex, "[EmcosSiteWrapper] Функция Login - ошибка: " + ex.Message);
                return false;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }
        /// <summary>
        /// Авторизация с поддержкой отмены
        /// </summary>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Состояние сервиса</returns>
        public Task<State> Login(CancellationToken ct)
        {
            var task = new Task<State>((t) =>
            {
                Status = State.Online;
                bool online = false;
                try
                {
                    online = IsServerOnline();
                    if (online)
                    {
                        if (Login(AuthorizationType.Login) == false)
                        {
                            if (IsSiteOnline() == false)
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
                    SetFailResult(se, "[EmcosSiteWrapper] Ошибка авторизации: " + se.Message);
                    return Status;
                }
                ToLogInfo("[EmcosSiteWrapper] Авторизация - " + Status.ToString());
                SetOKResult();
                return Status;
            }, ct);

            task.Start();

            return task;
        }
        /// <summary>
        /// Возвращает задачу со списком точек родительской группы с заданным ИД
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public Task<string> GetAPointsAsync(string parentId)
        {
            var url = @"{0}scripts/point.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (TryAddCookie(ref httpWebRequest) == false)
            {
                SetFailResult(null, "[EmcosSiteWrapper] Функция GetAPointsAsync - ошибка: не удалось добавить куки");
                return Task.FromResult(String.Empty);
            }

            var data = String.Format("refresh=true&TYPE=GROUP&action=expand&ID={0}", parentId);
            var buffer = Encoding.ASCII.GetBytes(data);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            try
            {
                using (var postStream = httpWebRequest.GetRequestStream())
                {
                    postStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                SetFailResult(ex, "[EmcosSiteWrapper] Функция GetAPoints - ошибка: " + ex.Message);
                return Task.FromResult(String.Empty);
            }

            var task = Task.Factory.FromAsync(
                httpWebRequest.BeginGetResponse,
                asyncResult => httpWebRequest.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t =>
            {
                SetOKResult();
                var result = ReadStreamFromResponse(t.Result);
                return result;
            });
        }
        /// <summary>
        /// Возвращает задачу со списком направлений для выбранного измерения
        /// </summary>
        /// <param name="senddata"></param>
        /// <returns></returns>
        public Task<string> GetParamsAsync(string senddata)
        {
            var url = @"{0}scripts/param.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (TryAddCookie(ref httpWebRequest) == false)
            {
                SetFailResult(null, "[EmcosSiteWrapper] Функция GetParamsAsync - ошибка: не удалось добавить куки");
                return Task.FromResult(String.Empty);
            }

            var buffer = Encoding.ASCII.GetBytes(senddata);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            using (var postStream = httpWebRequest.GetRequestStream())
            {
                postStream.Write(buffer, 0, buffer.Length);
            }

            var task = Task.Factory.FromAsync(
                httpWebRequest.BeginGetResponse,
                asyncResult => httpWebRequest.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t =>
            {
                SetOKResult();
                return ReadStreamFromResponse(t.Result);
            });
        }
        /// <summary>
        /// Возвращает данные архива с заданным ИД
        /// </summary>
        /// <param name="senddata"></param>
        /// <returns></returns>
        public Task<string> GetViewAsync(string senddata)
        {
            var url = @"{0}scripts/view.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (TryAddCookie(ref httpWebRequest) == false)
            {
                SetFailResult(null, "[EmcosSiteWrapper] Функция GetViewAsync - ошибка: не удалось добавить куки");
                return Task.FromResult(String.Empty);
            }

            var buffer = Encoding.UTF8.GetBytes(senddata);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            using (var postStream = httpWebRequest.GetRequestStream())
            {
                postStream.Write(buffer, 0, buffer.Length);
            }

            var task = Task.Factory.FromAsync(
                httpWebRequest.BeginGetResponse,
                asyncResult => httpWebRequest.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t =>
            {
                SetOKResult();
                return ReadStreamFromResponse(t.Result);
            });
        }
        /// <summary>
        /// Возвращает ИД архива для указанных измерений, точек, периода
        /// </summary>
        /// <param name="measurements">измерения</param>
        /// <param name="points">точки</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">конец периода</param>
        /// <param name="ct">токен отмены</param>
        /// <param name="procent">процент группы</param>
        /// <param name="showGroup">это группа</param>
        /// <returns>ИД архива</returns>
        public string[] GetArchiveWIds(string measurements, string points, 
            DateTime startDate, DateTime endDate, CancellationToken ct, bool procent = false, bool showGroup = false)
        {
            if (Login(AuthorizationType.GetRights) == false)
            {
                if (IsSiteOnline() == false)
                    Status = State.Offline;
                else
                    Status = State.NotAuthorized;
                LastException = null;
                ErrorMessage = string.Empty;
                return null;
            }
            else
                Status = State.Online;

            var wids = new List<string>();

            var footer = new StringBuilder();
            footer.AppendFormat("SHOW_VERGES=0&");
            footer.AppendFormat("FROM={0}&", startDate.ToString("yyyy.MM.dd"));
            footer.AppendFormat("TO={0}&", endDate.ToString("yyyy.MM.dd"));
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

            var response = GetViewAsync(senddata).Result;
            if (String.IsNullOrWhiteSpace(response))
            {
                SetFailResult(null, "[EmcosSiteWrapper] Ошибка при получении ИД архива. Нет получен ответ сервера.");
                return null;
            }
            var list = Utils.ParsePairs(response);
            bool ok = list.Get("result") == "0";
            if (ok == false)
            {
                SetFailResult(null, "[EmcosSiteWrapper] Ошибка при получении ИД архива. Ответ сервера: ");
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
               ToLogInfo("[EmcosSiteWrapper] Разное количество ИД архива: " + wids.Count + " и " + count);
            ToLogInfo("[EmcosSiteWrapper] Получен ИД архива: " + String.Join(", ", wids));

            SetOKResult();
            return wids.ToArray();
        }
        /// <summary>
        /// Возвращает архивные значения указанной точке по указанному измерению и периоду
        /// </summary>
        /// <param name="param">измерение</param>
        /// <param name="point">точка</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">конец периода</param>
        /// <param name="ct">токен отмены</param>
        /// <returns></returns>
        public Task<IEnumerable<Model.ArchData>> GetArchiveData(Model.ML_Param param, Model.ArchAP point,
            DateTime startDate, DateTime endDate, CancellationToken ct)
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
                startDate.ToString("yyyy.MM.dd"),
                endDate.ToString("yyyy.MM.dd"),
                aggs,
                datacode,
                type,
                id);
            sendData = System.Web.HttpUtility.UrlPathEncode(sendData).Replace("_", "%5F").Replace("+", "%2B").ToUpper();


            var url = @"{0}scripts/arch.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (TryAddCookie(ref httpWebRequest) == false)
            {
                SetFailResult(null, "[EmcosSiteWrapper] Функция GetArchiveData - ошибка: не удалось добавить куки");
                IEnumerable<Model.ArchData> empty = new List<Model.ArchData>();
                return Task.FromResult(empty);
            }
            var buffer = Encoding.UTF8.GetBytes(sendData);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            using (var postStream = httpWebRequest.GetRequestStream())
            {
                postStream.Write(buffer, 0, buffer.Length);
            }

            var task = Task.Factory.FromAsync(
                httpWebRequest.BeginGetResponse,
                asyncResult => httpWebRequest.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t =>
            {
                var data = ReadStreamFromResponse(t.Result);
                if (String.IsNullOrWhiteSpace(data))
                {
                    SetFailResult(null, "[EmcosSiteWrapper] Архивные данные отсутствуют. Точка: " + point.Name);
                    return null;
                }
                var list = Utils.ArchiveData(data);
                SetOKResult();
                return list;
            });            
        }
        /// <summary>
        /// Формирует строку параметров для указанного списка точек для получения ИД архива
        /// </summary>
        /// <param name="points">Список точек</param>
        /// <returns></returns>
        public string CreatePointsParam(IEnumerable<Model.IEmcosPoint> points)
        {
            if (points == null)
                return null;
            var sb = new StringBuilder();
            int index = 0;
            foreach (var point in points)
            {
                bool isGroup = point.Type == Model.ElementTypes.SECTION;// && (point as Model.Balans.SubstationSection).IsLowVoltage == true;

                sb.AppendFormat("T1_TYPE_{0}={1}&", index, isGroup ? "GROUP" : "POINT");
                if (isGroup)
                {
                    sb.AppendFormat("T1_NAME_{0}={1}&", index, point.Name);
                    sb.AppendFormat("T1_GR_CODE_{0}={1}&", index, point.Code);
                    sb.AppendFormat("T1_GR_NAME_{0}={1}&", index, point.Name);
                    sb.AppendFormat("T1_GR_TYPE_ID_{0}=25&", index);
                    sb.AppendFormat("T1_GR_ID_{0}={1}&", index, point.Id);
                    sb.AppendFormat("T1_GR_TYPE_NAME_{0}=Напряжение&", index);
                    sb.AppendFormat("T1_GR_TYPE_CODE_{0}=VOLTAGE&", index);
                }
                else
                {
                    sb.AppendFormat("T1_ID_{0}={1}&", index, point.Id);
                    sb.AppendFormat("T1_NAME_{0}={1} [{2}]&", index, point.Name, point.Code);
                    sb.AppendFormat("T1_POINT_ID_{0}={1}&", index, point.Id);
                    sb.AppendFormat("T1_POINT_NAME_{0}={1}&", index, point.Name);
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
                    case Model.ElementTypes.FIDER:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Линии");
                        break;
                    case Model.ElementTypes.POWERTRANSFORMER:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Трансформаторы");
                        break;
                    case Model.ElementTypes.UNITTRANSFORMER:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Свои нужды");
                        break;
                    case Model.ElementTypes.UNITTRANSFORMERBUS:
                        sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, "Свои нужды");
                        break;
                }
                index++;
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
            var data = GetViewAsync(sendData).Result;
            if (String.IsNullOrWhiteSpace(data))
            {
                SetFailResult(null, "[EmcosSiteWrapper] GetParamsForArchiveData - данные отсутствуют");
                return null;
            }
            var list = Utils.Params(data).Cast<Model.ML_Param>();
            SetOKResult();
            return list;
        }
        /// <summary>
        /// Возвращает список точек архива с указанным ИД
        /// </summary>
        /// <param name="wid"></param>
        /// <returns></returns>
        public ICollection<Model.ArchAP> GetPointsForArchiveData(string wid)
        {
            var sendData = String.Format("dataBlock=AP&action=GET&wID={0}", wid);
            var data = GetViewAsync(sendData).Result;
            if (String.IsNullOrWhiteSpace(data))
            {
                SetFailResult(null, "[EmcosSiteWrapper] GetPointsForArchiveData - данные отсутствуют");
                return null;
            }

            var list = Utils.ArchAPs(data);
            SetOKResult();
            return list.ToArray();
        }

        /// <summary>
        /// Оболочка для выполнения задач
        /// </summary>
        /// <param name="cts"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task<State> ExecuteAction(CancellationTokenSource cts, Action action)
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
                            case State.Online:
                                action();
                                break;

                            case State.Offline:
                                ToLogWarning("[EmcosSiteWrapper] Web-сервис Emcos Corporate не доступен!");
                                _messageBoxService.Show("Web-сервис Emcos Corporate не доступен!\nПроверьте сетевые настройки.",
                                    "Нет доступа", MsgBoxButtons.OK, MsgBoxImage.Error);
                                break;
                            case State.AccessDenied:
                                ToLogWarning("[EmcosSiteWrapper] Доступ запрещён!");
                                _messageBoxService.Show("Доступ запрещён.",
                                    "Нет доступа", MsgBoxButtons.OK, MsgBoxImage.Error);
                                break;

                            case State.NotAuthorized:
                                ToLogWarning("[EmcosSiteWrapper] Не удалось авторизоваться.");
                                _messageBoxService.Show("Не удалось авторизоваться!",
                                    "Нет доступа", MsgBoxButtons.OK, MsgBoxImage.Error);
                                break;
                        }
                        return state;
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);

                task.ContinueWith((s) =>
                {
                    ToLogInfo("[EmcosSiteWrapper] Получение данных отменено.");
                    _messageBoxService.Show("Отменено.", "", MsgBoxButtons.OK, MsgBoxImage.Warning);
                    return State.Online;
                },
                  TaskContinuationOptions.OnlyOnCanceled);

                task.ContinueWith((s) =>
                {
                    ToLogError("[EmcosSiteWrapper] Получение данных. Ошибка авторизации - " + s.Exception.Message);
                    _messageBoxService.Show("Произошла ошибка.\n" + s.Exception.Message, "", MsgBoxButtons.OK, MsgBoxImage.Error);
                    return State.NotAuthorized;
                }, TaskContinuationOptions.OnlyOnFaulted);

                return actionTask;
            }
            catch (Exception ex)
            {
                LastException = ex;
                ToLogError("[EmcosSiteWrapper] Получение данных. Ошибка - " + ex.Message);
                _messageBoxService.Show("Произошла ошибка.\n" + ex.Message, "", MsgBoxButtons.OK, MsgBoxImage.Error);
                return null;
            }
        }


        public async Task<string> ExecuteFunction(Func<string, Task<string>> function, string param)
        {
            LastException = null;
            if (Status != State.Online)
                Login(AuthorizationType.Login);

            Task<string> result = function(param);
            if (LastException != null)
            {
                switch (Status)
                {
                    case State.Offline:
                        Login(AuthorizationType.Login);
                        return await ExecuteFunction(function, param);
                    case State.NotAuthorized:
                        Login(AuthorizationType.Login);
                        return await ExecuteFunction(function, param);
                    case State.AccessDenied:
                        return await Task.FromResult(string.Empty);
                    case State.Error:
                        return await Task.FromResult(string.Empty);
                    default:
                        return await Task.FromResult(string.Empty);
                }
            }
            else
                return result.Result;
        }

        #endregion

        #region Properties
        public State Status { get; private set; }
        public Exception LastException { get; set; }
        public string ErrorMessage { get; private set; }
        #endregion
    }
}