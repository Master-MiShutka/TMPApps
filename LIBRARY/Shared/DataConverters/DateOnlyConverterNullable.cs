namespace TMP.Shared.DataConverters
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class DateOnlyConverterNullable : JsonConverter<DateOnly?>
    {
        private const string DateFormat = "dd-MM-yyyy";

        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return null;

            return DateOnly.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value == null ? string.Empty : value.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
        }
    }
}
