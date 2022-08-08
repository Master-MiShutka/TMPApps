namespace DBF
{
    using System.IO;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// Файл типа memo содержит одну запись заголовка и
    /// произвольное число блочных структур.В записи заголовка
    /// располагается указатель на следующий свободный блок и размер
    /// блока в байтах.Размер устанавливается командой SET
    /// BLOCKSIZE при создании файла. Запись заголовка начинается с
    /// нулевой позиции файла и занимает 512 байтов.
    /// За записью заголовка следуют блоки, в которых
    /// содержатся заголовок блока и текст memo.В файл базы данных
    /// включены номера блоков, которые используются для ссылки на
    /// блоки memo.Расположение блока в файле типа memo
    /// определяется умножением номера блока на размер блока
    /// (находящийся в записи заголовка файла типа memo). Все блоки
    /// memo начинаются с четных адресов границ блоков.Блок memo
    /// может занимать более, чем один последовательный блок.
    /// (находящийся в записи заголовка файла типа memo). Все  блоки
    /// memo начинаются  с четных  адресов границ  блоков.Блок memo
    /// может занимать более, чем один последовательный блок.
    ///
    ///  г==========================================================¬
    ///  ¦              Запись заголовка файла типа memo            ¦
    ///  ¦-------T--------------------------------------------------¦
    ///  ¦ Байты ¦                Описание                          ¦
    ///  ¦=======+==================================================¦
    ///  ¦ 00-03 ¦Расположение следующего свободного блока*         ¦
    ///  ¦-------+--------------------------------------------------¦
    ///  ¦ 04-05 ¦Не используются                                   ¦
    ///  ¦-------+--------------------------------------------------¦
    ///  ¦ 06-07 ¦Размер блока (число байтов в блоке)               ¦
    ///  ¦-------+--------------------------------------------------¦
    ///  ¦ 08-511¦Не используются                                   ¦
    ///  ¦=======¦==================================================¦
    ///  ¦          Заголовок блока memo и текст memo               ¦
    ///  ¦=======T==================================================¦
    ///  ¦ 00-03 ¦Сигнатура блока* (указывает тип данных в блоке):  ¦
    ///  ¦       ¦ а. 0 - шаблон(поле типа шаблон);                 ¦
    ///  ¦       ¦ б. 1 - текст(поле типа memo)                     ¦
    ///  ¦-------+--------------------------------------------------¦
    ///  ¦ 04-07 ¦Длина* memo(в байтах)                             ¦
    ///  ¦-------+--------------------------------------------------¦
    ///  ¦ 08-n  ¦Текст memo(n= длина)                              ¦
    ///  L=======¦==================================================-
    /// </remarks>
    public class DbfMemoFoxPro : DbfMemo
    {
        public DbfMemoFoxPro(string path)
            : this(path, Encoding.GetEncoding(866))
        {
        }

        public DbfMemoFoxPro(string path, Encoding encoding)
            : base(path, encoding)
        {
            this.BlockSize = this.CalculateBlockSize();
        }

        public DbfMemoFoxPro(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
            this.BlockSize = this.CalculateBlockSize();
        }

        public override int BlockSize { get; }

        private int CalculateBlockSize()
        {
            this.BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

            this.BinaryReader.ReadUInt32(); // next block
            this.BinaryReader.ReadUInt16(); // unused
            return this.BinaryReader.ReadBigEndianInt16();
        }

        public override string BuildMemo(long startBlock)
        {
            var offset = this.Offset(startBlock);

            if (offset > this.BinaryReader.BaseStream.Length)
            {
                return string.Empty;
            }

            string value;
            lock (this.BinaryReader)
            {
                this.BinaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                var blockType = this.BinaryReader.ReadBigEndianInt32();
                var memoLength = this.BinaryReader.ReadBigEndianInt32();

                if (blockType != 1 || memoLength == 0)
                {
                    return string.Empty;
                }

                value = this.BinaryReader.ReadString(memoLength, this.CurrentEncoding);
            }

            return value;
        }
    }
}
