using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TMP.ARMTES.Model
{
    public class SubscribersModel
    {
        public int TotalSubscribersCount { get; set; }
        public List<Subscriber> Subscribers { get; set; }
    }
    /// <summary>
    /// Потребитель
    /// </summary>
    [DataContract]
    public class Subscriber
    {
        [DataMember]
        public int SubscriberId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember(Name = "PersonalAccount")]
        public string PersonalAccountAsString { get; set; }
        [IgnoreDataMember]
        public ulong PersonalAccount
        {
            get {
                if (String.IsNullOrWhiteSpace(PersonalAccountAsString))
                    return 0;
                ulong value = 0;
                ulong.TryParse(PersonalAccountAsString, out value);
                return value;
            }
            set { PersonalAccountAsString = value.ToString(); }
        }
        [DataMember]
        public string SubscribersType { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string Street { get; set; }
        [DataMember]
        public string House { get; set; }
        [DataMember]
        public string Flat { get; set; }
        [DataMember]
        public string ContractNumber { get; set; }
    }
}