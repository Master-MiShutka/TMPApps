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
        public static string GetHash<T>(this object instance)
            where T : HashAlgorithm, new()
        {
            T cryptoServiceProvider = new T();
            return ComputeHash(instance, cryptoServiceProvider);
        }

        public static string GetKeyedHash<T>(this object instance, byte[] key)
            where T : KeyedHashAlgorithm, new()
        {
            T cryptoServiceProvider = new T { Key = key };
            return ComputeHash(instance, cryptoServiceProvider);
        }

        private static string ComputeHash<T>(object instance, T cryptoServiceProvider)
            where T : HashAlgorithm, new()
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
            return instance.GetHash<MD5CryptoServiceProvider>();
        }

        public static string GetSHA1Hash(this object instance)
        {
            return instance.GetHash<SHA1CryptoServiceProvider>();
        }
    }
}
