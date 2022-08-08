namespace TMP.UI.WPF.Controls.TreeListView
{
    using System.Windows;
    using System.Windows.Controls;

    public class RowExpander : Control
    {
        static RowExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(typeof(RowExpander)));
        }
    }
}
