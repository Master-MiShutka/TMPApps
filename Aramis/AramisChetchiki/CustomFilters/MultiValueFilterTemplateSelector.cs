namespace TMP.WORK.AramisChetchiki.CustomFilters
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class MultiValueFilterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LittleAvailableValuesTemplate { get; set; }

        public DataTemplate ManyAvailableValuesTemplate { get; set; }

        public DataTemplate StringFilterTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item != null)
            {
                switch (item)
                {
                    case ItemsFilter.Model.IMultiValueFilter multiValueFilter:
                        if (multiValueFilter.AvailableValues.Cast<object>().Count() <= 4)
                        {
                            return this.LittleAvailableValuesTemplate;
                        }
                        else
                        {
                            return this.ManyAvailableValuesTemplate;
                        }

                    case ItemsFilter.Model.IStringFilter stringFilter:
                        return this.StringFilterTemplate;

                    default:
                        break;
                }
            }

            return null;
        }
    }
}
