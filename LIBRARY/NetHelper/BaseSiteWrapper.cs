using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

using TMP.Common.Logger;

namespace TMP.Common.NetHelper
{
    public abstract class BaseSiteWrapper
    {
        #region Fields

        protected const string _chromeUserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
        protected static ManualResetEvent _allDone = new ManualResetEvent(false);

        protected ILoggerFacade Log;

        protected CancellationTokenSource _cts;

        public const int EMPTY_STATUS_CODE = -1;

        #endregion Fields

        #region Constructor

        protected BaseSiteWrapper()
        {
            var appWithLogger = System.Windows.Application.Current as IAppWithLogger;
            if (appWithLogger != null)
                Log = appWithLogger.Log;
            else
                System.Diagnostics.Debug.WriteLine("Application as IAppWithLogger not found!");
        }

        #endregion Constructor

        #region Private and Internal methods

        protected virtual void ConfigureRequest(ref HttpWebRequest httpWebRequest)
        {
            httpWebRequest.Timeout = this.TimeOut * 1000;
            httpWebRequest.ReadWriteTimeout = this.TimeOut * 1000;
            httpWebRequest.Date = DateTime.Now;
            httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpWebRequest.UserAgent = _chromeUserAgent;
            httpWebRequest.AllowAutoRedirect = false;
            //httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.KeepAlive = true;
        }

        protected string GetDomain()
        {
            if (ServerAddress.Contains(":"))
                return ServerAddress.Substring(0, ServerAddress.IndexOf(':'));
            else
                return ServerAddress;
        }

        protected bool TryAddCookie(ref HttpWebRequest request)
        {
            if (request == null)
            {
                return false;
            }
            if (request.CookieContainer == null)
                request.CookieContainer = new CookieContainer(1);
            try
            {
                request.CookieContainer.Add(Cookie);
            }
            catch (Exception ex)
            {
                Log?.Log("Не удалось добавить куки. Ошибка: " + ex.Message, Category.Exception, Priority.High);
                return true;
            }
            return true;
        }

        #endregion Private and Internal methods

        #region Public Methods

        public bool IsServerOnline()
        {
            if (String.IsNullOrEmpty(ServerAddress))
                throw new ArgumentNullException("ServerAddress");

            var pingSender = new Ping();
            PingReply reply;

            try
            {
                if (ServerAddress.Contains(":"))
                    reply = pingSender.Send(ServerAddress.Substring(0, ServerAddress.IndexOf(':')), 100);
                else
                    reply = pingSender.Send(ServerAddress, 100);
                if (reply.Status == IPStatus.Success)
                {
                    Log?.Log("Проверка доступности сервера - доступен", Category.Info, Priority.None);
                    return true;
                }
                else
                {
                    Log?.Log("Проверка доступности сервера - не доступен. Ответ: " + reply.Status, Category.Info, Priority.None);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log?.Log("Проверка доступности сервера - ошибка: " + ex.Message, Category.Exception, Priority.High);
                return false;
            }
            finally
            {
                if (pingSender != null) pingSender.Dispose();
                pingSender = null;
                reply = null;
            }
        }

        public bool IsSiteOnline()
        {
            if (Uri.CheckHostName(SiteAddress) == UriHostNameType.Unknown)
                throw new ArgumentNullException("SiteAddress");
            if (String.IsNullOrEmpty(SiteAddress))
                throw new ArgumentNullException("SiteAddress");

            Boolean ret = false;

            try
            {
                var req = (HttpWebRequest)HttpWebRequest.Create(this.SiteAddress + "/");
                req.Method = "GET";
                req.KeepAlive = false;
                var resp = (HttpWebResponse)req.GetResponse();
                req.Accept = "*/*";
                req.Headers.Add("Accept-Encoding: gzip, deflate");
                req.UserAgent = _chromeUserAgent;
                req.AllowAutoRedirect = false;

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    // HTTP = 200 - Internet connection available, server online
                    ret = true;
                }
                resp.Close();
                Log?.Log("Проверка доступности сайта, ответ:" + resp.StatusCode, Category.Info, Priority.None);
                return ret;
            }
            catch (Exception ex)
            {
                Log?.Log("Проверка доступности сайта, ошибка:" + ex.Message, Category.Exception, Priority.High);
                return false;
            }
        }

        public ServiceResult SendRequestWithBody(string url, byte[] data)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // настройка запроса
                ConfigureRequest(ref httpWebRequest);
                httpWebRequest.Method = WebRequestMethods.Http.Post;

                // содержимое запроса
                httpWebRequest.ContentLength = data.Length;
                using (Stream stream = httpWebRequest.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    Stream streamToRead = responseStream;
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }

