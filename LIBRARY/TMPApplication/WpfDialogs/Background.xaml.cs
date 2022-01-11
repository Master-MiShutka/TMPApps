namespace TMPApplication.WpfDialogs
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for Background.xaml
    /// </summary>
    public partial class Background : UserControl
    {
        public Background()
        {
            this.InitializeComponent();
        }

        public Background(bool hideCopyright = false) : this()
        {

            this.HideCopyright(hideCopyright);
        }

        public void HideCopyright(bool value)
        {
            if (value)
            {
                this.tbCopyright.Visibility = Visibility.Hidden;
            }
            else
            {
                this.tbCopyright.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window parent = Application.Current.MainWindow;
            if (parent != null)
            {
                this.FontSize = parent.FontSize * 2.0;
            }
        }
    }
}
