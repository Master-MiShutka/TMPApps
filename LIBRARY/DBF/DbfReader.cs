namespace DBF
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks.Dataflow;

    public class DbfReader : Disposable
    {
        #region Constructors

        public DbfReader(string pathToDbfFile, Encoding encoding, bool skipDeletedRecords)
        {
            this.SkipDeletedRecords = skipDeletedRecords;
            this.DbfTable = new DbfTable(pathToDbfFile, encoding, skipDeletedRecords);
            this.DbfRecord = new DbfRecord(this.DbfTable);
        }

        public DbfReader(string pathToDbfFile, bool skipDeletedRecords)
        {
            this.SkipDeletedRecords = skipDeletedRecords;
            this.DbfTable = new DbfTable(pathToDbfFile, Encoding.GetEncoding(866), skipDeletedRecords);
            this.DbfRecord = new DbfRecord(this.DbfTable);
        }

        public DbfReader(Stream stream, Encoding encoding, bool skipDeletedRecords)
        {
            this.SkipDeletedRecords = skipDeletedRecords;
            this.DbfTable = new DbfTable(stream, encoding, skipDeletedRecords);
            this.DbfRecord = new DbfRecord(this.DbfTable);
        }

        public DbfReader(Stream stream, Stream memoStream, Encoding encoding, bool skipDeletedRecords)
        {
            this.SkipDeletedRecords = skipDeletedRecords;
            this.DbfTable = new DbfTable(stream, memoStream, encoding, skipDeletedRecords);
            this.DbfRecord = new DbfRecord(this.DbfTable);
        }

        #endregion

        #region IDisposable implementation

        protected override void Dispose(bool dispose)
        {
            try
            {
                this.DbfTable.Close();
            }
            finally
            {
                this.DbfTable = null;
                this.DbfRecord = null;
            }
        }

        #endregion

        #region Public properties

        public bool SkipDeletedRecords { get; set; } = true;

        public DbfTable DbfTable { get; private set; }

        public DbfRecord DbfRecord { get; private set; }

        #endregion

        #region Public methods

        public DbfRecord ReadRecord()
        {
            DbfRecord dbfRecord;
            bool skip;
            do
            {
                dbfRecord = this.DbfTable.ReadRecord();
                if (dbfRecord == null)
                {
                    break;
                }

                skip = this.SkipDeletedRecords && this.DbfRecord.IsDeleted;
            }
            while (skip);

            return dbfRecord;
        }

        /// <summary>
        /// Чтение записи из массива байт
        /// </summary>
        /// <param name="bytes">массив байт</param>
        /// <returns>запись</returns>
        public DbfRecord ReadRecord(byte[] bytes)
        {
            var dbfRecord = new DbfRecord(this.DbfTable);
            return !dbfRecord.Read(bytes)
                ? null
                : (this.SkipDeletedRecords && dbfRecord.IsDeleted ? null : dbfRecord);
        }

        public DbfRecord ReadRecord(ArraySegment<byte> segment)
        {
            var dbfRecord = new DbfRecord(this.DbfTable);
            return !dbfRecord.Read(segment)
                ? null
                : (this.SkipDeletedRecords && dbfRecord.IsDeleted ? null : dbfRecord);
        }

        public T GetValue<T>(byte ordinal)
        {
            return this.DbfRecord.GetValue<T>(ordinal);
        }

        public T? GetNullableValue<T>(byte ordinal)
            where T : struct
        {
            return this.GetValue<T>(ordinal);
        }

        /// <summary>
        /// Чтение записей из файла и создание модели данных
        /// </summary>
        /// <param name="recordParseAction">Процедура обратного вызова для каждой записи для создания модели данных</param>
        public void ParseRecords(Action<DbfRecord> recordParseAction)
        {
            // размер записи в байтах
            int recordSize = this.DbfHeader.RecordLength;

            // структура общего назначения для асинхронного обмена сообщениями
            // хранит массив байтов, прочитанных из файла
            var bytesBuffer = new BufferBlock<byte[]>(new DataflowBlockOptions { BoundedCapacity = BUFFERSIZE });

            // парсинг массива байтов: нарезка массива на части равные длине записи и ее парсинг
            var transormBytesToDbfRecord = this.GetBytesToDbfRecodsTransformBlock();

            // разбор полученных записий и преобразование в модель данных
            int passed = 0;
            var parseRecords = new ActionBlock<DbfRecord[]>(records =>
            {
                foreach (var record in records)
                {
                    recordParseAction(record);
                    passed++;
                }
            },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 100,
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                    SingleProducerConstrained = false,
                });
            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true, // true, успешное или неуспешное завершение одного блока в конвейере приведет к завершению следующего блока в конвейере.
            };

            // создание конвеера обработки данных
            bytesBuffer.LinkTo(transormBytesToDbfRecord, linkOptions);
            transormBytesToDbfRecord.LinkTo(parseRecords, linkOptions);

            // запуск чтения файла и заполнения буфера
            this.ReadDataFromFile(bytesBuffer);

            while (parseRecords.Completion.IsCompleted == false) { }

            try
            {
                // ожидание завершения
                parseRecords.Completion.Wait();

                System.Diagnostics.Debugger.Log(0, "DbfReader", $"On table '{this.DbfTable.FileName}' skipped {this.skipped} records\n");
            }
            catch (AggregateException ae)
            {
                this.logger?.Error(ae);
            }
        }

        #endregion

        #region Private properties

        /// <summary>
        /// заголовок таблицы
        /// </summary>
        private DBFHeader DbfHeader => this.DbfTable.Header;

        /// <summary>
        /// количество блоков записей, которые можно прочитать за раз с учетом размера буфера
        /// </summary>
        private int BlockRecordsCount => BUFFERSIZE / this.DbfHeader.RecordLength;

        /// <summary>
        /// размера буфера
        /// </summary>
        private int BufferSize => this.BlockRecordsCount * this.DbfHeader.RecordLength;

        private int GetBytesToDbfRecodsTransformBlockBoundedCapacity => 20; // BlockRecordsCount;

        #endregion

        #region Private methods

        /// <summary>
        /// чтение сырых данных из файла в буфер
        /// </summary>
        /// <param name="bytesBuffer">буфер байтов, прочитанных из файла</param>
        private void ReadDataFromFile(ITargetBlock<byte[]> bytesBuffer, Action callback = null)
        {
            int bufferSize = this.BufferSize;
            int totalBytesToRead = this.DbfHeader.RecordLength * this.DbfHeader.NumberOfRecords;

            // количество циклов чтения: = 1, если данных мало
            int cyclesCount = 1;

            // корректировка размера буфеоа, если данных мало
            if (totalBytesToRead < this.BufferSize)
            {
                bufferSize = totalBytesToRead;
            }
            else
            {
                // количество циклов чтения: общий размер данных для чтения / размер буфера
                cyclesCount = totalBytesToRead / this.BufferSize;

                // корректировка округления
                if (cyclesCount * this.BufferSize < totalBytesToRead)
                {
                    cyclesCount++;
                }
            }

            // буфер
            byte[] bytes = ArrayPool<byte>.Shared.Rent(bufferSize);
            int readedBytesCount = 0;
            try
            {
                // читаем cyclesCount - 1 раз
                for (int i = 0; i < cyclesCount - 1; i++)
                {
                    @do(bytes, bufferSize);
                }

                // читаем оставшиеся байты
                if (totalBytesToRead - readedBytesCount > bufferSize)
                {
                    System.Diagnostics.Debugger.Break();
                }

                @do(bytes, totalBytesToRead - readedBytesCount);

                // проверка
                if (totalBytesToRead != readedBytesCount)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            catch (Exception e)
            {
                this.logger?.Error(e);
            }

            // return the array to pool
            ArrayPool<byte>.Shared.Return(bytes);

            // чтение файла завершено
            bytesBuffer.Complete();

            void @do(byte[] buffer, int length)
            {
                buffer = this.DbfTable.BinaryReader.ReadBytes(length);
                int readedCount = buffer.Length;
                if (readedCount != length)
                {
                    System.Diagnostics.Debugger.Break();
                }

                readedBytesCount += readedCount;

                // попытка передать данные в буфер
                while (bytesBuffer.Post(buffer) == false)
                {
                    System.Threading.Thread.Sleep(50);
                }

                callback?.Invoke();
            }
        }

        /// <summary>
        /// Возращает блок выполнения для преобразования переданного
        /// массива байтов в массив записей <see cref="DbfRecord"/>
        /// </summary>
        /// <returns>массив записей <see cref="DbfRecord"/></returns>
        private TransformBlock<byte[], DbfRecord[]> GetBytesToDbfRecodsTransformBlock()
        {
            // парсинг массива байтов: нарезка массива на части равные длине записи и ее парсинг
            return new TransformBlock<byte[], DbfRecord[]>(bytes =>
            {
                // что-то пошло не так - получено количество байтов не кратное длине записи
                if (bytes.Length % this.DbfHeader.RecordLength != 0)
                {
                    System.Diagnostics.Debugger.Break();
                }

                // количество записей
                int count = bytes.Length / this.DbfHeader.RecordLength;
                List<DbfRecord> dbfRecords = new (count);
                try
                {
                    int position = 0;
                    for (int i = 0; i < count; i++)
                    {
                        ArraySegment<byte> oneRecord = new ArraySegment<byte>(bytes, position, this.DbfHeader.RecordLength);

                        DbfRecord dbfRecord = this.ReadRecord(oneRecord);
                        if (dbfRecord == null)
                        {
                            this.skipped++;
                        }
                        else
                        {
                            dbfRecords.Add(dbfRecord);
                        }

                        position += this.DbfHeader.RecordLength;
                    }
                }
                catch (Exception e)
                {
                    this.logger?.Error(e);
                }

                return dbfRecords.ToArray();
            },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = this.GetBytesToDbfRecodsTransformBlockBoundedCapacity,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    SingleProducerConstrained = true,
                });
        }

        #endregion

        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private int skipped = 0;

        #endregion

        #region Constants

        private const int BUFFERSIZE = 64 * 1024;

        #endregion
    }
}
