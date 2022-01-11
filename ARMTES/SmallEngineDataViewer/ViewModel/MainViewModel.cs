using System;
using System.Data;
using System.Windows;
using System.ComponentModel;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;


namespace TMP.ARMTES.ViewModel
{
    using Model;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Input;

    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            GetData = new DelegateCommand(() =>
            {
                IsBusy = true;
                var task = Task.Factory.StartNew(Start);
                task.ContinueWith(t =>
                {
                    IsBusy = false;
                    Status = null;
                    DetailedStatus = null;
                });
                task.ContinueWith(t =>
                {
                    MessageBox.Show(App.GetExceptionDetails(t.Exception), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }, TaskContinuationOptions.OnlyOnFaulted);

            });

            GetData.Execute(null);
        }

        public void Start()
        {
            Status = "Получение данных";

            DateTime today = DateTime.Today;
            DateTime month = new DateTime(today.Year, today.Month, 1);
            DateTime prevMonth = month.AddMonths(-1);
            DateTime prevPrevMonth = month.AddMonths(-2);

            var enterprises = Webservice.GetEnterprises();
            var flat = enterprises.SelectMany(i => i.ChildEnterprises);
            var fes = flat.Where(i => i.EnterpriseName == "ОЭС");
            var reses = fes.SelectMany(i => i.ChildEnterprises);

            Enterprises = new List<EnterpriseViewItem>(reses);
            Enterprises.Insert(0, fes.First());

            /*var q1 = Webservice.GetSmallEngineObjects(prevMonth, prevPrevMonth, "ОшРЭС");
            var qqq = q1.Items[0].LastSession;

            var q2 = Webservice.GetMeterProblemInformations(OrderRule.ByLastUpdate);

            //var q3 = Webservice.GetIndicationsAnalyzing(prevMonth, prevPrevMonth, Profile.Days);

            var q4 = Webservice.Statistics(Profile.Months, 0, prevMonth, prevPrevMonth);

            var q5 = Webservice.SmallEngineStatistics(Profile.Months, 0, prevMonth, prevPrevMonth);

            var q6 = Webservice.GetCollectorDevicesReport();

            var q7 = Webservice.GetChildElementsInHouseHoldSector("e19");


            var t = Webservice.GetCountersGroupInformations("ОшРЭС");*/

            DetailedStatus = "получение информации по абонентам ...";
            IEnumerable<Subscriber> subscribers = Webservice.GetSubscribers();

            var groupedSubscribers = subscribers.GroupBy(i => i.SubscribersType).Select(i => new { Key = i.Key, Count = i.Count(), Items = i.ToList() });

            var sesSubscribers = groupedSubscribers.Where(i => i.Key == "");

            DetailedStatus = "получение расходов ...";
            List<AllTariffsExportIndicationViewItem> indications = Webservice.GetSmallEngineExportIndications(prevMonth);

            DetailedStatus = "получение информации по абонентам ...";
            IEnumerable<SmallEngineBillingObject> objects = Webservice.GetSmallEngineBillingObjects();

            var groupedObjects = objects.GroupBy(i => i.ResName).Select(i => new { Key = i.Key, Count = i.Count(), Items = i.ToList() });

            var emp = objects.Where(i => i.BillingPointPersonalAccount != 0);

            DetailedStatus = "подготовка списка ...";
            List<SmallEngineDataModel> list = new List<SmallEngineDataModel>();
            if (indications != null)
            {
                SmallEngineDataModel dataItem;

                List<AllTariffsExportIndicationViewItem> notFoundedAccounts = new List<AllTariffsExportIndicationViewItem>();
                foreach (AllTariffsExportIndicationViewItem item in indications)
                {
                    if (item.PersonalAccount != 0)
                    {
                        //SmallEngineBillingObject obj1 = Webservice.GetPersonalAccountInfoFromAmper(item.PersonalAccount);

                        SmallEngineBillingObject obj = objects.FirstOrDefault(i => i.BillingPointPersonalAccount == item.PersonalAccount);
                        if (obj != null)
                        {
                            dataItem = new SmallEngineDataModel(obj);
                            dataItem.SetIndications(item);
                            list.Add(dataItem);
                        }
                        else
                            notFoundedAccounts.Add(item);
                    }
                    else
                    {
                        dataItem = new SmallEngineDataModel() { BillingPointPersonalAccount = item.PersonalAccount };
                        dataItem.SetIndications(item);
                        list.Add(dataItem);
                    }
                }

                List<SmallEngineDataModel> found = new List<SmallEngineDataModel>();
                List<SmallEngineDataModel> notfound = new List<SmallEngineDataModel>();
                foreach (var item in list)
                    if (reses.Any(res => res.EnterpriseName == item.ResName))
                        found.Add(item);
                    else
                        notfound.Add(item);

                View = CollectionViewSource.GetDefaultView(found);
            }
        }

