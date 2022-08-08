namespace TMP.PrintEngine.Controls.ProgressDialog
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using TMP.PrintEngine.Controls.WaitScreen;
    using TMP.PrintEngine.Resources;

    public class ProgressDialogViewModel : WaitScreenViewModel, IProgressDialogViewModel
    {
        #region IProgressDialogPresenter Properties

        public ProgressDialogViewModel(IProgressDialogView view)
            : base(view)
        {
        }

        public string DialogTitle
        {
            get => (string)this.GetValue(DialogTitleProperty);

            set => this.SetValue(DialogTitleProperty, value);
        }

        public Visibility CancelButtonVisibility
        {
            get => (Visibility)this.GetValue(CancelButtonVisibilityProperty);

            set => this.SetValue(CancelButtonVisibilityProperty, value);
        }

        public ICommand CancelCommand { get; set; }

        public string CancelButtonCaption
        {
            get => (string)this.GetValue(CancelButtonCaptionProperty);

            set => this.SetValue(CancelButtonCaptionProperty, value);
        }

        public double MaxProgressValue
        {
            get => (double)this.GetValue(MaxProgressValueProperty);

            set => this.SetValue(MaxProgressValueProperty, value);
        }

        public double CurrentProgressValue
        {
            get => (double)this.GetValue(CurrentProgressValueProperty);

            set => this.SetValue(CurrentProgressValueProperty, value);
        }

        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty.Register(
            "DialogTitle",
            typeof(string),
            typeof(ProgressDialogViewModel));

        public static readonly DependencyProperty CancelButtonCaptionProperty = DependencyProperty.Register(
            "CancelButtonCaption",
            typeof(string),
            typeof(ProgressDialogViewModel),
            new PropertyMetadata("Отменить"));

        public static readonly DependencyProperty CancelButtonVisibilityProperty = DependencyProperty.Register(
            "CancelButtonVisibility",
            typeof(Visibility),
            typeof(ProgressDialogViewModel));

        public static readonly DependencyProperty MaxProgressValueProperty = DependencyProperty.Register(
            "MaxProgressValue",
            typeof(double),
            typeof(ProgressDialogViewModel));

        public static readonly DependencyProperty CurrentProgressValueProperty = DependencyProperty.Register(
            "CurrentProgressValue",
            typeof(double),
            typeof(ProgressDialogViewModel), new PropertyMetadata(OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var presener = (ProgressDialogViewModel)dependencyObject;
            switch (e.Property.Name)
            {
                case "CurrentProgressValue":
                    presener.UpdateProgressText();
                    break;
            }
        }

        private void UpdateProgressText()
        {
            var percentage = Convert.ToInt32(this.CurrentProgressValue / this.MaxProgressValue * 100);

            // percentage = Math.Max(percentage, 100);
            this.ProgressText = string.Format(ProgressTextFormat, percentage);
        }

        #endregion

        public string ProgressText
        {
            get => (string)this.GetValue(ProgressTextProperty);
            set => this.SetValue(ProgressTextProperty, value);
        }

        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(ProgressDialogViewModel), new UIPropertyMetadata("0% завершено"));

        private const string ProgressTextFormat = "{0}% завершено";

        public void Initialize(ICommand cancelCommand, int maxProgressValue)
        {
            this.MaxProgressValue = maxProgressValue;
            this.CurrentProgressValue = 0;
            this.CancelCommand = cancelCommand;
            this.CancelButtonCaption = Strings.AbortButtonCaption;
            this.Message = Strings.WaitMessage;
            ////CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
