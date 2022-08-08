using System.Collections.Generic;

namespace TMP.ARMTES.Model
{
    public class MeterSubstitutionView
    {
        public List<MeterSubstitutionViewItem> MeterSubstitutionViewItems { get; set; }

        public MeterSubstitutionView()
        {
            MeterSubstitutionViewItems = new List<MeterSubstitutionViewItem>();
        }
    }
    public class MeterSubstitutionViewItem
    {
        public string DiscoverDate { get; set; }
        public string OldSerial { get; set; }
        public string NewSerial { get; set; }
    }
}
