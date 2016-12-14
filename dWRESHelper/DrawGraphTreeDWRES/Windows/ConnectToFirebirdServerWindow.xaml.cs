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

using TMP.DWRES.DB;

namespace TMP.DWRES.GUI
{
    /// <summary>
    /// Логика взаимодействия для ConnectToFirebirdServerWindow.xaml
    /// </summary>
    public partial class ConnectToFirebirdServerWindow : Window
    {
        private DBConnectionParams dbConnectionParams = new DBConnectionParams();
        public ConnectToFirebirdServerWindow(Window parent)
        {
            InitializeComponent();
            this.Owner = parent;
        }

        public new DBConnectionParams Show()
        {
            bool? result = base.ShowDialog();

            if (result != null)
                if (result == true)
                {
                    dbConnectionParams.DataSource = tbServerAddress.Text;
                    dbConnectionParams.Database = tbDBName.Text;

                    return dbConnectionParams;
                }
                else return null;
            else
                return null;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
