using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace TMP.Work.Emcos.View
{
    using TMP.Wpf.Common;
    using TMP.Common.Logger;
    using Model;

    /// <summary>
    /// Interaction logic for BalansView.xaml
    /// </summary>
    public partial class BalansView : Window, IStateObject, IDisposable
    {
        private System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
        private EmcosSite emcosSite = new EmcosSite();
        private ViewModel.BalansViewModel vm;

        #region | Реализация IStateObject |
        public string Log { get { return null; } set {  } }
        private int _progress = 0;
        public int Progress {
            get { return _progress; }
            set { _progress = value; RaisePropertyChanged("Progress"); }
        }
        private State _state = State.Idle;
        public State State { get { return _state; } set { _state = value; RaisePropertyChanged("State");} }

        #endregion

        #region | Реализация INotifyPropertyChanged |

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
#if DEBUG
            App.LOG.Log(String.Format(
                "{0}\t{1}\t{2}",
                "BalansView",
                "RaisePropertyChanged",
                propertyName), Common.Logger.Category.Info, Common.Logger.Priority.None);
#endif
            var e = PropertyChanged;
            if (e != null)
                e(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDisposable

        ~BalansView()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (cts != null)
                cts.Dispose();
            if (vm != null)
                vm.Loaded -= ViewModel_Loaded;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public BalansView()
        {
            App.Log.Log("Инициализация BalansView", Category.Info, Priority.None);

            InitializeComponent();

            vm = new ViewModel.BalansViewModel(this);
            DataContext = vm;

            State = State.Busy;
            vm.Loaded += ViewModel_Loaded;

            vm.PropertyChanged += Vm_PropertyChanged;

            vm.GetDataCommand = new DelegateCommand(GetEmcosArchivesForSubstations);

            vm.SaveDataCommand = new DelegateCommand(() =>
            {
                var result = U.InputBox("Сохранение", "Укажите название сессии", vm.Session.Period.GetFileNameForSaveSession());
                if (String.IsNullOrWhiteSpace(result) == true)
                    return;
                vm.SaveSessionData(result);
                vm.FillSessionsList();
            },
            () => (vm.Session != null && vm.Session.IsLoaded));

            vm.SelectSessionCommand = new TMP.Wpf.Common.DelegateCommand(() =>
            {
                State = State.Busy;
                vm.FillSessionsList();
                var sm = new SessionManager(this, vm);
                var show = sm.ShowDialog();
                if (show.HasValue)
                {
                    if (show.Value)
                    {
                        if (vm.LoadSessionData(sm.SelectedSession.FileName) == false)
                            MessageBox.Show("Не удалось загрузить сессию. ", this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                        if (vm.Session == null)
                        vm.CreateEmptySession();
                }
                else
                    if (vm.Session == null)
                    vm.CreateEmptySession();
                State = State.Idle;
            });

            vm.CancelCommand = new DelegateCommand(() =>
            {
                vm.IsCancel = true;
                cts.Cancel();
            },
            () => !vm.IsCancel);

            vm.ExportList = new List<ICommand>
            {
                new DelegateCommand(() => ExportSubstationsBalans(), "Баланс подстанций"),
                new DelegateCommand(() => BalansExport(), "Для программы 'Balans'"),
                new DelegateCommand(() => ExportFiderAnaliz(), "Для пофидерного анализа")
            };

            vm.OpenAuxiliaryReportCommand = new DelegateCommand(() =>
            {
                IList<AuxiliaryReportItem> model = new List<Model.AuxiliaryReportItem>();

                var list = vm.Substations
                    .GroupBy(i => i.Departament, i => i,
                    (k, g) => new { Departament = k, Substations = g.ToList() })
                    .OrderBy(i => i.Departament)
                    .ToList();

                double? summ = 0d;
                foreach (var item in list)
                {
                    var departament = item.Departament;
                    var dep = new AuxiliaryReportItem();
                    dep.Name = departament;
                    dep.Type = "Departament";

                    dep.Children = new List<AuxiliaryReportItem>();

                    foreach (Model.Balans.Substation substation in item.Substations)
                    {
                        var sub = new AuxiliaryReportItem();
                        sub.Children = new List<AuxiliaryReportItem>();
                        sub.Name = substation.Title;
                        sub.Type = "Substation";

                        summ = null;
                        foreach (Model.Balans.IBalansItem balansItem in substation.Items)
                        {
                            if (balansItem.Type == ElementTypes.UnitTransformer || balansItem.Type == ElementTypes.UnitTransformerBus)
                                sub.Children.Add(new AuxiliaryReportItem { Name = balansItem.Title, Value = balansItem.Eplus });
                        }
                        summ = sub.Children.Sum(i => i.Value.HasValue ? i.Value : 0d);
                        if (summ != null)
                        {
                            if (sub.Value == null) sub.Value = 0d;
                            sub.Value = summ;
                        }
                        dep.Children.Add(sub);
                    }
                    summ = null;
                    summ = dep.Children.Sum(i => i.Value.HasValue ? i.Value : 0d);
                    if (summ != null)
                    {
                        if (dep.Value == null) dep.Value = 0d;
                        dep.Value += summ;
                    }
                    model.Add(dep);
                }
                var arw = new AuxiliaryReportWindow(new AuxiliaryReportTreeModel(model));
                arw.Owner = this;
                arw.ShowInTaskbar = false;

                arw.ShowDialog();
            });


            Func<bool> canExecuteUpdateSubstationDataCommand =
            () =>
            {
                if (vm == null) return false;
                if (vm.SelectedBalansItem == null) return false;
                if (vm.SelectedBalansItem is Model.Balans.IBalansGroup && (vm.SelectedBalansItem as Model.Balans.IBalansGroup).Type == ElementTypes.Substation)
                    return true;
                else
                    return false;
            };

            vm.UpdateSubstationDataCommand = new DelegateCommand(() =>
            {
                Progress = 0;
                var substation = vm.SelectedBalansItem as Model.Balans.Substation;
                if (substation == null) return;

                Action oncompleted = () =>
                {
                    Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None),
                        System.Windows.Threading.DispatcherPriority.Normal);
                    State = State.Idle;
                    //vm.IsGettingData = false;
                    vm.Raise();
                };

                try
                {
                    State = State.Busy;
                    //vm.IsGettingData = true;
                    vm.IsCancel = false;
                    cts = new System.Threading.CancellationTokenSource();

                    substation.Status = Model.DataStatus.Wait;
                    TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;

                    var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        emcosSite.GetDaylyArchiveDataForSubstation(vm.Session.Period, substation, cts, UpdateCallBack), cts.Token);

                    task.ContinueWith((s) =>
                    {
                        if (s.Result == true)
                            Dispatcher.BeginInvoke((Action)(() => MessageBox.Show("Выполнено. ", this.Title, MessageBoxButton.OK, MessageBoxImage.Information)));
                        else
                            CheckOnError();
                        oncompleted();
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);
                    task.ContinueWith((s) =>
                        {
                            Dispatcher.BeginInvoke((Action)(() => MessageBox.Show("Произошла ошибка.\n" + s.Exception, "", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                            oncompleted();
                        }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }
                catch (Exception e)
                {
                    App.Log.Log(String.Format("Обновление данных [{0}]. Ошибка - {1}", substation.Code, e.Message), Category.Exception, Priority.High);
                    MessageBox.Show("Произошла ошибка.\n" + e.Message, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    oncompleted();
                }
            }, canExecuteUpdateSubstationDataCommand);

            vm.SetSubstationToUseMonthValueCommand = new DelegateCommand<System.Windows.Controls.MenuItem>((menuItem) =>
            {
                if (menuItem == null) return;
                var substation = vm.SelectedBalansItem as Model.Balans.Substation;
                if (substation == null) return;
                else
                    substation.UseMonthValue = menuItem.IsChecked;
            }, canExecuteUpdateSubstationDataCommand);
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedPeriod")
            {
                if (vm.SelectedPeriod == vm.Session.Period) return;
                if (MessageBox.Show("Изменён период. Получить данные?", vm.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    vm.Session.Period = vm.SelectedPeriod;
                    vm.GetDataCommand.Execute(null);
                }
                else
                    vm.SelectedPeriod = vm.Session.Period;
            }
        }

        private void CheckOnError()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                switch (emcosSite.Status)
                {
                    case EmcosSite.State.Online:
                        break;
                    case EmcosSite.State.Offline:
                        MessageBox.Show("Произошла ошибка.\n" + emcosSite.LastException.Message, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    case EmcosSite.State.NotAuthorized:
                        break;
                    case EmcosSite.State.Error:
                        break;
                    default:
                        break;
                }
            }));
        }

        private void ViewModel_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                State = State.Idle));
        }

        private void GetEmcosArchivesForSubstations()
        {
            App.Log.Log("Получение архивных данных", Category.Info, Priority.None);

            App.Log.Log("Сохранение текущей сессии", Category.Info, Priority.None);
            vm.SaveSessionData(System.IO.Path.Combine(vm.BALANS_SESSION_FILENAME + vm.SESSION_FILE_EXTENSION + ".bak"));

            cts = new System.Threading.CancellationTokenSource();
            vm.IsCancel = false;
            vm.IsGettingData = true;

            try
            {

                var task = emcosSite.ExecuteAction(cts, GetSubstationsDaylyArchives);

                task.ContinueWith((s) =>
                    {
                        App.Log.Log("Выполнено получение архивных данных", Category.Info, Priority.None);
                        try
                        {
                            Dispatcher.BeginInvoke((Action)(() => MessageBox.Show("Выполнено. ", this.Title, MessageBoxButton.OK, MessageBoxImage.Information)));
                            vm.IsGettingData = false;
                            vm.Raise();
                        }
                        catch (Exception ex)
                        {
                            App.Log.Log("Получение данных. Ошибка - " + ex.Message, Category.Exception, Priority.High);
                        }
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);
                task.ContinueWith((s) =>
                    {
                        App.Log.Log("Прервано получение архивных данных", Category.Info, Priority.None);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("Прервано. ", this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }));
                        vm.IsGettingData = false;
                        vm.Raise();
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                App.Log.Log("Получение архивных данных. Ошибка - " + ex.Message, Category.Exception, Priority.High);
                MessageBox.Show("Произошла ошибка.\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void GetSubstationsDaylyArchives()
        {
            Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal),
                System.Windows.Threading.DispatcherPriority.Normal);
            Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressValue = 0.01),
                System.Windows.Threading.DispatcherPriority.Normal);

            vm.ClearCurrentSubstations();

            foreach (Model.Balans.Substation substation in vm.Substations)
                substation.Status = Model.DataStatus.Wait;
            for (int index = 0; index < vm.Substations.Count; index++)
            {
                if (cts.IsCancellationRequested)
                    break;
                Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressValue = index == 0 ? 0.01 : index / (vm.Substations.Count * 1.0)),
                System.Windows.Threading.DispatcherPriority.Normal);

                var substation = vm.Substations[index];

                emcosSite.GetDaylyArchiveDataForSubstation(vm.Session.Period, substation, cts, UpdateCallBack);
            }
            Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None),
                System.Windows.Threading.DispatcherPriority.Normal);
        }
        private void UpdateCallBack(int current, int total)
        {
            Progress = 100* current / total;
            /*Dispatcher.BeginInvoke((Action)(() =>
                  {
                      ;
                  }));*/
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (vm.IsGettingData) return;
            var bvm = DataContext as ViewModel.BalansViewModel;
            if (bvm == null) return;
            switch (e.Key)
            {
                case Key.F1:
                    bvm.IsGettingData = !bvm.IsGettingData;
                    break;

                case Key.F2:
                    bvm.IsCancel = !bvm.IsCancel;
                    break;
                case Key.F3:
                    if (State == State.Busy)
                        State = State.Idle;
                    else
                        State = State.Busy;
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm != null && vm.Session == null)
                vm.SelectSessionCommand.Execute(sender);
        }

        private void ShowSubstationDetails()
        {
            if (vm.SelectedBalansItem is Model.Balans.SubstationSection || vm.SelectedBalansItem is Model.Balans.Substation)
            {
                var item = vm.SelectedBalansItem as Model.Balans.IBalansGroup;
                var group = new Model.BalansGrop(item, vm.Session.Period);

                var bsv = new BalansSubstationView(group);
                bsv.Owner = this;
                bsv.ShowInTaskbar = false;
                this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                bsv.ShowDialog();
                this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
            else
                return;
        }
        private void ShowItemDetails()
        {
            var biv = new BalansItemView(vm.Session.Period);
            biv.Owner = this;
            biv.ShowInTaskbar = false;
            biv.DataContext = vm;
            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            biv.ShowDialog();
            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
        }

        private void ShowItemDetails(Model.ElementTypes elementType)
        {
            switch (elementType)
            {
                case Model.ElementTypes.Group:
                    break;
                case Model.ElementTypes.Substation:
                    ShowSubstationDetails();
                    break;
                case Model.ElementTypes.Voltage:
                    break;
                case Model.ElementTypes.Section:
                    ShowSubstationDetails();
                    break;
                case Model.ElementTypes.PowerTransformer:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.UnitTransformer:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.UnitTransformerBus:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.Fider:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.PowerTransformers:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.Auxiliary:
                    ShowItemDetails();
                    break;
                default:
                    break;
            }
        }

        private void GroupItemInfo(object sender)
        {
            var tlv = sender as TMP.Wpf.Common.Controls.TreeListView.TreeListView;
            if (tlv == null || tlv.SelectedNode == null) return;
            if (tlv.SelectedNode.Tag == null) return;
            var o = tlv.SelectedNode.Tag;
            var item = o as Model.Balans.IBalansItem;
            if (item == null) return;

            ShowItemDetails(item.Type);
        }

        private void tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GroupItemInfo(sender);
        }

        private void tree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.OriginalSource != null && !(e.OriginalSource is System.Windows.Controls.TextBox))
            {
                GroupItemInfo(sender);
            }
        }

        private string CreateNameForReport(string reportName)
        {
            return String.Format(@"Reports\{0}-{1}{2}", reportName, vm.Session.Period.GetFriendlyDateRange(), ".xlsx");
        }

        /// <summary>
        /// Баланс подстанций
        /// </summary>
        private void ExportSubstationsBalans()
        {
            if (vm.HasData == false)
            {
                MessageBox.Show("Отсутствуют данные.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            State = State.Busy;

            var r = MessageBox.Show(String.Format("Скопировать файл отчёта в папку '{0}'?",
                Properties.Settings.Default.ReportBalansPSFolder),
                "Подготовка отчёта ...", MessageBoxButton.YesNo, MessageBoxImage.Question);

            var task = new System.Threading.Tasks.Task(() =>
            {
                var defaultFileName = CreateNameForReport("out");
                var sbe = new Export.SubstationsBalansExport(
                new Export.ExportInfo
                {
                    Title = "Отчёт по балансам подстанций",
                    StartDate = vm.Session.Period.StartDate,
                    EndDate = vm.Session.Period.EndDate,
                    Substations = vm.Substations
                });
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (sbe.Export(defaultFileName) == false)
                        return;
                }));

                if (r == MessageBoxResult.Yes)
                {
                    if (System.IO.Directory.Exists(Properties.Settings.Default.ReportBalansPSFolder) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(Properties.Settings.Default.ReportBalansPSFolder);
                        }
                        catch (System.IO.IOException ioe)
                        {
                            Dispatcher.BeginInvoke((Action)(() => MessageBox.Show(String.Format("Папка {0} не найдена. При попытке её создать произошла ошибка: {1}.\nФайл отчёта не скопирован.", Properties.Settings.Default.ReportBalansPSFolder, ioe.Message),
                                "Подготовка отчёта ...", MessageBoxButton.OK, MessageBoxImage.Error)));
                        }
                    }
                    var filename = System.IO.Path.Combine(
                        Properties.Settings.Default.ReportBalansPSFolder,
                        String.Format(Properties.Settings.Default.ReportBalansPSFileNameTemplate, vm.Session.Period.StartDate) + ".xlsx");
                    System.IO.File.Copy(defaultFileName, filename, true);
                    System.Diagnostics.Process.Start(filename);
                }
                else
                    System.Diagnostics.Process.Start(defaultFileName);
            }, System.Threading.Tasks.TaskCreationOptions.AttachedToParent);

            task.ContinueWith((t) =>
            {
                State = State.Idle;
                Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None),
                    System.Windows.Threading.DispatcherPriority.Normal);
            }, System.Threading.Tasks.TaskContinuationOptions.None);

            task.ContinueWith((s) =>
            {
                var sb = new System.Text.StringBuilder();
                Exception e = s.Exception.Flatten();
                while (e != null)
                    if (e.InnerException != null)
                    {
                        sb.AppendLine(e.InnerException.Message);
                        e = e.InnerException;
                    }
                App.Log.Log("Экспорт балансов подстанций - ошибка: " + sb.ToString(), Category.Exception, Priority.High);
                Dispatcher.Invoke(new Action(delegate () { MessageBox.Show("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation); }));
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }
        /// <summary>
        /// Список фидеров с расходом для пофидерного анализа
        /// </summary>
        private void ExportFiderAnaliz()
        {
            if (vm.HasData == false)
            {
                MessageBox.Show("Отсутствуют данные.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            State = State.Busy;
            var fileName = CreateNameForReport("FiderAnaliz");
            var task = new System.Threading.Tasks.Task(() =>
            {
                var fa = new Export.FiderAnaliz(
                new Export.ExportInfo
                {
                    Title = "Данные для пофидерного анализа",
                    StartDate = vm.Session.Period.StartDate,
                    EndDate = vm.Session.Period.EndDate,
                    Substations = vm.Substations
                });
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (fa.Export(fileName) == false)
                        return;
                }));
                System.Diagnostics.Process.Start(fileName);
            });

            task.ContinueWith((t) =>
            {
                State = State.Idle;
                Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None),
                    System.Windows.Threading.DispatcherPriority.Normal);
            }, System.Threading.Tasks.TaskContinuationOptions.None);

            task.ContinueWith((s) =>
            {
                var sb = new System.Text.StringBuilder();
                Exception e = s.Exception.Flatten();
                while (e != null)
                    if (e.InnerException != null)
                    {
                        sb.AppendLine(e.InnerException.Message);
                        e = e.InnerException;
                    }
                App.Log.Log("Экспорт для пофидерного анализа - ошибка: " + sb.ToString(), Category.Exception, Priority.High);
                Dispatcher.Invoke(new Action(delegate () { MessageBox.Show("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation); }));
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }
        /// <summary>
        /// Спсиок точек с расходом для программы 'Balans'
        /// </summary>
        private void BalansExport()
        {
            if (vm.HasData == false)
            {
                MessageBox.Show("Отсутствуют данные.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            State = State.Busy;
            var fileName = CreateNameForReport("ForBalans");
            var task = new System.Threading.Tasks.Task(() =>
            {
                var be = new Export.ForBalansExport(
                new Export.ExportInfo
                {
                    Title = "Данные для програмы Balans",
                    StartDate = vm.Session.Period.StartDate,
                    EndDate = vm.Session.Period.EndDate,
                    Substations = vm.Substations
                });
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (be.Export(fileName) == false)
                        return;
                }));
                System.Diagnostics.Process.Start(fileName);
            });

            task.ContinueWith((t) =>
            {
                State = State.Idle;
                Dispatcher.BeginInvoke((Action)(() => this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None),
                    System.Windows.Threading.DispatcherPriority.Normal);
            }, System.Threading.Tasks.TaskContinuationOptions.None);

            task.ContinueWith((s) =>
            {
                var sb = new System.Text.StringBuilder();
                Exception e = s.Exception.Flatten();
                while (e != null)
                    if (e.InnerException != null)
                    {
                        sb.AppendLine(e.InnerException.Message);
                        e = e.InnerException;
                    }
                App.Log.Log("Экспорт для программы 'Balans' - ошибка: " + sb.ToString(), Category.Exception, Priority.High);
                Dispatcher.Invoke(new Action(delegate () { MessageBox.Show("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation); }));
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }
    }
}