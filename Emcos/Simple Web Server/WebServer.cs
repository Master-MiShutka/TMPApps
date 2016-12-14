using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;

namespace Simple_Web_Server
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        public delegate void CallBack(ref HttpListenerContext ctx);
        private readonly CallBack _callback;

        public WebServer(string[] prefixes, CallBack callback)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (callback == null)
                throw new ArgumentException("callback");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _callback = callback;
            _listener.Start();
        }

        public WebServer(CallBack callback, params string[] prefixes)
            : this(prefixes, callback)
        { }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                _callback(ref ctx);                                                                                                
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debugger.Log(0, "ERR", e.Message);
                            } // suppress any exceptions                            
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
