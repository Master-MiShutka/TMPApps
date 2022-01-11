namespace DBF
{
    using System;
    using System.IO;

    public class DbfValueLong : DbfValue<long?>, IDbfValue<long>
    {
        public DbfValueLong(int length)
            : base(length)
        {
            if (length != 8)
                System.Diagnostics.Debugger.Break();
        }

        long IDbfValue<long>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueLong(this.Length);
        }

        public override void Parse(byte[] bytes)
        {
            this.Value = BitConverter.ToInt64(bytes, 0);
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            this.Value = BitConverter.ToInt64(span);
        }

        public override void Read(BinaryReader binaryReader)
        {
            this.Value = binaryReader.ReadInt64();
        }

        public override void Read(byte[] buffer, int index)
        {
            var bytes = new byte[this.Length];

            Array.Copy(buffer, index, bytes, 0, this.Length);
            this.Parse(bytes);
        }
    }
}
