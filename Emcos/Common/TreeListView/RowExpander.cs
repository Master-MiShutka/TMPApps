using System.Windows;
using System.Windows.Controls;

namespace TMP.Wpf.Common.Controls.TreeListView
{
    public class RowExpander : Control
    {
        static RowExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(typeof(RowExpander)));
        }
    }
}
