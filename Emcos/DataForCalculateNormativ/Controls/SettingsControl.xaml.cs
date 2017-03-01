using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl, INotifyPropertyChanged
    {        
        private bool _serverAvailability = false;
        private bool _serviceAvailability = false;
        private string _messageError = String.Empty;
        private bool _isReportSettingsEnabled = false;
        private IEnumerable<Model.EmcosReportType> _reportsGroupList;
        private IEnumerable<Model.EmcosReport> _reportsList;
        private IEnumerable<ListPoint> _departamentsList;

        private Action _closeAction;

        public SettingsControl(Action closeAction)
        {
            if (closeAction == null)
                throw new ArgumentNullException("Close action must be not null");
            _closeAction = closeAction;

            InitializeComponent();

            IsReportSettingsEnabled = false;
            ServerAvailability = false;
            ServiceAvailability = false;

            CheckCommand = new DelegateCommand(async (o) =>
            {
                var dialog = App.WaitingScreen("Проверка корректности настроек ...");
                dialog.Show();
                if (await CheckServiceAvailability())
                    await InitReportsGroupAsync();

                dialog.Close();
            });
            ToDefaultCommand = new DelegateCommand((o) =>
            {
                Properties.Settings.Default.ServerAddress = "10.96.18.16";
                Properties.Settings.Default.SiteName = "emcos";
                Properties.Settings.Default.ServiceName = "testWebService/Service.asmx";
                Properties.Settings.Default.UserName = "sbyt";
                Properties.Settings.Default.Password = "sbyt";
                Properties.Settings.Default.NetTimeOutInSeconds = 1800;
            });
            SaveCommand = new DelegateCommand((o) =>
            {
                Properties.Settings.Default.Save();
                _closeAction();
            });

            DataContext = this;

            var dialog2 = App.WaitingScreen("Проверка корректности настроек ...");
            dialog2.Show();
            System.Threading.ThreadPool.QueueUserWorkItem(async o =>
                {

                    if (await CheckServiceAvailability())
                    {
                        await InitReportsGroupAsync();
                        //await InitReportsAsync();
                        await InitDepartamentsAsync();
                    }
                    dialog2.Close();
                });
        }

        #region Command and Properties

        public ICommand CheckCommand { get; private set; }
        public ICommand ToDefaultCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public bool ServerAvailability
        {
            get { return _serverAvailability; }
            private set { SetProperty(ref _serverAvailability, value); }
        }
        public bool ServiceAvailability
        {
            get { return _serviceAvailability; }
            private set { SetProperty(ref _serviceAvailability, value); }
        }
        public string MessageError
        {
            get { return _messageError; }
            private set { SetProperty(ref _messageError, value); }
        }
        public bool IsReportSettingsEnabled
        {
            get { return _isReportSettingsEnabled; }
            private set { SetProperty(ref _isReportSettingsEnabled, value); }
        }

        public IEnumerable<Model.EmcosReportType> ReportsGroupList
        {
            get { return _reportsGroupList; }
            private set
            {
                var dialog = App.WaitingScreen("Загрузка списка отчётов ...");
                dialog.Show();
                System.Threading.ThreadPool.QueueUserWorkItem(async o =>
                {
                    await InitReportsAsync();
                    dialog.Close();
                    SetProperty(ref _reportsGroupList, value);
                });
            }
        }
        public IEnumerable<Model.EmcosReport> ReportsList
        {
            get { return _reportsList; }
            private set { SetProperty(ref _reportsList, value); }
        }
        public IEnumerable<ListPoint> DepartamentsList
        {
            get { return _departamentsList; }
            private set { SetProperty(ref _departamentsList, value); }
        }
        #endregion

        private async Task<bool> CheckServiceAvailability()
        {
            bool result = false;

            Action<bool> onFinally = (ok) =>
            {
                if (ok)
                {
                    result = true;
                    ServiceAvailability = true;

                    MessageError = String.Empty;
                    IsReportSettingsEnabled = true;
                }
                else
                {
                    result = false;
                    ServiceAvailability = false;
                }
            };

            try
            {
                throw new Exception();

                bool serverAvailability = ServiceHelper.IsServerOnline();
                if (serverAvailability)
                {
                    ServerAvailability = true;

                    // проверка наличия доступа
                    bool hasRights = await ServiceHelper.LoginAsync(true);
                    string answer = ServiceHelper.ErrorMessage;
                    // если доступ не получен и не авторизованы
                    if (hasRights == false)
                    {
                        if (String.IsNullOrEmpty(answer))
                        {
                            MessageError = Strings.IncorrectService;
                            onFinally(false);
                        }
                        hasRights = await ServiceHelper.LoginAsync();
                        // авторизация не успешна
                        if (hasRights == false)
                        {
                            MessageError = Strings.AuthorizationFailed;
                            onFinally(false);
                        }
                        else
                            onFinally(true);
                    }
                    else
                    {
                        if (answer == "result=0&user=")
                        {
                            hasRights = await ServiceHelper.LoginAsync();
                            if (hasRights)
                                onFinally(true);
                            else
                                onFinally(false);
                        }
                        else
                            onFinally(true);
                    }
                }
                else
                {
                    ServerAvailability = false;
                    onFinally(false);
                }
            }
            catch (Exception ex)
            {
                string message = String.Format(Strings.Error, ex.Message);
                App.ShowErrorDialog(message);
                MessageError = message;
                onFinally(false);
                return false;
            }
            return result;
        }

        private async Task<bool> InitReportsGroupAsync()
        {
            string paramGetGroupsList = "callerUID=INIT_ID_1&instance=_level1.scene.spContentHolder8.PANEL_RP_TREE.spContentHolder0.RP_TREE.spContentHolder&RP_PUBLIC=1&__const=TYPE=RP_TYPE&datablock=RP_TYPE&action=GET";
            var list = new List<Model.EmcosReportType>();
            try
            {
                string url = @"{0}scripts/reports.asp";
                string data = await ServiceHelper.ExecuteFunctionAsync(ServiceHelper.MakeRequestAsync, url, paramGetGroupsList, true, (answer) => ServiceHelper.DecodeAnswer(answer));
                //string data = "&RP_TYPE_ID_0=72&RP_TYPE_NAME_0=DWRES Отчеты&TYPE_0=RP_TYPE&RP_PUBLIC_0=1&RP_TYPE_ID_1=42&RP_TYPE_NAME_1=Балансы подстанций&TYPE_1=RP_TYPE&RP_PUBLIC_1=1&RP_TYPE_ID_2=2&RP_TYPE_NAME_2=Волковысские сети&TYPE_2=RP_TYPE&RP_PUBLIC_2=1&RP_TYPE_ID_3=22&RP_TYPE_NAME_3=Выработка&TYPE_3=RP_TYPE&RP_PUBLIC_3=1&RP_TYPE_ID_4=7&RP_TYPE_NAME_4=Отчеты ТЭЦ-2&TYPE_4=RP_TYPE&RP_PUBLIC_4=1&RP_TYPE_ID_5=32&RP_TYPE_NAME_5=Отчётные формы&TYPE_5=RP_TYPE&RP_PUBLIC_5=1&RP_TYPE_ID_6=17&RP_TYPE_NAME_6=Перетоки&TYPE_6=RP_TYPE&RP_PUBLIC_6=1&RP_TYPE_ID_7=67&RP_TYPE_NAME_7=Промышленные предприятия&TYPE_7=RP_TYPE&RP_PUBLIC_7=1&RP_TYPE_ID_8=27&RP_TYPE_NAME_8=Структура дерева&TYPE_8=RP_TYPE&RP_PUBLIC_8=1&RP_TYPE_ID_9=47&RP_TYPE_NAME_9=Тест&TYPE_9=RP_TYPE&RP_PUBLIC_9=1&RP_TYPE_ID_10=37&RP_TYPE_NAME_10=Утвержденные&TYPE_10=RP_TYPE&RP_PUBLIC_10=1&result=0&recordCount=11";

                if (data.Contains("result=0") == false)
                    return false;

                var records = Utils.ParseRecords(data);
                if (records == null)
                    return false;

                foreach (var nvc in records)
                    if (nvc.Get("TYPE") == Model.EmcosReportType.TYPE)
                    {
                        Model.EmcosReportType element = new Model.EmcosReportType();
                        for (int i = 0; i < nvc.Count; i++)
                        {
                            #region Разбор полей
                            int intValue = 0;
                            switch (nvc.GetKey(i))
                            {
                                case "RP_TYPE_ID":
                                    int.TryParse(nvc[i], out intValue);
                                    element.RP_TYPE_ID = intValue;
                                    break;
                                case "RP_TYPE_NAME":
                                    element.RP_TYPE_NAME = nvc[i];
                                    break;
                                case "RP_PUBLIC":
                                    element.RP_PUBLIC = nvc[i] == "1";
                                    break;
                            }
                            #endregion
                        }
                        list.Add(element);
                    }
            }
            catch
            {
                return false;
            }
            ReportsGroupList = list;
            return true;
        }

        private async Task<bool> InitReportsAsync()
        {
            int groupID = Properties.Settings.Default.SelectedReportsGroupId;

            var list = new List<Model.EmcosReport>();
            try
            {
                string paramGetReports = string.Format("callerUID=INIT_ID_1/RP_TYPE_ID_{0}&instance=_level1.scene.spContentHolder6.PANEL_RP_TREE.spContentHolder0.RP_TREE.spContentHolder&RP_PUBLIC=1&RP_TYPE_ID={0}&__const=TYPE=RP&datablock=RP&action=GET",
                    groupID);

                string url = @"{0}scripts/reports.asp";
                string data = await ServiceHelper.ExecuteFunctionAsync(ServiceHelper.MakeRequestAsync, url, paramGetReports, true, (answer) => ServiceHelper.DecodeAnswer(answer));
                //string data = "&RP_ID_0=679&RP_TYPE_ID_0=72&RP_NAME_0=Выборка точек по группе&RP_PUBLIC_0=1&RP_DESCRIPTION_0=&RPF_ID_0=1&RP_LOG_ENABLED_0=0&USER_NAME_0=System user EMCOS&TYPE_0=RP&RP_ID_1=675&RP_TYPE_ID_1=72&RP_NAME_1=Для расчета балансов ПС до 35кВ&RP_PUBLIC_1=1&RP_DESCRIPTION_1=&RPF_ID_1=2&RP_LOG_ENABLED_1=0&USER_NAME_1=Щелухин Д.Д.&TYPE_1=RP&RP_ID_2=698&RP_TYPE_ID_2=72&RP_NAME_2=Для расчета балансов ПС 35-770кВ&RP_PUBLIC_2=1&RP_DESCRIPTION_2=&RPF_ID_2=2&RP_LOG_ENABLED_2=1&USER_NAME_2=System user EMCOS&TYPE_2=RP&result=0&recordCount=3";
                if (data.Contains("result=0") == false)
                    return false;

                var records = Utils.ParseRecords(data);
                if (records == null)
                    return false;
                
                foreach (var nvc in records)
                    if (nvc.Get("TYPE") == Model.EmcosReport.TYPE)
                    {
                        Model.EmcosReport element = new Model.EmcosReport();
                        for (int i = 0; i < nvc.Count; i++)
                        {
                            #region Разбор полей
                            int intValue = 0;
                            switch (nvc.GetKey(i))
                            {
                                case "RP_ID":
                                    int.TryParse(nvc[i], out intValue);
                                    element.RP_ID = intValue;
                                    break;
                                case "RP_TYPE_ID":
                                    int.TryParse(nvc[i], out intValue);
                                    element.RP_TYPE_ID = intValue;
                                    break;
                                case "RP_NAME":
                                    element.RP_NAME = nvc[i];
                                    break;
                                case "RP_PUBLIC":
                                    element.RP_PUBLIC = nvc[i] == "1";
                                    break;
                                case "RP_DESCRIPTION":
                                    element.RP_DESCRIPTION = nvc[i];
                                    break;
                                case "RPF_ID":
                                    int.TryParse(nvc[i], out intValue);
                                    element.RPF_ID = intValue;
                                    break;
                                case "RP_LOG_ENABLED":
                                    element.RP_LOG_ENABLED = nvc[i] == "1";
                                    break;
                                case "USER_NAME":
                                    element.USER_NAME = nvc[i];
                                    break;
                            }
                            #endregion
                        }
                        list.Add(element);
                    }
            }
            catch
            {
                return false;
            }
            ReportsList = list;
            /*var selected = Properties.Settings.Default.SelectedReport;
            Model.EmcosReport r = App.Base64StringToObject<Model.EmcosReport>(selected);
            if (r != null)
                cmbReports.SelectedItem = list.Where(i => i.RP_ID == r.RP_ID).FirstOrDefault();*/
            return true;
        }

        private async Task<bool> InitDepartamentsAsync()
        {
            IList<ListPoint> result = new List<ListPoint>();
            try
            {
                IList<ListPoint> feses = await ServiceHelper.CreatePointsListAsync(new ListPoint { Id = 53, TypeCode = "REGION", Name = "РУП Гродноэнерго" });
                //ServiceHelper.GetPointsList("&GR_ID_0=5102&GRC_NR_0=&GRC_DESC_0=&GR_CODE_0=AUTO_DELETE_OLD_DATA&GR_NAME_0=Автоматическое удаление&USER_NAME_0=System user EMCOS&IS_PUBLIC_0=1&IS_PUBLIC_TXT_0=Да&GR_TYPE_ID_0=47&GR_TYPE_NAME_0=Служебная&GR_TYPE_CODE_0=Service&PARENT_0=0&HASCHILDS_0=1&GRC_BT_0=&TYPE_0=GROUP&GR_ID_1=3482&GRC_NR_1=&GRC_DESC_1=&GR_CODE_1=Import_ata&GR_NAME_1=Импортированные данные&USER_NAME_1=System user EMCOS&IS_PUBLIC_1=1&IS_PUBLIC_TXT_1=Да&GR_TYPE_ID_1=6&GR_TYPE_NAME_1=Группы пользователей&GR_TYPE_CODE_1=UD&PARENT_1=0&HASCHILDS_1=1&GRC_BT_1=&TYPE_1=GROUP&GR_ID_2=3067&GRC_NR_2=&GRC_DESC_2=&GR_CODE_2=1.МП&GR_NAME_2=Межгосударственные перетоки&USER_NAME_2=System user EMCOS&IS_PUBLIC_2=1&IS_PUBLIC_TXT_2=Да&GR_TYPE_ID_2=6&GR_TYPE_NAME_2=Группы пользователей&GR_TYPE_CODE_2=UD&PARENT_2=0&HASCHILDS_2=1&GRC_BT_2=&TYPE_2=GROUP&GR_ID_3=55&GRC_NR_3=&GRC_DESC_3=&GR_CODE_3=14.Генерация РУП&GR_NAME_3=Генерация РУП&USER_NAME_3=System user EMCOS&IS_PUBLIC_3=1&IS_PUBLIC_TXT_3=Да&GR_TYPE_ID_3=9&GR_TYPE_NAME_3=Генерация&GR_TYPE_CODE_3=GENERATION&PARENT_3=0&HASCHILDS_3=1&GRC_BT_3=&TYPE_3=GROUP&GR_ID_4=8668&GRC_NR_4=&GRC_DESC_4=&GR_CODE_4=14.Лимиты&GR_NAME_4=Лимиты&USER_NAME_4=System user EMCOS&IS_PUBLIC_4=1&IS_PUBLIC_TXT_4=Да&GR_TYPE_ID_4=8&GR_TYPE_NAME_4=РУП&GR_TYPE_CODE_4=RUP&PARENT_4=0&HASCHILDS_4=1&GRC_BT_4=&TYPE_4=GROUP&GR_ID_5=1667&GRC_NR_5=&GRC_DESC_5=&GR_CODE_5=14.ММП&GR_NAME_5=Межсистемные перетоки&USER_NAME_5=System user EMCOS&IS_PUBLIC_5=1&IS_PUBLIC_TXT_5=Да&GR_TYPE_ID_5=27&GR_TYPE_NAME_5=Сальдо&GR_TYPE_CODE_5=Saldo&PARENT_5=0&HASCHILDS_5=1&GRC_BT_5=&TYPE_5=GROUP&GR_ID_6=6712&GRC_NR_6=&GRC_DESC_6=&GR_CODE_6=14.Отпуск с шин 110 кВ РУП&GR_NAME_6=Отпуск с шин 110 кВ РУП&USER_NAME_6=Руслан Петько&IS_PUBLIC_6=1&IS_PUBLIC_TXT_6=Да&GR_TYPE_ID_6=8&GR_TYPE_NAME_6=РУП&GR_TYPE_CODE_6=RUP&PARENT_6=0&HASCHILDS_6=1&GRC_BT_6=&TYPE_6=GROUP&GR_ID_7=2732&GRC_NR_7=&GRC_DESC_7=&GR_CODE_7=14.Поступление в сеть 6-10 кВ РУП&GR_NAME_7=Поступление в сеть 6-10 кВ РУП&USER_NAME_7=System user EMCOS&IS_PUBLIC_7=1&IS_PUBLIC_TXT_7=Да&GR_TYPE_ID_7=8&GR_TYPE_NAME_7=РУП&GR_TYPE_CODE_7=RUP&PARENT_7=0&HASCHILDS_7=1&GRC_BT_7=&TYPE_7=GROUP&GR_ID_8=3022&GRC_NR_8=&GRC_DESC_8=&GR_CODE_8=14.Потери РУП&GR_NAME_8=Потери РУП&USER_NAME_8=Гржешкевич А.Ч.&IS_PUBLIC_8=1&IS_PUBLIC_TXT_8=Да&GR_TYPE_ID_8=8&GR_TYPE_NAME_8=РУП&GR_TYPE_CODE_8=RUP&PARENT_8=0&HASCHILDS_8=1&GRC_BT_8=&TYPE_8=GROUP&GR_ID_9=342&GRC_NR_9=&GRC_DESC_9=&GR_CODE_9=14.СОК Энергетик&GR_NAME_9=СОК Энергетик&USER_NAME_9=System user EMCOS&IS_PUBLIC_9=1&IS_PUBLIC_TXT_9=Да&GR_TYPE_ID_9=14&GR_TYPE_NAME_9=Подстанция&GR_TYPE_CODE_9=SUBSTATION&PARENT_9=0&HASCHILDS_9=1&GRC_BT_9=&TYPE_9=GROUP&GR_ID_10=8825&GRC_NR_10=&GRC_DESC_10=&GR_CODE_10=14.Теплосети&GR_NAME_10=Теплосети&USER_NAME_10=Руслан Петько&IS_PUBLIC_10=1&IS_PUBLIC_TXT_10=Да&GR_TYPE_ID_10=12&GR_TYPE_NAME_10=ФЭС&GR_TYPE_CODE_10=FES&PARENT_10=0&HASCHILDS_10=1&GRC_BT_10=&TYPE_10=GROUP&GR_ID_11=58&GRC_NR_11=&GRC_DESC_11=&GR_CODE_11=140&GR_NAME_11=Волковыские ЭС&USER_NAME_11=System user EMCOS&IS_PUBLIC_11=1&IS_PUBLIC_TXT_11=Да&GR_TYPE_ID_11=12&GR_TYPE_NAME_11=ФЭС&GR_TYPE_CODE_11=FES&PARENT_11=0&HASCHILDS_11=1&GRC_BT_11=&TYPE_11=GROUP&GR_ID_12=54&GRC_NR_12=&GRC_DESC_12=&GR_CODE_12=141&GR_NAME_12=Гродненские ЭС&USER_NAME_12=System user EMCOS&IS_PUBLIC_12=1&IS_PUBLIC_TXT_12=Да&GR_TYPE_ID_12=12&GR_TYPE_NAME_12=ФЭС&GR_TYPE_CODE_12=FES&PARENT_12=0&HASCHILDS_12=1&GRC_BT_12=&TYPE_12=GROUP&GR_ID_13=192&GRC_NR_13=&GRC_DESC_13=&GR_CODE_13=141031.ТЭЦ-23&GR_NAME_13=ТЭЦ-23&USER_NAME_13=System user EMCOS&IS_PUBLIC_13=1&IS_PUBLIC_TXT_13=Да&GR_TYPE_ID_13=12&GR_TYPE_NAME_13=ФЭС&GR_TYPE_CODE_13=FES&PARENT_13=0&HASCHILDS_13=1&GRC_BT_13=&TYPE_13=GROUP&GR_ID_14=59&GRC_NR_14=&GRC_DESC_14=&GR_CODE_14=142&GR_NAME_14=Лидские ЭС&USER_NAME_14=System user EMCOS&IS_PUBLIC_14=1&IS_PUBLIC_TXT_14=Да&GR_TYPE_ID_14=12&GR_TYPE_NAME_14=ФЭС&GR_TYPE_CODE_14=FES&PARENT_14=0&HASCHILDS_14=1&GRC_BT_14=&TYPE_14=GROUP&GR_ID_15=60&GRC_NR_15=&GRC_DESC_15=&GR_CODE_15=143&GR_NAME_15=Ошмянские ЭС&USER_NAME_15=System user EMCOS&IS_PUBLIC_15=1&IS_PUBLIC_TXT_15=Да&GR_TYPE_ID_15=12&GR_TYPE_NAME_15=ФЭС&GR_TYPE_CODE_15=FES&PARENT_15=0&HASCHILDS_15=1&GRC_BT_15=&TYPE_15=GROUP&GR_ID_16=57&GRC_NR_16=&GRC_DESC_16=&GR_CODE_16=145.Промпредприятия РУП&GR_NAME_16=Промышленные предприятия РУП&USER_NAME_16=System user EMCOS&IS_PUBLIC_16=1&IS_PUBLIC_TXT_16=Да&GR_TYPE_ID_16=11&GR_TYPE_NAME_16=Промпредпрятия&GR_TYPE_CODE_16=ENTERPRISE&PARENT_16=0&HASCHILDS_16=1&GRC_BT_16=&TYPE_16=GROUP&GR_ID_17=3068&GRC_NR_17=&GRC_DESC_17=&GR_CODE_17=2.МП&GR_NAME_17=Межсистемные перетоки&USER_NAME_17=System user EMCOS&IS_PUBLIC_17=1&IS_PUBLIC_TXT_17=Да&GR_TYPE_ID_17=6&GR_TYPE_NAME_17=Группы пользователей&GR_TYPE_CODE_17=UD&PARENT_17=0&HASCHILDS_17=1&GRC_BT_17=&TYPE_17=GROUP&POINT_ID_18=7782&GRP_NR_18=&GRP_DESC_18=&POINT_NAME_18=Генерация РУП&POINT_CODE_18=Генерация РУП.Р&POINT_ENABLED_TXT_18=Да&POINT_ENABLED_18=1&POINT_COMMERCIAL_18=Да&POINT_INTERNAL_18=Да&POINT_AUTO_READ_ENABLED_18=Да&ECP_NAME_18=Генерация&POINT_TYPE_NAME_18=Учет электричества&POINT_TYPE_CODE_18=ELECTRICITY&MOU_BT_18=&MOU_ET_18=&METER_NUMBER_18=&METER_TYPE_NAME_18=&GRP_BT_18=&PARENT_18=0&TYPE_18=POINT&POINT_ID_19=7792&GRP_NR_19=&GRP_DESC_19=&POINT_NAME_19=Генерация РУП&POINT_CODE_19=Генерация РУП.РР&POINT_ENABLED_TXT_19=Да&POINT_ENABLED_19=1&POINT_COMMERCIAL_19=Да&POINT_INTERNAL_19=Да&POINT_AUTO_READ_ENABLED_19=Да&ECP_NAME_19=Генерация&POINT_TYPE_NAME_19=Учет электричества&POINT_TYPE_CODE_19=ELECTRICITY&MOU_BT_19=&MOU_ET_19=&METER_NUMBER_19=&METER_TYPE_NAME_19=&GRP_BT_19=&PARENT_19=0&TYPE_19=POINT&DS_ID_20=17&DS_NAME_20=Test OB&DS_CODE_20=TOB1&DS_ENABLED_20=0&ENUM_ID_20=586&DS_V_20=1&PARENT_20=0&TYPE_20=DS&result=0&recordCount=21",
                //new ListPoint { Id = 53, TypeCode = "REGION", Name = "РУП Гродноэнерго" });
                // await ServiceHelper.CreatePointsListAsync(new ListPoint { Id = 53, TypeCode = "REGION", Name = "РУП Гродноэнерго" });
                if (feses != null && feses.Count > 0)
                {
                    feses = feses.Where(item => item.TypeCode == "FES").ToList();
                    foreach (ListPoint fes in feses)
                    {
                        result.Add(fes);
                        IList<ListPoint> reses = await ServiceHelper.CreatePointsListAsync(fes);
                        //ServiceHelper.GetPointsList("&GR_ID_0=2422&GRC_NR_0=&GRC_DESC_0=&GR_CODE_0=143.Генерация ОЭС&GR_NAME_0=Генерация ОЭС&USER_NAME_0=System user EMCOS&IS_PUBLIC_0=1&IS_PUBLIC_TXT_0=Да&GR_TYPE_ID_0=9&GR_TYPE_NAME_0=Генерация&GR_TYPE_CODE_0=GENERATION&PARENT_0=0&HASCHILDS_0=1&GRC_BT_0=&TYPE_0=GROUP&GR_ID_1=2423&GRC_NR_1=&GRC_DESC_1=&GR_CODE_1=143.МежФЭСовские перетоки ОЭС&GR_NAME_1=МежФЭСовские перетоки ОЭС&USER_NAME_1=System user EMCOS&IS_PUBLIC_1=1&IS_PUBLIC_TXT_1=Да&GR_TYPE_ID_1=27&GR_TYPE_NAME_1=Сальдо&GR_TYPE_CODE_1=Saldo&PARENT_1=0&HASCHILDS_1=1&GRC_BT_1=&TYPE_1=GROUP&GR_ID_2=2713&GRC_NR_2=&GRC_DESC_2=&GR_CODE_2=143.Поступление в сеть 6-10 кВ ОЭС&GR_NAME_2=Поступление в сеть 6-10 кВ ОЭС&USER_NAME_2=System user EMCOS&IS_PUBLIC_2=1&IS_PUBLIC_TXT_2=Да&GR_TYPE_ID_2=12&GR_TYPE_NAME_2=ФЭС&GR_TYPE_CODE_2=FES&PARENT_2=0&HASCHILDS_2=1&GRC_BT_2=&TYPE_2=GROUP&GR_ID_3=2588&GRC_NR_3=&GRC_DESC_3=&GR_CODE_3=143.Собственные нужды ОЭС&GR_NAME_3=Собственные нужды ОЭС&USER_NAME_3=System user EMCOS&IS_PUBLIC_3=1&IS_PUBLIC_TXT_3=Да&GR_TYPE_ID_3=12&GR_TYPE_NAME_3=ФЭС&GR_TYPE_CODE_3=FES&PARENT_3=0&HASCHILDS_3=1&GRC_BT_3=&TYPE_3=GROUP&GR_ID_4=317&GRC_NR_4=&GRC_DESC_4=&GR_CODE_4=1431&GR_NAME_4=Ивьевский РЭС&USER_NAME_4=System user EMCOS&IS_PUBLIC_4=1&IS_PUBLIC_TXT_4=Да&GR_TYPE_ID_4=13&GR_TYPE_NAME_4=РЭС&GR_TYPE_CODE_4=RES&PARENT_4=0&HASCHILDS_4=1&GRC_BT_4=&TYPE_4=GROUP&GR_ID_5=255&GRC_NR_5=&GRC_DESC_5=&GR_CODE_5=1432&GR_NAME_5=Сморгонский РЭС&USER_NAME_5=System user EMCOS&IS_PUBLIC_5=1&IS_PUBLIC_TXT_5=Да&GR_TYPE_ID_5=13&GR_TYPE_NAME_5=РЭС&GR_TYPE_CODE_5=RES&PARENT_5=0&HASCHILDS_5=1&GRC_BT_5=&TYPE_5=GROUP&GR_ID_6=294&GRC_NR_6=&GRC_DESC_6=&GR_CODE_6=1433&GR_NAME_6=Ошмянский РЭС&USER_NAME_6=System user EMCOS&IS_PUBLIC_6=1&IS_PUBLIC_TXT_6=Да&GR_TYPE_ID_6=13&GR_TYPE_NAME_6=РЭС&GR_TYPE_CODE_6=RES&PARENT_6=0&HASCHILDS_6=1&GRC_BT_6=&TYPE_6=GROUP&GR_ID_7=281&GRC_NR_7=&GRC_DESC_7=&GR_CODE_7=1434&GR_NAME_7=Островецкий РЭС&USER_NAME_7=System user EMCOS&IS_PUBLIC_7=1&IS_PUBLIC_TXT_7=Да&GR_TYPE_ID_7=13&GR_TYPE_NAME_7=РЭС&GR_TYPE_CODE_7=RES&PARENT_7=0&HASCHILDS_7=1&GRC_BT_7=&TYPE_7=GROUP&GR_ID_8=2752&GRC_NR_8=&GRC_DESC_8=&GR_CODE_8=1453.Промпредприятия ОЭС&GR_NAME_8=Промышленные предприятия ОЭС&USER_NAME_8=System user EMCOS&IS_PUBLIC_8=1&IS_PUBLIC_TXT_8=Да&GR_TYPE_ID_8=11&GR_TYPE_NAME_8=Промпредпрятия&GR_TYPE_CODE_8=ENTERPRISE&PARENT_8=0&HASCHILDS_8=1&GRC_BT_8=&TYPE_8=GROUP&result=0&recordCount=9",
                        //fes);
                        //await ServiceHelper.CreatePointsListAsync(fes);
                        if (reses != null && reses.Count > 0)
                        {
                            reses = reses.Where(item => item.TypeCode == "RES").ToList();
                            foreach (ListPoint res in reses)
                                result.Add(res);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            DepartamentsList = result;
            /*ListPoint dep = cmbDepartaments.SelectedValue as ListPoint;
            if (dep != null)
                cmbDepartaments.SelectedItem = result.Where(i => i.Id == dep.Id).FirstOrDefault();*/
            return true;
        }

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
}
