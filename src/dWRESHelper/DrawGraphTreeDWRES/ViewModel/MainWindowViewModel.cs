using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using TMP.DWRES.Graph;
using TMP.Shared;
using Model = TMP.DWRES.Objects;

namespace TMP.DWRES.ViewModel
{
    public enum StateType
    {
        /// <summary>
        /// Схема построена
        /// </summary>
        Ready,
        /// <summary>
        /// База данных подключена
        /// </summary>
        Loaded,
        NoData,
        /// <summary>
        /// Идёт построение
        /// </summary>
        Building
    }

    public partial class MainWindowViewModel : PropertyChangedBase
    {
        #region Data

        private ICollection<Model.Line> _lines;
        private ICollection<Model.Line> _linesWithNames;
        private FiderGraph _modelgraph;

        private GUI.FiderSchemeTableWindow _schemeTableWindow;

        private Model.EnergoSystem _selectedEnergoSystem;

        private Model.Fider _selectedFider;

        private Model.Filial _selectedFilial;

        private Model.Res _selectedRes;

        private Model.Substation _selectedSubstation;

        private Model.Tp _selectedTp;

        private StateType _state = StateType.NoData;

        // настройки
        private TMP.DWRES.GUI.UserPreferences _userPrefs = new TMP.DWRES.GUI.UserPreferences();
        private string _waitText;

        private ObservableCollection<Model.Tp> _tps = new ObservableCollection<Model.Tp>();
        #endregion Data

        #region Ctor

        private static MainWindowViewModel instance;



        private MainWindowViewModel()
        {
            _dbLoaded = false;
            _state = StateType.NoData;
        }

