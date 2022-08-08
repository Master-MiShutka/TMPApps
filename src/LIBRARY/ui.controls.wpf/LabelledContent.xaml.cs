namespace TMP.UI.WPF.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for LabelledContent.xaml
    /// </summary>
    public class LabelledContent : ContentControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty
        .Register("Label",
                typeof(string),
                typeof(LabelledContent),
                new FrameworkPropertyMetadata("Unnamed Label"));

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        public Dock LabelDock
        {
            get => (Dock)this.GetValue(LabelDockProperty);
            set => this.SetValue(LabelDockProperty, value);
        }

        public static readonly DependencyProperty LabelDockProperty =
            DependencyProperty.Register("LabelDock", typeof(Dock), typeof(LabelledContent), new PropertyMetadata(Dock.Left));
    }
}
