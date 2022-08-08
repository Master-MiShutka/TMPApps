using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace TMP.Wpf.CommonControls
{
    using TMP.Shared;
    using TMP.Shared.Common;

    /// <summary>
    /// 
    /// </summary>
    [TemplatePart(Name = ElementCancelButton, Type = typeof(Button))]
    [TemplatePart(Name = ElementMessageTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementLoading, Type = typeof(Button))]
    public class Wait : Control, INotifyPropertyChanged
    {
        private const string ElementCancelButton = "PART_CancelButton";
        private const string ElementMessageTextBlock = "PART_MessageTextBlock";
        private const string ElementLoading = "PART_Loading";

        private IWaitableObject parent;
        private Button _cancelButton;
        private TextBlock _messageTextBlock;
        private Button _loadingControl;
        static Wait()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Wait), new FrameworkPropertyMetadata(typeof(Wait)));
        }
        public Wait()
        {
            /*progressBar.Value = 0d;
            progressBar.Minimum = 0d;
            progressBar.Maximum = 100d;*/
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _cancelButton = EnforceInstance<Button>("PART_Button");
            _messageTextBlock = EnforceInstance<TextBlock>("PART_MessageTextBlock");
            _loadingControl = EnforceInstance<Button>("PART_Loading");

            //_cancelButton.Command = CancelCommand;
            _messageTextBlock.Text = DefaultMessage;
        }
        //Get element from name. If it exist then element instance return, if not, new will be created
        T EnforceInstance<T>(string partName) where T : FrameworkElement, new()
        {
            T element = GetTemplateChild(partName) as T ?? new T();
            return element;
        }

        public const string DefaultMessage = "Пожалуйста, подождите ...";
        /*
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(Wait), 
            new FrameworkPropertyMetadata(DefaultMessage, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMessageChanged));

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Wait me = (Wait)d;
            me.SetValue(MessageProperty, e.NewValue.ToString());
            if (me._messageTextBlock != null)
                me._messageTextBlock.Text = e.NewValue.ToString();
        }

        [Category("Behavior")]
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }


        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", 
            typeof(System.Windows.Input.ICommand), typeof(Wait), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCancelCommandChanged));

        private static void OnCancelCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Wait me = (Wait)d;
            if (me._cancelButton != null)
                me._cancelButton.Command = e.NewValue as ICommand;
        }

        [Category("Behavior")]
        public System.Windows.Input.ICommand CancelCommand
        {
            get { return (System.Windows.Input.ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }
        */
        /*public static readonly DependencyProperty ProgressVisibleProperty = DependencyProperty.Register("ProgressVisible", typeof(bool), typeof(Wait), new PropertyMetadata(false));

        [Bindable(true)]
        [DefaultValue(true)]
        [Category("Behavior")]
        public bool ProgressVisible
        {
            get { return (bool)GetValue(ProgressVisibleProperty); }
            set { SetValue(ProgressVisibleProperty, value); }
        }*/
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (Parent == null)
                throw new NullReferenceException("Wait control - Parent is Null");
            if (Parent is FrameworkElement == false)
                throw new Exception("Wait control - Parent not is FrameworkElement");

            parent = (Parent as FrameworkElement).DataContext as IWaitableObject;

            if (parent == null)
                this.Visibility = Visibility.Hidden;
            else
            {
                parent.PropertyChanged += Parent_PropertyChanged;
                CheckVisibility(parent.State);
            }
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IStateObject parent = sender as IStateObject;
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
                /*if (parent.Progress > 0)
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                    {
                        progressBar.IsIndeterminate = false;
                        progressBar.Value = parent.Progress;
                    }));
                if (parent.Progress == 0)
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => progressBar.IsIndeterminate = true));*/
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
            else if (state == State.Idle)
            {
                if (parent != null)
                    parent.IsCanceled = false;

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

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
