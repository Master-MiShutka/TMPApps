namespace TMP.WORK.AramisChetchiki.Charts
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Возращает значение свойства объекта, который отображается в диаграмме
    /// </summary>
    [ValueConversion(typeof(Visual), typeof(object))]
    public class LegendConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // объект хранится в поле Tag
            TextBlock label = (TextBlock)value;
            object item = label.Tag;

            // поиск контейнера объекта
            DependencyObject container = WindowWithDialogs.WPFHelpers.FindElementOfTypeUp((Visual)value, typeof(ListBoxItem));

            ItemsControl owner = ItemsControl.ItemsControlFromItemContainer(container);

            // получение легенды
            Legend legend = (Legend)WindowWithDialogs.WPFHelpers.FindElementOfTypeUp(owner, typeof(Legend));

            PropertyDescriptorCollection filterPropDesc = TypeDescriptor.GetProperties(item);
            object itemValue = filterPropDesc[legend.ObjectProperty].GetValue(item);
            return itemValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
