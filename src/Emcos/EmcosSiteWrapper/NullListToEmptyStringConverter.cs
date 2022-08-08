using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace TMP.Work.Emcos
{
    public class NullListToEmptyStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IList<double?>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize(reader, objectType);
            }
            if (reader.TokenType == JsonToken.String)
            {
                var value = serializer.Deserialize<string>(reader);
                var list = new List<double?>();
                if (value.StartsWith("<") && value.EndsWith(">"))
                {
                    int count = Convert.ToInt32(value.Substring(1, value.Length - 2));
                    for (int i = 0; i < count; i++)
                        list.Add(0d);
                }
                return list;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();

            var list = value as IList<double?>;
            if (list != null && list.All(i => i.HasValue && i.Value == 0d))
                writer.WriteValue("<" + list.Count + ">");
            else
                if (list != null && list.All(i => i.HasValue == false))
                writer.WriteNull();
            else
                serializer.Serialize(writer, value);
        }
    }
}
