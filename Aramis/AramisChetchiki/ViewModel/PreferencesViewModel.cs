namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using GongSolutions.Wpf.DragDrop;
    using TMP.Shared.Commands;

    public class PreferencesViewModel : BaseViewModel, IDropTarget
    {
        private string dbPath;
        private string oldPath;
        private string isDBPathValidMessage;
        private Model.AramisDataInfo selectedDataFileInfo;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public PreferencesViewModel()
        {
            this.oldPath = AppSettings.Default.DataFilesStorePath;

            this.CommandAddAramisDbPath = new DelegateCommand(
                () =>
            {
                if (Repository.Instance.ContainsDataInfoCollectionAramisDbPath(this.DBPath))
                {
                    App.ShowWarning("Это подразделение уже добавлено!");
                    return;
                }

                Repository.Instance.AddNewAramisDbPath(this.DBPath);

                // теперь если список файлов был пуст, нужно выбрать добавленный
                List<Common.RepositoryCommon.IDataFileInfo> selected = Repository.Instance.AvailableDataFiles.Where(file => file.IsSelected == true).ToList();
                this.SelectedDataFileInfo = selected.FirstOrDefault() as Model.AramisDataInfo;
            }, () => string.IsNullOrWhiteSpace(this.DBPath) == false && System.IO.Directory.Exists(this.DBPath) && string.IsNullOrEmpty(this.isDBPathValidMessage));

            this.CommandRemoveAramisDbPath = new DelegateCommand(
                () =>
            {
                if (Repository.Instance.ContainsDataInfo(this.SelectedDataFileInfo))
                {
                    try
                    {
                        System.IO.File.Delete(this.SelectedDataFileInfo.FileName);
                    }
                    catch
                    {
                    }

                    Repository.Instance.AvailableDataFiles.Remove(this.SelectedDataFileInfo);
                    this.SelectedDataFileInfo = Repository.Instance.AvailableDataFiles.FirstOrDefault() as Model.AramisDataInfo;
                    if (this.SelectedDataFileInfo != null)
                    {
                        this.SelectedDataFileInfo.IsSelected = true;
                    }
                }
            }, () => this.SelectedDataFileInfo != null);

            this.CommandClearAramisDbPathList = new DelegateCommand(
                () =>
            {
                try
                {
                    foreach (Model.AramisDataInfo d in Repository.Instance.AvailableDataFiles)
                    {
                        System.IO.File.Delete(d.FileName);
                    }
                }
                catch
                {
                }

                Repository.Instance.AvailableDataFiles.Clear();
            }, () => Repository.Instance.AvailableDataFilesCount > 0);

            this.CommandSaveAndClose = new DelegateCommand(() =>
            {
                this.logger?.Info(this.Status = "Сохранение настроек программы ...");
                AppSettings.Default.Save();
                this.logger?.Info("Сохранены!");

                if (string.Equals(this.oldPath, AppSettings.Default.DataFilesStorePath, AppSettings.StringComparisonMethod) == false && MainViewModel.Data != null)
                {
                    Task<Common.RepositoryCommon.SaveStatus> result = Repository.Instance.SaveAsync();

                    this.logger?.Info(result);
                }

                if (this.SelectedDataFileInfo != null)
                {
                    MainViewModel.SelectedDataFileInfo = this.SelectedDataFileInfo;
                }
                else if (Repository.Instance.AvailableDataFiles.Count == 1)
                {
                    MainViewModel.SelectedDataFileInfo = Repository.Instance.AvailableDataFiles[0] as Model.AramisDataInfo;
                }
                else
                {
                    this.ShowDialogWarning("Для работы программы необходимо в разделе 'Расположение данных'\nуказать путь к базе данных программы Арамис\nи выбрать подразделение.");
                }

                MainViewModel.GoBack();
            });

            this.CommandRemoveField = new DelegateCommand<object>((param) =>
            {
                if (param != null && param is System.Collections.IEnumerable list)
                {
                }
            });

            Repository.Instance.PropertyChanged += this.Repository_PropertyChanged;
        }

        private void Repository_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Repository.AvailableDataFilesCount))
            {
                (this.CommandClearAramisDbPathList as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            Repository.Instance.PropertyChanged -= this.Repository_PropertyChanged;
        }

        #region GongSolutions.Wpf.DragDrop IDropTarget implementation

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            object sourceItem = dropInfo.Data;
            object targetItem = dropInfo.TargetItem;

            if (sourceItem != null && targetItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = System.Windows.DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            object sourceItem = dropInfo.Data;
            object targetItem = dropInfo.TargetItem;
            // targetItem.Children.Add(sourceItem);
        }

        #endregion

        #region Properties

        public string DBPath
        {
            get => this.dbPath;
            set
            {
                if (this.SetProperty(ref this.dbPath, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsDBPathValidMessage));
                    this.CheckDbPath();
                    (this.CommandAddAramisDbPath as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string IsDBPathValidMessage
        {
            get => this.isDBPathValidMessage;
            private set
            {
                if (this.SetProperty(ref this.isDBPathValidMessage, value))
                {
                    (this.CommandAddAramisDbPath as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public Model.AramisDataInfo SelectedDataFileInfo
        {
            get => this.selectedDataFileInfo;
            set
            {
                if (this.SetProperty(ref this.selectedDataFileInfo, value))
                {
                    (this.CommandRemoveAramisDbPath as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand CommandAddAramisDbPath { get; }

        public ICommand CommandRemoveAramisDbPath { get; }

        public ICommand CommandClearAramisDbPathList { get; }

        public ICommand CommandSaveAndClose { get; }

        public ICommand CommandRemoveField { get; }

        public Shared.PlusPropertyDescriptor SelectedSourceProperty { get; set; }

        public List<TMPApplication.VisualTheme> VisualThemesList
        {
            get => App.Instance.VisualThemesList;
            set => App.Instance.VisualThemesList = value;
        }

        public TMPApplication.VisualTheme SelectedVisualTheme
        {
            get => App.Instance.SelectedVisualTheme;
            set => App.Instance.SelectedVisualTheme = value;
        }

        #endregion

        #region Public methods

        #endregion

        #region Private methods

        private void CheckDbPath()
        {
            if (string.IsNullOrWhiteSpace(this.dbPath))
            {
                this.IsDBPathValidMessage = null;
            }

            this.IsBusy = true;
            Task.Run(() =>
            {
                try
                {
                    this.DetailedStatus = "проверка наличия в списке";
                    if (Repository.Instance.ContainsDataInfoCollectionAramisDbPath(this.DBPath))
                    {
                        this.IsDBPathValidMessage = "Это подразделение уже добавлено!";
                    }
                    else
                    {
                        this.DetailedStatus = "получение названия подразделения";
                        string departamentName = Repository.GetDepartamentName(this.DBPath);
                        this.IsDBPathValidMessage = departamentName == null ? "Указанный путь не содержит базу данных программы 'Арамис'!" : null;
                    }
                }
                catch (Exception)
                {
                    this.IsDBPathValidMessage = "Неверный путь";
                }
            })
                .ContinueWith((t) =>
                {
                    this.IsBusy = false;
                    this.DetailedStatus = null;
                });
        }

        #endregion
    }
}
