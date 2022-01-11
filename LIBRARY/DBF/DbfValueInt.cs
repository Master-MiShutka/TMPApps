namespace DBF
{
    using System;
    using System.Globalization;
    using System.IO;

    public class DbfValueInt : DbfValue<int?>, IDbfValue<int>
    {
        private static readonly NumberFormatInfo intNumberFormat = new NumberFormatInfo();

        public DbfValueInt(int length)
            : base(length)
        {
        }

        int IDbfValue<int>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueInt(this.Length);
        }

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

        private void Parse(string stringValue)
        {
            if (int.TryParse(stringValue,
                NumberStyles.Integer | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                intNumberFormat, out var value))
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
