namespace DBF
{
    using System;
    using System.IO;
    using System.Text;

    public static class BinaryReaderExtensions
    {
        private const char NullChar = '\0';

        public static short ReadBigEndianInt16(this BinaryReader binaryReader)
        {
            var bytes = binaryReader.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static ushort ReadBigEndianUInt16(this BinaryReader binaryReader)
        {
            var bytes = binaryReader.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static int ReadBigEndianInt32(this BinaryReader binaryReader)
        {
            var bytes = binaryReader.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static uint ReadBigEndianUInt32(this BinaryReader binaryReader)
        {
            var bytes = binaryReader.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string ReadString(this BinaryReader binaryReader, int fieldLength, Encoding encoding)
        {
            var chars = binaryReader.ReadBytes(fieldLength);
            if (chars[0] == NullChar)
            {
                // return null;
            }

            byte[] bytes = new byte[chars.Length];

            int j = 0;
            for (int index = 0; index < chars.Length; index++)
            {
                if ((byte)chars[index] < 32)
                {
                    if ((byte)chars[index] == 13)
                    {
                        bytes[j++] = 13;
                    }
                }
                else
                {
                    bytes[j++] = chars[index];
                }
            }

            string value = encoding.GetString(bytes);

            return value.Trim('\0');
        }
    }
}
