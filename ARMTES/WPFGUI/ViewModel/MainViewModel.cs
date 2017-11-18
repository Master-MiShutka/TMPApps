using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;

using TMP.Shared;

namespace TMP.ARMTES
{
    using Model;
    public class MainViewModel : PropertyChangedBase
    {
        #region | Fields |

        private bool _notAuthorized = true;
        private bool _hostAvailable = false;
        private SectorType _sectorType = SectorType.SES;
        private ProfileType _profileType = ProfileType.Days;

        private DateTime _startDate;
        private DateTime _endDate;
        
        private List<string> _exportList;

        private ArmtesElement _selectedElement;
        private Collector _selectedCollector = null;

        private bool _quitConfirmationEnabled = true;

        private ObservableCollection<Collector> _collectors;
        private Statistics _statistics;

        private bool _isFullDataLoaded = false;
        private bool _updatingProcessStarted = false;

        private FilterType _filter = FilterType.All;

        /// <summary>
        /// e0 - дерево согласно принадлежности к филиалу
        /// 0 - дерево согласно географического положения
        /// </summary>
        private string _rootElementId = "e0";

        private static MainViewModel instance;

        #endregion | Fields |

        static MainViewModel()
        {
            instance = new MainViewModel();
        }

        public static MainViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainViewModel();
                }
                return instance;
            }
        }

        public MainViewModel()
        {
            // авторизованы
            Authorized = false;

            Collectors = new ObservableCollection<Collector>();// this.GenerateCollectors();
            
            _exportList = new List<string>();
            _exportList.Add("Вся таблица");
            _exportList.Add("В расчётную группу");

            Statistics = new Statistics();

        }

        public enum FilterType
        {
            All,
            Answered,
            NotAnswered,
            Missing,
            WrongSettings,
            MissingPersonalAccount,
            Unknown
        }
        
        #region | Сообщения |
        public void ShowMessage(string message)
        {
            ShowMessage(message, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowMessage(string message, MessageBoxButton button, MessageBoxImage image)
        {
            MessageBox.Show(message, Title, button, image);
        }
        #endregion

        #region | Команды |

        #endregion

        #region | Constants |

        public string Title { get { return "АРМТЕС"; } }
        public string DataFileName { get { return "data"; } }

        #endregion | Constants |

        #region | Properties  |

        public bool Authorized
        {
            get { return _notAuthorized; }
            set
            {
                if (value.Equals(_notAuthorized)) return;
                _notAuthorized = value;
                OnPropertyChanged("Authorized");
            }
        }

        public bool IsShutdown { get; set; }

        public bool HostAvailable
        {
            get { return _hostAvailable; }
            set
            {
                _hostAvailable = value;
                OnPropertyChanged("HostAvailable");
            }
        }

        public string RootElementId
        {
            get { return _rootElementId; }
            set
            {
                _rootElementId = value;
                OnPropertyChanged("RootElementId");
            }
        }

        public SectorType SectorType
        {
            get { return _sectorType; }
            set
            {
                _sectorType = value;
                OnPropertyChanged("SectorType");
            }
        }

        public ProfileType ProfileType
        {
            get { return _profileType; }
            set
            {
                _profileType = value;
                OnPropertyChanged("ProfileType");
            }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                OnPropertyChanged("EndDate");
            }
        }
        
        public List<string> ExportList
        {
            get { return _exportList; }
            set
            {
                _exportList = value;
                OnPropertyChanged("ExportList");
            }
        }

        public ObservableCollection<Collector> Collectors
        {
            get { return _collectors; }
            set
            {
                SetProperty<ObservableCollection<Collector>>(ref _collectors, value, "Collectors");
                OnPropertyChanged("HasData");
                OnPropertyChanged("FilteredCollectors");

                if (value == null)
                {
                    _selectedCollector = null;
                    OnPropertyChanged("SelectedCollector");
                }
            }
        }
        public Collector SelectedCollector
        {
            get { return _selectedCollector; }
            set
            {
                if (value == null)
                    return;
                SetProperty<Collector>(ref _selectedCollector, value, "SelectedCollector");
            }
        }

        public FilterType Filter
        {
            get { return _filter; }
            set
            {
                SetProperty<FilterType>(ref _filter, value, "Filter");
                OnPropertyChanged("FilteredCollectors");
            }
        }

        public ICollectionView FilteredCollectors
        {
            get
            {
                var cvs = new CollectionViewSource { Source = Collectors };

                /*cvs.SortDescriptions.Add(
                    new SortDescription("Contract", ListSortDirection.Ascending));*/
                if (cvs.View != null)
                cvs.View.Filter = item =>
                {
                    var c = item as Collector;
                    switch (Filter)
                    {
                        case FilterType.All:
                            return true;
                        case FilterType.Answered:
                            return c.IsAnswered == true;
                        case FilterType.NotAnswered:
                            return c.IsAnswered == false | (c.IsUSPD & c.IsAnsweredUSPD == false);
                        case FilterType.Missing:
                            return c.HasMissingData == true;
                        case FilterType.WrongSettings:
                            return c.Objects.Any(o => o.Counters.Any(counter => counter.HasWrongSettings));
                        case FilterType.MissingPersonalAccount:
                            return c.Objects.Any(o => o.Counters.Any(counter => counter.MissingPersonalAccount));
                        case FilterType.Unknown:
                            return c.Objects.Any(o => String.IsNullOrEmpty(o.Contract));
                        default:
                            return true;
                    }
                };
                return cvs.View;                
            }
        }


        public Statistics Statistics
        {
            get { return _statistics; }
            set { SetProperty<Statistics>(ref _statistics, value, "Statistics"); }
        }

        public ArmtesElement SelectedElement
        {
            get { return _selectedElement; }
            set 
            { 
                SetProperty<ArmtesElement>(ref _selectedElement, value, "SelectedElement");
                OnPropertyChanged("SelectedDepartament");
                OnPropertyChanged("CurrentPath");
            }
        }

        public string SelectedDepartament
        {
            get
            {
                if (SelectedElement == null) return null;
                ArmtesElement element = SelectedElement;

                if (element.ParentId.Length == 1) return element.Label;

                if (element.Parent == null) return element.Label;

                try
                {
                    while (element.Parent.ParentId.Length != 1 && element != null) element = element.Parent;
                }
                catch { return element.Label; }

                return element.Parent.Label;
            }
        }
        public string CurrentPath
        {
            get
            {
                if (SelectedElement == null) return null;
                ArmtesElement element = SelectedElement;

                string result = String.Empty;
                try
                {
                    while (element != null)
                    {
                        result = "\\" + element.Label + result;
                        element = element.Parent;
                    }
                }
                catch { return result; }

                return result;
            }
        }

        public bool QuitConfirmationEnabled
        {
            get { return _quitConfirmationEnabled; }
            set { SetProperty<bool>(ref _quitConfirmationEnabled, value, "QuitConfirmationEnabled"); }
        }

        public bool HasData
        {
            get { return _collectors == null ? false : true; }
        }

        public bool IsFullDataLoaded
        {
            get { return _isFullDataLoaded; }
            set { SetProperty<bool>(ref _isFullDataLoaded, value, "IsFullDataLoaded"); }
        }
        public bool UpdatingProcessStarted
        {
            get { return _updatingProcessStarted; }
            set { SetProperty<bool>(ref _updatingProcessStarted, value, "UpdatingProcessStarted"); }
        }


        private List<AccentColorMenuData> _accentColors;
        private List<AppThemeMenuData> _appThemes;
        public List<AccentColorMenuData> AccentColors
        {
            get { return _accentColors; }
            set { SetProperty<List<AccentColorMenuData>>(ref _accentColors, value, "AccentColors"); }
        }
        public List<AppThemeMenuData> AppThemes
        {
            get { return _appThemes; }
            set { SetProperty<List<AppThemeMenuData>>(ref _appThemes, value, "AppThemes"); }
        }

        #endregion
    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }

        private ICommand changeAccentCommand;

        public ICommand ChangeAccentCommand
        {
            get { return this.changeAccentCommand ?? (changeAccentCommand = new SimpleCommand { CanExecuteDelegate = x => true, ExecuteDelegate = x => this.DoChangeTheme(x) }); }
        }

        protected virtual void DoChangeTheme(object sender)
        {
            var theme = TMP.Wpf.CommonControls.ThemeManager.DetectAppStyle(Application.Current);
            var accent = TMP.Wpf.CommonControls.ThemeManager.GetAccent(this.Name);
            TMP.Wpf.CommonControls.ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme(object sender)
        {
            var theme = TMP.Wpf.CommonControls.ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = TMP.Wpf.CommonControls.ThemeManager.GetAppTheme(this.Name);
            TMP.Wpf.CommonControls.ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }
    }

    public class SimpleCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true; // if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }
    }
}