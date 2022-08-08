﻿namespace TMP.Shared.Common
{
    using System;

    public class SerializableStringDictionary : System.Collections.Specialized.StringDictionary, System.Xml.Serialization.IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            while (reader.Read() &&
                !(reader.NodeType == System.Xml.XmlNodeType.EndElement && reader.LocalName == this.GetType().Name))
            {
                var name = reader["Name"];
                if (name == null)
                    throw new FormatException();

                var value = reader["Value"];
                this[name] = value;
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (System.Collections.DictionaryEntry entry in this)
            {
                writer.WriteStartElement("Pair");
                writer.WriteAttributeString("Name", (string)entry.Key);
                writer.WriteAttributeString("Value", (string)entry.Value);
                writer.WriteEndElement();
            }
        }
    }
}
