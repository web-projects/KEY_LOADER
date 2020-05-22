using System;
using System.Text;

namespace Devices.Common.Helpers
{
    public static class ConversionHelper
    {
        /// <summary>
        /// Expects string in Hexadecimal format
        /// </summary>
        /// <param name="valueInHexadecimalFormat"></param>
        /// <returns>returns byte array</returns>
        public static byte[] HexToByteArray(String valueInHexadecimalFormat)
        {
            int NumberChars = valueInHexadecimalFormat.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(valueInHexadecimalFormat.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Expects string in Ascii format
        /// </summary>
        /// <param name="valueInAsciiFormat"></param>
        /// <returns>returns byte array</returns>
        public static byte[] AsciiToByte(string valueInAsciiFormat)
        {
            return UnicodeEncoding.ASCII.GetBytes(valueInAsciiFormat);
        }

        /// <summary>
        /// Expects byte array and converts it to Hexadecimal formatted string
        /// </summary>
        /// <param name="value"></param>
        /// <returns>returns Hexadecimal formatted string</returns>
        public static string ByteArrayToHexString(byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", "");
        }

        /// <summary>
        /// Expects byte array and converts it to Ascii formatted string
        /// </summary>
        /// <param name="value"></param>
        /// <returns>returns ascii formatted string</returns>
        public static string ByteArrayToAsciiString(byte[] value)
        {
            return UnicodeEncoding.ASCII.GetString(value);
        }

        /// <summary>
        /// Expects the first array to equal or smaller than the second array
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static byte[] XORArrays(byte[] array1, byte[] array2)
        {
            byte[] result = new byte[array1.Length];
            for (int i = 0; i < array1.Length; i++)
            {
                result[i] = (byte)(array1[i] ^ array2[i]);
            }
            return result;
        }
    }

}
