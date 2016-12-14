using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace TMP.Common.NetHelper
{
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
                long pos = BytesToRead == 0 ? 0 : BytesReaded * 100 / BytesToRead;
                return Math.Min(100, (int)pos);
            }
        }

        public RequestState(int buffeSize = 10240)
        {
            BUFFER_SIZE = buffeSize;
            BufferRead = new byte[buffeSize];
            RequestData = new StringBuilder(String.Empty);
            Request = null;
            ResponseStream = null;

            ListOfBuffers = new List<byte[]>();
        }
    }
}
