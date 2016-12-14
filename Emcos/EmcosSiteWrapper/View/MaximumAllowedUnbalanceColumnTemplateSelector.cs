using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TMP.Work.Emcos.View
{
    public class MaximumAllowedUnbalanceColumnTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BalansGroupTemplate { get; set; }
        public DataTemplate BalansItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return (item is Model.Balans.IBalansGroup) ? BalansGroupTemplate: BalansItemTemplate;
        }
    }
}
