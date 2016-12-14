using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    public class AGGS_Param : Param
    {
        public MS_Param MS { get; set; }
        public override string Type { get { return "AGGS_TYPE"; } }

        public string Value { get; set; }
        public string Per_Id { get; set; }
        public string Per_Code { get; set; }
        public string Per_Name { get; set; }

        public AGGS_Param()
            : base()
        {
            MS = new MS_Param();
        }
    }
}
