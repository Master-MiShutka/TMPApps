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

namespace TMP.Work.Emcos.View
{
    using Model;
    /// <summary>
    /// Interaction logic for SessionManager.xaml
    /// </summary>
    public partial class SessionManager : Window
    {
        private ViewModel.BalansViewModel _vm;
        private SessionManager()
        {
            InitializeComponent();
        }
        public SessionManager(Window parent, ViewModel.BalansViewModel vm) : this()
        {
            this._vm = vm;
            Owner = parent;
            SessionsList = vm.SessionsList;
            DataContext = this;
        }

        public BalansSession SelectedSession { get; set; }
        public IList<BalansSession> SessionsList { get; private set; }

        private void ButtonNewSession_Click(object sender, RoutedEventArgs e)
        {
            using (var session = new BalansSession())
                session.Substations = new List<Model.Balans.Substation>();

        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSession == null)
            {
                MessageBox.Show("Необходимо выбрать сессию из списка!", this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
            Close();
        }

        private void NewSessionButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.CreateEmptySession();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
