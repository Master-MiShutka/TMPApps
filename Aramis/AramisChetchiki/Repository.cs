namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using TMP.Common.RepositoryCommon;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.WORK.AramisChetchiki.Properties;

    public sealed class Repository : BaseRepository<AramisData, AramisDataInfo>
    {
        #region Constants

        public override string DATA_FILENAME_EXTENSION { get; } = ".aramis_data";

        public override Version LastSupportedVersion { get; } = new Version(1, 1, 0, 0);

        private const string PART_METERS = "Meters";
        private const string PART_CHANGES_OF_METERS = "ChangesOfMeters";
        private const string PART_SUMMARY_INFOS = "SummaryInfos";
        private const string PART_ELECTRICITY_SUPPLYS = "ElectricitySupplys";
        private const string PART_PAYMENTS = "PaymentDataInfos";
        private const string PART_CONTROLDATA = "MetersControlData";
        private const string PART_EVENTS = "Events";

        #endregion

        #region Fields

        private static Repository instance;

        // Helper for Thread Safety
        private static readonly object @lock = new();

        private WindowWithDialogs.Contracts.IWindowWithDialogs mainWindow = WindowWithDialogs.BaseApplication.Instance.MainWindowWithDialogs;

        #endregion

        #region Singleton

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static Repository()
        {
        }

        private Repository()
            : base(AppSettings.Default.DataFilesStorePath)
        {
            logger?.Info(">>> TMP.WORK.AramisChetchiki.Repository -> Constructor");

            this.DATA_DESCRIPTION = "Данные из программы расчётов бытовых абонентов за электроэнергию 'Арамис'";

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                System.Diagnostics.Debug.Write("Repository IsInDesignMode");
                return;
            }

            this.OnSaving = this.SaveRestDataInfo;

            this.OnLoadingFromPackage = this.LoadRestDataInfo;
        }

        public static Repository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (@lock)
                    {
                        instance = new Repository();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Private methods

        private void SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState taskbarItemProgressState, double value = -1)
        {
            WindowWithDialogs.DispatcherExtensions.InUi(() =>
            {
                this.mainWindow.TaskbarItemInfo = value != -1
                    ? new WindowWithDialogs.Contracts.TaskbarItemInfo
                    {
                        ProgressState = taskbarItemProgressState,
                        ProgressValue = value,
                    }
                    : new WindowWithDialogs.Contracts.TaskbarItemInfo
                    {
                        ProgressState = taskbarItemProgressState,
                    };
            });
        }

        protected override string GetDataStorePath()
        {
            return AppSettings.Default.DataFilesStorePath;
        }

        protected override void SetDataStorePath(string value)
        {
            var appPath = WindowWithDialogs.BaseApplication.ExecutionPath;
            var relativePath = System.IO.Path.GetRelativePath(appPath, value);

            AppSettings.Default.DataFilesStorePath = relativePath;
            base.SetDataStorePath(relativePath);
        }

        private void LoadRestDataInfo(System.IO.Packaging.Package package, AramisData aramisData)
        {
            this.UpdateUI("загрузка счётчиков");
            Meter[] meters = this.LoadDataListFromPackageAsync<Meter>(package, PART_METERS).Result;
            aramisData.Meters = meters != null ? new ObservableCollection<Meter>(meters) : throw new NoNullAllowedException();

            this.UpdateUI("загрузка замен счётчиков");
            System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.IList<ChangeOfMeter>> chom = this.LoadDictionaryFromPackageAsync<ulong, ChangeOfMeter>(package, PART_CHANGES_OF_METERS).Result;
            if (chom != null)
            {
                aramisData.ChangesOfMeters = chom;
            }
            else
            {
                aramisData.ChangesOfMeters = new ();
            }

            this.UpdateUI("загрузка сводной информации");
            SummaryInfoItem[] si = this.LoadDataListFromPackageAsync<SummaryInfoItem>(package, PART_SUMMARY_INFOS).Result;
            if (si != null)
            {
                aramisData.SummaryInfos = new ObservableCollection<SummaryInfoItem>(si);
            }
            else
            {
                aramisData.SummaryInfos = new System.Collections.Generic.List<SummaryInfoItem>();
            }

            this.UpdateUI("загрузка данных о полезном отпуске");
            ElectricitySupply[] es = this.LoadDataListFromPackageAsync<ElectricitySupply>(package, PART_ELECTRICITY_SUPPLYS).Result;
            if (es != null)
            {
                aramisData.ElectricitySupplyInfo = new ObservableCollection<ElectricitySupply>(es);
            }
            else
            {
                aramisData.ElectricitySupplyInfo = new System.Collections.Generic.List<ElectricitySupply>();
            }

            this.UpdateUI("загрузка данных об произведенных оплатах");
            System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.IList<Payment>> pd = this.LoadDictionaryFromPackageAsync<ulong, Payment>(package, PART_PAYMENTS).Result;
            if (pd != null)
            {
                aramisData.Payments = pd;
            }
            else
            {
                aramisData.Payments = new ();
            }

            this.UpdateUI("загрузка контрольных показаний по лицевому счету");
            System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.IList<ControlData>> cd = this.LoadDictionaryFromPackageAsync<ulong, ControlData>(package, PART_CONTROLDATA).Result;
            if (cd != null)
            {
                aramisData.MetersControlData = cd;
            }
            else
            {
                aramisData.MetersControlData = new();
            }

            this.UpdateUI("загрузка событий по лицевому счету");
            System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.IList<MeterEvent>> events = this.LoadDictionaryFromPackageAsync<ulong, MeterEvent>(package, PART_EVENTS).Result;
            if (events != null)
            {
                aramisData.Events = events;
            }
            else
            {
                aramisData.Events = new();
            }
        }

        private async void SaveRestDataInfo(System.IO.Packaging.Package package, AramisData aramisData)
        {
            try
            {
                this.UpdateUI("сохранение счётчиков");
                byte[] bytes = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToBytesAsync(this.Data.Meters);
                this.SaveJsonDataToPart(bytes, package, PART_METERS);

                this.UpdateUI("сохранение замен счётчиков");
                bytes = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToBytesAsync(this.Data.ChangesOfMeters);
                this.SaveJsonDataToPart(bytes, package, PART_CHANGES_OF_METERS);

                this.UpdateUI("сохранение сводной информации");
                bytes = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToBytesAsync(this.Data.SummaryInfos);
                this.SaveJsonDataToPart(bytes, package, PART_SUMMARY_INFOS);

                this.UpdateUI("сохранение данных о полезном отпуске");
                bytes = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToBytesAsync(this.Data.ElectricitySupplyInfo);
                this.SaveJsonDataToPart(bytes, package, PART_ELECTRICITY_SUPPLYS);

                this.UpdateUI("сохранение данных об произведенных оплатах");
                bytes = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToBytesAsync(this.Data.Payments);
                this.SaveJsonDataToPart(bytes, package, PART_PAYMENTS);

                this.UpdateUI("сохранение контрольных показаний по лицевому счету");
                bytes = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToBytesAsync(this.Data.MetersControlData);
                this.SaveJsonDataToPart(bytes, package, PART_CONTROLDATA);

                this.UpdateUI("сохранение событий по лицевому счету");
                bytes = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToBytesAsync(this.Data.Events);
                this.SaveJsonDataToPart(bytes, package, PART_EVENTS);
            }
            catch (Exception e)
            {
                logger?.Error($"Ошибка при частей файла.\nОписание: {App.GetExceptionDetails(e)}");

                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.Error);
                WindowWithDialogs.DispatcherExtensions.InUi(() => this.mainWindow.ShowDialogError("Ошибка сохранения данных." + Environment.NewLine + WindowWithDialogs.BaseApplication.GetExceptionDetails(e)));
                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);
            }
        }

        #endregion

        #region Public methods

        public void AddNewAramisDbPath(string aramisDbPath)
        {
            if (this.ContainsDataInfoCollectionAramisDbPath(aramisDbPath))
            {
                return;
            }

            string departamentName = GetDepartamentName(aramisDbPath);
            AramisDataInfo newAramisDataInfo = new()
            {
                AramisDbPath = aramisDbPath,
                FileName = "Data-" + departamentName + Instance.DATA_FILENAME_EXTENSION,
                DepartamentName = departamentName,
                Version = this.LastSupportedVersion,
                IsSelected = true,
            };
            this.AvailableDataFiles.Add(newAramisDataInfo);
        }

        public bool ContainsDataInfo(AramisDataInfo aramisDataInfo)
        {
            if (this.AvailableDataFiles != null && this.AvailableDataFiles.Count > 0)
            {
                System.Collections.Generic.List<AramisDataInfo> dataFiles = this.AvailableDataFiles
                    .Cast<AramisDataInfo>()
                    .Where(f => f.FileName == aramisDataInfo.FileName && f.DepartamentName == aramisDataInfo.DepartamentName)
                    .ToList();
                return dataFiles.Count > 0;
            }
            else
            {
                return false;
            }
        }

        public bool ContainsDataInfoCollectionAramisDbPath(string aramisDbPath)
        {
            if (this.AvailableDataFiles != null && this.AvailableDataFiles.Count > 0)
            {
                System.Collections.Generic.List<AramisDataInfo> dataFiles = this.AvailableDataFiles
                    .Cast<AramisDataInfo>()
                    .Where(f => f.AramisDbPath == aramisDbPath)
                    .ToList();
                return dataFiles.Count > 0;
            }
            else
            {
                return false;
            }
        }

        public static string GetDepartamentName(string aramisDbPath)
        {
            if (Directory.Exists(aramisDbPath) == false)
            {
                // App.ShowWarning(String.Format("Папка '{0}' не найдена.", aramisDbPath));
                return null;
            }

            string pathDBFC = Path.Combine(aramisDbPath, "DBFC");
            try
            {
                if (File.Exists(Path.Combine(pathDBFC, "KartPd.DBF")) == false)
                {
                    return null;
                }

                using DBF.DbfTable dbfTable = new(Path.Combine(pathDBFC, "KartPd.DBF"), System.Text.Encoding.GetEncoding("cp866"), false);
                string value = dbfTable.ReadRecord().GetValue<string>("PNPD");
                string name = value.Trim();
                return name;
            }
            catch (IOException e)
            {
                logger?.Error($"Не удалось обнаружить файл 'KartPd.DBF'.\nОписание ошибки: {App.GetExceptionDetails(e)}");
                return null;
            }
            catch (Exception e)
            {
                logger?.Error($"Неизвестная ошибка.\nОписание: {App.GetExceptionDetails(e)}");
                return null;
            }
        }

        /// <summary>
        /// Загрузка необходимых данных из базы данных Арамис
        /// </summary>
        /// <param name="departament">Подразделение</param>
        /// <param name="actionUpdateStatusCallBack">Процедура обратного вызова для обновления интерфейса</param>
        /// <returns></returns>
        public async Task<bool> GetDataFromDb(AramisDataInfo aramisDataInfo, ViewModel.LoadingDataViewModel workTasksProgressViewModel)
        {
            logger?.Info(">>> TMP.WORK.AramisChetchiki.Repository>GetDataFromDb");

            if (aramisDataInfo == null)
            {
                throw new ArgumentNullException(nameof(aramisDataInfo));
            }

            bool result = false;

            if (string.IsNullOrWhiteSpace(aramisDataInfo.AramisDbPath))
            {
                this.mainWindow.ShowDialogWarning(
                    string.Format(AppSettings.CurrentCulture, "Не задан путь к папке с программой 'Арамис' для подразделения '{1}'!{0}Для этого откройте параметры программы, выберите раздел 'Расположение данных' и{0}укажите путь и повторите операцию снова.",
                    Environment.NewLine,
                    aramisDataInfo.DepartamentName));
                return false;
            }

            if (Directory.Exists(aramisDataInfo.AramisDbPath) == false)
            {
                this.mainWindow.ShowDialogWarning(string.Format(
                    AppSettings.CurrentCulture,
                    "Указанный в параметрах путь к папке с программой 'Арамис' для подразделения '{0}' не доступен!",
                    aramisDataInfo.DepartamentName));
                return false;
            }

            if (Directory.Exists(Path.Combine(aramisDataInfo.AramisDbPath, "DBF")) == false)
            {
                this.mainWindow.ShowDialogWarning("В папке с программой 'Арамис' не найдена папка 'DBF'!");
                return false;
            }

            if (Directory.Exists(Path.Combine(aramisDataInfo.AramisDbPath, "DBFC")) == false)
            {
                this.mainWindow.ShowDialogWarning("В папке с программой 'Арамис' не найдена папка 'DBFC'!");
                return false;
            }

            this.Data?.Clear();
            AramisData data = null;
            this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.Normal, 0d);

            try
            {
                AramisDBParser aramisDBParser = new(aramisDataInfo, workTasksProgressViewModel);
                data = await aramisDBParser.GetDataAsync();
                (data.Info as AramisDataInfo).AramisDbPath = aramisDataInfo.AramisDbPath;
                (data.Info as AramisDataInfo).LastModifiedDate = DateTime.Now;
                (data.Info as AramisDataInfo).IsLoaded = true;
                data.Info.Period = new Common.RepositoryCommon.DatePeriod();
                this.Data = data;

                Model.WorkTask workTask = new("сохранение данных")
                {
                    Status = "сохранение информации в файл",
                    IsIndeterminate = true,
                };
                workTasksProgressViewModel.WorkTasks.Add(workTask);

                await this.SaveDataAsync(this.Data.Info.FileName);

                workTask.IsCompleted = true;

                this.Data.Info.FileSize = new FileInfo(this.Data.Info.FileName).Length;

                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);

                result = true;
            }
            catch (Exception ex)
            {
                logger?.Error(ex);

                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.Error);
                WindowWithDialogs.DispatcherExtensions.InUi(() => this.mainWindow.ShowDialogError("Ошибка при получении данных." + Environment.NewLine + WindowWithDialogs.BaseApplication.GetExceptionDetails(ex)));
                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);

                this.Data = null;
            }

            return result;
        }

        public async Task<bool> GetDataFromFile(AramisDataInfo aramisDataInfo)
        {
            logger?.Info(">>> TMP.WORK.AramisChetchiki.Repository>GetDataFromFile");

            if (aramisDataInfo == null)
            {
                throw new ArgumentNullException(nameof(aramisDataInfo));
            }

            bool result = false;
            this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.Normal, 0d);

            try
            {
                // загрузка
                LoadStatus status = await this.LoadAsync(aramisDataInfo.FileName);
                logger?.Info(status);

                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);

                result = status == LoadStatus.Ok ? this.Data.Meters != null : false;
            }
            catch (Exception ex)
            {
                logger?.Error(ex);
                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.Error);
                WindowWithDialogs.DispatcherExtensions.InUi(() => this.mainWindow.ShowDialogError("Ошибка при получении данных." + Environment.NewLine + WindowWithDialogs.BaseApplication.GetExceptionDetails(ex)));
                this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);
                this.Data = null;
            }

            return result;
        }

        #endregion
    }
}
