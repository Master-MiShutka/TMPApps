using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace TMP.ARMTES.Model
{
    [DataContract]
    public class SmallEngineDataModel : SmallEngineBillingObject
    {
        [DataMember]
        public AllTariffsExportIndicationViewItem Indications { get; set; }

        [DataMember]
        public string CounterType { get; set; }
        [DataMember]
        public string CounterSerialNumber { get; set; }

        public void SetIndications(AllTariffsExportIndicationViewItem indications)
        {
            this.Indications = new AllTariffsExportIndicationViewItem(indications);
        }

        public SmallEngineDataModel() { }

        public SmallEngineDataModel(SmallEngineBillingObject source)
        {
            this.BillingPointId = source.BillingPointId;
            this.BillingPointPersonalAccount = source.BillingPointPersonalAccount;
            this.SubscriberName = source.SubscriberName;
            this.ContractNumber = source.ContractNumber;
            this.AccountingPointContractNumber = source.AccountingPointContractNumber;
            this.ObjectName = source.ObjectName;
            this.AccountingPointName = source.AccountingPointName;
            this.ResName = source.ResName;
            this.FiderName = source.FiderName;
            this.SubscriberAdress = source.SubscriberAdress;
            this.CityName = source.CityName;
            this.SubscriberFullName = source.SubscriberFullName;
            this.SubscriberShortName = source.SubscriberShortName;
            this.TransformationSubStation = source.TransformationSubStation;
            this.SubstationName = source.SubstationName;
            this.ObjectAdress = source.ObjectAdress;
            this.ParentPersonalAccount = source.ParentPersonalAccount;
            this.SerialNumberFromBilling = source.SerialNumberFromBilling;
            this.ObjectTown = source.ObjectTown;
        }

        public SmallEngineDataModel(AmperGetPointInfo source)
        {
            this.AccountingPointName = source.Point.Name;
            this.BillingPointId = source.Point.Id;
            this.BillingPointPersonalAccount = source.Point.Id;
            this.ResName = source.Object.Res;
            this.SubscriberName = source.Dogovor.Subscriber.Name;
            this.SubscriberFullName = source.Dogovor.Subscriber.FullName;
            this.SubscriberShortName = source.Dogovor.Subscriber.ShortName;
            this.SubscriberAdress = source.Dogovor.Subscriber.Adress;
            this.ContractNumber = source.Dogovor.ContractNumber;
            this.TransformationSubStation = source.Point.TransformerSubstation;
            this.FiderName = source.Point.Fider;
            this.SubstationName = source.Point.Substation;
            this.ObjectName = source.Object.Name;
            this.ObjectAdress = source.Object.Adress;
            this.ParentPersonalAccount = source.Point.ParentId;
            this.SerialNumberFromBilling = source.Point.Counter.SerialNumber;
            this.ObjectTown = source.Object.Town;

            this.CounterSerialNumber = source.Point.Counter.SerialNumber;
            this.CounterType = source.Point.Counter.Type.Name;
        }
    }
}