                    using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        string answer = streamReader.ReadToEnd();
                        result.SetData(answer, (int)response.StatusCode);

                    }
                }
            }
            #region EXCEPTIONS
            catch (System.Net.WebException we)
            {
                System.Net.WebResponse response = we.Response;
                string message = null;

                if (response != null)
                {
                    using (var stream = we.Response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        message = reader.ReadToEnd();
                    }
                }
                result.SetError(we, (int)we.Status, message);
                return result;
            }
            catch (Exception ex)
            {
                result.SetError(ex, EMPTY_STATUS_CODE);
                return result;
            }
            #endregion
            return result;
        }
        public async Task<ServiceResult> SendRequestWithBodyAsync(string url, byte[] data)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // настройка запроса
                ConfigureRequest(ref httpWebRequest);
                httpWebRequest.Method = WebRequestMethods.Http.Post;

                // содержимое запроса
                httpWebRequest.ContentLength = data.Length;
                using (Stream stream = httpWebRequest.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (HttpWebResponse response = (HttpWebResponse) await httpWebRequest.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                {
                    Stream streamToRead = responseStream;
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }

                    using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        string answer = streamReader.ReadToEnd();
                        result.SetData(answer, (int)response.StatusCode);

                    }
                }
            }
            #region EXCEPTIONS
            catch (System.Net.WebException we)
            {
                System.Net.WebResponse response = we.Response;
                string message = null;

                if (response != null)
                {
                    using (var stream = we.Response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        message = reader.ReadToEnd();
                    }
                }
                result.SetError(we, (int)we.Status, message);
                return result;
            }
            catch (Exception ex)
            {
                result.SetError(ex, EMPTY_STATUS_CODE);
                return result;
            }
            #endregion
            return result;
        }

        public ServiceResult SendRequest(string url)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // настройка запроса
                ConfigureRequest(ref httpWebRequest);
                httpWebRequest.Method = WebRequestMethods.Http.Get;

                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    Stream streamToRead = responseStream;
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentType == "image/png")
                    {
                        MemoryStream ms = new MemoryStream();
                        streamToRead.CopyTo(ms);

                        byte[] buffer = ms.ToArray();
                        result.SetData(buffer, (int)response.StatusCode);
                        return result;
                    }

                    using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        string answer = streamReader.ReadToEnd();
                        result.SetData(answer, (int)response.StatusCode);
                    }
                }
            }
            #region EXCEPTIONS
            catch (System.Net.WebException we)
            {
                System.Net.WebResponse response = we.Response;
                string message = null;

                if (response != null)
                {
                    using (var stream = we.Response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        message = reader.ReadToEnd();
                    }
                }
                result.SetError(we, (int)we.Status, message);
                return result;
            }
            catch (Exception ex)
            {
                result.SetError(ex, EMPTY_STATUS_CODE);
                return result;
            }
            #endregion
            return result;
        }
        public async Task<ServiceResult> SendRequestAsync(string url)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // настройка запроса
                ConfigureRequest(ref httpWebRequest);
                httpWebRequest.Method = WebRequestMethods.Http.Get;

                using (HttpWebResponse response = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                {
                    Stream streamToRead = responseStream;
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentType == "image/png")
                    {
                        MemoryStream ms = new MemoryStream();
                        streamToRead.CopyTo(ms);

                        byte[] buffer = ms.ToArray();
                        result.SetData(buffer, (int)response.StatusCode);
                        return result;
                    }

                    using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        string answer = streamReader.ReadToEnd();
                        result.SetData(answer, (int)response.StatusCode);
                    }
                }
            }
            #region EXCEPTIONS
            catch (System.Net.WebException we)
            {
                System.Net.WebResponse response = we.Response;
                string message = null;

                if (response != null)
                {
                    using (var stream = we.Response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        message = reader.ReadToEnd();
                    }
                }
                result.SetError(we, (int)we.Status, message);
                return result;
            }
            catch (Exception ex)
            {
                result.SetError(ex, EMPTY_STATUS_CODE);
                return result;
            }
            #endregion
            return result;
        }
        public async Task<System.Windows.Media.Imaging.BitmapImage> GetImage(string url)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // настройка запроса
                ConfigureRequest(ref httpWebRequest);
                httpWebRequest.Method = WebRequestMethods.Http.Get;

                using (HttpWebResponse response = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                {
                    Stream streamToRead = responseStream;
                    if (response.ContentType == "image/png")
                    {
                        System.Windows.Media.Imaging.BitmapImage bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = streamToRead;
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        return bitmap;
                    }
                    else
                        return null;
                }
            }
            #region EXCEPTIONS
            catch
            {
                return null;
            }
            #endregion
        }

        #endregion Public Methods

        #region CallBacks

        protected string ReadStreamFromResponse(WebResponse response)
        {
            var responseStream = response.GetResponseStream();
            using (System.IO.StreamReader streamReader = new System.IO.StreamReader(responseStream, new UTF8Encoding()))
            {
                var answer = streamReader.ReadToEnd();
                return System.Web.HttpUtility.UrlDecode(answer);
                //return Uri.UnescapeDataString(answer);
            }
        }

        #endregion CallBacks

        #region Properties

        public CancellationTokenSource Cts
        {
            get { if (_cts == null) _cts = new CancellationTokenSource(); return _cts; }
            set { _cts = value; }
        }

        public Cookie Cookie { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; } = "sbyt";
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; } = "sbyt";

        public string ServerAddress { get; set; }
        public string ServiceName { get; set; }
        public string WebServicePath { get; set; }

        public string SiteAddress {
            get
            {
                return @"http://" + ServerAddress + @"/" 
                    + (String.IsNullOrEmpty(ServiceName) ? String.Empty : (ServiceName + @"/"))
                    + (String.IsNullOrEmpty(WebServicePath) ? String.Empty : (WebServicePath + @"/"));
            }
        }

        /// <summary>
        /// Таймаут в секундах
        /// </summary>
        public int TimeOut { get; set; } = 120;

        #endregion Properties
    }
}