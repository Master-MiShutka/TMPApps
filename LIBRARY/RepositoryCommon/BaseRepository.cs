namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Threading.Tasks;

    public class RepositoryEventArgs : EventArgs
    {
        public IDataFileInfo Info { get; set; }

        public RepositoryAction Action { get; set; }

        public double ProgressValue { get; set; }

        /// <summary>
        /// Описание последней возникшей ошибки
        /// </summary>
        public string Detail { get; set; }

        public static RepositoryEventArgs New(RepositoryAction action)
        {
            return new RepositoryEventArgs
            {
                Action = action,
            };
        }

        public static RepositoryEventArgs New(RepositoryAction action, IDataFileInfo info)
        {
            return new RepositoryEventArgs
            {
                Action = action,
                Info = info,
            };
        }

        public static RepositoryEventArgs UICallback(string message)
        {
            return new RepositoryEventArgs
            {
                Action = RepositoryAction.UICallback,
                Detail = message,
            };
        }

        public static RepositoryEventArgs Error(string message)
        {
            return new RepositoryEventArgs
            {
                Action = RepositoryAction.ErrorOccurred,
                Detail = message,
            };
        }
    }

    public enum RepositoryAction
    {
        Initialized,
        Loading,
        Loaded,
        Saving,
        Saved,
        Updated,
        FoundNewFile,
        ErrorOccurred,
        UICallback,
    }

    public delegate void RepositoryHandler(object sender, RepositoryEventArgs e);

    #region Statuses

    public enum LoadStatus
    {
        Ok,
        Error,
        DataFileNotExists,
        UnknownDataFileFormat,
        UnknownDataFileVersion,
        IOError,
        NotHavePermissions,
        CorruptedDataFile,
    }

    public enum SaveStatus
    {
        Ok,
        Error,
        NotEnoughSpace,
        IOError,
        NotHavePermissions,
    }

    #endregion

    public class BaseRepository<T, TDataFileInfo> : Shared.PropertyChangedBase, IDisposable
        where T : class, IData, new()
        where TDataFileInfo : class, IDataFileInfo, new()
    {
        #region Constants
        public string DATA_DESCRIPTION { get; init; }

        public virtual string DATA_FILENAME => "DATA";

        public virtual string DATA_FILENAME_EXTENSION => ".DATA";

        public virtual string FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME => "lastusedfile";

        public virtual Version LastSupportedVersion => new Version(1, 0, 0, 0);

        private const string PART_Info = "Info";

        #endregion

        #region Fields

        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected Action<Exception> callBackAction;
        private string dataStorePath;
        private T data;
        private readonly Shared.AsyncObservableCollection<IDataFileInfo> availableDataFiles = new();
        protected FileSystemWatcher dataFileStorePathWatcher;

        private object @lock = new object();
        private bool isGettingDataFilesListInDataStore = false;
        private bool isSavingDataInDataStore = false;

        #endregion

        public BaseRepository(string dataStorePath = default)
        {
            this.callBackAction = (e) => logger?.Error(e);

            this.availableDataFiles.CollectionChanged += this.AvailableDataFiles_CollectionChanged;

            if (string.IsNullOrEmpty(dataStorePath) == false)
                this.dataStorePath = dataStorePath;
            else
                this.dataStorePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            Task.Run(async () => await this.InitializationAsync());
        }

        private void AvailableDataFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.AvailableDataFilesCount));
        }

        #region IDisposable implementation

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.availableDataFiles.CollectionChanged -= this.AvailableDataFiles_CollectionChanged;

                    if (this.dataFileStorePathWatcher != null)
                    {
                        this.dataFileStorePathWatcher.EnableRaisingEvents = false;
                        this.dataFileStorePathWatcher.Created -= this.OnDataFileStorePathChanged;
                        this.dataFileStorePathWatcher.Deleted -= this.OnDataFileStoreFileDeleted;
                        this.dataFileStorePathWatcher.Renamed -= this.OnDataFileStoreFileRenamed;
                        this.dataFileStorePathWatcher.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BaseRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Properties

        protected bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Путь к папке с данными
        /// </summary>
        public string DataStorePath
        {
            get
            {
                this.dataStorePath = this.GetDataStorePath();
                if (string.IsNullOrEmpty(this.dataStorePath))
                    this.dataStorePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                return this.dataStorePath;
            }
            set => this.SetDataStorePath(value);
        }

        public T Data
        {
            get => this.data;
            set
            {
                if (this.data == value)
                {
                    return;
                }

                this.data = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.DataFileSize));
                this.Handler?.Invoke(this, new RepositoryEventArgs() { Action = RepositoryAction.Updated });
            }
        }

        /// <summary>
        /// Размер файла данных в понятном виде
        /// </summary>
        public string DataFileSize => File.Exists(this.Data?.Info?.FileName) ? GetFriendlySize(new FileInfo(this.Data.Info.FileName).Length) : string.Empty;

        /// <summary>
        /// Коллекция доступных файлов с данными
        /// </summary>
        public Shared.AsyncObservableCollection<IDataFileInfo> AvailableDataFiles => this.availableDataFiles;

        public int AvailableDataFilesCount => this.AvailableDataFiles != null ? this.AvailableDataFiles.Count : 0;

        public int LocalDataFilesCount => this.AvailableDataFiles != null ? this.AvailableDataFiles.Count(i => i.IsLocal) : 0;

        /// <summary>
        /// Сохранять ли имя последнего используемого файла
        /// </summary>
        public bool MustStoreLastUsedDataFileName { get; set; } = true;

        /// <summary>
        /// Оповещение о событиях
        /// </summary>
        public event RepositoryHandler Handler;

        protected Action<dynamic, T> OnLoadingFromGz { get; set; }

        protected Action<Package, T> OnLoadingFromPackage { get; set; }

        protected Action<T> OnLoaded { get; set; }

        public Action<Package, T> OnSaving { get; set; }

        public Action OnSaved { get; set; }

        #endregion

        #region Private methods

        protected virtual string GetDataStorePath()
        {
            return this.dataStorePath;
        }

        protected virtual void SetDataStorePath(string value)
        {
            logger?.Info($">>> SetDataStorePath: '{value ?? "<none>"}'");
            if (ReferenceEquals(value, this.dataStorePath))
            {
                return;
            }

            this.dataStorePath = value;
            this.RaisePropertyChanged(nameof(this.DataStorePath));

            if (this.IsInitialized == false)
            {
                return;
            }

            // настройка слежения за файлами
            this.InitDataStore();

            // поиск файлов с данными
            new Task(async () =>
            {
                logger?.Info(">>> SetDataStorePath > GetDataFilesListInDataStoreAsync");
                System.Threading.Thread.CurrentThread.Name = "GetDataFilesListInDataStore";
                await this.GetDataFilesListInDataStoreAsync();
            }).Start();
        }

        protected void UpdateUI(string message)
        {
            this.Handler?.Invoke(this, RepositoryEventArgs.UICallback(message));
        }

        /// <summary>
        /// настройка слежения за файлами
        /// </summary>
        /// <returns></returns>
        protected bool InitDataStore()
        {
            try
            {
                string path = this.DataStorePath;
                if (string.IsNullOrWhiteSpace(path) == false && Directory.Exists(path))
                {
                    if (this.dataFileStorePathWatcher == null)
                    {
                        this.dataFileStorePathWatcher = new FileSystemWatcher(path)
                        {
                            NotifyFilter = NotifyFilters.LastWrite
                                | NotifyFilters.FileName,
                            Filter = "*" + this.DATA_FILENAME_EXTENSION,
                            EnableRaisingEvents = true,
                        };
                    }
                    else
                    {
                        this.dataFileStorePathWatcher.Path = path;
                    }

                    this.dataFileStorePathWatcher.Created += this.OnDataFileStorePathChanged;
                    this.dataFileStorePathWatcher.Deleted += this.OnDataFileStoreFileDeleted;
                    this.dataFileStorePathWatcher.Renamed += this.OnDataFileStoreFileRenamed;
                }
            }
            catch (IOException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static string GetFriendlySize(long size)
        {
            string[] sizes = { "байт", "Кб", "Мб", "Гб" };
            int order = 0;
            while (size >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                size /= 1024;
            }

            return string.Format("{0:0.##} {1}", size, sizes[order]);
        }

        private void OnDataFileStorePathChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created && this.AvailableDataFiles != null && this.AvailableDataFiles.Count > 0)
            {
                if (this.isSavingDataInDataStore)
                {
                    logger?.Warn($"Идет сохранение файла '{e.FullPath}'");
                    return;
                }

                try
                {
                    new Task(async () =>
                    {
                        System.Threading.Thread.CurrentThread.Name = "AddNewLocalDataFile";

                        await this.AddNewLocalDataFileAsync(e.FullPath);

                        if (await this.IsSupportedDataFileAsync(e.FullPath))
                        {
                            IDataFileInfo dataFileInfo = this.LoadDataFileInfoFromPackageAsync(e.FullPath).Result;
                            if (dataFileInfo != null)
                            {
                                this.Handler?.Invoke(this, RepositoryEventArgs.New(RepositoryAction.FoundNewFile, dataFileInfo));
                            }
                        }
                    }).Start();
                }
                catch (Exception ex)
                {
                    logger?.Error(ex.Message);
                }
            }
        }

        private void OnDataFileStoreFileDeleted(object source, FileSystemEventArgs e)
        {
            if (this.AvailableDataFiles != null && this.AvailableDataFiles.Count > 0)
            {
                try
                {
                    var dataFiles = this.AvailableDataFiles.Where(f => f.FileName == e.Name);
                    foreach (var dataFile in dataFiles)
                    {
                        this.AvailableDataFiles.Remove(dataFile);
                    }
                }
                catch (Exception ex)
                {
                    logger?.Error(ex.Message);
                }
            }
        }

        private void OnDataFileStoreFileRenamed(object source, RenamedEventArgs e)
        {
            if (this.AvailableDataFiles != null && this.AvailableDataFiles.Count > 0)
            {
                var dataFiles = this.AvailableDataFiles.Where(f => f.FileName == e.OldName);
                foreach (var dataFile in dataFiles)
                {
                    dataFile.FileName = e.Name;
                }
            }
        }

        private async Task<List<IDataFileInfo>> GetDataFilesListInDataStoreAsync()
        {
            if (this.isGettingDataFilesListInDataStore)
            {
                logger?.Trace($"GetDataFilesListInDataStoreAsync exit, already running, thread '{System.Threading.Thread.CurrentThread.ManagedThreadId}'");

                return null;
            }

            lock (this.@lock)
            {
                this.isGettingDataFilesListInDataStore = true;
            }

            logger?.Trace($"GetDataFilesListInDataStoreAsync, thread '{System.Threading.Thread.CurrentThread.ManagedThreadId}'");

            // составляем список файлов с данными
            List<IDataFileInfo> dataFilesList = new List<IDataFileInfo>();

            var di = new DirectoryInfo(this.DataStorePath);
            List<FileInfo> files = null;
            try
            {
                files = di.GetFiles("*" + this.DATA_FILENAME_EXTENSION).OrderByDescending(p => p.LastWriteTime).ToList();
                foreach (FileInfo fi in files)
                {
                    if (await this.IsSupportedDataFileAsync(fi.FullName))
                    {
                        IDataFileInfo dataFileInfo = await this.LoadDataFileInfoFromPackageAsync(fi.FullName);
                        if (dataFileInfo != null)
                        {
                            dataFilesList.Add(dataFileInfo);
                        }
                    }
                }

                // если найдено более одного
                if (dataFilesList.Count >= 1)
                {
                    this.AvailableDataFiles.Clear();
                    dataFilesList.ForEach(item => this.AvailableDataFiles.Add(item));
                }
                else
                {
                    this.AvailableDataFiles.Clear();
                }
            }
            catch (Exception e)
            {
                logger?.Error(e);
            }

            return dataFilesList
                .OrderByDescending(i => i.LastModifiedDate)
                .ToList();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Проверка наличия и загрузка последнего файла
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InitializationAsync()
        {
            logger?.Info(">>> Initialization > start");

            this.InitDataStore();

            // поиск файлов с данными
            logger?.Info(">>> Initialization > GetDataFilesListInDataStoreAsync");
            var list = await this.GetDataFilesListInDataStoreAsync();

            IDataFileInfo dataFileInfo = null;
            if (list != null && list.Count != 0)
            {
                dataFileInfo = await this.LoadAsync();
            }

            RepositoryEventArgs repositoryEventArgs = new RepositoryEventArgs
            {
                Action = RepositoryAction.Initialized,
                Detail = string.Empty,
            };
            if (dataFileInfo != null)
            {
                repositoryEventArgs.Info = dataFileInfo;
            }

            this.Handler?.Invoke(this, repositoryEventArgs);

            logger?.Info(">>> BaseRepository>Initialization > end");
            this.IsInitialized = true;
            return true;
        }

        #region **** Load ****

        /// <summary>
        /// Загрузка данных
        /// </summary>
        /// <returns>True если загрузка произошла успешно</returns>
        public async Task<IDataFileInfo> LoadAsync()
        {
            logger?.Info(">>> Load > start");

            IDataFileInfo dataFileInfo = default;

            // если существует файл, хранящий имя последнего используемого файла с данными
            if (File.Exists(this.FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME))
            {
                try
                {
                    // чтение этого имени
                    string fileName = File.ReadAllText(this.FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME);

                    // проверка на наличие
                    dataFileInfo = this.AvailableDataFiles.Where(i => i.FileName == Path.GetFileName(fileName)).FirstOrDefault();
                    if (dataFileInfo != null)
                    {
                        // загрузка
                        var result = await this.LoadAsync(fileName);
                        if (result != LoadStatus.Ok)
                        {
                            // если файл не загружен, возможно поврежден, попытаемся загрузить данные из базы
                            dataFileInfo.FileName = string.Empty;

                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.callBackAction(ex);
                    this.Handler?.Invoke(this, RepositoryEventArgs.Error(Utils.GetExceptionDetails(ex)));
                }
            }
            else
            {
                if (this.AvailableDataFiles.Count == 1)
                {
                    // загрузка
                    dataFileInfo = this.AvailableDataFiles[0];
                    var result = await this.LoadAsync(dataFileInfo.FileName);
                    if (result != LoadStatus.Ok)
                    {
                        // если файл не загружен, возможно поврежден, попытаемся загрузить данные из базы
                        dataFileInfo.FileName = string.Empty;

                        return null;
                    }
                }
            }

            return dataFileInfo;
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        /// <param name="fileName">Имя файла, если не указано, то загрузка из стандартного файла <see cref="DATA_FILENAME"/></param>
        /// <returns>True если загрузка произошла успешно</returns>
        public async Task<LoadStatus> LoadAsync(string fileName = null)
        {
            logger?.Info(">>> LoadDataAsync > start");

            this.Handler?.Invoke(this, RepositoryEventArgs.UICallback("попытка загрузки\nранее сохраненных данных ..."));

            LoadStatus result = LoadStatus.Ok;

            RepositoryEventArgs repositoryEventArgs = new RepositoryEventArgs
            {
                Action = RepositoryAction.Loading,
            };

            this.Handler?.Invoke(this, repositoryEventArgs);

            bool mustStoreLastDataFileName = this.MustStoreLastUsedDataFileName;
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = this.DATA_FILENAME + this.DATA_FILENAME_EXTENSION;
                    mustStoreLastDataFileName = false;
                }
                else
                    if (Path.GetExtension(fileName).ToLowerInvariant() != this.DATA_FILENAME_EXTENSION)
                {
                    fileName += this.DATA_FILENAME_EXTENSION;
                }

                var fi = new FileInfo(Path.Combine(this.DataStorePath, fileName));
                if (fi.Exists == false)
                {
                    this.Handler?.Invoke(this, RepositoryEventArgs.Error($"Файл '{fileName}' в папке '{this.DataStorePath}' не найден!"));
                    return LoadStatus.DataFileNotExists;
                }

                T data = null;

                bool isOldVersionFile = false;
                try
                {
                    if (Path.IsPathRooted(fileName) == false)
                    {
                        fileName = Path.Combine(this.DataStorePath, fileName);
                    }

                    // попытка прочитать файл как пакет
                    using Package package = System.IO.Packaging.Package.Open(fileName, FileMode.Open, FileAccess.Read);
                    IDataFileInfo info = await this.LoadDataFromPackageAsync(this.GetPackagePartFromPartName(package, PART_Info));
                    Version version = info != null ? info.Version : new Version(0, 0);

                    if (this.IsSupportedDataFileVersion(info.Version))
                    {
                        void unknownVersion()
                        {
                            string msg = string.Format("Файл '{1}'\nнеизвестной версии - {0}\nЗагрузка невозможна.\nОбновите программу или обратитесь к разработчику.", info.Version, fi.FullName);
                            logger?.Error(msg);
                            this.Handler?.Invoke(this, RepositoryEventArgs.Error(msg));
                        }

                        logger?.Info($">>> BaseRepository>LoadData1 > файл данных версии {info.Version}");
                        switch (info.Version.Major)
                        {
                            case 1:
                                switch (info.Version.Minor)
                                {
                                    case 0:
                                        isOldVersionFile = true;
                                        break;
                                    case 1:
                                        data = await this.LoadDataFromFileVersion_1_1Async(package);
                                        break;
                                    default:
                                        unknownVersion();
                                        return LoadStatus.UnknownDataFileVersion;
                                }

                                break;
                            default:
                                unknownVersion();
                                return LoadStatus.UnknownDataFileVersion;
                        }
                    }
                }
                catch (IOException ioe)
                {
                    logger?.Error(ioe);
                    isOldVersionFile = true;
                    this.Handler?.Invoke(this, RepositoryEventArgs.Error(Utils.GetExceptionDetails(ioe)));
                }
                catch (System.Data.NoNullAllowedException e)
                {
                    logger?.Error(e);
                    return LoadStatus.CorruptedDataFile;
                }
                catch (Exception e)
                {
                    isOldVersionFile = true;
                    logger?.Error(e);
                }

                if (isOldVersionFile)
                {
                    data = await this.LoadDataFromFileVersion_1_0(fi.FullName);
                }

                if (data != null)
                {
                    if (mustStoreLastDataFileName)
                    {
                        File.WriteAllText(Path.Combine(this.DataStorePath, this.FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME), fileName);
                    }

                    fi = new FileInfo(fileName);
                    data.Info.FileName = fi.Name;
                    data.Info.FileSize = fi.Length;
                    data.Info.IsLoaded = true;
                    data.Info.IsSelected = true;
                    data.Info.LastModifiedDate = fi.LastWriteTime;

                    this.Data = data;

                    this.Handler?.Invoke(this, RepositoryEventArgs.New(RepositoryAction.Loaded, data.Info));
                }

                result = data != null ? LoadStatus.Ok : LoadStatus.Error;
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
                this.Data = null;
                this.Handler?.Invoke(this, RepositoryEventArgs.Error(Utils.GetExceptionDetails(ex)));
            }

            return result;
        }

        /// <summary>
        /// Поддерживается ли файл данных
        /// </summary>
        /// <param name="fileName">Имя файла данных</param>
        /// <returns>True если поддерживается</returns>
        public async Task<bool> IsSupportedDataFileAsync(string fileName)
        {
            return await this.GetDataFileVersionAsync(fileName) >= 0;
        }

        /// <summary>
        /// Определяет поддерживается ли указанная версия файла данных
        /// </summary>
        /// <param name="version">версия файла данных</param>
        /// <returns>True, если поддерживается</returns>
        public bool IsSupportedDataFileVersion(Version version)
        {
            return version == null
                ? false
                : version.CompareTo(this.LastSupportedVersion) switch
                {
                    0 => true,
                    1 => false,
                    -1 => true,
                    _ => false,
                };
        }

        public string CheckLastUsedDataFileStoredAndGetFileName()
        {
            logger?.Info(">>> CheckLastUsedDataFileStoredAndGetFileName");

            // имя файла с данными
            string dataFileNameToLoad = null;

            // проверка: есть ли файл, с которым работали в последний раз
            logger?.Info($"Поиск в папке: '{this.DataStorePath}'");
            if (File.Exists(Path.Combine(this.DataStorePath, this.FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME)))
            {
                logger?.Info($"Обнаружен файл ('{this.FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME}') с именем файла, с которым работали в последний раз");
                var lastusedfile = string.Empty;
                try
                {
                    logger?.Info("Попытка чтения имени файла, с которым работали в последний раз");

                    // чтение имени файла
                    lastusedfile = File.ReadAllText(Path.Combine(this.DataStorePath, this.FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME)).Trim();
                    logger?.Info($"Имя файла, с которым работали в последний раз, получено: '{lastusedfile}'");
                    if (File.Exists(lastusedfile))
                    {
                        dataFileNameToLoad = lastusedfile;
                        logger?.Info("Файл, с которым работали в последний раз, существует, попытаемся его загрузить");
                        return dataFileNameToLoad;
                    }
                    else
                    {
                        logger?.Info("Указанный файл, с которым работали в последний раз, не найден");
                    }
                }
                catch (IOException ex)
                {
                    logger?.Info("Ошибка при чтении имени файла, с которым работали в последний раз");
                    this.callBackAction(ex);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(this.DATA_FILENAME) || File.Exists(Path.Combine(this.DataStorePath, this.DATA_FILENAME + this.DATA_FILENAME_EXTENSION)) == false)
                {
                    logger?.Info("Файл с данными не обнаружен.");
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Загрузка файла данных версии 1.0
        /// </summary>
        /// <remarks>
        /// Файл данных версии 1.0 представляет собой сериализованный в json объект, дополнительно сжатый gzip
        /// </remarks>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Данные</returns>
        protected async Task<T> LoadDataFromFileVersion_1_0(string fileName)
        {
            T data = new T();
            try
            {
                T obj = await JsonDeserializer.FromGzFileAsync<T>(fileName);
                if (obj != null)
                {
                    var fi = new FileInfo(fileName);
                    IDataFileInfo dataFileInfo = obj.Info;
                    dataFileInfo.FileSize = fi.Length;
                    data.Info = dataFileInfo;

                    this.OnLoadingFromGz?.Invoke(obj, data);
                }

                this.OnLoaded?.Invoke(data);
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
            }

            return data;
        }

        /// <summary>
        /// Загрузка файла данных версии 1.1
        /// </summary>
        /// <returns>Данные</returns>
        protected async Task<T> LoadDataFromFileVersion_1_1Async(Package package)
        {
            T data = new T();
            data.Info = await this.LoadDataFromPackageAsync(this.GetPackagePartFromPartName(package, PART_Info));

            if (data.Info == null)
            {
                return null;
            }

            this.OnLoadingFromPackage?.Invoke(package, data);

            this.OnLoaded?.Invoke(data);

            return data;
        }

        protected PackagePart GetPackagePartFromPartName(Package package, string partName)
        {
            return package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri("/" + partName, UriKind.Relative)));
        }

        /// <summary>
        /// Возвращает версию файла данных
        /// </summary>
        /// <param name="fileName">Имя файла данных</param>
        /// <returns>-1 - файл не поддерживается, 0 - файл старого формата, 1 - нового</returns>
        protected async Task<int> GetDataFileVersionAsync(string fileName)
        {
            byte[] zipHeader = { 0x1F, 0x8B, 0x08 };
            byte[] packageHeader = { 0x50, 0x4B, 0x03, 0x04 };

            if (Path.GetExtension(fileName) == this.DATA_FILENAME_EXTENSION)
            {
                logger?.Trace($"GetDataFileVersionAsync '{fileName}', thread '{System.Threading.Thread.CurrentThread.ManagedThreadId}'");

                byte[] bytes = new byte[5];
                using FileStream fs = new FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                using MemoryStream mso = new MemoryStream();
                await fs.ReadAsync(bytes.AsMemory(0, bytes.Length));
                await mso.WriteAsync(bytes.AsMemory(0, bytes.Length));

                mso.Close();
                fs.Close();

                if (bytes.Take(3).SequenceEqual(zipHeader))
                {
                    return this.IsSupportedDataFileVersion(this.LoadDataFileVersionFromZip(fileName)) ? 0 : -1;
                }
                else
                    if (bytes.Take(4).SequenceEqual(packageHeader))
                {
                    try
                    {
                        Version version = await this.LoadDataFileVersionFromPackageAsync(fileName);
                        return this.IsSupportedDataFileVersion(version) ? 1 : -1;
                    }
                    catch (IOException ioe)
                    {
                        logger?.Error(ioe);
                        return -1;
                    }
                    catch (FileFormatException ffe)
                    {
                        logger?.Error(ffe);
                        return -1;
                    }
                    catch
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Чтение версии из файла данных старого типа
        /// </summary>
        /// <param name="fileName">имя файла данных</param>
        /// <returns>Версия файла данных</returns>
        protected Version LoadDataFileVersionFromZip(string fileName)
        {
            try
            {
                dynamic obj = (dynamic)JsonDeserializer.FromGzFileAsync<object>(fileName);
                return obj != null ? (Version)obj.Version : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Возвращает информацию о данных
        /// </summary>
        /// <param name="fileName">Имя файла данных</param>
        /// <returns>Версия файла данных</returns>
        protected async Task<Version> LoadDataFileVersionFromPackageAsync(string fileName)
        {
            try
            {
                IDataFileInfo info = await this.LoadDataFileInfoFromPackageAsync(fileName);
                return info != null ? info.Version : new Version(0, 0);
            }
            catch (FileFormatException)
            {
                return this.LoadDataFileVersionFromZip(fileName);
            }
            catch (IOException ioe)
            {
                logger?.Error(ioe);
                return default;
            }
            catch
            {
                return default;
            }
        }

        protected async Task<IDataFileInfo> LoadDataFileInfoFromPackageAsync(string fileName)
        {
            logger?.Trace($"LoadDataFileInfoFromPackage '{fileName}', thread '{System.Threading.Thread.CurrentThread.ManagedThreadId}'");
            try
            {
                using Package package = Package.Open(fileName, FileMode.Open, FileAccess.Read);
                TDataFileInfo info = await this.LoadDataFromPackageAsync(this.GetPackagePartFromPartName(package, PART_Info));

                if (info != null)
                {
                    var fi = new FileInfo(fileName);
                    info.FileName = fi.Name;
                    info.FileSize = fi.Length;
                    info.LastModifiedDate = fi.LastWriteTime;
                }

                return info;
            }
            catch (FileFormatException)
            {
                return null;
            }
            catch (IOException ioe)
            {
                logger?.Error(ioe);
                return default;
            }
            catch (Exception e)
            {
                logger?.Error(e);
                return default;
            }
        }

        protected async Task<TDataFileInfo> LoadDataFromPackageAsync(PackagePart packagePart)
        {
            try
            {
                TDataFileInfo result = await MessagePackDeserializer.FromStreamAsync<TDataFileInfo>(packagePart.GetStream());
                return result;
            }
            catch (IOException ioe)
            {
                this.callBackAction(ioe);
                return default;
            }
            catch (Exception e)
            {
                this.callBackAction(e);
                return default;
            }
        }

        protected async Task<TIModel[]> LoadDataListFromPackageAsync<TIModel>(Package package, string partName)
        {
            try
            {
                if (package.PartExists(new Uri("/" + partName, UriKind.Relative)) == false)
                {
                    logger?.Error($"Not found part '{partName}' in package.");
                }

                Stream stream = this.GetPackagePartFromPartName(package, partName).GetStream();

                IEnumerable<TIModel> result = await MessagePackDeserializer.FromStreamAsync<List<TIModel>>(stream);

                return result?.ToArray();
            }
            catch (IOException ioe)
            {
                this.callBackAction(ioe);
                return default;
            }
            catch (Exception e)
            {
                this.callBackAction(e);
                return default;
            }
        }

        protected async Task<Dictionary<TKey, IList<TIModel>>> LoadDictionaryFromPackageAsync<TKey, TIModel>(Package package, string partName)
        {
            try
            {
                if (package.PartExists(new Uri("/" + partName, UriKind.Relative)) == false)
                {
                    logger?.Error($"Not found part '{partName}' in package.");
                }

                Stream stream = this.GetPackagePartFromPartName(package, partName).GetStream();

                Dictionary<TKey, IList<TIModel>> result = await MessagePackDeserializer.FromStreamAsync<Dictionary<TKey, IList<TIModel>>>(stream);

                return result;
            }
            catch (IOException ioe)
            {
                this.callBackAction(ioe);
                return default;
            }
            catch (Exception e)
            {
                this.callBackAction(e);
                return default;
            }
        }

        #endregion

        #region **** Save ****

        /// <summary>
        /// Серилизация данных и сохранение в файл
        /// </summary>
        public async Task<SaveStatus> SaveAsync()
        {
            logger?.Info(">>> BaseRepository>Save > start");

            SaveStatus result = SaveStatus.Ok;

            if (this.Data == null)
            {
                throw new InvalidOperationException();
            }

            this.Handler?.Invoke(this, RepositoryEventArgs.New(RepositoryAction.Saving));

            string path = string.Empty;

            // в параметрах указан путь?
            if (string.IsNullOrWhiteSpace(this.DataStorePath))
            {
                // нет, не указан, проверка наличия разрешения на запись в папку с исполняемым файлом программы
                path = Utils.ExecutionPath;
                if (CheckPermissionToWriteToFolder(path) == false)
                {
                    // проверка наличия разрешения на запись в папку AppData папки пользователя
                    path = Utils.AppDataSettingsPath;
                    if (CheckPermissionToWriteToFolder(path) == false)
                    {
                        this.Handler?.Invoke(this, RepositoryEventArgs.UICallback(string.Format("Отсутствуют разрешения на запись в папку '{0}'.{1}Укажите папку, доступную на запись, в параметрах программы и сохраните данные.",
                                                path, Environment.NewLine)));
                        return SaveStatus.NotHavePermissions;
                    }
                }
            }

            if (Directory.Exists(path) == false)
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (IOException ioex)
                {
                    this.callBackAction(ioex);
                    this.Handler?.Invoke(this, RepositoryEventArgs.UICallback(string.Format("Не удалось создать папку '{0}'.{2}Данные не сохранены.{2}Устраните проблему и сохраните данные.\nПодробное описание ошибки:\n{1}",
                        path, Utils.GetExceptionDetails(ioex), Environment.NewLine)));
                    return SaveStatus.IOError;
                }
            }

            string fileName = System.IO.Path.Combine(path, this.Data.Info.FileName);

            try
            {
                if (await this.SaveAsAsync(fileName) == false)
                {
                    return SaveStatus.Error;
                }

                this.Handler?.Invoke(this, RepositoryEventArgs.New(RepositoryAction.Saved, this.Data.Info));

                result = SaveStatus.Ok;
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
                this.Handler?.Invoke(this, RepositoryEventArgs.Error(Utils.GetExceptionDetails(ex)));
                return SaveStatus.Error;
            }

            return result;
        }

        /// <summary>
        /// Сохранение данных в указанный файл
        /// </summary>
        /// <param name="newFileName">Имя файла</param>
        /// <returns>True если сохранение произошло успешно</returns>
        public async Task<bool> SaveAsAsync(string newFileName)
        {
            logger?.Info(">>> BaseRepository>SaveAs > start");

            bool result = await this.SaveDataAsync(newFileName);

            return result;
        }

        /// <summary>
        /// Создание резервной копии данных
        /// </summary>
        public async Task SaveBackupAsync()
        {
            logger?.Info(">>> BaseRepository>SaveBackup > start");

            await this.SaveDataAsync(this.DATA_FILENAME + this.DATA_FILENAME_EXTENSION + ".bak");
        }

        /// <summary>
        /// Сохранение данных
        /// </summary>
        /// <param name="fileName">Имя файла, если не указано, то загрузка из стандартного файла <see cref="DATA_FILENAME"/></param>
        /// <returns>True если сохранение произошло успешно</returns>
        protected async Task<bool> SaveDataAsync(string fileName = null)
        {
            logger?.Info(">>> BaseRepository>SaveData > start");

            if (this.Data == null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                this.isSavingDataInDataStore = true;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = this.DATA_FILENAME + this.DATA_FILENAME_EXTENSION;
                }

                this.Data.Info.FileName = fileName;
                this.RaisePropertyChanged(nameof(this.DataFileSize));

                using (Package package = System.IO.Packaging.Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    package.PackageProperties.ContentType = "application/json";
                    package.PackageProperties.Created = DateTime.Now;
                    package.PackageProperties.Creator = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                    package.PackageProperties.Description = this.DATA_DESCRIPTION;
                    package.PackageProperties.Version = this.Data.Info.Version.ToString();

                    // Общая информация
                    byte[] bytes = await MessagePackSerializer.ToBytesAsync(this.Data.Info);

                    this.SaveJsonDataToPart(bytes, package, PART_Info);

                    this.OnSaving?.Invoke(package, this.Data);
                }

                // обновляем размер данных
                this.Data.Info.FileSize = new FileInfo(fileName).Length;

                this.OnSaved?.Invoke();
                this.Handler?.Invoke(this, RepositoryEventArgs.New(RepositoryAction.Saved, this.Data.Info));

                // сохранение имени файла последней сессии
                if (this.MustStoreLastUsedDataFileName)
                {
                    File.WriteAllText(Path.Combine(this.DataStorePath, this.FILENAME_WHAT_STOTORED_LAST_USED_DATA_FILENAME), fileName);
                }
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
                this.Handler?.Invoke(this, RepositoryEventArgs.Error(Utils.GetExceptionDetails(ex)));
                return false;
            }
            finally
            {
                this.isSavingDataInDataStore = false;
            }

            return true;
        }

        protected void SaveJsonDataToPart(byte[] bytes, Package package, string partName)
        {
            PackagePart part = package.CreatePart(
                PackUriHelper.CreatePartUri(new Uri(partName, UriKind.Relative)),
                "application/messagepack", // "application/json"
                CompressionOption.SuperFast);
            using Stream outStream = part.GetStream();
            outStream.Write(bytes, 0, bytes.Length);
        }

        protected bool ContainsDataInfoCollectionDataFileName(string fileName)
        {
            if (this.AvailableDataFiles != null && this.AvailableDataFiles.Count > 0)
            {
                var dataFiles = this.AvailableDataFiles
                    .Where(f => f.FileName == fileName)
                    .ToList();
                return dataFiles.Count > 0;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region Others

        protected virtual async Task AddNewLocalDataFileAsync(string fullFileNamePath)
        {
            logger?.Info(">>> BaseRepository>AddNewLocalDataFile");

            if (this.ContainsDataInfoCollectionDataFileName(fullFileNamePath))
            {
                return;
            }

            if (await this.IsSupportedDataFileAsync(fullFileNamePath))
            {
                IDataFileInfo dataFileInfo = await this.LoadDataFileInfoFromPackageAsync(fullFileNamePath);
                if (dataFileInfo != null)
                {
                    this.AvailableDataFiles.Add(dataFileInfo);
                }
            }
        }

        // Проверка наличия разрешения на запись в указанную папку
        public static bool CheckPermissionToWriteToFolder(string path)
        {
#pragma warning disable SYSLIB0003 // Type or member is obsolete
            var permissionSet = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            var writePermission = new System.Security.Permissions.FileIOPermission(System.Security.Permissions.FileIOPermissionAccess.Write, path);
            permissionSet.AddPermission(writePermission);
            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
#pragma warning restore SYSLIB0003 // Type or member is obsolete
        }

        #endregion
    }
}
