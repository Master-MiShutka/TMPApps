namespace TMPApplication.WpfDialogs
{
    using System.ComponentModel;
    using System.Windows;

    partial class WaitProgressDialogControl : INotifyPropertyChanged
    {
        internal WaitProgressDialogControl(bool isIndeterminate)
        {
            this.AnimationVisibility = isIndeterminate == false
                ? Visibility.Visible : Visibility.Collapsed;
            this.ProgressVisibility = isIndeterminate
                ? Visibility.Visible : Visibility.Collapsed;

            this.IsIndeterminate = isIndeterminate;

            this.InitializeComponent();
        }

        private Visibility animationVisibility;

        public Visibility AnimationVisibility
        {
            get => this.animationVisibility;
            private set
            {
                this.animationVisibility = value;
                this.OnPropertyChanged(nameof(this.AnimationVisibility));
            }
        }

        private Visibility progressVisibility;

        public Visibility ProgressVisibility
        {
            get => this.progressVisibility;
            private set
            {
                this.progressVisibility = value;
                this.OnPropertyChanged(nameof(this.ProgressVisibility));
            }
        }

        private string displayText;

        public string DisplayText
        {
            get => this.displayText;
            set
            {
                this.displayText = Strings.PleaseWait + " (" + value + ")";
                this.displayText = value;
                this.OnPropertyChanged(nameof(this.DisplayText));
            }
        }

        private double progress;

        public double Progress
        {
            get => this.progress;
            set
            {
                this.progress = value;
                if (this.progress < 0)
                {
                    this.progress = 0;
                    this.IsIndeterminate = true;
                }
                else
                {
                    if (this.progress > 100d)
                    {
                        this.progress = 100d;
                    }

                    this.IsIndeterminate = false;
                }

                this.OnPropertyChanged(nameof(this.Progress));
            }
        }

        private bool isIndeterminate = true;

        public bool IsIndeterminate
        {
            get => this.isIndeterminate;
            set
            {
                this.isIndeterminate = value;
                this.OnPropertyChanged(nameof(this.IsIndeterminate));
            }
        }

        public void Finish()
        {
            this.AnimationVisibility = Visibility.Collapsed;
            this.ProgressVisibility = Visibility.Collapsed;
        }

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
