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

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for GetAuxiliaryControl.xaml
    /// </summary>
    public partial class GetAuxiliaryControl : WaitableControl
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

        public GetAuxiliaryControl()
        {
            InitializeComponent();

            CancelCommand = new DelegateCommand(
                o =>
                {
                    App.UIAction(() =>
                    {
                        var r = App.ShowQuestion(Strings.InterruptQuestion);
                        if (r == MessageBoxResult.Yes)
                        {
                            _cts.Cancel();
                            App.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        }
                    });
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
                        ListPointWithResult point = o as ListPointWithResult;
                        if (point != null)
                        {
                            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                            sfd.FileName = String.Format(Strings.SingleFileNameTemplate, point.ResultName, DateTime.Now.AddMonths(-1));
                            sfd.AddExtension = true;
                            Nullable<bool> result = sfd.ShowDialog(App.Current.MainWindow);
                            if (result == true)
                            {
                                //
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.ShowError(ex, Strings.ErrorOnSave);
                    }
                });
            DataContext = this;
        }

        public GetAuxiliaryControl(IList<ListPoint> source, Action closeed, Action canceled = null, Action<Exception> faulted = null) : this()
        {
            if (source == null || closeed == null)
                throw new ArgumentNullException();
            List = new ObservableCollection<ListPointWithResult>(source.Select(i => new ListPointWithResult(i)).ToList());

            _onClosed = closeed;
            _onCanceled = canceled;
            _onFaulted = faulted;
        }

        #endregion

        #region Private Methods

        private void Go()
        {
            int itemsCount = _list.Count;
            Action<int> report = pos =>
                {
                    Progress = 100d * pos / itemsCount;
                    App.UIAction(() => App.Current.MainWindow.TaskbarItemInfo.ProgressValue = ((double)pos) / itemsCount);
                };

            IsGettingData = true;
            IsCompleted = false;

            int index = 0;
            foreach (var item in _list)
                item.Status = ListPointStatus.Ожидание;

            StringBuilder points = new StringBuilder();
            points.Append("[");
            foreach (var point in _list)
                points.AppendFormat("{{id:{0}}},", point.Id);
            points.Remove(points.Length - 1, 1);
            points.Append("]");

            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);

            var table = App.EmcosWebServiceClient.GetSTPLData("PSDTU_SERVER", points.ToString(), "[{id:385},{id:386}]", 
                month.AddMonths(-1).ToString("dd.MM.yyyy"), month.AddDays(-1).ToString("dd.MM.yyyy"));
            System.Data.DataView d = null;
            if (table != null && table.DefaultView != null)
                d = table.DefaultView;


            var r = d[0][0];


            foreach (var item in _list)
            {
                if (_cts.IsCancellationRequested)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    return;
                }
                item.Status = ListPointStatus.Получение;
                try
                {                      

                    item.ResultValue = 0d;

                    item.ResultType = "double";
                }
                catch
                {
                    item.Status = ListPointStatus.Ошибка;
                }
                item.Status = ListPointStatus.Готово;

                report(++index);
            }
            IsCompleted = true;
            IsGettingData = false;
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var task = Task.Run(() => Go(), token);
            task.ContinueWith((t) =>
                {
                    _onCanceled();
                }, TaskContinuationOptions.OnlyOnCanceled);
            task.ContinueWith((t) =>
                {
                    _onFaulted(t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
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
                View.GroupDescriptions.Add(new PropertyGroupDescription { PropertyName = "ParentName" });
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
    }
}
