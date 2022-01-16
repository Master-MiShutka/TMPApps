namespace WpfApplicationTest
{
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.DataContext = Enumerable.Repeat(new A(), 10);
        }
    }

    public class A
    {
        public string N { get; set; } = "ddfefef e";

        public Child C1 { get; set; }

        public Child C2 { get; set; }

        public A()
        {
            this.C1 = new Child();
            this.C2 = new Child();
        }
    }

    public class Child
    {
        public string H { get; set; } = "JHw9 w";

        public int V { get; set; } = 100;
    }
}
