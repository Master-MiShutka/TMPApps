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

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task _backgroudTask;
        private CancellationTokenSource cts = new CancellationTokenSource();

        private ObservableCollection<ListPoint> _pointsList;
        /// <summary>
        /// ИД корневой точки с которой строится дерево точек, 60 - Ошмянские ЭС
        /// Id 52 Белэнерго Code 1
        /// Id 53 РУП Гродноэнерго Code 14
        /// Id 60 Ошмянские ЭС Code 143
        /// </summary>
        private readonly ListPoint DEFAULT_ROOT_EMCOS_GROUP = new ListPoint { Id = 60, TypeCode = "FES", Name = "Ошмянские ЭС" };
        private ListPoint _rootEmcosGroup;

        

        public MainWindow()
        {
            InitializeComponent();
            btnGet.IsEnabled = false;
            btnSave.IsEnabled = false;

            App.UIDispatcher = Dispatcher.CurrentDispatcher;

            cts = new CancellationTokenSource();
        }

        private void ShowProgress(string message)
        {
            Cursor = Cursors.Wait;
            ProgressControl progress = new ProgressControl();
            progress.progressLabel.Content = message;
            dialogHost.Content = progress;
        }
        private void HideProgress()
        {
            dialogHost.Content = null;
            Cursor = Cursors.Arrow;
            status.Text = Strings.ReadyMessage;
        }

        private bool CheckAvailability()
        {
            if (ServiceHelper.IsServerOnline())
                return true;
            else
            {
                App.ShowWarning(String.Format(Strings.ServerAvailability, Strings.No));
                return false;
            }
        }

        private void UpdatePoints()
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

            ShowProgress(Strings.UpdatingInProgress);
            status.Text = Strings.UpdatingInProgress;
            _backgroudTask = Task.Factory.StartNew(async () =>
            {
                _index = 0;
                var source = await FillPointsTree(_rootEmcosGroup);
                if (source.Count == 0 && String.IsNullOrEmpty(ServiceHelper.ErrorMessage) == false)
                {
                    App.UIAction(() =>
                    {
                        App.ShowWarning(ServiceHelper.ErrorMessage);
                        HideProgress();
                    });
                    return;
                }
                _pointsList = new ObservableCollection<ListPoint>(source);
                App.UIAction(() =>
                {                    
                    tree.ItemsSource = _pointsList;
                    btnGet.IsEnabled = true;
                    SaveList(_pointsList.ToList());
                    HideProgress();
                });
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        internal readonly int maxItemsCount = 10000;
        private int _index = 0;
        private async Task<IList<ListPoint>> FillPointsTree(ListPoint point)
        {
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = "CSV файл - значения, разделённые точкой с запятой (*.csv)|*.csv";
            sfd.DefaultExt = ".csv";
            sfd.AddExtension = true;
            Nullable<bool> result = sfd.ShowDialog(App.Current.MainWindow);
        }
        private void ButtonPlus_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.WindowsAPICodePack.Dialogs.CommonSaveFileDialog saveCFD = new Microsoft.WindowsAPICodePack.Dialogs.CommonSaveFileDialog();
            saveCFD.AlwaysAppendDefaultExtension = true;
            saveCFD.DefaultExtension = ".csv";
            saveCFD.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("CSV файл - значения, разделённые точкой с запятой", "*.csv"));
            saveCFD.ShowDialog(App.Current.MainWindow);
        }
    }
}