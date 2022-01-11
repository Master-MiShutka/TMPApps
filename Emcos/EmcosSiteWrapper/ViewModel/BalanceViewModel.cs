/*******************************************************************************
*
* Copyright (C) 2016 Trus Mikhail Petrovich
*
 *******************************************************************************/
namespace TMP.Work.Emcos.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Timers;

    using Model;
    using Model.Balance;
    using TMP.Work.Emcos.View;
    using TMPApplication;
    using TMPApplication.WpfDialogs.Contracts;
    using TMP.Shared;
    using TMP.Shared.Commands;

    public class BalanceViewModel : BaseViewModel
    {
        #region | Constants |

        public readonly string REPORTS_FOLDER = "Reports";

        #endregion | Constants |

        #region Fields

        BalanceSession _session;

        Repository _repository = Repository.Instance;
        Properties.Settings _settings = Properties.Settings.Default;

        private DatePeriod _selectedPeriod;
        private IHierarchicalEmcosPoint _selectedDepartament;
        private ICollectionView _substationsCollectionView;

        private TMP.UI.Controls.WPF.TreeListView.TreeNode _selectedBalanceItemNode;

        #endregion

        #region Constructor

        public BalanceViewModel() : this(null)
        {

        }

        public BalanceViewModel(System.Windows.Window window) : base(window as IWindowWithDialogs)
        {
            if (window == null) return;

            try
            {
                if (System.IO.Directory.Exists(REPORTS_FOLDER) == false)
                    Directory.CreateDirectory(REPORTS_FOLDER);
            }
            catch (System.IO.IOException ex)
            {
                _callBackAction(ex);
            }
            #region Добавление обработчиков окна
            if (window != null)
                window.Closed += (s, e) =>
                {
                    if (Session == null)
                        return;
                    // сохранение сессии
                    if (Repository.Save())
                        EmcosSiteWrapperApp.LogInfo("Сессия сохранена");
                    else
                        EmcosSiteWrapperApp.LogInfo("Сессия не сохранена");
                };
            if (window != null)
                window.Loaded += (s, e) =>
                {
                    if (Status == State.Busy)
                        ShowDialogWaitingScreen("Подготовка данных ....");

                    if (Session == null && Repository.SessionsInfoList.Count == 0)
                    {
                        ShowDialogInfo(string.Format("В папке '{0}' не обнаружены сессии (файлы с расширением '{1}').\nБудет создана новая сессия.",
                                Repository.SESSIONS_FOLDER, Repository.SESSION_FILE_EXTENSION),
                            Title,
                            () => Repository.CreateEmptySession());
                    }
                    else
                    {
                        if (Session == null)
                            SelectSessionCommand.Execute(s);
                        else
                            Status = State.Ready;
                    }
                };
            #endregion

            // инициализация репозитория
            _repository.Loaded += Repository_LoadedOrSaved;
            _repository.Saved += Repository_LoadedOrSaved;
            _repository.PropertyChanged += Repository_PropertyChanged;
            _repository.Init();

            // Инициализация команд
            InitCommands(window);

            _timer.Elapsed += Timer_Elapsed;
        }

        ~BalanceViewModel()
        {
            if (_repository != null)
            {
                _repository.Loaded -= Repository_LoadedOrSaved;
                _repository.Saved -= Repository_LoadedOrSaved;
                _repository.PropertyChanged -= Repository_PropertyChanged;
            }
            if (_session != null)
                _session.PropertyChanged -= Session_PropertyChanged;
        }

        #endregion

        #region Private methods

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan timeSpan = e.SignalTime - _operationStartDateTime;

            if (ProcessedItemsCount == 0)
            {
                RemainingTime = "(идёт подсчёт)";
                return;
            }

            var perItemMs = timeSpan.TotalMilliseconds / ProcessedItemsCount;
            int remainingSeconds = Convert.ToInt32((ProcessingItemsCount - ProcessedItemsCount) * perItemMs / 1000);

            if (remainingSeconds < 10)
                RemainingTime = "почти завершено";
            else
                if (remainingSeconds < 30)
                RemainingTime = "ещё совсем немного";
            else
            if (remainingSeconds < 60)
                RemainingTime = "меньше минуты";
            else
                RemainingTime = String.Format("{0} мин. {1} сек.", remainingSeconds / 60, remainingSeconds % 60);
        }

        private void StartTimer()
        {
            _timer.Start();
            _operationStartDateTime = DateTime.Now;
        }

        private void EndTimer()
        {
            _timer.Stop();
        }

        private void Repository_LoadedOrSaved(object sender, EventArgs e)
        {
            CloseDialog();
        }

        private void Repository_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ActiveSession":

                    // подписка на изменение свойств сессии
                    if (_session != null)
                        _session.PropertyChanged -= null;
                    _session = Repository.ActiveSession;
                    if (_session != null)
                    {
                        _session.PropertyChanged += Session_PropertyChanged;
                    }

                    RaisePropertyChanged("SelectedPeriod");
                    RaisePropertyChanged("Session");

                    RaisePropertyChanged("SelectedDepartament");

                    InitView();

                    SelectedBalanceItemNode = null;

                    RaisePropertyChanged("WindowTitle");
                    break;
                case "ConfigPoints":
                    RaisePropertyChanged("SubstationsTree");
                    break;
                default:
                    break;
            }
        }

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("WindowTitle");
        }

        private void CheckOnError()
        {
            DispatcherExtensions.InUi(() =>
            {
                switch (EmcosSiteWrapper.Instance.Status)
                {
                    case EmcosSiteWrapper.State.Online:
                        break;
                    case EmcosSiteWrapper.State.Offline:
                        ShowDialogError("Произошла ошибка.\n" + EmcosSiteWrapper.Instance.LastException.Message);
                        break;
                    case EmcosSiteWrapper.State.NotAuthorized:
                        break;
                    case EmcosSiteWrapper.State.Error:
                        break;
                    default:
                        break;
                }
            });
        }

        private void GetEmcosArchivesForSubstations()
        {
            EmcosSiteWrapperApp.LogInfo("Получение архивных данных");
            Status = State.Busy;
            DialogMessage = "Получение архивных данных ...";

            EmcosSiteWrapperApp.LogInfo("Сохранение текущей сессии");
            Repository.SaveBackup();

            _cts = new System.Threading.CancellationTokenSource();
            IsCancel = false;
            IsGettingData = true;

            try
            {
                var task = EmcosSiteWrapper.Instance.ExecuteAction(_cts, GetSubstationsDaysArchives);

                task.ContinueWith((s) =>
                {
                    EmcosSiteWrapperApp.LogInfo("Выполнено получение архивных данных");
                    try
                    {
                        DispatcherExtensions.InUi(() => ShowDialogInfo("Выполнено. "));
                        IsGettingData = false;
                        Status = State.Ready;
                        /*RaisePropertyChanged("HasData");
                        RaisePropertyChanged("SubstationsTree");
                        RaisePropertyChanged("WindowTitle");*/
                    }
                    catch (Exception ex)
                    {
                        EmcosSiteWrapperApp.LogError("Получение данных. Ошибка - " + ex.Message);
                    }
                }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);
                task.ContinueWith((s) =>
                {
                    EmcosSiteWrapperApp.LogInfo("Прервано получение архивных данных");
                    ShowDialogWarning("Прервано. ");
                    IsGettingData = false;
                    Status = State.Ready;
                    /*RaisePropertyChanged("HasData");
                    RaisePropertyChanged("SubstationsTree");
                    RaisePropertyChanged("WindowTitle");*/
                }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                EmcosSiteWrapperApp.LogError("Получение архивных данных. Ошибка - " + ex.Message);
                ShowDialogError("Произошла ошибка.\n" + ex.Message, Title);
                IsGettingData = false;
            }
        }

        private void GetSubstationsDaysArchives()
        {
            string dialogMessage = "Получение суточных данных по подстанции {0}";
            IProgressDialog progressDialog = ShowDialogProgress("Получение суточных данных по подстанциям") as IProgressDialog;
            DispatcherExtensions.InUi(() =>
            {
                (Window as System.Windows.Window).TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                (Window as System.Windows.Window).TaskbarItemInfo.ProgressValue = 0.01;
            });
            Action<int, int> updateCallBack = (current, total) =>
            {
                DispatcherExtensions.InUi(() =>
                {
                    progressDialog.Progress = 100 * current / total;
                    (Window as System.Windows.Window).TaskbarItemInfo.ProgressValue = ((double)current) / total;
                });
            };

            ClearCurrentSubstations();

            foreach (Model.Balance.Substation substation in Substations)
                substation.Status = Model.DataStatus.Wait;
            for (int index = 0; index < Substations.Count; index++)
            {
                var substation = Substations[index];

                progressDialog.Message = String.Format(dialogMessage, substation.Name);

                if (_cts.IsCancellationRequested)
                    break;
                Emcos.Utils.GetArchiveDataForSubstation(Session.Info.Period.StartDate, Session.Info.Period.EndDate, substation, _cts, updateCallBack);
            }
            Status = State.Ready;
        }

        private void ShowSubstationDetails()
        {
            if (SelectedBalanceItem is SubstationSection || SelectedBalanceItem is Substation)
            {
                var item = SelectedBalanceItem as IBalanceGroupItem;
                var balance = new Balance<ActiveEnergy>(item);
                balance.Initialize(Session.Info.Period);

                var bsv = new BalanceSubstationView(balance)
                {
                    Owner = Window as System.Windows.Window,
                    ShowInTaskbar = false
                };
                Status = State.Busy;
                bsv.ShowDialog();
                Status = State.Ready;
            }
            else
                return;
        }

        private void ShowItemDetails()
        {
            var biv = new BalanceItemView(Session.Info.Period)
            {
                Owner = Window as System.Windows.Window,
                ShowInTaskbar = false,
                DataContext = SelectedBalanceItem
            };
            Status = State.Busy;
            biv.ShowDialog();
            Status = State.Ready;
        }

        private void ShowDetails(Model.ElementTypes elementType)
        {
            switch (elementType)
            {
                case Model.ElementTypes.GROUP:
                    break;
                case Model.ElementTypes.SUBSTATION:
                    ShowSubstationDetails();
                    break;
                case Model.ElementTypes.VOLTAGE:
                    break;
                case Model.ElementTypes.SECTION:
                    ShowSubstationDetails();
                    break;
                case Model.ElementTypes.POWERTRANSFORMER:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.UNITTRANSFORMER:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.UNITTRANSFORMERBUS:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.FIDER:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.POWERTRANSFORMERS:
                    ShowItemDetails();
                    break;
                case Model.ElementTypes.AUXILIARY:
                    ShowItemDetails();
                    break;
                default:
                    break;
            }
        }

        private void InitCommands(System.Windows.Window window)
        {
            // Редактор конфигурации
            PointsEditorCommand = new DelegateCommand(() =>
            {
                DialogMessage = "Редактор конфигурации";
                var pe = new Controls.PointsEditor(Repository.ConfigPoints, Repository.ConfigOtherPoints, this.Window);
                var dialog = this.CreateDialogCustom(pe, TMPApplication.WpfDialogs.DialogMode.Ok);
                dialog.Caption = DialogMessage;
                dialog.CloseBehavior = TMPApplication.WpfDialogs.DialogCloseBehavior.ExplicitClose;

                dialog.OkText = "Закрыть";
                dialog.Ok = () =>
                {
                    dialog.Close();
                    this.ShowDialogWaitingScreen("Применение конфигурации");
                    // сохранить конфигурацию
                    if (Repository.SaveConfiguration() == false)
                        EmcosSiteWrapperApp.LogWarning("Конфигурация не сохранена");
                    else
                        EmcosSiteWrapperApp.LogWarning("Конфигурация сохранена");
                    
                    // обновить формулы групп в сессии
                    foreach (IHierarchicalEmcosPoint point in pe.EmcosPoints.FlatItemsList)
                    {
                        if (point is IBalanceGroupItem balanceGroupItem)
                        {
                            Repository.UpdateGroupBalanceFormula(balanceGroupItem.Id, balanceGroupItem.Formula);
                        }
                    }
                    // если появились новые точки - добавить точки в сессию и получить данные
                    if (pe.NewPoints != null && pe.NewPoints.Count > 1)
                    {
                        ;
                    }
                    if (pe.DeletedPoints != null && pe.DeletedPoints.Count > 1)
                    {
                        ;
                    }

                    // пересчёт баланса
                    var balanceGroups = Repository.ActiveSession.BalancePoints.FlatItemsList
                        .Where(p => (p.TypeCode == "SUBSTATION" || p.ElementType == ElementTypes.SUBSTATION) || (p.TypeCode == "SECTIONBUS" || p.ElementType == ElementTypes.SECTION));
                    foreach (IBalanceGroupItem group in balanceGroups)
                    {
                        group.RecalculateBalance();
                    }
                    this.CloseDialog();
                };

                dialog.Show();
            });
            //
            GetDataCommand = new DelegateCommand(GetEmcosArchivesForSubstations);
            // Сохранение данных
            SaveDataCommand = new DelegateCommand(() =>
            {
                var result = U.InputBox("Сохранение", "Укажите название сессии", Session.Info.Period.GetFileNameForSaveSession());
                if (String.IsNullOrWhiteSpace(result) == true)
                    return;
                Repository.SaveAs(result);
                Repository.FillSessionsList();
            },
            (o) => (IsSessionLoadedAdnPeriodSelected));
            // Выбор сессии
            SelectSessionCommand = new DelegateCommand(() =>
            {
                CloseDialog();
                DialogMessage = "Выбор сессии ...";
                Repository.FillSessionsList();

                Controls.SessionManager sm = new Controls.SessionManager(Repository);

                var dialog = this.CreateDialogCustom(sm, TMPApplication.WpfDialogs.DialogMode.YesNoCancel);
                dialog.Caption = DialogMessage;
                dialog.CloseBehavior = TMPApplication.WpfDialogs.DialogCloseBehavior.ExplicitClose;

                void closeDialogAction()
                {
                    dialog.Close();
                }

                dialog.YesText = "Загрузить";
                dialog.Yes = () =>
                {
                    if (sm.SelectedSessionInfo == null)
                    {
                        ShowDialogWarning("Необходимо выбрать сессию из списка!", DialogMessage);
                        return;
                    }
                    else
                    {
                        if (_repository.LoadFromFile(sm.SelectedSessionInfo.FileName) == false)
                            TMPApp.ShowWarning("Не удалось загрузить сессию.");
                        else
                            closeDialogAction();
                    }
                };

                dialog.NoText = "Новая";
                dialog.No = () =>
                {
                    _repository.CreateEmptySession();
                    closeDialogAction();
                };

                dialog.CancelText = "Закрыть";
                dialog.Cancel = () =>
                {
                    closeDialogAction();
                };

                dialog.Show();
            });

            CancelCommand = new DelegateCommand(() =>
            {
                IsCancel = true;
                _cts.Cancel();
            },
            (o) => !IsCancel);
            // Детальная информация об элементе
            ViewDetailsCommand = new DelegateCommand(() =>
            {
                ShowDetails(SelectedBalanceItem.ElementType);
            },
            (o) => SelectedBalanceItem != null);

            ExportList = new List<ICommand>
            {
                new DelegateCommand(() => ExportSubstationsBalance(), (o) => (IsSessionLoadedAdnPeriodSelected), "Баланс подстанций"),
                new DelegateCommand(() => BalanceExport(), (o) => (IsSessionLoadedAdnPeriodSelected), "Для программы 'Balance'"),
                new DelegateCommand(() => ExportFiderAnaliz(), (o) => (IsSessionLoadedAdnPeriodSelected), "Для пофидерного анализа")
            };

            OpenAuxiliaryReportCommand = new DelegateCommand(() =>
            {
                IList<AuxiliaryReportItem> model = new List<Model.AuxiliaryReportItem>();

                var list = Substations
                    .GroupBy(i => i.Departament, i => i,
                    (k, g) => new { Departament = k, Substations = g.ToList() })
                    .OrderBy(i => i.Departament)
                    .ToList();

                foreach (var item in list)
                {
                    var departament = item.Departament;
                    var dep = new AuxiliaryReportItem
                    {
                        Name = departament,
                        Type = "Departament",

                        Children = new List<AuxiliaryReportItem>()
                    };

                    foreach (Model.Balance.Substation substation in item.Substations)
                    {
                        var sub = new AuxiliaryReportItem
                        {
                            Children = new List<AuxiliaryReportItem>(),
                            Name = substation.Name,
                            Type = "Substation"
                        };

                        foreach (Model.Balance.IBalanceItem BalanceItem in substation.Items)
                        {
                            if (BalanceItem.ElementType == ElementTypes.UNITTRANSFORMER || BalanceItem.ElementType == ElementTypes.UNITTRANSFORMERBUS)
                                sub.Children.Add(new AuxiliaryReportItem
                                {
                                    Name = BalanceItem.Name,
                                    APlus = BalanceItem.ActiveEnergy.Plus.Value,
                                    AMinus = BalanceItem.ActiveEnergy.Minus.Value,
                                    RPlus = BalanceItem.ReactiveEnergy.Plus.Value,
                                    RMinus = BalanceItem.ReactiveEnergy.Minus.Value,
                                });
                        }
                        dep.Children.Add(sub);
                    }
                    model.Add(dep);
                }
                var arw = new AuxiliaryReportWindow(new AuxiliaryReportTreeModel(model))
                {
                    Owner = Window as System.Windows.Window,
                    ShowInTaskbar = false
                };

                arw.ShowDialog();
            },
            (o) => IsSessionLoadedAdnPeriodSelected);

            bool canExecuteUpdateSubstationDataCommand(object o)
            {
                if (SelectedBalanceItem == null) return false;
                if (SelectedBalanceItem is Model.Balance.IBalanceGroupItem && (SelectedBalanceItem as Model.Balance.IBalanceGroupItem).ElementType == ElementTypes.SUBSTATION)
                    return true;
                else
                    return false;
            }

            UpdateSubstationDataCommand = new DelegateCommand(() =>
            {
                IProgressDialog progressDialog = ShowDialogProgress("Обновление данных по подстанции ...") as IProgressDialog;
                DispatcherExtensions.InUi(() =>
                {
                    (Window as System.Windows.Window).TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    (Window as System.Windows.Window).TaskbarItemInfo.ProgressValue = 0;
                });
                var substation = SelectedBalanceItem as Model.Balance.Substation;
                if (substation == null) return;
                substation.ClearData();

                Action oncompleted = () =>
                {
                    Status = State.Ready;
                    //vm.IsGettingData = false;
                    /*RaisePropertyChanged("HasData");
                    RaisePropertyChanged("SubstationsTree");
                    RaisePropertyChanged("WindowTitle");*/
                };

                void updateCallBack(int current, int total)
                {
                    DispatcherExtensions.InUi(() =>
                    {
                        progressDialog.Progress = 100 * current / total;
                        (Window as System.Windows.Window).TaskbarItemInfo.ProgressValue = ((double)current) / total;
                    });
                }

                try
                {
                    Status = State.Busy;
                    IsGettingData = true;
                    IsCancel = false;

                    _cts = new System.Threading.CancellationTokenSource();

                    substation.Status = Model.DataStatus.Wait;

                    var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        Emcos.Utils.GetArchiveDataForSubstation(Session.Info.Period.StartDate, Session.Info.Period.EndDate, substation, _cts, updateCallBack), _cts.Token);

                    task.ContinueWith((s) =>
                    {
                        if (s.Result == true)
                            DispatcherExtensions.InUi(() => ShowDialogInfo("Выполнено. "));
                        else
                            CheckOnError();
                        oncompleted();
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);
                    task.ContinueWith((s) =>
                    {
                        DispatcherExtensions.InUi(() => ShowDialogError("Произошла ошибка.\n" + s.Exception));
                        oncompleted();
                    }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }
                catch (Exception e)
                {
                    EmcosSiteWrapperApp.LogError(String.Format("Обновление данных [{0}]. Ошибка - {1}", substation.Code, e.Message));
                    ShowDialogError("Произошла ошибка.\n" + e.Message);
                    oncompleted();
                }
            }, canExecuteUpdateSubstationDataCommand);

            SetSubstationToUseMonthValueCommand = new DelegateCommand((o) =>
            {
                System.Windows.Controls.MenuItem menuItem = o as System.Windows.Controls.MenuItem;
                if (menuItem == null) return;
                var substation = SelectedBalanceItem as Model.Balance.Substation;
                if (substation == null) return;
                else
                    substation.UseMonthValue = menuItem.IsChecked;
            }, canExecuteUpdateSubstationDataCommand);
        }

        private void InitView()
        {
            Substations = (Session == null || Session.Substations == null)
                ? new BindableCollection<Substation>()
                : new BindableCollection<Substation>((SelectedDepartament == null || SelectedDepartament.Children.Count == 0)
                ? Session.Substations
                    .OrderBy(s => s.Departament)
                    .ThenBy(s => s.Name)
                    .ToList()
                : Session.Substations
                    .Where(s => s.Departament == SelectedDepartament.Name)
                    .OrderBy(s => s.Name)
                    .ToList());
            RaisePropertyChanged("SubstationsTree");

            HasData = (Substations == null || Substations.Count == 0)
                ? false
                : Substations.Any((s) => s.Status == DataStatus.Processed);

            _substationsCollectionView = CollectionViewSource.GetDefaultView(Substations);

            _substationsCollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _substationsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("ElementType")); ;
        }

        #region Exports

        private string CreateNameForReport(string reportName)
        {
            return String.Format(@"{0}\{1}-{2}{3}", REPORTS_FOLDER, reportName, Session.Info.Period.GetFriendlyDateRange(), ".xlsx");
        }

        /// <summary>
        /// Баланс подстанций
        /// </summary>
        private void ExportSubstationsBalance()
        {
            if (HasData == false)
            {
                EmcosSiteWrapperApp.ShowWarning("Отсутствуют данные.");
                return;
            }
            Status = State.Busy;
            DialogMessage = "Экспорт данных: Баланс подстанций";

            var dialog = ShowDialogQuestion(
                String.Format("Скопировать файл отчёта в папку '{0}'?", Properties.Settings.Default.ReportBalancePSFolder),
                Title, TMPApplication.WpfDialogs.DialogMode.YesNo);
            var r = dialog.Result;

            var task = new System.Threading.Tasks.Task(() =>
            {
                var defaultFileName = CreateNameForReport("out");
                var sbe = new Export.SubstationsBalanceExport(
                new Export.ExportInfo
                {
                    Title = "Отчёт по балансам подстанций",
                    StartDate = Session.Info.Period.StartDate,
                    EndDate = Session.Info.Period.EndDate,
                    Substations = Substations
                });
                DispatcherExtensions.InUi(() =>
                {
                    if (sbe.Export(defaultFileName) == false)
                        return;
                });

                if (r == TMPApplication.WpfDialogs.DialogResultState.Yes)
                {
                    if (System.IO.Directory.Exists(Properties.Settings.Default.ReportBalancePSFolder) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(Properties.Settings.Default.ReportBalancePSFolder);
                        }
                        catch (System.IO.IOException ioe)
                        {
                            ShowDialogError(String.Format("Папка {0} не найдена. При попытке её создать произошла ошибка: {1}.\nФайл отчёта не скопирован.", Properties.Settings.Default.ReportBalancePSFolder, ioe.Message));
                        }
                    }
                    var filename = System.IO.Path.Combine(
                        Properties.Settings.Default.ReportBalancePSFolder,
                        String.Format(Properties.Settings.Default.ReportBalancePSFileNameTemplate, Session.Info.Period.StartDate) + ".xlsx");
                    System.IO.File.Copy(defaultFileName, filename, true);
                    System.Diagnostics.Process.Start(filename);
                }
                else
                    System.Diagnostics.Process.Start(defaultFileName);
            }, System.Threading.Tasks.TaskCreationOptions.AttachedToParent);

            task.ContinueWith((t) =>
            {
                Status = State.Ready;
            }, System.Threading.Tasks.TaskContinuationOptions.None);

            task.ContinueWith((s) =>
            {
                EmcosSiteWrapperApp.LogError("Экспорт балансов подстанций - ошибка");
                EmcosSiteWrapperApp.LogException(s.Exception);

                ShowDialogError("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику.");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }
        /// <summary>
        /// Список фидеров с расходом для пофидерного анализа
        /// </summary>
        private void ExportFiderAnaliz()
        {
            if (HasData == false)
            {
                ShowDialogWarning("Отсутствуют данные.");
                return;
            }
            Status = State.Busy;
            DialogMessage = "Экспорт данных: Список фидеров с расходом для пофидерного анализа";

            var fileName = CreateNameForReport("FiderAnaliz");
            var task = new System.Threading.Tasks.Task(() =>
            {
                var fa = new Export.FiderAnaliz(
                new Export.ExportInfo
                {
                    Title = "Данные для пофидерного анализа",
                    StartDate = Session.Info.Period.StartDate,
                    EndDate = Session.Info.Period.EndDate,
                    Substations = Substations
                });
                DispatcherExtensions.InUi(() =>
                {
                    if (fa.Export(fileName) == false)
                        return;
                });
                System.Diagnostics.Process.Start(fileName);
            });

            task.ContinueWith((t) =>
            {
                Status = State.Ready;
            }, System.Threading.Tasks.TaskContinuationOptions.None);

            task.ContinueWith((s) =>
            {
                EmcosSiteWrapperApp.LogError("Экспорт для пофидерного анализа - ошибка");
                EmcosSiteWrapperApp.LogException(s.Exception);
                ShowDialogError("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику.");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }
        /// <summary>
        /// Список точек с расходом для программы 'Balans'
        /// </summary>
        private void BalanceExport()
        {
            if (HasData == false)
            {
                EmcosSiteWrapperApp.ShowWarning("Отсутствуют данные.");
                return;
            }
            Status = State.Busy;
            DialogMessage = "Экспорт данных: Список точек с расходом для программы 'Balans'";

            var fileName = CreateNameForReport("ForBalance");
            var task = new System.Threading.Tasks.Task(() =>
            {
                var be = new Export.ForBalanceExport(
                new Export.ExportInfo
                {
                    Title = "Данные для програмы Balance",
                    StartDate = Session.Info.Period.StartDate,
                    EndDate = Session.Info.Period.EndDate,
                    Substations = Substations
                });
                DispatcherExtensions.InUi(() =>
                {
                    if (be.Export(fileName) == false)
                        return;
                });
                System.Diagnostics.Process.Start(fileName);
            });

            task.ContinueWith((t) =>
            {
                Status = State.Ready;
            }, System.Threading.Tasks.TaskContinuationOptions.None);

            task.ContinueWith((s) =>
            {
                EmcosSiteWrapperApp.LogError("Экспорт для программы 'Balance' - ошибка");
                EmcosSiteWrapperApp.LogException(s.Exception);
                ShowDialogError("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику.");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }

        #endregion

        #endregion Private

        #region Public Methods

        public void ClearCurrentSubstations()
        {
            foreach (Substation s in Substations)
            {
                s.ClearData();
            }
            RaisePropertyChanged("Substations");
        }
        #endregion Public Methods

        #region Public Properties

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public Repository Repository => _repository;
        /// <summary>
        /// Выбранный временной период
        /// </summary>
        public DatePeriod SelectedPeriod
        {
            get
            {
                if (_selectedPeriod == null && IsSessionLoadedAdnPeriodSelected)
                    _selectedPeriod = Session.Info.Period;
                return _selectedPeriod;
            }
            set
            {
                if (_selectedPeriod != null && _selectedPeriod.Equals(value))
                    return;
                if (Session.Info.Period == value)
                    return;
                if (EmcosSiteWrapperApp.ShowQuestion("Изменён период. Получить данные?") == System.Windows.MessageBoxResult.Yes)
                {
                    SetProp(ref _selectedPeriod, value, "SessionsList");
                    Session.Info.Period = _selectedPeriod;
                    GetDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Активная сессия
        /// </summary>
        public BalanceSession Session => _session ?? Repository.ActiveSession;

        /// <summary>
        /// Загружена ли сессия и выбран ли временной период
        /// </summary>
        public bool IsSessionLoadedAdnPeriodSelected => Session != null && Session.Info.IsLoaded && Session.Info.Period != null;

        private int _selectedEnergyIndex = 0;
        /// <summary>
        /// Список энергий выбранного элемента
        /// </summary>
        public int SelectedEnergyIndex
        {
            get => _selectedEnergyIndex;
            set
            {
                SetProp(ref _selectedEnergyIndex, value, "SelectedEnergyIndex");

                if (SelectedBalanceItem == null)
                {
                    if (_selectedEnergyIndex == 0)
                        SelectedEnergy = SelectedBalanceItem?.ActiveEnergy;
                    else
                        SelectedEnergy = SelectedBalanceItem?.ReactiveEnergy;
                }
            }
        }

        /// <summary>
        /// Выбранная энергия
        /// </summary>
        [Magic]
        public IEnergy SelectedEnergy { get; set; }

        /// <summary>
        /// Выбранное подразделение
        /// </summary>
        public IHierarchicalEmcosPoint SelectedDepartament
        {
            get
            {
                return _selectedDepartament ?? 
                    (
                        (Repository != null && Repository.ActiveSession != null && Repository.ActiveSession.Departaments != null && Repository.ActiveSession.Departaments.Count > 0) 
                        ? Repository.ActiveSession.Departaments[0] 
                        : null
                    );
            }
            set
            {
                _selectedDepartament = value;
                RaisePropertyChanged("SelectedDepartament");
                InitView();
            }
        }
        /// <summary>
        /// Представление коллекции подстанций
        /// </summary>
        [Magic]
        public ICollectionView SubstationsCollectionView { get; private set; }
        /// <summary>
        /// Список подстанций выбранного подразделения
        /// </summary>
        [Magic]
        public IList<Substation> Substations { get; private set; }

        /// <summary>
        /// Модель дерева балансов
        /// </summary>
        [Magic]
        public ITreeModel SubstationsTree
        {
            get
            {
                if (Substations != null)
                    return new BalanceGroupItem(Substations) { Name = "Root" };
                else return (Repository.ConfigPoints == null)
                    ? new EmcosPoint() 
                    : new EmcosPoint(Repository.ConfigPoints); //{ get; private set; }
            }
        }

        /// <summary>
        /// Выбранный в дереве узел
        /// </summary>
        public UI.Controls.WPF.TreeListView.TreeNode SelectedBalanceItemNode
        {
            get { return _selectedBalanceItemNode; }
            set
            {
                _selectedBalanceItemNode = value;
                if (value == null)
                    SelectedBalanceItem = null;
                else
                    SelectedBalanceItem = value.Tag as IBalanceItem;
                RaisePropertyChanged("SelectedBalanceItemNode");
            }
        }
        /// <summary>
        /// Выбранный в дереве элемент баланса
        /// </summary>
        [Magic]
        public IBalanceItem SelectedBalanceItem { get; private set; }

        /// <summary>
        /// Редактор конфигурации
        /// </summary>
        public ICommand PointsEditorCommand { get; private set; }
        /// <summary>
        /// Вобор сессии из списка или создание новой
        /// </summary>
        public ICommand SelectSessionCommand { get; private set; }
        /// <summary>
        /// Команда на получение данных из сервиса
        /// </summary>
        public ICommand GetDataCommand { get; private set; }
        /// <summary>
        /// Команда сохранения сессии
        /// </summary>
        public ICommand SaveDataCommand { get; private set; }
        /// <summary>
        /// Команда отмены текущей операции
        /// </summary>
        public ICommand CancelCommand { get; private set; }
        /// <summary>
        /// Просмотр детальной информации об элементе
        /// </summary>
        public ICommand ViewDetailsCommand { get; private set; }
        /// <summary>
        /// Просмотр информации о расходе на собственные нужды подстанций
        /// </summary>
        public ICommand OpenAuxiliaryReportCommand { get; private set; }
        /// <summary>
        /// Обновление значений энергий по подстанции
        /// </summary>
        public ICommand UpdateSubstationDataCommand { get; private set; }
        /// <summary>
        /// Указание использовать месячные данные по подстанции
        /// </summary>
        public ICommand SetSubstationToUseMonthValueCommand { get; private set; }
        /// <summary>
        /// Признак, указавающий, что происходит получение данных из сервиса
        /// </summary>
        [Magic]
        public bool IsGettingData { get; private set; }
        /// <summary>
        /// Признак отмены
        /// </summary>
        [Magic]
        public bool IsCancel { get; private set; }
        /// <summary>
        /// Признак наличия данных
        /// </summary>
        [Magic]
        public bool HasData { get; private set; }
        /// <summary>
        /// Список команд для экпорта информации
        /// </summary>
        [Magic]
        public IList<ICommand> ExportList { get; set; }

        [Magic]
        public string RemainingTime { get; private set; }
        [Magic]
        public int ProcessedItemsCount { get; private set; }
        [Magic]
        public int ProcessingItemsCount { get; private set; }

        /// <summary>
        /// Заголовок для сообщений
        /// </summary>
        public string Title { get { return Emcos.Strings.MainTitle; } }
        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string WindowTitle
        {
            get
            {
                if (Session == null)
                {
                    PointsTreeMessage = String.Format("{0}\n{1}", Emcos.Strings.NoDataForDisplay, Emcos.Strings.SessionNotLoadedSelectOrCreateNew);
                    return String.Format("{0} :: {1}", Title, Emcos.Strings.SessionNotLoadedSelectOrCreateNew);
                }
                else
                {
                    if (Session.Info.Period == null)
                    {
                        PointsTreeMessage = String.Format("{0}\n{1}\n{2}", Emcos.Strings.NoDataForDisplay, Emcos.Strings.EmptySession, Emcos.Strings.SelectPeriodAndGetData);
                        return String.Format("{0} :: {1}", Title, Emcos.Strings.EmptySession);
                    }
                    else
                    {
                        PointsTreeMessage = null;
                        return String.Format("{0} :: {1} :: файл сессии '{2}'",
                            Title,
                            Session.Info.Title,
                            (String.IsNullOrEmpty(Session.Info.FileName) ? "<не загружен>" : Session.Info.FileName));
                    }
                }
            }
        }
        /// <summary>
        /// Сообщение, выводимое в дереве точек, при их отсутствии
        /// </summary>
        [Magic]
        public string PointsTreeMessage { get; private set; }

        /// <summary>
        /// Версия программы
        /// </summary>
        public string AppVersion
        {
            get
            {
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            }
        }
        /// <summary>
        /// Описание программы
        /// </summary>
        public string AppDescription
        {
            get { return "нет описания"; }
        }
        /// <summary>
        /// Права на ПО
        /// </summary>
        public string AppCopyright
        {
            get { return "© 2016-2018, Ведущий инженер отдела сбыта\r\nэлектроэнергии Ошмянских ЭС\r\nТрус Михаил Петрович\r\nЦените и уважайте чужой труд!"; }
        }

        #endregion | Public Properties |
    }
}
