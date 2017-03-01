using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    using Dialogs = TMPApplication.WpfDialogs.Contracts;

    /// <summary>
    /// Interaction logic for GetEnergyControl.xaml
    /// </summary>
    public partial class GetEnergyControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        private bool _isCompleted = false;
        private bool _isGettingData = false;
        private ObservableCollection<ListPointWithResult> _list;
        private ICollectionView _view;
        private Action _onClosed, _onCanceled;
        private Action<Exception> _onFaulted;
        private double _progress = 0d;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        #endregion

        #region Constructors

        public GetEnergyControl()
        {
            InitializeComponent();

            CancelCommand = new DelegateCommand(
                o =>
                {
                    var r = App.ShowQuestion(Strings.InterruptQuestion);
                    if (r == MessageBoxResult.Yes)
                    {
                        _cts.Cancel();
                        App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    }
                },
                o => _cts != null && (_cts.IsCancellationRequested == false));
            CloseControlCommand = new DelegateCommand(o =>
                {
                    if (_onClosed != null)
                        _onClosed();
                });

            SaveCommand = new DelegateCommand(o =>
                {
                    try
                    {
                        if (_list != null)
                        {
                            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                            sfd.Filter = Strings.DialogCsvFilter;
                            sfd.DefaultExt = ".csv";
                            sfd.AddExtension = true;
                            Nullable<bool> result = sfd.ShowDialog(App.Current.MainWindow);
                            if (result == true)
                            {
                                App.Log("Сохранение данных по ПС");
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("Ид;Родитель;Название;A+;A-;R+;R-");
                                foreach (var item in _list)
                                {
                                    EnergyValues values = (EnergyValues)item.ResultValue;
                                    if (values != null && item.IsGroup == false)
                                        sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6}\n", 
                                            item.Id, item.ParentTypeCode, item.Name,
                                            values.AEnergyPlus, values.AEnergyMinus,
                                            values.REnergyPlus, values.REnergyMinus);
                                }
                                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.ShowErrorDialog(ex, Strings.ErrorOnSave);
                    }
                },
                o => _list != null && _list.Count > 0);
            DataContext = this;            
        }

        public GetEnergyControl(IList<ListPoint> source, Action closeed, Action canceled = null, Action<Exception> faulted = null) : this()
        {
            if (source == null || closeed == null)
                throw new ArgumentNullException();
            
            _onClosed = closeed;
            _onCanceled = canceled;
            _onFaulted = faulted;

            List<ListPointWithResult> list = null;

            Dialogs.IDialog dialog = App.WaitingScreen("Подготовка ...");
            dialog.Show();
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {               
                var _s = source
                    .Flatten(i => i.Items)
                    .Where(i => i.TypeCode == "SUBSTATION")
                    .OrderBy(i => i.ParentName)
                    .ThenBy(i => i.Name)
                    .ToList<ListPoint>();

                list = new List<ListPointWithResult>();
                foreach (var point in _s)
                {
                    ListPointWithResult pwr = new ListPointWithResult(point) { Items = null };
                    var items = point.Items
                        .Flatten(i => i.Items)
                        .Where(i =>
                            (i.ParentTypeCode == "AUXILIARY" && i.TypeCode == "ELECTRICITY" && i.EсpName == "Свои нужды") ||
                            (i.ParentTypeCode == "SECTIONBUS" && (i.ParentName.Contains("10кВ") || i.ParentName.Contains("6кВ"))
                                && i.TypeCode == "ELECTRICITY" && i.EсpName == "Трансформаторы"))
                        .Select(i => new ListPointWithResult(i) { ParentName = pwr.ParentName, ParentTypeCode = pwr.Name })
                        .ToList();

                    pwr.Items = items;

                    list.Add(pwr);
                    foreach (var item in items)
                        list.Add(item);
                }

                if (list == null || list.Count == 0)
                {
                    App.ShowWarningDialog(Strings.EmptyList);
                    dialog.Close();
                    _onClosed();
                }
                List = new ObservableCollection<ListPointWithResult>(list);
                dialog.Close();
            });
        }

        #endregion

        #region Private Methods

        private void GetData()
        {
            IsGettingData = true;
            IsCompleted = false;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var task = Task.Factory.StartNew(() => Go(), token);
            task.ContinueWith((t) =>
                {
                    _onCanceled();
                }, TaskContinuationOptions.OnlyOnCanceled);
            task.ContinueWith((t) =>
                {
                    _onFaulted(t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith((t) =>
            {
                IsCompleted = true;
                IsGettingData = false;
                System.Media.SystemSounds.Asterisk.Play();
                App.Log("Завершено");
                App.UIAction(() => App.Current.MainWindow.Flash());
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void Go()
        {
            int itemsCount = _list.Where(i => i.TypeCode == "SUBSTATION").ToList().Count;
            App.Log("Получение суточных значений по " + itemsCount.ToString() + " подстанциям");
            Action<int> report = pos =>
                {
                    Progress = 100d * pos / itemsCount;
                    App.UIAction(() => App.Current.MainWindow.TaskbarItemInfo.ProgressValue = ((double)pos) / itemsCount);
                };

            int index = 0;
            foreach (var item in _list)
                item.Status = ListPointStatus.Ожидание;

            // columns - POINT_ID, POINT_CODE, ML_ID, PL_T, BT, ET, PL_V, DSTATUS 
            // 381, А+ энергия за месяц"
            // 382, А- энергия за месяц"
            // 383, R+ энергия за месяц"
            // 384, R- энергия за месяц"
            // 385, А+ энергия за сутки"
            // 386, А- энергия за сутки"
            // 387, R+ энергия за сутки"
            // 388, R- энергия за сутки"
            // 389, Средняя P+ мощность за сутки"
            // 390, Средняя P- мощность за сутки"
            // 391, Средняя Q+ мощность за сутки
            // 392, Средняя Q- мощность за сутки
            // 3489, Максимальная получасовая P- мощность за сутки
            // 2301, Максимальная получасовая P+ мощность за сутки
            // 3490, Максимальная получасовая P- мощность  за месяц
            // 2303, Максимальная получасовая P+ мощность за месяц
            // 3491, Максимальная часовая P- мощность за сутки
            // 2305, Максимальная часовая P+ мощность за сутки
            // 3492, Максимальная часовая P- мощность за месяц
            // 2307, Максимальная часовая P+ мощность за месяц
            // 3497, Максимальная получасовая Q+ мощность за сутки
            // 3498, Максимальная получасовая Q+ мощность за месяц
            // 3499, Максимальная часовая Q+ мощность за сутки
            // 3500, Максимальная часовая Q+ мощность за месяц
            // 3505, Максимальная получасовая Q- мощность за сутки
            // 3506, Максимальная получасовая Q- мощность за месяц
            // 3507, Максимальная часовая Q- мощность за сутки
            // 3508, Максимальная часовая Q- мощность за месяц

            Func<List<ListPointWithResult>, EnergyValues> getResultForPoints = (points) =>
            {
                decimal? aEnergyPlus = null, aEnergyMinus = null, rEnergyPlus = null, rEnergyMinus = null;

                var values = GetValues(points, "[{id:385},{id:386},{id:387},{id:388}]");
                var aplus = values.Where(r => r.Field<decimal?>("ML_ID") == 385).ToList();
                var aminus = values.Where(r => r.Field<decimal?>("ML_ID") == 386).ToList();
                var rplus = values.Where(r => r.Field<decimal?>("ML_ID") == 387).ToList();
                var rminus = values.Where(r => r.Field<decimal?>("ML_ID") == 388).ToList();
                foreach (var point in points)
                {
                    EnergyValues energy = (EnergyValues)point.ResultValue;
                    if (energy == null) point.ResultValue = energy = new EnergyValues();

                    aEnergyPlus = GetSummOfValues(aplus, point);
                    aEnergyMinus = GetSummOfValues(aminus, point);
                    energy.AEnergyPlus = aEnergyPlus;
                    energy.AEnergyMinus = aEnergyMinus;
                    rEnergyPlus = GetSummOfValues(rplus, point);
                    rEnergyMinus = GetSummOfValues(rminus, point);
                    energy.REnergyPlus = rEnergyPlus;
                    energy.REnergyMinus = rEnergyMinus;
                }
                return new EnergyValues()
                {
                    AEnergyPlus = points.Sum(i => (i.ResultValue as EnergyValues).AEnergyPlus.HasValue ? (i.ResultValue as EnergyValues).AEnergyPlus : 0),
                    AEnergyMinus = points.Sum(i => (i.ResultValue as EnergyValues).AEnergyMinus.HasValue ? (i.ResultValue as EnergyValues).AEnergyMinus : 0),
                    REnergyPlus = points.Sum(i => (i.ResultValue as EnergyValues).REnergyPlus.HasValue ? (i.ResultValue as EnergyValues).REnergyPlus : 0),
                    REnergyMinus = points.Sum(i => (i.ResultValue as EnergyValues).REnergyMinus.HasValue ? (i.ResultValue as EnergyValues).REnergyMinus : 0)
                };
            };

            foreach (var item in _list)
            {
                if (_cts.IsCancellationRequested)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    return;
                }

                if (item.TypeCode != "SUBSTATION") continue;
                item.Status = ListPointStatus.Получение;
                try
                {
                    if (item.IsGroup)
                    {
                        EnergyValues energy = (EnergyValues)item.ResultValue;
                        if (energy == null) item.ResultValue = energy = new EnergyValues();

                        var auxItems = item.Items.Where(i => i.EсpName == "Свои нужды").ToList();
                        var trItems = item.Items.Where(i => i.EсpName == "Трансформаторы").ToList();

                        if (auxItems != null && auxItems.Count > 0)
                        {
                            EnergyValues auxResult = getResultForPoints(auxItems);
                            energy.AuxiliaryAPlus = auxResult.AEnergyPlus;
                            energy.AuxiliaryAMinus = auxResult.AEnergyMinus;
                        }
                        if (trItems != null && trItems.Count > 0)
                        {
                            EnergyValues trResult = getResultForPoints(trItems);
                            energy.AEnergyPlus = trResult.AEnergyPlus;
                            energy.AEnergyMinus = trResult.AEnergyMinus;
                            energy.REnergyPlus = trResult.REnergyPlus;
                            energy.REnergyMinus = trResult.REnergyMinus;
                        }

                        foreach (var child in item.Items)
                            child.Status = ListPointStatus.Готово;
                    }
                    item.ResultType = "decimal";
                }
                catch (Exception ex)
                {
                    App.Log("ЭНЕРГИЯ ОШИБКА: ид точки - " + item.Id.ToString() + ", название - " + item.Name + ", ошибка - " + App.GetExceptionDetails(ex));
                    item.Status = ListPointStatus.Ошибка;
                }
                item.Status = ListPointStatus.Готово;
                report(++index);
            }
        }

        private EnumerableRowCollection<DataRow> GetValues(List<ListPointWithResult> list, string mlids)
        {
            StringBuilder points = new StringBuilder();
            points.Append("[");
            foreach (var item in list)
                points.AppendFormat("{{id:{0}}},", item.Id);
            points.Remove(points.Length - 1, 1);
            points.Append("]");

            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);

            var table = App.EmcosWebServiceClient.GetSTPLData("PSDTU_SERVER", points.ToString(), mlids,
                month.AddMonths(-1).ToString("dd.MM.yyyy"), month.ToString("dd.MM.yyyy"));

            var result = table.AsEnumerable();
            return result;
        }

        private decimal? GetSummOfValues(IEnumerable<DataRow> rows, ListPointWithResult point)
        {
            decimal? result = null;
            if (rows != null)
            {
                var aminusvalues = rows
                .Where(i => i.Field<decimal>("POINT_ID") == point.Id)
                .Select(i => i.Field<decimal?>("PL_V"))
                .Where(i => i.HasValue)
                .Select(i => i / 1000)
                .ToArray();
                if (aminusvalues.Length > 0)
                    result = aminusvalues.Sum();
                else
                    result = null;
            }
            return result;
        }

        #endregion

        #region Command and Properties

        public ICommand CancelCommand { get; private set; }
        public ICommand CloseControlCommand { get; private set; }
        public bool IsCompleted
        {
            get { return _isCompleted; }
            private set { SetProperty(ref _isCompleted, value); }
        }

        public bool IsGettingData
        {
            get { return _isGettingData; }
            private set { SetProperty(ref _isGettingData, value); }
        }

        public ObservableCollection<ListPointWithResult> List
        {
            get { return _list; }
            private set
            {
                SetProperty(ref _list, value);
                View = CollectionViewSource.GetDefaultView(_list);
                View.GroupDescriptions.Add(new PropertyGroupDescription("ParentName"));

                GetData();
            }
        }
        public ICollectionView View
        {
            get { return _view; }
            private set { SetProperty(ref _view, value); }
        }

        public double Progress
        {
            get { return _progress; }
            private set { SetProperty(ref _progress, value); }
        }

        public ICommand SaveAllCommand { get; private set; }
        public ICommand SaveAllInSigleFileCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        #endregion Command and Properties

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

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
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
