using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public static class ServiceHelper
    {
        private static string _userAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
        private static Cookie Cookie;

        public static HttpStatusCode StatusCode { get; set; }

        public static string ErrorMessage { get; private set; }

        public static string SiteAddress
        {
            get
            {
                return @"http://" + Properties.Settings.Default.ServerAddress + @"/"
                  + (String.IsNullOrEmpty(Properties.Settings.Default.SiteName) ? String.Empty : (Properties.Settings.Default.SiteName + @"/"));
            }
        }
        private static string GetDomain()
        {
            if (Properties.Settings.Default.ServerAddress.Contains(":"))
                return Properties.Settings.Default.ServerAddress.Substring(0, Properties.Settings.Default.ServerAddress.IndexOf(':'));
            else
                return Properties.Settings.Default.ServerAddress;
        }
        public static string StatusCodeAsString
        {
            get
            {
                switch (StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                            return "OK - операция выполнена успешно";
                        case System.Net.HttpStatusCode.Found:
                            return "302 - перемещено временно";
                        case System.Net.HttpStatusCode.NotModified:
                            return "304 - не изменялось";
                        case System.Net.HttpStatusCode.BadRequest:
                            return "400 - плохой, неверный запрос";
                        case System.Net.HttpStatusCode.Unauthorized:
                            return "401 - не авторизован";
                        case System.Net.HttpStatusCode.Forbidden:
                            return "403 - запрещено";
                        case System.Net.HttpStatusCode.NotFound:
                            return "404 - не найдено";
                        case System.Net.HttpStatusCode.RequestTimeout:
                            return "408 - истекло время ожидания";
                        case System.Net.HttpStatusCode.LengthRequired:
                            return "411 - необходима длина";
                        case System.Net.HttpStatusCode.RequestEntityTooLarge:
                            return "413 - размер запроса слишком велик";
                        case System.Net.HttpStatusCode.RequestUriTooLong:
                            return "414 - запрашиваемый URI слишком длинный";
                        case System.Net.HttpStatusCode.InternalServerError:
                            return "500 - внутренняя ошибка сервера";
                        case System.Net.HttpStatusCode.NotImplemented:
                            return "501 - не реализовано";
                        case System.Net.HttpStatusCode.BadGateway:
                            return "502 - плохой, ошибочный шлюз";
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                            return "503 - сервис недоступен";
                        case System.Net.HttpStatusCode.GatewayTimeout:
                            return "504 - шлюз не отвечает";
                        default:
                            return String.Format("{0} - {1}", StatusCode, System.Web.HttpWorkerRequest.GetStatusDescription((int)StatusCode));
                    }
            }
        }
        public static async Task<bool> IsServerOnline()
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.ServerAddress))
                throw new ArgumentNullException("ServerAddress");

            var pingSender = new Ping();
            PingReply reply;

            try
            {
                if (Properties.Settings.Default.ServerAddress.Contains(":"))
                    reply = await pingSender.SendPingAsync(Properties.Settings.Default.ServerAddress.Substring(0, Properties.Settings.Default.ServerAddress.IndexOf(':')), 100);
                else
                    reply = await pingSender.SendPingAsync(Properties.Settings.Default.ServerAddress, 100);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    ErrorMessage = "Сервер не доступен.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = App.GetExceptionDetails(ex);
                return false;
            }
            finally
            {
                if (pingSender != null) pingSender.Dispose();
                pingSender = null;
                reply = null;
            }
        }

        public static bool IsSiteOnline()
        {
            if (Uri.CheckHostName(SiteAddress) == UriHostNameType.Unknown)
                throw new ArgumentNullException("SiteAddress");
            if (String.IsNullOrEmpty(SiteAddress))
                throw new ArgumentNullException("SiteAddress");

            Boolean ret = false;

            try
            {
                var req = (HttpWebRequest)HttpWebRequest.Create(SiteAddress + "/");
                req.Method = "GET";
                req.KeepAlive = false;
                var resp = (HttpWebResponse)req.GetResponse();
                req.Accept = "*/*";
                req.Headers.Add("Accept-Encoding: gzip, deflate");
                req.UserAgent = _userAgent;
                req.AllowAutoRedirect = false;

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    // HTTP = 200 - Internet connection available, server online
                    ret = true;
                }
                resp.Close();
                return ret;
            }
            catch (Exception ex)
            {
                ErrorMessage = App.GetExceptionDetails(ex);
                return false;
            }
        }

        private static bool TryAddCookie(ref HttpWebRequest request)
        {
            if (request == null || Cookie == null)
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
                ErrorMessage = App.GetExceptionDetails(ex);
                return true;
            }
            return true;
        }

        public static string DecodeAnswer(string input)
        {
            try
            {
                return System.Web.HttpUtility.UrlDecode(input);
                //return Uri.UnescapeDataString(answer);
            }
            catch (Exception e)
            {
                var s = e.Message;
                return null;
            }
        }

        public static async Task<bool> Login(bool getRights = false)
        {
            var url = @"{0}scripts/autentification.asp";
            url = string.Format(url, SiteAddress);
            string data;
            string answer;
            if (getRights)
            {
                data = "action=getrights";
                answer = await MakeRequestAsync(url, data, false);
            }
            else
            {
                data = string.Format("user={0}&password={1}&action=login", Properties.Settings.Default.UserName, Properties.Settings.Default.Password);
                answer = await MakeRequestAsync(url, data);
            }
            if (string.IsNullOrEmpty(answer))
            {
                ErrorMessage = StatusCodeAsString;
                return false;
            }
            ErrorMessage = answer;
            if (answer.Contains("result=0"))
            {
                if (answer.Contains("user=" + Properties.Settings.Default.UserName))
                {
                    ErrorMessage = string.Empty;
                    return true;
                }
                if (getRights == false)
                    return true;
                else
                    return false;
            }
            if (answer.Contains("result=1") && answer.Contains("errType=2"))
            {
                return false;
            }
            ErrorMessage = answer;
            return false;
        }

        public static Task<string> MakeRequestAsync(string url, string data, bool useCookie = true)
        {
            url = string.Format(url, SiteAddress);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Date = DateTime.Now;
            httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpWebRequest.UserAgent = _userAgent;
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.Timeout = Properties.Settings.Default.NetTimeOutInSeconds * 1000;
            //httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = WebRequestMethods.Http.Post;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers.Add("Accept-Encoding: gzip, deflate");

            if (useCookie)
                // передаём куки
                if (Cookie != null && TryAddCookie(ref httpWebRequest) == false)
                {
                    return Task.FromResult(String.Empty);
                }
            data = Uri.EscapeUriString(data).Replace("_", "%5F");
            var buffer = Encoding.ASCII.GetBytes(data);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            try
            {
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
                    HttpWebResponse response = t.Result as HttpWebResponse;
                    StatusCode = response.StatusCode;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return String.Empty;
                    }

                    var cookiekeys = response.Headers.AllKeys.Where(h => h == "Set-Cookie");
                    if (cookiekeys != null && cookiekeys.Count() > 0)
                    {
                        Cookie = new Cookie("ASPSESSIONIDQSATBDCD", string.Empty);
                        // получаем куки
                        Cookie.Value = string.Empty;

                        // Set-Cookie: ASPSESSIONIDQSATBDCD=OMEJJIGBLEAOINHJMCOELNGH; path=/
                        var value = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
                        var parts = value.Split(new char[] { ';' });
                        if (parts.Length > 0)
                        {
                            foreach (string part in parts)
                            {
                                if (part.StartsWith("ASPSESSIONID"))
                                {
                                    var kvp = Utils.ParseKeyValuePair(part);

                                    Cookie.Name = kvp.Key;
                                    Cookie.Value = kvp.Value;
                                    Cookie.Domain = GetDomain();
                                }
                            }
                        }
                    }
                    string answer = String.Empty;
                    var responseStream = response.GetResponseStream();
                    using (System.IO.StreamReader streamReader = new System.IO.StreamReader(responseStream, new UTF8Encoding()))
                    {
                        answer = streamReader.ReadToEnd();
                    }
                    return answer;
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = App.GetExceptionDetails(ex);
                return Task.FromResult(String.Empty);
            }
        }

        public static async Task<string> GetPoints(ListPoint parent)
        {
            ErrorMessage = String.Empty;
            string url = @"{0}scripts/point.asp";
            string data = String.Format("refresh=true&TYPE=GROUP&action=expand&ID={0}", parent.Id.ToString());

            return await ExecuteFunctionAsync(MakeRequestAsync, url, data, true, (answer) => DecodeAnswer(answer));
        }
        public static IList<ListPoint> GetPointsList(string data, ListPoint parent)
        {
            var records = Utils.ParseRecords(data);
            if (records == null)
                return new List<ListPoint>();

            var list = new List<Model.IEmcosElement>();
            foreach (var nvc in records)
            {
                Emcos.Model.IEmcosElement element;
                if (nvc.Get("Type") == "POINT")
                    element = new Model.EmcosPointElement();
                else
                    element = new Model.EmcosGrElement();

                for (int i = 0; i < nvc.Count; i++)
                {
                    #region Разбор полей
                    int intValue = 0;
                    switch (nvc.GetKey(i))
                    {
                        case "GR_ID":
                        case "POINT_ID":
                            int.TryParse(nvc[i], out intValue);
                            element.Id = intValue;
                            break;
                        case "GR_NAME":
                        case "POINT_NAME":
                            element.Name = nvc[i];
                            break;
                        case "ECP_NAME":
                            Model.EmcosPointElement pe = (element as Model.EmcosPointElement);
                            pe.EcpName = nvc[i];
                            break;
                        case "TYPE":
                            Model.ElementTypes type;
                            if (Enum.TryParse<Model.ElementTypes>(nvc[i], out type) == false)
                                type = Model.ElementTypes.GROUP;
                            element.Type = type;
                            break;
                        case "GR_TYPE_CODE":
                        case "POINT_TYPE_CODE":
                            element.TypeCode = nvc[i];
                            break;
                    }
                    #endregion
                }
                list.Add(element);
            }
            IList<ListPoint> points = list.Select(i => new ListPoint
            {
                Id = i.Id,
                Name = i.Name,
                IsGroup = i.Type == Model.ElementTypes.GROUP,
                TypeCode = i.TypeCode,
                EсpName = i is Model.EmcosPointElement ? (i as Model.EmcosPointElement).EcpName : String.Empty,
                Type = i.Type,
                Checked = false,
                ParentId = parent.Id,
                ParentTypeCode = parent.TypeCode,
                ParentName = parent.Name
            }).ToList();
            return points;
        }

        public static async Task<IList<ListPoint>> CreatePointsListAsync(ListPoint parent)
        {
            try
            {
                string data = await GetPoints(parent);

                if (String.IsNullOrEmpty(data))
                    return new List<ListPoint>();

                return GetPointsList(data, parent);
            }
            catch (Exception ex)
            {
                ErrorMessage = App.GetExceptionDetails(ex);
                return new List<ListPoint>();
            }
        }

        private static int _attemptCount = 0;
        public static async Task<string> ExecuteFunctionAsync(Func<string, string, bool, Task<string>> function, string param1, string param2, bool param3, Func<string, string> postAction = null)
        {
            string answer;
            if (Cookie == null)
            {
                // нет куков - логин
                bool hasRights = await ServiceHelper.Login();
                answer = ServiceHelper.ErrorMessage;
                // если доступ не получен и не авторизованы
                if (hasRights == false)
                {
                    // ещё раз
                    hasRights = await ServiceHelper.Login();
                    // авторизация не успешна
                    if (hasRights == false)
                        return string.Empty;
                }
            }

            answer = await function(param1, param2, param3);
            // нет резульатата - что-то не так с сервером - возврат пустого значения
            if (String.IsNullOrEmpty(answer))
                return string.Empty;
            // Пользователь незарегистрирован. Необходима повторная регистрация ?
            // &result=1&errDescription=Пользователь незарегистрирован. Необходима повторная регистрация.&errType=2
            if (answer.Contains("result=1") && answer.Contains("errType=2") && _attemptCount < 4)
            {
                _attemptCount++;
                Login();
                answer = await ExecuteFunctionAsync(function, param1, param2, param3, postAction);
                if (postAction != null)
                    answer = postAction(answer);
                return answer;
            }
            // есть результат, но неверный ответ
            if (answer == "result=0&user=")
            {
                _attemptCount++;
                Login();
                answer = await ExecuteFunctionAsync(function, param1, param2, param3, postAction);
                if (postAction != null)
                    answer = postAction(answer);
                return answer;
            }
            // итак, ответ есть, проверка результата
            if (answer.Contains("result=0"))
            {
                ErrorMessage = string.Empty;
                if (postAction != null)
                    answer = postAction(answer);
                return answer;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}