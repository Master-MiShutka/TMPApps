using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.Controls
{
    /// <summary>
    /// Interaction logic for ArchView.xaml
    /// </summary>
    public partial class ArchView : UserControl, IStateObject
    {
        private string _wid = string.Empty;
        private DateTime _timeBegin;
        private DateTime _timeEnd;

        private Model.ML_Param _ml;
        private Model.ArchAP _ap;

        public ArchView()
        {
            InitializeComponent();
        }
        public void Init(string wID, DateTime timeBegin, DateTime timeEnd)
        {
            if (String.IsNullOrWhiteSpace(wID))
                throw new ArgumentNullException("wID");
            if (timeBegin == null)
                throw new ArgumentNullException("timeBegin");
            if (timeEnd == null)
                throw new ArgumentNullException("timeEnd");

            _wid = wID;
            _timeBegin = timeBegin;
            _timeEnd = timeEnd;
            State = State.Busy;
            GetParameters();
            GetPoints();
            GetExtra();

            if ((parametersList.Items != null && parametersList.Items.Count > 0))
                parametersList.SelectedIndex = 0;
            if ((pointsList.Items != null && pointsList.Items.Count > 0))
                pointsList.SelectedIndex = 0;
            State = State.Idle;
        }
        //****************************************************************
        private void GetParameters()
        {
            var sendData = String.Format("dataBlock=PARAM&action=GET&wID={0}", _wid);
            Func<string, bool> post = (data) =>
            {
                if (String.IsNullOrWhiteSpace(data)) return false;

                var list = Utils.Params(data);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    parametersList.ItemsSource = list;
                }));
                return true;
            };
            Utils.GetData(this, EmcosSiteWrapper.Instance.GetViewAsync, sendData, post);
        }
        //****************************************************************
        private void GetPoints()
        {
            var sendData = String.Format("dataBlock=AP&action=GET&wID={0}", _wid);
            Func<string, bool> post = (data) =>
            {
                if (String.IsNullOrWhiteSpace(data)) return false;

                var list = Utils.ArchAPs(data);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    pointsList.ItemsSource = list;
                }));
                return true;
            };
            Utils.GetData(this, EmcosSiteWrapper.Instance.GetViewAsync, sendData, post);
        }
        //****************************************************************
        private void GetExtra()
        {
            var sendData = String.Format("scope=ARCH&dataBlock=EXTRA&action=GET&wID={0}", _wid);
            Func<string, bool> post = (data) =>
            {
                if (String.IsNullOrWhiteSpace(data)) return false;

                var nvc = Utils.ParsePairs(data);
                if ((nvc != null) && (nvc.Get("result") == "0"))
                {
                    var list = Utils.ParseRecords(data);
                    var records = list[0];

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lblFrom.Content = records.Get("FROM");
                        lblTo.Content = records.Get("TO");
                        lblTitle.Content = nvc.Get("WIN_TITLE");
                    }));
                }
                return true;
            };
            Utils.GetData(this, EmcosSiteWrapper.Instance.GetViewAsync, sendData, post);
        }
        //****************************************************************
        private void GetArchiveData()
        {
            if (_ml == null)
            {
                MessageBox.Show("Не выбрано измерение!");
                return;
            }
            if (_ap == null)
            {
                MessageBox.Show("Не выбрана точка!");
                return;
            }
            State = State.Busy;
            Progress = 1;
            Log = String.Empty;

            var md = _ml.MD.Id;
            var aggs = _ml.AGGS.Id;
            var datacode = _ml.Id;
            var type = _ap.Type;
            var id = _ap.Id;

            var sendData = String.Format(
                "action=GET&dataBlock=DW&BILLING_HOUR=0&FREEZED=0&SHOW_MAP_DATA=0&GR_VALUE={0}&"+
                "EC_ID=&MD_ID={1}&WO_ACTS=0&WO_BYP=0&GMOD_ID=&TimeEnd={3}&TimeBegin={2}&AGGS_ID={4}&DATA_CODE={5}&TYPE={6}&ID={7}",
                "VAL",
                md,
                _timeBegin,
                _timeEnd,
                aggs,
                datacode,
                type,
                id);
            sendData = System.Web.HttpUtility.UrlPathEncode(sendData).Replace("_", "%5F").Replace("+", "%2B").ToUpper();

            var task = EmcosSiteWrapper.Instance.GetArchiveData(_ml, _ap, _timeBegin, _timeEnd);
            task.ContinueWith((s) =>
                {
                    MessageBox.Show("Произошла!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    State = State.Idle;
                    Progress = 0;
                },
                System.Threading.CancellationToken.None,
                System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted,
                System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

            task.ContinueWith((t) =>
            {
                var list = t.Result;

                if (list == null)
                    MessageBox.Show("Нет данных!", "Просмотр архивов", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                dg.ItemsSource = list;
                dataList.ItemsSource = list;

                State = State.Idle;
                Progress = 0;
                Log = String.Empty;
            },
                System.Threading.CancellationToken.None,
                System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion,
                System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            
        }
        //****************************************************************

        private void parametr_Changed(object sender, SelectionChangedEventArgs e)
        {
            _ml = parametersList.SelectedItem as Model.ML_Param;
            if ((_ml != null) && (_ap != null))
                GetArchiveData();
        }
        private void pointsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ap = pointsList.SelectedItem as Model.ArchAP;
            if ((_ml != null) && (_ap != null))
                GetArchiveData();
        }

        //****************************************************************        

        private void GetParameters_Click(object sender, RoutedEventArgs e)
        {
            GetParameters();
        }

        private void GetPoints_Click(object sender, RoutedEventArgs e)
        {
            GetPoints();
        }

        private void GetExtra_Click(object sender, RoutedEventArgs e)
        {
            GetExtra();
        }

        private void GetData_Click(object sender, RoutedEventArgs e)
        {
            GetArchiveData();
        }

        //****************************************************************
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IStateObject implementation
        private State _state = State.Idle;
        public State State
        {
            get { return _state; }
            set { _state = value; RaisePropertyChanged("State"); }
        }
        private int _progress = 0;
        public int Progress
        {
            get { return _progress; }
            set { _progress = value; RaisePropertyChanged("Progress"); }
        }
        public string Log { get; set; }
        #endregion
    }
}
/*
gui 9
> GetData - 9
> call func - 16
getpoints -16
> return func result
** return string
** return string 2
*15
> start BeginInvoke - 16
> get response
post 16
gui 9
> GetData - 9
> call func - 16
getpoints -16
> return func result
** return string
** return string 2
*15
> start BeginInvoke - 16
> get response
post 16
*/