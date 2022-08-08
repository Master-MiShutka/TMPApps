namespace TMP.WORK.AramisChetchiki.Charts
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Defines the layout of the pie chart
    /// </summary>
    public partial class PieChartLayout : UserControl
    {
        #region dependency properties

        /// <summary>
        /// The property of the bound object that will be plotted (CLR wrapper)
        /// </summary>
        public string ObjectProperty
        {
            get => GetObjectProperty(this);
            set => SetObjectProperty(this, value);
        }

        // ObjectProperty dependency property
        public static readonly DependencyProperty ObjectPropertyProperty =
                       DependencyProperty.RegisterAttached(nameof(ObjectProperty), typeof(string), typeof(PieChartLayout),
                       new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

        // ObjectProperty attached property accessors
        public static void SetObjectProperty(UIElement element, string value)
        {
            element.SetValue(ObjectPropertyProperty, value);
        }

        public static string GetObjectProperty(UIElement element)
        {
            return (string)element.GetValue(ObjectPropertyProperty);
        }

        /// <summary>
        /// A class which selects a color based on the item being rendered.
        /// </summary>
        public IColorSelector ColorSelector
        {
            get => GetColorSelector(this);
            set => SetColorSelector(this, value);
        }

        // ColorSelector dependency property
        public static readonly DependencyProperty ColorSelectorProperty =
                       DependencyProperty.RegisterAttached("ColorSelectorProperty", typeof(IColorSelector), typeof(PieChartLayout),
                       new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        // ColorSelector attached property accessors
        public static void SetColorSelector(UIElement element, IColorSelector value)
        {
            element.SetValue(ColorSelectorProperty, value);
        }

        public static IColorSelector GetColorSelector(UIElement element)
        {
            return (IColorSelector)element.GetValue(ColorSelectorProperty);
        }

        #endregion

        public PieChartLayout()
        {
            this.InitializeComponent();
        }
    }
}
