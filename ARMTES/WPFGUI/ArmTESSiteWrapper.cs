using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;


using System.IO;
using System.IO.Compression;
using System.Net;


namespace TMP.ARMTES
{
    using Model;
    public enum ProfileType
    {
        [Display(Name = "текущие показания")]
        Current = 1,
        [Display(Name = "показания на начало суток")]
        Days = 2,
        [Display(Name = "показания на начало месяца")]
        Months = 3
    }

    public enum SectorType
    {
        [Display(Name = "Аскуэ-быт")]
        HHS,
        [Display(Name = "Мелкомоторный сектор")]
        [Description("СДСП")]
        SES
    }

    public sealed class ArmTESSiteWrapper
    {
        #region Fields
        protected static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const string chromeUserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        private const string firefoxUserAgent = "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0";

        private const string ieUserAgent = "Mozilla/5.0 (compatible, MSIE 11, Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

        private static Object theLock = new Object();

        /// <summary>
        /// Куки для авторизация
        /// </summary>
        private Cookie aspxauth = new Cookie(".ASPXAUTH", string.Empty);
        /// <summary>
        /// Логин
        /// </summary>
        private string login;

        /// <summary>
        /// Пароль
        /// </summary>
        private string password;

        /// <summary>
        /// Адрес сайта, без завершающего /
        /// </summary>
        private string siteUrl;

        /// <summary>
        /// Куки для хранения значения выбранного сектора
        /// </summary>
        private Cookie userSettings = new Cookie("UserSettings", string.Empty);  
        #endregion

        #region Constructor
        /// <summary>
        /// Создание экземпляра класса
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <param name="siteUrl">Адрес</param>
        public ArmTESSiteWrapper(string login, string password, string siteUrl)
        {
            this.login = login;
            this.password = password;
            this.siteUrl = siteUrl;
            // таймаут
            this.TimeOut = 180;

            // Авторизация
            if (!LoginAndGetCookie())
                IsAuthorized = false;
            else
                IsAuthorized = true;
        }
        #endregion

        #region Private and Internal methods

