namespace TMP.Work.Emcos.Model
{
    public class MS_Param : Param
    {
        public string Code { get; set; }
        public string Type_Id { get { return "1"; } }
        public string Type_Name { get { return "Электричество"; } }
        public override string Type { get { return "MS_TYPE"; } }
        public MS_Param()
            : base()
        {
            Name = "Электричество";
            Id = "1";
        }
    }
}
