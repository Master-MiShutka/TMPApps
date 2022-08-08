namespace DBF
{
    using System;
    using System.IO;
    using System.Text;

    public class DbfValueString : DbfValue<string>
    {
        protected readonly Encoding CurrentEncoding;

        public DbfValueString(int length, Encoding encoding)
            : base(length)
        {
            this.CurrentEncoding = encoding;
        }

        public override object Clone()
        {
            return new DbfValueString(this.Length, this.CurrentEncoding);
        }

        public override void Parse(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            throw new NotImplementedException();
        }

        public override void Read(BinaryReader binaryReader)
        {
            this.Value = binaryReader.ReadString(this.Length, this.CurrentEncoding);
        }

        public override void Read(byte[] buffer, int index)
        {
            this.Value = this.CurrentEncoding.GetString(buffer, index, this.Length);
        }

        public override void Read(ReadOnlySpan<byte> span)
        {
            this.Value = this.CurrentEncoding.GetString(span);
        }
    }
}
