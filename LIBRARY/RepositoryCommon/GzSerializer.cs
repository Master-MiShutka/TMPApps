namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    public static class GzSerializer
    {
        public static bool GzXmlDataContractSerialize(object model, string fileName, Type[] types)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(object), types);
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, Encoding = new System.Text.UTF8Encoding(false, false) }; // no BOM in a .NET string
            using System.IO.FileStream fs = System.IO.File.Open(fileName, System.IO.FileMode.Create);
            using System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress, false);
            using System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(gzip, settings);
            serializer.WriteObject(xmlWriter, model);

            return true;
        }
    }
}
