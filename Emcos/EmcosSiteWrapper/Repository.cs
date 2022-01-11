using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace TMP.Work.Emcos
{
    using TMP.Shared;
    using TMP.Work.Emcos.Model;
    using TMP.Work.Emcos.Model.Balance;

    public partial class Repository : PropertyChangedBase
    {
        #region Constants

        public readonly string SETTINGS_FOLDER = "Settings";
        public readonly string SESSIONS_FOLDER = "Sessions";
        public readonly string SESSION_FILE_EXTENSION = ".session-data";

        public readonly string CONFIGURATION_FILENAME;
        public readonly string BALANCE_SESSION_FILENAME;

        public readonly Version LastSupportedVersion = new Version("1.1");

        #endregion

        #region Fields

#if DEBUG
        private Action<Exception> _callBackAction = (e) => System.Diagnostics.Debug.Fail(EmcosSiteWrapperApp.GetExceptionDetails(e));
#else
        private Action<Exception> _callBackAction = App.LogException;
#endif

        BalanceSession _activeSession;
        Properties.Settings _settings = Properties.Settings.Default;

        IList<BalanceSessionInfo> _sessionsInfoList;
        IList<IHierarchicalEmcosPoint> _pointsCollection;

        #endregion

        #region Singleton

        private static Repository _instance;

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static Repository()
        {
        }

        private Repository()
        {
            CONFIGURATION_FILENAME = Path.Combine(SETTINGS_FOLDER, _settings.BalancePointsFileName);
            BALANCE_SESSION_FILENAME = _settings.SessionFileName;


        }

        public static Repository Instance
        {
            get { return _instance ?? (_instance = new Repository()); }
        }

        ~Repository()
        {
            if (ActiveSession != null && ActiveSession.Info.Period != null)
            {
                Save();
            }
        }

        #endregion

        #region Private methods

        private IHierarchicalEmcosPoint GetPointById(int id)
        {
            return _pointsCollection.Where((p) => p.Id == id).FirstOrDefault();
        }

        private void ProcessPointsToFloatCollection(IHierarchicalEmcosPoint point, ref IList<IHierarchicalEmcosPoint> collection)
        {
            if (point == null) return;
            collection.Add(point);
            if (point.Children != null)
                foreach (Model.EmcosPoint child in point.Children)
                    ProcessPointsToFloatCollection(child, ref collection);
        }

        // функция для обновления свойств в списке точек
        void PointPropertyChangedEventHandler(object sender, PropertyChangedEventArgs args)
        {
            var bi = sender as IBalanceItem;
            if (bi == null)
                return;
            IHierarchicalEmcosPoint point = point = _pointsCollection.Where((p) => p.Id == bi.Id).FirstOrDefault();
            if (point != null)
                switch (args.PropertyName)
                {
                    case "Description":
                        point.Description = bi.Description;
                        break;
                    case "Id":
                        point.Id = bi.Id;
                        break;
                    case "Code":
                        point.Code = bi.Code;
                        break;
                    case "Name":
                        point.Name = bi.Name;
                        break;
                }
        }

        void ActiveSession_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Info")
                RaisePropertyChanged("ActiveSession");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Инициализация - загрузка точек измерений, загрузка последней сессии
        /// </summary>
        public void Init()
        {
            try
            {
                // Проверка наличия папок согласно настроек и наличия файла с описанием точек
                if (System.IO.Directory.Exists(SESSIONS_FOLDER) == false)
                    Directory.CreateDirectory(SESSIONS_FOLDER);

                if (System.IO.Directory.Exists(SETTINGS_FOLDER) == false)
                    Directory.CreateDirectory(SETTINGS_FOLDER);

                if (File.Exists(CONFIGURATION_FILENAME) == false)
                {
                    EmcosSiteWrapperApp.LogInfo("Файл с точками не найден.");
                    // попытка построить дерево точек из сервиса

                    int rootCode = _settings.RootEmcosPointCode;


                    //var bytes = Properties.Resources.DataModel_xml_gz;
                    //Stream stream = new MemoryStream(bytes);
                    //var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, false);
                    //using (FileStream fs = new FileStream(LIST_Balance_POINTS_FILENAME, FileMode.Create))
                    //{
                    //    gzip.CopyTo(fs);
                    //}
                    //File.WriteAllBytes(LIST_Balance_POINTS_FILENAME, Properties.Resources.DataModel_xml_gz);
                }
            }
            catch (Exception ex)
            {
                _callBackAction(ex);
            }
            // создание в фоне списка сессий
            var task = System.Threading.Tasks.Task.Factory.StartNew(() => FillSessionsList());

            // чтение конфигурации
            if (LoadConfiguration() == false)
                EmcosSiteWrapperApp.LogWarning("Конфигурация не загружена");
            else
                EmcosSiteWrapperApp.LogWarning("Конфигурация загружена");

            // загрузка сессии
            Load();
        }

        /// <summary>
        /// Текущая сессия имеет стандартное имя - т.е. ещё не сохранялась
        /// </summary>
        /// <returns>True если сессия имеет стандартное имя</returns>
        public bool HasSessionDefaultFileName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(ActiveSession.Info.FileName) == BALANCE_SESSION_FILENAME;
        }
        /// <summary>
        /// Создание списка сессий
        /// </summary>
        public void FillSessionsList()
        {
            SessionsInfoList = new List<BalanceSessionInfo>();
            var di = new DirectoryInfo(SESSIONS_FOLDER);
            try
            {
                var files = di.GetFiles().OrderByDescending(p => p.LastWriteTime).ToArray();
                foreach (FileInfo fi in files)
                {
                    int versionNumber = GetSessionVersion(fi.FullName);
                    if (versionNumber >= 0)
                    {
                        BalanceSessionInfo balanceSessionInfo = LoadSessionInfo(fi.FullName, versionNumber);
                        SessionsInfoList.Add(balanceSessionInfo);
                    }
                }
            }
            catch (Exception e)
            {
                _callBackAction(e);
            }
        }
        /// <summary>
        /// Создание пустой сессии
        /// </summary>
        public void CreateEmptySession()
        {
            ActiveSession = new BalanceSession(ConfigPoints, ConfigOtherPoints);
        }

        /// <summary>
        /// Возвращает формулу расчёта баланса группы
        /// </summary>
        /// <param name="groupId">Ид группы в Emcos</param>
        /// <returns>Формула баланса</returns>
        public BalanceFormula GetGroupBalanceFormula(int groupId)
        {
            if (BalanceGroupsFormulaById.ContainsKey(groupId))
                return BalanceGroupsFormulaById[groupId];
            else
                return BalanceFormula.CreateDefault();
        }
        /// <summary>
        /// Обновление или добавление в словарь формулы расчёта баланса группы
        /// </summary>
        /// <param name="groupId">Ид группы в Emcos</param>
        /// <param name="balanceFormula">Формула баланса</param>
        public void UpdateGroupBalanceFormula(int groupId, BalanceFormula balanceFormula)
        {
            if (BalanceGroupsFormulaById.ContainsKey(groupId))
                BalanceGroupsFormulaById[groupId] = balanceFormula;
            else
                BalanceGroupsFormulaById.Add(groupId, balanceFormula);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Оповещение о завершении операции загрузки данных
        /// </summary>
        public event EventHandler Loaded;
        /// <summary>
        /// Оповещение о завершении операции сохранения данных
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Активная загруженная сессия
        /// </summary>
        public BalanceSession ActiveSession
        {
            get => _activeSession;
            private set
            {
                // отписка
                if (_activeSession != null && _activeSession.BalancePoints != null)
                {
                    _activeSession.PropertyChanged -= ActiveSession_PropertyChanged;

                    foreach (IHierarchicalEmcosPoint point in _activeSession.BalancePoints)
                    {
                        // подпись на изменение свойств
                        if (point.Children.FlatItemsList != null)
                            foreach (IHierarchicalEmcosPoint i in point.Children.FlatItemsList)
                            {
                                i.PropertyChanged -= PointPropertyChangedEventHandler;
                                //i.SetSubstation(point);
                            }
                        point.PropertyChanged -= PointPropertyChangedEventHandler;
                    }
                }
                // задание значения
                SetProp(ref _activeSession, value, "ActiveSession");
                // подписка
                if (_activeSession != null)
                {
                    if (_activeSession.BalancePoints != null)
                    {
                        // обновление дочерних элементов
                        foreach (IHierarchicalEmcosPoint point in _activeSession.BalancePoints)
                        {
                            // подпись на изменение свойств
                            if (point.Children.FlatItemsList != null)
                                foreach (IHierarchicalEmcosPoint i in point.Children.FlatItemsList)
                                {
                                    i.PropertyChanged += PointPropertyChangedEventHandler;
                                    //i.SetSubstation(point);
                                }
                            point.PropertyChanged += PointPropertyChangedEventHandler;
                        }

                        {
                            var groups = _activeSession.Substations.FlatItemsList
                                .Where(i => i.ElementType == ElementTypes.SECTION || i.ElementType == ElementTypes.SUBSTATION)
                                .ToList();
                            foreach (var item in groups)
                                BalanceGroupsFormulaById.Add(item.Id, BalanceFormula.CreateDefault());
                        }

                        // задание формул баланса
                        if (BalanceGroupsFormulaById != null)
                        {
                            var groups = _activeSession.BalancePoints.FlatItemsList
                                .Where(i => i.ElementType == ElementTypes.SECTION || i.ElementType == ElementTypes.SUBSTATION)
                                .Where (i => i is IBalanceGroupItem)
                                .Cast<IBalanceGroupItem>()
                                .ToList();
                            foreach (var item in BalanceGroupsFormulaById)
                            {
                                IBalanceGroupItem group = groups.Where(i => i.Id == item.Key).FirstOrDefault();
                                if (group != null)
                                    group.Formula = item.Value;
                                else
                                {
                                    ;
                                }
                            }
                        }
                    }

                    _activeSession.PropertyChanged += ActiveSession_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Список сессий
        /// </summary>
        public IList<BalanceSessionInfo> SessionsInfoList
        {
            get
            {
                if (_sessionsInfoList == null)
                    FillSessionsList();
                return _sessionsInfoList;
            }
            set
            {
                SetProp(ref _sessionsInfoList, value, "SessionsInfoList");
            }
        }

        #region Конфигурация

        /// <summary>
        /// Коллекция конфигурационных точек
        /// </summary>
        [Magic]
        public HierarchicalEmcosPointCollection ConfigPoints { get; private set; }

        /// <summary>
        /// Словарь пар код группы - формула расчёта баланса
        /// </summary>
        [Magic]
        public Dictionary<int, BalanceFormula> BalanceGroupsFormulaById { get; private set; } = new Dictionary<int, BalanceFormula>();

        public IEnumerable<IHierarchicalEmcosPoint> ConfigOtherPoints
        {
            get
            {
                IList<IHierarchicalEmcosPoint> result = new List<IHierarchicalEmcosPoint>();
                foreach (IHierarchicalEmcosPoint point in ConfigPoints)
                {
                    if (point.ElementType != ElementTypes.DEPARTAMENT && point.TypeCode != "RES" && point.Children != null)
                    {
                        result.Add(point);
                    }
                }
                return result;
            }
        }

        #endregion

        #endregion
    }
}
