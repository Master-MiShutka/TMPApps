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

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Логика взаимодействия для ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        public ProgressControl()
        {
            InitializeComponent();
        }
        public ProgressControl(string message = "", bool hideCopyright = false) : this()
        {
            InitializeComponent();
            progressText.Text = message;
            background.HideCopyright(hideCopyright);
        }
    }
}
