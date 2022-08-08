using System.Collections.Generic;

namespace TMP.ARMTES.Model
{
    public class DeviceSessions
    {
        public List<DeviceSession> Items { get; set; }

        public DeviceSessions()
        {
            Items = new List<DeviceSession>();
        }
    }
    public class DeviceSession
    {
        public int DeviceScanningSessionId { get; set; }
        public int? RepeatCycleNumber { get; set; }
        public string TimeCorrectionResult { get; set; }
        public string Duration { get; set; }
        public string HandshakeTime { get; set; }
        public string DeviceScanningSessionResult { get; set; }
        public string SecondsDifference { get; set; }
    }
}
