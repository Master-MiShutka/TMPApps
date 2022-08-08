namespace TMP.Extensions
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Encryption and decryption methods based on a password key.
    /// </summary>
    public static class EncryptDecrypt
    {
        /// <summary>
        /// Encrypt a byte array into a byte array using a key and an IV
        /// </summary>
        /// <param name="clearData">Byte array to encrypt</param>
        /// <param name="key">Key</param>
        /// <param name="iV">IV</param>
        /// <returns>Encrypted byte array</returns>
        public static byte[] Encrypt(byte[] clearData, byte[] key, byte[] iV)
        {
            MemoryStream memoryStream = new MemoryStream();
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = key;
            rijndael.IV = iV;
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(clearData, 0, clearData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Encrypt a string into a string using a password
        /// </summary>
        /// <param name="clearText">String to encrypt</param>
        /// <param name="password">Password</param>
        /// <returns>Encrypted string</returns>
        public static string Encrypt(string clearText, string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118,
            });
            byte[] inArray = EncryptDecrypt.Encrypt(bytes, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// Encrypt bytes into bytes using a password
        /// </summary>
        /// <param name="clearData">Byte array to encrypt</param>
        /// <param name="password">Password</param>
        /// <returns>Encrypted byte array</returns>
        public static byte[] Encrypt(byte[] clearData, string password)
        {
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118,
            });
            return EncryptDecrypt.Encrypt(clearData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
        }

        /// <summary>
        /// Encrypt a file into another file using a password
        /// </summary>
        /// <param name="fileIn">File to be encrypted</param>
        /// <param name="fileOut">Filename where encrypted data will be saved</param>
        /// <param name="password">Password</param>
        public static void Encrypt(string fileIn, string fileOut, string password)
        {
            FileStream fileStream = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            FileStream stream = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118,
            });
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = passwordDeriveBytes.GetBytes(32);
            rijndael.IV = passwordDeriveBytes.GetBytes(16);
            CryptoStream cryptoStream = new CryptoStream(stream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            int num = 4096;
            byte[] buffer = new byte[num];
            int num2;
            do
            {
                num2 = fileStream.Read(buffer, 0, num);
                cryptoStream.Write(buffer, 0, num2);
            }
            while (num2 != 0);
            cryptoStream.Close();
            fileStream.Close();
        }

        /// <summary>
        /// Decrypt a byte array into a byte array using a key and an IV
        /// </summary>
        /// <param name="cipherData">Byte array to decrypt</param>
        /// <param name="key">Password Key</param>
        /// <param name="iV">IV</param>
        /// <returns>Returns decrypted byte array</returns>
        public static byte[] Decrypt(byte[] cipherData, byte[] key, byte[] iV)
        {
            MemoryStream memoryStream = new MemoryStream();
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = key;
            rijndael.IV = iV;
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(cipherData, 0, cipherData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Decrypt a string into a string using a password
        /// </summary>
        /// <param name="cipherText">String to decrypt</param>
        /// <param name="password">password</param>
        /// <returns>Decrypted string</returns>
        public static string Decrypt(string cipherText, string password)
        {
            byte[] cipherData = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes passwordDeriveBytes = new(password, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118,
            });
            byte[] bytes = EncryptDecrypt.Decrypt(cipherData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
            return Encoding.Unicode.GetString(bytes);
        }

        /// <summary>
        /// Decrypt bytes into bytes using a password
        /// </summary>
        /// <param name="cipherData">Byte array to decrypt</param>
        /// <param name="password">password</param>
        /// <returns>Returns decrypted byte array</returns>
        public static byte[] Decrypt(byte[] cipherData, string password)
        {
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118,
            });
            return EncryptDecrypt.Decrypt(cipherData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
        }

        /// <summary>
        /// Decrypt a file into another file using a password
        /// </summary>
        /// <param name="fileIn">File to decrypt</param>
        /// <param name="fileOut">Filename where decrypted data will be saved</param>
        /// <param name="password">password</param>
        public static void Decrypt(string fileIn, string fileOut, string password)
        {
            FileStream fileStream = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            FileStream stream = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, new byte[]
            {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118,
            });
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = passwordDeriveBytes.GetBytes(32);
            rijndael.IV = passwordDeriveBytes.GetBytes(16);
            CryptoStream cryptoStream = new CryptoStream(stream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
            int num = 4096;
            byte[] buffer = new byte[num];
            int num2;
            do
            {
                num2 = fileStream.Read(buffer, 0, num);
                cryptoStream.Write(buffer, 0, num2);
            }
            while (num2 != 0);
            cryptoStream.Close();
            fileStream.Close();
        }
    }
}
