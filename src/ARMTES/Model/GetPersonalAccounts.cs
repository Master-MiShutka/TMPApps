using System;
using System.Runtime.Serialization;

namespace TMP.ARMTES.Model
{
    [DataContract]
    public class GetPersonalAccountsJsonImportingItem
    {
        [DataMember(Name = "PersonalAccount")]
        public string PersonalAccountAsString { get; set; }
        [IgnoreDataMember]
        public ulong PersonalAccount
        {
            get
            {
                if (String.IsNullOrWhiteSpace(PersonalAccountAsString))
                    return 0;
                ulong value = 0;
                ulong.TryParse(PersonalAccountAsString, out value);
                return value;
            }
            set { PersonalAccountAsString = value.ToString(); }
        }

        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string Street { get; set; }
        [DataMember]
        public string House { get; set; }
        [DataMember]
        public string Flat { get; set; }
        [DataMember]
        public string ResName { get; set; }
        [DataMember]
        public string SubscriberName { get; set; }
        [DataMember]
        public string ContractNumber { get; set; }
        [DataMember]
        public string AccountingPointContractNumber { get; set; }
    }
}
