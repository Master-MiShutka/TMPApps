using System;
using System.Collections.Generic;
using System.Text;

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

        #region Fields

        private Properties.Settings _settings = Properties.Settings.Default;

        #endregion

        #region Constructor
        /// <summary>
        /// Создание экземпляра класса
        /// </summary>
        private EmcosSiteWrapper()
        {
            Cookie = new Cookie("ASPSESSIONIDQSATBDCD", string.Empty);

            UserName = _settings.UserName;
            Password = _settings.Password;

            ServerAddress = _settings.ServerAddress;
            //_serverAddress = "localhost:1000";
            // в секундах
            TimeOut = _settings.NetTimeOutInSeconds;

            ArchiveData = new BufferedAsyncNetClient(string.Format(@"{0}/scripts/arch.asp", this.SiteAddress))
            {
                Configure = ConfigureArchiveData
            };
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

        private void ConfigureArchiveData(ref HttpWebRequest request)
        {
            ConfigureRequest(ref request);
            request.Method = WebRequestMethods.Http.Post;
            request.Accept = "*/*";
            request.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            TryAddCookie(ref request);
        }

        #endregion

        #region Public Methods

        public enum AuthorizationType
        {
            Login,
            Logout,
            GetRights
        }


        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool Login(AuthorizationType method)
        {
            /*if (isSiteOnline() == false)
                return false;            */

            var url = @"{0}/scripts/autentification.asp";
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
                                                        var kvp = AnswerParser.ParseKeyValuePair(part);

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
                                            App.Log.Log("Попытка авторизации - не получены куки ASPSESSIONID", Category.Exception, Priority.High);
                                            bool attemp2 = Login(method);
                                            if (attemp2 == false)
                                            {
                                                App.Log.Log("Попытка авторизации #2- не получены куки ASPSESSIONID", Category.Exception, Priority.High);
                                                throw new InvalidOperationException("Cookie ASPSESSIONID not exist.");
                                            }
                                        }

                                        return true;
                                    case AuthorizationType.Logout:
                                        return true;
                                    case AuthorizationType.GetRights:
                                        /* &result=0&DB_TIME=2016.01.06 15:18:45&user=sbyt */
                                        if (answer.Contains("user=" + this.Cookie))
                                            return true;
                                        else
                                        {
                                            if (Login(AuthorizationType.Login))
                                                return true;
                                            else
                                            {
                                                App.Log.Log("Проверка прав - отсутствуют. Ответ сервера: " + answer, Category.Exception, Priority.High);
                                                return false;
                                            }
                                        }
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                App.Log.Log("Функция Login - неверный ответ сервера. Ответ : " + answer, Category.Exception, Priority.High);
                                return false;
                            }
                            /* result=0&DB_TIME=2016.01.06 15:18:45&CONFIG_XML=<data><supportedEnergy type="object"><OTHER>0</OTHER><ELECTRICITY>1</ELECTRICITY><WEATHER>2</WEATHER><COLD>5</COLD><TERMO_WATER>6</TERMO_WATER><GAS>7</GAS><WATER>8</WATER><HOT_WATER>11</HOT_WATER></supportedEnergy></data> */
                        }
                    }
                }
                App.Log.Log("Функция Login - статус: " + httpWebResponse.StatusCode, Category.Info, Priority.None);
                return false;
            }
            catch (Exception ex)
            {
                App.Log.Log("Функция Login - ошибка: " + ex.Message, Category.Exception, Priority.High);
                return false;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        public Task<string> GetAPointsAsync(string parentId)
        {
            var url = @"{0}/scripts/point.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (TryAddCookie(ref httpWebRequest) == false)
                return null;

            var data = String.Format("refresh=true&TYPE=GROUP&action=expand&ID={0}", parentId);
            var buffer = Encoding.ASCII.GetBytes(data);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            try {
                using (var postStream = httpWebRequest.GetRequestStream())
                {
                    postStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                App.Log.Log("Функция GetAPoints - ошибка: " + ex.Message, Category.Exception, Priority.High);
                return null;
            }

            var task = Task.Factory.FromAsync(
                httpWebRequest.BeginGetResponse,
                asyncResult => httpWebRequest.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        public Task<string> GetParamsAsync(string senddata)
        {
            var url = @"{0}/scripts/param.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (TryAddCookie(ref httpWebRequest) == false)
                return null;

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

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }


        public Task<string> GetViewAsync(string senddata)
        {
            var url = @"{0}/scripts/view.asp";
            url = string.Format(url, this.SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            // передаём куки
            if (TryAddCookie(ref httpWebRequest) == false)
                return null;

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

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        #endregion

        #region Properties

        public TMP.Common.NetHelper.BufferedAsyncNetClient ArchiveData { get; }

        #endregion
    }
}
