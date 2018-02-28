using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;

using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;

using Exp = System.Linq.Expressions;

using TMP.UI.Controls.WPF;
using TMP.DBF;
using TMP.Extensions;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using Model;
    using Extensions;
    using Settings = Properties.Settings;

    public class MainViewModel : BaseViewModel, IMainViewModel
    {
        private Theme[] themesList = new Theme[6]
        {
                        new Theme() { ShortName ="Aero", FullName = "/PresentationFramework.Aero;component/themes/aero.normalcolor.xaml"},
                        new Theme() { ShortName ="Classic", FullName = "/PresentationFramework.classic;component/themes/classic.xaml"},
                        new Theme() { ShortName ="Luna normalcolor", FullName = "/PresentationFramework.Luna;component/themes/luna.normalcolor.xaml"},
                        new Theme() { ShortName ="Luna homestead", FullName = "/PresentationFramework.Luna;component/themes/luna.homestead.xaml"},
                        new Theme() { ShortName ="Luna metallic", FullName = "/PresentationFramework.Luna;component/themes/luna.metallic.xaml"},
                        new Theme() { ShortName ="Royale", FullName = "/PresentationFramework.Royale;component/themes/royale.normalcolor.xaml"}
        };

        private Theme _selectTheme;
        private ObservableCollection<Theme> _themes;
        // ID constants
        private const Int32 m_baseID = 1001;


        private Data _data;

        private string _userAppDataPath;
        private string _executionPath;

        private Window _mainWindow;

        public MainViewModel()
        {
            // команда получения данных из базы данных Арамис
            CommandGetData = new DelegateCommand(() => GetDataFromAramisDB(),
                () => SelectedDepartament != null,
                "Обновить данные\nиз базы");
            // команда сохранения данных
            CommandSaveData = new DelegateCommand(() => SaveData(),
                () => SelectedDepartament != null,
                "Сохранить\nданные");

            CommandShowHelp = new DelegateCommand(() =>
            {
                ;
            },
            "Помощь");

            CommandShowPreferences = new DelegateCommand(() =>
            {
                string oldPath = Properties.Settings.Default.DataStorePath;

                var window = new PreferencesWindow(this);
                window.Owner = _mainWindow;
                window.ShowDialog();
                Settings.Default.Save();

                SelectedDepartament = Settings.Default.SelectedDepartament;

                if (String.Equals(oldPath, Properties.Settings.Default.DataStorePath) == false && SelectedDepartament != null && Data != null)
                    SaveData();

                Init();
            },
            "Параметры\nпрограммы");


            CommandShowMetersFilter = new DelegateCommand(() =>
            {

            },
            () => Data != null && Data.Meters != null,
            "Отфильтровать\nсписок счётчиков");

            CommandShowAll = new DelegateCommand(() =>
            {
                (new ViewCollectionWindow(new ViewCollectionViewModel(_data.Meters.ToList()))
                { Owner = App.Current.MainWindow }).ShowDialog();
            }, () => _data != null && _data.Meters != null, "Список всех\nсчётчиков");


            Quarters = new List<KeyValuePair<int, string>>(4);
            Quarters.Add(new KeyValuePair<int, string>(1, "I кв"));
            Quarters.Add(new KeyValuePair<int, string>(2, "II кв"));
            Quarters.Add(new KeyValuePair<int, string>(3, "III кв"));
            Quarters.Add(new KeyValuePair<int, string>(4, "IV кв"));

            int nowYear = DateTime.Now.Year;
            Years = new List<int>(5);
            for (int year = nowYear; year < nowYear + 5; year++)
                Years.Add(year);

            //
            CurrentMode = Mode.MetersList;

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Data = new Data()
                {
                    ChangesOfMeters = new List<ChangesOfMeters>(),
                    Date = DateTime.Today,
                    Departament = new Departament() { Name = "QWERTY" },
                    Meters = new List<Meter>(),
                    Version = new Version(1, 0),
                    Infos = new ObservableCollection<SummaryInfoItem>()
                };
                Init();
                return;
            }

            Init();

            _mainWindow = App.Current.MainWindow;
            if (_mainWindow != null)
                _mainWindow.Loaded += (s, e) =>
                {
                    MakeMenu();

                    // запуск в отдельном потоке операции чтения данных
                    if (SelectedDepartament != null)
                        Task.Factory.StartNew(() => LoadData());
                };

            // Получение сборки приложения
            var assm = System.Reflection.Assembly.GetEntryAssembly();
            var at = typeof(System.Reflection.AssemblyCompanyAttribute);
            object[] customAttributes = null;
            try
            {
                // Получение из метаданных коллекции аттрибутов
                customAttributes = assm.GetCustomAttributes(at, false);
            }
            catch (Exception) { }
            // Получения из метаданных аттрибута компания
            System.Reflection.AssemblyCompanyAttribute ct =
                          ((System.Reflection.AssemblyCompanyAttribute)(customAttributes[0]));
            // получение пути к данным программы в папке пользователя
            if (ct != null)
                _userAppDataPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    ct.Company,
                    assm.GetName().Name,
                    assm.GetName().Version.ToString()
                    );
            // путь к папке с исполняемым файлом программы
            _executionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        #region Private methods

        private void InUI(Action action)
        {
            _mainWindow.Dispatcher.BeginInvoke(action);
        }
        // Проверка наличия разрешения на запись в указанную папку
        private bool CheckPermissionToWriteToFolder(string path)
        {
            var permissionSet = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            var writePermission = new System.Security.Permissions.FileIOPermission(System.Security.Permissions.FileIOPermissionAccess.Write, path);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }
        // Чтение и десерилизация
        private void LoadData()
        {
            string dataFileName = SelectedDepartament.DataFileName;
            string fileName = System.IO.File.Exists(System.IO.Path.Combine(_executionPath, dataFileName)) ?
                System.IO.Path.Combine(_executionPath, dataFileName) :
                System.IO.Path.Combine(_userAppDataPath, dataFileName);

            if (File.Exists(fileName) == false) return;

            IsBusy = true;
            Status = "загрузка данных";

            System.Diagnostics.Debug.Assert(_mainWindow != null);

            try
            {
                InUI(() =>
                {
                    _mainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                    _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                });
                var task = new Task(() => Data = Serializer.GzJsonDeSerialize(fileName));
                task.ContinueWith(t =>
                    {
                        InUI(() =>
                        {
                            _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                            MessageBox.Show(
                                String.Format("Не удалось загрузить ранее сохраненные данные.\n{0}\n\nОбновите данные из базы.",
                                    App.GetExceptionDetails(t.Exception)),
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        });
                        IsBusy = false;
                        Status = String.Empty;
                    }, TaskContinuationOptions.OnlyOnFaulted);
                task.ContinueWith(t =>
                    {
                        IsBusy = false;
                        Status = String.Empty;
                        InUI(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None);
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                task.Start();
            }
            catch (Exception ex)
            {
#if DEBUG
                App.ToDebug(ex);
#endif
            }
        }
        // Серилизация и сохранение в файл
        private void SaveData()
        {
            if (SelectedDepartament == null || Data == null)
                throw new InvalidOperationException();
            string path = String.Empty;
            // в параметрах указан путь?
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.DataStorePath))
            {
                // нет, не указан, проверка наличия разрешения на запись в папку с исполняемым файлом программы
                path = _executionPath;
                if (CheckPermissionToWriteToFolder(path) == false)
                {
                    // проверка наличия разрешения на запись в папку AppData папки пользователя
                    path = _userAppDataPath;
                    if (CheckPermissionToWriteToFolder(path) == false)
                    {
                        MessageBox.Show(String.Format("Отсутствуют разрешения на запись в папку '{0}'.{1}Укажите папку, доступную на запись, в параметрах программы и сохраните данные.",
                        path, Environment.NewLine),
                        "Ошибка сохранения данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                        DataNotSaved = true;
                        return;
                    }
                }
            }
            else
                path = Properties.Settings.Default.DataStorePath;

            if (Directory.Exists(path) == false)
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (IOException ioex)
                {
#if DEBUG
                    App.ToDebug(ioex);
#endif
                    MessageBox.Show(String.Format("Не удалось создать папку '{0}'.{2}Данные не сохранены.{2}Устраните проблему и сохраните данные.\nПодробное описание ошибки:\n{1}",
                        path, App.GetExceptionDetails(ioex), Environment.NewLine),
                        "Ошибка сохранения данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DataNotSaved = true;
                    return;
                }
            string fileName = System.IO.Path.Combine(path, SelectedDepartament.DataFileName);

            System.Diagnostics.Debug.Assert(_mainWindow != null);

            IsBusy = true;
            Status = "сохранение данных";
            try
            {
                InUI(() =>
                {
                    _mainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                    _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                });
                var task = new Task(() => Serializer.GzJsonSerialize(Data, fileName));
                task.ContinueWith(t =>
                    {
                        InUI(() =>
                        {
                            _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                            MessageBox.Show("Не удалось сохранить данные." + Environment.NewLine + App.GetExceptionDetails(t.Exception), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        });
                        IsBusy = false;
                        Status = String.Empty;
                    }, TaskContinuationOptions.OnlyOnFaulted);
                task.ContinueWith(t =>
                    {
                        IsBusy = false;
                        RaisePropertyChanged("DataVersion");
                        RaisePropertyChanged("DataSize");
                        RaisePropertyChanged("DataDate");
                        Status = String.Empty;
                        InUI(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None);

                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                task.Start();
            }
            catch (Exception ex)
            {
#if DEBUG
                App.ToDebug(ex);
#endif
            }
        }
        // Загрузка необходимых данных из базы данных Арамис
        private void GetDataFromAramisDB()
        {
            if (SelectedDepartament == null)
                throw new ArgumentNullException("Departament");

            if (String.IsNullOrWhiteSpace(SelectedDepartament.Path))
            {
                MessageBox.Show(
                    String.Format("Не задан путь к папке с программой 'Арамис' для подразделения '{1}'!{0}Для этого откройте параметры программы, выберите раздел 'Расположение данных' и{0}укажите путь и повторите операцию снова.",
                    Environment.NewLine,
                    SelectedDepartament.Name), "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Directory.Exists(SelectedDepartament.Path) == false)
            {
                MessageBox.Show(String.Format(
                    "Указанный в параметрах путь к папке с программой 'Арамис' для подразделения '{1}' не доступен!",
                    SelectedDepartament.Name),
                    "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Directory.Exists(Path.Combine(SelectedDepartament.Path, "DBF")) == false)
            {
                MessageBox.Show("В папке с программой 'Арамис' не найдена папка 'DBF'!", "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Directory.Exists(Path.Combine(SelectedDepartament.Path, "DBFC")) == false)
            {
                MessageBox.Show("В папке с программой 'Арамис' не найдена папка 'DBFC'!", "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            Status = "получение данных из базы данных" + Environment.NewLine + "программы 'Арамис'" + Environment.NewLine + "подразделения '" + SelectedDepartament.Name + "'";

            _data = new Data();

            System.Diagnostics.Debug.Assert(_mainWindow != null);

            _mainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            _mainWindow.TaskbarItemInfo.ProgressValue = 0d;

            var task = new Task<Data>(() => GetData(SelectedDepartament));
            task.ContinueWith(t =>
                {
                    InUI(() =>
                    {
                        _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        MessageBox.Show("Ошибка при получении данных." + Environment.NewLine + App.GetExceptionDetails(t.Exception), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    });

                    Data = null;

                    IsBusy = false;
                    Status = String.Empty;
                    DetailedStatus = String.Empty;
                }, TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t =>
                {
                    Data = t.Result;
                    _data.Date = DateTime.Now;

                    InUI(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None);

                    IsBusy = false;
                    Status = String.Empty;
                    DetailedStatus = String.Empty;
                })
            .ContinueWith(t => SaveData(), TaskContinuationOptions.OnlyOnRanToCompletion);
            task.Start();
        }
        private Data GetData(Departament departament)
        {
            if (departament == null)
                throw new ArgumentNullException("Departament");

            string pathDBF = Path.Combine(departament.Path, "DBF");
            string pathDBFC = Path.Combine(departament.Path, "DBFC");

            System.Diagnostics.Debug.Assert(_mainWindow != null);

            InUI(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate);

            Data data = new Data();
            data.Departament.Path = departament.Path;
            try
            {

                #region чтение таблиц
                DetailedStatus = "чтение таблиц";
                DataTable dtKARTSCH = ParseDBF.ReadDBF(Path.Combine(pathDBF, "KARTSCH.DBF"));
                // справочник типов счетчиков
                DataTable dtKARTTSCH = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KARTTSCH.DBF"));
                DataTable dtKARTAB = ParseDBF.ReadDBF(Path.Combine(pathDBF, "KARTAB.DBF"));
                // справочник мест установки
                DataTable dtASVIDYST = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "ASVIDYST.DBF"));

                // Справочник  населенных пунктов
                DataTable dtKartTn = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KartTn.DBF"));
                // Справочник  улиц
                DataTable dtKartSt = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KartSt.DBF"));
                // Справочник  токоприемников
                DataTable dtKartTpr = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KartTpr.DBF"));
                // Справочник  использования
                DataTable dtKartIsp = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KartIsp.DBF"));
                // Справочник  категорий
                DataTable dtKartKat = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KartKat.DBF"));

                // подстанции
                DataTable dtKartps = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "Kartps.DBF"));
                // фидера 10 кВ
                DataTable dtKartfid = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "Kartfid.DBF"));
                // пс 10 кВ
                DataTable dtKartktp_old = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "Kartktp.DBF"));

                // замены
                DataTable dtAssmena = ParseDBF.ReadDBF(Path.Combine(pathDBF, "Assmena.DBF"));

                // наименование РЭС
                DataTable dtKartPd = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KartPd.DBF"));

                #endregion

                #region обработка таблиц
                DetailedStatus = "обработка таблиц";
                // так как поле "КОД_ТП" в таблице Kartktp имеет тип Numeric, а поле "НОМЕР_ТП" в таблице KARTAB тип Character,
                // создаем новую таблицу на основе Kartktp
                DataTable dtKartktp = new DataTable(dtKartktp_old.TableName);
                string[] ktpColumns = { "ФИДЕР", "НОМЕР_ТП", "НАИМ_ТП", "КОД_ТП", "ПОДСТАНЦИЯ", "РЭС", "НАИМЕНОВ" };
                foreach (string field in ktpColumns)
                    dtKartktp.Columns.Add(new DataColumn(field, typeof(String)));

                dtKartktp.BeginLoadData();
                foreach (DataRow row in dtKartktp_old.Rows)
                {
                    DataRow r = dtKartktp.NewRow();
                    foreach (string field in ktpColumns)
                        r[field] = row[field].ToString();
                    dtKartktp.Rows.Add(r);
                }
                dtKartktp.EndLoadData();

                // обрезание пробелов в поле "НОМЕР_ТП" таблицы KARTAB
                dtKARTAB.BeginLoadData();
                foreach (DataRow row in dtKARTAB.Rows)
                {
                    row["НОМЕР_ТП"] = (row["НОМЕР_ТП"].ToString()).Trim();
                }
                dtKARTAB.EndLoadData();
                #endregion

                #region создание набора данных
                DetailedStatus = "создание набора данных";
                DataSet ds = new DataSet();
                ds.Tables.Add(dtKARTAB);
                ds.Tables.Add(dtKARTSCH);
                ds.Tables.Add(dtKARTTSCH);
                ds.Tables.Add(dtASVIDYST);
                ds.Tables.Add(dtKartTn);
                ds.Tables.Add(dtKartSt);
                ds.Tables.Add(dtKartTpr);
                ds.Tables.Add(dtKartIsp);
                ds.Tables.Add(dtKartKat);
                ds.Tables.Add(dtKartps);
                ds.Tables.Add(dtKartfid);
                ds.Tables.Add(dtKartktp);

                ds.Tables.Add(dtAssmena);

                ds.Tables.Add(dtKartPd);

                #endregion

                #region указание зависимостей
                DetailedStatus = "указание зависимостей";
                ds.Relations.Add("счетчики", dtKARTAB.Columns["LIC_SCH"], dtKARTSCH.Columns["LIC_SCH"], false);
                // счетчики
                ds.Relations.Add("счетчики_типы", dtKARTSCH.Columns["COD_TSCH"], dtKARTTSCH.Columns["COD_TSCH"], false);
                // населенные пункты
                ds.Relations.Add("населенный_пункт", dtKARTAB.Columns["COD_TN"], dtKartTn.Columns["COD_TN"], false);
                // улицы
                ds.Relations.Add("улицы", dtKARTAB.Columns["COD_ST"], dtKartSt.Columns["COD_ST"], false);
                // токоприемники
                ds.Relations.Add("токоприемники", dtKARTAB.Columns["COD_TPR"], dtKartTpr.Columns["COD_TPR"], false);
                // использование
                ds.Relations.Add("использование", dtKARTAB.Columns["COD_ISP"], dtKartIsp.Columns["COD_ISP"], false);
                // категория
                ds.Relations.Add("категория", dtKARTAB.Columns["COD_KAT"], dtKartKat.Columns["COD_KAT"], false);

                // подстанция
                ds.Relations.Add("подстанция", dtKartfid.Columns["ПОДСТАНЦИЯ"], dtKartps.Columns["ПОДСТАНЦИЯ"], false);
                // фидер10
                ds.Relations.Add("фидер10", dtKARTAB.Columns["FIDER10"], dtKartfid.Columns["ФИДЕР"], false);
                // тп
                ds.Relations.Add("тп", dtKARTAB.Columns["НОМЕР_ТП"], dtKartktp.Columns["КОД_ТП"], false);

                // место установки
                ds.Relations.Add("место_установки", dtKARTSCH.Columns["COD_SS"], dtASVIDYST.Columns["COD_SS"], false);

                // тип счетчика в таблице замен
                ds.Relations.Add("замены_тип_счетчика", dtAssmena.Columns["ТИП_СЧЕТЧ"], dtKARTTSCH.Columns["COD_TSCH"], false);
                ds.Relations.Add("замены_инф_по_счетчику", dtAssmena.Columns["ЛИЦ_СЧЕТ"], dtKARTSCH.Columns["LIC_SCH"], false);
                ds.Relations.Add("замены_абонент", dtAssmena.Columns["ЛИЦ_СЧЕТ"], dtKARTAB.Columns["LIC_SCH"], false);
                #endregion

                Func<object, string> _getString = value => value.ToString().Trim();
                Func<object, int, int, byte, byte> _getByte = (value, startPos, length, defaultValue) =>
                {
                    string s = value.ToString();
                    if (String.IsNullOrWhiteSpace(s) || s.Length <= length)
                        return defaultValue;
                    try
                    {
                        var substring = s.Substring(startPos, length);
                        return Convert.ToByte(substring);
                    }
                    catch { return defaultValue; }
                };
                Func<object, int, int, int> _getInt = (value, startPos, length) =>
                {
                    string s = value.ToString();
                    if (String.IsNullOrWhiteSpace(s) || s.Length <= length)
                        return 0;
                    try
                    {
                        var substring = s.Substring(startPos, length);
                        return Convert.ToInt32(substring);
                    }
                    catch { return 0; }
                };
                Func<object, DateTime?> _getDate = value =>
                {
                    string valueAsString = value.ToString().Trim();
                    DateTime d = default(DateTime);
                    bool result = true;
                    try
                    {
                        result = DateTime.TryParse(valueAsString, out d);
                    }
                    catch
                    {
                        result = false;
                    }
                    return result ? new Nullable<DateTime>(d) : null;
                };
                Func<DataRow, string, string, string> _getChildRelationValue = (row, relationName, fieldName) =>
                {
                    DataRow[] rows = row.GetChildRows(relationName);
                    if (rows == null || rows.Length == 0)
                        return String.Empty;
                    else
                    {
                        if (rows.Length > 1)
                            System.Diagnostics.Debugger.Break();
                        return _getString(rows[0][fieldName]);
                    }
                };

                int totalRows = 0;
                int processedRows = 0;
                Action<int> _updateUI = value =>
                {
                    if (value % 100 == 0)
                        DetailedStatus = String.Format("обработка записей: {0} из {1}, {2:N1}%",
                        value, totalRows, 100d * value / totalRows);

                    InUI(() => _mainWindow.TaskbarItemInfo.ProgressValue = 100d * value / totalRows);
                };

                InUI(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal);



                // получение названия РЭС
                DataTable kartPdTable = ds.Tables[dtKartPd.TableName];
                if (kartPdTable.Rows != null && kartPdTable.Rows.Count > 0)
                    data.Departament.Name = _getString(kartPdTable.Rows[0]["PNPD"].ToString());

                #region проход по всем абонентам

                // получение таблицы с абонентами
                DataTable AbonentsTable = ds.Tables[dtKARTAB.TableName];
                totalRows = AbonentsTable.Rows.Count;

                List<Meter> meters = new List<Meter>();

                List<string> errors = new List<string>();

                foreach (DataRow abonentRow in AbonentsTable.Rows)
                {
                    DataRow[] abonentMeters = abonentRow.GetChildRows("счетчики");
                    foreach (DataRow meterRow in abonentMeters)
                    {
                        Meter meter = new Meter();

                        if (String.IsNullOrWhiteSpace(abonentRow["LIC_SCH"].ToString().Trim()))
                            continue;

                        #region Meter info
                        meter.Договор = String.IsNullOrWhiteSpace(abonentRow["DOG"].ToString()) == false;
                        meter.Принадлежность_РУП = Convert.ToBoolean(abonentRow["PR_MO"].ToString());
                        meter.Баланс_РУП = abonentRow["PRIZNAK"].ToString().Substring(13, 1) == "1";
                        meter.Горячее_водоснабжение = abonentRow["PRIZNAK"].ToString().Substring(0, 1) == "1";
                        meter.Имеющие_природный_газ = abonentRow["PRIZNAK"].ToString().Substring(4, 1) == "1";
                        meter.Многодетная_семья = abonentRow["PRIZNAK"].ToString().Substring(2, 1) == "1";
                        meter.Выносное_АСКУЭ = abonentRow["PRIZNAK"].ToString().Substring(14, 1) == "1";

                        meter.Лицевой = Convert.ToUInt64(abonentRow["LIC_SCH"]);
                        meter.ФИО = String.Join(" ", _getString(abonentRow["FAM"]), _getString(abonentRow["NAME"]), _getString(abonentRow["OTCH"]));

                        meter.Населённый_пункт = _getChildRelationValue(abonentRow, "населенный_пункт", "TOWN");
                        meter.Адрес = String.Join(", ",
                            meter.Населённый_пункт,
                            _getChildRelationValue(abonentRow, "улицы", "STREET"),
                            _getString(abonentRow["HOME"]),
                            _getString(abonentRow["KV"]));

                        meter.Шифр_тарифа = _getString(abonentRow["ШИФР"]);

                        meter.Категория = _getChildRelationValue(abonentRow, "категория", "KATEGAB");

                        meter.Расположение = _getChildRelationValue(abonentRow, "токоприемники", "TPRIEM");
                        meter.Использование = _getChildRelationValue(abonentRow, "использование", "ISPIEM");

                        meter.Дата_оплаты = _getDate(abonentRow["DATE_R"]);
                        meter.Дата_обхода = _getDate(abonentRow["DATE_KON"]);

                        meter.Посл_показание_обхода = Convert.ToDouble(abonentRow["DATA_KON"], System.Globalization.CultureInfo.InvariantCulture);

                        meter.Аскуэ = Convert.ToBoolean(abonentRow["ASKUE"]);
                        meter.SMS = _getString(abonentRow["SMS"]);

                        var fider10Rows = abonentRow.GetChildRows("фидер10");
                        if (fider10Rows.Length > 0)
                        {
                            var fider10 = fider10Rows[0];
                            string s = _getString(fider10["ФИДЕР"]);
                            meter.Фидер10 = String.IsNullOrWhiteSpace(s) ? String.Empty : _getString(fider10["НАИМЕНОВ"]) + "-" + s;
                            meter.Подстанция = _getChildRelationValue(fider10, "подстанция", "НАИМЕНОВ");
                        }
                        else
                        {
                            errors.Add(String.Format("Не найдена информация по фидеру 10 кВ: л/с {0}, код фидера {1}", meter.Лицевой, _getString(abonentRow["FIDER10"])));
                        }

                        meter.ТП = _getChildRelationValue(abonentRow, "тп", "НАИМЕНОВ");
                        meter.Фидер04 = _getString(abonentRow["ФИДЕР"]);
                        meter.Опора = _getString(abonentRow["НОМЕР_ОПОР"]);

                        #endregion

                        #region meter info
                        meter.Номер_счетчика = _getString(meterRow["N_SCH"]);
                        meter.Квартал_поверки = _getByte(meterRow["G_PROV"], 0, 1, 1);
                        meter.Год_поверки = _getByte(meterRow["G_PROV"], 2, 2, 0);

                        meter.Дата_установки = _getDate(meterRow["DUSTAN"]);
                        meter.Год_выпуска = Convert.ToInt32(meterRow["GODVYPUSKA"].ToString());

                        meter.Посл_показание = Convert.ToDouble(meterRow["DATA_NEW"], System.Globalization.CultureInfo.InvariantCulture);

                        var meterInfos = meterRow.GetChildRows("счетчики_типы");
                        if (meterInfos != null && meterInfos.Length != 0)
                        {
                            DataRow meterInfo = meterInfos[0];
                            meter.Тип_счетчика = _getString(meterInfo["NAME"]);
                            meter.Ампераж = _getString(meterInfo["TOK"]);
                            meter.Период_поверки = Convert.ToByte(meterInfo["PERIOD"]);
                            meter.Фаз = Convert.ToByte(meterInfo["ФАЗ"]);
                            meter.Принцип = _getString(meterInfo["TIP"]);
                        }

                        meter.Место_установки = _getChildRelationValue(meterRow, "место_установки", "MESTO");
                        #endregion
                        meters.Add(meter);
                    }
                    _updateUI(++processedRows);
                }
                data.Meters = new ReadOnlyCollection<Meter>(meters);

                #endregion

                #region Просмотр замен

                // получение таблицы
                DataTable changeOfMeterTable = ds.Tables[dtAssmena.TableName];

                processedRows = 0;
                totalRows = changeOfMeterTable.Rows.Count;

                List<ChangesOfMeters> changes = new List<ChangesOfMeters>();
                foreach (DataRow changeOfMeterRow in changeOfMeterTable.Rows)
                {
                    ChangesOfMeters change = new ChangesOfMeters();

                    DataRow[] meterTypes = changeOfMeterRow.GetChildRows("замены_тип_счетчика");
                    DataRow[] meterInfos = changeOfMeterRow.GetChildRows("замены_инф_по_счетчику");
                    DataRow[] abonents = changeOfMeterRow.GetChildRows("замены_абонент");

                    change.Тип_снятого_счетчика = _getChildRelationValue(changeOfMeterRow, "замены_тип_счетчика", "NAME");
                    if (meterInfos != null && meterInfos.Length != 0)
                    {
                        DataRow meterInfo = meterInfos[0];
                        change.Квартал_поверки_снятого = _getByte(meterInfo["G_PROV"], 0, 1, 1);
                        change.Год_поверки_снятого = _getByte(meterInfo["G_PROV"], 2, 2, 0);
                        change.Год_выпуска_снятого = (uint)_getInt(meterInfo["GODVYPUSKA"], 0, 4);
                        change.Дата_установки_снятого = _getDate(meterInfo["DATE_UST"]);
                    }
                    if (abonents != null && abonents.Length != 0)
                    {
                        DataRow abonent = abonents[0];
                        change.Населённый_пункт = _getChildRelationValue(abonent, "населенный_пункт", "TOWN");
                        string name = _getString(abonent["NAME"]);
                        string otch = _getString(abonent["OTCH"]);
                        change.ФИО = _getString(abonent["FAM"]) +
                            " " + name.FirstOrDefault() + "." + otch.FirstOrDefault() + ".";
                    }
                    change.Лицевой = Convert.ToUInt64(changeOfMeterRow["ЛИЦ_СЧЕТ"]);
                    change.Номер_снятого_счетчика = _getString(changeOfMeterRow["НОМЕР_СНЯТ"]);
                    change.Показание_снятого = Convert.ToDouble(changeOfMeterRow["ПОКАЗ_СНЯТ"], System.Globalization.CultureInfo.InvariantCulture);
                    change.Номер_установленного_счетчика = _getString(changeOfMeterRow["НОМЕР_УСТ"]);
                    change.Показание_установленного = Convert.ToDouble(changeOfMeterRow["ПОКАЗ_УСТ"], System.Globalization.CultureInfo.InvariantCulture);
                    change.Дата_замены = _getDate(changeOfMeterRow["ДАТА_ЗАМЕН"]);
                    change.Номер_акта = _getString(changeOfMeterRow["НОМЕР_АКТА"]);
                    change.Фамилия = _getString(changeOfMeterRow["ФАМИЛИЯ"]);
                    change.Причина = _getString(changeOfMeterRow["ПРИЧИНА"]);

                    changes.Add(change);

                    _updateUI(++processedRows);
                }

                data.ChangesOfMeters = changes;
                #endregion

                if (errors.Count > 0)
                    InUI(() => MessageBox.Show(String.Format("При чтении базы обнаружено {0} ошибок.\nПервые 20:\n{1}", errors.Count, String.Join("\n", errors.Take(20))), "Получение данных", MessageBoxButton.OK, MessageBoxImage.Warning));

                //создание сводной информации
                data.Infos = BuildSummaryInfo(data.Meters);
            }
            finally
            {
                DetailedStatus = null;
            }
            return data;
        }

        private ObservableCollection<SummaryInfoItem> BuildSummaryInfo(ICollection<Meter> meters)
        {
            if (meters == null) return null;

            int totalRows = ModelHelper.MeterSummaryInfoProperties.Count;
            int processedRows = 0;
            Action<int> _updateUI = value =>
            {
                if (value % 100 == 0)
                    DetailedStatus = String.Format("создание сводной информации: {0} из {1}, {2:N1}%",
                    value, totalRows, 100d * value / totalRows);

                InUI(() => _mainWindow.TaskbarItemInfo.ProgressValue = 100d * value / totalRows);
            };

            List<SummaryInfoItem> infos = new List<SummaryInfoItem>();
            // по всем свойствам
            foreach (Xceed.Wpf.DataGrid.TableField field in Settings.Default.SummaryInfoFields.OrderBy(f => f.DisplayOrder))
            {
                infos.Add(SummaryInfoHelper.BuildSummaryInfoItem(meters, field.Name));
                _updateUI(++processedRows);
            }

            return new ObservableCollection<SummaryInfoItem>(infos);
        }

        private void Init()
        {
            if (SelectedDepartament == null)
                _selectedDepartament = Settings.Default.SelectedDepartament;
            RaisePropertyChanged("SelectedDepartament");
            CreateViewNotLoadedData();
        }
        private void CreateView()
        {
            if (Data == null)
                return;
            IsAnalizingData = true;
            switch (_currentMode)
            {
                case Mode.MetersList:
                    _currentView = new Views.SummaryInfoView() { DataContext = new SummaryInfoViewViewModel(this, Data?.Infos) };
                    break;
                case Mode.ChangesOfMeters:
                    _currentView = new Views.ChangesOfMetersView() { DataContext = new ChangesOfMetersViewModel(Data?.ChangesOfMeters) };
                    break;
                case Mode.AbonentsBinding:
                    _currentView = new Views.AbonentsBindingView() { DataContext = new AbonentsBindingViewViewModel(Data.Meters) };
                    break;
                case Mode.Metrology:
                    _currentView = new Views.MetrologyView() { DataContext = new MetrologyViewViewModel() };
                    break;
                case Mode.MetersInfo:
                    this._currentView = new Views.MetersInfoView() { DataContext = new MetersInfoViewModel(this.Data) };
                    break;
                default:
                    throw new NotImplementedException("CreateView: unknown CurrentMode");
            }
            IsAnalizingData = false;  
        }

        private void CreateViewNotLoadedData()
        {
            if (Data != null)
                return;

            System.Windows.FrameworkElement content = (System.Windows.Controls.Border)App.Current.FindResource("PrepareDataTemplate");
            string msg;
            if (SelectedDepartament == null && Settings.Default.HasDepartaments)
            {
                msg = "Выберите необходимое подразделение\nвнизу из списка";
            }
            else
                if (SelectedDepartament == null)
                msg = "Для продолжения работы с программой,\nперейдите к параметрам.";
            else
                msg = "Данные не загружены.\nОбновите данные из базы.";
            content.DataContext = new { Status = msg };
            _currentView = content;
            RaisePropertyChanged("CurrentView");
        }
        /// <summary>
        /// Обновление статуса поверки счётчиков
        /// </summary>
        private void UpdateПоверка()
        {
            if (_data == null || _data.Meters == null || _data.Infos == null) return;

            Meter.Дата_сравнения_поверки = new DateTime(SelectedYear, SelectedQuarter.Key * 3 - 2, 1);

            IsAnalizingData = true;
            var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                SummaryInfoItem item = Data.Infos.Where(i => i.FieldName == "Поверен").FirstOrDefault();
                int index = Data.Infos.IndexOf(item);
                if (item != null)
                {
                    item = SummaryInfoHelper.BuildSummaryInfoItem(_data.Meters, "Поверен");
                    App.Current.Dispatcher.Invoke((Action)(() => Data.Infos[index] = item));
                }
            });
            task.ContinueWith(t =>
            {
                UpdateSummaryInfo();
                IsAnalizingData = false;
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void UpdateSummaryInfo()
        {
            if (ActiveViewModel is SummaryInfoViewViewModel vm)
            {
                ;
            }
        }

        #region Themes support

        private void ChangeTheme(Theme _SelectTheme)
        {
            LoadTheme(_SelectTheme.FullName);
        }

        private void LoadTheme(string themePath)
        {
            List<Uri> dictionaries = null;
            if (Application.Current.Resources.MergedDictionaries != null)
            {
                dictionaries = Application.Current.Resources.MergedDictionaries.Where(d => d.Source.OriginalString.StartsWith(@"/PresentationFramework") == false).Select(d => d.Source).ToList();
                // очищаем перед загрузкой темы
                App.Current.Resources.MergedDictionaries.Clear();
            }
            // загружаем необходимые для работы ресурсы
            // загружаем тему
            try
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themePath, UriKind.RelativeOrAbsolute) });
                if (dictionaries != null)
                    dictionaries.ForEach((uri) => Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri }));
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось применить тему.", _mainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MakeMenu()
        {
            if (Themes == null)
                Themes = new ObservableCollection<Theme>();
            foreach (var themeItem in themesList)
                Themes.Add(themeItem);

            if (Directory.Exists("Themes"))
            {
                FileInfo[] localthemes = new DirectoryInfo("Themes").GetFiles();
                foreach (var item in localthemes)
                {
                    Themes.Add(new Theme { ShortName = item.Name, FullName = item.FullName });
                }
            }

            //Create a new submenu structure
            IntPtr hMenu = SystemMenu.AddSysMenuSubMenu();
            if (hMenu != IntPtr.Zero)
            {
                // Build submenu items of hMenu
                uint index = 0;
                for (int i = 0; i < Themes.Count; i++)
                {
                    SystemMenu.AddSysMenuSubItem(Themes[i].ShortName, index, m_baseID + index, hMenu);
                    index++;
                }
                // Now add to main system menu (position 6)
                SystemMenu.AddSysMenuItem("Визуальная тема", 0, 6, hMenu);
                SystemMenu.AddSysMenuItem("-", 0, 7, IntPtr.Zero);
            }

            if (Properties.Settings.Default.Theme != null)
                SelectedTheme = Properties.Settings.Default.Theme;
            else
                SelectedTheme = Themes[0];

            // Attach our WndProc handler to this Window
            System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(_mainWindow).Handle);
            source.AddHook(new System.Windows.Interop.HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == (int)SystemMenu.WindowMessages.wmSysCommand)
            {
                int menuID = wParam.ToInt32();

                if (menuID <= (m_baseID + this.Themes.Count()))
                    if (menuID >= m_baseID)
                        this.SelectedTheme = Themes[menuID - m_baseID];
            }

            return IntPtr.Zero;
        }

        #endregion

        #endregion

        #region Properties

        private Model.Departament _selectedDepartament;

        public Model.Departament SelectedDepartament
        {
            get { return _selectedDepartament; }
            set
            {
                if (value == null || String.IsNullOrWhiteSpace(value.DataFileName) || value == _selectedDepartament)
                    return;
                _selectedDepartament = value;
                RaisePropertyChanged("SelectedDepartament");
                Settings.Default.SelectDepartamentAndStore(_selectedDepartament);
                Task.Factory.StartNew(() => LoadData());
            }
        }
        public IDataViewModel<IModel> ActiveViewModel { get; private set; }

        public Data Data
        {
            get { return _data; }
            private set
            {
                _data = value;
                RaisePropertyChanged("Data");
                RaisePropertyChanged("Meters");
                RaisePropertyChanged("DataVersion");
                RaisePropertyChanged("DataSize");
                RaisePropertyChanged("DataDate");
                RaisePropertyChanged("CurrentView");
            }
        }

        public ICollection<Meter> Meters
        {
            get { return (_data == null || _data.Meters == null) ? null : _data.Meters; }
        }
        public Version DataVersion
        {
            get { return (_data == null || _data.Version == null) ? null : _data.Version; }
        }
        public DateTime DataDate
        {
            get { return _data == null ? default(DateTime) : _data.Date; }
        }
        public string DataSize
        {
            get
            {
                string[] sizes = { "байт", "Кб", "Мб", "Гб" };
                int order = 0;
                long size = Serializer.DataSize;
                while (size >= 1024 && order + 1 < sizes.Length)
                {
                    order++;
                    size = size / 1024;
                }
                return String.Format("{0:0.##} {1}", size, sizes[order]);
            }
        }
        public ObservableCollection<SummaryInfoItem> SummaryInfo
        {
            get { return (_data == null || _data.Infos == null) ? null : _data.Infos; }
            set { _data.Infos = value; RaisePropertyChanged("SummaryInfo"); }
        }

        private bool _dataNotSaved = false;
        public bool DataNotSaved
        {
            get { return _dataNotSaved; }
            private set { _dataNotSaved = value; RaisePropertyChanged("DataNotSaved"); }
        }


        public ICommand CommandGetData { get; }
        public ICommand CommandSaveData { get; }

        public ICommand CommandShowPreferences { get; }

        public ICommand CommandShowHelp { get; }

        public ICommand CommandShowMetersFilter { get; }

        /// <summary>
        /// Команда отображения списка счётчиков
        /// </summary>
        public ICommand CommandShowAll { get; }


        private Mode _currentMode = Mode.MetersList;
        public Mode CurrentMode
        {
            get { return _currentMode; }
            set
            {
                if (value != _currentMode)
                {
                    _currentMode = value;
                    RaisePropertyChanged("CurrentMode");
                    CreateView();
                    RaisePropertyChanged("CurrentView");
                }
            }
        }

        private System.Windows.FrameworkElement _currentView = null;
        public System.Windows.FrameworkElement CurrentView
        {
            get
            {
                if (Data != null && _currentView is System.Windows.Controls.UserControl == false)
                    CreateView();
                return _currentView;
            }
        }

        private object _currentViewDataContext;
        public object CurrentViewDataContext
        {
            get { return _currentViewDataContext; }
            set { _currentViewDataContext = value; RaisePropertyChanged("CurrentViewDataContext"); }
        }

        #region SummaryView

        private KeyValuePair<int, string> _selectedQuarter;
        /// <summary>
        /// Выбранный квартал - пара номер квартала и и его название: I кв
        /// </summary>
        public KeyValuePair<int, string> SelectedQuarter
        {
            get
            {
                if (_selectedQuarter.Key == 0)
                    _selectedQuarter = Quarters[Meter.Дата_сравнения_поверки.GetQuarter() - 1];
                return _selectedQuarter;
            }
            set { _selectedQuarter = value; RaisePropertyChanged("SelectedQuarter"); UpdateПоверка(); }
        }
        private int _selectedYear;
        /// <summary>
        /// Выбранный год
        /// </summary>
        public int SelectedYear
        {
            get
            {
                if (_selectedYear == 0)
                    _selectedYear = Meter.Дата_сравнения_поверки.Year;
                return _selectedYear;
            }
            set { _selectedYear = value; RaisePropertyChanged("SelectedYear"); UpdateПоверка(); }
        }
        /// <summary>
        /// Список кварталов
        /// </summary>
        public List<KeyValuePair<int, string>> Quarters { get; private set; }
        /// <summary>
        /// Список годов
        /// </summary>
        public List<int> Years { get; private set; }

        #endregion

        public Theme SelectedTheme
        {
            get { return _selectTheme; }
            set
            {
                if (Properties.Settings.Default.Theme != value)
                    Properties.Settings.Default.Theme = value;
                _selectTheme = value;
                RaisePropertyChanged("SelectedTheme");
                ChangeTheme(_selectTheme);
            }
        }

        public ObservableCollection<Theme> Themes
        {
            get { return _themes; }
            set { _themes = value; RaisePropertyChanged("Themes"); }
        }


        #endregion

        #region IMainViewModel implementation

        public void ShowAllMeters()
        {
            throw new NotImplementedException();
        }

        public void ShowMetersWithGroupingAtField(string fieldName)
        {
            if (_data == null || _data.Meters == null)
                return;
            (new ViewCollectionWindow(
                    new ViewModel.ViewCollectionViewModel(_data.Meters.ToList(), ModelHelper.MeterPropertyDisplayNames[fieldName], fieldName)
                    )
            { Owner = App.Current.MainWindow }).ShowDialog();
        }

        public void ShowMeterFilteredByFieldValue(string fieldName, string value)
        {
            if (_data == null || _data.Meters == null)
                return;

            Exp.ParameterExpression pe = Exp.Expression.Parameter(typeof(Meter), "Meters");

            Exp.Expression left = Exp.Expression.Property(pe, ModelHelper.MeterProperties[fieldName]);
            Exp.Expression right = Exp.Expression.Constant(value);
            Exp.Expression InnerLambda = Exp.Expression.Equal(left, right);
            Exp.Expression<Func<Meter, bool>> innerFunction = Exp.Expression.Lambda<Func<Meter, bool>>(InnerLambda, pe);

            var method = typeof(Enumerable).GetMethods().Where(m => m.Name == "Any" && m.GetParameters().Length == 2).Single().MakeGenericMethod(typeof(Meter));
            Exp.MethodCallExpression outerLambda = Exp.Expression.Call(method, innerFunction);

            var v = outerLambda.Object;

            IEnumerable<Meter> items = _data.Meters.Where(i => i.HasSMS);

            (new ViewCollectionWindow(
                              new ViewModel.ViewCollectionViewModel(
                                  items.ToList(),
                              ModelHelper.MeterPropertyDisplayNames[fieldName],
                              fieldName,
                              value)
                          )
            { Owner = App.Current.MainWindow }).ShowDialog();
        }

        public string GetDepartamentName(Departament departament)
        {
            if (System.IO.Directory.Exists(departament.Path) == false)
            {
                MessageBox.Show(String.Format("Папка '{0}' не найдена.", departament.Path), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            string pathDBFC = Path.Combine(departament.Path, "DBFC");
            try
            {
                DataTable dtKartPd = ParseDBF.ReadDBF(Path.Combine(pathDBFC, "KartPd.DBF"));
                string name = dtKartPd.Rows[0]["PNPD"].ToString().Trim();
                departament.Name = name;
                return name;
            }
            catch (IOException e)
            {
                MessageBox.Show(String.Format("Не удалось обнаружить файл 'KartPd.DBF'.\nОписание ошибки: {0}", App.GetExceptionDetails(e), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning));
                return null;
            }
        }

        #endregion

    }

    [DataContract]
    public class Theme
    {
        [DataMember]
        public string ShortName { get; set; }
        [DataMember]
        public string FullName { get; set; }
    }
}
