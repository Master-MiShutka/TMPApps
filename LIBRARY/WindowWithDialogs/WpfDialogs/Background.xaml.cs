namespace WindowWithDialogs
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = "Part_Copyright", Type = typeof(TextBlock))]

    public class Background : Control
    {
        TextBlock tbCopyright;

        static Background()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Background), new FrameworkPropertyMetadata(typeof(Background)));
        }

        public bool ShowCopyright
        {
            get { return (bool)GetValue(ShowCopyrightProperty); }
            set { SetValue(ShowCopyrightProperty, value); }
        }

        public static readonly DependencyProperty ShowCopyrightProperty =
            DependencyProperty.Register("ShowCopyright", typeof(bool), typeof(Background), new FrameworkPropertyMetadata(default, propertyChangedCallback: OnShowCopyrightChanged));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.tbCopyright = this.EnforceInstance<TextBlock>("Part_Copyright");
        }

        // Get element from name. If it exist then element instance return, if not, new will be created
        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            T element = this.GetTemplateChild(partName) as T ?? new T();
            return element;
        }

        private static void OnShowCopyrightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Background control = (Background)d;

            control.tbCopyright.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
