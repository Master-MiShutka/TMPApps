namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for FiltersWindow.xaml
    /// </summary>
    public partial class FiltersWindow : Window
    {
        public FiltersWindow()
        {
            this.InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
