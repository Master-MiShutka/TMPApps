using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace TMP.ARMTES.Model
{
    [XmlRoot("Collector")]
    public class Collector : PropertyChangedBase
    {
        private DateTime _creationDate;
        private bool _isAnswered = true;
        private bool _isAnsweredUSPD = true;
        private bool _isUSPD = false;
        private byte _networkAddress;
        private string _modemType;
        private string _phoneNumber;

        private List<AccountingObject> _objects;

        private string _description;
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { SetProperty<DateTime>(ref _creationDate, value, "CreationDate"); }
        }
        public bool IsAnswered
        {
            get { return _isAnswered; }
            set { SetProperty<bool>(ref _isAnswered, value, "IsAnswered"); }
        }
        public bool IsUSPD
        {
            get { return _isUSPD; }
            set { SetProperty<bool>(ref _isUSPD, value, "IsUSPD"); }
        }
        public bool IsAnsweredUSPD
        {
            get { return _isAnsweredUSPD; }
            set { SetProperty<bool>(ref _isAnsweredUSPD, value, "IsAnsweredUSPD"); }
        }
        public byte NetworkAddress
        {
            get { return _networkAddress; }
            set { SetProperty<byte>(ref _networkAddress, value, "NetworkAddress"); }
        }
        public string ModemType
        {
            get { return _modemType; }
            set { SetProperty<string>(ref _modemType, value, "ModemType"); }
        }
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { SetProperty<string>(ref _phoneNumber, value, "PhoneNumber"); }
        }       
        public List<AccountingObject> Objects
        {
            get { return _objects; }
            set { SetProperty<List<AccountingObject>>(ref _objects, value, "Objects"); }
        }
        public string Description
        {
            get { return _description; }
            set { SetProperty<string>(ref _description, value, "Description"); }
        }

        public string Id { get; set; }
        public string ParentId { get; set; }

        [XmlIgnore]
        public int CountersCount
        {
            get
            {
                return _objects == null ? -1 : _objects.Sum(o => o.CountersCount);
            }
        }

        [XmlIgnore]
        public bool HasMissingData
        {
            get
            {
                if (Objects == null) return false;
                foreach (var o in Objects)
                {
                    if (o.HasMissingData) return true;
                }
                return false;
            }
        }

        public Collector()
        {
            Objects = new List<AccountingObject>();
            CreationDate = new DateTime();
            IsAnswered = false;
            IsUSPD = false;
            NetworkAddress = 0;
            ModemType = Strings.ValueNotDefined;
            PhoneNumber = Strings.ValueNotDefined;            

            Description = Strings.ValueNotDefined;
        }
    }
}
