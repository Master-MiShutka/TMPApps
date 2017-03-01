using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMPApplication.WpfDialogs
{
    /// <summary>
    /// Interaction logic for Background.xaml
    /// </summary>
    public partial class Background : UserControl
    {
        public Background()
        {
            InitializeComponent();
        }
        public Background(bool hideCopyright = false) : this()
        { 

            HideCopyright(hideCopyright);
        }

        public void HideCopyright(bool value)
        {
            if (value)
                tbCopyright.Visibility = Visibility.Hidden;
            else
                tbCopyright.Visibility = Visibility.Visible;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window parent = Application.Current.MainWindow;
            if (parent != null)
                this.FontSize = parent.FontSize * 2.0;
        }
    }
}
