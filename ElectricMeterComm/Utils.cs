using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.ElectricMeterComm
{
    public static class Utils
    {
        public static byte[] HexToBytes(string input)
        {
            byte[] result = new byte[input.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
            }
            return result;
        }

        public static string ToHex(this byte value)
        {
            return value.ToString("X2");
        }
        public static string ToHex(this int value)
        {
            return value.ToString("X2");
        }
        public static string ToHex(this string value)
        {
            return Convert.ToByte(value).ToString("X2");
        }
    }
}
