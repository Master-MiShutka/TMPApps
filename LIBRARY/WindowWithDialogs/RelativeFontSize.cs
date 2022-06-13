namespace WindowWithDialogs
{
    using System.Windows;
    using System.Windows.Controls;

    public static class RelativeFontSize
    {
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.RegisterAttached("Scale", typeof(double), typeof(RelativeFontSize), new PropertyMetadata(1.0, OnScaleChanged));

        private static bool isInTrickery = false;

        public static double GetScale(FrameworkElement target)
                    => (double)target.GetValue(ScaleProperty);

        public static void SetScale(FrameworkElement target, double value)
            => target.SetValue(ScaleProperty, value);

        public static void OnScaleChanged(object target, DependencyPropertyChangedEventArgs args)
        {
            if (isInTrickery)
            {
                return;
            }

            FrameworkElement? element = null;
            DependencyProperty? property = null;
            double unchangedFontSize = SystemFonts.MessageFontSize;

            switch (target)
            {
                case Control control:
                    element = target as Control;
                    property = Control.FontSizeProperty;

                    // unchangedFontSize = (element as Control).FontSize;
                    break;
                case TextBlock textBlock:
                    element = textBlock;
                    property = TextBlock.FontSizeProperty;

                    // unchangedFontSize = textBlock.FontSize;
                    break;
            }

            if (element != null)
            {
                isInTrickery = true;

                try
                {
                    element.SetValue(property, DependencyProperty.UnsetValue);
                    var value = (double)args.NewValue;

                    // control.FontSize = unchangedFontSize * value;
                    element.SetValue(property, unchangedFontSize * value);
                }
                finally
                {
                    isInTrickery = false;
                }
            }
        }
    }
}
