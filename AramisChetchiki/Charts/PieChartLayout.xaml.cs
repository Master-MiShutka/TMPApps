using System;
using System.Windows;
using System.Windows.Controls;

namespace TMP.WORK.AramisChetchiki.Charts
{
    /// <summary>
    /// Defines the layout of the pie chart
    /// </summary>
    public partial class PieChartLayout : UserControl
    {
        #region dependency properties

        /// <summary>
        /// The property of the bound object that will be plotted (CLR wrapper)
        /// </summary>
        public String ObjectProperty
        {
            get { return GetObjectProperty(this); }
            set { SetObjectProperty(this, value); }
        }

        // ObjectProperty dependency property
        public static readonly DependencyProperty ObjectPropertyProperty =
                       DependencyProperty.RegisterAttached("ObjectProperty", typeof(String), typeof(PieChartLayout),
                       new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Inherits));

        // ObjectProperty attached property accessors
        public static void SetObjectProperty(UIElement element, String value)
        {
            element.SetValue(ObjectPropertyProperty, value);
        }
        public static String GetObjectProperty(UIElement element)
        {
            return (String)element.GetValue(ObjectPropertyProperty);
        }

        /// <summary>
        /// A class which selects a color based on the item being rendered.
        /// </summary>
        public IColorSelector ColorSelector
        {
            get { return GetColorSelector(this); }
            set { SetColorSelector(this, value); }
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
            InitializeComponent();
        }
    }
}
