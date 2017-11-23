using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TMP.ARMTES.Model
{
    [DataContract]
    public class SmallEngineBillingObject
    {
        [DataMember(Name = "BillingPointId")]
        public string BillingPointIdAsString { get; set; }
        [IgnoreDataMember]
        public ulong BillingPointId
        {
            get
            {
                if (String.IsNullOrWhiteSpace(BillingPointIdAsString))
                    return 0;
                ulong value = 0;
                ulong.TryParse(BillingPointIdAsString, out value);
                return value;
            }
            set { BillingPointIdAsString = value.ToString(); }
        }

        [DataMember(Name = "BillingPointPersonalAccount")]
        public string BillingPointPersonalAccountAsString { get; set; }
        [IgnoreDataMember]
        public ulong BillingPointPersonalAccount
        {
            get
            {
                if (String.IsNullOrWhiteSpace(BillingPointPersonalAccountAsString))
                    return 0;
                ulong value = 0;
                ulong.TryParse(BillingPointPersonalAccountAsString, out value);
                return value;
            }
            set { BillingPointPersonalAccountAsString = value.ToString(); }
        }

        [DataMember]
        public string SubscriberName { get; set; }
        [DataMember]
        public string ContractNumber { get; set; }
        [DataMember]
        public string AccountingPointContractNumber { get; set; }
        [DataMember]
        public string ObjectName { get; set; }
        [DataMember]
        public string AccountingPointName { get; set; }
        [DataMember]
        public string ResName { get; set; }
        [DataMember]
        public string FiderName { get; set; }
        [DataMember]
        public string SubscriberAdress { get; set; }
        [DataMember]
        public string CityName { get; set; }
        [DataMember]
        public string SubscriberFullName { get; set; }
        [DataMember]
        public string SubscriberShortName { get; set; }
        [DataMember]
        public string TransformationSubStation { get; set; }
        [DataMember]
        public string SubstationName { get; set; }
        [DataMember]
        public string ObjectAdress { get; set; }
        [DataMember]
        public string ParentPersonalAccount { get; set; }
        [DataMember]
        public string SerialNumberFromBilling { get; set; }
        [DataMember]
        public string ObjectTown { get; set; }
    }
}
