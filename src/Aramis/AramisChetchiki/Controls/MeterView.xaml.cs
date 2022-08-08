namespace TMP.WORK.AramisChetchiki.Controls
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;
    using System.Windows.Input;
    using TMP.Shared.Commands;

    /// <summary>
    /// Interaction logic for MeterView.xaml
    /// </summary>
    public partial class MeterView : UserControl, INotifyPropertyChanged
    {
        private Action closeAction;

        public MeterView()
        {
            this.InitializeComponent();

            this.CommandOK = new DelegateCommand(() =>
            {
                this.CloseAction();
            });

            this.Focusable = true;
            this.Loaded += (s, e) => Keyboard.Focus(this);
        }

        public ICommand CommandOK { get; }

        public Action CloseAction
        {
            get => this.closeAction;
            set => this.SetProperty(ref this.closeAction, value);
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
        #endregion

    }
}
