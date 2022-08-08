namespace WpfMouseWheel.Windows.Input
{
    public interface IMouseWheelInputListener
    {
        void OnPreviewInput(object sender, MouseWheelInputEventArgs e);

        void OnInput(object sender, MouseWheelInputEventArgs e);
    }
}
