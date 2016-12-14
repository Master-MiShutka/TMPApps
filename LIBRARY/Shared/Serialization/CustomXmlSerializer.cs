using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;

namespace TMP.Shared.Serialization
{
    public interface IXmlDeserializationCallback
    {
        void OnXmlDeserialization(object sender);
    }
    public class CustomXmlSerializer : XmlSerializer
    {
        public CustomXmlSerializer(Type type) : base(type) { }
        protected override object Deserialize(XmlSerializationReader reader)
        {
            var result = base.Deserialize(reader);

            var deserializedCallback = result as IXmlDeserializationCallback;
            if (deserializedCallback != null)
            {
                deserializedCallback.OnXmlDeserialization(this);
            }

            return result;
        }
    }
}
