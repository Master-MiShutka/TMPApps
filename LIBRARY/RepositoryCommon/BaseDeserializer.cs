namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    public class BaseDeserializer<T>
        where T : class, new()
    {
        #region XML

        public static T XmlDeSerialize(string fileName)
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

        public static T XmlDataContractDeSerialize(string fileName, Type[] types)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(T), types);

            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs);
            T result = (T)ser.ReadObject(xmlReader, true);
            return result;
        }

        #endregion

        #region GzXML

        public static T GzXmlDataContractDeSerialize(string fileName, Type[] types)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(T), types);

            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false);
            using System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(gzip);
            T result = (T)ser.ReadObject(xmlReader, true);
            return result;
        }

        #endregion

        #region GzJson

        public static async Task<T> GzJsonDeserializeAsync(string fileName)
        {
            if (System.IO.File.Exists(fileName) == false)
            {
                return null;
            }

            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            using System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false);

            // using StreamReader reader = new StreamReader(gz, Encoding.UTF8);
            T result = await MessagePack.MessagePackSerializer.DeserializeAsync<T>(gz, MessagePack.MessagePackSerializer.DefaultOptions);

            // string json = await reader.ReadToEndAsync();
            // result = JsonDeserializeFromString(json, toLog);
            return result;
        }

        #endregion

        #region Json

        public static async Task<T> JsonDeserializeAsync(string fileName)
        {
            if (System.IO.File.Exists(fileName) == false)
            {
                return null;
            }

            using System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            T result = await MessagePack.MessagePackSerializer.DeserializeAsync<T>(fs, MessagePack.MessagePackSerializer.DefaultOptions);

            /*using StreamReader reader = new StreamReader(fs, Encoding.UTF8);
            string json = await reader.ReadToEndAsync();
            result = JsonDeserializeFromString(json, toLog);*/
            return result;
        }

        public static async Task<T> JsonDeserializeFromStreamAsync(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            T result;
            result = await MessagePack.MessagePackSerializer.DeserializeAsync<T>(stream, MessagePack.MessagePackSerializer.DefaultOptions);

            /*using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string json = await reader.ReadToEndAsync();
            result = JsonDeserializeFromString(json, toLog);*/
            return result;
        }

        public static async Task<string> ReadJsonFromStreamAsync(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string json = await reader.ReadToEndAsync();

            return json;
        }

        public static T JsonDeserializeFromBytes(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            T result = MessagePack.MessagePackSerializer.Deserialize<T>(data, MessagePack.MessagePackSerializer.DefaultOptions);

            // result = System.Text.Json.JsonSerializer.Deserialize<T>(data);
            return result;
        }

        #endregion

    }
}
