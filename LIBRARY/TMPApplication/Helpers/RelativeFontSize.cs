namespace TMPApplication.Helpers
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public static class RelativeFontSize
    {
        public static readonly DependencyProperty RelativeFontSizeProperty =
            DependencyProperty.RegisterAttached("RelativeFontSize", typeof(double), typeof(RelativeFontSize), new PropertyMetadata(1.0, HandlePropertyChanged));

        private static bool isInTrickery = false;

        public static double GetRelativeFontSize(FrameworkElement target)
                    => (double)target.GetValue(RelativeFontSizeProperty);

        public static void HandlePropertyChanged(object target, DependencyPropertyChangedEventArgs args)
        {
            if (isInTrickery)
            {
                return;
            }

            FrameworkElement element = null;
            DependencyProperty property = null;
            double unchangedFontSize = SystemFonts.MessageFontSize;

            if (target is Control control)
            {
                element = target as Control;
                property = Control.FontSizeProperty;

                // unchangedFontSize = (element as Control).FontSize;
            }
            else
            {
                if (target is TextBlock textBlock)
                {
                    element = textBlock;
                    property = TextBlock.FontSizeProperty;

                    // unchangedFontSize = textBlock.FontSize;
                }
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

        public static void SetRelativeFontSize(FrameworkElement target, double value)
                    => target.SetValue(RelativeFontSizeProperty, value);
    }
}