        /// <summary>
        /// Авторизация и получение "печенек"
        /// </summary>
        public bool LoginAndGetCookie()
        {
            string url = @"{0}/ARMTES/Account/Login?ReturnUrl=%2fARMTES%2f";
            url = string.Format(url, this.siteUrl);
            string loginData = "UserName={0}&Password={1}";
            loginData = string.Format(loginData, this.login, this.password);

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            httpWebRequest.CookieContainer = new CookieContainer();

            byte[] buffer = Encoding.ASCII.GetBytes(loginData);
            httpWebRequest.ContentLength = buffer.Length;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            HttpWebResponse httpWebResponse = null;

            try
            {
                using (Stream reqStream = httpWebRequest.GetRequestStream())
                {
                    reqStream.Write(buffer, 0, buffer.Length);
                    reqStream.Close();
                }

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.Found)
                {
                    // получаем куки
                    this.aspxauth.Value = string.Empty;
                    CookieCollection cookies = httpWebResponse.Cookies;
                    foreach (Cookie cookie in cookies)
                        if (cookie.Name == this.aspxauth.Name)
                        {
                            this.aspxauth.Value = cookie.Value;
                            this.aspxauth.Domain = cookie.Domain;
                        }
                    // проверка
                    if (String.IsNullOrEmpty(this.aspxauth.Value))
                        throw new InvalidOperationException("Cookie .ASPAUTH not exist.");
                }
                LastException = null;
                return true;
            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return false;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                if (we.Status == WebExceptionStatus.Timeout)
                    LastStatusCode = HttpStatusCode.RequestTimeout;
                else
                    LastStatusCode = HttpStatusCode.BadRequest;
                LastException = we;
                return false;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return false;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        private void ConfigureRequest(ref HttpWebRequest httpWebRequest)
        {
            httpWebRequest.Timeout = this.TimeOut * 1000;
            httpWebRequest.ReadWriteTimeout = this.TimeOut * 1000;
            httpWebRequest.Date = DateTime.Now;            
            httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpWebRequest.UserAgent = ieUserAgent;
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.KeepAlive = true;
        }
        private string ShortDate(DateTime date)
        {
            return date.ToString("dd.MM.yyyy");
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Выбор сектора
        /// </summary>
        /// <param name="sector">Сектор</param>
        /// <param name="fromDate">Начало периода</param>
        /// <param name="toDate">Конец периода</param>
        /// <param name="profile">Профиль показаний (текущие, начало суток, начало месяца)</param>
        public bool ChooseSector(SectorType sector, DateTime fromDate, DateTime toDate, ProfileType profile)
        {
            string url = @"{0}/ARMTES/Home/ChooseSector?";
            url = string.Format(url, this.siteUrl);
            string queryData = "sectorType={0}&fromDate={1}&toDate={2}&profileId={3}&isGeographicalTree=false";            

            // если аскуэ-быт
            if (sector == SectorType.HHS)
                this.userSettings.Value = "SectorType=HHS";
            else
                // Мелкомоторный сектор
                this.userSettings.Value = "SectorType=SES";

            queryData = string.Format(queryData, sector, ShortDate(fromDate), ShortDate(toDate), profile);

            url = url + queryData;

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");
            httpWebRequest.Referer = string.Format("{0}/ARMTES/", this.siteUrl);

            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(1);
            httpWebRequest.CookieContainer.Add(this.aspxauth);

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.Found)
                {
                    // получаем куки
                    this.userSettings.Value = string.Empty;
                    CookieCollection cookieCollection = httpWebResponse.Cookies;
                    foreach (Cookie cookie in cookieCollection)
                        if (cookie.Name == this.userSettings.Name)
                        {
                            this.userSettings.Value = cookie.Value;
                            this.userSettings.Domain = cookie.Domain;
                        }
                    // проверка
                    if (String.IsNullOrEmpty(this.userSettings.Value))
                        throw new InvalidOperationException("Cookie UserSettings not exist.");
                }
                LastException = null;
                return true;
            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return false;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                if (we.Status == WebExceptionStatus.Timeout)
                    LastStatusCode = HttpStatusCode.RequestTimeout;
                else
                    LastStatusCode = HttpStatusCode.BadRequest;
                LastException = we;
                return false;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return false;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }
        public PostTariffIndications CounterData(long flatId, DateTime fromDate, DateTime toDate, ProfileType profile, byte tariffNumber)
        {
            PostTariffIndications result = null;

            string url = @"{0}/ARMTES/api/SingleMeterApi/PostTariffIndications";
            url = string.Format(url, this.siteUrl);
            string domain = new Uri(url).Host;
            string postData = @"""FlatId"":""{0}"",""DateFrom"":""{1}"",""DateTo"":""{2}"",""ProfileId"":""{3}"",""TariffNumber"":""{4}"",""SectorTypeId"":""SES""";
            postData = string.Format(postData, flatId, ShortDate(fromDate), ShortDate(toDate), (byte)profile, tariffNumber);
            postData = "{" + postData + "}";

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(4);
            httpWebRequest.CookieContainer.Add(this.userSettings);
            httpWebRequest.CookieContainer.Add(this.aspxauth);
            httpWebRequest.CookieContainer.Add(new Cookie("SmallEngine_SingleMeterTab", "1") { Domain = domain });
            httpWebRequest.CookieContainer.Add(new Cookie("SmallObjects_AllObjectsTab", "1") { Domain = domain });

            byte[] buffer = Encoding.ASCII.GetBytes(postData);
            httpWebRequest.ContentLength = buffer.Length;
            httpWebRequest.ContentType = "application/json";
            using (Stream reqStream = httpWebRequest.GetRequestStream())
            {
                reqStream.Write(buffer, 0, buffer.Length);
                reqStream.Close();
            }

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "application/json; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                string json = streamReader.ReadToEnd();
                                System.Diagnostics.Debugger.Break();

                                result = System.Text.Json.JsonSerializer.Deserialize<PostTariffIndications>(json);
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;
            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                if (we.Status == WebExceptionStatus.Timeout)
                    LastStatusCode = HttpStatusCode.RequestTimeout;
                else
                    LastStatusCode = HttpStatusCode.BadRequest;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        /// <summary>
        /// История сеансов связи с устройством
        /// </summary>
        /// <param name="deviceId">Идентификатор устройства</param>
        /// <returns></returns>
        public DeviceSessionsData GetDeviceSessionHistory(long deviceId)
        {
            DeviceSessionsData result = null;

            string url = @"{0}/ARMTES/api/DeviceSessionApi/GetDeviceSessionHistory'";
            url = string.Format(url, this.siteUrl);
            string domain = new Uri(url).Host;
            string postData = @"{""DeviceId"":""{0}""}";
            postData = string.Format(postData, deviceId);

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(4);
            httpWebRequest.CookieContainer.Add(this.userSettings);
            httpWebRequest.CookieContainer.Add(this.aspxauth);
            httpWebRequest.CookieContainer.Add(new Cookie("SmallEngine_SingleMeterTab", "1") { Domain = domain });
            httpWebRequest.CookieContainer.Add(new Cookie("SmallObjects_AllObjectsTab", "1") { Domain = domain });

            byte[] buffer = Encoding.ASCII.GetBytes(postData);
            httpWebRequest.ContentLength = buffer.Length;
            httpWebRequest.ContentType = "application/json";
            using (Stream reqStream = httpWebRequest.GetRequestStream())
            {
                reqStream.Write(buffer, 0, buffer.Length);
                reqStream.Close();
            }

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "application/json; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                string json = streamReader.ReadToEnd();
                                System.Diagnostics.Debugger.Break();

                                result = System.Text.Json.JsonSerializer.Deserialize<DeviceSessionsData>(json);
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;
            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                if (we.Status == WebExceptionStatus.Timeout)
                    LastStatusCode = HttpStatusCode.RequestTimeout;
                else
                    LastStatusCode = HttpStatusCode.BadRequest;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        /// <summary>
        /// Возвращает список дочерних объектов СДСП указанного объекта
        /// </summary>
        /// <param name="parentId">ИД родительского объекта</param>
        /// <returns></returns>
        public List<ArmtesElement> GetElements(string parentId)
        {
            List<ArmtesElement> elements = null;

            string url = @"{0}/ARMTES/Home/GetElements?";
            url = string.Format(url, this.siteUrl);
            string queryData = "parentId={0}";
            queryData = string.Format(queryData, parentId);

            url = url + queryData;

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(2);
            httpWebRequest.CookieContainer.Add(this.userSettings);
            httpWebRequest.CookieContainer.Add(this.aspxauth);

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "application/json; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                /*
                                 [{"label":"Гродноэнерго","parentid":0,"value":"e2","items":[{"label":"Loading...","parentid":0,"value":"/ARMTES/Home/GetElements?parentId=e2","items":null}]}]
                                 */
                                string json = streamReader.ReadToEnd();
                                //System.Diagnostics.Debugger.Break();

                                elements = System.Text.Json.JsonSerializer.Deserialize<List<ArmtesElement>>(json);
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return elements;

            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                LastStatusCode = HttpStatusCode.InternalServerError;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }    
        }

        public PageResult<EnterpriseViewItem> GetEnterprises()
        {
            PageResult<EnterpriseViewItem> result = null;

            string url = @"{0}/ARMTES/api/SmallEngineApi/GetEnterprises";
            url = string.Format(url, this.siteUrl);

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");


            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "application/json; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                string json = streamReader.ReadToEnd();
                                //System.Diagnostics.Debugger.Break();

                                result = System.Text.Json.JsonSerializer.Deserialize<PageResult<EnterpriseViewItem>>(json);
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;

            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                LastStatusCode = HttpStatusCode.InternalServerError;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        public PageResult<ExportPersonalAccountViewItem> GetPersonalAccounts(string enterprise = "", SectorType sectorType = SectorType.SES)
        {
            PageResult<ExportPersonalAccountViewItem> result = null;

            string url = @"{0}/ARMTES/api/PersonalAccountsApi/GetPersonalAccounts?";
            url = string.Format(url, this.siteUrl);
            string queryData = "enterprise={0}&sectorTypeId={1}";
            queryData = string.Format(queryData, enterprise, sectorType);

            url = url + queryData;

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");


            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "application/json; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                string json = streamReader.ReadToEnd();
                                //System.Diagnostics.Debugger.Break();

                                result = System.Text.Json.JsonSerializer.Deserialize<PageResult<ExportPersonalAccountViewItem>>(json);
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;

            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                LastStatusCode = HttpStatusCode.InternalServerError;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        public PageResult<AllTariffsExportIndicationViewItem> GetSmallEngineExportIndications(DateTime date, string enterprise = "")
        {
            string dateAsString = date.ToString("d.M.yyyy", CultureInfo.InvariantCulture);

            PageResult<AllTariffsExportIndicationViewItem> result = null;

            string url = @"{0}/ARMTES/api/IndicationsExportApi/GetSmallEngineExportIndications?";
            url = string.Format(url, this.siteUrl);
            string queryData = "date={0}&enterprise={1}";
            queryData = string.Format(queryData, dateAsString, enterprise);

            url = url + queryData;

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");


            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "application/json; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                string json = streamReader.ReadToEnd();
                                //System.Diagnostics.Debugger.Break();

                                result = System.Text.Json.JsonSerializer.Deserialize<PageResult<AllTariffsExportIndicationViewItem>>(json);
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;

            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                LastStatusCode = HttpStatusCode.InternalServerError;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public string SelectElement(string objectId, DateTime fromDate, DateTime toDate, ProfileType profile, SectorType sector)
        {
            string result = String.Empty;

            // если аскуэ-быт
            if (sector == SectorType.HHS)
                this.userSettings.Value = "SectorTypeId=HHS";
            else
                // Мелкомоторный сектор
                this.userSettings.Value = "SectorTypeId=SES";

            string url = @"{0}/ARMTES/Home/SelectElement?";
            url = string.Format(url, this.siteUrl);
            string domain = new Uri(url).Host;
            string queryData = "objectId={0}&dateFrom={1}&dateTo={2}&profileId={3}&sectorTypeId={4}&filterMask=0";
            queryData = string.Format(queryData, objectId, ShortDate(fromDate), ShortDate(toDate), profile, sector);

            url = url + queryData;

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
                        
            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(4);
            httpWebRequest.CookieContainer.Add(this.userSettings);
            httpWebRequest.CookieContainer.Add(this.aspxauth);
            httpWebRequest.CookieContainer.Add(new Cookie("HouseHold_AllObjectsTab", "0") { Domain = domain });
            httpWebRequest.CookieContainer.Add(new Cookie("SmallEngine_SingleMeterTab", "1") { Domain = domain });

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "text/html; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                result = streamReader.ReadToEnd();
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;
            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                if (we.Status == WebExceptionStatus.Timeout)
                    LastStatusCode = HttpStatusCode.RequestTimeout;
                else
                    LastStatusCode = HttpStatusCode.BadRequest;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }           
        }

        public object test(string parentId)
        {
            List<object> elements = null;

            string url = @"{0}/ARMTES/SubscribersRegistryEdit/GetSubscribers?pagenum=1&pagesize=10&pagenum=1&pagesize=10";
            url = string.Format(url, this.siteUrl);


            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(2);
            httpWebRequest.CookieContainer.Add(this.userSettings);
            httpWebRequest.CookieContainer.Add(this.aspxauth);

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "application/json; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                /*
                                 [{"label":"Гродноэнерго","parentid":0,"value":"e2","items":[{"label":"Loading...","parentid":0,"value":"/ARMTES/Home/GetElements?parentId=e2","items":null}]}]
                                 */
                                string json = streamReader.ReadToEnd();
                                //System.Diagnostics.Debugger.Break();

                                /*
                                 * 
                                {"Subscribers":[{"SubscriberId":237955,"Name":"","PersonalAccount":"31091000392","SubscribersType":"Бытовой потребитель","City":"д. Стерково","Street":"","House":"СТП-2","Flat":"23","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"31091801641","SubscribersType":"Бытовой потребитель","City":"д. Бердовка","Street":"","House":"ЗТП-3","Flat":"Коренюка, 16","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"31090900312","SubscribersType":"Бытовой потребитель","City":"д. Стерково","Street":"","House":"СТП-2","Flat":"47","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"31091801611","SubscribersType":"Бытовой потребитель","City":"д. Бердовка","Street":"","House":"ЗТП-3","Flat":"Коренюка, 10","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"31091600481","SubscribersType":"Бытовой потребитель","City":"д. Бердовка","Street":"","House":"ЗТП-3","Flat":"Коренюка, хутор","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"31091801591","SubscribersType":"Бытовой потребитель","City":"д. Бердовка","Street":"","House":"ЗТП-3","Flat":"Коренюка, 14","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"31091800842","SubscribersType":"Бытовой потребитель","City":"д. Бердовка","Street":"","House":"ЗТП-3","Flat":"Советская, 38 кв.1","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"","SubscribersType":"Бытовой потребитель","City":"д. Бакуны","Street":"","House":"МТП-1","Flat":"хутор(Ковалева)","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"","SubscribersType":"Бытовой потребитель","City":"д. Бакуны","Street":"","House":"МТП-1","Flat":"хутор(Новокумская)","ContractNumber":null},{"SubscriberId":237955,"Name":"","PersonalAccount":"31091801621","SubscribersType":"Бытовой потребитель","City":"д. Бердовка","Street":"","House":"ЗТП-3","Flat":"Коренюка, 8","ContractNumber":null}],"TotalSubscribersCount":65435}

                                    */

                                elements = System.Text.Json.JsonSerializer.Deserialize<List<object>>(json);
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return elements;

            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                LastStatusCode = HttpStatusCode.InternalServerError;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        public string ViewMeter(string elementId, string selectedElementId, string parentSelectedDeviceId, DateTime fromDate, DateTime toDate, ProfileType profile, SectorType sector)
        {
            string result = String.Empty;

            // если аскуэ-быт
            if (sector == SectorType.HHS)
                this.userSettings.Value = "SectorType=HHS";
            else
                // Мелкомоторный сектор
                this.userSettings.Value = "SectorType=SES";

            string url = @"{0}/ARMTES/Home/ViewMeter?";
            url = string.Format(url, this.siteUrl);
            string domain = new Uri(url).Host;
            string queryData = "elementId={0}&selectedElementId={1}&parentSelectedDeviceId={2}&dateFrom={3}&dateTo={4}&profileId={5}&sectorTypeId={6}&filterMask=0";
            queryData = string.Format(queryData, elementId, selectedElementId, parentSelectedDeviceId, ShortDate(fromDate), ShortDate(toDate), profile, sector);

            url = url + queryData;

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(4);
            httpWebRequest.CookieContainer.Add(this.userSettings);
            httpWebRequest.CookieContainer.Add(this.aspxauth);
            httpWebRequest.CookieContainer.Add(new Cookie("HouseHold_AllObjectsTab", "0") { Domain = domain });
            httpWebRequest.CookieContainer.Add(new Cookie("SmallEngine_SingleMeterTab", "1") { Domain = domain });

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "text/html; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                result = streamReader.ReadToEnd();
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;
            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                if (we.Status == WebExceptionStatus.Timeout)
                    LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        public string ViewObject(string deviceId, string selectedElementId, DateTime fromDate, DateTime toDate, ProfileType profile, SectorType sector)
        {
            string result = String.Empty;

            // если аскуэ-быт
            if (sector == SectorType.HHS)
                this.userSettings.Value = "SectorType=HHS";
            else
                // Мелкомоторный сектор
                this.userSettings.Value = "SectorType=SES";

            string url = @"{0}/ARMTES/Home/ViewObject?";
            url = string.Format(url, this.siteUrl);
            string domain = new Uri(url).Host;
            string queryData = "deviceId={0}&selectedElementId={1}&dateFrom={2}&dateTo={3}&profileId={4}&sectorTypeId={5}&filterMask=0";
            queryData = string.Format(queryData, deviceId, selectedElementId, ShortDate(fromDate), ShortDate(toDate), profile, sector);

            url = url + queryData;

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            ConfigureRequest(ref httpWebRequest);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("X-Requested-With: XMLHttpRequest");
            
            // передаём куки
            httpWebRequest.CookieContainer = new CookieContainer(4);
            httpWebRequest.CookieContainer.Add(this.userSettings);
            httpWebRequest.CookieContainer.Add(this.aspxauth);
            httpWebRequest.CookieContainer.Add(new Cookie("SmallEngine_SingleMeterTab", "1") { Domain = domain });
            httpWebRequest.CookieContainer.Add(new Cookie("SmallObjects_AllObjectsTab", "0") { Domain = domain });

            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // сохраняем состояние ответа
                LastStatusCode = httpWebResponse.StatusCode;

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // получаем результат
                    if (httpWebResponse.ContentType == "text/html; charset=utf-8")
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream, new UTF8Encoding()))
                            {
                                try
                                {
                                    result = streamReader.ReadToEnd();
                                }
                                catch (WebException we) { return null; }
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("Invalid response content type");
                }
                LastException = null;
                return result;
            }
            catch (TimeoutException ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.RequestTimeout;
                LastException = ex;
                return null;
            }
            catch (WebException we)
            {
                _logger?.Error(we);
                if (we.Status == WebExceptionStatus.Timeout)
                    LastStatusCode = HttpStatusCode.RequestTimeout;
                else
                    LastStatusCode = HttpStatusCode.BadRequest;
                LastException = we;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                LastStatusCode = HttpStatusCode.BadRequest;
                LastException = ex;
                return null;
            }
            finally
            {
                if (httpWebResponse != null) httpWebResponse.Close();
            }            
        }
        #endregion

        #region Properties
        public bool IsAuthorized { get; private set; }

        public Exception LastException { get; private set; }

        /// <summary>
        /// Последнее состояние ответа
        /// </summary>
        public HttpStatusCode LastStatusCode { get; private set; }
/// <summary>
/// Таймаут в секундах
/// </summary>
public int TimeOut { get; set; }
#endregion

}
}
