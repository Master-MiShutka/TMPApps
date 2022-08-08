namespace TMP.UI.WPF.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    [System.Windows.Markup.ContentProperty(nameof(Additional))]
    /// <summary>
    /// Interaction logic for NoData.xaml
    /// </summary>
    public class NoData : ContentControl
    {
        public NoData()
        {
        }

        public const string DefaultMessage = "Нет данных\nдля отображения";

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(NoData),
            new PropertyMetadata(DefaultMessage));

        [Bindable(true)]
        [DefaultValue(DefaultMessage)]
        [Category("Behavior")]
        public string Message
        {
            get => (string)this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty AdditionalProperty = DependencyProperty.Register(nameof(Additional), typeof(object), typeof(NoData),
            new PropertyMetadata(null));

        [Bindable(true)]
        [DefaultValue(null)]
        [Category("Behavior")]
        public object Additional
        {
            get => (object)this.GetValue(AdditionalProperty);
            set => this.SetValue(AdditionalProperty, value);
        }
    }
}
