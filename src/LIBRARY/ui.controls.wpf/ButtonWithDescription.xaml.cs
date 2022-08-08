namespace TMP.UI.WPF.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using TMP.Shared.Common.Commands;

    public class ButtonWithDescription : ContentControl
    {
        public IconKind ImageKind
        {
            get => (IconKind)this.GetValue(ImageKindProperty);
            set => this.SetValue(ImageKindProperty, value);
        }

        public static readonly DependencyProperty ImageKindProperty =
            DependencyProperty.Register("ImageKind", typeof(IconKind), typeof(ButtonWithDescription),
                new FrameworkPropertyMetadata(IconKind.None, FrameworkPropertyMetadataOptions.AffectsRender));

        public double ImageSize
        {
            get => (double)this.GetValue(ImageSizeProperty);
            set => this.SetValue(ImageSizeProperty, value);
        }

        public static readonly DependencyProperty ImageSizeProperty =
            DependencyProperty.Register("ImageSize", typeof(double), typeof(ButtonWithDescription),
            new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty
            .Register("Description",
                    typeof(string),
                    typeof(ButtonWithDescription),
                    new FrameworkPropertyMetadata("Not defined"));

        public string Description
        {
            get => (string)this.GetValue(DescriptionProperty);
            set => this.SetValue(DescriptionProperty, value);
        }

        public DelegateCommand Command
        {
            get => (DelegateCommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(DelegateCommand), typeof(ButtonWithDescription), new FrameworkPropertyMetadata(null));

        public Dock ButtonDock
        {
            get => (Dock)this.GetValue(ButtonDockProperty);
            set => this.SetValue(ButtonDockProperty, value);
        }

        public static readonly DependencyProperty ButtonDockProperty =
            DependencyProperty.Register("ButtonDock", typeof(Dock), typeof(ButtonWithDescription), new FrameworkPropertyMetadata(Dock.Right));
    }
}
