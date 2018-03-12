using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TMP.ARMTES.Model
{
    [XmlRoot("Counter")]
    public class Counter : PropertyChangedBase
    {
        private bool _isAnswered;
        private int _tarifsCount;
        private string _accountingPoint;
        private string _counterType;
        private string _сounterNetworkAdress;
        private string _serialNumber;
        private Indications _previousIndications;
        private Indications _nextIndications;
        private long? _difference;
        private string _description;

        public string CounterLink { get; set; }

        public bool IsAnswered
        {
            get { return _isAnswered; }
            set { SetProperty<bool>(ref _isAnswered, value, "IsAnswered"); }
        }
        [XmlIgnore]
        public bool HasDifferenceBetweenT1AndT0
        {
            get
            {
                double d0 = DifferenceT0.HasValue ? DifferenceT0.Value : 0.0;
                double d1 = DifferenceT1.HasValue ? DifferenceT1.Value : 0.0;
                double d2 = DifferenceT2.HasValue ? DifferenceT2.Value : 0.0;
                double d3 = DifferenceT3.HasValue ? DifferenceT3.Value : 0.0;
                double d4 = DifferenceT4.HasValue ? DifferenceT4.Value : 0.0;

                return Math.Abs(d0 - (d1 + d2 + d3 + d4)) >= 0.01;
            }
        }
        public int TarifsCount
        {
            get
            {
                if (
                    (PreviousIndications.Tarriff4.HasValue && PreviousIndications.Tarriff4.Value > 0.01) ||
                    (NextIndications.Tarriff4.HasValue && NextIndications.Tarriff4.Value > 0.01))
                {
                    _tarifsCount = 4;
                    return _tarifsCount;
                }
                if (
                    (PreviousIndications.Tarriff3.HasValue && PreviousIndications.Tarriff3.Value > 0.01) ||
                    (NextIndications.Tarriff3.HasValue && NextIndications.Tarriff3.Value > 0.01))
                {
                    _tarifsCount = 3;
                    return _tarifsCount;
                }
                if (
                    (PreviousIndications.Tarriff2.HasValue && PreviousIndications.Tarriff2.Value > 0.01) ||
                    (NextIndications.Tarriff2.HasValue && NextIndications.Tarriff2.Value > 0.01))
                {
                    _tarifsCount = 2;
                    return _tarifsCount;
                }
                else
                    _tarifsCount = 1;
                return _tarifsCount;
            }
            set { SetProperty<int>(ref _tarifsCount, value, "TarifsCount"); }
        }
        public string AccountingPoint
        {
            get { return _accountingPoint; }
            set { SetProperty<string>(ref _accountingPoint, value, "AccountingPoint"); }
        }
        public string CounterType
        {
            get { return _counterType; }
            set { SetProperty<string>(ref _counterType, value, "CounterType"); }
        }
        public string CounterNetworkAdress
        {
            get { return _сounterNetworkAdress; }
            set { SetProperty<string>(ref _сounterNetworkAdress, value, "CounterNetworkAdress"); }
        }
        public string SerialNumber
        {
            get { return _serialNumber; }
            set { SetProperty<string>(ref _serialNumber, value, "SerialNumber"); }
        }

        public long? PreviousIndication { get; set; }

        public long? NextIndication { get; set; }

        public long? Difference { get; set; }
        public Indications PreviousIndications
        {
            get { return _previousIndications; }
            set { SetProperty<Indications>(ref _previousIndications, value, "PreviousIndications"); }
        }
        public Indications NextIndications
        {
            get { return _nextIndications; }
            set { SetProperty<Indications>(ref _nextIndications, value, "NextIndications"); }
        }

        [XmlIgnore]
        public bool HasMissingData
        {
            get
            {
                if (PreviousIndications == null || NextIndications == null) return true;
                return (PreviousIndications.HasMissingData || NextIndications.HasMissingData);
            }
        }
        public bool HasWrongSettings { get; set; }

        public bool MissingPersonalAccount { get; set; }

        [XmlIgnore]
        public double? DifferenceT0 { get { return GetDifference(PreviousIndications.Tarriff0, NextIndications.Tarriff0); } }
        [XmlIgnore]
        public double? DifferenceT1 { get { return GetDifference(PreviousIndications.Tarriff1, NextIndications.Tarriff1); } }
        [XmlIgnore]
        public double? DifferenceT2 { get { return GetDifference(PreviousIndications.Tarriff2, NextIndications.Tarriff2); } }
        [XmlIgnore]
        public double? DifferenceT3 { get { return GetDifference(PreviousIndications.Tarriff3, NextIndications.Tarriff3); } }
        [XmlIgnore]
        public double? DifferenceT4 { get { return GetDifference(PreviousIndications.Tarriff4, NextIndications.Tarriff4); } }

        public double? Differences(int index)
        {
               switch (index)
                {
                    case 0:
                    return DifferenceT0;
                    case 1:
                    return DifferenceT1;
                    case 2:
                    return DifferenceT2;
                    case 3:
                    return DifferenceT3;
                    case 4:
                    return DifferenceT4;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty<string>(ref _description, value, "Description"); }
        }
        public ViewCounterModel ViewModel { get; set; }

        public string Id { get; set; }
        public string AmperPointId { get; set; }

        public string ParentId { get; set; }

        public Counter()
        {
            PreviousIndications = new Indications();
            NextIndications = new Indications();
            ViewModel = new ViewCounterModel();
        }
        internal double? GetDifference(double? previousIndication, double? nextIndication)
        {
            bool hasvalue = previousIndication.HasValue && nextIndication.HasValue;
            if (hasvalue)
                return nextIndication.Value - previousIndication.Value;
            else
                return new Nullable<double>();
        }
    }
}
