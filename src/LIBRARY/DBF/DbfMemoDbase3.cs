namespace DBF
{
    using System.IO;
    using System.Text;

    public class DbfMemoDbase3 : DbfMemo
    {
        public DbfMemoDbase3(string path)
            : base(path)
        {
        }

        public DbfMemoDbase3(string path, Encoding encoding)
            : base(path, encoding)
        {
        }

        public DbfMemoDbase3(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
        }

        public override string BuildMemo(long startBlock)
        {
            var offset = this.Offset(startBlock);
            this.BinaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var finished = false;
            var stringBuilder = new StringBuilder();

            do
            {
                var block = this.BinaryReader.ReadString(DefaultBlockSize, this.CurrentEncoding);
                stringBuilder.Append(block);

                if (block.Length >= DefaultBlockSize)
                {
                    finished = true;
                }
            }
            while (!finished);

            var value = stringBuilder.ToString();
            value = value.TrimEnd('\0', ' ');
            return value;
        }
    }
}
