namespace DBF
{
    using System;
    using System.IO;

    public class DbfValueNull : IDbfValue
    {
        public DbfValueNull(int length)
        {
            this.Length = length;
        }

        public object Clone()
        {
            return new DbfValueNull(this.Length);
        }

        public int Length { get; }

        public Type BaseType => typeof(Nullable);

        public object GetValue()
        {
            return null;
        }

        public void Read(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(this.Length);
        }

        public void Read(byte[] buffer, int index)
        {
        }

        public void Read(ReadOnlySpan<byte> span)
        {
        }

        public T GetValue<T>()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
