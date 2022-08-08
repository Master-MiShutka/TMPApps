namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;

    public static class XmlDeserializer
    {
        public static T FromGzFile<T>(string fileName, Type[] types)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(T), types);

            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false);
            using System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(gzip);
            T result = (T)ser.ReadObject(xmlReader, true);
            return result;
        }

        public static T FromFile<T>(string fileName)
            where T : class, new()
        {
            T result = new T();

            // DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(DataModel));
            Type t = typeof(T);
            Type[] extraTypes = t.GetProperties()
                .Where(p => p.PropertyType.IsInterface)
                .Select(p => p.GetValue(result, null).GetType())
                .ToArray();

            System.Xml.Serialization.XmlSerializer deserializer = new System.Xml.Serialization.XmlSerializer(typeof(T), extraTypes);
            string xmlData = File.ReadAllText(fileName);
            using MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlData));
            object obj = deserializer.Deserialize(stream);
            T data = (T)obj;
            result = data;
            return result;
        }

        public static T FromFileWithDataContract<T>(string fileName, Type[] types)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(T), types);

            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs);
            T result = (T)ser.ReadObject(xmlReader, true);
            return result;
        }
    }
}
