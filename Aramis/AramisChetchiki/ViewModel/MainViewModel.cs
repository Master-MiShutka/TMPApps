namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;
    using Exp = System.Linq.Expressions;

    public class MainViewModel : BaseMainViewModel, IMainViewModel
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool isInitialized;
        private bool isWindowLoaded;
        private Model.AramisDataInfo selectedDataFileInfo;

        private IViewModel currentViewModel;
        private Mode currentMode;
        private Mode previousMode;

        private readonly System.Timers.Timer getAppMemoryUsageTimer;

        public MainViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Repository.Instance.Data = new AramisData()
                {
                    Info = new Model.AramisDataInfo()
                    {
                        FileName = "TestFileName.aramisData",
                        FileSize = 123456789,
                        IsLoaded = false,
                        DepartamentName = "Ressssss",
                        Version = new Version(1, 1),
                        Period = new Common.RepositoryCommon.DatePeriod(),
                    },

                    ChangesOfMeters = new (),
                    Meters = new List<Meter>(),
                    SummaryInfos = new ObservableCollection<SummaryInfoItem>(),
                };

                this.SelectedDataFileInfo = new AramisDataInfo()
                {
                    DepartamentName = "Test RES",
                    FileName = @"c:\Test1\test.yyy",
                    AramisDbPath = @"d:\test2",
                    FileSize = 123456789,
                };
                System.Diagnostics.Debug.Write("MainViewModel IsInDesignMode");
                return;
            }

            this.getAppMemoryUsageTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(2_000).TotalMilliseconds);
            this.getAppMemoryUsageTimer.Elapsed += this.AppMemoryUsageTimerCallback;
            this.getAppMemoryUsageTimer.Start();

            this.CommandGoHome = new DelegateCommand(action: () =>
            {
                this.ChangeMode(Mode.Home);
            });

            this.IsInitialized = false;
            this.Status = "запуск программы";
            this.DetailedStatus = "поиск файлов с данными ...";

            Repository.Instance.Handler += this.Repository_Handler;

            if (App.Current.MainWindow != null)
            {
                App.Current.MainWindow.Loaded += this.MainWindow_Loaded;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.isWindowLoaded = true;
        }

        #region Private methods

        private void AppMemoryUsageTimerCallback(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.TotalMemory = GC.GetTotalMemory(true);
            this.RaisePropertyChanged(nameof(this.TotalMemory));
        }

        private void Repository_Handler(object sender, Common.RepositoryCommon.RepositoryEventArgs e)
        {
            switch (e.Action)
            {
                case Common.RepositoryCommon.RepositoryAction.Initialized:
                    while (this.isWindowLoaded == false)
                    {
                    }

                    if (e.Info != null)
                    {
                        var fi = Repository.Instance.AvailableDataFiles.Cast<AramisDataInfo>().FirstOrDefault(i => i.FileName == e.Info.FileName);
                        if (this.IsDataLoaded == false)
                        {
                            this.SelectedDataFileInfo = fi;
                        }
                        else
                        {
                            this.selectedDataFileInfo = fi;
                            this.RaisePropertyChanged(nameof(this.SelectedDataFileInfo));
                            this.GoHome();
                        }
                    }
                    else
                    {
                        if (Repository.Instance.AvailableDataFiles.Count == 1)
                        {
                            this.SelectedDataFileInfo = Repository.Instance.AvailableDataFiles.Cast<AramisDataInfo>().FirstOrDefault();
                        }
                        else
                        {
                            this.GoStart();
                        }
                    }
                    this.IsInitialized = true;

                    break;
                case Common.RepositoryCommon.RepositoryAction.Loading:
                case Common.RepositoryCommon.RepositoryAction.Saving:
                    this.SetWindowTaskbarItemProgressState(TMPApplication.WpfDialogs.Contracts.TaskbarItemProgressState.Indeterminate);
                    break;
                case Common.RepositoryCommon.RepositoryAction.Loaded:
                    this.SetWindowTaskbarItemProgressState(TMPApplication.WpfDialogs.Contracts.TaskbarItemProgressState.None);
                    this.RaisePropertyChanged(nameof(this.IsDataLoaded));
                    this.RaisePropertyChanged(nameof(this.Data));
                    this.RaisePropertyChanged(nameof(this.Meters));
                    this.RaisePropertyChanged(nameof(this.AvailableDataFiles));

                    //TMP.Common.RepositoryCommon.JsonSerializer.JsonSerializeAsync(this.Data.Meters.Take(10), "meters10.json");

                    // TMP.Common.RepositoryCommon.JsonSerializer.JsonSerializeAsync(Data.Meters, "meters.json");
                    break;
                case Common.RepositoryCommon.RepositoryAction.Saved:
                    this.SetWindowTaskbarItemProgressState(TMPApplication.WpfDialogs.Contracts.TaskbarItemProgressState.None);
                    this.RaisePropertyChanged(nameof(this.AvailableDataFiles));
                    //TMP.Common.RepositoryCommon.JsonSerializer.JsonSerializeAsync(this.Data.Meters, "meters.json");
                    //TMP.Common.RepositoryCommon.JsonSerializer.JsonSerializeAsync(this.Data.Meters.Take(10), "meters10.json");
                    break;
                case Common.RepositoryCommon.RepositoryAction.Updated:
                    this.RaisePropertyChanged(nameof(this.Data));
                    this.RaisePropertyChanged(nameof(this.Meters));
                    this.RaisePropertyChanged(nameof(this.AvailableDataFiles));
                    break;
                case Common.RepositoryCommon.RepositoryAction.FoundNewFile:
                    break;
                case Common.RepositoryCommon.RepositoryAction.ErrorOccurred:
                    this.ShowDialogWarning(e.Detail);
                    break;
                case Common.RepositoryCommon.RepositoryAction.UICallback:
                    this.DetailedStatus = e.Detail;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        private async System.Threading.Tasks.Task<bool> LoadEarlierSavedDataAsync()
        {
            this.logger?.Info(">>> LoadEarlierSavedDataAsync");

            if (this.SelectedDataFileInfo == null)
            {
                return false;
            }

            this.Status = $"Попытка загрузки ранее сохраненных данных:\nРЭС - {this.SelectedDataFileInfo.DepartamentName};\nимя файла - {this.SelectedDataFileInfo.FileName}";
            this.logger?.Info(this.Status);

            this.SetWindowTaskbarItemProgressState(TMPApplication.WpfDialogs.Contracts.TaskbarItemProgressState.Indeterminate);

            var result = await Repository.Instance.GetDataFromFile(this.SelectedDataFileInfo).ConfigureAwait(false);

            this.IsInitialized = true;
            this.SetWindowTaskbarItemProgressState(TMPApplication.WpfDialogs.Contracts.TaskbarItemProgressState.None);

            if (result)
            {
                this.logger?.Info($"Данные загружены успешно из файла '{this.SelectedDataFileInfo.FileName}'");
                this.Status = null;
                this.RaisePropertyChanged(nameof(this.IsDataLoaded));
                this.GoHome();
            }
            else
            {
                this.GoStart();
            }

            return false;
        }

        private void DoShowMetersCollection(IEnumerable<Meter> meters, string fieldDisplayName = null, string fieldName = null, string value = null)
        {
            var vm = new ViewModel.ViewCollectionViewModel(meters, fieldDisplayName, fieldName, value);
            this.previousMode = this.CurrentMode;
            this.currentMode = Mode.ViewCollection;
            this.currentViewModel = vm;

            this.RaisePropertyChanged(propertyName: nameof(this.CurrentMode));
            this.RaisePropertyChanged(propertyName: nameof(this.CurrentViewModel));
            this.RaisePropertyChanged(propertyName: nameof(this.WindowTitle));
        }

        #endregion Private methods

        #region Public methods

        #endregion

        #region Properties

        /// <summary>
        /// The amount of memory used by an application
        /// </summary>
        public long TotalMemory { get; private set; }

        /// <summary>
        /// Признак, указывающий, что данные загружены
        /// </summary>
        public bool IsDataLoaded => this.Data != null;

        /// <summary>
        /// Флаг, указывающий завершена ли инициализация
        /// </summary>
        public bool IsInitialized
        {
            get => this.isInitialized;
            set => this.SetProperty(ref this.isInitialized, value);
        }

        /// <summary>
        /// Коллекция доступных файлов с данными
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Common.RepositoryCommon.IDataFileInfo> AvailableDataFiles => Repository.Instance.AvailableDataFiles;

        /// <summary>
        /// Выбранный файл данных
        /// </summary>
        public Model.AramisDataInfo SelectedDataFileInfo
        {
            get => this.selectedDataFileInfo;
            set
            {
                if (this.SetProperty(ref this.selectedDataFileInfo, value))
                {
                    if (this.selectedDataFileInfo != null)
                    {
                        this.selectedDataFileInfo.IsSelected = true;
                        this.LoadData();
                    }
                }
            }
        }

        /// <summary>
        /// Данные
        /// </summary>
        public AramisData Data => Repository.Instance.Data;

        /// <summary>
        /// Коллекция счётчиков
        /// </summary>
        public IEnumerable<Meter> Meters => (this.Data == null) ? null : this.Data.Meters.Where(i => i.Удалён == false);

        /// <summary>
        /// Коллекция сводной информации
        /// </summary>
        public IEnumerable<SummaryInfoItem> SummaryInfo
        {
            get => (this.Data == null || this.Data.SummaryInfos == null) ? null : this.Data.SummaryInfos;
            private set
            {
                if (ReferenceEquals(value, this.Data.SummaryInfos))
                {
                    return;
                }

                this.Data.SummaryInfos = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Текущий режим работы
        /// </summary>
        public Mode CurrentMode => this.currentMode;

        /// <summary>
        /// Текущая модель данных
        /// </summary>
        public IViewModel CurrentViewModel => this.currentViewModel;

        /// <summary>
        /// Команда возврата к начальному экрану
        /// </summary>
        public ICommand CommandGoHome { get; }

        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string WindowTitle
        {
            get
            {
                string result = TMPApplication.TMPApp.Instance.Title;
                if (this.CurrentMode != Mode.Home)
                {
                    System.Reflection.FieldInfo info = this.CurrentMode.GetType().GetField(this.CurrentMode.ToString());
                    if (!info.CustomAttributes.Any())
                    {
                        return result;
                    }

                    var valueDescription = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (valueDescription != null && valueDescription.Length == 1)
                    {
                        result += " :: " + valueDescription[0].Description;
                    }
                }

                return result;
            }
        }

        #endregion Properties

        #region IMainViewModel implementation

        private void LoadData()
        {
            // проверка на наличие файла
            if (System.IO.File.Exists(this.SelectedDataFileInfo.FileName))
            {
                Task.Run(async () =>
                {
                    System.Threading.Thread.CurrentThread.Name = "LoadEarlierSavedData";
                    bool result = await this.LoadEarlierSavedDataAsync().ConfigureAwait(false);
                    if (result == false)
                    {
                        this.SelectedDataFileInfo = null;
                        this.ShowDialogError("Файл с данными не удалось загрузить.");
                    }
                });
            }
            else
            {
                this.ChangeMode(Mode.LoadingData);
            }
        }

        public void ShowAllMeters()
        {
        }

        public void ShowMetersCollection(IEnumerable<Meter> meters)
        {
            if (meters == null)
            {
                return;
            }

            this.DoShowMetersCollection(meters);
        }

        public void ShowMetersWithGroupingAtField(string fieldName)
        {
            if (this.Data == null || this.Meters == null)
            {
                return;
            }

            this.DoShowMetersCollection(this.Meters.ToList(), ModelHelper.MeterPropertyDisplayNames[fieldName], fieldName);
        }

        public void ShowMeterFilteredByFieldValue(string fieldName, string value)
        {
            if (this.Data == null || this.Meters == null)
            {
                return;
            }

            Exp.ParameterExpression pe = Exp.Expression.Parameter(typeof(Meter), "meter");

            Exp.Expression left = Exp.Expression.Property(pe, ModelHelper.MeterProperties[fieldName]);
            Exp.Expression right = Exp.Expression.Constant(value);
            Exp.Expression innerLambda = Exp.Expression.Equal(left, right);
            Exp.Expression<Func<Meter, bool>> innerFunction = Exp.Expression.Lambda<Func<Meter, bool>>(innerLambda, pe);

            var method = typeof(Enumerable).GetMethods().Where(m => m.Name == "Where" && m.GetParameters().Length == 2).Single().MakeGenericMethod(typeof(Meter));

            Exp.MethodCallExpression outerLambda = Exp.Expression.Call(method, Exp.Expression.Constant(this.Meters), innerFunction);

            Func<IEnumerable<Meter>> a = (Func<IEnumerable<Meter>>)Exp.Expression.Lambda(outerLambda).Compile();

            var items = a();

            this.DoShowMetersCollection(items, ModelHelper.MeterPropertyDisplayNames[fieldName], fieldName, value);
        }

        public void ChangeMode(Mode newMode)
        {
            if (this.CurrentViewModel is not null and IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (newMode == this.currentMode && this.currentViewModel != null)
            {
                return;
            }

            this.previousMode = this.currentMode;

            this.currentMode = newMode;
            this.currentViewModel = ModelHelper.ModeFactory(newMode);

            this.RaisePropertyChanged(propertyName: nameof(this.CurrentMode));
            this.RaisePropertyChanged(propertyName: nameof(this.CurrentViewModel));
            this.RaisePropertyChanged(propertyName: nameof(this.WindowTitle));
        }

        public void GoBack()
        {
            this.ChangeMode(Mode.Home);
        }

        public void GoStart()
        {
            this.ChangeMode(Mode.None);
        }

        public void GoHome()
        {
            this.ChangeMode(Mode.Home);
        }

        public void ShowSettingsPage(ISettingsPage settingsPage)
        {
            throw new NotImplementedException();
        }

        #endregion IMainViewModel implementation
    }
}