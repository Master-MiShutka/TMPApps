using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TMP.Work.Emcos.Controls
{
    /// <summary>
    /// Interaction logic for TableViewHistogramCongfigEditor.xaml
    /// </summary>
    public partial class TableViewHistogramCongfigEditor : ToggleButton
    {
        public TableViewHistogramCongfigEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public TMP.UI.Controls.WPF.TableView.TableView Table
        {
            get { return (TMP.UI.Controls.WPF.TableView.TableView)GetValue(TableProperty); }
            set { SetValue(TableProperty, value); }
        }

        public static readonly DependencyProperty TableProperty =
            DependencyProperty.Register("Table", typeof(TMP.UI.Controls.WPF.TableView.TableView), typeof(TableViewHistogramCongfigEditor), new PropertyMetadata(null));


    }
}
