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

namespace TestApp
{
    using TMP.Wpf.CommonControls;
    /// <summary>
    /// Interaction logic for TestWindow1.xaml
    /// </summary>
    public partial class TestWindow1 : TMPWindow
    {
        public TestWindow1()
        {
            InitializeComponent();

            SampleData.Seed();
            Albums = SampleData.Albums;

            DataContext = this;
        }

        public List<Album> Albums { get; set; }
    }
}
