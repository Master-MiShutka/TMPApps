using System.Windows;
using System.Windows.Threading;

namespace TMP.Work.AmperM.TestApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.UIDispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
