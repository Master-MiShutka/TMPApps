using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TMP.Wpf.CommonControls.Dialogs;

namespace TMP.ARMTES
{
    using Model;
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ArmTESSiteWrapper armtes;
        private System.Threading.CancellationToken cancelToken;
        private System.Threading.CancellationTokenSource cancelTokenSource;

        private System.Diagnostics.TraceSource logger = new System.Diagnostics.TraceSource("LowLevelDesign");
        private const string ERROR_MESSAGE_TO_LOG = "ОШИБКА! Описание: {0}\r\nПодробнее: {1}\r\n\r\n";

        public MainWindow()
        {
            InitializeComponent();

            SetAppStatus("Ожидание команды");

            // create accent color menu items for the demo
            MainViewModel.Instance.AccentColors = TMP.Wpf.CommonControls.ThemeManager.Accents
                                            .Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                            .ToList();

            // create metro theme color menu items for the demo
            MainViewModel.Instance.AppThemes = TMP.Wpf.CommonControls.ThemeManager.AppThemes
                                           .Select(a => new AppThemeMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                                           .ToList();

            MainViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //this.Dispatcher.Invoke(new Action(() => log.Items.Add(e.PropertyName)));
        }

        #region | Команды |

        private void ExecuteExpandingCommand(object obj)
        {
            if (null == obj) return;
            RoutedEventArgs args = obj as System.Windows.RoutedEventArgs;
            ArmtesElement element = null;
            TreeViewItem tvi = null;
            if (null == args)
            {
                if (obj is TreeViewItem)
                {
                    tvi = obj as TreeViewItem;
                    if (tvi.Header is ArmtesElement)
                        element = tvi.Header as ArmtesElement;
                }
                else
                    element = obj as ArmtesElement;
                if (null == element) return;
                else
                {
                    if (null == element.Items) return;
                    SetAppStatus("Загрузка данных подразделения...");
                    GetTreeElement(element);
                    SetAppStatus();
                }
            }
            else
            {
                if (null == args.Source) return;
                TreeViewItem treeViewItem = args.Source as System.Windows.Controls.TreeViewItem;
                if (null == treeViewItem) return;
                if (null == treeViewItem.Header) return;
                element = treeViewItem.Header as ArmtesElement;
                if (null == element) return;
                if (null == element.Items) return;

                if (element.Items.Count > 1) return;

                args.Handled = true;

                SetAppStatus("Загрузка данных подразделения...");
                GetTreeElement(element);
                SetAppStatus();
            }
        }


        private async Task<bool> SelectSector()
        {
            if (!armtes.ChooseSector(MainViewModel.Instance.SectorType, MainViewModel.Instance.StartDate, MainViewModel.Instance.EndDate, MainViewModel.Instance.ProfileType))
            {
                if (armtes.LastStatusCode == HttpStatusCode.RequestTimeout)
                {
                    armtes.LoginAndGetCookie();
                    return false;
                }
                string errorDescription = this.GetNetErrorDescription(armtes.LastException);
                await this.ShowErrorMessageAsync(errorDescription);
                return false;
            }
            else
                return true;
        }

        private async Task<bool> LoadTree()
        {
            List<ArmtesElement> items = null;
            await Task.Factory.StartNew(() =>
            {
                byte attempts = 1;
                items = null;
                while (items == null && attempts <= 3)
                {
                    if (armtes.LastException != null)
                        if ((armtes.LastException as WebException) == null)
                            break;

                    if (progressController != null)
                        if (progressController.IsCanceled)
                            break;

                    if (progressController != null)
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (progressController.IsOpen)
                                progressController.SetMessage(String.Format("{0}\r\n{1}-я попытка ...", "Загрузка списка объектов", attempts));
                        }));

                    items = armtes.GetElements("e0");
                    attempts++;
                }
            });
            if (items == null || items.Count == 0)
            {
                string errorDescription = this.GetNetErrorDescription(armtes.LastException);
                await this.ShowErrorMessageAsync(String.Format("Список объектов не получен!{0}Описание ошибки: {1}", Environment.NewLine, errorDescription));
                return false;
            }

            ArmtesElement root = items.FirstOrDefault<ArmtesElement>();

            ExecuteExpandingCommand(root);

            //TODO: А надо ли?
            items = new List<ArmtesElement>();
            items.Add(root);

            this.tree.ItemsSource = items;
            return true;
        }

        #endregion | Команды |

        #region | Задачи |

        private async Task<bool> SelectSectorAndInitTree()
        {
            if (MainViewModel.Instance.Authorized)
            {
                await ShowProgress("Пожалуйста, подождите...", "Выбор сектора...", true);

                await SelectSector();

                await ShowProgress("Пожалуйста, подождите...", "Загрузка списка объектов", true);

                await LoadTree();

                await CloseProgress();

                SetAppStatus();
                return true;
            }
            else
            {
                await this.ShowErrorMessageAsync("Почему-то не авторизованы. Ошибка. Программа будет закрыта.");
                MainViewModel.Instance.IsShutdown = true;
                this.Close();
                return false;
            }
        }

        private async Task<bool> ShowLoginAndConnect()
        {
            armtes = null;
            SetAppStatus("Авторизация пользователя");
            LoginDialogData loginResult = await this.ShowLoginAsync("Требуется авторизация", "Введите Ваши учётные данные", new LoginDialogSettings { ColorScheme = TMPDialogColorScheme.Accented });
            if (loginResult == null)
            {
                await this.ShowErrorMessageAsync("Для работы с программой необходима авторизация!");
                return false;
            }
            else
            {
                if (String.IsNullOrEmpty(loginResult.Username))
                {
                    await this.ShowErrorMessageAsync("Укажите имя пользователя!");
                    return false;
                }

                // пользователь ввел данные, пробуем аутентифицироваться
                await ShowProgress("Пожалуйста, подождите...", "Аутентификация пользователя...");

                bool connected = false;
                string errorDescription = string.Empty;
                await Task.Factory.StartNew(() =>
                {
                    string url = String.Format("http://{0}", Properties.Settings.Default.SiteUrl);
                    armtes = new ArmTESSiteWrapper(loginResult.Username, loginResult.Password, url);
                    // проверяем код операции
                    if (armtes.LastStatusCode == HttpStatusCode.Found)
                    {
                        if (armtes.IsAuthorized)
                            connected = true;
                    }
                    else
                    {
                        errorDescription = this.GetNetErrorDescription(armtes.LastException);
                        armtes = null;
                        connected = false;
                    }
                });
                await CloseProgress();
                // проверяем
                if (!connected)
                {
                    // не авторизованы. Выясняем почему
                    if (string.IsNullOrEmpty(errorDescription) == false)
                    {
                        // значит возникло исключение
                        await this.ShowErrorMessageAsync(
                                                    "Возникла ошибка!" + Environment.NewLine +
                                                    "Описание: " + errorDescription + Environment.NewLine + Environment.NewLine +
                                                    "Дальнейшая работа программы невозможна. Программа будет закрыта." + Environment.NewLine +
                                                    "Устраните проблему и повторите позже.");
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        // аторизация не удалась
                        await this.ShowErrorMessageAsync("Вход не удался. Проверьте имя пользователя и пароль.");
                    }
                    return false;
                }
                else
                {
                    // авторизованы
                    MainViewModel.Instance.Authorized = true;
                    SetAppStatus();
                    return true;
                }
            }
        }
        #endregion | Задачи |

        #region | Private Methods |

        private void ClearAllData()
        {
            tree.ItemsSource = null;
            MainViewModel.Instance.Collectors = null;
            MainViewModel.Instance.SelectedElement = null;
            MainViewModel.Instance.IsFullDataLoaded = false;
            SetAppStatus();
        }

        private string ClearString(string data)
        {
            if (String.IsNullOrEmpty(data)) return null;

            data = System.Text.RegularExpressions.Regex.Replace(data, "[\r\n\r\n]", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            data = System.Text.RegularExpressions.Regex.Replace(data, "[\r\n]*<", "<", System.Text.RegularExpressions.RegexOptions.Multiline);
            data = System.Text.RegularExpressions.Regex.Replace(data, ">[ ]*<", "><", System.Text.RegularExpressions.RegexOptions.Multiline);
            data = System.Text.RegularExpressions.Regex.Replace(data, ">[ ]*", ">", System.Text.RegularExpressions.RegexOptions.Multiline);
            data = System.Text.RegularExpressions.Regex.Replace(data, "[ ]*<", "<", System.Text.RegularExpressions.RegexOptions.Multiline);
            data = System.Text.RegularExpressions.Regex.Replace(data, ">[ \t]*<", "><", System.Text.RegularExpressions.RegexOptions.Multiline);

            return data = data.Trim().
                    Replace("&quot;", "\"").
                    Replace("&#171;", "\"").
                    Replace("&#187;", "\"").
                    Replace("&#39;", "\'");
        }

        private string GetNetErrorDescription(Exception exception)
        {
            if (exception == null) return null;

            WebException we = exception as WebException;
            if (we != null)
                switch (we.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        return "Ошибка протокола.";

                    case WebExceptionStatus.RequestCanceled:
                        return "Запрос отменён.";

                    case WebExceptionStatus.Timeout:
                        return "Таймаут операции.";

                    case WebExceptionStatus.UnknownError:
                        return "Неизвестная ошибка.\nПодробно: " + exception.Message;

                    default:
                        return "Проблема с сетью.\nПодробно: " + exception.Message;
                }
            else return exception.Message;
        }
        private void GetTreeElement(ArmtesElement parent)
        {
            string loading = "Loading...";

            if (parent.Items.Count == 1
                && parent.Items[0].Label == loading)
            {
                List<ArmtesElement> items = null;

                this.Dispatcher.Invoke(new Action(() => tree.Cursor = Cursors.Wait));

                items = armtes.GetElements(parent.Value);

                if (items == null)
                {
                    string errorDescription = this.GetNetErrorDescription(armtes.LastException);
                    this.ShowErrorMessage(errorDescription);
                }
                else
                {
                    parent.Items.Clear();
                    parent.Items.AddRange(items);
                    // установка родителя
                    foreach (var item in items)
                        item.Parent = parent;
                }
            }
            else
            {
                //System.Diagnostics.Debugger.Break();
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                tree.Cursor = Cursors.Arrow;
            }));
        }
        private async Task<bool> LoadTreeItemData()
        {
            if (MainViewModel.Instance.SelectedElement == null) return false;

            // отменяем выполняюшиеся задачи
            if (cancelTokenSource != null)
                cancelTokenSource.Cancel();

            await ShowProgress("Пожалуйста, подождите...", "Получение данных...");

            string data = string.Empty;
            try
            {
                bool result = await Task.Run<bool>(() =>
                {
                    /*PageResult<AllTariffsExportIndicationViewItem> pageResult = armtes.GetSmallEngineExportIndications(MainViewModel.Instance.StartDate);
                    if (pageResult != null)
                    {
                        List<AllTariffsExportIndicationViewItem> items = pageResult.Items;                        
                        if (items != null)
                        {
                            var accounts = armtes.GetPersonalAccounts();
                            if (accounts != null)
                            {
                                var list = accounts.Items;
                                var accountsReses = list.GroupBy(i => i.ResName);

                                var entp = armtes.GetEnterprises();
                                var enterprises = entp.Items;

                                var flat = enterprises.SelectMany(i => i.ChildEnterprises);

                                var fes = flat.Where(i => i.EnterpriseName == "ОЭС");
                                var reses = fes.SelectMany(i => i.ChildEnterprises);

                                foreach (var res in reses)
                                {
                                    var resAccounts = accountsReses.Where(a => a.Key == res.EnterpriseName).Select(i => i.ToList()).ToList();
                                    if (resAccounts != null && resAccounts.Count > 0)
                                    foreach (var item in resAccounts[0])
                                    {
                                        var viewitem = items.Where(i => i.PersonalAccount == item.PersonalAccount).ToList();
                                        if (viewitem != null)
                                            {
                                                ;
                                            }
                                        else
                                            {
                                                ;
                                            }
                                    }
                                }

                            }


                            var c = pageResult.Count;
                        }
                    }*/

                    data = armtes.SelectElement(
                        MainViewModel.Instance.SelectedElement.Value,
                        MainViewModel.Instance.StartDate,
                        MainViewModel.Instance.EndDate,
                        MainViewModel.Instance.ProfileType,
                        MainViewModel.Instance.SectorType);

                    if (String.IsNullOrEmpty(data))
                        return false;

                    // убираем лишнее
                    data = ClearString(data);

                    SelectElementModel elementModel = null;
                    elementModel = HtmlParser.ViewElement(data, MainViewModel.Instance.SelectedElement.Value, progressController);

                    MainViewModel.Instance.Collectors = new System.Collections.ObjectModel.ObservableCollection<Collector>(elementModel.Collectors);
                    MainViewModel.Instance.Statistics = elementModel.Statistics;

                    return true;
                });

                if (result == false)
                {
                    string errorDescription = this.GetNetErrorDescription(armtes.LastException);
                    await this.ShowErrorMessageAsync(errorDescription);
                    return false;
                }

                MainViewModel.Instance.IsFullDataLoaded = false;

                await CloseProgress();

                cancelTokenSource = new System.Threading.CancellationTokenSource();
                cancelToken = cancelTokenSource.Token;

                try
                {
                    // теперь обновляем информацию по всем системам
                    MainViewModel.Instance.UpdatingProcessStarted = true;
                    await Task.Factory.StartNew(() =>
                    {

                        int collectorsCount = MainViewModel.Instance.Collectors.Count;
                        string info = "Обновляем информацию по всем системам ... {0}%";

                        int processed = 0;
                        object sync = new object();
                        ParallelOptions po = new ParallelOptions();
                        po.MaxDegreeOfParallelism = 4;
                        Parallel.For(0, collectorsCount, po, (i, state) =>
                        {
                            if (state.ShouldExitCurrentIteration)
                            {
                                if (state.LowestBreakIteration < i)
                                    return;
                            }

                            if (cancelToken.IsCancellationRequested)
                            {
                                this.DebugPrint("Отмена на {0}-й системе.)", i);
                                state.Stop();
                                return;
                            }

                            Collector collector = MainViewModel.Instance.Collectors[i];
                            data = String.Empty;
                            foreach (var obj in collector.Objects)
                            {
                                if (cancelToken.IsCancellationRequested)
                                {
                                    this.DebugPrint("Отмена на {0}-й системе.)", i);
                                    state.Stop();
                                    return;
                                }

                                data = null;
                                int attempts = 1;
                                while (data == null && attempts <= 3)
                                {
                                    data = armtes.ViewObject(
                                    obj.Id,
                                    MainViewModel.Instance.SelectedElement.Value,
                                    MainViewModel.Instance.StartDate,
                                    MainViewModel.Instance.EndDate,
                                    MainViewModel.Instance.ProfileType,
                                    MainViewModel.Instance.SectorType);
                                    attempts++;
                                }
                                if (data != null)
                                {
                                    // убираем лишнее
                                    data = ClearString(data);

                                    ViewDeviceModel viewDeviceModel = HtmlParser.ViewDevice(data, null);

                                    if (viewDeviceModel != null)
                                        obj.ViewModel = viewDeviceModel;

                                    foreach (var counter in obj.Counters)
                                    {
                                        if (cancelToken.IsCancellationRequested)
                                        {
                                            this.DebugPrint("Отмена на {0}-й системе.)", i);
                                            state.Stop();
                                            return;
                                        }

                                        data = null;
                                        attempts = 1;
                                        while (data == null && attempts <= 3)
                                        {
                                            data = armtes.ViewMeter(
                                                counter.Id,
                                                MainViewModel.Instance.SelectedElement.Value,
                                                counter.ParentId,
                                                MainViewModel.Instance.StartDate,
                                                MainViewModel.Instance.EndDate,
                                                MainViewModel.Instance.ProfileType,
                                                MainViewModel.Instance.SectorType);
                                            attempts++;
                                        }
                                        if (data == null)
                                            continue;

                                        // убираем лишнее
                                        data = ClearString(data);

                                        ViewCounterModel vm = HtmlParser.ViewCounter(data, progressController);

                                        counter.ViewModel = vm;

                                        counter.PreviousIndications = vm.IndicationViewItem.PreviousIndications;
                                        counter.NextIndications = vm.IndicationViewItem.NextIndications;
                                    }
                                }
                            }
                            lock (sync)
                            {
                                processed++;
                                Dispatcher.BeginInvoke(new Action(() => SetAppStatus(String.Format(info, 100 * processed / collectorsCount))), System.Windows.Threading.DispatcherPriority.Send);
                            }
                        });

                    }, cancelTokenSource.Token)
                    .ContinueWith((_) =>
                    {
                        DebugPrint("Обновление информации по системам завершено.");

                        MainViewModel.Instance.IsFullDataLoaded = true;
                        MainViewModel.Instance.UpdatingProcessStarted = false;
                        SetAppStatus();

                        cancelTokenSource.Dispose();
                        cancelTokenSource = null;

                        MainViewModel.Instance.FilteredCollectors.Refresh();

                        return true;
                    });
                }
                catch (OperationCanceledException)
                {
                    cancelTokenSource.Cancel();
                    MainViewModel.Instance.ShowMessage("Операция отменена.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (AggregateException ae)
                {
                    cancelTokenSource.Cancel();
                    foreach (Exception exception in ae.InnerExceptions)
                    {
                        if (exception is TaskCanceledException)
                            this.ShowErrorMessage(String.Format("Возникла ошибка!\nException: {0}", ((TaskCanceledException)exception).Message));
                        else
                            this.ShowErrorMessage("Возникла ошибка!\nException: " + exception.GetType().Name);
                    }
                    cancelTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                cancelTokenSource.Cancel();
                cancelTokenSource = null;
                this.ShowErrorMessage(ex.Message);
            }
            return true;
        }

        #endregion | Private Methods |

        #region | Обработка событий |

        private async void CheckARMTESHostAvailablity()
        {
            // проверяем доступен ли сервер
            await ShowProgress("Пожалуйста, подождите...", "Проверка доступности сервера АРМТЕС...");

            byte attempts = 1;
            while (MainViewModel.Instance.HostAvailable == false && attempts <= 3)
            {
                if (progressController != null)
                    if (progressController.IsOpen)
                        progressController.SetMessage(String.Format("{0}-я попытка ...", attempts));

                CheckHostAvailablity();
                await Task.Delay(100);
                attempts++;
            }
            await CloseProgress();
            if (MainViewModel.Instance.HostAvailable == false)
            {
                MessageDialogResult dr = await this.ShowMessageAsync(
                    "Проблема с сетью", "Сервер АРМТЕС не доступен! Попробовать снова или завершить программу?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new TMPDialogSettings() { AffirmativeButtonText = "Повторить", NegativeButtonText = "Завершить" });
                switch (dr)
                {
                    case MessageDialogResult.Negative:
                        MainViewModel.Instance.IsShutdown = true;
                        this.Close();
                        break;

                    case MessageDialogResult.Affirmative:
                        CheckARMTESHostAvailablity();
                        break;
                }
            }
            else
            {
                bool connected = false;
                while (connected == false)
                {
                    // показываем диалог авторизации
                    connected = await ShowLoginAndConnect();
                }
                if (connected)
                {
                    bool result = false;

                    while (result == false)
                    {
                        result = await SelectSectorAndInitTree();
                        if (progressController != null && progressController.IsCanceled)
                        {
                            CheckHostAvailablity();
                        }
                    }

                    armtes.test("424014");
                }
            }
        }

        private void CheckHostAvailablity()
        {
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            System.Net.NetworkInformation.PingReply reply;
            try
            {
                reply = pingSender.Send(Properties.Settings.Default.SiteUrl, timeout, buffer, options);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    MainViewModel.Instance.HostAvailable = true;
                }
                else
                {
                    MainViewModel.Instance.HostAvailable = false;
                }
            }
            catch (System.Net.NetworkInformation.PingException)
            {
                MainViewModel.Instance.HostAvailable = false;
            }
        }

        private async void DateSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Control c = sender as Control;
            string tag = c.Tag == null ? String.Empty : c.Tag.ToString();

            if (String.Equals(tag, "start"))
                MainViewModel.Instance.StartDate = this.startDate.SelectedDate.GetValueOrDefault();
            if (String.Equals(tag, "end"))
                MainViewModel.Instance.EndDate = this.endDate.SelectedDate.GetValueOrDefault();
            await LoadTreeItemData();
        }

        private void ExportSmallEngineFullReport_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance.FilteredCollectors == null)
            {
                this.ShowErrorMessage("Чтот-то пошло не так... Источник данных пуст!");
                return;
            }

            BaseSmallEngineExport ecl;
            ecl = new ExportCollectorListAsIs(
                new ExportInfo()
                {
                    ElementName = MainViewModel.Instance.SelectedDepartament,
                    Param = MainViewModel.Instance.ProfileType,
                    StartDate = MainViewModel.Instance.StartDate,
                    StartDateFormat = "dd.MM.yyyy",
                    EndDate = MainViewModel.Instance.EndDate,
                    EndDateFormat = "dd.MM.yyyy"
                },
                    MainViewModel.Instance.FilteredCollectors.Cast<Collector>());
            ecl.Export("report.xml");

            System.Diagnostics.Process.Start("report.xml");
        }

        private void ExportSmallEngineSimpleReport_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance.FilteredCollectors == null)
            {
                this.ShowErrorMessage("Чтот-то пошло не так... Источник данных пуст!");
                return;
            }

            BaseSmallEngineExport ecl;
            ecl = new ExportCollectorListSimple(
                new ExportInfo()
                {
                    ElementName = MainViewModel.Instance.SelectedDepartament,
                    Param = MainViewModel.Instance.ProfileType,
                    StartDate = MainViewModel.Instance.StartDate,
                    StartDateFormat = "dd.MM.yyyy",
                    EndDate = MainViewModel.Instance.EndDate,
                    EndDateFormat = "dd.MM.yyyy"
                },
                    MainViewModel.Instance.FilteredCollectors.Cast<Collector>().OrderBy((item) => item.Objects.FirstOrDefault().Contract).ToList<Collector>());
            ecl.Export("report.xml");

            System.Diagnostics.Process.Start("report.xml");
        }

        private void ExportNotAnswered_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance.Collectors == null)
            {
                this.ShowErrorMessage("Чтот-то пошло не так... Источник данных пуст!");
                return;
            }

            BaseSmallEngineExport ecl;
            ecl = new ExportCollectorListSimple(
                new ExportInfo()
                {
                    ElementName = MainViewModel.Instance.SelectedDepartament,
                    Param = MainViewModel.Instance.ProfileType,
                    StartDate = MainViewModel.Instance.StartDate,
                    StartDateFormat = "dd.MM.yyyy",
                    EndDate = MainViewModel.Instance.EndDate,
                    EndDateFormat = "dd.MM.yyyy"
                },
                    MainViewModel.Instance.Collectors.Where((c) => (c.IsAnswered == false | (c.IsUSPD & c.IsAnsweredUSPD == false))));
            ecl.Export("report.xml");

            System.Diagnostics.Process.Start("report.xml");
        }

        private void ExportObjectsList_MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance.Collectors == null)
            {
                this.ShowErrorMessage("Чтот-то пошло не так... Источник данных пуст!");
                return;
            }

            BaseSmallEngineExport ecl;
            ecl = new ExportObjectsList(new ExportInfo() { ElementName = MainViewModel.Instance.SelectedDepartament },
                MainViewModel.Instance.Collectors.OrderBy((item) => item.Objects.FirstOrDefault().Contract).ToList<Collector>());
            ecl.Export("report.xml");

            System.Diagnostics.Process.Start("report.xml");
        }

        private void Logout_MenuItemClick(object sender, RoutedEventArgs e)
        {
            ClearAllData();
            MainViewModel.Instance.Authorized = false;
            armtes = null;

            CheckARMTESHostAvailablity();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private async void RefreshData_MenuItemClick(object sender, RoutedEventArgs e)
        {
            await LoadTreeItemData();
        }

        private async void TMPWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !MainViewModel.Instance.IsShutdown && MainViewModel.Instance.QuitConfirmationEnabled;
            if (MainViewModel.Instance.IsShutdown) return;

            var mySettings = new TMPDialogSettings()
            {
                AffirmativeButtonText = "Завершение работы с программой",
                NegativeButtonText = "Отменить",
                AnimateShow = true,
                AnimateHide = false
            };
            var result = await this.ShowMessageAsync("Выход из программы?",
                "Вы действительно хотите закрыть программу?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            MainViewModel.Instance.IsShutdown = result == MessageDialogResult.Affirmative;

            if (MainViewModel.Instance.IsShutdown)
                App.Current.Shutdown(0);
        }

        private async void treeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null) return;
            MainViewModel.Instance.SelectedElement = e.NewValue as ArmtesElement;

            await LoadTreeItemData();
        }
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null)
            {
                ExecuteExpandingCommand(tvi);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {           
            logger.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "Program start.");

            CheckARMTESHostAvailablity();

        }

        private void FilterButtons_Click(object sender, RoutedEventArgs e)
        {
            RadioButton button = e.Source as RadioButton;
            string filter = button.Tag.ToString();
            if (String.IsNullOrEmpty(filter))
                return;

            if (Enum.IsDefined(typeof(MainViewModel.FilterType), filter))
                MainViewModel.Instance.Filter = (MainViewModel.FilterType)Enum.Parse(typeof(MainViewModel.FilterType), filter);
            else
                System.Diagnostics.Debugger.Break();
        }

        private async void LoadTree_Click(object sender, RoutedEventArgs e)
        {
            if (armtes == null) return;

            await ShowProgress("Пожалуйста, подождите...", "Загрузка списка объектов", true);

            await LoadTree();

            await CloseProgress();

            SetAppStatus();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    await ShowProgress("Пожалуйста, подождите...", "Сохранение списка объектов", true);
                    await SaveData();
                    await CloseProgress();
                    e.Handled = true;
                    break;
                case Key.F9:
                    await ShowProgress("Пожалуйста, подождите...", "Загрузка списка объектов", true);
                    await LoadData();
                    await CloseProgress();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void StopUpdatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (cancelToken != null && cancelToken.CanBeCanceled)
                if (cancelTokenSource != null)
                {
                    cancelTokenSource.Cancel();
                    MainViewModel.Instance.UpdatingProcessStarted = false;
                }
        }

        #endregion | Обработка событий |

        private async Task<bool> SaveData()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(System.Collections.ObjectModel.ObservableCollection<Collector>));

                    System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
                    settings.Encoding = new UTF8Encoding(false, false); // no BOM in a .NET string
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = false;

                    using (System.IO.FileStream fs = System.IO.File.Open(MainViewModel.Instance.DataFileName, System.IO.FileMode.Create))
                    using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress, false))
                    using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(gzip, settings))
                    {
                        xmlWriter.WriteComment("Список систем дистанционного съема показаний:");
                        xmlWriter.WriteComment(" * подразделение: " + MainViewModel.Instance.SelectedDepartament);
                        switch (MainViewModel.Instance.ProfileType)
                        {
                            case ProfileType.Current:
                                xmlWriter.WriteComment(" * параметр: текущие показания");
                                break;
                            case ProfileType.Days:
                                xmlWriter.WriteComment(" * параметр: показания на начало суток");
                                break;
                            case ProfileType.Months:
                                xmlWriter.WriteComment(" * параметр: показания на начало месяца");
                                break;
                        }
                        xmlWriter.WriteComment(" * начало периода: " + MainViewModel.Instance.StartDate.ToString("dd.MM.yyyy"));
                        xmlWriter.WriteComment(" * конец периода: " + MainViewModel.Instance.EndDate.ToString("dd.MM.yyyy"));
                        xmlWriter.WriteComment("********************************************");

                        TMP.Shared.GenericSerializer.Serialize(MainViewModel.Instance.Collectors, xmlWriter);
                    }
                }
                catch (Exception ex)
                {
                    logger.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, String.Format(ERROR_MESSAGE_TO_LOG, ex.Message, ex.StackTrace));
                }
            });
            return true;
        }

        private async Task<bool> LoadData()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(System.Collections.ObjectModel.ObservableCollection<Collector>));

                    using (System.IO.FileStream fs = System.IO.File.Open(MainViewModel.Instance.DataFileName, System.IO.FileMode.Open))
                    using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
                    using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(gzip))
                    {
                        var result = TMP.Shared.GenericSerializer.Deserialize<System.Collections.ObjectModel.ObservableCollection<Collector>>(xmlReader);
                        if (result is System.Collections.ObjectModel.ObservableCollection<Collector>)
                        {
                            MainViewModel.Instance.Collectors = result as System.Collections.ObjectModel.ObservableCollection<Collector>;
                            MainViewModel.Instance.IsFullDataLoaded = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, String.Format(ERROR_MESSAGE_TO_LOG, ex.Message, ex.StackTrace));
                }
            });
            return true;
        }

    }
}