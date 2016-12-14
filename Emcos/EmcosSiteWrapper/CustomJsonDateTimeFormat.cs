namespace TMP.Work.Emcos
{
    public class CustomJsonDateTimeFormat : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public CustomJsonDateTimeFormat(string format)
        {
            DateTimeFormat = format;
        }
    }
}