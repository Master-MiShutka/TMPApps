using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WPFHexaEditor.Core.Bytes
{
    /// <summary>
    /// ByteCharConverter for convert data
    /// </summary>
    public class ByteConverters
    {
        /// <summary>
        /// Convert Byte to Char
        /// </summary>
        public static char ByteToChar(byte b)
        {
            return b > 0x1F && !(b > 0x7E && b < 0xA0) ? (char)b : '.';
        }

        /// <summary>
        /// Convert Char to Byte 
        /// </summary>
        public static byte CharToByte(char c)
        {
            return (byte)c;
        }

        /// <summary>
        /// Converts a byte array to a hex string. For example: {10,11} = "0A 0B"
        /// </summary>
        public static string ByteToHex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in data)
            {
                string hex = ByteToHex(b);
                sb.Append(hex);
                sb.Append(" ");
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            
            return sb.ToString();
        }
        /// <summary>
        /// Converts the byte to a hex string. For example: "10" = "0A";
        /// </summary>
        public static string ByteToHex(byte b)
        {
            string sB = b.ToString(ConstantReadOnly.HexStringFormat, CultureInfo.InvariantCulture );//System.Threading.Thread.CurrentThread.CurrentCulture);

            if (sB.Length == 1)
                sB = "0" + sB;

            return sB;
        }

        /// <summary>
        /// Convert byte to ASCII string
        /// </summary>
        public static string BytesToString(byte[] buffer, ByteToString converter = ByteToString.ByteToCharProcess)
        {
            switch (converter)
            {
                case ByteToString.ASCIIEncoding:
                    return Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                case ByteToString.ByteToCharProcess:
                    StringBuilder builder = new StringBuilder();

                    foreach (byte @byte in buffer)                    
                        builder.Append(ByteToChar(@byte));                    

                    return builder.ToString();
            }

            return "";         
        }

        /// <summary>
        /// Converts the hex string to an byte array. The hex string must be separated by a space char ' '. If there is any invalid hex information in the string the result will be null.
        /// </summary>
        public static byte[] HexToByte(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return null;
            hex = hex.Trim();
            var hexArray = hex.Split(' ');
            var byteArray = new byte[hexArray.Length];

            for (int i = 0; i < hexArray.Length; i++)
            {
                var hexValue = hexArray[i];

                byte b;
                var isByte = HexToByte(hexValue, out b);
                if (!isByte)
                    return null;
                byteArray[i] = b;
            }

            return byteArray;
        }

        public static bool HexToByte(string hex, out byte b)
        {
            return byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b);
        }

        public static long HexLiteralToLong(string hex)
        {
            if (string.IsNullOrEmpty(hex)) throw new ArgumentException("hex");

            int i = hex.Length > 1 && hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X') ? 2 : 0;
            long value = 0;

            while (i < hex.Length)
            {
                int x = hex[i++];

                if
                    (x >= '0' && x <= '9') x = x - '0';
                else if
                    (x >= 'A' && x <= 'F') x = (x - 'A') + 10;
                else if
                    (x >= 'a' && x <= 'f') x = (x - 'a') + 10;
                else
                    throw new ArgumentOutOfRangeException("hex");

                value = 16 * value + x;

            }

            return value;
        }

        /// <summary>
        /// Check if is an hexa string
        /// </summary>
        /// <param name="hexastring"></param>
        /// <returns></returns>
        public static bool IsHexaValue(string hexastring)
        {
            long position = 0;
            try
            {
                position = HexLiteralToLong(hexastring);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Convert string to byte array
        /// </summary>
        public static byte[] StringToByte(string str)
        {
            List<byte> byteList = new List<byte>();

            foreach (char c in str)            
                byteList.Add(CharToByte(c));            

            return byteList.ToArray();
        }

        /// <summary>
        /// Convert String to hex string For example: "barn" = "62 61 72 6e"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToHex(string str)
        {
            return ByteToHex(StringToByte(str));
        }
    }
}
