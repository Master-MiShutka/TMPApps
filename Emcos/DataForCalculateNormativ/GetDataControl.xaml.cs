using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for GetDataControl.xaml
    /// </summary>
    public partial class GetDataControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        private bool _isCompleted = false;
        private bool _isGettingData = false;
        private ObservableCollection<ListPointWithResult> _list;
        private Action _onClosed, _onCanceled;
        private double _progress = 0d;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Model.EmcosReport selectedReport;
        #endregion Fields

        #region Constructors

        public GetDataControl()
        {
            InitializeComponent();

            CancelCommand = new DelegateCommand(
                o =>
                {
                    App.UIAction(() =>
                    {
                        if (App.ShowQuestion("Прервать операцию?") == MessageBoxResult.Yes)
                        {
                            cts.Cancel();
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                            OnPropertyChanged("CanCancel");
                        }
                    });
                },
                o => cts != null && (cts.IsCancellationRequested == false));
            CloseControlCommand = new DelegateCommand(o =>
                {
                    if (_onClosed != null)
                        _onClosed();
                });
            SaveAllCommand = new DelegateCommand(o =>
                {
                    CommonOpenFileDialog cfd = new CommonOpenFileDialog();
                    cfd.Title = "Выберите папку, куда будут сохранены файлы";
                    cfd.IsFolderPicker = true;
                    cfd.AddToMostRecentlyUsedList = false;
                    cfd.EnsurePathExists = true;
                    cfd.Multiselect = false;
                    cfd.ShowPlacesList = true;
                    cfd.AllowNonFileSystemItems = true;
                    if (cfd.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        try
                        {
                            var folder = cfd.FileName;
                            foreach (var item in _list)
                                if (item.ResultType == "csv")
                                {
                                    System.IO.File.WriteAllText(
                                        System.IO.Path.Combine(folder, item.ResultName + ".csv"), (string)item.ResultValue, Encoding.UTF8);
                                }
                                else 
                                if (item.ResultType == "xls")
                                {
                                    byte[] bytes = (byte[])item.ResultValue;
                                    if (bytes != null)
                                        System.IO.File.WriteAllBytes(
                                            System.IO.Path.Combine(folder, item.ResultName + ".xls"), bytes);
                                }
                                else
                                {
                                    System.IO.File.WriteAllText(
                                        System.IO.Path.Combine(folder, item.ResultName + ".txt"), (string)item.ResultValue, Encoding.UTF8);
                                }
                        }
                        catch (Exception ex)
                        {
                            App.ShowError("При сохранении произошла ошибка:\n" + App.GetExceptionDetails(ex));
                        }
                    }
                },
                o => _list != null && _list.All(i => i.ResultValue != null));
            SaveAllInSigleFileCommand = new DelegateCommand(o =>
                {
                    bool isStringContent = _list.All(i => i.ResultType != "xls");
                    if (isStringContent)
                    {
                        try
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var item in _list)
                            {
                                sb.AppendLine((string)item.ResultValue);
                            }

                            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();

                            sfd.Filter = "CSV файл - значения, разделённые точкой с запятой  (*.csv)|*.csv";
                            sfd.DefaultExt = ".csv";
                            sfd.AddExtension = true;
                            sfd.FileName = DateTime.Now.AddMonths(-1).ToString("Режимные данные за MM-yyyy");
                            Nullable<bool> result = sfd.ShowDialog(App.Current.MainWindow);
                            if (result == true)
                            {
                                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                            }
                        }
                        catch (Exception ex)
                        {
                            App.ShowError("При сохранении произошла ошибка:\n" + App.GetExceptionDetails(ex));
                        }
                    }
                },
                o => _list != null && _list.All(i => i.ResultValue != null) && _list.All(i => i.ResultType != "xls"));
            SaveCommand = new DelegateCommand(o =>
                {
                    try
                    {
                        ListPointWithResult point = o as ListPointWithResult;
                        if (point != null)
                        {
                            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                            sfd.FileName = String.Format("Режимные данные по '{0}' за {1:MM-yyyy}", point.ResultName, DateTime.Now.AddMonths(-1));
                            sfd.AddExtension = true;
                            if (point.ResultType == "xls")
                            {
                                sfd.Filter = "Электронная таблица  (*.xls)|*.xls";
                                sfd.DefaultExt = ".xls";
                            }
                            else 
                            if (point.ResultType == "csv")
                            {
                                sfd.Filter = "CSV файл - значения, разделённые точкой с запятой  (*.csv)|*.csv";
                                sfd.DefaultExt = ".csv";
                            }
                            else
                            {
                                sfd.Filter = "Текстовый файл (*.txt)|*.txt";
                                sfd.DefaultExt = ".txt";
                            }
                            Nullable<bool> result = sfd.ShowDialog(App.Current.MainWindow);
                            if (result == true)
                            {
                                if (point.ResultType == "xls")
                                {
                                    byte[] bytes = (byte[])point.ResultValue;
                                    if (bytes != null)
                                        System.IO.File.WriteAllBytes(sfd.FileName, bytes);
                                }
                                else
                                    System.IO.File.WriteAllText(sfd.FileName, (string)point.ResultValue, Encoding.UTF8);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.ShowError("При сохранении произошла ошибка:\n" + App.GetExceptionDetails(ex));
                    }
                });
            DataContext = this;
        }

        public GetDataControl(IList<ListPoint> source, Action closeed, Action canceled = null) : this()
        {
            if (source == null || closeed == null || canceled == null)
                throw new ArgumentNullException();
            List = new ObservableCollection<ListPointWithResult>(source.Select(i => new ListPointWithResult(i)).ToList());

            _onClosed = closeed;
            _onCanceled = canceled;
        }

        #endregion Constructors

        #region Private Methods

        private void Go()
        {
            try
            {
                int itemsCount = _list.Count;
                Action<int> report = pos =>
                    {
                        Progress = 100d * pos / itemsCount;
                        App.UIAction(() => App.Current.MainWindow.TaskbarItemInfo.ProgressValue = ((double)pos) / itemsCount);
                    };

                IsGettingData = true;
                IsCompleted = false;

                int index = 0;
                foreach (var item in _list)
                    item.Status = "Wait";
                foreach (var item in _list)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }
                    item.Status = "Processing";
                    PrepareReport(item);
                    item.Status = "Processed";

                    report(++index);
                }
                IsCompleted = true;
                IsGettingData = false;
                App.UIAction(() => App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal);
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                App.UIAction(() =>
                {
                    App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    App.ShowError("Произошла ошибка:\n" + App.GetExceptionDetails(ex));
                    IsCompleted = true;
                    IsGettingData = false;
                });
            }
        }

        private void PrepareReport(ListPointWithResult point)
        {
            int groupId = point.Id;
            string param = String.Format("__invoker=undefined&RP_ID={0}&RP_NAME={1}&errorDispatch=false&dataBlock=PARAMETERS&action=GET",
                selectedReport.RP_ID, selectedReport.RP_NAME);
            string url = @"{0}scripts/reports.asp";
            string answer = ServiceHelper.ExecuteFunctionAsync(ServiceHelper.MakeRequestAsync, url, param, true, (p) => ServiceHelper.DecodeAnswer(p)).Result;

            if (String.IsNullOrEmpty(answer) == false && answer.Contains("result=0"))
            {
                var records = Utils.ParseRecords(answer);
                if (records == null)
                    System.Diagnostics.Debugger.Break();

                List<RPQ> rpqs = new List<RPQ>(records.Count);
                foreach (var nvc in records)
                {
                    if (nvc.Get("COLUMN_NAME") != "GR_ID")
                    {
                        App.UIAction(() => App.ShowError("Выбран неверный отчёт! Ошибка при подготовке параметров."));
                        return;
                    }
                    RPQ rpq = new RPQ();
                    for (int i = 0; i < nvc.Count; i++)
                    {
                        #region Разбор полей

                        switch (nvc.GetKey(i))
                        {
                            case "RPQ_ID":
                                rpq.RPQ_ID = nvc[i];
                                break;

                            case "RPQ_NAME":
                                rpq.RPQ_NAME = nvc[i];
                                break;

                            case "TABLE_NAME":
                                rpq.TABLE_NAME = nvc[i];
                                break;

                            case "RPQF_ID":
                                rpq.RPQF_ID = nvc[i];
                                break;

                            case "RPQO_ID":
                                rpq.RPQO_ID = nvc[i];
                                break;

                            case "RPQO_CODE":
                                rpq.RPQO_CODE = nvc[i];
                                break;

                            case "RPQF_VALUE":
                                rpq.RPQF_VALUE = nvc[i];
                                break;

                            case "FIELD_DESC":
                                rpq.FIELD_DESC = nvc[i];
                                break;

                            case "RPQF_ALLOW_EMPTY_VALUES":
                                rpq.RPQF_ALLOW_EMPTY_VALUES = nvc[i];
                                break;

                            case "DATA_TYPE":
                                rpq.DATA_TYPE = nvc[i];
                                break;
                        }

                        #endregion Разбор полей
                    }
                    rpqs.Add(rpq);
                }
                param = String.Format("__invoker=undefined&RP_ID={0}&RP_NAME={1}&errorDispatch=false&dataBlock=PREPARED_RP&action=GET",
                    selectedReport.RP_ID, selectedReport.RP_NAME);
                url = @"{0}scripts/reports.asp";
                answer = ServiceHelper.ExecuteFunctionAsync(ServiceHelper.MakeRequestAsync, url, param, true, (p) => ServiceHelper.DecodeAnswer(p)).Result;

                if (String.IsNullOrEmpty(answer) == false && answer.Contains("result=0"))
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < rpqs.Count; i++)
                    {
                        RPQ rpq = rpqs[i];
                        sb.AppendFormat("RPQF_{0}={1}&DATA_TYPE_0_{2}={3}&RPQF_VALUE_0_{2}={1}&COLUMN_NAME_0_{2}={4}&RPQO_ID_0_{2}={5}&RPQF_ID_0_{2}={0}&",
                            rpq.RPQF_ID,    //0
                            groupId,        //1
                            i,              //2
                            rpq.DATA_TYPE,  //3
                            rpq.COLUMN_NAME,//4
                            rpq.RPQO_ID     //5
                            );
                    }
                    sb.AppendFormat("RPQ_NAME_0={0}&RPQ_ID_0={1}&RPQF_COUNT_0={2}&RPQ_COUNT=1&export_to_html=0&RP_ID={3}&action=SHOW_REPORT",
                        rpqs[0].RPQ_NAME,
                        rpqs[0].RPQ_ID,
                        rpqs.Count,
                        selectedReport.RP_ID);

                    url = @"{0}scripts/ReportGenerator.asp";
                    answer = ServiceHelper.MakeRequestAsync(url, sb.ToString()).Result;

                    if (ServiceHelper.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // получение имени файла
                        /*
                        <HTML>
                        <HEAD>
                        <META HTTP-EQUIV=Refresh CONTENT="0;URL=../\\reports\2016\12\30\Для расчета балансов ПС до 35кВ_2016_12_30_113404_1.txt">
                        </HEAD>
                        </HTML>
                        */
                        Match KeywordMatch = Regex.Match(answer, "<meta HTTP-EQUIV=Refresh content=\"([^<]*)\">", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string metaKeywords = KeywordMatch.Groups[1].Value;
                        string file = metaKeywords.Substring(8).Replace("../", "").Replace("\\", "/");

                        var client = new WebClient();

                        if (file.EndsWith(".txt"))
                        {
                            point.ResultValue = client.DownloadString(ServiceHelper.SiteAddress + file);
                            point.ResultType = "txt";
                        }
                        else
                        if (file.EndsWith(".xls"))
                        {
                            point.ResultValue = client.DownloadData(ServiceHelper.SiteAddress + file);
                            point.ResultType = "xls";
                        }
                        else
                        {
                            point.ResultValue = client.DownloadString(ServiceHelper.SiteAddress + file);
                            point.ResultType = "unknown";
                        }
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            selectedReport = null;
            try
            {
                selectedReport = App.Base64StringToObject<Model.EmcosReport>(Properties.Settings.Default.SelectedReport);
            }
            catch { }
            if (selectedReport == null)
            {
                App.ShowWarning(Strings.ReportNotSpecified);
                _onClosed();
            }

            App.Current.MainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            App.Current.MainWindow.TaskbarItemInfo.Description = "Получение данных";
            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

            Task.Factory.StartNew(() => Go())
                .ContinueWith((t) => _onCanceled(), cts.Token, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
        }
        #endregion Private Methods

        #region Command and Properties

        public ICommand CancelCommand { get; private set; }
        public ICommand CloseControlCommand { get; private set; }
        public bool IsCompleted
        {
            get { return _isCompleted; }
            private set { SetProperty(ref _isCompleted, value); }
        }

        public bool IsGettingData
        {
            get { return _isGettingData; }
            private set { SetProperty(ref _isGettingData, value); }
        }

        public ObservableCollection<ListPointWithResult> List
        {
            get { return _list; }
            private set { SetProperty(ref _list, value); }
        }

        public double Progress
        {
            get { return _progress; }
            private set { SetProperty(ref _progress, value); }
        }

        public ICommand SaveAllCommand { get; private set; }
        public ICommand SaveAllInSigleFileCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        #endregion Command and Properties

        #region INotifyPropertyChanged Members

        #region Debugging Aides

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }

    #region Public Classes

    public class ListPointWithResult : ListPoint
    {
        private string _resultName;
        private string _resultType;
        private object _resultValue;
        private string _status;

        public ListPointWithResult() { }
        public ListPointWithResult(ListPoint source)
        {
            this.ParentId = source.ParentId;
            this.ParentTypeCode = source.ParentTypeCode;
            this.ParentName = source.ParentName;
            this.Id = source.Id;
            this.Name = source.Name;
            this.IsGroup = source.IsGroup;
            this.TypeCode = source.TypeCode;
            this.EсpName = source.EсpName;
            this.Type = source.Type;
            this.Checked = source.Checked;

            string name = source.Name;
            if (source.ParentTypeCode == "SUBSTATION")
                if (String.IsNullOrEmpty(source.ParentName) == false)
                    name = source.ParentName + " - " + name;
            this.ResultName = name;
        }

        public bool Processed
        {
            get { return Status == "Processed" && ResultValue != null; }
        }

        public string ResultName
        {
            get { return _resultName; }
            set { SetProperty(ref _resultName, value); }
        }

        public string ResultType
        {
            get { return _resultType; }
            set { SetProperty(ref _resultType, value); }
        }

        public object ResultValue
        {
            get { return _resultValue; }
            set { SetProperty(ref _resultValue, value); }
        }

        /// <summary>
        /// Processed | Wait
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);
                OnPropertyChanged("Processed");
            }
        }
    }

    public class RPQ
    {
        public string COLUMN_NAME { get { return "GR_ID"; } }
        public string DATA_TYPE { get; set; }
        public string FIELD_DESC { get; set; }
        public string RPQ_ID { get; set; }
        public string RPQ_NAME { get; set; }
        public string RPQF_ALLOW_EMPTY_VALUES { get; set; }
        public string RPQF_ID { get; set; }
        public string RPQF_VALUE { get; set; }
        public string RPQO_CODE { get; set; }
        public string RPQO_ID { get; set; }
        public string TABLE_NAME { get; set; }
    }

    #endregion Public Classes

}

