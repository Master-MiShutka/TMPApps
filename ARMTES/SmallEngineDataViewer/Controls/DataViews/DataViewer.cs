using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TMP.ARMTES.Controls.DataViews
{
    using TMP.ARMTES.Model;

    /// <summary>
    /// Элемент управления для представления данных
    /// </summary>
    public class DataViewer : Control, INotifyPropertyChanged
    {
        static DataViewer()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DataViewer),
                new FrameworkPropertyMetadata(typeof(DataViewer)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewer"/> class.
        /// </summary>
        public DataViewer()
        {
            DataContext = this;

            UpdateCommand = new DelegateCommand(() =>
            {
                IsBusy = true;
                Status = "Получение данных";

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var task = Task.Factory.StartNew(Start);
                task.ContinueWith(t =>
                {
                    watch.Stop();
                    System.Diagnostics.Trace.TraceInformation("Update выполнено за -> {0} ms", watch.ElapsedMilliseconds);

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

        /// <summary>
        /// Defines the ToolsProperty
        /// </summary>
        public static readonly DependencyProperty ToolsProperty =
            DependencyProperty.Register("Tools", typeof(object), typeof(DataViewer), new UIPropertyMetadata(null));

        /// <summary>
        /// Элемент управления на панели
        /// </summary>
        public object Tools
        {
            get { return (object)GetValue(ToolsProperty); }
            set { SetValue(ToolsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(DataViewList), typeof(DataViewer), new UIPropertyMetadata(null));

        public DataViewList Items
        {
            get { return (DataViewList)GetValue(ItemsProperty); }
            set
            {
                Dispatcher.BeginInvoke((Action)(() =>
                    SetValue(ItemsProperty, value)
                    ));
            }
        }

        /// <summary>
        /// Команда обновления данных
        /// </summary>
        public virtual ICommand UpdateCommand { get; protected set; }

        /// <summary>
        /// Defines the _status
        /// </summary>
        private string _status = null;

        /// <summary>
        /// Статус
        /// </summary>
        public String Status
        {
            get { return _status; }
            protected set { _status = value; RaisePropertyChanged("Status"); }
        }

        /// <summary>
        /// Defines the _detailedStatus
        /// </summary>
        private string _detailedStatus = null;

        /// <summary>
        /// Подробное описание
        /// </summary>
        public String DetailedStatus
        {
            get { return _detailedStatus; }
            protected set { _detailedStatus = value; RaisePropertyChanged("DetailedStatus"); }
        }

        /// <summary>
        /// Defines the _isBusy
        /// </summary>
        private bool _isBusy = false;

        /// <summary>
        /// Указывает, что выполняется получение данных
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { _isBusy = value; RaisePropertyChanged("IsBusy"); }
        }

        /// <summary>
        /// Запуск операции обновления данных
        /// </summary>
        protected virtual void Start()
        {
            System.Threading.Thread.Sleep(3000);
        }

        /// <summary>
        /// The Init
        /// </summary>
        protected virtual void Init()
        {
            if (Items == null)
                Items = new DataViewList();
        }

        /// <summary>
        /// Defines the PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The SetProperty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">The <see cref="T"/></param>
        /// <param name="value">The <see cref="T"/></param>
        /// <param name="propertyName">The <see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// The RaisePropertyChanged
        /// </summary>
        /// <param name="propertyName">The <see cref="string"/></param>
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The OnPropertyChanged
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}