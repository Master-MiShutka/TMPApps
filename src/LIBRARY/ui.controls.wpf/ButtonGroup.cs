namespace TMP.UI.WPF.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class ButtonGroup : ListBox
    {
        protected override bool IsItemItsOwnContainerOverride(object item) => item is Button || item is RadioButton || item is ToggleButton;

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(ButtonGroup),
            new PropertyMetadata(default(Orientation)));

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            var count = this.Items.Count;
            for (var i = 0; i < count; i++)
            {
                var item = (ButtonBase)this.Items[i];
                item.Style = this.ItemContainerStyleSelector?.SelectStyle(item, this);
            }
        }
    }
}
