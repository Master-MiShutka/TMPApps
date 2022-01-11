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
        public DataTemplate BalanceGroupTemplate { get; set; }
        public DataTemplate BalanceItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return (item is Model.Balance.IBalanceGroupItem) ? BalanceGroupTemplate: BalanceItemTemplate;
        }
    }
}
