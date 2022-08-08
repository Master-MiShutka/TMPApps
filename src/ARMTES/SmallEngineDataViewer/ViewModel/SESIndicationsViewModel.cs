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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace TMP.ARMTES.ViewModel
{
    using Model;

    public class SESIndicationsViewModel : BaseViewModel, IMainViewModel
    {
        private readonly PropertyGroupDescription _propertyGroupDescriptionRes = new PropertyGroupDescription("ResName");

        Window _mainWindow;

        public SESIndicationsViewModel()
        {

        }
        public SESIndicationsViewModel(Window ownerWindow)
        {
            _mainWindow = ownerWindow;
            if (_mainWindow != null)
                _mainWindow.Loaded += (s, e) =>
                {
                    //LoadData();
                };


            GetData = new DelegateCommand(() =>
            {
                if (IsServiceAvailable == false)
                {
                    MessageBox.Show("Web-сервис АРМТЕС не доступен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var task = Task.Factory.StartNew(Start);
                task.ContinueWith(t =>
                {
                    watch.Stop();
                    System.Diagnostics.Trace.TraceInformation("GetData -> {0} ms", watch.ElapsedMilliseconds);

                    IsBusy = false;
                    Status = null;
                    DetailedStatus = null;
                });
                task.ContinueWith(t =>
                {
                    MessageBox.Show(App.GetExceptionDetails(t.Exception), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }, TaskContinuationOptions.OnlyOnFaulted);

            });

            Save = new DelegateCommand<Xceed.Wpf.DataGrid.DataGridControl>((o) =>
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                Xceed.Wpf.DataGrid.DataGridControl grid = o as Xceed.Wpf.DataGrid.DataGridControl;

                

                ExcelLibrary.SpreadSheet.Workbook workbook = new ExcelLibrary.SpreadSheet.Workbook();
                ExcelLibrary.SpreadSheet.Worksheet worksheet = new ExcelLibrary.SpreadSheet.Worksheet("First Sheet");
                worksheet.Cells[0, 1] = new ExcelLibrary.SpreadSheet.Cell((short)1);
                worksheet.Cells[2, 0] = new ExcelLibrary.SpreadSheet.Cell(9999999);
                worksheet.Cells[3, 3] = new ExcelLibrary.SpreadSheet.Cell((decimal)3.45);
                worksheet.Cells[2, 2] = new ExcelLibrary.SpreadSheet.Cell("Text string");
                worksheet.Cells[2, 4] = new ExcelLibrary.SpreadSheet.Cell("Second string");
                worksheet.Cells[4, 0] = new ExcelLibrary.SpreadSheet.Cell(32764.5, "#,##0.00");
                worksheet.Cells[5, 1] = new ExcelLibrary.SpreadSheet.Cell(DateTime.Now, @"YYYY-MM-DD");
                worksheet.Cells.ColumnWidth[0, 1] = 3000;

                string executionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string fileName = System.IO.Path.Combine(executionPath, "export.xls");

                workbook.Worksheets.Add(worksheet); workbook.Save(fileName);

                watch.Stop();
                System.Diagnostics.Trace.TraceInformation("Save -> {0} ms", watch.ElapsedMilliseconds);

                var p = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(fileName)
                    {
                        UseShellExecute = true
                    }
                };
                p.Start();

                /*if (grid != null)
                    Xceed.Wpf.DataGrid.Export.*/
            });

            Print = new DelegateCommand(() =>
            {

            }, () => true, "Печать");

            Update = new DelegateCommand(() =>
            {

            }, () => Items != null, "Обновить");

        }

        public void Init()
        {
            if (Configuration.Instance.Settings != null)
            {
                FromDate = Configuration.Instance.Settings.FromDate;
                ToDate = Configuration.Instance.Settings.ToDate;
                Items = Configuration.Instance.Settings.Items;
                SelectedProfile = Configuration.Instance.Settings.Profile;

                List<SmallEngineExportViewItemWithCounter> counters = new List<SmallEngineExportViewItemWithCounter>();
                if (Configuration.Instance.Settings.SEO != null)
                    foreach (SmallEngineExportViewItem item in Configuration.Instance.Settings.SEO)
                        foreach (SmallEngineCounterViewItem counter in item.Counters)
                            counters.Add(new SmallEngineExportViewItemWithCounter()
                            {
                                Status = item.Status,
                                Name = item.Name,
                                DialNumber = item.DialNumber,
                                LastSession = item.LastSession,
                                CounterName = counter.Name,
                                SerialNumber = counter.SerialNumber,
                                NetworkAdress = counter.NetworkAdress,
                                CounterStatus = counter.Status,
                                PreviousIndications = counter.PreviousIndications,
                                NextIndications = counter.NextIndications,
                                IndicationsDifference = counter.IndicationsDifference,
                                PreviousIndicationsDifference = counter.PreviousIndicationsDifference
                            });

                SEO = new ListCollectionView(counters);
                SEO.GroupDescriptions.Add(new PropertyGroupDescription("Status"));
                SEO.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                RaisePropertyChanged("SEO");
            }
        }

        public void Start()
        {
            Status = "Получение данных";

            DateTime today = DateTime.Today;
            DateTime month = new DateTime(today.Year, today.Month, 1);
            DateTime prevMonth = month.AddMonths(-1);
            DateTime prevPrevMonth = month.AddMonths(-2);

            IList<EnterpriseFromAmper> enterpriseFromAmper = Webservice.GetEnterprisesFromAmper("ОЭС");
            var resesNamesFromAmper = enterpriseFromAmper.Select(i => i.FullName);


            //var indicationsAnalizing = Webservice.GetIndicationsAnalyzing(prevPrevMonth, prevMonth, Profile.Months);

            var problemMeters = Webservice.GetMeterProblemInformations(OrderRule.ByLastUpdate);
            if (problemMeters != null && problemMeters.Count > 0)
            {
                ProblemMetersList = problemMeters
                    .Where(i => Configuration.Instance.ResNames.Contains(i.ResName))
                    .OrderBy(i => i.ResName)
                    .OrderBy(i => i.City)
                    .OrderBy(i => i.Street)
                    .ToList();
                RaisePropertyChanged("ProblemMetersList");
            }

            List<SmallEngineExportViewItem> sesObjects = new List<SmallEngineExportViewItem>();
            foreach (string res in Configuration.Instance.ResNames)
            {
                var data = Webservice.GetSmallEngineObjects(prevPrevMonth, prevMonth, res);
                if (data != null && data.Count > 0)
                    sesObjects.AddRange(data.Items);
            }

            List<ConfigurationContainer> configs = new List<ConfigurationContainer>();
            foreach (EnterpriseViewItem res in Configuration.Instance.Reses)
            {
                var data = Webservice.GetAllConfigurationInformation(res.EnterpriseId, res.EnterpriseName);
                if (data != null)
                    configs.AddRange(data);
            }


            /*var qqq = q1.Items[0].LastSession;


            //var q3 = Webservice.GetIndicationsAnalyzing(prevPrevMonth, prevMonth, Profile.Days);

            //var q4 = Webservice.Statistics(Profile.Months, 0, prevPrevMonth, prevMonth);

            //var q5 = Webservice.SmallEngineStatistics(Profile.Months, 0, prevMonth, prevPrevMonth);
            // список модемов и УСПД
            var q6 = Webservice.GetCollectorDevicesReport();

            var q7 = Webservice.GetChildElementsInHouseHoldSector("421990");

            //var t = Webservice.GetCountersGroupInformations("ОшРЭС");*/

            // !! 
            //DetailedStatus = "получение информации по абонентам ...";
            //IEnumerable<Subscriber> subscribers = Webservice.GetSubscribers();
            //var groupedSubscribers = subscribers.GroupBy(i => i.SubscribersType).Select(i => new { Key = i.Key, Count = i.Count(), Items = i.ToList() });
            //var sesSubscribers = groupedSubscribers.Where(i => i.Key == "");

            DetailedStatus = "получение расходов ...";
            // записи вида PersonalAccount, TimeStamp и показания и расход по тарифам
            //TODO: Fix prevMonth
            List<AllTariffsExportIndicationViewItem> indications = Webservice.GetSmallEngineExportIndications(prevPrevMonth);
            //List<AllTariffsExportIndicationViewItem> indications1 = Webservice.GetSmallEngineExportIndications(prevPrevMonth, resesNames[0]);

            DetailedStatus = "получение информации по абонентам ...";
            // Получение списка потребителей с полями как в Ампер-М
            //IEnumerable<SmallEngineBillingObject> objects = Webservice.GetSmallEngineBillingObjects();

            //IEnumerable<SmallEngineBillingObject> objects1 = Webservice.GetSmallEngineBillingObjects(resesNames[0]);
            // группировка по РЭС
            //var groupedObjects = objects.GroupBy(i => i.ResName).Select(i => new { Key = i.Key, Count = i.Count(), Items = i.ToList() });
            // отбор с не пустым ИД
            //var emp = objects.Where(i => i.BillingPointPersonalAccount != 0);

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
                        AmperGetPointInfo info = Webservice.GetPersonalAccountInfoFromAmper(item.PersonalAccount);
                        if (info != null && resesNamesFromAmper.Contains(info.Dogovor.Res))
                        {
                            dataItem = new SmallEngineDataModel(info);
                            dataItem.SetIndications(item);
                            list.Add(dataItem);
                        }
                        else
                        {
                            notFoundedAccounts.Add(item);
                        }
                        //SmallEngineBillingObject obj = objects.FirstOrDefault(i => i.BillingPointPersonalAccount == item.PersonalAccount);
                        //if (obj != null)
                        //{
                        //    dataItem = new SmallEngineDataModel(obj);
                        //    dataItem.SetIndications(item);
                        //    list.Add(dataItem);
                        //}
                        //else
                        //    notFoundedAccounts.Add(item);
                    }
                    else
                    {
                        dataItem = new SmallEngineDataModel() { BillingPointPersonalAccount = item.PersonalAccount };
                        dataItem.SetIndications(item);
                        list.Add(dataItem);
                    }
                }

                System.Diagnostics.Debug.WriteLine(">> не найдено " + notFoundedAccounts.Count);

                Items = list;

                Configuration.Instance.Settings = new Data()
                {
                    Items = list,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    Profile = SelectedProfile,
                    Enterprises = Configuration.Instance.Enterprises.ToList(),
                    SEO = sesObjects,
                    Configs = configs
                };
                Configuration.Instance.SaveData();
            }
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

        private bool ViewFilter(object item)
        {
            if (SelectedEnterprise == null || SelectedEnterprise.EnterpriseType == "ФЭС")
                return true;

            SmallEngineDataModel data = item as SmallEngineDataModel;
            if (null == data)
                return true;

            return data.ResName == SelectedEnterprise.EnterpriseName;
        }

        private void UpdateView()
        {
            if (_collectionView != null)
                _collectionView.Refresh();
        }

        #endregion

        #region Properties

        private IList<SmallEngineDataModel> _items;
        public IList<SmallEngineDataModel> Items
        {
            get { return _items; }
            private set
            {
                _items = value;
                RaisePropertyChanged("Items");
                RaisePropertyChanged("View");
            }
        }

        private ICollectionView _collectionView;
        public ICollectionView View
        {
            get
            {
                if (_collectionView == null && _items != null)
                {
                    _collectionView = CollectionViewSource.GetDefaultView(_items);
                    /*_collectionView.GroupDescriptions.Add(_propertyGroupDescriptionRes);
                    _collectionView.GroupDescriptions.Add(new PropertyGroupDescription("ContractNumber"));
                    _collectionView.GroupDescriptions.Add(new PropertyGroupDescription("CityName"));
                    _collectionView.GroupDescriptions.Add(new PropertyGroupDescription("ObjectName"));*/

                    _collectionView.SortDescriptions.Add(new SortDescription("ResName", ListSortDirection.Ascending));
                    _collectionView.SortDescriptions.Add(new SortDescription("ContractNumber", ListSortDirection.Ascending));
                    _collectionView.SortDescriptions.Add(new SortDescription("CityName", ListSortDirection.Ascending));
                    _collectionView.SortDescriptions.Add(new SortDescription("ObjectName", ListSortDirection.Ascending));
                    _collectionView.SortDescriptions.Add(new SortDescription("AccountingPointName", ListSortDirection.Ascending));

                    _collectionView.Filter = ViewFilter;
                }
                return _collectionView;
            }
            set
            {
                _collectionView = value;
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


        private DateTime _fromDate = DateTime.Today;
        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; RaisePropertyChanged("FromDate"); }
        }
        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; RaisePropertyChanged("ToDate"); }
        }
        private Profile _profile = Profile.Days;

        public Profile SelectedProfile
        {
            get { return _profile; }
            set { _profile = value; RaisePropertyChanged("SelectedProfile"); OnSelectedProfileChanged(); }
        }


        private EnterpriseViewItem _selectedEnterprise;
        public EnterpriseViewItem SelectedEnterprise
        {
            get { return _selectedEnterprise; }
            set
            {
                _selectedEnterprise = value;
                RaisePropertyChanged("SelectedEnterprise");

                if (_selectedEnterprise.EnterpriseType != "ФЭС")
                {
                    if (_collectionView != null && _collectionView.GroupDescriptions.Contains(_propertyGroupDescriptionRes))
                        _collectionView.GroupDescriptions.Remove(_propertyGroupDescriptionRes);
                }
                else if (_collectionView != null && _collectionView.GroupDescriptions.Contains(_propertyGroupDescriptionRes) == false)
                    _collectionView.GroupDescriptions.Insert(0, _propertyGroupDescriptionRes);
                UpdateView();
            }
        }

        public List<MeterProblemInformationViewItem> ProblemMetersList { get; private set; }

        public ListCollectionView SEO { get; private set; }

        public ICommand GetData { get; }
        public ICommand Update { get; }
        public ICommand Save { get; }
        public ICommand Print { get; }

        public bool IsServiceAvailable => Webservice.CheckHostAvailablity(Webservice.ArmtesServerAddress);

        #endregion
    }
}