        public static MainWindowViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainWindowViewModel();
                }
                return instance;
            }
        }

        #endregion Ctor

        #region Public Methods

        public void ShowFiderSchemeTable()
        {
            if (_schemeTableWindow == null)
            {
                _schemeTableWindow = new GUI.FiderSchemeTableWindow();
                _schemeTableWindow.Owner = Application.Current.MainWindow;
                _schemeTableWindow.Closing += delegate { _schemeTableWindow = null; };
                _schemeTableWindow.Show();
            }
            else
                if (_schemeTableWindow.IsInitialized)
                _schemeTableWindow.Activate();
        }

        #endregion Public Methods

        #region Public Properties

        public string AppVersion
        {
            get
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            }
        }

        public string Copyright
        {
            get { return "© 2015, Ведущий инженер отдела сбыта\r\nэлектроэнергии Ошмянских ЭС\r\nТрус Михаил Петрович"; }
        }

        public ICollection<Model.Line> Lines
        {
            get { return _lines; }
            set { SetProperty<ICollection<Model.Line>>(ref _lines, value, "Lines"); }
        }

        public ICollection<Model.Line> LinesWithNames
        {
            get { return _linesWithNames; }
            set { SetProperty<ICollection<Model.Line>>(ref _linesWithNames, value, "LinesWithNames"); }
        }

        public string MainDescription
        {
            get { return "Эта программа предназначена для поиска ошибок в топологии сети программы dWRES"; }
        }

        public string MainTitle
        {
            get { return "Поиск ошибок в топологии сети программы dWRES"; }
        }

        public Window MainWindow { get; set; }

        public FiderGraph ModelGraph
        {
            get { return _modelgraph; }
            set { SetProperty<FiderGraph>(ref _modelgraph, value, "ModelGraph"); }
        }

        public TMP.DWRES.GUI.UserPreferences UserPrefs
        {
            get { return _userPrefs; }
        }

        public string WaitText
        {
            get { return _waitText; }
            set { SetProperty<string>(ref _waitText, value, "WaitText"); }
        }
        #region | Списки объектов DwRes |

        public ICollection<Model.EnergoSystem> EnergoSystems
        {
            get
            {
                if (DBLoaded == false) return null;
                return _dwh.GetEnergoSystems();
            }
            //set { SetProp<ObservableCollection<Model.EnergoSystem>>(ref _energoSystems, value, "EnergoSystems"); }
        }
        public ICollection<Model.Fider> Fiders
        {
            get
            {
                if (DBLoaded == false) return null;
                if (SelectedSubstation == null) return null;
                else
                    return SelectedSubstation.Fiders;
            }
            //set { SetProp<ObservableCollection<Model.Fider>>(ref _fiders, value, "Fiders"); }
        }

        public ICollection<Model.Filial> Filials
        {
            get
            {
                if (DBLoaded == false) return null;
                if (SelectedEnergoSystem == null) return null;
                else return SelectedEnergoSystem.Filials;
            }
            //set { SetProp<ObservableCollection<Model.Filial>>(ref _filials, value, "Filials"); }
        }
        public ICollection<Model.Res> Reses
        {
            get
            {
                if (DBLoaded == false) return null;
                if (SelectedFilial == null) return null;
                else return SelectedFilial.Reses;
            }
            //set { SetProp<ObservableCollection<Model.Res>>(ref _reses, value, "Reses"); }
        }
        public ICollection<Model.Substation> Substations
        {
            get
            {
                if (DBLoaded == false) return null;
                if (SelectedRes == null) return null;
                else return SelectedRes.Substations;
            }
            //set { SetProp<ObservableCollection<Model.Substation>>(ref _substations, value, "Substations"); }
        }
        public ObservableCollection<Model.Tp> TPs
        {
            get { return _tps; }
            set { _tps = value; RaisePropertyChanged("TPs"); }
        }

        #endregion

        #region | Выбранные объекты DwRes |

        public Model.EnergoSystem SelectedEnergoSystem
        {
            get { return _selectedEnergoSystem; }
            set
            {
                SetProperty<Model.EnergoSystem>(ref _selectedEnergoSystem, value, "SelectedEnergoSystem");
                RaisePropertyChanged("Filials");
            }
        }
        public Model.Fider SelectedFider
        {
            get { return _selectedFider; }
            set
            {
                SetProperty<Model.Fider>(ref _selectedFider, value, "SelectedFider");
                RaisePropertyChanged("TPs");
                if (value != null)
                    BuildFiderGraph();
            }
        }

        public Model.Filial SelectedFilial
        {
            get { return _selectedFilial; }
            set
            {
                SetProperty<Model.Filial>(ref _selectedFilial, value, "SelectedFilial");
                RaisePropertyChanged("Reses");
            }
        }
        public Model.Res SelectedRes
        {
            get { return _selectedRes; }
            set
            {
                SetProperty<Model.Res>(ref _selectedRes, value, "SelectedRes");
                RaisePropertyChanged("Substations");
            }
        }
        public Model.Substation SelectedSubstation
        {
            get { return _selectedSubstation; }
            set
            {
                SetProperty<Model.Substation>(ref _selectedSubstation, value, "SelectedSubstation");
                _state = StateType.Loaded;
                RaisePropertyChanged("State");
                RaisePropertyChanged("Fiders");
                _selectedFider = null;
                RaisePropertyChanged("SelectedFider");
            }
        }
        public Model.Tp SelectedTp
        {
            get { return _selectedTp; }
            set
            {
                SetProperty<Model.Tp>(ref _selectedTp, value, "SelectedTp");
            }
        }
        public StateType State
        {
            get { return _state; }
            set
            {
                if (value == StateType.Loaded)
                {
                    RaisePropertyChanged("EnergoSystems");
                    /*EnergoSystems = new ObservableCollection<Model.EnergoSystem>(_dwh.GetEnergoSystems());
                    SelectedEnergoSystem = EnergoSystems[0];*/
                }
                if (value == StateType.NoData)
                {
                    _selectedEnergoSystem = null;
                    _selectedFilial = null;
                    _selectedRes = null;
                    _selectedSubstation = null;
                    _selectedFider = null;
                    RaisePropertyChanged("SelectedEnergoSystem");
                    RaisePropertyChanged("SelectedFilial");
                    RaisePropertyChanged("SelectedRes");
                    RaisePropertyChanged("SelectedSubstation");
                    RaisePropertyChanged("SelectedFider");
                }
                SetProperty<StateType>(ref _state, value, "State");
            }
        }
        #endregion | Выбранные объекты DwRes |

        #endregion Public Properties
    }
}