namespace DBF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    public class DbfTable : Disposable
    {
        #region Constants

        private const int FILEREADBUFFERSIZE = 4 * 1024;
        private const byte TERMINATOR = 0x0d;

        /// <summary>
        /// Size of DBFHeader
        /// </summary>
        private const int HEADERMETADATASIZE = 32;

        /// <summary>
        /// Size of DBFField
        /// </summary>
        private const int FIELDMETADATASIZE = 32;

        #endregion

        #region Fields

        private readonly BufferedStream bufferedStream;
        private readonly FileStream fileStream;

        private readonly Encoding DOSENC = Encoding.GetEncoding(866);

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public DbfTable(string filename, Encoding encoding, bool skipDeletedRecords)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }

            this.FileName = filename;
            this.CurrentEncoding = encoding;
            this.SkipDeletedRecords = skipDeletedRecords;

            try
            {
                this.fileStream = new FileStream(filename,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    128 * 1_024,
                    FileOptions.None);
                this.bufferedStream = new BufferedStream(this.fileStream, FILEREADBUFFERSIZE);
                this.BinaryReader = new BinaryReader(this.bufferedStream, encoding);

                this.ReadHeader();

                var memoPath = this.GetMemoPath();
                if (!string.IsNullOrEmpty(memoPath))
                {
                    this.Memo = this.CreateMemo(memoPath);
                }

                this.ReadFields();
                this.SeekToRecords();
            }
            catch (IOException ioe)
            {
                this.logger?.Error(ioe);
            }
            catch (Exception e)
            {
                this.logger?.Error(e);
            }
        }

        public DbfTable(Stream stream, Encoding encoding, bool skipDeletedRecords)
            : this(stream, null, encoding, skipDeletedRecords)
        {
        }

        public DbfTable(Stream stream, Stream memoStream, Encoding encoding, bool skipDeletedRecords)
        {
            this.FileName = string.Empty;
            this.CurrentEncoding = encoding;
            this.SkipDeletedRecords = skipDeletedRecords;

            this.BinaryReader = new BinaryReader(stream, encoding);

            this.ReadHeader();

            if (memoStream != null)
            {
                this.Memo = this.CreateMemo(memoStream);
            }

            this.ReadFields();
            this.SeekToRecords();
        }

        #endregion

        #region Internal properties

        internal BinaryReader BinaryReader { get; init; }

        #endregion

        #region IDisposable implementation

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing)
                {
                    return;
                }

                this.BinaryReader?.Dispose();
                this.bufferedStream?.Dispose();
                this.fileStream?.Dispose();
                this.Memo?.Dispose();
            }
            finally
            {
                this.Memo = null;
            }
        }

        #endregion

        #region Public properties

        public string FileName { get; }

        public Encoding CurrentEncoding { get; set; }

        public DBFHeader Header { get; private set; }

        public DbfMemo Memo { get; private set; }

        public Dictionary<string, DbfField> FieldsByName { get; private set; }

        public List<DbfField> FieldsByIndex { get; private set; }

        public int FieldsCount => (this.FieldsByIndex == null) ? 0 : this.FieldsByIndex.Count;

        public bool SkipDeletedRecords { get; set; } = true;

        public bool IsClosed => this.BinaryReader == null;

        #endregion

        #region Public methods

        /// <summary>
        /// Закрытие файла и освобождение ресурсов
        /// </summary>
        public void Close()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Переход к записям
        /// </summary>
        public void SeekToRecords()
        {
            // Skip past the end of the header.
            ((Stream)this.BinaryReader.BaseStream).Seek(this.Header.HeaderLenght, SeekOrigin.Begin);
        }

        /// <summary>
        /// Чтение записи
        /// </summary>
        /// <returns>запись</returns>
        public DbfRecord ReadRecord()
        {
            var dbfRecord = new DbfRecord(this);
            return !dbfRecord.Read(this.BinaryReader)
                ? null
                : (this.SkipDeletedRecords && dbfRecord.IsDeleted ? null : dbfRecord);
        }

        #endregion

        #region internal

        internal int GetFieldNameIndex(string fieldName)
        {
            var field = this.FieldsByIndex.Where(f => string.Equals(f.GetName(this.CurrentEncoding), fieldName)).FirstOrDefault();
            return this.FieldsByIndex.IndexOf(field);
        }

        internal IDbfValue CreateDbfValueByIndex(byte fieldIndex)
        {
            var field = this.FieldsByIndex[fieldIndex];
            var dbfValue = this.CreateDbfValue(field);

            return (IDbfValue)dbfValue.Clone();
        }

        internal IDbfValue CreateDbfValueByName(string fieldName)
        {
            var field = this.FieldsByName[fieldName];
            var dbfValue = this.CreateDbfValue(field);

            return (IDbfValue)dbfValue.Clone();
        }

        internal IDbfValue CreateDbfValue(DbfField field)
        {
            IDbfValue value;

            switch (field.Type)
            {
                case DbfFieldType.Numeric:

                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        if (field.DecimalCount == 0)
                        {
                            value = new DbfValueInt(field.Length);
                        }
                        else
                        {
                            value = new DbfValueDecimal(field.Length, field.DecimalCount);
                        }
                    }

                    break;
                case DbfFieldType.Integer:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueLong(field.Length);
                    }

                    break;
                case DbfFieldType.Float:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueFloat(field.Length, field.DecimalCount);
                    }

                    break;
                case DbfFieldType.Currency:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueCurrency(field.Length, field.DecimalCount);
                    }

                    break;
                case DbfFieldType.Date:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueDate(field.Length);
                    }

                    break;
                case DbfFieldType.DateTime:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueDateTime(field.Length);
                    }

                    break;
                case DbfFieldType.Logical:
                    value = new DbfValueBoolean(field.Length);
                    break;
                case DbfFieldType.Memo:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueMemo(field.Length, this.Memo, this.CurrentEncoding);
                    }

                    break;
                case DbfFieldType.Double:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueDouble(field.Length, field.DecimalCount);
                    }

                    break;
                case DbfFieldType.General:
                case DbfFieldType.Character:
                    if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                    {
                        value = new DbfValueNull(field.Length);
                    }
                    else
                    {
                        value = new DbfValueString(field.Length, this.CurrentEncoding);
                    }

                    break;
                default:
                    value = new DbfValueNull(field.Length);
                    break;
            }

            return value;
        }

        #endregion

        #region Private methods

        private string GetMemoPath()
        {
            var paths = new[]
            {
                System.IO.Path.ChangeExtension(this.FileName, "fpt"),
                System.IO.Path.ChangeExtension(this.FileName, "FPT"),
                System.IO.Path.ChangeExtension(this.FileName, "dbt"),
                System.IO.Path.ChangeExtension(this.FileName, "DBT"),
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private DbfMemo CreateMemo(Stream memoStream)
        {
            DbfMemo memo = null;

            if (this.Header.IsFoxPro)
            {
                memo = new DbfMemoFoxPro(memoStream, this.CurrentEncoding);
            }
            else
            {
                if (this.Header.Version == DBFVersion.FoxBaseDBase3WithMemo)
                {
                    memo = new DbfMemoDbase3(memoStream, this.CurrentEncoding);
                }
            }

            return memo;
        }

        private DbfMemo CreateMemo(string path)
        {
            DbfMemo memo = null;

            if (this.Header.IsFoxPro)
            {
                memo = new DbfMemoFoxPro(path, this.CurrentEncoding);
            }
            else
            {
                if (this.Header.Version == DBFVersion.FoxBaseDBase3WithMemo)
                {
                    memo = new DbfMemoDbase3(path, this.CurrentEncoding);
                }
            }

            return memo;
        }

        private DBFHeader ReadHeader()
        {
            try
            {
                byte[] buffer = this.BinaryReader.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

                // Marshall the header into a DBFHeader structure
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                this.Header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
                handle.Free();
            }
            catch (Exception e)
            {
                this.logger?.Error(e);
                throw new Exception($"Ошибка чтения заголовка таблицы '{this.FileName}'\nОписание:\n{e.Message}");
            }

            return this.Header;
        }

        private bool ReadFields()
        {
            this.FieldsByIndex = new List<DbfField>();
            this.FieldsByName = new Dictionary<string, DbfField>();
            try
            {
                byte[] buffer;
                GCHandle handle;
                while (this.BinaryReader.PeekChar() != TERMINATOR)
                {
                    buffer = this.BinaryReader.ReadBytes(Marshal.SizeOf(typeof(DbfField)));
                    handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    var fieldDescriptor = (DbfField)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DbfField));

                    // fieldDescriptor.Name = CurrentEncoding.GetString(Encoding.Default.GetBytes(fieldDescriptor.Name));
                    if ((fieldDescriptor.Flags & DBFFieldFlags.System) != DBFFieldFlags.System)
                    {
                        this.FieldsByIndex.Add(fieldDescriptor);
                        this.FieldsByName.Add(fieldDescriptor.GetName(this.CurrentEncoding), fieldDescriptor);
                    }

                    handle.Free();
                }

                var terminator = this.BinaryReader.ReadByte();
                if (terminator != TERMINATOR)
                {
                    throw new InvalidDataException("Invalid DBF format");
                }
            }
            catch (Exception e)
            {
                this.logger?.Error(e);
                throw new Exception($"Ошибка чтения описаний полей таблицы '{this.FileName}'\nОписание:\n{e.Message}");
            }

            return true;
        }

        #endregion
    }
}
