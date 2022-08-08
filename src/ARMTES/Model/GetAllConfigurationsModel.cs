using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.ARMTES.Model
{
    public class ConfigurationContainer
    {
        public string ResName { get; set; }
        public string CollectorModel { get; set; }
        public string CollectorSoftwareVersion { get; set; }
        public string CollectorSerial { get; set; }
        public string CollectorNetworkAdress { get; set; }
        public string PhoneNumber { get; set; }
        public string ModemName { get; set; }
        public string ModemSerial { get; set; }

        public string RegionName { get; set; }
        public string VillageUnionName { get; set; }
        public string TransformationSubstationName { get; set; }
        public string ProjectName { get; set; }
        public string TechnicalConditionsName { get; set; }
        public string CollectorsPasswords { get; set; }
        public string MetersPasswords { get; set; }

        public List<MeterConfiguration> MeterConfigurations { get; set; }
    }

    public class MeterConfiguration
    {
        public int Number { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Flat { get; set; }
        public string MeterType { get; set; }
        public string MeterSerial { get; set; }
        public string MeterVirtualAdress { get; set; }
        public string MeterBelnoging { get; set; }
        public string InterfaceNumber { get; set; }
        public string MacCollectorModem { get; set; }
        public string MacMeterModem { get; set; }
        public string TransformationCoefficient { get; set; }
        public string SubscriberName { get; set; }
        public string PersonalAccount { get; set; }
        public string MeterNetworkAdress { get; set; }
    }
}
