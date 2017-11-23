using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.ARMTES.Controls.DataViews
{
    /// <summary>
    /// Базовый элемент управления для представления данных
    /// </summary>
    [ContentProperty("Items")]
    [DefaultProperty("Items")]
    public class BaseDataView : ItemsControl, INotifyPropertyChanged
    {
        public BaseDataView()
        {
            UpdateCommand = new DelegateCommand(() =>
            {
                Status = "Получение данных";

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var task = Task.Factory.StartNew(Start);
                task.ContinueWith(t =>
                {
                    watch.Stop();
                    System.Diagnostics.Trace.TraceInformation("Update -> {0} ms", watch.ElapsedMilliseconds);

                    Init();

                    IsBusy = false;
                    Status = null;
                    DetailedStatus = null;
                });
                task.ContinueWith(t =>
                {
                    MessageBox.Show(SmallEngineViewerApp.GetExceptionDetails(t.Exception), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }, TaskContinuationOptions.OnlyOnFaulted);
            });

            Init();
        }

        #region Dependency properties

        public static readonly DependencyProperty ToolsProperty =
            DependencyProperty.Register("Tools", typeof(object), typeof(BaseDataView), new UIPropertyMetadata(null));
        /// <summary>
        /// Элемент управления на панели
        /// </summary>
        public object Tools
        {
            get { return (object)GetValue(ToolsProperty); }
            set { SetValue(ToolsProperty, value); }
        }

        #endregion

        #region Properties

        private ICollection<DataItem> _dataItems;
        public virtual ICollection<DataItem> DataItems
        {
            get { return _dataItems; }
            protected set { _dataItems = value; RaisePropertyChanged("DataItems"); }
        }

        /// <summary>
        /// Команда обновления данных
        /// </summary>
        public virtual ICommand UpdateCommand { get; protected set; }

        private string _status = null;
        /// <summary>
        /// Статус
        /// </summary>
        public String Status
        {
            get { return _status; }
            protected set { _status = value; RaisePropertyChanged("Status"); }
        }
        private string _detailedStatus = null;
        /// <summary>
        /// Подробное описание
        /// </summary>
        public String DetailedStatus
        {
            get { return _detailedStatus; }
            protected set { _detailedStatus = value; RaisePropertyChanged("DetailedStatus"); }
        }

        private bool _isBusy = false;
        /// <summary>
        /// Указывает, что выполняется получение данных
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { _isBusy = value; RaisePropertyChanged("IsBusy"); }
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Запуск операции обновления данных
        /// </summary>
        protected virtual void Start()
        {
            System.Threading.Thread.Sleep(3000);
        }

        protected virtual void Init()
        {
            DataItems = new ObservableCollection<DataItem>();
        }

        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }

    public class ColumnDescriptor
    {
        public string HeaderText { get; set; }
        public string DisplayMember { get; set; }

        public ColumnDescriptor(string header, string displayMember)
        {
            HeaderText = header;
            DisplayMember = displayMember;
        }
    }
    public class DataItem
    {
        public string Header { get; set; }
        public IEnumerable<object> Items { get; set; }
        public ICollection<ColumnDescriptor> Columns { get; set; }
    }
}
