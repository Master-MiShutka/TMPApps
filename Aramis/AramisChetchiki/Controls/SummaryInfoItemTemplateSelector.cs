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
                return null;

            var frameworkElement = container as FrameworkElement;
            if (frameworkElement == null)
                return null;

            return this.ViewAsDiagramTemplate;

            var type = item.GetType();

            var key = new DataTemplateKey(type);
            return frameworkElement.FindResource(key) as DataTemplate;
        }
    }
}
