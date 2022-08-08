namespace DBF
{
    using System;
    using System.Globalization;
    using System.IO;

    public class DbfValueDate : DbfValue<DateTime?>, IDbfValue<DateTime>
    {
        public DbfValueDate(int length)
            : base(length)
        {
        }

        DateTime IDbfValue<DateTime>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueDate(this.Length);
        }

        public override string ToString()
        {
            return this.Value?.ToString("d") ?? string.Empty;
        }

        public void Parse(string value)
        {
            value = value.TrimEnd((char)0);

            if (string.IsNullOrWhiteSpace(value))
            {
                this.Value = default;
            }
            else
            {
                this.Value = DateTime.ParseExact(value, "yyyyMMdd", null,
                    DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
            }
        }

        public override void Parse(byte[] bytes)
        {
            string value = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
            this.Parse(value);
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            string value = System.Text.ASCIIEncoding.ASCII.GetString(span);
            this.Parse(value);
        }
    }
}
