namespace TMP.Work.Emcos.Model
{
    public class MSF_Param : Param
    {
        public AGGS_Param AGGS { get; set; }
        public MS_Param MS { get; set; }

        public string Code { get; set; }
        public override string Type { get { return "MSF"; } }
        public MSF_Param()
            : base()
        {
            AGGS = new AGGS_Param();
            MS = new MS_Param();
        }
    }
}
