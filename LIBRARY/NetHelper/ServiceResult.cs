using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Common.NetHelper
{
    public struct ServiceResult
    {
        private object Result { get; set; }

        public string ResultAsString { get { if (Result is String) return Result as String; else return null; } }
        public byte[] ResultBytes { get { if (Result is byte[]) return Result as byte[]; else return null; } }
        public int StatusCode { get; private set; }
        public Exception Exception { get; private set; }
        public string Error { get; private set; }

        public bool HasData { get { return !String.IsNullOrEmpty(ResultAsString) || (ResultBytes != null && ResultBytes.Length > 0); } }

        public void SetData(object value, int code)
        {
            StatusCode = code;
            Result = value;
            Error = null;
        }

        public void SetError(Exception ex, int code, string error = null)
        {
            StatusCode = code;

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
