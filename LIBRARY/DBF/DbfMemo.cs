namespace DBF
{
    using System.IO;
    using System.Text;

    public abstract class DbfMemo : Disposable
    {
        private const int FILEREADBUFFERSIZE = 4 * 1024;

        protected const int BlockHeaderSize = 8;
        protected const int DefaultBlockSize = 512;

        protected BinaryReader BinaryReader;

        protected DbfMemo(string path)
            : this(path, Encoding.GetEncoding(1252))
        {
        }

        protected DbfMemo(string path, Encoding encoding)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            this.Path = path;
            this.CurrentEncoding = encoding;

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BufferedStream bufferedStream = new BufferedStream(fileStream, FILEREADBUFFERSIZE);
            this.BinaryReader = new BinaryReader(bufferedStream, encoding);
        }

        protected DbfMemo(Stream stream, Encoding encoding)
        {
            this.Path = string.Empty;
            this.CurrentEncoding = encoding;

            this.BinaryReader = new BinaryReader(stream, encoding);
        }

        public Encoding CurrentEncoding { get; set; }

        public virtual int BlockSize => DefaultBlockSize;

        public string Path { get; set; }

        public void Close()
        {
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing)
                {
                    return;
                }

                this.BinaryReader?.Dispose();
            }
            finally
            {
                this.BinaryReader = null;
            }
        }

        public abstract string BuildMemo(long startBlock);

        public string Get(long startBlock)
        {
            return startBlock <= 0 ? string.Empty : this.BuildMemo(startBlock);
        }

        public long Offset(long startBlock)
        {
            return startBlock * this.BlockSize;
        }

        public int ContentSize(int memoSize)
        {
            return memoSize - this.BlockSize + BlockHeaderSize;
        }

        public int BlockContentSize()
        {
            return this.BlockSize + BlockHeaderSize;
        }
    }
}
