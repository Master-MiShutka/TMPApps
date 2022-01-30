namespace TMP.Common.RepositoryCommon
{
    using System.IO;
    using System.IO.Compression;
    using System.Security.Cryptography;
    using System.Text;

    public class CustomSerializer
    {
        public static string Key { get; set; } = "ABCDEFGH";

        private static T DecryptAndDeserialize<T>(string fileName, SymmetricAlgorithm key)
        {
            T obj;
            using (FileStream fs = File.Open(fileName, FileMode.Open))
            using (CryptoStream cs = new CryptoStream(fs, key.CreateDecryptor(), CryptoStreamMode.Read))
            using (GZipStream gzip = new GZipStream(cs, CompressionMode.Decompress, false))
            {
                obj = Binaron.Serializer.BinaronConvert.Deserialize<T>(gzip);
            }

            return obj;
        }

        public static T LoadFromFile<T>(string fileName)
        {
            using (DES key = DES.Create())
            {
                key.Key = Encoding.ASCII.GetBytes(Key);
                key.IV = Encoding.ASCII.GetBytes(Key);

                return DecryptAndDeserialize<T>(fileName, key);
            }
        }

        private static void EncryptAndSerialize<T>(T obj, string fileName, SymmetricAlgorithm key)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Create))
            using (CryptoStream cs = new CryptoStream(fs, key.CreateEncryptor(), CryptoStreamMode.Write))
            using (GZipStream gzip = new GZipStream(cs, CompressionMode.Compress, false))
            {
                Binaron.Serializer.BinaronConvert.Serialize(obj, gzip);
            }
        }

        public static void SaveToFile<T>(T obj, string fileName)
        {
            DES key = DES.Create();
            key.Key = Encoding.ASCII.GetBytes(Key);
            key.IV = Encoding.ASCII.GetBytes(Key);

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
