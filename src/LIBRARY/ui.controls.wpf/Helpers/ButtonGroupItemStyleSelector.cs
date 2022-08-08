namespace TMP.UI.WPF.Controls.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    public class ButtonGroupItemStyleSelector : StyleSelector
    {
        private static readonly Dictionary<string, Style> StyleDict = new()
        {
            [ResourceToken.RadioGroupItemSingle] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.RadioGroupItemSingle),
            [ResourceToken.RadioGroupItemHorizontalFirst] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.RadioGroupItemHorizontalFirst),
            [ResourceToken.RadioGroupItemHorizontalLast] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.RadioGroupItemHorizontalLast),
            [ResourceToken.RadioGroupItemVerticalFirst] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.RadioGroupItemVerticalFirst),
            [ResourceToken.RadioGroupItemVerticalLast] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.RadioGroupItemVerticalLast),
            [ResourceToken.RadioGroupItemDefault] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.RadioGroupItemDefault),

            [ResourceToken.ButtonGroupItemSingle] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ButtonGroupItemSingle),
            [ResourceToken.ButtonGroupItemHorizontalFirst] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ButtonGroupItemHorizontalFirst),
            [ResourceToken.ButtonGroupItemHorizontalLast] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ButtonGroupItemHorizontalLast),
            [ResourceToken.ButtonGroupItemVerticalFirst] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ButtonGroupItemVerticalFirst),
            [ResourceToken.ButtonGroupItemVerticalLast] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ButtonGroupItemVerticalLast),
            [ResourceToken.ButtonGroupItemDefault] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ButtonGroupItemDefault),

            [ResourceToken.ToggleButtonGroupItemSingle] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ToggleButtonGroupItemSingle),
            [ResourceToken.ToggleButtonGroupItemHorizontalFirst] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ToggleButtonGroupItemHorizontalFirst),
            [ResourceToken.ToggleButtonGroupItemHorizontalLast] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ToggleButtonGroupItemHorizontalLast),
            [ResourceToken.ToggleButtonGroupItemVerticalFirst] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ToggleButtonGroupItemVerticalFirst),
            [ResourceToken.ToggleButtonGroupItemVerticalLast] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ToggleButtonGroupItemVerticalLast),
            [ResourceToken.ToggleButtonGroupItemDefault] = ResourceHelper.GetResourceInternal<Style>(ResourceToken.ToggleButtonGroupItemDefault),
        };

        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = this.FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var control = container as Control;
            ControlTemplate t = control.Template;

            Border border = this.FindVisualChild<Border>(control);

            if (container is ButtonGroup buttonGroup && buttonGroup.Orientation == Orientation.Horizontal)
            {
            }
            else
            {
            }

            var s = ((Control)container).Style;
            var ss = s.Setters.Cast<Setter>().Where(setter => setter.Property.Name == "Template").FirstOrDefault().Value;

            ControlTemplate ct = ss as ControlTemplate;
            if (ct != null)
            {
                TemplateContent templateContent = ct.Template;
                // var border = templateContent;
            }

            // if (container is ButtonGroup && item is ButtonBase buttonBase)
            // {
            //    var count = buttonGroup.Items.Count;

            // switch (buttonBase)
            //    {
            //        case RadioButton:
            //            {
            //                if (count == 1)
            //                {
            //                    return StyleDict[ResourceToken.RadioGroupItemSingle];
            //                }

            // var index = buttonGroup.Items.IndexOf(buttonBase);
            //                return buttonGroup.Orientation == Orientation.Horizontal
            //                    ? index == 0
            //                        ? StyleDict[ResourceToken.RadioGroupItemHorizontalFirst]
            //                        : StyleDict[index == count - 1
            //                            ? ResourceToken.RadioGroupItemHorizontalLast
            //                            : ResourceToken.RadioGroupItemDefault]
            //                    : index == 0
            //                        ? StyleDict[ResourceToken.RadioGroupItemVerticalFirst]
            //                        : StyleDict[index == count - 1
            //                            ? ResourceToken.RadioGroupItemVerticalLast
            //                            : ResourceToken.RadioGroupItemDefault];
            //            }

            // case Button:
            //            {
            //                if (count == 1)
            //                {
            //                    return StyleDict[ResourceToken.ButtonGroupItemSingle];
            //                }

            // var index = buttonGroup.Items.IndexOf(buttonBase);
            //                return buttonGroup.Orientation == Orientation.Horizontal
            //                    ? index == 0
            //                        ? StyleDict[ResourceToken.ButtonGroupItemHorizontalFirst]
            //                        : StyleDict[index == count - 1
            //                            ? ResourceToken.ButtonGroupItemHorizontalLast
            //                            : ResourceToken.ButtonGroupItemDefault]
            //                    : index == 0
            //                        ? StyleDict[ResourceToken.ButtonGroupItemVerticalFirst]
            //                        : StyleDict[index == count - 1
            //                            ? ResourceToken.ButtonGroupItemVerticalLast
            //                            : ResourceToken.ButtonGroupItemDefault];
            //            }

            // case ToggleButton:
            //            {
            //                if (count == 1)
            //                {
            //                    return StyleDict[ResourceToken.ToggleButtonGroupItemSingle];
            //                }

            // var index = buttonGroup.Items.IndexOf(buttonBase);
            //                return buttonGroup.Orientation == Orientation.Horizontal
            //                    ? index == 0
            //                        ? StyleDict[ResourceToken.ToggleButtonGroupItemHorizontalFirst]
            //                        : StyleDict[index == count - 1
            //                            ? ResourceToken.ToggleButtonGroupItemHorizontalLast
            //                            : ResourceToken.ToggleButtonGroupItemDefault]
            //                    : index == 0
            //                        ? StyleDict[ResourceToken.ToggleButtonGroupItemVerticalFirst]
            //                        : StyleDict[index == count - 1
            //                            ? ResourceToken.ToggleButtonGroupItemVerticalLast
            //                            : ResourceToken.ToggleButtonGroupItemDefault];
            //            }
            //    }
            // }
            return null;
        }
    }
}
