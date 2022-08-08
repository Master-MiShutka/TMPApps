namespace WpfMouseWheel.Windows.Input
{
    using System.Windows.Input;

    public static class MouseDeviceExtensions
    {
        public static MouseWheel GetWheel(this MouseDevice source)
        {
            return MouseWheel.Wheels.GetOrAdd(source, mouseDevice => new MouseWheel(mouseDevice, MouseWheel.Wheels.Count));
        }
    }
}
