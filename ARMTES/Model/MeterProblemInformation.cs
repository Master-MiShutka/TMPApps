using System.Collections.Generic;

namespace TMP.ARMTES.Model
{
    public class MeterProblemInformation
    {
        public List<MeterProblemInformationViewItem> MeterProblemInformationViewItems { get; set; }

        public MeterProblemInformation()
        {
            MeterProblemInformationViewItems = new List<MeterProblemInformationViewItem>();
        }
    }
    public class MeterProblemInformationViewItem
    {
        public int DeviceId { get; set; }
        public string ResName { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Flat { get; set; }
        public string MeterRepairmentStatus { get; set; }
        public string MeterRepairmentImage { get; set; }
        public string RepairmentComments { get; set; }
        public string LastUserWhoModifiedStatus { get; set; }
        public string LastStatusChange { get; set; }
    }
}
