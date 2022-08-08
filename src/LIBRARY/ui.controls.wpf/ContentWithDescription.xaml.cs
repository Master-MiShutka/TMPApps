namespace TMP.UI.WPF.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for ContentWithDescription.xaml
    /// </summary>
    public class ContentWithDescription : ContentControl
    {
        public ContentWithDescription()
        {
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty
            .Register("Description",
                    typeof(string),
                    typeof(ContentWithDescription),
                    new FrameworkPropertyMetadata("Not defined"));

        public string Description
        {
            get => (string)this.GetValue(DescriptionProperty);
            set => this.SetValue(DescriptionProperty, value);
        }

        public Dock ContentDock
        {
            get => (Dock)this.GetValue(ContentDockProperty);
            set => this.SetValue(ContentDockProperty, value);
        }

        public static readonly DependencyProperty ContentDockProperty =
            DependencyProperty.Register("ContentDock", typeof(Dock), typeof(ContentWithDescription), new FrameworkPropertyMetadata(Dock.Right));

        [Bindable(true)]
        public Brush DescriptionForeground
        {
            get => (Brush)this.GetValue(DescriptionForegroundProperty);
            set => this.SetValue(DescriptionForegroundProperty, value);
        }

        public static readonly DependencyProperty DescriptionForegroundProperty = DependencyProperty.Register("DescriptionForeground", typeof(Brush), typeof(ContentWithDescription),
            new FrameworkPropertyMetadata(SystemColors.HighlightBrush));

        [Bindable(true)]
        public double DescriptionFontSize
        {
            get => (double)this.GetValue(DescriptionFontSizeProperty);
            set => this.SetValue(DescriptionFontSizeProperty, value);
        }

        public static readonly DependencyProperty DescriptionFontSizeProperty = DependencyProperty.Register("DescriptionFontSize", typeof(double), typeof(ContentWithDescription),
            new FrameworkPropertyMetadata(SystemFonts.CaptionFontSize));

        [Bindable(true)]
        public FontFamily DescriptionFontFamily
        {
            get => (FontFamily)this.GetValue(DescriptionFontFamilyProperty);
            set => this.SetValue(DescriptionFontFamilyProperty, value);
        }

        public static readonly DependencyProperty DescriptionFontFamilyProperty = DependencyProperty.Register("DescriptionFontFamily", typeof(FontFamily), typeof(ContentWithDescription),
            new FrameworkPropertyMetadata(SystemFonts.CaptionFontFamily));
    }
}
