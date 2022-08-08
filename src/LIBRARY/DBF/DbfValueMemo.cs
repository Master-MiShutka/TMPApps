namespace DBF
{
    using System;
    using System.IO;
    using System.Text;

    public class DbfValueMemo : DbfValueString
    {
        private readonly DbfMemo memo;

        public DbfValueMemo(int length, DbfMemo memo, Encoding encoding)
            : base(length, encoding)
        {
            this.memo = memo;
        }

        public override object Clone()
        {
            return new DbfValueMemo(this.Length, this.memo, this.CurrentEncoding);
        }

        public override void Read(BinaryReader binaryReader)
        {
            if (this.Length == 4)
            {
                var startBlock = binaryReader.ReadUInt32();
                this.Value = this.memo?.Get(startBlock);
            }
            else
            {
                if (this.Length == 10)
                {
                    string s = binaryReader.ReadString(this.Length, this.CurrentEncoding);
                    long.TryParse(s, out long pos);
                    this.Value = this.memo?.Get(pos);
                }
                else
                {
                    this.Parse(binaryReader.ReadString(this.Length, this.CurrentEncoding));
                }
            }
        }

        public override void Read(byte[] buffer, int index)
        {
            if (this.Length == 4)
            {
                var startBlock = BitConverter.ToInt32(buffer, index);
                this.Value = this.memo?.Get(startBlock);
            }
            else
            {
                if (this.Length == 10)
                {
                    string s = this.CurrentEncoding.GetString(buffer, index, this.Length);
                    long.TryParse(s, out long pos);
                    this.Value = this.memo?.Get(pos);
                }
                else
                {
                    this.Value = this.CurrentEncoding.GetString(buffer, index, this.Length);
                }
            }
        }

        public override void Read(ReadOnlySpan<byte> span)
        {
            if (this.Length == 4)
            {
                var startBlock = BitConverter.ToInt32(span);
                this.Value = this.memo?.Get(startBlock);
            }
            else
            {
                if (this.Length == 10)
                {
                    string s = this.CurrentEncoding.GetString(span);
                    long.TryParse(s, out long startBlock);

                    this.Value = this.memo?.Get(startBlock);
                }
                else
                {
                    this.Value = this.CurrentEncoding.GetString(span);
                }
            }
        }

        private void Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.Value = string.Empty;
            }
            else
            {
                var startBlock = long.Parse(value);
                this.Value = this.memo?.Get(startBlock);
            }
        }
    }
}
