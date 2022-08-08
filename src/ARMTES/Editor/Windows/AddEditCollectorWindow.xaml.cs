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
using System.Windows.Shapes;

using TMP.ARMTES.Model;

namespace TMP.ARMTES.Editor
{
    /// <summary>
    /// Логика взаимодействия для AddEditCollectorWindow.xaml
    /// </summary>
    public partial class AddEditCollectorWindow
    {
        public AddEditCollectorWindow()
        {
            InitializeComponent();
            AddEditCollectorModel.Instance.Collector = new RegistryCollector();
        }

        public AddEditCollectorWindow(RegistryCollector collector)
        {
            if (collector == null) throw new ArgumentNullException("Collector is null!");
            InitializeComponent();
            AddEditCollectorModel.Instance.Collector = collector;
        }        
    }
}
