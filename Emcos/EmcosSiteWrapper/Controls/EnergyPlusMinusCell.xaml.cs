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
using TMP.Work.Emcos.Model.Balance;

namespace TMP.Work.Emcos.Controls
{
    /// <summary>
    /// Interaction logic for EnergyPlusMinusCell.xaml
    /// </summary>
    public partial class EnergyPlusMinusCell : UserControl
    {
        public EnergyPlusMinusCell()
        {
            InitializeComponent();
        }

        public IDirectedEnergy DirectedEnergy
        {
            get { return (IDirectedEnergy)GetValue(DirectedEnergyProperty); }
            set { SetValue(DirectedEnergyProperty, value); }
        }

        public static readonly DependencyProperty DirectedEnergyProperty =
            DependencyProperty.Register("DirectedEnergy", typeof(IDirectedEnergy), typeof(EnergyPlusMinusCell), new PropertyMetadata(null));


    }
}
