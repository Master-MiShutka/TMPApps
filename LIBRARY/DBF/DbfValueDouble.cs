namespace DBF
{
    using System;
    using System.Globalization;
    using System.IO;

    public class DbfValueDouble : DbfValue<double?>, IDbfValue<double>
    {
        private static readonly NumberFormatInfo doubleNumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        [Obsolete("This constructor should no longer be used. Use DbfValueDouble(System.Int32, System.Int32) instead.")]
        public DbfValueDouble(int length)
            : this(length, 0)
        {
        }

        public DbfValueDouble(int length, int decimalCount)
            : base(length)
        {
            this.DecimalCount = decimalCount;
        }

        double IDbfValue<double>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueDouble(this.Length, this.DecimalCount);
        }

        public int DecimalCount { get; }

        public override string ToString()
        {
            var format = this.DecimalCount != 0
                ? $"0.{new string('0', this.DecimalCount)}"
                : null;

            return this.Value?.ToString(format, NumberFormatInfo.CurrentInfo) ?? string.Empty;
        }

        public override void Parse(byte[] bytes)
        {
            this.Value = BitConverter.ToDouble(bytes, 0);
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            this.Value = BitConverter.ToDouble(span);
        }
    }
}
