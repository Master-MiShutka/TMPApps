using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace TMP.Shared.Serialization
{
    public class CustomSerializer
    {
        private static T DecryptAndDeserialize<T>(string fileName, SymmetricAlgorithm key)
        {
            T obj;

            using (FileStream fs = File.Open(fileName, FileMode.Open))
            using (CryptoStream cs = new CryptoStream(fs, key.CreateDecryptor(), CryptoStreamMode.Read))
            using (GZipStream gzip = new GZipStream(cs, CompressionMode.Decompress, false))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                obj = (T)formatter.Deserialize(gzip);
            }

            return obj;
        }

        public static T LoadFromFile<T>(string fileName)
        {
            using (DESCryptoServiceProvider key = new DESCryptoServiceProvider())
            {
                key.Key = Encoding.ASCII.GetBytes("ABCDEFGH");
                key.IV = Encoding.ASCII.GetBytes("ABCDEFGH");

                return DecryptAndDeserialize<T>(fileName, key);
            }
        }

        private static void EncryptAndSerialize<T>(T obj, string fileName, SymmetricAlgorithm key)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Create))
            using (CryptoStream cs = new CryptoStream(fs, key.CreateEncryptor(), CryptoStreamMode.Write))
            using (GZipStream gzip = new GZipStream(cs, CompressionMode.Compress, false))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(gzip, obj);
            }
        }

        public static void SaveToFile<T>(T obj, string fileName)
        {
            DESCryptoServiceProvider key = new DESCryptoServiceProvider();
            key.Key = Encoding.ASCII.GetBytes("ABCDEFGH");
            key.IV = Encoding.ASCII.GetBytes("ABCDEFGH");

            string tempFileName = Path.GetTempFileName();
            try
            {
                EncryptAndSerialize<T>(obj, tempFileName, key);
                EncryptAndSerialize<T>(obj, fileName, key);
            }
            catch
            {
                throw;
            }
            finally
            {
                key.Clear();
            }
        }
    }
}
