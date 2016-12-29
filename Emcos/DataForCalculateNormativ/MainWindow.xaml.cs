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
        private const string _pointsListFileName = "data.list";
        /// <summary>
        /// ИД корневой точки, 60 - Ошмянские ЭС
        /// Id 52 Белэнерго Code 1
        /// Id 53 РУП Гродноэнерго Code 14
        /// Id 60 Ошмянские ЭС Code 143
        /// </summary>
        private const decimal _rootEmcosGroupIndex = 60;

        public MainWindow()
        {
            InitializeComponent();

            ProgressControl progress = new ProgressControl();
            progress.progressLabel.Content = Strings.TerminatingMessage;
            dialogHost.Content = progress;
            return;


            cts = new CancellationTokenSource();
            if (System.IO.File.Exists(_pointsListFileName))
            {
                var list = Common.RepositoryCommon.BaseRepository<List<ListPoint>>.GzJsonDeSerialize(_pointsListFileName,
                        (e) => App.ShowError(String.Format(Strings.ErrorOnLoadPointsList, e.Message)));
                if (list != null)
                {
                    Data = new ObservableCollection<ListPoint>(list);
                    tree.ItemsSource = Data;
                    btnGet.IsEnabled = true;
                }
            }
            else
            {
                if (App.ShowWarning(
                    String.Format(Strings.ErrorNotFoundPointsList,
                        _pointsListFileName)) == MsgBox.MsgBoxResult.Close)
                    Close();
                _backgroudTask = Task.Factory.StartNew(async () =>
                {
                    _index = 0;
                    var source = await FillPointsTree(new ListPoint { Id = _rootEmcosGroupIndex });
                    Dispatcher.Invoke((Action)(() =>
                    {
                        tree.ItemsSource = new ObservableCollection<ListPoint>(source);
                        Common.RepositoryCommon.BaseRepository<List<ListPoint>>.GzJsonSerialize(
                                source.ToList(),
                                _pointsListFileName,
                                (e) => App.ShowError(String.Format(Strings.ErrorOnSavePointsList, e.Message)));
                    }));
                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_backgroudTask != null)
            {
                if (_backgroudTask.IsCompleted == false)
                {
                    e.Cancel = true;
                    if (cts != null) cts.Cancel();
                    ProgressControl progress = new ProgressControl();
                    progress.progressLabel.Content = Strings.TerminatingMessage;
                    dialogHost.Content = progress;
                    await _backgroudTask;
                    this.Close();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Data != null)
                Common.RepositoryCommon.BaseRepository<List<ListPoint>>.GzJsonSerialize(
                                Data.ToList(),
                                _pointsListFileName,
                                (ex) => App.ShowError(String.Format(Strings.ErrorOnSavePointsList, ex.Message)));
        }

        public ObservableCollection<ListPoint> Data = new ObservableCollection<ListPoint>();

        internal readonly int maxItemsCount = 10000;
        private int _index = 0;
        private async Task<IList<ListPoint>> FillPointsTree(ListPoint point)
        {
            IList<ListPoint> list = await GetElementsAsync(point);
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


        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            btnGet.Content = Strings.InterruptGet;
            btnUpdate.IsEnabled = false;
            btnSave.IsEnabled = false;
            Cursor = Cursors.Wait;

            try
            {
                App.ShowInfo(Strings.OnGettingData);

                var list = HierarchicalListToFlatList(Data.ToList());
                var substations = list.Where(i => i.Type == Model.ElementTypes.SUBSTATION && i.Checked);


                bool isCompleted = false;
                cts = new CancellationTokenSource();
                while (cts.IsCancellationRequested == false && isCompleted == false)
                {
                    ;
                }

            }
            catch (Exception ex)
            {
                Cursor = Cursors.Arrow;
                App.ShowError(String.Format(Strings.Error, ex.Message));
            }

            Cursor = Cursors.Arrow;
            btnGet.Content = Strings.Get;
            btnUpdate.IsEnabled = true;
            btnSave.IsEnabled = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = false;
            Cursor = Cursors.Wait;

            try
            {
                ;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Arrow;
                App.ShowError(String.Format(Strings.Error, ex.Message));
            }

            Cursor = Cursors.Arrow;
            btnSave.IsEnabled = true;
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnUpdate.Content = Strings.InterruptUpdate;
            btnUpdate.IsEnabled = false;
            btnGet.IsEnabled = false;
            Cursor = Cursors.Wait;

            try
            {
                App.ShowInfo(Strings.OnUpdatingPointsList);
                var list = await GetElementsAsync(new ListPoint { Id = _rootEmcosGroupIndex });
                departamentsList.ItemsSource = list;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Arrow;
                App.ShowError(String.Format(Strings.Error, ex.Message));
            }
            
            Cursor = Cursors.Arrow;
            btnUpdate.Content = Strings.Update;
            btnUpdate.IsEnabled = true;
            btnGet.IsEnabled = true;
        }

        private async void departamentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = substationsList.ItemsSource as IList<ListPoint>;
            if (items != null)
                items.Clear();

            items = voltagesList.ItemsSource as IList<ListPoint>;
            if (items != null)
                items.Clear();

            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;
            ListPoint point = e.AddedItems[0] as ListPoint;
            if (point == null)
                return;
            decimal nodeId = point.Id;
            Cursor = Cursors.Wait;
            var list = await GetElementsAsync(new ListPoint { Id = nodeId });
            substationsList.ItemsSource = list;
            Cursor = Cursors.Arrow;
        }

        private async void substationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = voltagesList.ItemsSource as IList<ListPoint>;
            if (items != null)
                items.Clear();

            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;
            ListPoint point = e.AddedItems[0] as ListPoint;
            if (point == null)
                return;
            decimal nodeId = point.Id;
            Cursor = Cursors.Wait;
            var list = await GetElementsAsync(new ListPoint { Id = nodeId });
            voltagesList.ItemsSource = list;
            Cursor = Cursors.Arrow;
        }

        private async Task<IList<ListPoint>> GetElementsAsync(ListPoint parent)
        {
            string data = await EmcosSiteWrapper.Instance.ExecuteFunction(EmcosSiteWrapper.Instance.GetAPointsAsync, parent.Id.ToString());

            if (String.IsNullOrWhiteSpace(data))
            {
                Dispatcher.Invoke((Action)(() => App.ShowWarning(EmcosSiteWrapper.Instance.ErrorMessage)));
                return new List<ListPoint>();
            }
            var records = Utils.ParseRecords(data);
            if (records == null)
                return new List<ListPoint>();

            var list = new List<Model.IEmcosElement>();
            foreach (var nvc in records)
            {
                Emcos.Model.IEmcosElement element;
                if (nvc.Get("Type") == "POINT")
                    element = new Model.EmcosPointElement();
                else
                    element = new Model.EmcosGrElement();

                for (int i = 0; i < nvc.Count; i++)
                {
                    #region Разбор полей
                    int intValue = 0;
                    switch (nvc.GetKey(i))
                    {
                        case "GR_ID":
                        case "POINT_ID":
                            int.TryParse(nvc[i], out intValue);
                            element.Id = intValue;
                            break;
                        case "GR_NAME":
                        case "POINT_NAME":
                            element.Name = nvc[i];
                            break;
                        case "ESP_NAME":
                            (element as Model.EmcosPointElement).EcpName = nvc[i];
                            break;
                        case "TYPE":
                            Model.ElementTypes type;
                            if (Enum.TryParse<Model.ElementTypes>(nvc[i], out type) == false)
                                type = Model.ElementTypes.GROUP;
                            element.Type = type;
                            break;
                        case "GR_TYPE_CODE":
                        case "POINT_TYPE_CODE":
                            element.TypeCode = nvc[i];
                            break;
                    }
                    #endregion
                }
                list.Add(element);
            }
            IList<ListPoint> points = list.Select(i => new ListPoint
            {
                Id = i.Id,
                Name = i.Name,
                IsGroup = i.Type == Model.ElementTypes.GROUP,
                TypeCode = i.TypeCode,
                EspName = i is Model.EmcosPointElement ? (i as Model.EmcosPointElement).EcpName : String.Empty,
                Type = i.Type,
                Checked = false,
                ParentId = parent.Id,
                ParentTypeCode = parent.TypeCode
            }).ToList();
            return points;
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
    }
}