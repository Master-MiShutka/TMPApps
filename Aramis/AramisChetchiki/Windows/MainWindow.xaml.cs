namespace TMP.WORK.AramisChetchiki
{
    using System;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TMPApplication.WpfDialogs.WindowWithDialogs
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            AppSettings.Default.Save();
        }
    }
}
