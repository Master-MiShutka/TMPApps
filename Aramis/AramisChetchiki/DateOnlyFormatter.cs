using MessagePack;
using MessagePack.Formatters;
using System;
using System.Buffers;
using System.Text;

namespace TMP.WORK.AramisChetchiki
{
    public class DateOnlyFormatter : IMessagePackFormatter<DateOnly>
    {
        public DateOnly Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var msg = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);

            return DateOnly.Parse(msg);
        }

        public void Serialize(ref MessagePackWriter writer, DateOnly value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            if (value == DateOnly.MinValue)
            {
                writer.WriteNil();
                return;
            }

            options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ToString("dd.MM.yy"), options);
        }
    }
}
