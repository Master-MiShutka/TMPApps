namespace TMP.UI.WPF.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public class ButtonWithIconKind : Button
    {
        public ButtonWithIconKind()
        {
        }

        static ButtonWithIconKind()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonWithIconKind), new FrameworkPropertyMetadata(typeof(ButtonWithIconKind)));
        }

        public IconKind ImageKind
        {
            get => (IconKind)this.GetValue(ImageKindProperty);
            set => this.SetValue(ImageKindProperty, value);
        }

        public static readonly DependencyProperty ImageKindProperty =
            DependencyProperty.Register(nameof(ImageKind), typeof(IconKind), typeof(ButtonWithIconKind),
                new FrameworkPropertyMetadata(IconKind.None, FrameworkPropertyMetadataOptions.AffectsRender));

        public double ImageSize
        {
            get => (double)this.GetValue(ImageSizeProperty);
            set => this.SetValue(ImageSizeProperty, value);
        }

        public static readonly DependencyProperty ImageSizeProperty =
            DependencyProperty.Register(nameof(ImageSize), typeof(double), typeof(ButtonWithIconKind),
            new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsRender));

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(ButtonWithIconKind),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender));
    }
}
