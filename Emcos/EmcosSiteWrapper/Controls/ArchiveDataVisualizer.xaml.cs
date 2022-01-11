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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.Work.Emcos.Controls
{
    using Model.Balance;
    /// <summary>
    /// Interaction logic for BalanceItemDataVisualizer.xaml
    /// </summary>
    public partial class ArchiveDataVisualizer : UserControl
    {
        public ArchiveDataVisualizer()
        {
            InitializeComponent();
        }

        public IEnergy Energy
        {
            get { return (IEnergy)GetValue(EnergyProperty); }
            set { SetValue(EnergyProperty, value); }
        }
        public static readonly DependencyProperty EnergyProperty =
            DependencyProperty.Register("Energy", typeof(IEnergy), typeof(ArchiveDataVisualizer), new PropertyMetadata(null));

        public IList<DateTime> Dates
        {
            get { return (IList<DateTime>)GetValue(DatesProperty); }
            set { SetValue(DatesProperty, value); }
        }
        public static readonly DependencyProperty DatesProperty =
            DependencyProperty.Register("Dates", typeof(IList<DateTime>), typeof(ArchiveDataVisualizer), new PropertyMetadata(null));
    }
}
