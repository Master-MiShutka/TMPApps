using System.Windows;
using System.Windows.Controls;

namespace TMP.Work.AmperM.TestApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для LabelledContent.xaml
    /// </summary>
    public partial class LabelledContent : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty
        .Register("Label",
                typeof(string),
                typeof(LabelledContent),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public LabelledContent()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
    }
}
