using System;
using System.Runtime.Serialization;

namespace TMP.ARMTES.Model
{
    [DataContract]
    public class ExportIndicationViewItem
    {
        [DataMember]
        public string PersonalAccount { get; set; }
        [DataMember]
        public string TimeStamp { get; set; }
        [DataMember]
        public double? PreviousValue { get; set; }
        [DataMember]
        public double? NextValue { get; set; }
        [DataMember]
        public double? Difference { get; set; }
        [DataMember]
        public string ErrorCode { get; set; }
    }

    [DataContract]
    public class AllTariffsExportIndicationViewItem
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
        public string TimeStamp { get; set; }
        [DataMember]
        public double? PreviousValue { get; set; }
        [DataMember]
        public double? NextValue { get; set; }
        [DataMember]
        public double? Difference { get; set; }
        [DataMember]
        public string ErrorCode { get; set; }
        [DataMember]
        public double? PreviousTariff1Value { get; set; }
        [DataMember]
        public double? NextTariff1Value { get; set; }
        [DataMember]
        public double? Tariff1Difference { get; set; }
        [DataMember]
        public double? PreviousTariff2Value { get; set; }
        [DataMember]
        public double? NextTariff2Value { get; set; }
        [DataMember]
        public double? Tariff2Difference { get; set; }
        [DataMember]    
        public double? PreviousTariff3Value { get; set; }
        [DataMember]
        public double? NextTariff3Value { get; set; }
        [DataMember]
        public double? Tariff3Difference { get; set; }
        [DataMember]
        public double? PreviousTariff4Value { get; set; }
        [DataMember]
        public double? NextTariff4Value { get; set; }
        [DataMember]
        public double? Tariff4Difference { get; set; }

        public AllTariffsExportIndicationViewItem() { }

        public AllTariffsExportIndicationViewItem(AllTariffsExportIndicationViewItem indication)
        {
            this.PersonalAccount = indication.PersonalAccount;
            this.TimeStamp = indication.TimeStamp;
            this.PreviousValue = indication.PreviousValue;
            this.NextValue = indication.NextValue;
            this.Difference = indication.Difference;
            this.ErrorCode = indication.ErrorCode;

            this.PreviousTariff1Value = indication.PreviousTariff1Value;
            this.NextTariff1Value = indication.NextTariff1Value;
            this.Tariff1Difference = indication.Tariff1Difference;

            this.PreviousTariff2Value = indication.PreviousTariff2Value;
            this.NextTariff2Value = indication.NextTariff2Value;
            this.Tariff2Difference = indication.Tariff2Difference;

            this.PreviousTariff3Value = indication.PreviousTariff3Value;
            this.NextTariff3Value = indication.NextTariff3Value;
            this.Tariff3Difference = indication.Tariff3Difference;

            this.PreviousTariff4Value = indication.PreviousTariff4Value;
            this.NextTariff4Value = indication.NextTariff4Value;
            this.Tariff4Difference = indication.Tariff4Difference;
        }
    }
    [DataContract]
    public class AllExportIndicationViewItem
    {
        [DataMember]
        public string PersonalAccount { get; set; }
        [DataMember]
        public string TimeStamp { get; set; }
        [DataMember]
        public double? PreviousValue { get; set; }
        [DataMember]
        public double? NextValue { get; set; }
        [DataMember]
        public double? Difference { get; set; }
        [DataMember]
        public string ErrorCode { get; set; }
        [DataMember]
        public int? Validity { get; set; }
    }
}
