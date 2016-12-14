using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    /// <summary>
    /// Параметр - направление измерения
    /// </summary>
    public class ML_Param : Param
    {
        public override string Type { get { return "ML"; } }

        public AGGF_Param AGGF { get; set; }
        public AGGS_Param AGGS { get; set; }
        public DIR_Param DIR { get; set; }
        public EU_Param EU { get; set; }
        public TFF_Param TFF { get; set; }
        public MS_Param MS { get; set; }
        public MD_Param MD { get; set; }
        public MSF_Param MSF { get; set; }
        public string OBIS { get; set; }
        public ML_Param()
            : base()
        {
            AGGF = new AGGF_Param();
            AGGS = new AGGS_Param();
            DIR = new DIR_Param();
            EU = new EU_Param();
            TFF = new TFF_Param();
            MS = new MS_Param();
            MD = new MD_Param();
            MSF = new MSF_Param();
        }
        public override bool HasChildren { get {return false;} }
    }
}
