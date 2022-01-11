namespace TMP.WORK.AramisChetchiki.Model
{
    using TMP.Shared;

    public class PersonalAccountDataFormat : DataFormatAttribute
    {
        public override string ExportFormatString => @"##' '####' '####' '#";

        public override string DataFormatString => "{0:##' '####' '####' '#}";
    }
}
