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
        private long totalMemory;
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
                return;
            }

            this.getAppMemoryUsageTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(2_000).TotalMilliseconds);
            this.getAppMemoryUsageTimer.Elapsed += this.AppMemoryUsageTimerCallback;
            this.getAppMemoryUsageTimer.Start();

            this.CommandGoHome = new DelegateCommand(() =>
            {
                this.ChangeMode(Mode.Home);
            });

            this.CommandShowErrors = new DelegateCommand(() =>
            {
                System.Windows.Controls.ScrollViewer scrollViewer = new System.Windows.Controls.ScrollViewer();
                System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsReadOnlyCaretVisible = true,
                    Text = System.IO.File.ReadAllText(AramisDBParser.ErrorsFileName, System.Text.Encoding.UTF8),
                };
                scrollViewer.Content = textBox;

                this.ShowCustomDialog(scrollViewer, "-= Выявленные ошибки в БД Арамис =-", WindowWithDialogs.DialogMode.Ok);
            }, () => System.IO.File.Exists(AramisDBParser.ErrorsFileName));

            this.CommandShowPreferences = new DelegateCommand(() =>
            {
                this.ChangeMode(Mode.Preferences);
            });

            this.isInitialized = false;
            this.Status = "запуск программы";
            this.DetailedStatus = "поиск файлов с данными ...";

            Repository.Instance.Handler += this.Repository_Handler;

            if (App.Current.MainWindow != null)
            {
                App.Current.MainWindow.Loaded += this.MainWindow_Loaded;
            }

            // Загрузка кэша сводных таблиц
            if (System.IO.File.Exists("MatrixCache.data"))
            {
                using (var fs = new System.IO.FileStream("MatrixCache.data", System.IO.FileMode.Open))
                {
                    this.MatrixCache = MessagePack.MessagePackSerializer.Deserialize<Dictionary<string, IList<UI.Controls.WPF.Reporting.MatrixGrid.IMatrixCell>>>(
                        fs,
                        MessagePack.MessagePackSerializer.DefaultOptions.WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray));
                }
            }

            if (this.MatrixCache == null)
                this.MatrixCache = new();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.isWindowLoaded = true;
        }

        protected override void OnClosingMainWindow()
        {
            base.OnClosingMainWindow();

            using (var fs = new System.IO.FileStream("MatrixCache.data", System.IO.FileMode.OpenOrCreate))
            {
                MessagePack.MessagePackSerializer.Serialize<Dictionary<string, IList<UI.Controls.WPF.Reporting.MatrixGrid.IMatrixCell>>>(
                    fs,
                    this.MatrixCache,
                    MessagePack.MessagePackSerializer.DefaultOptions.WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray));
            }
        }


        #region Private methods

        private void AppMemoryUsageTimerCallback(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.TotalMemory = GC.GetTotalMemory(true);
            this.RaisePropertyChanged(nameof(this.TotalMemory));
        }

        private void Repository_Handler(object sender, Common.RepositoryCommon.RepositoryEventArgs e)
        {
            AramisDataInfo fi;
            switch (e.Action)
            {
                case Common.RepositoryCommon.RepositoryAction.Initialized:
                    while (this.isWindowLoaded == false)
                    {
                    }

                    if (e.Info != null)
                    {
                        fi = Repository.Instance.AvailableDataFiles.Cast<AramisDataInfo>().FirstOrDefault(i => i.FileName == e.Info.FileName);
                        if (fi != this.selectedDataFileInfo)
                        {
                            this.selectedDataFileInfo = fi;
                            this.RaisePropertyChanged(nameof(this.SelectedDataFileInfo));
                        }

                        if (this.IsDataLoaded)
                        {
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

                    this.RaisePropertyChanged(nameof(this.AvailableDataFiles));
                    this.RaisePropertyChanged(nameof(this.SelectedDataFileInfo));
                    this.IsInitialized = true;
                    break;
                case Common.RepositoryCommon.RepositoryAction.Loading:
                case Common.RepositoryCommon.RepositoryAction.Saving:
                    this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.Indeterminate);
                    break;
                case Common.RepositoryCommon.RepositoryAction.Loaded:
                    this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);
                    this.RaisePropertyChanged(nameof(this.IsDataLoaded));
                    this.RaisePropertyChanged(nameof(this.Data));
                    this.RaisePropertyChanged(nameof(this.Meters));
                    this.RaisePropertyChanged(nameof(this.AvailableDataFiles));
                    fi = Repository.Instance.AvailableDataFiles.Cast<AramisDataInfo>().FirstOrDefault(i => i.FileName == e.Info.FileName);
                    this.selectedDataFileInfo = fi;
                    this.RaisePropertyChanged(nameof(this.SelectedDataFileInfo));
                    break;
                case Common.RepositoryCommon.RepositoryAction.Saved:
                    this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);
                    this.RaisePropertyChanged(nameof(this.AvailableDataFiles));
                    this.RaisePropertyChanged(nameof(this.SelectedDataFileInfo));
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

            this.IsBusy = true;
            this.MatrixCache.Clear();

            this.Status = $"Попытка загрузки ранее сохраненных данных:\nРЭС - {this.SelectedDataFileInfo.DepartamentName};\nимя файла - {this.SelectedDataFileInfo.FileName}";
            this.logger?.Info(this.Status);

            this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.Indeterminate);

            bool result = await Repository.Instance.GetDataFromFile(this.SelectedDataFileInfo).ConfigureAwait(false);

            this.IsInitialized = true;
            this.SetWindowTaskbarItemProgressState(WindowWithDialogs.Contracts.TaskbarItemProgressState.None);

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
            ViewCollectionViewModel vm = new ViewModel.ViewCollectionViewModel(meters, fieldDisplayName, fieldName, value);
            this.previousMode = this.CurrentMode;
            this.currentMode = Mode.ViewMeters;
            this.currentViewModel = vm;
            this.RaisePropertyChanged(nameof(this.IsDataLoaded));
            this.RaisePropertyChanged(propertyName: nameof(this.CurrentMode));
            this.RaisePropertyChanged(propertyName: nameof(this.CurrentViewModel));
            this.RaisePropertyChanged(propertyName: nameof(this.WindowTitle));
        }

        #endregion Private methods

        #region Public methods

        #endregion

        #region Properties

        public Dictionary<string, IList<UI.Controls.WPF.Reporting.MatrixGrid.IMatrixCell>> MatrixCache { get; init; }


        /// <summary>
        /// The amount of memory used by an application
        /// </summary>
        public long TotalMemory { get => this.totalMemory; private set => this.SetProperty(ref this.totalMemory, value); }

        /// <summary>
        /// Признак, указывающий, что данные загружены
        /// </summary>
        public bool IsDataLoaded => this.Data != null && this.CurrentMode != Mode.LoadingData;

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
        public ObservableCollection<Common.RepositoryCommon.IDataFileInfo> AvailableDataFiles => Repository.Instance.AvailableDataFiles;

        /// <summary>
        /// Выбранный файл данных
        /// </summary>
        public Model.AramisDataInfo SelectedDataFileInfo
        {
            get => this.selectedDataFileInfo ?? (Repository.Instance.AvailableDataFilesCount > 0 ? this.AvailableDataFiles.Cast<AramisDataInfo>().First() : default);
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
        public IEnumerable<Meter> Meters => this.Data?.Meters.Where(i => i.Удалён == false);

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

        public ICommand CommandShowErrors { get; }

        public ICommand CommandShowPreferences { get; }

        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string WindowTitle
        {
            get
            {
                string result = WindowWithDialogs.BaseApplication.Instance.Title;
                if (this.CurrentMode != Mode.Home)
                {
                    System.Reflection.FieldInfo info = this.CurrentMode.GetType().GetField(this.CurrentMode.ToString());
                    if (!info.CustomAttributes.Any())
                    {
                        return result;
                    }

                    DescriptionAttribute[] valueDescription = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);
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

        private async void LoadData()
        {
            // проверка на наличие файла
            if (System.IO.File.Exists(this.SelectedDataFileInfo.FileName))
            {
                bool result = await this.LoadEarlierSavedDataAsync();
                if (result == false)
                {
                    this.SelectedDataFileInfo = null;
                    this.ShowDialogError("Файл с данными не удалось загрузить.");
                }
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

            System.Reflection.MethodInfo method = typeof(Enumerable).GetMethods().Where(m => m.Name == "Where" && m.GetParameters().Length == 2).Single().MakeGenericMethod(typeof(Meter));

            Exp.MethodCallExpression outerLambda = Exp.Expression.Call(method, Exp.Expression.Constant(this.Meters), innerFunction);

            Func<IEnumerable<Meter>> a = (Func<IEnumerable<Meter>>)Exp.Expression.Lambda(outerLambda).Compile();

            IEnumerable<Meter> items = a();

            this.DoShowMetersCollection(items, ModelHelper.MeterPropertyDisplayNames[fieldName], fieldName, value);
        }

        public void ChangeMode(Mode newMode)
        {
            if (this.currentViewModel is not null and IDisposable disposable)
            {
                disposable.Dispose();
                this.currentViewModel = null;
            }

            if (newMode == this.currentMode && this.currentViewModel != null)
            {
                return;
            }

            this.previousMode = this.currentMode;

            this.currentMode = newMode;
            this.currentViewModel = ModelHelper.ModeFactory(newMode);

            this.RaisePropertyChanged(propertyName: nameof(this.IsDataLoaded));
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

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0457");
            return guid.GetHashCode();
        }
    }
}