using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace TMP.Common.NetHelper
{
    public class BufferedAsyncNetClient : IDisposable
    {
        private ManualResetEvent allDone = new ManualResetEvent(false);
        ~BufferedAsyncNetClient()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (allDone != null)
                allDone.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private string _address = String.Empty;

        public delegate void ConfigureRequest(ref HttpWebRequest request);

        public ConfigureRequest Configure { get; set; }
        public Action<int, string> UpdateCallback { get; set; }

        public BufferedAsyncNetClient(string address)
        {
            if (String.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Site address");
            _address = address;
        }
        
        public string Get(string sendParam)
        {
            allDone = new ManualResetEvent(false);

            // Create the request object.
            var httpWebRequest = WebRequest.Create(_address) as HttpWebRequest;

            // Create the state object.
            var requestState = new RequestState();

            if (UpdateCallback != null)
                UpdateCallback(requestState.Progress, "Подготовка запроса");

            // дополнительная настройка
            if (Configure != null)
                Configure(ref httpWebRequest);

            httpWebRequest.Timeout = Timeout.Infinite;
            httpWebRequest.ReadWriteTimeout = Timeout.Infinite;

            var buffer = Encoding.UTF8.GetBytes(sendParam);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            using (var postStream = httpWebRequest.GetRequestStream())
            {
                postStream.Write(buffer, 0, buffer.Length);
            }

            // Put the request into the state object so it can be passed around.
            requestState.Request = httpWebRequest;

            if (UpdateCallback != null)
                UpdateCallback(requestState.Progress, "Отправка запроса");

            // Issue the async request.
            var asyncResult = (IAsyncResult)httpWebRequest.BeginGetResponse(
               new AsyncCallback(ResponseCallback), requestState);

            // Wait until the ManualResetEvent is set so that the application 
            // does not exit until after the callback is called.
            allDone.WaitOne();

            //**************
            int bufsize = 0;
            requestState.ListOfBuffers.ForEach((i) => bufsize += i.Length);

            var resultbuffer = new byte[bufsize];
            int resultBufCurIndex = 0;
            for (int i = 0; i < requestState.ListOfBuffers.Count; i++)
            {
                Array.ConstrainedCopy(requestState.ListOfBuffers[i], 0, resultbuffer, resultBufCurIndex, requestState.ListOfBuffers[i].Length);
                resultBufCurIndex += requestState.ListOfBuffers[i].Length;
            }

            // Prepare a Char array buffer for converting to Unicode.
            var charBuffer = new Char[bufsize];

            // Convert byte stream to Char array and then to String.
            // len contains the number of characters converted to Unicode.
            int len = requestState.StreamDecode.GetChars(resultbuffer, 0, bufsize, charBuffer, 0);

            var str = System.Web.HttpUtility.UrlDecode(resultbuffer, 0, bufsize, Encoding.UTF8);
            //**************

            return str;
        }
        private void ResponseCallback(IAsyncResult asyncResult)
        {
            // Get the RequestState object from the async result.
            var requestState = (RequestState)asyncResult.AsyncState;

            // Get the WebRequest from RequestState.
            var httpWebRequest = requestState.Request;

            // Call EndGetResponse, which produces the WebResponse object
            //  that came from the request issued above.
            var webResponse = httpWebRequest.EndGetResponse(asyncResult);

            System.Diagnostics.Debug.WriteLine("[ResponseCallback] ContentLength=" + webResponse.ContentLength);
            requestState.BytesToRead = webResponse.ContentLength;

            //  Start reading data from the response stream.
            var responseStream = webResponse.GetResponseStream();

            // Store the response stream in RequestState to read 
            // the stream asynchronously.
            requestState.ResponseStream = responseStream;

            if (UpdateCallback != null)
                UpdateCallback(requestState.Progress, "Получение данных");

            //  Pass rs.BufferRead to BeginRead. Read data into rs.BufferRead
            var asyncResultRead = responseStream.BeginRead(requestState.BufferRead, 0, requestState.BUFFER_SIZE, new AsyncCallback(ReadCallBack), requestState);
        }


        private void ReadCallBack(IAsyncResult asyncResult)
        {
            // Get the RequestState object from AsyncResult.
            var requestState = (RequestState)asyncResult.AsyncState;

            // Retrieve the ResponseStream that was set in RespCallback. 
            var responseStream = requestState.ResponseStream;

            // Read requestState.BufferRead to verify that it contains data. 
            int read = responseStream.EndRead(asyncResult);
            if (read > 0)
            {
                var buffer = new byte[read];
                Array.ConstrainedCopy(requestState.BufferRead, 0, buffer, 0, read);

                requestState.ListOfBuffers.Add(buffer);

                requestState.BytesReaded += read;

                System.Diagnostics.Debug.WriteLine("[ReadCallBack] read=" + requestState.Progress);
                if (UpdateCallback != null)
                    UpdateCallback(requestState.Progress, String.Empty);

                // Continue reading data until responseStream.EndRead returns –1.
                var ar = responseStream.BeginRead(requestState.BufferRead, 0, requestState.BUFFER_SIZE, new AsyncCallback(ReadCallBack), requestState);
            }
            else
            {
                // Close down the response stream.
                responseStream.Close();

                System.Diagnostics.Debug.WriteLine("[ReadCallBack] Ended");
                if (UpdateCallback != null)
                    UpdateCallback(requestState.Progress, null);

                // Set the ManualResetEvent so the main thread can exit.
                allDone.Set();
            }
            return;
        }
    }
}
