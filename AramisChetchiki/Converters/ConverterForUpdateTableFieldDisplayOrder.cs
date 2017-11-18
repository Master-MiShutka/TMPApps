using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.WORK.AramisChetchiki.Converters
{
    public class ConverterForUpdateTableFieldDisplayOrder : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;
            ContentControl item = value as ContentControl;
            if (item == null)
                return 0;

            ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(item) as ItemsControl;
            if (itemsControl == null) return 0;
            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(item) + 1;

            itemsControl.InvalidateVisual();

            Properties.TableField field = (item?.DataContext) as Properties.TableField;
            if (field != null)
                field.DisplayOrder = index - 1;

            Windows.SelectFieldsAndSortCollectionViewWindow.Field field2 = (item?.DataContext) as Windows.SelectFieldsAndSortCollectionViewWindow.Field;
            if (field2 != null)
                field2.DisplayIndex = index - 1;

            return index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
