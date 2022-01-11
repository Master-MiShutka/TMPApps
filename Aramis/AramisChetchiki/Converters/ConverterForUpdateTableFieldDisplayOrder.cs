namespace TMP.WORK.AramisChetchiki.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using TMP.UI.Controls.WPF;

    public class ConverterForUpdateTableFieldDisplayOrder : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            if (value is not ContentControl item)
            {
                return 0;
            }

            if (ItemsControl.ItemsControlFromItemContainer(item) is not ItemsControl itemsControl)
            {
                return 0;
            }

            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(item) + 1;

            itemsControl.InvalidateVisual();

            if (item.DataContext is TableField field)
            {
                field.DisplayOrder = index - 1;
            }

            /*Controls.SelectorFieldsAndSortCollectionView.Field field2 = (item?.DataContext) as Controls.SelectorFieldsAndSortCollectionView.Field;
            if (field2 != null)
                field2.DisplayIndex = index - 1;*/

            return index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
