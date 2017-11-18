using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace TMP.UI.Controls.WPF.Converters
{
    public class RemoveSpacesInTextConverter : MarkupExtension
    {
        [ConstructorArgument("value")] // IMPORTANT!!
        public string Value { get; set; }
        public RemoveSpacesInTextConverter() : base() { }
        public RemoveSpacesInTextConverter(object value) : base()
        {
            this.Value = value as string;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object returnValue = null;
            try
            {
                IProvideValueTarget service = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
                if (String.IsNullOrWhiteSpace(this.Value) == false)
                {
                    returnValue = Value.Replace(" ", Environment.NewLine);
                }
            }
            catch (Exception) { returnValue = DependencyProperty.UnsetValue; }
            return returnValue;
        }
    }
}
