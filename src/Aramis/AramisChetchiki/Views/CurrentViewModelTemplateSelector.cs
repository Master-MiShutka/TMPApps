namespace TMP.WORK.AramisChetchiki.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.WORK.AramisChetchiki.ViewModel;

    public class CurrentViewModelTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DataTemplate tmpl)
            {
                return tmpl;
            }

            if (item is string msg)
            {
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
                factory.SetValue(TextBlock.TextProperty, msg);
                factory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);

                return new DataTemplate { VisualTree = factory };
            }

            if (container is FrameworkElement element && item != null)
            {
                IViewModel viewModel = item as IViewModel;

                Mode currentMode = ModelHelper.ViewModelToModeDictionary[viewModel.GetType()];

                (System.Type view, System.Type vm) tupple = ModelHelper.ModesViewModelTypes[currentMode];

                DataTemplate dataTemplate = new(tupple.view);
                FrameworkElementFactory factory = new(tupple.view);
                dataTemplate.VisualTree = factory;
                dataTemplate.Seal();

                return dataTemplate;
            }

            return null;
        }
    }
}
