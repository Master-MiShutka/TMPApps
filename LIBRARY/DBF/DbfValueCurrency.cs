namespace DBF
{
    using System;
    using System.Globalization;
    using System.IO;

    public class DbfValueCurrency : DbfValue<float?>, IDbfValue<float>
    {
        public DbfValueCurrency(int length, int decimalCount)
            : base(length)
        {
            this.DecimalCount = decimalCount;
        }

        float IDbfValue<float>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueCurrency(this.Length, this.DecimalCount);
        }

        public int DecimalCount { get; set; }

        public override string ToString()
        {
            var format = this.DecimalCount != 0
                ? $"0.{new string('0', this.DecimalCount)}"
                : null;

            return this.Value?.ToString(format, NumberFormatInfo.CurrentInfo) ?? string.Empty;
        }

        public override void Parse(byte[] bytes)
        {
            var value = BitConverter.ToUInt64(bytes, 0);
            this.Value = value / 10000.0f;
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            var value = BitConverter.ToUInt64(span);
            this.Value = value / 10000.0f;
        }
    }
}
