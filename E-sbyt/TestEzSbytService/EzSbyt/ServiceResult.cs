using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.AmperM.TestApp.EzSbyt
{
    public struct ServiceResult
    {
        public string Result { get; set; }
        public int StatusCode { get; set; }
        public Exception Exception { get; private set; }
        public string Error { get; private set; }

        public bool HasData { get { return !String.IsNullOrEmpty(Result); } }

        public void SetError(Exception ex, string error = null)
        {
            Result = null;
            Exception = ex;

            Exception e = ex;
            StringBuilder sb = new StringBuilder();
            while (e != null)
            {
                sb.Append(e.Message);
                sb.Append(Environment.NewLine);
                e = e.InnerException;
            }

            if (error != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(error);
            }

            Error = sb.ToString();
        }
    }
}
