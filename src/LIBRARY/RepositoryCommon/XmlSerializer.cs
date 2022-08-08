namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml;

    public static class XmlSerializer
    {
        public static bool XmlSerialize(object model, string fileName, Action<Exception> toLog = null)
        {
            try
            {
                Type t = typeof(object);
                Type[] extraTypes = t.GetProperties()
                    .Where(p => p.PropertyType.IsInterface)
                    .Select(p => p.GetValue(model, null).GetType())
                    .ToArray();

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(object), extraTypes);
                using (TextWriter writer = new StreamWriter(fileName))
                {
                    serializer.Serialize(writer, model);
                }
            }
            catch (Exception e)
            {
                toLog?.Invoke(e);
                return false;
            }

            return true;
        }

        public static bool XmlDataContractSerialize(object model, string fileName, Type[] types, Action<Exception> toLog = null)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(object), types);
                XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, Encoding = new System.Text.UTF8Encoding(false, false) }; // no BOM in a .NET string
                using (System.IO.FileStream fs = System.IO.File.Open(fileName, System.IO.FileMode.Create))
                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(fs, settings))
                {
                    serializer.WriteObject(xmlWriter, model);
                }
            }
            catch (Exception e)
            {
                toLog?.Invoke(e);
                return false;
            }

            return true;
        }
    }
}
