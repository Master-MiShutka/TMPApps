namespace TMP.WORK.AramisChetchiki.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for GlowEffectTextBlock.xaml
    /// </summary>
    public partial class GlowEffectTextBlock : UserControl
    {
        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(GlowEffectTextBlock),
                new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        // Using a DependencyProperty as the backing store for GlowColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register(
                nameof(GlowColor),
                typeof(Color),
                typeof(GlowEffectTextBlock),
                new FrameworkPropertyMetadata(Colors.WhiteSmoke, FrameworkPropertyMetadataOptions.AffectsRender));

        public GlowEffectTextBlock()
        {
            this.InitializeComponent();
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public Color GlowColor
        {
            get => (Color)this.GetValue(GlowColorProperty);
            set => this.SetValue(GlowColorProperty, value);
        }
    }
}
