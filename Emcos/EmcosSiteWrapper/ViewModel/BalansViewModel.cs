/*******************************************************************************
*
* Copyright (C) 2016 Trus Mikhail Petrovich
*
 *******************************************************************************/
namespace TMP.Work.Emcos.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Xml;
    using System.Xml.Serialization;
    using Newtonsoft;

    using Model;
    using Model.Balans;
    using TMP.Common.Logger;
    using TMP.Common.RepositoryCommon;

    public class BalansViewModel : PropertyChangedBase
    {
        #region | Constants |

        public readonly string SETTINGS_FOLDER = "Settings";
        public readonly string REPORTS_FOLDER = "Reports";
        public readonly string SESSIONS_FOLDER = "Sessions";
        public readonly string SESSION_FILE_EXTENSION = ".session-data";        

        public readonly string LIST_BALANS_POINTS_FILENAME;
        public readonly string BALANS_SESSION_FILENAME;

        #endregion | Constants |

        #region Private

        private bool _hasSession = false;

        private Properties.Settings _settings = Properties.Settings.Default;

        private bool SaveConfigPoints()
        {
            return BaseRepository<EmcosPoint>.XmlSerialize(new EmcosPoint { Children = Points }, LIST_BALANS_POINTS_FILENAME, App.ToLogException);
        }

        private List<EmcosPoint> LoadConfigPoints()
        {
            if (File.Exists(LIST_BALANS_POINTS_FILENAME))
            {
                var result = BaseRepository<EmcosPoint>.XmlDeSerialize(LIST_BALANS_POINTS_FILENAME, App.ToLogException);
                return result != null ? result.Children : null;
            }
            else
                return new List<EmcosPoint>();
        }

        private void ProcessPointsToFloatCollection(Model.EmcosPoint point, ref List<Model.EmcosPoint> collection)
        {
            if (point == null) return;
            collection.Add(point);
            if (point.Children != null)
                foreach (Model.EmcosPoint child in point.Children)
                    ProcessPointsToFloatCollection(child, ref collection);
        }

        private void InitializeCollectionsView()
        {
            _substationsCollectionView = CollectionViewSource.GetDefaultView(Substations);

            _substationsCollectionView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
            _substationsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            RaisePropertyChanged("SubstationsCollectionView");
        }

        private EmcosPoint GetPointById(int id)
        {
            return _pointsCollection.Where((p) => p.Id == id).FirstOrDefault();
        }

        // делегат для обновления свойств в списке точек
        public event PropertyChangedEventHandler PointPropertyChangedEventHandler;

        private void UpdateSubstationsList()
        {
            if (Session == null)
                throw new InvalidOperationException();
            if (Session.Substations != null)
            {
                // делегат для обновления свойств в списке точек
                PointPropertyChangedEventHandler = (sender, args) =>
                    {
                        var bi = sender as IBalansItem;
                        if (bi == null)
                            return;
                        EmcosPoint point = point = GetPointById(bi.Id);
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
                                case "Title":
                                    point.Name = bi.Name;
                                    break;
                            }
                    };
                // обновление дочерних элементов подстанций
                foreach (Substation s in Session.Substations)
                {
                    // при десериализации обновление запускается автоматически
                    if (_hasSession == false)
                        s.UpdateChildren();
                    // подпись на изменение свойств                    
                    if (s.Items != null)
                        foreach (IBalansItem i in s.Items)
                        {
                            i.PropertyChanged += PointPropertyChangedEventHandler;
                            i.SetSubstation(s);
                        }

                    //! TODO: удалить существующие группы 'трансформаторы' и 'собственные нужды' и создать их на основе ссылок на элементы из секций шин

                    // собственные нужды
                    IList<IBalansItem> sectionAux = s.Children.Where((c) => c.Type == Model.ElementTypes.AUXILIARY && c.Name == "Собственные нужды").ToList();
                    if (sectionAux == null)
                        continue;
                    foreach (IBalansGroup aux in sectionAux)
                    {
                        if (aux == null || aux.Children == null)
                            continue;
                        foreach (IBalansItem auxChild in aux.Children)
                        {
                            IBalansItem unit = null;
                            if (auxChild is UnitTransformer)
                                unit = auxChild as UnitTransformer;
                            if (unit == null)
                                continue;
                        }
                    }

                    s.PropertyChanged += PointPropertyChangedEventHandler;
                }
                InitializeCollectionsView();
            }
        }

        #endregion Private

        #region Public Methods

        public event System.Windows.RoutedEventHandler Loaded;

        #region | SESSION |

        public bool HasSessionDefaultFileName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(Session.FileName) == BALANS_SESSION_FILENAME;
        }

        public bool LoadSessionData(string filename = null)
        {
            bool mustStoreLastSessionFileName = true;
            try
            {
                if (String.IsNullOrWhiteSpace(filename))
                {
                    filename = BALANS_SESSION_FILENAME + SESSION_FILE_EXTENSION;
                    mustStoreLastSessionFileName = false;
                }
                else
                    if (Path.GetExtension(filename).ToLowerInvariant() != SESSION_FILE_EXTENSION)
                        filename = filename + SESSION_FILE_EXTENSION;

                var fi = new FileInfo(Path.Combine(SESSIONS_FOLDER, filename));

                Session = BaseRepository<BalansSession>.GzJsonDeSerialize(Path.Combine(SESSIONS_FOLDER, filename), App.ToLogException);

                Session.FileName = fi.Name;
                Session.FileSize = fi.Length;
                Session.LastModifiedDate = fi.LastWriteTime;
                Session.IsLoaded = true;

                SelectedPeriod = Session.Period;

                if (Session != null)
                {
                    /*if (Session.Period == null)
                        Session.Period = new DatePeriod(Session.StartDate, Session.EndDate);*/

                    UpdateSubstationsList();

                    _hasSession = true;
                    RaisePropertyChanged("Session");
                    RaisePropertyChanged("WindowTitle");

                    SelectedDepartament = null;

                    // сохранение имени файла последней сессии
                    if (mustStoreLastSessionFileName)
                        File.WriteAllText(Path.Combine(SESSIONS_FOLDER, "lastsession"), filename);

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                App.ToLogException(ex);
                return false;
            }
        }

        public bool SaveSessionData(string fileName = null)
        {
            if (Session == null)
                throw new InvalidOperationException();

            bool mustStoreLastSessionFileName = false;
            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = BALANS_SESSION_FILENAME + SESSION_FILE_EXTENSION;
                try
                {
                    if (File.Exists(Path.Combine(SESSIONS_FOLDER, "lastsession")))
                        File.Delete(Path.Combine(SESSIONS_FOLDER, "lastsession"));
                }
                catch (IOException ex)
                {
                    App.ToLogException(ex);
                }
            }
            else
            {
                if (fileName.EndsWith(".bak") == false)
                {
                    fileName = fileName + SESSION_FILE_EXTENSION;
                    mustStoreLastSessionFileName = true;
                }
            }

            Session.FileName = fileName;
            RaisePropertyChanged("WindowTitle");

            if (BaseRepository<BalansSession>.GzJsonSerialize(
                Session,
                Path.Combine(SESSIONS_FOLDER, fileName),
                App.ToLogException) == false)
                return false;

            // сохранение имени файла последней сессии
            if (mustStoreLastSessionFileName)
                File.WriteAllText(Path.Combine(SESSIONS_FOLDER, "lastsession"), fileName);
            return true;
        }

        public void FillSessionsList()
        {
            if (SessionsList != null)
                foreach (BalansSession session in SessionsList)
                    session.Dispose();
            SessionsList = new List<BalansSession>();
            var di = new DirectoryInfo(SESSIONS_FOLDER);
            try
            {
                var files = di.GetFiles().OrderByDescending(p => p.LastWriteTime).ToArray();
                foreach (FileInfo fi in files)
                if (fi.Extension == SESSION_FILE_EXTENSION)
                    {
                        //LoadSession(fi.FullName);
                        var session = new BalansSession
                        {
                            FileName = fi.Name,
                            FileSize = fi.Length,
                            LastModifiedDate = fi.LastWriteTime,
                            IsLoaded = Session == null ? false : fi.Name == Session.FileName
                        };
                        try
                        {
                            var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
                            var gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false);
                            {
                                var bytes = new byte[1024];
                                int cnt;
                                cnt = gzip.Read(bytes, 0, bytes.Length);
                                if (cnt != bytes.Length)
                                {
                                    App.ToLogError(String.Format("Ошибка чтения 1024 байт файла '{0}'", fi.FullName));
                                    continue;
                                }
                                var data = System.Text.Encoding.UTF8.GetString(bytes);
                                int periodPropertyPos = data.IndexOf(@"""$type"":""TMP.Work.Emcos.Model.DatePeriod, EmcosSiteWrapper"",""StartDate"":");
                                if (periodPropertyPos > 0)
                                {
                                    var part = "{" + data.Substring(periodPropertyPos, 125) + "}";
                                    var period = Common.RepositoryCommon.BaseRepository<DatePeriod>.JsonDeSerializeFromString(part, App.ToLogException);
                                    if (period != null)
                                        session.Period = period;
                                }
                            }
                        }
                        catch (Exception e) { App.ToLogException(e); }
                        SessionsList.Add(session);
                    }
                if (Loaded != null)
                    Loaded(this, null);
            }
            catch (Exception e)
            {
                App.ToLogException(e);
            }
        }
        public void CreateEmptySession()
        {
            Session = new BalansSession
            {
                Period = null,
                // преобразование списка точек в список подстанций
                Substations = Model.ModelConverter.PointToBalansSubstations(new EmcosPoint { Children = _points })
            };
            //
            _hasSession = true;
            SelectedDepartament = null;
        }

        #endregion

        public BalansViewModel() : this(App.Current.MainWindow)
        {

        }

        public BalansViewModel(System.Windows.Window window)
        {
            if (window == null) return;
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(window))
            {
                /*Points = LoadConfigPoints();
                bool sampleFileExists = false;
                if (File.Exists(@"Settings\sample-session.data"))
                    if (LoadSessionData(@"Settings\sample-session.data") == true)
                        sampleFileExists = true;
                if (sampleFileExists == false)
                {
                    // преобразование списока точек в список подстанций
                    //_substations = Model.ModelConverter.PointToBalansSubstations(new EmcosPoint() { Children = _points });
                }*/
                return;
            }
            #region Проверка наличия папок согласно настроек и наличия файла с описанием точек
            try
            {
                if (System.IO.Directory.Exists(SESSIONS_FOLDER) == false)
                    Directory.CreateDirectory(SESSIONS_FOLDER);

                if (System.IO.Directory.Exists(SETTINGS_FOLDER) == false)
                    Directory.CreateDirectory(SETTINGS_FOLDER);
                // var sdcde = App.GetUserAppDataPath();

                if (System.IO.Directory.Exists(REPORTS_FOLDER) == false)
                    Directory.CreateDirectory(REPORTS_FOLDER);

                LIST_BALANS_POINTS_FILENAME = Path.Combine(SETTINGS_FOLDER, _settings.BalansPointsFileName);
                BALANS_SESSION_FILENAME = _settings.SessionFileName;

                if (File.Exists(LIST_BALANS_POINTS_FILENAME) == false)
                {
                    App.ToLogInfo("Файл с точками не найден.");
                    // попытка восстановить из ресурсов
                    var bytes = Properties.Resources.DataModel_xml_gz;
                    Stream stream = new MemoryStream(bytes);
                    var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, false);
                    using (FileStream fs = new FileStream(LIST_BALANS_POINTS_FILENAME, FileMode.Create))
                    {
                        gzip.CopyTo(fs);
                    }

                    //File.WriteAllBytes(LIST_BALANS_POINTS_FILENAME, Properties.Resources.DataModel_xml_gz);
                }
            }
            catch (System.IO.IOException ex)
            {
                App.ToLogException(ex);
            }
            #endregion
            #region Добавление обработчика на закрытие окна
            if (window != null)
                window.Closed += (s, e) =>
                {
                    // сохранение сессии
                    if (SaveSessionData())
                        App.ToLogInfo("Сессия сохранена");
                    else
                        App.ToLogInfo("Сессия не сохранена");
                    // сохранение списка точек
                    if (SaveConfigPoints())
                        App.ToLogInfo("Список точек сохранен");
                    else
                        App.ToLogInfo("Список точек не сохранен");
                };
            #endregion
            // Загрузка точек измерений
            Points = LoadConfigPoints();

            // имя файла с сессией
            string sessionFileNameToLoad = null;
            // проверка есть файл с которым работали в последний раз
            if (File.Exists(Path.Combine(SESSIONS_FOLDER, "lastsession")))
            {
                App.ToLogInfo("Обнаружен файл с именем файла последней сессии.");
                var lastusedfile = string.Empty;
                try
                {
                    // чтение имени файла
                    lastusedfile = File.ReadAllText(Path.Combine(SESSIONS_FOLDER, "lastsession")).Trim();
                    App.ToLogInfo("Имя файла последней сессии получено.");
                    if (File.Exists(lastusedfile))
                    {
                        sessionFileNameToLoad = lastusedfile;
                        App.ToLogInfo("Файл последней сессии существует, попытаемся его загрузить.");
                    }
                    else
                        App.ToLogInfo("Указанный файл последней сессии не найден.");
                }
                catch (System.IO.IOException ex)
                {
                    App.ToLogInfo("Ошибка при чтении файла с именем файла последней сессии.");
                    App.ToLogException(ex);
                }
            }
            else
            {
                if (File.Exists(Path.Combine(SESSIONS_FOLDER, BALANS_SESSION_FILENAME + SESSION_FILE_EXTENSION)) == false)
                {
                    App.ToLogInfo("Сессия не обнаружена.");
                }
            }
            //
            if (LoadSessionData(sessionFileNameToLoad))
                App.ToLogInfo("Сессия обнаружена и загружена.");
            else
                App.ToLogInfo(String.Format("Не удалось загрузить сессию. Файл [{0}].", 
                    sessionFileNameToLoad == null ? BALANS_SESSION_FILENAME + SESSION_FILE_EXTENSION : sessionFileNameToLoad));
            //
            var task = System.Threading.Tasks.Task.Factory.StartNew(() => FillSessionsList());
        }

        public void Raise()
        {
            RaisePropertyChanged("HasData");
            RaisePropertyChanged("SubstationsTree");
            RaisePropertyChanged("WindowTitle");
        }

        public void ClearCurrentSubstations()
        {
            foreach (Substation s in Substations)
            {
                s.Clear();
            }
            RaisePropertyChanged("Substations");
        }
        #endregion Public Methods

        #region | Public Properties |

        private DatePeriod _selectedPeriod;
        public DatePeriod SelectedPeriod
        {
            get
            {
                if (_selectedPeriod == null && Session != null && Session.Period != null)
                    _selectedPeriod = Session.Period;
                return _selectedPeriod;
            }
            set
            {
                if (_selectedPeriod != null && _selectedPeriod.Equals(value)) return;
                _selectedPeriod = value;
                RaisePropertyChanged("SelectedPeriod");
            }
        }

        private BalansSession _session = null;
        public BalansSession Session
        {
            get
            {
                return _session;
            }
            private set
            {
                if (_session != null) _session.Dispose();
                _session = value;
            }
        }
        private IList<BalansSession> _sessionsList;
        public IList<BalansSession> SessionsList
        {
            get
            {
                return _sessionsList;
            }
            private set
            {
                if (_sessionsList != null)
                    foreach (BalansSession session in _sessionsList)
                        session.Dispose();
                _sessionsList = value;
            }
        }


        private List<EmcosPoint> _points;
        private List<EmcosPoint> _pointsCollection;
        public List<EmcosPoint> Points
        {
            get { return _points; }
            set
            {
                _points = null;
                GC.Collect();
                _points = value;
                _pointsCollection = new List<Model.EmcosPoint>();
                foreach (var point in _points)
                {
                    point.Initialize();
                    ProcessPointsToFloatCollection(point, ref _pointsCollection);
                }
                RaisePropertyChanged("Points");
            }
        }
        public IList<EmcosPoint> Departaments
        {
            get
            {
                IList<EmcosPoint> result = Points == null
                    ? null
                    : Points
                       .Where(p => p.TypeCode == "RES")
                       .OrderBy(p1 => p1.Name)
                       .ToList();
                result.Insert(0, new EmcosPoint { Name = "ВСЕ" });
                return new BindableCollection<EmcosPoint>(result);
            }
        }

        private EmcosPoint _selectedDepartament;

        public EmcosPoint SelectedDepartament
        {
            get { return _selectedDepartament == null ? (Departaments == null ? null : Departaments[0]) : _selectedDepartament; }
            set
            {
                _selectedDepartament = value;
                RaisePropertyChanged("SelectedDepartament");
                RaisePropertyChanged("Substations");
                RaisePropertyChanged("SubstationsTree");
                RaisePropertyChanged("HasData");
                InitializeCollectionsView();
            }
        }

        private ICollectionView _substationsCollectionView;

        public ICollectionView SubstationsCollectionView
        {
            get
            {
                return _substationsCollectionView;
            }
        }

        public IList<Substation> Substations
        {
            get
            {
                if (Session == null) return null;
                return new BindableCollection<Substation>((SelectedDepartament == null || SelectedDepartament.Children == null)
                    ? Session.Substations
                        .OrderBy(s => s.Departament)
                        .ThenBy(s => s.Name)
                        .ToList()
                    : Session.Substations
                        .Where(s => s.Departament == SelectedDepartament.Name)
                        .OrderBy(s => s.Name)
                        .ToList());
            }
            private set
            {
                if (Session == null) throw new InvalidOperationException("Substations set: Session == null");
                if (Session.Substations != null)
                    foreach (Substation s in Session.Substations)
                        s.Dispose();
                Session.Substations = value;
                RaisePropertyChanged("Substations");
                RaisePropertyChanged("SubstationsTree");
                RaisePropertyChanged("HasData");
                InitializeCollectionsView();    
            }
        }

        private IBalansItem _selectedBalansItem;
        private TMP.Wpf.Common.Controls.TreeListView.TreeNode _selectedBalansItemNode;

        public TMP.Wpf.Common.Controls.TreeListView.TreeNode SelectedBalansItemNode
        {
            get { return _selectedBalansItemNode; }
            set
            {
                _selectedBalansItemNode = value;
                if (value == null)
                    _selectedBalansItem = null;
                else
                    _selectedBalansItem = value.Tag as IBalansItem;
                RaisePropertyChanged("SelectedBalansItemNode");
                RaisePropertyChanged("SelectedBalansItem");
            }
        }

        public IBalansItem SelectedBalansItem
        {
            get { return _selectedBalansItem; }
        }
        public BalansTreeModel SubstationsTree
        {
            get
            {
                return new BalansTreeModel(Substations);
            }
        }

        public string Title { get { return "Баланс подстанций"; } }
        public string WindowTitle
        {
            get
            {
                return String.Format("{0} :: {1} :: файл сессии '{2}'",
                    Title,
                    Session.Title,
                    (String.IsNullOrEmpty(Session.FileName) ? "<не загружен>" : Session.FileName));
            }
        }

        public ICommand SelectSessionCommand { get; set; }
        public ICommand GetDataCommand { get; set; }
        public ICommand SaveDataCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ICommand OpenAuxiliaryReportCommand { get; set; }

        public ICommand UpdateSubstationDataCommand { get; set; }

        public ICommand SetSubstationToUseMonthValueCommand { get; set; }

        private bool _isGettingData = false;

        public bool IsGettingData
        {
            get { return _isGettingData; }
            set
            {
                _isGettingData = value;
                RaisePropertyChanged("IsGettingData");
            }
        }

        private bool _isCancel = false;

        public bool IsCancel
        {
            get
            {
                return _isCancel;
            }
            set
            {
                _isCancel = value;
                RaisePropertyChanged("IsCancel");
            }
        }

        public bool HasData
        {
            get
            {
                var substations = Substations;
                if ((substations == null || substations.Count == 0)) return false;
                return substations.Any((s) => s.Status == DataStatus.Processed);
            }
        }

        public IList<ICommand> ExportList { get; set; }

        public string AppVersion
        {
            get
            {
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            }
        }
        public string AppDescription
        {
            get { return "нет описания"; }
        }
        public string AppCopyright
        {
            get { return "© 2016, Ведущий инженер отдела сбыта\r\nэлектроэнергии Ошмянских ЭС\r\nТрус Михаил Петрович\r\nЦените и уважайте чужой труд!"; }
        }

        #endregion | Public Properties |
    }
}