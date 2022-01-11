namespace TMP.PrintEngine.Controls.WaitScreen
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using TMP.PrintEngine.ViewModels;

    /// <summary>
    /// Interaction logic for WaitScreenView.xaml
    /// </summary>
    public partial class SmallWaitScreenView : UserControl, IWaitScreenView
    {
        public SmallWaitScreenView()
        {
            this.InitializeComponent();
            this.animationTimer = new DispatcherTimer(
                DispatcherPriority.Input, this.Dispatcher)
            { Interval = new TimeSpan(0, 0, 0, 0, 75) };
        }

        #region IView Members

        public IWaitScreenViewModel WaitScreenViewModel;

        public IViewModel ViewModel
        {
            set
            {
                this.WaitScreenViewModel = value as IWaitScreenViewModel;
                this.DataContext = value;
            }
        }

        #endregion

        #region Data
        private readonly DispatcherTimer animationTimer;
        #endregion

        #region Private Methods
        private void Start()
        {
            this.animationTimer.Tick += this.HandleAnimationTick;
            this.animationTimer.Start();
        }

        private void Stop()
        {
            this.animationTimer.Stop();
            this.animationTimer.Tick -= this.HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            this.SpinnerRotate.Angle = (this.SpinnerRotate.Angle + 36) % 360;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            const double offset = Math.PI;
            const double step = Math.PI * 2 / 10.0;

            SetPosition(this.C0, offset, 0.0, step);
            SetPosition(this.C1, offset, 1.0, step);
            SetPosition(this.C2, offset, 2.0, step);
            SetPosition(this.C3, offset, 3.0, step);
            SetPosition(this.C4, offset, 4.0, step);
            SetPosition(this.C5, offset, 5.0, step);
            SetPosition(this.C6, offset, 6.0, step);
            SetPosition(this.C7, offset, 7.0, step);
            SetPosition(this.C8, offset, 8.0, step);
        }

        private static void SetPosition(Ellipse ellipse, double offset,
            double posOffSet, double step)
        {
            const double diff = 10.0;
            ellipse.SetValue(Canvas.LeftProperty, diff
                + (Math.Sin(offset + (posOffSet * step)) * diff));

            ellipse.SetValue(Canvas.TopProperty, diff
                + (Math.Cos(offset + (posOffSet * step)) * diff));
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            this.Stop();
        }

        private void HandleVisibleChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;

            if (isVisible)
            {
                this.Start();
            }
            else
            {
                this.Stop();
            }
        }
        #endregion

    }
}
