using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TMP.ARMTES.Model
{
    [XmlRoot("AccountingObject")]
    public class AccountingObject : PropertyChangedBase
    {
        private string _city;
        private string _contract;
        private string _subscriber;
        private string _name;
        private string _tp;
        private List<Counter> _counters;
        private string _description;

        /// <summary>
        /// Адрес объекта
        /// </summary>
        public string City
        {
            get { return _city; }
            set { SetProperty<string>(ref _city, value, "City"); }
        }
        /// <summary>
        /// № договора
        /// </summary>
        public string Contract
        {
            get { return _contract; }
            set { SetProperty<string>(ref _contract, value, "Contract"); }
        }
        /// <summary>
        /// Наименование абонента
        /// </summary>
        public string Subscriber
        {
            get { return _subscriber; }
            set { SetProperty<string>(ref _subscriber, value, "Subscriber"); }
        }
        /// <summary>
        /// Объект учета
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty<string>(ref _name, value, "Name"); }
        }
        /// <summary>
        /// Номер ТП
        /// </summary>
        public string Tp
        {
            get { return _tp; }
            set { SetProperty<string>(ref _tp, value, "Tp"); }
        }
        public string TpLink { get; set; }

        public List<Counter> Counters
        {
            get { return _counters; }
            set { SetProperty<List<Counter>>(ref _counters, value, "Counters"); }
        }
        public string Description
        {
            get { return _description; }
            set { SetProperty<string>(ref _description, value, "Description"); }
        }

        [XmlIgnore]
        public int CountersCount
        {
            get
            {
                return _counters == null ? -1 : _counters.Count;
            }
        }
        public string Id { get; set; }
        [XmlIgnore]
        public bool HasMissingData
        {
            get
            {
                if (Counters == null) return false;
                foreach (var counter in Counters)
                {
                    if (counter.HasMissingData) return true;
                }
                return false;
            }
        }
        public ViewDeviceModel ViewModel { get; set; }
        public AccountingObject()
        {
            _city = Strings.ValueNotDefined;
            _contract = Strings.ValueNotDefined;
            _subscriber = Strings.ValueNotDefined;
            _name = Strings.ValueNotDefined;
            _tp = Strings.ValueNotDefined;

            _counters = new List<Counter>();

            ViewModel = new ViewDeviceModel();
        }
    }
}