        private bool ViewFilter(object item)
        {
            if (SelectedEnterprise == null || SelectedEnterprise.EnterpriseType == "ФЭС")
                return true;

            SmallEngineDataModel data = item as SmallEngineDataModel;
            if (null == data)
                return true;

            return data.ResName == SelectedEnterprise.EnterpriseName;

        }

        #region Private methods

        private void OnSelectedProfileChanged()
        {
            DateTime now = DateTime.Now;
            switch (SelectedProfile)
            {
                case Profile.Current:
                    _fromDate = now;
                    _toDate = now;
                    break;
                case Profile.Days:
                    _fromDate = new DateTime(now.Year, now.Month, 1);
                    _toDate = now;
                    break;
                case Profile.Months:
                    {
                        if (now.Month == 1)
                            _fromDate = new DateTime(now.Year - 1, 12, 1);
                        else
                            _fromDate = new DateTime(now.Year, now.Month - 1, 1);
                        _toDate = new DateTime(now.Year, now.Month, 1);
                        break;
                    }
            }
            RaisePropertyChanged("FromDate");
            RaisePropertyChanged("ToDate");
        }

        #endregion

        #region Properties

        private ICollectionView _view;
        public ICollectionView View
        {
            get { return _view; }
            private set
            {
                _view = value;
                _view.Filter = ViewFilter;

                _view.SortDescriptions.Add(new SortDescription("ContractNumber", ListSortDirection.Ascending));
                _view.SortDescriptions.Add(new SortDescription("CityName", ListSortDirection.Ascending));
                _view.SortDescriptions.Add(new SortDescription("ObjectName", ListSortDirection.Ascending));
                _view.SortDescriptions.Add(new SortDescription("AccountingPointName", ListSortDirection.Ascending));

                RaisePropertyChanged("View");
            }
        }

        private SmallEngineDataModel _selectedData = null;
        public SmallEngineDataModel SelectedData
        {
            get { return _selectedData; }
            set
            {
                _selectedData = value;
                RaisePropertyChanged("SelectedData");
                //SelectedDataTableIndications = Webservice.GetIndicatiosInTableView(SelectedData.BillingPointPersonalAccount, SelectedProfile, FromDate, ToDate, "SES");
            }
        }

        private TableIndications _selectedDataTableIndications;

        public TableIndications SelectedDataTableIndications
        {
            get { return _selectedDataTableIndications; }
            set { _selectedDataTableIndications = value; RaisePropertyChanged("SelectedDataTableIndications"); }
        }


        private DateTime _fromDate;
        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; RaisePropertyChanged("FromDate"); }
        }
        private DateTime _toDate;
        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; RaisePropertyChanged("ToDate"); }
        }
        private Profile _profile;

        public Profile SelectedProfile
        {
            get { return _profile; }
            set { _profile = value; RaisePropertyChanged("SelectedProfile"); OnSelectedProfileChanged(); }
        }


        private IList<EnterpriseViewItem> _enterprises;
        public IList<EnterpriseViewItem> Enterprises
        {
            get { return _enterprises; }
            private set { _enterprises = value; RaisePropertyChanged("Enterprises"); }
        }

        private EnterpriseViewItem _selectedEnterprise;
        public EnterpriseViewItem SelectedEnterprise
        {
            get { return _selectedEnterprise; }
            set { _selectedEnterprise = value; RaisePropertyChanged("SelectedEnterprise"); if (View != null) View.Refresh(); }
        }


        public ICommand GetData { get; }
        public ICommand Update { get; }
        public ICommand Save { get; }
        public ICommand Print { get; }


        private string _status = null;
        public String Status
        {
            get { return _status; }
            private set { _status = value; RaisePropertyChanged("Status"); }
        }
        private string _detailedStatus = null;
        public String DetailedStatus
        {
            get { return _detailedStatus; }
            private set { _detailedStatus = value; RaisePropertyChanged("DetailedStatus"); }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { _isBusy = value; RaisePropertyChanged("IsBusy"); }
        }

        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
