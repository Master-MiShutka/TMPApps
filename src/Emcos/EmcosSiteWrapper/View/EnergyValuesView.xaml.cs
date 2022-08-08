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

namespace TMP.Work.Emcos.View
{
    /// <summary>
    /// Interaction logic for EnergyValuesView.xaml
    /// </summary>
    public partial class EnergyValuesView : UserControl
    {
        public EnergyValuesView()
        {
            InitializeComponent();
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                e.Error.ErrorContent = "Введённое значение не является числом!";
                e.Handled = true;
            }
        }
    }
}
