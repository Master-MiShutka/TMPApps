using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    using Dialogs = TMPApplication.WpfDialogs.Contracts;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TMPApplication.CustomWpfWindow.WindowWithDialogs, INotifyPropertyChanged
    {
        #region Fields

        private bool _isReadyToGetData = false;
        private TreeModel _treeModel;

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

        #region Constructor and desstructor

        public MainWindow()
        {
            App.Log("Создание главного окна");

            InitializeComponent();
            Title = App.Title;

            DataContext = this;

            // настройка сервиса
            App.InitServiceClient(String.Format("http://{0}/{1}",
                    Properties.Settings.Default.ServerAddress,
                    Properties.Settings.Default.ServiceName));

            cts = new CancellationTokenSource();

            InitCommands();

            this.Loaded += (s, e) =>
            {
                App.Log("Главное окно загружено");
                var points = LoadList();
                if (points != null)
                {
                    InitModel(points);
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
                        else
                        {
                            cts = new CancellationTokenSource();
                            cts.Cancel();
                        }
                        this.ShowDialogWaitingScreen(Strings.TerminatingMessage);
                        await _backgroudTask;                     

                        this.Close();
                    }
                }
            };
            this.Closed += (s, e) =>
            {
                if (_treeModel != null)
                    SaveList(_treeModel.Point.Items);
            };

            TMPApplication.ServiceInjector.Instance.AddService<TMPApplication.WpfDialogs.Contracts.IWindowWithDialogs>(this);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void InitModel(IList<ListPoint> points)
        {
            var model = new TreeModel(new ListPoint() { Name = "ROOT", Items = points });
            UpdateCheackableList(model);
            TreeModel = model;
        }

        private void UpdateCheackableList(ICSharpCode.TreeView.SharpTreeNode node)
        {
            if (node.Children != null && node.Children.Count > 0)
                foreach (ICSharpCode.TreeView.SharpTreeNode child in node.Children)
                    UpdateCheackableList(child);
            if (node.IsCheckable)
            {
                int childCount = node.Children.Count((c) => c.IsCheckable);
                if (childCount == 0)
                    return;
                int checkedChildCount = node.Children.Count((c) => c.IsCheckable && c.IsChecked != null && c.IsChecked.Value);

                if (checkedChildCount == 0)
                    node.IsChecked = false;
                else
                    if (checkedChildCount == childCount)
                        node.IsChecked = true;
                    else
                        node.IsChecked = null;
            }

        }

        private void InitCommands()
        {
            App.Log("Иницилизация команд главного окна");
            SettingsCommand = new DelegateCommand(o =>
            {
                App.Log("Просмотр настроек");
                Dialogs.IDialog dialog = null;
                Action updateUI = () => 
                    dialog.Close();
                var control = new SettingsControl(updateUI);
                dialog = this.DialogCustom(control);
                dialog.Show();
            });

            UpdateCommand = new DelegateCommand(o =>
            {
                cts = new CancellationTokenSource();

                Dialogs.IDialog dialog = this.DialogInfo(Strings.UpdatingInProgress, null, MessageBoxImage.None, TMPApplication.WpfDialogs.DialogMode.Cancel);
                dialog.CloseBehavior = TMPApplication.WpfDialogs.DialogCloseBehavior.ExplicitClose;
                dialog.Cancel = () =>
                {
                    cts.Cancel();
                    dialog.CanCancel = false;
                };
                dialog.Show();

                if (CheckAvailability() == false)
                {
                    this.ShowDialogWarning(Strings.MessageServerNotAvailability, null, () => dialog.Close());
                    return;
                }
                try
                {
                    this.ShowDialogInfo(Strings.OnUpdatingPointsList, null, () =>
                    {
                        App.Log("Получение списка точек от сервиса");
                        _rootEmcosGroup = null;
                        try
                        {
                            _rootEmcosGroup = Utils.Base64StringToObject<ListPoint>(Properties.Settings.Default.RootPoint);
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
                                this.ShowDialogWarning(ServiceHelper.ErrorMessage);
                                dialog.Close();
                                return;
                            }
                            InitModel(source);
                            IsReadyToGetData = true;
                            SaveList(source);
                            dialog.Close();
                        }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    });
                }
                catch (Exception ex)
                {
                    App.Log("Получение списка точек от сервиса - ошибка");
                    dialog.Close();
                    this.ShowDialogError(String.Format(Strings.Error, App.GetExceptionDetails(ex)));
                }
            });
            GetReportCommand = new DelegateCommand(o =>
            {
                this.ShowDialogInfo(Strings.OnGettingData, null, () => 
                {
                    App.Log("Получение отчёта по точкам");

                    if (CheckAvailability() == false)
                    {
                        this.ShowDialogWarning(Strings.MessageServerNotAvailability);
                        return;
                    }

                    var substations = _treeModel.Point.Items
                        .Flatten(i => i.Items)
                        .Where(i => (i.TypeCode == "SUBSTATION" || i.TypeCode == "VOLTAGE") && i.IsChecked)
                        .OrderBy(i => i.ParentName)
                        .ThenBy(i => i.Name)
                        .ToList<ListPoint>();
                    if (substations == null || substations.Count == 0)
                    {
                        this.ShowDialogWarning(Strings.EmptyList);
                        return;
                    }

                    Dialogs.IDialog dialog = null;

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
                            dialog.Close();
                        };
                        Action completed = () =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                            updateUI();
                        };
                        Action canceled = () =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                            this.ShowDialogInfo(Strings.Canceled);
                            updateUI();
                        };
                        Action<Exception> faulted = (Exception e) =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                            this.ShowDialogError(App.GetExceptionDetails(e));
                            updateUI();
                        };

                        dialog = this.DialogCustom(new GetDataControl(substations, completed, canceled, faulted));
                        dialog.Show();
                    }
                    catch (Exception ex)
                    {
                        App.Log("Получение отчётов - ошибка");
                        this.ShowDialogError(App.GetExceptionDetails(ex), null,
                            () => TMPApplication.DispatcherExtensions.InUi(() => btnPanel.IsEnabled = true));
                        dialog.Close();
                    }
                });
            },
            o => IsReadyToGetData);
            GetEnergyCommand = new DelegateCommand(o =>
            {
                this.ShowDialogInfo(Strings.OnGettingData, null, () =>
                {
                    App.Log("Получение суточных значений");
                    if (CheckAvailability() == false)
                    {
                        this.ShowDialogWarning(Strings.MessageServerNotAvailability);
                        return;
                    }

                    Dialogs.IDialog dialog = null;

                    btnPanel.IsEnabled = false;
                    try
                    {
                        App.Current.MainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                        App.Current.MainWindow.TaskbarItemInfo.Description = Strings.GetDataHeader;
                        App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                        App.Current.MainWindow.TaskbarItemInfo.ProgressValue = 0.01;

                        Action updateUI = () =>
                        {
                            TMPApplication.DispatcherExtensions.InUi(() => btnPanel.IsEnabled = true);
                            dialog.Close();
                        };
                        Action completed = () =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                            updateUI();
                        };
                        Action canceled = () =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                            this.ShowDialogInfo(Strings.Canceled, Strings.Message, null);
                            updateUI();
                        };
                        Action<Exception> faulted = (Exception e) =>
                        {
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;

                            this.ShowDialogError(App.GetExceptionDetails(e));
                            updateUI();
                        };

                        dialog = this.DialogCustom(new GetEnergyControl(_treeModel.Point.Items, completed, canceled, faulted));
                        dialog.Show();
                    }
                    catch (Exception ex)
                    {
                        App.Log("Получение суточных значений - ошибка");
                        this.ShowDialogError(App.GetExceptionDetails(ex), null,
                            () => TMPApplication.DispatcherExtensions.InUi(() => btnPanel.IsEnabled = true));
                        dialog.Close();
                    }
                });
            },
            o => IsReadyToGetData);

            TreeCheckOrUncheckItemsCommand = new DelegateCommand(
                o =>
                {
                    ICSharpCode.TreeView.SharpTreeView tree = o as ICSharpCode.TreeView.SharpTreeView;
                    if (tree != null)
                    {
                        ListPoint point = (ListPoint)tree.SelectedItem;
                        if (point != null && point.Items != null)
                        {
                            foreach (var child in point.Items)
                                if (child.TypeCode == "FES" || child.TypeCode == "RES" || child.TypeCode == "SUBSTATION" || child.TypeCode == "VOLTAGE")
                                    child.IsChecked = !child.IsChecked;
                        }
                    }
                },
                o => _treeModel != null && _treeModel.Children != null && _treeModel.Children.Count > 0);
            TreeUnselectAllCommand = new DelegateCommand(
                o =>
                {
                    if (_treeModel != null)
                    {
                        foreach (var item in _treeModel.Point.Items)
                            ForEachPointInTree(item, p => true, p => p.IsChecked = false);
                    }
                },
                o => _treeModel != null && _treeModel.Children != null && _treeModel.Children.Count > 0);
            TreeSelectAllCommand = new DelegateCommand(
                o =>
                {
                    if (_treeModel != null)
                    {
                        foreach (var item in _treeModel.Point.Items)
                            ForEachPointInTree(item, p => p.TypeCode == "SUBSTATION" || p.TypeCode == "VOLTAGE", p => p.IsChecked = true);
                    }
                },
                o => _treeModel != null && _treeModel.Children != null && _treeModel.Children.Count > 0);
        }

        private bool CheckAvailability()
        {
            if (ServiceHelper.IsServerOnline())
                return true;
            else
                return false;
        }

        private readonly int maxItemsCount = 10000;
        private int _index = 0;
        private async Task<IList<ListPoint>> FillPointsTree(ListPoint point)
        {
            if (cts.Token.IsCancellationRequested) return null;

            var table = App.EmcosWebServiceClient.GetPointInfo("PSDTU_SERVER", point.Id.ToString());

            IList<ListPoint> list = await ServiceHelper.CreatePointsListAsync(point);
            if (list != null && list.Count > 0)
                for (int i = 0; i < list.Count; i++)
                {
                    if (cts.Token.IsCancellationRequested) return null;
                    if (list[i].IsGroup)
                    {
                        list[i].Items = new ObservableCollection<ListPoint>(await FillPointsTree(list[i]));
                        _index++;
                        if (_index > maxItemsCount) throw new OverflowException("Превышено максимальное количество элементов в дереве - " + maxItemsCount);
                    }
                }
            if (cts.Token.IsCancellationRequested) return null;
            return list;
        }

        private IList<ListPoint> LoadList()
        {
            App.Log("Загрузка списка точек из параметров");
            //return Common.RepositoryCommon.BaseRepository<List<ListPoint>>.GzJsonDeSerialize("data.list");
            return Utils.Base64StringToObject<IList<ListPoint>>(Properties.Settings.Default.PointsList);
        }
        private void SaveList(IList<ListPoint> source)
        {
            App.Log("Сохранение списка точек");
            try
            {
                Properties.Settings.Default.PointsList = Utils.ObjectToBase64String(source);
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
            {
                this.ShowDialogError(String.Format(Strings.ErrorOnSavePointsList, App.GetExceptionDetails(e)));
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

        public ICommand UpdateCommand { get; private set; }
        public ICommand GetReportCommand { get; private set; }
        public ICommand GetEnergyCommand { get; private set; }

        public ICommand TreeCheckOrUncheckItemsCommand { get; private set; }
        public ICommand TreeUnselectAllCommand { get; private set; }
        public ICommand TreeSelectAllCommand { get; private set; }

        public bool IsReadyToGetData
        {
            get { return _isReadyToGetData; }
            private set { SetProperty(ref _isReadyToGetData, value); }
        }


        public TreeModel TreeModel
        {
            get { return _treeModel; }
            private set
            {
                SetProperty(ref _treeModel, value);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        #region Debugging Aides

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}