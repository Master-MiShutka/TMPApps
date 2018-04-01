using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// Interaction logic for NoData.xaml
    /// </summary>
    public partial class NoData : UserControl
    {
        public NoData()
        {
            InitializeComponent();
        }

        public const string DefaultMessage = "Нет данных\nдля отображения";

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(NoData), new PropertyMetadata(DefaultMessage));

        [Bindable(true)]
        [DefaultValue(DefaultMessage)]
        [Category("Behavior")]
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
    }
}
