namespace TMP.WORK.AramisChetchiki.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for EnumFlagsViewer.xaml
    /// </summary>
    public partial class EnumFlagsViewer : ItemsControl
    {
        public EnumFlagsViewer()
        {
            this.InitializeComponent();
        }

        public object Value
        {
            get => (object)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(EnumFlagsViewer), new PropertyMetadata(default));
    }
}
