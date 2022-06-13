namespace TMP.Shared.DataFormatters
{
    using System;
    using MessagePack;
    using MessagePack.Formatters;

    public class DateOnlyFormatter : IMessagePackFormatter<DateOnly>
    {
        private const string DateFormat = "dd-MM-yyyy";
        private const int DateFormatLength = 10;

        public DateOnly Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return default;
            }
            string date = reader.ReadString();
            return DateOnly.Parse(date);
        }

        public void Serialize(ref MessagePackWriter writer, DateOnly value, MessagePackSerializerOptions options)
        {
            if (value == default)
            {
                writer.WriteNil();
                return;
            }

            string date = value.ToString(DateFormat);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(date);
            writer.WriteString(bytes);
        }
    }
}
