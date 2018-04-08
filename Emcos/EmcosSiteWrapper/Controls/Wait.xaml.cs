using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TMP.Work.Emcos.Controls
{
    /// <summary>
    /// Interaction logic for Wait.xaml
    /// </summary>
    public partial class Wait : UserControl
    {
        //private System.Threading.Timer timer;
        public Wait()
        {
            //Visibility = Visibility.Hidden;
            Visibility = Visibility.Visible;
            DataContext = this;

            InitializeComponent();
            Message = DefaultMessage;
            progressBar.IsIndeterminate = false;
            progressBar.Value = 0d;
            progressBar.Minimum = 0d;
            progressBar.Maximum = 100d;

            Unloaded += Wait_Unloaded;
        }

        private void Wait_Unloaded(object sender, RoutedEventArgs e)
        {
            var stateObj = GetParentobject() as IStateObject;
            if (stateObj != null)
                stateObj.PropertyChanged -= Parent_PropertyChanged;
        }

        public static readonly DependencyProperty LogProperty = DependencyProperty.Register("Log", typeof(string), typeof(Wait), new PropertyMetadata(String.Empty));

        [Bindable(true)]
        [DefaultValue(DefaultMessage)]
        [Category("Behavior")]
        public string Log
        {
            get { return (string)GetValue(LogProperty); }
            set { SetValue(LogProperty, value); }
        }

        public const string DefaultMessage = "Пожалуйста, подождите ...";

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(Wait), new PropertyMetadata(DefaultMessage));

        [Bindable(true)]
        [DefaultValue(DefaultMessage)]
        [Category("Behavior")]
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty ProgressVisibleProperty = DependencyProperty.Register("ProgressVisible", typeof(bool), typeof(Wait), new PropertyMetadata(false));

        [Bindable(true)]
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool ProgressVisible
        {
            get { return (bool)GetValue(ProgressVisibleProperty); }
            set { SetValue(ProgressVisibleProperty, value); }
        }
        
        private FrameworkElement GetParentobject()
        {
            var parentObj = this.Parent as FrameworkElement;
            while ((parentObj != null) && ((parentObj is IStateObject) == false))
            {
                if ((parentObj is IStateObject) == false)
                    parentObj = parentObj.Parent as FrameworkElement;
            }
            return parentObj;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            var stateObj = GetParentobject() as IStateObject;

            if (stateObj == null)
                this.Visibility = Visibility.Hidden;
            else
            {
                stateObj.PropertyChanged += Parent_PropertyChanged;
                CheckVisibility(stateObj.State);
            }
        }
        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var parent = sender as IStateObject;
            if (parent == null)
                return;
            if (e.PropertyName == "State")
            {
                    if (this.Dispatcher.CheckAccess())
                        CheckVisibility(parent.State);
                    else
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => CheckVisibility(parent.State)));
            }
            if (e.PropertyName == "Progress")
            {
                if (parent.Progress > 0)
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                    {
                        progressBar.Value = parent.Progress;

                        Log = parent.Log;
                    }));
                if (parent.Progress == 0)
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => progressBar.Value = 0));
            }
        }
        private void CheckVisibility(State state)
        {
            if (state == State.Busy)
            {
                if (this.Dispatcher.CheckAccess())
                    Visibility = Visibility.Visible;
                else
                    Dispatcher.Invoke(new Action(() => Visibility = Visibility.Visible));// Init();
            }
            else
            {
                if (this.Dispatcher.CheckAccess())
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    Dispatcher.Invoke(new Action(() => Visibility = Visibility.Hidden));
                }
            }
        }

        private void Init()
        {
           /* timer = new System.Threading.Timer(timerCallback, null, TimeSpan.FromSeconds(2).Milliseconds, 
                System.Threading.Timeout.Infinite);*/
        }

        private void timerCallback(object state)
        {
            //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => Visibility = Visibility.Visible));
        }
    }
}
