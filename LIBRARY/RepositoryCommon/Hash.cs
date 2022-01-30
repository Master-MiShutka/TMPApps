namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;

    public static class Hash
    {
        public static string GetHash(this object instance, string hashName)
        {
            HashAlgorithm cryptoServiceProvider = HashAlgorithm.Create(hashName);
            return ComputeHash(instance, cryptoServiceProvider);
        }

        private static string ComputeHash(object instance, HashAlgorithm cryptoServiceProvider)
        {
            DataContractSerializer serializer = new DataContractSerializer(instance.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, instance);
                cryptoServiceProvider.ComputeHash(memoryStream.ToArray());
                return Convert.ToBase64String(cryptoServiceProvider.Hash);
            }
        }

        public static string GetMD5Hash(this object instance)
        {
            MD5 md5 = (MD5)HashAlgorithm.Create("MD5");

            return ComputeHash(instance, md5);
        }

        public static string GetSHA1Hash(this object instance)
        {
            SHA1 sha1 = (SHA1)HashAlgorithm.Create("SHA1");

            return ComputeHash(instance, sha1);
        }
    }
}
