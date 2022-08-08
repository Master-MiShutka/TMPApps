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

namespace TMP.DWRES.GUI
{
    /// <summary>
    /// Логика взаимодействия для NoDataControl.xaml
    /// </summary>
    public partial class NoDataControl : UserControl
    {
        public NoDataControl()
        {
            DataContext = this;
            InitializeComponent();
        }

        public string Text1
        {
            get { return (string)GetValue(Text1Property); }
            set { SetValue(Text1Property, value); }
        }
        public static readonly DependencyProperty Text1Property = DependencyProperty.Register("Text1", typeof(string), typeof(NoDataControl));
        public string Text2
        {
            get { return (string)GetValue(Text2Property); }
            set { SetValue(Text2Property, value); }
        }
        public static readonly DependencyProperty Text2Property = DependencyProperty.Register("Text2", typeof(string), typeof(NoDataControl));
    }
}
