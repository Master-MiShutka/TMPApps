namespace DBF
{
    using System;
    using System.IO;

    public class DbfValueDateTime : DbfValue<DateTime?>, IDbfValue<DateTime>
    {
        public DbfValueDateTime(int length)
            : base(length)
        {
        }

        DateTime IDbfValue<DateTime>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueDateTime(this.Length);
        }

        public override void Parse(byte[] bytes)
        {
            if (bytes[0] == '\0')
            {
                this.Value = default;
            }
            else
            {
                var datePart = BitConverter.ToInt32(bytes, 0);
                var timePart = BitConverter.ToInt32(bytes, 4);
                this.Value = new DateTime(1, 1, 1).AddDays(datePart).Subtract(TimeSpan.FromDays(1721426))
                    .AddMilliseconds(timePart);
            }
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            if (span[0] == '\0')
            {
                this.Value = default;
            }
            else
            {
                int datePart = BitConverter.ToInt32(span);
                int timePart = BitConverter.ToInt32(span.Slice(4));
                this.Value = new DateTime(1, 1, 1).AddDays(datePart).Subtract(TimeSpan.FromDays(1721426))
                    .AddMilliseconds(timePart);
            }
        }
    }
}
