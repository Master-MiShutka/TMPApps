using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TMP.ARMTES.Views
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void ShowHES(object sender, RoutedEventArgs e)
        {
            HESViewWindow wnd = new HESViewWindow();
            wnd.Owner = this;
            wnd.Show();
        }

        private void ShowSESIndications(object sender, RoutedEventArgs e)
        {
            SESIndicationsWindow wnd = new SESIndicationsWindow();
            wnd.Owner = this;
            wnd.DataContext = new ViewModel.SESIndicationsViewModel(wnd);
            wnd.Show();
        }

        private void ShowSES(object sender, RoutedEventArgs e)
        {
            SESViewWindow wnd = new SESViewWindow();
            wnd.Owner = this;
            wnd.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Webservice.CheckHostAvailablity(Webservice.ArmtesServerAddress) == false)
            {
                MessageBox.Show("Web-сервис АРМТЕС не доступен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
