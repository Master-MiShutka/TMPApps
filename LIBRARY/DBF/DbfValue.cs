namespace DBF
{
    using System;
    using System.IO;

    public abstract class DbfValue<T> : IDbfValue<T>
    {
        protected DbfValue(int length)
        {
            this.Length = length;
        }

        public int Length { get; }

        public T Value { get; protected set; }

        Type IDbfValue.BaseType => typeof(T);

        public virtual void Read(BinaryReader binaryReader)
        {
            var bytes = binaryReader.ReadBytes(this.Length);
            this.Parse(bytes);
        }

        public virtual void Read(byte[] buffer, int index)
        {
            var bytes = new byte[this.Length];
            Array.Copy(buffer, index, bytes, 0, this.Length);
            this.Parse(bytes);
        }

        public virtual void Read(ReadOnlySpan<byte> span)
        {
            // var bytes = new byte[this.Length];
            // Array.Copy(segment.Array, index, bytes, 0, this.Length);

            this.Parse(span);
        }

        public object GetValue()
        {
            return this.Value == null ? default : this.Value;
        }

        public override string ToString()
        {
            return this.Value?.ToString();
        }

        public abstract object Clone();

        public abstract void Parse(byte[] bytes);

        public abstract void Parse(ReadOnlySpan<byte> span);
    }
}
