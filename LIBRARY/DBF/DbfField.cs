namespace DBF
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.None, Pack = 1)]
    [DebuggerDisplay("DbfField: name='{NameInCP866Encoding}'")]
    public struct DbfField
    {
        /// <summary>The field name.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] NameBytes;

        /// <summary>The field type.</summary>
        public readonly DbfFieldType Type;

        /// <summary>The field address.</summary>
        public readonly int Address;

        /// <summary>The field length in bytes.</summary>
        public readonly byte Length;

        /// <summary>The field precision.</summary>
        public readonly byte DecimalCount;

        /// <summary>Field Flags</summary>
        public readonly DBFFieldFlags Flags;

        /// <summary>AutoIncrement next value</summary>
        public readonly int AutoIncrementNextValue;

        /// <summary>AutoIncrement step value</summary>
        public readonly byte AutoIncrementStepValue;

        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] Reserved;

        public string NameInCP866Encoding => System.Text.Encoding.GetEncoding(866).GetString(this.NameBytes).Trim((char)0);

        public string GetName(Encoding encoding)
        {
            return encoding.GetString(this.NameBytes).Trim((char)0);
        }
    }

    public enum DbfFieldType : byte
    {
        Character = (byte)'C',
        Currency = (byte)'Y',
        Numeric = (byte)'N',
        Float = (byte)'F',
        Date = (byte)'D',
        DateTime = (byte)'T',
        Double = (byte)'B',
        Integer = (byte)'I',
        Logical = (byte)'L',
        Memo = (byte)'M',
        General = (byte)'G',
        Picture = (byte)'P',
    }

    [Flags]
    public enum DBFFieldFlags : byte
    {
        None = 0x00,
        System = 0x01,
        AllowNullValues = 0x02,
        Binary = 0x04,
        AutoIncrementing = 0x0C,
    }
}
