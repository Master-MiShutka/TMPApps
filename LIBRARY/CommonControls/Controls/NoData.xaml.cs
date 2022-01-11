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

namespace TMP.Wpf.CommonControls
{
    /// <summary>
    /// Логика взаимодействия для NoData.xaml
    /// </summary>
    public partial class NoData : UserControl
    {
        public NoData()
        {
            InitializeComponent();
            DataContext = this;
        }
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message",
            typeof(string),
            typeof(NoData),
            new UIPropertyMetadata("Нет данных\nдля отображения")            
           );
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
    }
}
