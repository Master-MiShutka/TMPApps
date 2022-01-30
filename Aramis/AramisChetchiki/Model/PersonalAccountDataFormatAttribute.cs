namespace TMP.WORK.AramisChetchiki.Model
{
    using TMP.Shared;

    public class PersonalAccountDataFormatAttribute : DataFormatAttribute
    {
        public override string ExportFormatString => "##\" \"####\" \"####\" \"#";

        public override string DataFormatString => "{0:##\" \"####\" \"####\" \"#}";
    }
}
