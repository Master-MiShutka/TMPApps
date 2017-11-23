using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace TMP.WORK.AramisChetchiki.Charts
{

    /// <summary>
    /// Возращает значение свойства объекта, который отображается в диаграмме
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class LegendConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // объект хранится в поле Tag
            TextBlock label = (TextBlock)value;
            object item = label.Tag;

            // поиск контейнера объекта
            DependencyObject container = (DependencyObject)BaseWPFHelpers.FindElementOfTypeUp((Visual)value, typeof(ListBoxItem));

            ItemsControl owner = ItemsControl.ItemsControlFromItemContainer(container);

            // получение легенды
            Legend legend = (Legend)BaseWPFHelpers.FindElementOfTypeUp(owner, typeof(Legend));

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
