using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using Newtonsoft;

namespace TMP.Common.RepositoryCommon
{
    public class BaseRepository<T> where T : class, new()
    {
        public static bool XmlSerialize(T model, string fileName, Action<Exception> toLog = null)
        {
            try
            {
                Type t = typeof(T);
                Type[] extraTypes = t.GetProperties()
                    .Where(p => p.PropertyType.IsInterface)
                    .Select(p => p.GetValue(model, null).GetType())
                    .ToArray();

                XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
                using (TextWriter writer = new StreamWriter(fileName))
                {
                    serializer.Serialize(writer, model);
                }
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                return false;
            }
            return true;
        }
        public static T XmlDeSerialize(string fileName, Action<Exception> toLog = null)
        {
            T result = null;

            //DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(DataModel));

            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(T));
                string xmlData = File.ReadAllText(fileName);
                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlData)))
                {
                    object obj = deserializer.Deserialize(stream);
                    T data = (T)obj;
                    result = data;
                }
            }
            catch (Exception ex)
            {
                toLog(ex); return null;
            }
            return result;
        }

        public static bool GzXmlDataContractSerialize(T model, string fileName, Type[] types, Action<Exception> toLog = null)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T), types);
                XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, Encoding = new System.Text.UTF8Encoding(false, false) }; // no BOM in a .NET string
                using (System.IO.FileStream fs = System.IO.File.Open(fileName, System.IO.FileMode.Create))
                using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress, false))
                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(gzip, settings))
                {
                    serializer.WriteObject(xmlWriter, model);
                }
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                return false;
            }
            return true;
        }
        public static T GzXmlDataContractDeSerialize(string fileName, Type[] types, Action<Exception> toLog = null)
        {
            T result = null;
            try
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(T), types);

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
                using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(gzip))
                {
                    result = (T)ser.ReadObject(xmlReader, true);
                }
            }
            catch (Exception ex) { toLog(ex); return null; }
            return result;
        }

        public static bool GzJsonSerialize(T model, string fileName, Action<Exception> toLog = null)
        {
            try
            {
                string json = JsonSerializeObject(model);

                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress))
                {
                    gz.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                return false;
            }
            return true;
        }
        public static bool JsonSerialize(T model, string fileName, Action<Exception> toLog = null)
        {
            try
            {
                string json = JsonSerializeObject(model);

                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                return false;
            }
            return true;
        }
        public static bool JsonSerializeToStream(T model, out Stream stream, Action<Exception> toLog = null)
        {
            try
            {
                string json = JsonSerializeObject(model);

                byte[] bytes = Encoding.UTF8.GetBytes(json);
                stream = new MemoryStream(bytes);
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                stream = null;
                return false;
            }
            return true;
        }
        public static bool JsonSerializeToString(T model, out string data, Action<Exception> toLog = null)
        {
            try
            {
                data = JsonSerializeObject(model);
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                data = null;
                return false;
            }
            return true;
        }
        public static T GzJsonDeSerialize(string fileName, Action<Exception> toLog = null)
        {
            if (System.IO.File.Exists(fileName) == false) return null;
            T result = null;
            try
            {
                string json = string.Empty;
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                using (System.IO.MemoryStream mso = new System.IO.MemoryStream())
                using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
                {
                    byte[] bytes = new byte[4096];

                    int cnt;

                    while ((cnt = gz.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        mso.Write(bytes, 0, cnt);
                    }
                    json = Encoding.UTF8.GetString(mso.ToArray());
                }
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects });
            }
            catch (Exception ex) { toLog(ex); return null; }
            return result;
        }
        public static T JsonDeSerialize(string fileName, Action<Exception> toLog = null)
        {
            if (System.IO.File.Exists(fileName) == false) return null;
            T result = null;
            try
            {
                string json = string.Empty;
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                using (System.IO.MemoryStream mso = new System.IO.MemoryStream())
                {
                    byte[] bytes = new byte[4096];

                    int cnt;

                    while ((cnt = fs.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        mso.Write(bytes, 0, cnt);
                    }
                    json = Encoding.UTF8.GetString(mso.ToArray());
                }
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects });
            }
            catch (Exception ex) { toLog(ex); return null; }
            return result;
        }
        public static T JsonDeSerializeFromStream(Stream stream, Action<Exception> toLog = null)
        {
            if (stream == null) return null;
            T result = null;
            try
            {
                string json = string.Empty;
                using (System.IO.MemoryStream mso = new System.IO.MemoryStream())
                {
                    byte[] bytes = new byte[4096];

                    int cnt;

                    while ((cnt = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        mso.Write(bytes, 0, cnt);
                    }
                    json = Encoding.UTF8.GetString(mso.ToArray());
                }
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects });
            }
            catch (Exception ex) { toLog(ex); return null; }
            return result;
        }
        public static T JsonDeSerializeFromString(string data, Action<Exception> toLog = null)
        {
            if (data == null) return null;
            T result = null;
            try
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data, new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects });
            }
            catch (Exception ex) { toLog(ex); return null; }
            return result;
        }

        public static Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get; set; } = new Newtonsoft.Json.JsonSerializerSettings
        {
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects,
            TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
        };

        private static string JsonSerializeObject(T model)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(
                    model,
                    Newtonsoft.Json.Formatting.None, 
                    JsonSerializerSettings);
        }
    }
}
