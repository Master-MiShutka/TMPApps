namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using TMP.Shared;
    using TMP.WORK.AramisChetchiki.Model;

    /// <summary>
    /// Модель данных для отображения хода загрузки данных
    /// </summary>
    public class LoadingDataViewModel : BaseViewModel
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ObservableCollection<WorkTask> workTasks = new();

        /// <summary>
        /// Список выполняемых задач
        /// </summary>
        public ObservableCollection<WorkTask> WorkTasks
        {
            get => this.workTasks;
            init => this.SetProperty(ref this.workTasks, value);
        }

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Header => "получение данных из базы данных программы 'Арамис'" + Environment.NewLine + "подразделения '" + MainViewModel.SelectedDataFileInfo?.DepartamentName + "'";

        /// <summary>
        /// Выбранный файл данных
        /// </summary>
        public AramisDataInfo SelectedDataFileInfo => MainViewModel.SelectedDataFileInfo;

        public LoadingDataViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                this.workTasks = new ObservableCollection<WorkTask>()
                {
                        new WorkTask("Задача №1") { Status = "Status 1", Progress = 65, ElapsedTime = "05:56" },
                        new WorkTask("Задача #3") { Status = "Status 3", Progress = 84, ElapsedTime = "01:06", ChildProgress = 75, ChildRemainingTime = "23:22", RemainingTime = "efe" },
                        new WorkTask("Задача №4") { Status = "Status 4", IsCompleted = true },
                };

                WorkTask t1 = new WorkTask("Задача №2") { Status = "Status 2", Progress = 25 };
                t1.SetChildProgress(100, 33);
                this.workTasks.Insert(1, t1);

                WorkTask t2 = new WorkTask("Задача №5") { Status = "Status 5", Progress = 12 };
                t2.SetChildProgress(100, 3);
                this.workTasks.Insert(2, t2);

                return;
            }

            App.InvokeInUIThread(() => System.Windows.Data.BindingOperations.EnableCollectionSynchronization(this.workTasks, new object()));

            // Запуск получения данных
            System.Threading.Tasks.Task.Run(async () =>
            {
                System.Threading.Thread.CurrentThread.Name = "StartGetDataFromAramisDb";

                bool result = await this.GetDataFromAramisDbAsync().ConfigureAwait(false);

                if (result == false)
                {
                    this.ShowDialogWarning($"Данные из базы данных Арамис '{this.SelectedDataFileInfo.AramisDbPath}' не удалось загрузить.\nПерейдите к параметрам\nи укажите путь к базе данных.");
                    MainViewModel.ChangeMode(Mode.Preferences);
                }
                else
                {
                    MainViewModel.GoHome();
                }
            });
        }

        protected override void OnClosingMainWindow()
        {
            base.OnClosingMainWindow();
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        private void Init()
        {
            this.workTasks.Clear();
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        private async System.Threading.Tasks.Task<bool> GetDataFromAramisDbAsync()
        {
            this.logger?.Info(">>> GetDataFromAramisDbAsync");

            if (this.SelectedDataFileInfo == null)
            {
                return false;
            }

            string msg;
            msg = $"Попытка получить данные из базы данных Арамис: РЭС - {this.SelectedDataFileInfo.DepartamentName}, путь к базе - '{this.SelectedDataFileInfo.AramisDbPath}'";
            this.logger?.Info(msg);
            bool isSuccess;
            try
            {
                this.Init();
                isSuccess = await Repository.Instance.GetDataFromDb(this.SelectedDataFileInfo, this).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger?.Error(ex, msg);
                this.ShowDialogError(ex);
                return false;
            }

            if (isSuccess)
            {
                this.SelectedDataFileInfo.IsLoaded = true;
                this.SelectedDataFileInfo.IsSelected = true;
                msg = $"Данные из базы данных Арамис: РЭС - {this.SelectedDataFileInfo.DepartamentName} получены успешно";
                this.logger?.Info(msg);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0456");
            return guid.GetHashCode();
        }
    }
}
