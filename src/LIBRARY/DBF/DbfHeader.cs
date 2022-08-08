namespace DBF
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This class represents a DBF IV file header.
    /// </summary>
    ///
    /// <remarks>
    /// DBF files are really wasteful on space but this legacy format lives on because it's really really simple.
    /// It lacks much in features though.
    ///
    ///
    /// Thanks to Erik Bachmann for providing the DBF file structure information!!
    /// http://www.clicketyclick.dk/databases/xbase/format/dbf.html
    ///
    ///           _______________________  _______
    /// 00h /   0| Version number      *1|  ^
    ///          |-----------------------|  |
    /// 01h /   1| Date of last update   |  |
    /// 02h /   2|      YYMMDD        *21|  |
    /// 03h /   3|                    *14|  |
    ///          |-----------------------|  |
    /// 04h /   4| Number of records     | Record
    /// 05h /   5| in data file          | header
    /// 06h /   6| ( 32 bits )        *14|  |
    /// 07h /   7|                       |  |
    ///          |-----------------------|  |
    /// 08h /   8| Length of header   *14|  |
    /// 09h /   9| structure ( 16 bits ) |  |
    ///          |-----------------------|  |
    /// 0Ah /  10| Length of each record |  |
    /// 0Bh /  11| ( 16 bits )     *2 *14|  |
    ///          |-----------------------|  |
    /// 0Ch /  12| ( Reserved )        *3|  |
    /// 0Dh /  13|                       |  |
    ///          |-----------------------|  |
    /// 0Eh /  14| Incomplete transac.*12|  |
    ///          |-----------------------|  |
    /// 0Fh /  15| Encryption flag    *13|  |
    ///          |-----------------------|  |
    /// 10h /  16| Free record thread    |  |
    /// 11h /  17| (reserved for LAN     |  |
    /// 12h /  18|  only )               |  |
    /// 13h /  19|                       |  |
    ///          |-----------------------|  |
    /// 14h /  20| ( Reserved for        |  |            _        |=======================| ______
    ///          |   multi-user dBASE )  |  |           / 00h /  0| Field name in ASCII   |  ^
    ///          : ( dBASE III+ - )      :  |          /          : (terminated by 00h)   :  |
    ///          :                       :  |         |           |                       |  |
    /// 1Bh /  27|                       |  |         |   0Ah / 10|                       |  |
    ///          |-----------------------|  |         |           |-----------------------| For
    /// 1Ch /  28| MDX flag (dBASE IV)*14|  |         |   0Bh / 11| Field type (ASCII) *20| each
    ///          |-----------------------|  |         |           |-----------------------| field
    /// 1Dh /  29| Language driver     *5|  |        /    0Ch / 12| Field data address    |  |
    ///          |-----------------------|  |       /             |                     *6|  |
    /// 1Eh /  30| ( Reserved )          |  |      /              | (in memory !!!)       |  |
    /// 1Fh /  31|                     *3|  |     /       0Fh / 15| (dBASE III+)          |  |
    ///          |=======================|__|____/                |-----------------------|  |  -
    /// 20h /  32|                       |  |  ^          10h / 16| Field length       *22|  |   |
    ///          |- - - - - - - - - - - -|  |  |                  |-----------------------|  |   | *7
    ///          |                    *19|  |  |          11h / 17| Decimal count      *23|  |   |
    ///          |- - - - - - - - - - - -|  |  Field              |-----------------------|  |  -
    ///          |                       |  | Descriptor  12h / 18| ( Reserved for        |  |
    ///          :. . . . . . . . . . . .:  |  |array     13h / 19|   multi-user dBASE)*18|  |
    ///          :                       :  |  |                  |-----------------------|  |
    ///       n  |                       |__|__v_         14h / 20| Work area ID       *16|  |
    ///          |-----------------------|  |    \                |-----------------------|  |
    ///       n+1| Terminator (0Dh)      |  |     \       15h / 21| ( Reserved for        |  |
    ///          |=======================|  |      \      16h / 22|   multi-user dBASE )  |  |
    ///       m  | Database Container    |  |       \             |-----------------------|  |
    ///          :                    *15:  |        \    17h / 23| Flag for SET FIELDS   |  |
    ///          :                       :  |         |           |-----------------------|  |
    ///     / m+263                      |  |         |   18h / 24| ( Reserved )          |  |
    ///          |=======================|__v_ ___    |           :                       :  |
    ///          :                       :    ^       |           :                       :  |
    ///          :                       :    |       |           :                       :  |
    ///          :                       :    |       |   1Eh / 30|                       |  |
    ///          | Record structure      |    |       |           |-----------------------|  |
    ///          |                       |    |        \  1Fh / 31| Index field flag    *8|  |
    ///          |                       |    |         \_        |=======================| _v_____
    ///          |                       | Records
    ///          |-----------------------|    |
    ///          |                       |    |          _        |=======================| _______
    ///          |                       |    |         / 00h /  0| Record deleted flag *9|  ^
    ///          |                       |    |        /          |-----------------------|  |
    ///          |                       |    |       /           | Data               *10|  One
    ///          |                       |    |      /            : (ASCII)            *17: record
    ///          |                       |____|_____/             |                       |  |
    ///          :                       :    |                   |                       | _v_____
    ///          :                       :____|_____              |=======================|
    ///          :                       :    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |=======================|    |
    ///          |__End_of_File__________| ___v____  End of file ( 1Ah )  *11
    ///
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DBFHeader
    {
        /// <summary>The version.</summary>
        public readonly DBFVersion Version;

        /// <summary>The update year.</summary>
        public readonly byte UpdateYear;

        /// <summary>The update month.</summary>
        public readonly byte UpdateMonth;

        /// <summary>The update day.</summary>
        public readonly byte UpdateDay;

        /// <summary>The number of records.</summary>
        public readonly int NumberOfRecords;

        /// <summary>The length of the header.</summary>
        public readonly short HeaderLenght;

        /// <summary>The length of the bytes records.</summary>
        public readonly short RecordLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] Reserved;

        /// <summary>Table Flags</summary>
        public readonly DBFTableFlags TableFlags;

        /// <summary>Code Page Mark</summary>
        public readonly byte CodePage;

        /// <summary>Reserved, contains 0x00</summary>
        public readonly short EndOfHeader;

        public bool IsFoxPro => this.Version == DBFVersion.VisualFoxPro || this.Version == DBFVersion.VisualFoxProWithAutoIncrement || this.Version == DBFVersion.FoxPro2WithMemo || this.Version == DBFVersion.FoxBASE;
    }

    public enum DBFVersion : byte
    {
        Unknown = 0,
        FoxBase = 0x02,
        FoxBaseDBase3NoMemo = 0x03,
        VisualFoxPro = 0x30,
        VisualFoxProWithAutoIncrement = 0x31,
        dBase4SQLTableNoMemo = 0x43,
        dBase4SQLSystemNoMemo = 0x63,
        FoxBaseDBase3WithMemo = 0x83,
        dBase4WithMemo = 0x8B,
        dBase4SQLTableWithMemo = 0xCB,
        FoxPro2WithMemo = 0xF5,
        FoxBASE = 0xFB,
    }

    [Flags]
    public enum DBFTableFlags : byte
    {
        None = 0x00,
        HasStructuralCDX = 0x01,
        HasMemoField = 0x02,
        IsDBC = 0x04,
    }
}
