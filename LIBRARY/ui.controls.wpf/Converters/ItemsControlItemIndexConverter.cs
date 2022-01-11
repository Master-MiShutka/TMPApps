namespace TMP.UI.Controls.WPF.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    [System.Windows.Data.ValueConversion(typeof(ListBoxItem), typeof(int))]
    [System.Windows.Data.ValueConversion(typeof(ListViewItem), typeof(int))]
    public class ItemsControlItemIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ContentControl item = (ContentControl)value;
            ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(item);
            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(item);
            return (index + 1).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
