namespace DBF
{
    using System;
    using System.IO;

    public class DbfValueBoolean : DbfValue<bool?>, IDbfValue<bool>
    {
        public DbfValueBoolean(int length)
            : base(length)
        {
        }

        bool IDbfValue<bool>.Value => this.Value.GetValueOrDefault();

        public override object Clone()
        {
            return new DbfValueBoolean(this.Length);
        }

        public override void Parse(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public override void Parse(ReadOnlySpan<byte> span)
        {
            throw new System.NotImplementedException();
        }

        public override void Read(BinaryReader binaryReader)
        {
            var value = binaryReader.ReadChar();
            this.Parse(value);
        }

        public override void Read(byte[] buffer, int index)
        {
            var value = (char)buffer[index];
            this.Parse(value);
        }

        public override void Read(ReadOnlySpan<byte> span)
        {
            var value = (char)span[0];
            this.Parse(value);
        }

        public override string ToString()
        {
            return !this.Value.HasValue ? string.Empty : this.Value.Value ? "T" : "F";
        }

        private void Parse(char value)
        {
            if (value == 'Y' || value == 'y' || value == 'T' || value == 't')
            {
                this.Value = true;
            }
            else if (value == 'N' || value == 'n' || value == 'F' || value == 'f')
            {
                this.Value = false;
            }
            else
            {
                this.Value = null;
            }
        }
    }
}