/*

callerUID=INIT_ID_1	instance=_level1.scene.spContentHolder6.PANEL_RP_TREE.spContentHolder0.RP_TREE.spContentHolder	RP_PUBLIC=1	__const=TYPE=RP_TYPE	datablock=RP_TYPE	action=GET

RP_TYPE_ID_0=72	RP_TYPE_NAME_0=DWRES Отчеты	TYPE_0=RP_TYPE	RP_PUBLIC_0=1
RP_TYPE_ID_1=42	RP_TYPE_NAME_1=Балансы подстанций	TYPE_1=RP_TYPE	RP_PUBLIC_1=1
RP_TYPE_ID_2=2	RP_TYPE_NAME_2=Волковысские сети	TYPE_2=RP_TYPE	RP_PUBLIC_2=1
RP_TYPE_ID_3=22	RP_TYPE_NAME_3=Выработка	TYPE_3=RP_TYPE	RP_PUBLIC_3=1
RP_TYPE_ID_4=7	RP_TYPE_NAME_4=Отчеты ТЭЦ-2	TYPE_4=RP_TYPE	RP_PUBLIC_4=1
RP_TYPE_ID_5=32	RP_TYPE_NAME_5=Отчётные формы	TYPE_5=RP_TYPE	RP_PUBLIC_5=1
RP_TYPE_ID_6=17	RP_TYPE_NAME_6=Перетоки	TYPE_6=RP_TYPE	RP_PUBLIC_6=1
RP_TYPE_ID_7=67	RP_TYPE_NAME_7=Промышленные предприятия	TYPE_7=RP_TYPE	RP_PUBLIC_7=1
RP_TYPE_ID_8=27	RP_TYPE_NAME_8=Структура дерева	TYPE_8=RP_TYPE	RP_PUBLIC_8=1
RP_TYPE_ID_9=47	RP_TYPE_NAME_9=Тест	TYPE_9=RP_TYPE	RP_PUBLIC_9=1
RP_TYPE_ID_10=37	RP_TYPE_NAME_10=Утвержденные	TYPE_10=RP_TYPE	RP_PUBLIC_10=1
result=0	recordCount=11
-------------------------------------------------------
callerUID=INIT_ID_1/RP_TYPE_ID_72	instance=_level1.scene.spContentHolder6.PANEL_RP_TREE.spContentHolder0.RP_TREE.spContentHolder	RP_PUBLIC=1	RP_TYPE_ID=72	__const=TYPE=RP	datablock=RP	action=GET

RP_ID_0=679	RP_TYPE_ID_0=72	RP_NAME_0=Выборка точек по группе	RP_PUBLIC_0=1	RP_DESCRIPTION_0=	RPF_ID_0=1	RP_LOG_ENABLED_0=0	USER_NAME_0=System user EMCOS	TYPE_0=RP
RP_ID_1=675	RP_TYPE_ID_1=72	RP_NAME_1=Для расчета балансов ПС до 35кВ	RP_PUBLIC_1=1	RP_DESCRIPTION_1=	RPF_ID_1=2	RP_LOG_ENABLED_1=0	USER_NAME_1=Щелухин Д.Д.	TYPE_1=RP
RP_ID_2=698	RP_TYPE_ID_2=72	RP_NAME_2=Для расчета балансов ПС 35-770кВ	RP_PUBLIC_2=1	RP_DESCRIPTION_2=	RPF_ID_2=2	RP_LOG_ENABLED_2=1	USER_NAME_2=System user EMCOS	TYPE_2=RP
result=0	recordCount=3
-------------------------------------------------------

-------------------------------------------------------
-------------------------------------------------------
*******
__invoker=undefined RP_ID=679   RP_NAME=Выборка точек по группе errorDispatch=false dataBlock=PARAMETERS    action=GET

RPQ_ID_0=1288   RPQ_NAME_0=Запрос 1	TABLE_NAME_0=	RPQF_ID_0=5404	COLUMN_NAME_0=GR_ID	RPQO_ID_0=1	RPQO_CODE_0==	RPQF_VALUE_0=	FIELD_DESC_0=	RPQF_ALLOW_EMPTY_VALUES_0=0	DATA_TYPE_0=VARCHAR2	result=0	recordCount=1
&RPQ_ID_0=1288&RPQ_NAME_0=Запрос 1&TABLE_NAME_0=&RPQF_ID_0=5404&COLUMN_NAME_0=GR_ID&RPQO_ID_0=1&RPQO_CODE_0==&RPQF_VALUE_0=&FIELD_DESC_0=&RPQF_ALLOW_EMPTY_VALUES_0=0&DATA_TYPE_0=VARCHAR2&result=0&recordCount=1
*******
refresh=true    instance=_level1.scene.spContentHolder6.POINTS.POINTMENU.APTTabs.spContentHolder0.APT.spContentHolder   TYPE=GROUP  action=expand   ID=1

GR_ID_0=7257	GRC_NR_0=	GRC_DESC_0=	GR_CODE_0=Расчетные точки	GR_NAME_0=Расчетные точки	USER_NAME_0=System user EMCOS	IS_PUBLIC_0=1	IS_PUBLIC_TXT_0=Да	GR_TYPE_ID_0=52	GR_TYPE_NAME_0=Расчетные точки	GR_TYPE_CODE_0=CalcPoint	PARENT_0=0	HASCHILDS_0=1	GRC_BT_0=	TYPE_0=GROUP
GR_ID_1=52	GRC_NR_1=	GRC_DESC_1=	GR_CODE_1=1	GR_NAME_1=Белэнерго	USER_NAME_1=System user EMCOS	IS_PUBLIC_1=1	IS_PUBLIC_TXT_1=Да	GR_TYPE_ID_1=7	GR_TYPE_NAME_1=Белэнерго	GR_TYPE_CODE_1=CONCERN	PARENT_1=0	HASCHILDS_1=1	GRC_BT_1=	TYPE_1=GROUP	result=0	recordCount=2
*******
__invoker=undefined	RP_ID=679	RP_NAME=Выборка точек по группе     errorDispatch=false     dataBlock=PREPARED_RP	action=GET

&result=0   recordCount=0
-------------------------------------------------------
-------------------------------------------------------
*******
__invoker=undefined	RP_ID=675	RP_NAME=Для расчета балансов ПС до 35кВ	errorDispatch=false	dataBlock=PARAMETERS	action=GET

RPQ_ID_0=1283	RPQ_NAME_0=Запрос 1	TABLE_NAME_0=	RPQF_ID_0=5409	COLUMN_NAME_0=GR_ID	RPQO_ID_0=1	RPQO_CODE_0==	RPQF_VALUE_0=	FIELD_DESC_0=	RPQF_ALLOW_EMPTY_VALUES_0=0	DATA_TYPE_0=VARCHAR2
RPQ_ID_1=1283	RPQ_NAME_1=Запрос 1	TABLE_NAME_1=	RPQF_ID_1=5410	COLUMN_NAME_1=GR_ID	RPQO_ID_1=1	RPQO_CODE_1==	RPQF_VALUE_1=	FIELD_DESC_1=	RPQF_ALLOW_EMPTY_VALUES_1=0	DATA_TYPE_1=VARCHAR2
RPQ_ID_2=1283	RPQ_NAME_2=Запрос 1	TABLE_NAME_2=	RPQF_ID_2=5411	COLUMN_NAME_2=GR_ID	RPQO_ID_2=1	RPQO_CODE_2==	RPQF_VALUE_2=	FIELD_DESC_2=	RPQF_ALLOW_EMPTY_VALUES_2=0	DATA_TYPE_2=VARCHAR2
RPQ_ID_3=1283	RPQ_NAME_3=Запрос 1	TABLE_NAME_3=	RPQF_ID_3=5412	COLUMN_NAME_3=GR_ID	RPQO_ID_3=1	RPQO_CODE_3==	RPQF_VALUE_3=	FIELD_DESC_3=	RPQF_ALLOW_EMPTY_VALUES_3=0	DATA_TYPE_3=VARCHAR2
result=0	recordCount=4
*******
__invoker=undefined	RP_ID=675	RP_NAME=Для расчета балансов ПС до 35кВ	errorDispatch=false	dataBlock=PREPARED_RP	action=GET

&result=0&recordCount=0
-------------------------------------------------------
-------------------------------------------------------
*******
__invoker=undefined	RP_ID=698	RP_NAME=Для расчета балансов ПС 35-770кВ	errorDispatch=false	dataBlock=PARAMETERS	action=GET

RPQ_ID_0=1307	RPQ_NAME_0=Запрос 1	TABLE_NAME_0=	RPQF_ID_0=5534	COLUMN_NAME_0=GR_ID	RPQO_ID_0=1	RPQO_CODE_0==	RPQF_VALUE_0=	FIELD_DESC_0=	RPQF_ALLOW_EMPTY_VALUES_0=0	DATA_TYPE_0=VARCHAR2
RPQ_ID_1=1307	RPQ_NAME_1=Запрос 1	TABLE_NAME_1=	RPQF_ID_1=5535	COLUMN_NAME_1=GR_ID	RPQO_ID_1=1	RPQO_CODE_1==	RPQF_VALUE_1=	FIELD_DESC_1=	RPQF_ALLOW_EMPTY_VALUES_1=0	DATA_TYPE_1=VARCHAR2
RPQ_ID_2=1307	RPQ_NAME_2=Запрос 1	TABLE_NAME_2=	RPQF_ID_2=5536	COLUMN_NAME_2=GR_ID	RPQO_ID_2=1	RPQO_CODE_2==	RPQF_VALUE_2=	FIELD_DESC_2=	RPQF_ALLOW_EMPTY_VALUES_2=0	DATA_TYPE_2=VARCHAR2
RPQ_ID_3=1307	RPQ_NAME_3=Запрос 1	TABLE_NAME_3=	RPQF_ID_3=5537	COLUMN_NAME_3=GR_ID	RPQO_ID_3=1	RPQO_CODE_3==	RPQF_VALUE_3=	FIELD_DESC_3=	RPQF_ALLOW_EMPTY_VALUES_3=0	DATA_TYPE_3=VARCHAR2
result=0	recordCount=4
-------------------------------------------------------
-------------------------------------------------------
POST http://10.96.18.16/emcos/scripts/ReportGenerator.asp
1807 - Клиденята (GR_ID_14=1807&GRC_NR_14=&GRC_DESC_14=&GR_CODE_14=143688.Клиденяты&GR_NAME_14=ПС 35кВ Клиденяты)
RPQF_5404=1807	DATA_TYPE_0_0=VARCHAR2	RPQF_VALUE_0_0=1807	COLUMN_NAME_0_0=GR_ID	RPQO_ID_0_0=1	RPQF_ID_0_0=5404	RPQ_NAME_0=Запрос 1	RPQ_ID_0=1288	RPQF_COUNT_0=1
RPQ_COUNT=1	export_to_html=0	RP_ID=679	action=SHOW_REPORT

<HTML>
<HEAD>
<META HTTP-EQUIV=Refresh CONTENT="0;URL=../\\reports\2016\12\30\Выборка точек по группе_2016_12_30_112843_1.xls">
</HEAD>
</HTML>

*******
POST http://10.96.18.16/emcos/scripts/ReportGenerator.asp
RPQF_5412=1807	DATA_TYPE_0_3=VARCHAR2	RPQF_VALUE_0_3=1807	COLUMN_NAME_0_3=GR_ID	RPQO_ID_0_3=1	RPQF_ID_0_3=5412	RPQF_5411=1807	DATA_TYPE_0_2=VARCHAR2	RPQF_VALUE_0_2=1807	COLUMN_NAME_0_2=GR_ID	RPQO_ID_0_2=1	RPQF_ID_0_2=5411	RPQF_5410=1807	DATA_TYPE_0_1=VARCHAR2	RPQF_VALUE_0_1=1807	COLUMN_NAME_0_1=GR_ID	RPQO_ID_0_1=1	RPQF_ID_0_1=5410	RPQF_5409=1807	DATA_TYPE_0_0=VARCHAR2	RPQF_VALUE_0_0=1807	COLUMN_NAME_0_0=GR_ID	RPQO_ID_0_0=1	RPQF_ID_0_0=5409	RPQ_NAME_0=Запрос 1	RPQ_ID_0=1283	RPQF_COUNT_0=4	RPQ_COUNT=1	export_to_html=0	RP_ID=675	action=SHOW_REPORT

<HTML>
<HEAD>
<META HTTP-EQUIV=Refresh CONTENT="0;URL=../\\reports\2016\12\30\Для расчета балансов ПС до 35кВ_2016_12_30_113404_1.txt">
</HEAD>
</HTML>

4082;10,4;0;0;0;0;0;0
4087;10,4;16,82619525;4,83579369;146,400047;0;22,560049;0
4092;10,4;8,85270864;4,02588858;27,480012;0;6,360012;0
4093;10,4;7,89662588;,6989603;134,640031;0;21,240035;0

*******
POST http://10.96.18.16/emcos/scripts/ReportGenerator.asp
RPQF_5537=1807	DATA_TYPE_0_3=VARCHAR2	RPQF_VALUE_0_3=1807	COLUMN_NAME_0_3=GR_ID	RPQO_ID_0_3=1	RPQF_ID_0_3=5537	RPQF_5536=1807	DATA_TYPE_0_2=VARCHAR2	RPQF_VALUE_0_2=1807	COLUMN_NAME_0_2=GR_ID	RPQO_ID_0_2=1	RPQF_ID_0_2=5536	RPQF_5535=1807	DATA_TYPE_0_1=VARCHAR2	RPQF_VALUE_0_1=1807	COLUMN_NAME_0_1=GR_ID	RPQO_ID_0_1=1	RPQF_ID_0_1=5535	RPQF_5534=1807	DATA_TYPE_0_0=VARCHAR2	RPQF_VALUE_0_0=1807	COLUMN_NAME_0_0=GR_ID	RPQO_ID_0_0=1	RPQF_ID_0_0=5534	RPQ_NAME_0=Запрос 1	RPQ_ID_0=1307	RPQF_COUNT_0=4	RPQ_COUNT=1	export_to_html=0	RP_ID=698	action=SHOW_REPORT

<HTML>
<HEAD>
<META HTTP-EQUIV=Refresh CONTENT="0;URL=../\\reports\2016\12\30\Для расчета балансов ПС 35-770кВ_2016_12_30_121702_1.txt">
</HEAD>
</HTML>

0;0;0;0;0;0;0;0;0;0;0;0
4087;720;0;6,24;16,82619525;4,94451456;0;,10872086;,14640004;0;,02256004;0
4093;0;720;6,24;0;,26622548;7,89662588;,96518579;,13464003;0;,02124003;0
4092;0;720;6,24;0;,00240005;8,85270864;4,02828864;,02748001;0;,00636001;0

Код АСКУЭ;Тнаг;Тген;Uср.э.;Wpн;Wqн;Wpг;Wqг;Pмакс;Pмин;Qмакс;Qмин

&RP_TYPE_NAME_0=DWRES Отчеты&RPA_NAME_0=&REPORT_NAME_0=Reports\res\2016\12\30\Для расчета балансов ПС 35-770кВ_2016_12_30_122402_1.txt&DB_TIME_0=30.12.2016 12:25:42&GR_CODE_0=143126.Сморгонь&FROM_DA_0=&TO_DA_0=&RP_TYPE_NAME_1=DWRES Отчеты&RPA_NAME_1=&REPORT_NAME_1=Reports\res\2016\12\30\Для расчета балансов ПС 35-770кВ_2016_12_30_121702_1.txt&DB_TIME_1=30.12.2016 12:17:37&GR_CODE_1=143688.Клиденяты&FROM_DA_1=&TO_DA_1=&result=0&recordCount=2

*/
