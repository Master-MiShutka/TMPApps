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

namespace TMP.Work.Emcos
{
    using Model;
    /// <summary>
    /// Interaction logic for AuxiliaryReportWindow.xaml
    /// </summary>
    public partial class AuxiliaryReportWindow : Window
    {
        public AuxiliaryReportWindow(AuxiliaryReportTreeModel model)
        {
            InitializeComponent();
            tree.Model = model;
        }

        //public AuxiliaryReportTreeModel AuxiliaryTree { get; private set; }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            ;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
