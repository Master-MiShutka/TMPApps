namespace TMP.Shared.Common.DataFormatters
{
    public class DataFormatAttribute : System.Attribute
    {
        public static readonly string TextFormat = "@";

        /// <summary>
        /// Gets or sets the format string
        /// </summary>
        public virtual string ExportFormatString { get; set; }

        public virtual string DataFormatString { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataFormatAttribute()
        {
        }
    }

    public class TextDataFormatAttribute : DataFormatAttribute
    {
        public override string ExportFormatString => TextFormat;
    }

    public class NumericDataFormatAttribute : DataFormatAttribute
    {
        public override string ExportFormatString => @"0;[Blue]-0;;[Red]@";

        public override string DataFormatString => "{0:N0}";
    }

    public class NumericWithSeparatorDataFormatAttribute : DataFormatAttribute
    {
        public override string ExportFormatString => @"# ##0_ ;[Red]-# ##0\ ;;[Red]@";

        public override string DataFormatString => "{0:N0}";
    }

    public class DateTimeDataFormatAttribute : DataFormatAttribute
    {
        public override string ExportFormatString => @"[$-ru-BY-x-genlower]dddd, d mmmm yyyy";

        public override string DataFormatString => "{0:dd.MM.yy}";
    }
}
