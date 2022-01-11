namespace DBF
{
    using System;
    using System.IO;

    public interface IDbfValue : System.ICloneable
    {
        System.Type BaseType { get; }

        void Read(BinaryReader binaryReader);

        void Read(byte[] buffer, int index);

        void Read(ReadOnlySpan<byte> span);

        object GetValue();

        int Length { get; }
    }

    public interface IDbfValue<T> : IDbfValue
    {
        T Value { get; }
    }
}
