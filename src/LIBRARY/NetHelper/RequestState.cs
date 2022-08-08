namespace TMP.Common.NetHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;

    // The RequestState class passes data across async calls.
    public class RequestState
    {
        public int BUFFER_SIZE { get; private set; }

        public StringBuilder RequestData;
        public byte[] BufferRead;
        public HttpWebRequest Request;
        public Stream ResponseStream;

        public List<byte[]> ListOfBuffers;

        // Create Decoder for appropriate enconding type.
        public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

        public long BytesReaded;
        public long BytesToRead;

        public int Progress
        {
            get
            {
                long pos = this.BytesToRead == 0 ? 0 : this.BytesReaded * 100 / this.BytesToRead;
                return Math.Min(100, (int)pos);
            }
        }

        public RequestState(int buffeSize = 10240)
        {
            this.BUFFER_SIZE = buffeSize;
            this.BufferRead = new byte[buffeSize];
            this.RequestData = new StringBuilder(string.Empty);
            this.Request = null;
            this.ResponseStream = null;

            this.ListOfBuffers = new List<byte[]>();
        }
    }
}
