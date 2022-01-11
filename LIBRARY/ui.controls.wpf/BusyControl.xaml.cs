namespace TMP.UI.Controls.WPF
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for BusyControl.xaml
    /// </summary>
    public class BusyControl : ContentControl
    {

        public static readonly DependencyProperty BackgroundFillBrushProperty = DependencyProperty.Register(nameof(BackgroundFillBrush), typeof(Brush), typeof(BusyControl),
            new PropertyMetadata(Brushes.Black));

        [Bindable(true)]
        [DefaultValue(null)]
        [Category("Behavior")]
        public Brush BackgroundFillBrush
        {
            get => (Brush)this.GetValue(BackgroundFillBrushProperty);
            set => this.SetValue(BackgroundFillBrushProperty, value);
        }

        public static readonly DependencyProperty BackgroundFillOpacityProperty = DependencyProperty.Register(nameof(BackgroundFillOpacity), typeof(double), typeof(BusyControl),
            new PropertyMetadata(0.7d));

        [Bindable(true)]
        [DefaultValue(null)]
        [Category("Behavior")]
        public double BackgroundFillOpacity
        {
            get => (double)this.GetValue(BackgroundFillOpacityProperty);
            set => this.SetValue(BackgroundFillOpacityProperty, value);
        }
    }
}
