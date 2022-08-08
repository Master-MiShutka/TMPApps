namespace TMP.WORK.AramisChetchiki.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public class SummaryInfoItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ViewAsListTemplate { get; set; }

        public DataTemplate ViewAsTableTemplate { get; set; }

        public DataTemplate ViewAsDiagramTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return null;
            }

            if (container is not FrameworkElement frameworkElement)
            {
                return null;
            }

            return this.ViewAsDiagramTemplate;

            System.Type type = item.GetType();

            DataTemplateKey key = new DataTemplateKey(type);
            return frameworkElement.FindResource(key) as DataTemplate;
        }
    }
}
