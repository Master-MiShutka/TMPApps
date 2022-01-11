namespace DBF
{
    using System;
    using System.Globalization;
    using System.IO;

    public class DbfValueFloat : DbfValue<float?>, IDbfValue<float>
    {
        private static readonly NumberFormatInfo floatNumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        [Obsolete("This constructor should no longer be used. Use DbfValueFloat(System.Int32, System.Int32) instead.")]
        public DbfValueFloat(int length)
            : this(length, 0)
        {
        }

        public DbfValueFloat(int length, int decimalCount)
            : base(length)
        {
            this.DecimalCount = decimalCount;
        }

        float IDbfValue<float>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueFloat(this.Length, this.DecimalCount);
        }

        public int DecimalCount { get; }

        public override void Read(BinaryReader binaryReader)
        {
            if (binaryReader.PeekChar() == '\0')
            {
                binaryReader.ReadBytes(this.Length);
                this.Value = null;
            }
            else
            {
                this.Parse(binaryReader.ReadBytes(this.Length));
            }
        }

        public override string ToString()
        {
            var format = this.DecimalCount != 0
                ? $"0.{new string('0', this.DecimalCount)}"
                : null;

            return this.Value?.ToString(format, NumberFormatInfo.CurrentInfo) ?? string.Empty;
        }

        private void Parse(string stringValue)
        {
            if (float.TryParse(stringValue,
                NumberStyles.Float | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                floatNumberFormat, out var value))
            {
                this.Value = value;
            }
            else
            {
                this.Value = null;
            }
        }

        public override void Parse(byte[] bytes)
        {
            string stringValue = System.Text.ASCIIEncoding.ASCII.GetString(bytes);

            if (bytes[0] == '\0')
            {
                this.Value = null;
                return;
            }

            this.Parse(stringValue);
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            if (span[0] == '\0')
            {
                this.Value = null;
                return;
            }

            string value = System.Text.ASCIIEncoding.ASCII.GetString(span);
            this.Parse(value);
        }
    }
}
