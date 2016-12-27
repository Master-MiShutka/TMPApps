using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.IO;
using System.IO.Compression;

using TMP.Common.Logger;
using TMP.Common.NetHelper;

namespace TMP.Work.AmperM.TestApp.EzSbyt
{
    public sealed class EzSbytService : BaseSiteWrapper
    {
        #region Singleton

        private static volatile EzSbytService instance;
        private static readonly object syncRoot = new Object();
        public static EzSbytService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new EzSbytService();
                    }
                }

                return instance;
            }
        }

        #endregion
        #region Constructor

        public EzSbytService()
        {
            this.ServerAddress = Properties.Settings.Default.ServerAddress ?? "10.182.5.13";
            this.ServiceName = Properties.Settings.Default.ServerAddress ?? "esbyt";
            this.WebServicePath = Properties.Settings.Default.ServerAddress ?? "hs/web";
            this.TimeOut = Properties.Settings.Default.TimeOutInSeconds;

            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TimeOutInSeconds")
                this.TimeOut = Properties.Settings.Default.TimeOutInSeconds;
        }

        #endregion

        #region Properties

        #endregion

        #region Private Methods

        protected override void ConfigureRequest(ref HttpWebRequest httpWebRequest)
        {
            httpWebRequest.Timeout = this.TimeOut * 1000;
            httpWebRequest.ReadWriteTimeout = this.TimeOut * 1000;
            httpWebRequest.Date = DateTime.Now;
            httpWebRequest.Accept = "text/plain, */*; q=0.01";
            httpWebRequest.Headers.Add("Origin", @"http://10.182.5.13");
            httpWebRequest.Headers.Add("X-Requested-With", @"XMLHttpRequest");
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.Referer = "http://10.182.5.13/jqsbyt/";
            httpWebRequest.UserAgent = _chromeUserAgent;
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            //httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            httpWebRequest.KeepAlive = true;

            // Disable 'Expect: 100-continue' behavior. More info: http://haacked.com/archive/2004/05/15/http-web-request-expect-100-continue.aspx
            httpWebRequest.ServicePoint.Expect100Continue = false;
        }

        #endregion

        #region Public Methods

        public bool IsServiceOnline()
        {
            return this.IsServerOnline();
        }

        public ServiceResult FuncRequest(string funcName, string parameters, string body)
        {
            ServiceResult result = new ServiceResult();
            if (parameters.Contains("json"))
              result.SetData(System.IO.File.ReadAllText(@"..\..\сморгонь.json", Encoding.UTF8), 0);
            else
              result.SetData(System.IO.File.ReadAllText(@"..\..\остр.valuetable", Encoding.UTF8), 0);
            return result;


            if (Cts == null)
                Cts = new CancellationTokenSource();
            string url = String.Format("http://{0}/{1}/{2}/{3}?{4}",
                Properties.Settings.Default.ServerAddress,//0
                Properties.Settings.Default.ServiceName,//1
                Properties.Settings.Default.WebServicePath,//2
                funcName,//3
                parameters); //4
            // это длинный запрос?
            if (String.IsNullOrEmpty(body) == false)
            {
                // содержимое запроса
                // кодирование
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);

                return this.SendRequestWithBody(url, postBytes);
            }
            else return this.SendRequest(url);
        }
        public async Task<ServiceResult> FuncRequestAsync(string funcName, string parameters, string body)
        {
            if (Cts == null)
                Cts = new CancellationTokenSource();
            string url = String.Format("http://{0}/{1}/{2}/{3}?{4}",
                Properties.Settings.Default.ServerAddress,//0
                Properties.Settings.Default.ServiceName,//1
                Properties.Settings.Default.WebServicePath,//2
                funcName,//3
                parameters); //4
                             // это длинный запрос?
            if (String.IsNullOrEmpty(body) == false)
            {
                // содержимое запроса
                // кодирование
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);

                return await this.SendRequestWithBodyAsync(url, postBytes);
            }
            else return await this.SendRequestAsync(url);
        }
        #endregion
    }
}