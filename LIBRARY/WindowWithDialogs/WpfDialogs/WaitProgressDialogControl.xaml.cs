namespace WindowWithDialogs
{
    using System.Windows;
    using System.Windows.Controls;

    public class WaitProgressDialogControl : ContentControl
    {
        static WaitProgressDialogControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WaitProgressDialogControl), new FrameworkPropertyMetadata(typeof(WaitProgressDialogControl)));
        }

        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(WaitProgressDialogControl), new FrameworkPropertyMetadata(true, propertyChangedCallback: OnIsIndeterminateChanged));

        private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WaitProgressDialogControl control = (WaitProgressDialogControl)d;

            bool value = (bool)e.NewValue;

            control.AnimationVisibility = value == false
                ? Visibility.Visible : Visibility.Collapsed;
            control.ProgressVisibility = value
                ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility AnimationVisibility
        {
            get { return (Visibility)GetValue(AnimationVisibilityProperty); }
            set { SetValue(AnimationVisibilityProperty, value); }
        }

        public static readonly DependencyProperty AnimationVisibilityProperty =
            DependencyProperty.Register("AnimationVisibility", typeof(Visibility), typeof(WaitProgressDialogControl), new PropertyMetadata(Visibility.Visible));

        public Visibility ProgressVisibility
        {
            get { return (Visibility)GetValue(ProgressVisibilityProperty); }
            set { SetValue(ProgressVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ProgressVisibilityProperty =
            DependencyProperty.Register("ProgressVisibility", typeof(Visibility), typeof(WaitProgressDialogControl), new PropertyMetadata(Visibility.Visible));

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register("DisplayText", typeof(string), typeof(WaitProgressDialogControl), new PropertyMetadata(default));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(WaitProgressDialogControl), new FrameworkPropertyMetadata(0.0d, propertyChangedCallback: OnProgressChanged));

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WaitProgressDialogControl control = (WaitProgressDialogControl)d;

            double value = (double)e.NewValue;

            if (value < 0)
            {
                control.Progress = 0d;
                control.IsIndeterminate = true;
            }
            else
            {
                if (control.Progress > 100d)
                {
                    control.Progress = 100d;
                }

                control.IsIndeterminate = false;
            }
        }

        public void Finish()
        {
            this.AnimationVisibility = Visibility.Collapsed;
            this.ProgressVisibility = Visibility.Collapsed;
        }
    }
}
