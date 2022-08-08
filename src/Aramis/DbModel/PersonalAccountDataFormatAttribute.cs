namespace TMP.WORK.AramisChetchiki.DbModel
{
    using TMP.Shared.DataFormatters;

    public class PersonalAccountDataFormatAttribute : DataFormatAttribute
    {
        public override string ExportFormatString => "##\" \"####\" \"####\" \"#";

        public override string DataFormatString => "{0:##\" \"####\" \"####\" \"#}";
    }
}
