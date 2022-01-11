namespace TMP.WORK.AramisChetchiki.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for BaseView.xaml
    /// </summary>
    public class BaseView : UserControl
    {
        static BaseView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BaseView),
                new FrameworkPropertyMetadata(typeof(BaseView)));
        }

        public BaseView()
        {
        }

        public bool HeaderPanelVisible
        {
            get => (bool)this.GetValue(HeaderPanelVisibleProperty);
            set => this.SetValue(HeaderPanelVisibleProperty, value);
        }

        public static readonly DependencyProperty HeaderPanelVisibleProperty =
            DependencyProperty.Register(nameof(HeaderPanelVisible), typeof(bool), typeof(BaseView), new PropertyMetadata(true));

        public FrameworkElement HeaderPanel
        {
            get => (FrameworkElement)this.GetValue(HeaderPanelProperty);
            set => this.SetValue(HeaderPanelProperty, value);
        }

        public static readonly DependencyProperty HeaderPanelProperty =
            DependencyProperty.Register(nameof(HeaderPanel), typeof(FrameworkElement), typeof(BaseView), new PropertyMetadata(null));
    }
}
