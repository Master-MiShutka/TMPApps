namespace TMP.Shared.Windows.Extensions
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Markup;

    public class ValueFromStyleExtension : MarkupExtension
    {
        public ValueFromStyleExtension()
        {
        }

        public object StyleKey { get; set; }

        public DependencyProperty Property { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.StyleKey == null || this.Property == null)
            {
                return null;
            }

            object value = GetValueFromStyle(this.StyleKey, this.Property);
            if (value is MarkupExtension)
            {
                return ((MarkupExtension)value).ProvideValue(serviceProvider);
            }

            return value;
        }

        private static object GetValueFromStyle(object styleKey, DependencyProperty property)
        {
            Style style = Application.Current.TryFindResource(styleKey) as Style;
            while (style != null)
            {
                var setter =
                    style.Setters
                        .OfType<Setter>()
                        .FirstOrDefault(s => s.Property == property);

                if (setter != null)
                {
                    return setter.Value;
                }

                style = style.BasedOn;
            }

            return null;
        }
    }
}
