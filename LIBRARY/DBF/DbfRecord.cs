namespace DBF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class DbfRecord
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const byte ENDOFFILE = 0x1a;

        private DbfTable dbfTable;

        public DbfRecord(DbfTable dbfTable)
        {
            this.dbfTable = dbfTable;

            this.Values = new List<IDbfValue>();

            foreach (var field in dbfTable.FieldsByIndex)
            {
                var dbfValue = this.dbfTable.CreateDbfValue(field);
                this.Values.Add(dbfValue);
            }
        }

        public bool IsDeleted { get; private set; }

        public IList<IDbfValue> Values { get; init; }

        public bool Read(BinaryReader binaryReader)
        {
            if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
            {
                return false;
            }

            try
            {
                var value = binaryReader.ReadByte();
                if (value == ENDOFFILE)
                {
                    return false;
                }

                this.IsDeleted = value == 0x2A;

                for (byte index = 0; index < this.dbfTable.FieldsByIndex.Count; index++)
                {
                    var dbfValue = this.Values[index];
                    try
                    {
                        dbfValue.Read(binaryReader);
                    }
                    catch (Exception e)
                    {
                        logger?.Error(e);
                        System.Diagnostics.Debug.WriteLine($"DBFRecord Read: error on field #{index}, dbfValue.Key: {this.dbfTable.CreateDbfValueByIndex(index)}");
                    }

                    this.Values[index] = dbfValue;
                }

                return true;
            }
            catch (EndOfStreamException)
            {
                return false;
            }
        }

        public bool Read(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return false;
            }

            this.IsDeleted = bytes[0] == 0x2A;
            try
            {
                int bytesIndex = 1; // начинаем с единицы, т.к. первый байт признак, что запись отмечена к удалению
                for (byte index = 0; index < this.dbfTable.FieldsByIndex.Count; index++)
                {
                    var dbfValue = this.Values[index];
                    try
                    {
                        dbfValue.Read(bytes, bytesIndex);
                        bytesIndex += dbfValue.Length;
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debug.WriteLine($"DBFRecord Read: error on field #{index}, dbfValue.Key: {this.dbfTable.CreateDbfValueByIndex(index)}");
                    }

                    this.Values[index] = dbfValue;
                }
            }
            catch (Exception e)
            {
                logger?.Error(e);
                System.Diagnostics.Debug.WriteLine($"DBFRecord Read: error");
            }

            return true;
        }

        public bool Read(ArraySegment<byte> segment)
        {
            if (segment == null || segment.Count == 0)
            {
                return false;
            }

            this.IsDeleted = segment[0] == 0x2A;
            try
            {
                int bytesIndex = 1; // начинаем с единицы, т.к. первый байт признак, что запись отмечена к удалению
                for (byte index = 0; index < this.dbfTable.FieldsByIndex.Count; index++)
                {
                    var dbfValue = this.Values[index];
                    try
                    {
                        Span<byte> span = segment.Slice(bytesIndex, dbfValue.Length).AsSpan();
                        dbfValue.Read(span);
                        bytesIndex += dbfValue.Length;
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debug.WriteLine($"DBFRecord Read: error on field #{index}, dbfValue.Key: {this.dbfTable.CreateDbfValueByIndex(index)}");
                    }

                    this.Values[index] = dbfValue;
                }
            }
            catch (Exception e)
            {
                logger?.Error(e);
                System.Diagnostics.Debug.WriteLine($"DBFRecord Read: error");
            }

            return true;
        }

        public object GetValue(byte ordinal)
        {
            var dbfValue = this.Values[ordinal];
            return dbfValue.GetValue();
        }

        public T GetValue<T>(byte ordinal)
        {
            var dbfValue = this.Values[ordinal];
            try
            {
                return (T)dbfValue.GetValue();
            }
            catch (InvalidCastException ice)
            {
                logger?.Error(ice);
                throw new InvalidCastException(
                    $"Unable to cast object of type '{dbfValue.GetValue().GetType().FullName}' to type '{typeof(T).FullName}' at ordinal '{ordinal}'.");
            }
        }

        public object GetValue(string fieldName)
        {
            int index = this.dbfTable.GetFieldNameIndex(fieldName);
            var dbfValue = this.Values[index];
            return dbfValue.GetValue();
        }

        public T GetValue<T>(string fieldName)
        {
            int index = this.dbfTable.GetFieldNameIndex(fieldName);
            if (index == -1)
            {
                // System.Diagnostics.Debugger.Break();
                return default;
            }

#if DEBUG
            Type genericType = typeof(T);
            Type dbfValueType = this.Values[index].BaseType;
            if (genericType != dbfValueType)
            {
                if (Nullable.GetUnderlyingType(dbfValueType) != null)
                {
                    dbfValueType = Nullable.GetUnderlyingType(dbfValueType);

                    if (genericType != dbfValueType)
                    {
                        logger?.Warn($"Неверный формат данных: поле '{fieldName}', ожидался тип данных '{genericType}', прочитаны данные тип '{dbfValueType}'.");
                        System.Diagnostics.Debugger.Break();

                        //TODO: Decimal to int32
                    }
                }
                else
                {
                    logger?.Warn($"Неверный формат данных: поле '{fieldName}', ожидался тип данных '{genericType}', прочитаны данные тип '{dbfValueType}'.");
                    System.Diagnostics.Debugger.Break();
                }
            }
#endif

            IDbfValue<T> dbfValue = this.Values[index] as IDbfValue<T>;
            try
            {
                return dbfValue == null ? default : dbfValue.Value;
            }
            catch (NullReferenceException e)
            {
                string msg = $"Unable to get value from object type '{typeof(T).FullName}' at field name '{fieldName}'.";
                logger?.Error(e, msg);
                throw new NullReferenceException(msg);
            }
            catch (InvalidCastException ice)
            {
                logger?.Error(ice);
                throw new InvalidCastException(
                    $"Unable to cast object of type '{dbfValue.GetValue().GetType().FullName}' to type '{typeof(T).FullName}' at field name '{fieldName}'.");
            }
        }

        public string GetString(string fieldName)
        {
            return this.GetValue<string>(fieldName)?.Trim();
        }
    }
}
