using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WaitableWindow
    {
        #region Fields

        private bool _isReadyToGetData = false;
        private ObservableCollection<ListPoint> _pointsList;
        private ICollectionView _view;

        private Task _backgroudTask;
        private CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// ИД корневой точки с которой строится дерево точек, 60 - Ошмянские ЭС
        /// Id 52 Белэнерго Code 1
        /// Id 53 РУП Гродноэнерго Code 14
        /// Id 60 Ошмянские ЭС Code 143
        /// </summary>
        private readonly ListPoint DEFAULT_ROOT_EMCOS_GROUP = new ListPoint { Id = 60, TypeCode = "FES", Name = "Ошмянские ЭС" };
        private ListPoint _rootEmcosGroup;

        #endregion

        #region Constructor

        public MainWindow()
        {
            // настройка сервиса
            App.InitServiceClient(String.Format("http://{0}/{1}",
                    Properties.Settings.Default.ServerAddress,
                    Properties.Settings.Default.ServiceName));

            InitializeComponent();
            DataContext = this;

            App.UIDispatcher = Dispatcher.CurrentDispatcher;

            cts = new CancellationTokenSource();

            InitCommands();

            this.Loaded += (s, e) =>
            {
                var list = LoadList();
                if (list != null)
                {
                    PointsList = new ObservableCollection<ListPoint>(list);
                    IsReadyToGetData = true;
                }
                else
                {
                    UpdateCommand.Execute(null);
                }
            };
            this.Closing += async (s, e) =>
            {
                if (_backgroudTask != null)
                {
                    if (_backgroudTask.IsCompleted == false)
                    {
                        e.Cancel = true;
                        if (cts != null) cts.Cancel();
                        ShowWaitingScreen(Strings.TerminatingMessage);
                        await _backgroudTask;
                        this.Close();
                    }
                }
            };
            this.Closed += (s, e) =>
            {
                if (_pointsList != null)
                    SaveList(_pointsList.ToList());
            };
        }

        #endregion

        #region Private Methods

        private void InitCommands()
        {
            SettingsCommand = new DelegateCommand(o =>
            {
                Action updateUI = () =>
                {
                    DialogHost = null;
                };
                SettingsControl settings = new SettingsControl(updateUI);
                DialogHost = settings;
            });

            UpdateCommand = new DelegateCommand(async o =>
            {
                ShowWaitingScreen(Strings.UpdatingInProgress);

                if (await CheckAvailability() == false)
                {
                    ClearDialogHost();
                    return;
                }
                try
                {
                    if (App.ShowInfo(Strings.OnUpdatingPointsList) == MessageBoxResult.OK)
                    {
                        _rootEmcosGroup = null;
                        try
                        {
                            _rootEmcosGroup = App.Base64StringToObject<ListPoint>(Properties.Settings.Default.RootPoint);
                        }
                        catch
                        { }
                        if (_rootEmcosGroup == null)
                            _rootEmcosGroup = DEFAULT_ROOT_EMCOS_GROUP;


                        _backgroudTask = Task.Factory.StartNew(async () =>
                        {
                            _index = 0;

                            var source = await FillPointsTree(_rootEmcosGroup);
                            if (source.Count == 0 && String.IsNullOrEmpty(ServiceHelper.ErrorMessage) == false)
                            {
                                App.UIAction(() =>
                                {
                                    App.ShowWarning(ServiceHelper.ErrorMessage);
                                    ClearDialogHost();
                                });
                                return;
                            }
                            PointsList = new ObservableCollection<ListPoint>(source);
                            App.UIAction(() =>
                            {
                                IsReadyToGetData = true;
                                SaveList(_pointsList.ToList());
                                ClearDialogHost();
                            });
                        }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    }
                    else
                        ClearDialogHost();
                }
                catch (Exception ex)
                {
                    ClearDialogHost();
                    App.ShowError(String.Format(Strings.Error, App.GetExceptionDetails(ex)));
                }
            });
            GetReportCommand = new DelegateCommand(async o =>
            {
                App.ShowInfo(Strings.OnGettingData);

                if (await CheckAvailability() == false)
                {
                    ClearDialogHost();
                    return;
                }

                var list = HierarchicalListToFlatList(_pointsList.ToList());

                var substations = list
                    .Where(i => (i.TypeCode == "SUBSTATION" || i.TypeCode == "VOLTAGE") && i.Checked)
                    .OrderBy(i => i.Name).ToList<ListPoint>();
                if (substations == null || substations.Count == 0)
                {
                    App.ShowWarning(Strings.EmptyList);
                    return;
                }

                btnPanel.IsEnabled = false;
                try
                {
                    App.Current.MainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo()
                    {
                        Description = Strings.GetDataHeader,
                        ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal,
                        ProgressValue = 0.01
                    };
                    Action updateUI = () =>
                    {
                        btnPanel.IsEnabled = true;
                        ClearDialogHost();
                    };
                    Action completed = () =>
                    {
                        App.UIAction(() =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                            updateUI();
                        });                        
                    };
                    Action canceled = () =>
                    {
                        App.UIAction(() =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                            App.ShowInfo(Strings.Canceled);
                            updateUI();
                        });                        
                    };
                    Action<Exception> faulted = (Exception e) =>
                    {
                        App.UIAction(() =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                            App.ShowError(e, null);
                            updateUI();
                        });                        
                    };

                    GetDataControl get = new GetDataControl(substations, completed, canceled, faulted);
                    DialogHost = get;
                }
                catch (Exception ex)
                {
                    App.ShowError(ex, null);
                    btnPanel.IsEnabled = true;
                    ClearDialogHost();
                }
            },
            o => IsReadyToGetData);
            GetAuxiliariesCommand = new DelegateCommand(async o =>
            {
                App.ShowInfo(Strings.OnGettingData);

                if (await CheckAvailability() == false)
                {
                    ClearDialogHost();
                    return;
                }

                var list = HierarchicalListToFlatList(_pointsList.ToList());

                var auxiliary = list
                    .Where(i => i.ParentTypeCode == "AUXILIARY" && i.TypeCode == "ELECTRICITY" && i.EсpName == "Свои нужды")
                    .OrderBy(i => i.ParentName)
                    .OrderBy(i => i.Name)
                    .ToList<ListPoint>();
                if (auxiliary == null || auxiliary.Count == 0)
                {
                    App.ShowWarning(Strings.EmptyList);
                    return;
                }

                btnPanel.IsEnabled = false;
                try
                {
                    App.Current.MainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                    App.Current.MainWindow.TaskbarItemInfo.Description = Strings.GetDataHeader;
                    App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    App.Current.MainWindow.TaskbarItemInfo.ProgressValue = 0.01;

                    Action updateUI = () =>
                    {
                        btnPanel.IsEnabled = true;
                        ClearDialogHost();
                    };
                    Action completed = () =>
                    {
                        App.UIAction(() =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                            updateUI();
                        });
                    };
                    Action canceled = () =>
                    {
                        App.UIAction(() =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                            App.ShowInfo(Strings.Canceled);
                            updateUI();
                        });
                    };
                    Action<Exception> faulted = (Exception e) =>
                    {
                        App.UIAction(() =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                            App.ShowError(e, null);
                            updateUI();
                        });
                    };

                    GetAuxiliaryControl get = new GetAuxiliaryControl(auxiliary, completed, canceled, faulted);
                    DialogHost = get;
                }
                catch (Exception ex)
                {
                    App.ShowError(ex, null);
                    btnPanel.IsEnabled = true;
                    ClearDialogHost();
                }
            },
            o => IsReadyToGetData);

            TreeCheckOrUncheckItemsCommand = new DelegateCommand(
                o =>
                {
                    TreeView tree = o as TreeView;
                    if (tree != null)
                    {
                        ListPoint point = (ListPoint)tree.SelectedItem;
                        if (point != null && point.Items != null)
                        {
                            foreach (var child in point.Items)
                                if (child.TypeCode == "FES" || child.TypeCode == "RES" || child.TypeCode == "SUBSTATION" || child.TypeCode == "VOLTAGE")
                                    child.Checked = !child.Checked;
                        }
                    }
                },
                o => PointsList != null && PointsList.Count > 0);
            TreeUnselectAllCommand = new DelegateCommand(
                o =>
                {
                    if (PointsList != null)
                    {
                        foreach (var item in PointsList)
                            ForEachPointInTree(item, p => true, p => p.Checked = false);
                    }
                },
                o => PointsList != null && PointsList.Count > 0);
            TreeSelectAllCommand = new DelegateCommand(
                o =>
                {
                    if (PointsList != null)
                    {
                        foreach (var item in PointsList)
                            ForEachPointInTree(item, p => p.TypeCode == "SUBSTATION" || p.TypeCode == "VOLTAGE", p => p.Checked = true);
                    }
                },
                o => PointsList != null && PointsList.Count > 0);
        }

        private async Task<bool> CheckAvailability()
        {
            if (await ServiceHelper.IsServerOnline())
                return true;
            else
            {
                App.ShowWarning(String.Format(Strings.ServerAvailability, Strings.No));
                return false;
            }
        }

        private readonly int maxItemsCount = 10000;
        private int _index = 0;
        private async Task<IList<ListPoint>> FillPointsTree(ListPoint point)
        {
            var table = App.EmcosWebServiceClient.GetPointInfo("PSDTU_SERVER", point.Id.ToString());

            IList<ListPoint> list = await ServiceHelper.CreatePointsListAsync(point);
            if (list != null && list.Count > 0)
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].IsGroup)
                    {
                        list[i].Items = new ObservableCollection<ListPoint>(await FillPointsTree(list[i]));
                        _index++;
                        if (_index > maxItemsCount) throw new OverflowException("Превышено максимальное количество элементов в дереве - " + maxItemsCount);
                    }
                }
            return list;
        }

        private IList<ListPoint> HierarchicalListToFlatList(IList<ListPoint> source)
        {
            List<ListPoint> result = new List<ListPoint>();
            foreach (var item in source)
            {
                result.Add(item);
                if (item.Items != null && item.Items.Count > 0)
                    result.AddRange(HierarchicalListToFlatList(item.Items));
            }
            return result;
        }

        private IList<ListPoint> LoadList()
        {
            //return Common.RepositoryCommon.BaseRepository<List<ListPoint>>.GzJsonDeSerialize("data.list");
            return App.Base64StringToObject<IList<ListPoint>>(Properties.Settings.Default.PointsList);
        }
        private void SaveList(IList<ListPoint> source)
        {
            try
            {
                Properties.Settings.Default.PointsList = App.ObjectToBase64String(source);
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
            {
                App.ShowError(String.Format(Strings.ErrorOnSavePointsList, App.GetExceptionDetails(e)));
            }
        }

        private void ForEachPointInTree(ListPoint point, Func<ListPoint, bool> condition, Action<ListPoint> action)
        {
            if (condition(point))
                action(point);
            if (point.Items != null && point.Items.Count > 0)
                foreach (var item in point.Items)
                    ForEachPointInTree(item, condition, action);
        }

        #endregion

        #region Command and Properties

        public ICommand SettingsCommand { get; private set; }

        public ICommand UpdateCommand { get; private set; }
        public ICommand GetReportCommand { get; private set; }
        public ICommand GetAuxiliariesCommand { get; private set; }

        public ICommand TreeCheckOrUncheckItemsCommand { get; private set; }
        public ICommand TreeUnselectAllCommand { get; private set; }
        public ICommand TreeSelectAllCommand { get; private set; }

        public bool IsReadyToGetData
        {
            get { return _isReadyToGetData; }
            private set { SetProperty(ref _isReadyToGetData, value); }
        }
        public ObservableCollection<ListPoint> PointsList
        {
            get { return _pointsList; }
            private set
            {
                SetProperty(ref _pointsList, value);
                View = CollectionViewSource.GetDefaultView(_pointsList);
            }
        }

        public ICollectionView View
        {
            get { return _view; }
            private set { SetProperty(ref _view, value); }
        }

        #endregion
    }
}