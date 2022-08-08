namespace TMP.Common.NetHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public struct ServiceResult
    {
        private object Result { get; set; }

        public string ResultAsString
        {
            get
            {
                if (this.Result is string)
                {
                    return this.Result as string;
                }
                else
                {
                    return null;
                }
            }
        }

        public byte[] ResultBytes
        {
            get
            {
                if (this.Result is byte[])
                {
                    return this.Result as byte[];
                }
                else
                {
                    return null;
                }
            }
        }

        public int StatusCode { get; private set; }

        public Exception Exception { get; private set; }

        public string Error { get; private set; }

        public bool HasData => !string.IsNullOrEmpty(this.ResultAsString) || (this.ResultBytes != null && this.ResultBytes.Length > 0);

        public void SetData(object value, int code)
        {
            this.StatusCode = code;
            this.Result = value;
            this.Error = null;
        }

        public void SetError(Exception ex, int code, string error = null)
        {
            this.StatusCode = code;

            this.Result = null;
            this.Exception = ex;

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

            this.Error = sb.ToString();
        }
    }
}
