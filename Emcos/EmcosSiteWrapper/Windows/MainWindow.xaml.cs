using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace TMP.Work.Emcos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IStateObject
    {
        private DateTime _start;
        private DateTime _end;

        private List<String> _periodsData = new List<string>()
        {
            "предыдущий месяц",
            "текущий месяц",
            "период"
        };

        private EmcosSite emcosSite = new EmcosSite();

        public MainWindow()
        {
            InitializeComponent();

            period.ItemsSource = _periodsData;
            period.SelectedIndex = 2;
            startDate.SelectedDate = DateTime.Now;
            endDate.SelectedDate = DateTime.Now;

            State = State.Idle;
        }

        private bool _isAuthorized = false;
        public bool IsAuthorized
        {
            get { return _isAuthorized; }
            set { _isAuthorized = value; RaisePropertyChanged("IsAuthorized"); }
        }
        private void Authorization(object sender, RoutedEventArgs e)
        {
            ;
        }

        private void DateSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Control c = sender as Control;
            string tag = c.Tag == null ? String.Empty : c.Tag.ToString();

            if (String.Equals(tag, "start"))
                _start = this.startDate.SelectedDate.GetValueOrDefault();
            if (String.Equals(tag, "end"))
                _end = this.endDate.SelectedDate.GetValueOrDefault();
        }

        private void period_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime today = DateTime.Today;
            DateTime month = new DateTime(today.Year, today.Month, 1);

            ComboBox cb = e.Source as ComboBox;
            if ((cb == null) || (cb.SelectedIndex < 0)) return;

            switch (cb.SelectedIndex)
            {
                case 0:
                    var first = month.AddMonths(-1);
                    var last = month.AddDays(-1);

                    this.startDate.SelectedDate = first;
                    this.endDate.SelectedDate = last;
                    break;
                case 1:
                    this.startDate.SelectedDate = month;
                    this.endDate.SelectedDate = today;
                    break;
                case 2:
                    this.startDate.SelectedDate = today;
                    this.endDate.SelectedDate = today;
                    break;
            }
        }

        private void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                State = State.Busy;

                System.Threading.Tasks.Task<string[]> response = EmcosSiteWrapper.Instance.GetArchiveWIds(
                    aptree.GetListOfMeasurementsAsString,
                    aptree.GetListOfGroupsAndPointsAsString,
                    _start,
                    _end,
                    new System.Threading.CancellationToken());

                response.ContinueWith((t) =>
                {
                    string msg = t.Exception.Message;
                    MessageBox.Show(String.Format("Произошла ошибка!\n{0}", msg), "Просмотр архивов", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    State = State.Idle;

                }, System.Threading.CancellationToken.None,
                    System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted,
                    System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
                response.ContinueWith((t) =>
                {
                    string[] result = t.Result;
                   
                    State = State.Idle;
                    lblResult.Text = "Выполнено успешно!" + "\n" + result;
                    archViewer.Init(result[0], _start.ToString("yyyy.MM.dd"), _end.ToString("yyyy.MM.dd"));

                }, System.Threading.CancellationToken.None,
                    System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion,
                    System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                State = State.Idle;
            }
                     
        }

        private void main_Loaded(object sender, RoutedEventArgs e)
        {
            State = State.Busy;

            EmcosSiteWrapper.Instance.Login(new System.Threading.CancellationToken()).ContinueWith((s) => 
                State = State.Idle,
                System.Threading.CancellationToken.None,
                System.Threading.Tasks.TaskContinuationOptions.None,
                System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CheckRights_Click(object sender, RoutedEventArgs e)
        {
            State = State.Busy;
            try
            {
                EmcosSiteWrapper.Instance.CheckRights((answer) =>
                {
                    lblRights.Content = System.Web.HttpUtility.UrlDecode(answer);
                    State = State.Idle;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                State = State.Idle;
            }
        }


        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IStateObject implementation
        private State _state = State.Idle;
        public State State
        {
            get { return _state; }
            set { _state = value; RaisePropertyChanged("State"); }
        }
        private int _progress = 0;
        public int Progress
        {
            get { return _progress; }
            set { _progress = value; RaisePropertyChanged("Progress"); }
        }
        public string Log { get; set; }
        #endregion

        private void main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
                State = State.Busy;
            if (e.Key == Key.F12)
                State = State.Idle;
        }
    }
}
