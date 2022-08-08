namespace DBF
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    // Read an entire standard DBF file into a DataTable
    public class ParserDBF : IDisposable
    {
        private DBFHeader header;
        private List<DBFFieldDescriptor> fields = new List<DBFFieldDescriptor>();

        private List<Dictionary<DBFFieldDescriptor, object>> records = new List<Dictionary<DBFFieldDescriptor, object>>();

        private BinaryReader reader;
        private Encoding encoding;

        private Encoding DOSENC = Encoding.GetEncoding(866);
        private Encoding WINENC = Encoding.GetEncoding(1251);
        private Encoding SYSENC = Encoding.Default;

        // Read an entire standard DBF file into a DataTable
        /*public static DataTable ReadDBF(string dbfFile)
        {
            long start = DateTime.Now.Ticks;
            DataTable dt = new DataTable();
            BinaryReader recReader;
            string number;
            string year;
            string month;
            string day;
            long lDate;
            long lTime;
            DataRow row;
            int fieldIndex;



            // If there isn't even a file, just return an empty DataTable
            if ((false == File.Exists(dbfFile)))
            {
                throw new System.IO.FileNotFoundException(String.Format("Файл не найден!\n'{0}'", dbfFile));
            }
            try
            {
                //dbfFile = @"\\\\" + System.Environment.MachineName + @"\\" + dbfFile.Replace(@":", "$").Replace(@"\", @"\\");

                // Read the header into a buffer
                byte[] buffer = null;
                using (Stream stream = File.Open(dbfFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

                    // Marshall the header into a DBFHeader structure
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    DBFHeader header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
                    handle.Free();

                    // Read in all the field descriptors. Per the spec, 13 (0D) marks the end of the field descriptors
                    ArrayList fields = new ArrayList();
                    while ((13 != br.PeekChar()))
                    {
                        buffer = br.ReadBytes(Marshal.SizeOf(typeof(FieldDescriptor)));
                        handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        fields.Add((FieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FieldDescriptor)));
                        handle.Free();
                    }

                    // Read in the first row of records, we need this to help determine column types below
                    ((FileStream)br.BaseStream).Seek(header.headerLen + 1, SeekOrigin.Begin);
                    buffer = br.ReadBytes(header.recordLen);
                    recReader = new BinaryReader(new MemoryStream(buffer));

                    // Create the columns in our new DataTable
                    DataColumn col = null;
                    foreach (FieldDescriptor field in fields)
                    {
                        number = DOS_ENC.GetString(recReader.ReadBytes(field.fieldLen));

                        string fieldname = DOS_ENC.GetString(SYS_ENC.GetBytes(field.fieldName));

                        switch (field.fieldType)
                        {
                            case 'N':
                                if (number.IndexOf(".") > -1)
                                {
                                    col = new DataColumn(fieldname, typeof(decimal));
                                }
                                else
                                {
                                    col = new DataColumn(fieldname, typeof(int));
                                }
                                break;
                            case 'C':
                                col = new DataColumn(fieldname, typeof(string));
                                break;
                            case 'M':
                                col = new DataColumn(fieldname, typeof(string));
                                break;
                            case 'T':
                                // You can uncomment this to see the time component in the grid
                                //col = new DataColumn(field.fieldName, typeof(string));
                                col = new DataColumn(fieldname, typeof(DateTime));
                                break;
                            case 'D':
                                col = new DataColumn(fieldname, typeof(DateTime));
                                break;
                            case 'L':
                                col = new DataColumn(fieldname, typeof(bool));
                                break;
                            case 'F':
                                col = new DataColumn(fieldname, typeof(Double));
                                break;
                        }
                        dt.Columns.Add(col);
                    }

                    // Skip past the end of the header.
                    ((FileStream)br.BaseStream).Seek(header.headerLen, SeekOrigin.Begin);

                    // Read in all the records
                    for (int counter = 0; counter <= header.numRecords - 1; counter++)
                    {
                        // First we'll read the entire record into a buffer and then read each field from the buffer
                        // This helps account for any extra space at the end of each record and probably performs better
                        buffer = br.ReadBytes(header.recordLen);
                        recReader = new BinaryReader(new MemoryStream(buffer));

                        // All dbf field records begin with a deleted flag field. Deleted - 0x2A (asterisk) else 0x20 (space)
                        if (recReader.ReadChar() == '*')
                        {
                            continue;
                        }

                        // Loop through each field in a record
                        fieldIndex = 0;
                        row = dt.NewRow();
                        foreach (FieldDescriptor field in fields)
                        {
                            string fieldname = DOS_ENC.GetString(SYS_ENC.GetBytes(field.fieldName));

                            switch (field.fieldType)
                            {
                                case 'N':  // Number
                                           // If you port this to .NET 2.0, use the Decimal.TryParse method
                                    number = DOS_ENC.GetString(recReader.ReadBytes(field.fieldLen));
                                    if (IsNumber(number))
                                    {
                                        if (number.IndexOf(".") > -1)
                                        {
                                            row[fieldIndex] = decimal.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            row[fieldIndex] = int.Parse(number);
                                        }
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0;
                                    }

                                    break;

                                case 'C': // String
                                    row[fieldIndex] = DOS_ENC.GetString(recReader.ReadBytes(field.fieldLen));
                                    break;

                                case 'M': // Memo
                                    row[fieldIndex] = DOS_ENC.GetString(recReader.ReadBytes(field.fieldLen));
                                    break;

                                case 'D': // Date (YYYYMMDD)
                                    year = DOS_ENC.GetString(recReader.ReadBytes(4));
                                    month = DOS_ENC.GetString(recReader.ReadBytes(2));
                                    day = DOS_ENC.GetString(recReader.ReadBytes(2));
                                    row[fieldIndex] = System.DBNull.Value;
                                    try
                                    {
                                        if (IsNumber(year) && IsNumber(month) && IsNumber(day))
                                        {
                                            if ((Int32.Parse(year) > 1900))
                                            {
                                                row[fieldIndex] = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                                            }
                                        }
                                    }
                                    catch
                                    { }

                                    break;

                                case 'T': // Timestamp, 8 bytes - two integers, first for date, second for time
                                          // Date is the number of days since 01/01/4713 BC (Julian Days)
                                          // Time is hours * 3600000L + minutes * 60000L + Seconds * 1000L (Milliseconds since midnight)
                                    lDate = recReader.ReadInt32();
                                    lTime = recReader.ReadInt32() * 10000L;
                                    row[fieldIndex] = JulianToDateTime(lDate).AddTicks(lTime);
                                    break;

                                case 'L': // Boolean (Y/N)
                                    byte b = recReader.ReadByte();
                                    if ('Y' == b || 'T' == b)
                                    {
                                        row[fieldIndex] = true;
                                    }
                                    else
                                    {
                                        row[fieldIndex] = false;
                                    }

                                    break;

                                case 'F':
                                    number = DOS_ENC.GetString(recReader.ReadBytes(field.fieldLen));
                                    if (IsNumber(number))
                                    {
                                        row[fieldIndex] = double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0.0F;
                                    }
                                    break;
                            }
                            fieldIndex++;
                        }

                        recReader.Close();
                        dt.Rows.Add(row);
                    }
                }
            }

            catch (Exception e)
            {
                throw e;
            }

            long count = DateTime.Now.Ticks - start;

            return dt;
        }*/

        public ParserDBF(Stream stream, Encoding encoding)
        {
            this.encoding = encoding;
            this.reader = new BinaryReader(stream, encoding);

            this.ReadHeader();
        }

        public ParserDBF(string filename, Encoding encoding)
        {
            if (File.Exists(filename) == false)
            {
                throw new FileNotFoundException();
            }

            this.encoding = encoding;
            var bs = new BufferedStream(File.OpenRead(filename));
            this.reader = new BinaryReader(bs, encoding);

            this.ReadHeader();
        }

        public ParserDBF(string filename)
        {
            if (File.Exists(filename) == false)
            {
                throw new FileNotFoundException();
            }

            this.encoding = this.DOSENC;
            var bs = new BufferedStream(File.OpenRead(filename));
            this.reader = new BinaryReader(bs, this.encoding);

            this.ReadHeader();
        }

        private void ReadHeader()
        {
            byte[] buffer = this.reader.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

            // Marshall the header into a DBFHeader structure
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            this.header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
            handle.Free();

            this.fields = new List<DBFFieldDescriptor>();
            while (this.reader.PeekChar() != 13)
            {
                buffer = this.reader.ReadBytes(Marshal.SizeOf(typeof(DBFFieldDescriptor)));
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var fieldDescriptor = (DBFFieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFFieldDescriptor));
                if ((fieldDescriptor.Flags & DBFFieldFlags.System) != DBFFieldFlags.System)
                {
                    this.fields.Add(fieldDescriptor);
                }

                handle.Free();
            }

            byte headerTerminator = this.reader.ReadByte();
            byte[] backlink = this.reader.ReadBytes(263);
        }

        private void ReadRecords()
        {
            this.records = new List<Dictionary<DBFFieldDescriptor, object>>();

            // Skip back to the end of the header.
            this.reader.BaseStream.Seek(this.header.HeaderLenght, SeekOrigin.Begin);
            for (int i = 0; i < this.header.NumberOfRecords; i++)
            {
                if (this.reader.PeekChar() == '*') // DELETED
                {
                    continue;
                }

                var record = new Dictionary<DBFFieldDescriptor, object>();
                var row = this.reader.ReadBytes(this.header.RecordLenght);

                foreach (DBFFieldDescriptor field in this.fields)
                {
                    byte[] buffer = new byte[field.FieldLength];
                    Array.Copy(row, field.Address, buffer, 0, field.FieldLength);
                    string text = (this.encoding.GetString(buffer) ?? string.Empty).Trim();

                    switch ((DBFFieldType)field.FieldType)
                    {
                        case DBFFieldType.Character:
                            record[field] = text;
                            break;

                        case DBFFieldType.Currency:
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0.0m;
                                }
                            }
                            else
                            {
                                record[field] = Convert.ToDecimal(text);
                            }

                            break;

                        case DBFFieldType.Numeric:
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = default(decimal);
                                }
                            }
                            else
                            {
                                record[field] = Convert.ToDecimal(text);
                            }

                            break;

                        case DBFFieldType.Float:
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0.0f;
                                }
                            }
                            else
                            {
                                record[field] = Convert.ToSingle(text);
                            }

                            break;

                        case DBFFieldType.Date:
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = DateTime.MinValue;
                                }
                            }
                            else
                            {
                                record[field] = DateTime.ParseExact(text, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                            }

                            break;

                        case DBFFieldType.DateTime:
                            if (string.IsNullOrWhiteSpace(text) || BitConverter.ToInt64(buffer, 0) == 0)
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = DateTime.MinValue;
                                }
                            }
                            else
                            {
                                record[field] = JulianToDateTime(BitConverter.ToInt64(buffer, 0));
                            }

                            break;

                        case DBFFieldType.Double:
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0.0d;
                                }
                            }
                            else
                            {
                                record[field] = Convert.ToDouble(text);
                            }

                            break;

                        case DBFFieldType.Integer:
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0;
                                }
                            }
                            else
                            {
                                record[field] = BitConverter.ToInt32(buffer, 0);
                            }

                            break;

                        case DBFFieldType.Logical:
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = false;
                                }
                            }
                            else
                            {
                                record[field] = buffer[0] == 'Y' || buffer[0] == 'T';
                            }

                            break;

                        case DBFFieldType.Memo:
                        case DBFFieldType.General:
                        case DBFFieldType.Picture:
                        default:
                            record[field] = buffer;
                            break;
                    }
                }

                this.records.Add(record);
            }
        }

        public DataTable ReadToDataTable()
        {
            this.ReadRecords();

            var table = new DataTable();

            // Columns
            foreach (var field in this.fields)
            {
                var colType = ToDbType(field.FieldType);
                var column = new DataColumn(field.FieldName, colType ?? typeof(string));
                table.Columns.Add(column);
            }

            // Rows
            foreach (var record in this.records)
            {
                var row = table.NewRow();
                foreach (var column in record.Keys)
                {
                    row[column.FieldName] = record[column] ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public IEnumerable<Dictionary<string, object>> ReadToDictionary()
        {
            this.ReadRecords();
            return this.records.Select(record => record.ToDictionary(r => r.Key.FieldName, r => r.Value)).ToList();
        }

        public IEnumerable<T> ReadToObject<T>()
            where T : new()
        {
            this.ReadRecords();

            var type = typeof(T);
            var list = new List<T>();

            foreach (var record in this.records)
            {
                T item = new T();
                foreach (var pair in record.Select(s => new { Key = s.Key.FieldName, Value = s.Value }))
                {
                    var property = type.GetProperty(pair.Key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (property != null)
                    {
                        if (property.PropertyType == pair.Value.GetType())
                        {
                            property.SetValue(item, pair.Value, null);
                        }
                        else
                        {
                            if (pair.Value != DBNull.Value)
                            {
                                property.SetValue(item, System.Convert.ChangeType(pair.Value, property.PropertyType), null);
                            }
                        }
                    }
                }

                list.Add(item);
            }

            return list;
        }

        #region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing == false)
            {
                return;
            }

            if (this.reader != null)
            {
                this.reader.Close();
                this.reader.Dispose();
                this.reader = null;
            }
        }

        ~ParserDBF()
        {
            this.Dispose(false);
        }

        #endregion

        /// <summary>
        /// Simple function to test is a string can be parsed. There may be a better way, but this works
        /// If you port this to .NET 2.0, use the new TryParse methods instead of this
        ///   *Thanks to wu.qingman on code project for fixing a bug in this for me
        /// </summary>
        /// <param name="number">string to test for parsing</param>
        /// <returns>true if string can be parsed</returns>
        public static bool IsNumber(string numberString)
        {
            char[] numbers = numberString.ToCharArray();
            int number_count = 0;
            int point_count = 0;
            int space_count = 0;

            foreach (char number in numbers)
            {
                if (number >= 48 && number <= 57)
                {
                    number_count += 1;
                }
                else if (number == 46)
                {
                    point_count += 1;
                }
                else if (number == 32)
                {
                    space_count += 1;
                }
                else
                {
                    return false;
                }
            }

            return number_count > 0 && point_count < 2;
        }

        /// <summary>
        /// Convert a Julian Date to a .NET DateTime structure
        /// Implemented from pseudo code at http://en.wikipedia.org/wiki/Julian_day
        /// </summary>
        /// <param name="lJDN">Julian Date to convert (days since 01/01/4713 BC)</param>
        /// <returns>DateTime</returns>
        private static DateTime JulianToDateTime(long lJDN)
        {
            double p = Convert.ToDouble(lJDN);
            double s1 = p + 68569;
            double n = Math.Floor(4 * s1 / 146097);
            double s2 = s1 - Math.Floor(((146097 * n) + 3) / 4);
            double i = Math.Floor(4000 * (s2 + 1) / 1461001);
            double s3 = s2 - Math.Floor(1461 * i / 4) + 31;
            double q = Math.Floor(80 * s3 / 2447);
            double d = s3 - Math.Floor(2447 * q / 80);
            double s4 = Math.Floor(q / 11);
            double m = q + 2 - (12 * s4);
            double j = (100 * (n - 49)) + i + s4;
            return new DateTime(Convert.ToInt32(j), Convert.ToInt32(m), Convert.ToInt32(d));
        }

        /// <summary>
        /// This is the file header for a DBF. We do this special layout with everything
        /// packed so we can read straight from disk into the structure to populate it
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DBFHeader
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
            public readonly short RecordLenght;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] Reserved;

            /// <summary>Table Flags</summary>
            public readonly DBFTableFlags TableFlags;

            /// <summary>Code Page Mark</summary>
            public readonly byte CodePage;

            /// <summary>Reserved, contains 0x00</summary>
            public readonly short EndOfHeader;
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

        /// <summary>
        /// This is the field descriptor structure. There will be one of these for each column in the table.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DBFFieldDescriptor
        {
            /// <summary>The field name.</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
            public readonly string FieldName;

            /// <summary>The field type.</summary>
            public readonly char FieldType;

            /// <summary>The field address.</summary>
            public readonly int Address;

            /// <summary>The field length in bytes.</summary>
            public readonly byte FieldLength;

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

            public override string ToString()
            {
                return string.Format("{0} {1}", this.FieldName, this.FieldType);
            }
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

        public enum DBFFieldType : int
        {
            Character = 'C',
            Currency = 'Y',
            Numeric = 'N',
            Float = 'F',
            Date = 'D',
            DateTime = 'T',
            Double = 'B',
            Integer = 'I',
            Logical = 'L',
            Memo = 'M',
            General = 'G',
            Picture = 'P',
        }

        public static Type ToDbType(char type)
        {
            switch ((DBFFieldType)type)
            {
                case DBFFieldType.Float:
                    return typeof(float);

                case DBFFieldType.Integer:
                    return typeof(int);

                case DBFFieldType.Currency:
                    return typeof(decimal);

                case DBFFieldType.Character:
                case DBFFieldType.Memo:
                    return typeof(string);

                case DBFFieldType.Date:
                case DBFFieldType.DateTime:
                    return typeof(DateTime);

                case DBFFieldType.Logical:
                    return typeof(bool);

                case DBFFieldType.General:
                case DBFFieldType.Picture:
                    return typeof(byte[]);

                default:
                    return null;
            }
        }
    }
}