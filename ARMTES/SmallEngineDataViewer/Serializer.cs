using System;
using System.Runtime.Serialization.Json;

namespace TMP.ARMTES
{
    using Model;
    internal static class Serializer
    {
        public static long DataSize { get; private set; }

        public static bool GzJsonSerialize(Data model, string fileName, Action<Exception> toLog = null)
        {
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress))
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(Data));
                    serializer.WriteObject(ms, model);

                    DataSize = ms.Length;
                    ms.Position = 0;
                    ms.WriteTo(gz);
                }
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                else throw e;
                return false;
            }
            return true;
        }
        public static bool JsonSerialize(Data model, string fileName, Action<Exception> toLog = null)
        {
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    var serializer = new DataContractJsonSerializer(typeof(Data));
                    serializer.WriteObject(fs, model);
                }
            }
            catch (Exception e)
            {
                if (toLog != null) toLog(e);
                else throw e;
                return false;
            }
            return true;
        }
        public static Data GzJsonDeSerialize(string fileName, Action<Exception> toLog = null)
        {
            if (System.IO.File.Exists(fileName) == false) return null;
            Data result = null;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
                using (System.IO.MemoryStream mso = new System.IO.MemoryStream())
                {
                    gz.CopyTo(mso);
                    DataSize = mso.Length;
                    mso.Position = 0;
                    var serializer = new DataContractJsonSerializer(typeof(Data));
                    result = (Data)serializer.ReadObject(mso);
                }
            }
            catch (Exception ex) { if (toLog != null) toLog(ex); else throw ex; return null; }
            return result;
        }
        public static Data JsonDeSerialize(string fileName, Action<Exception> toLog = null)
        {
            if (System.IO.File.Exists(fileName) == false) return null;
            Data result = null;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                using (System.IO.MemoryStream mso = new System.IO.MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(Data));
                    result = (Data)serializer.ReadObject(mso);
                }
            }
            catch (Exception ex) { toLog(ex); return null; }
            return result;
        }
    }
}
