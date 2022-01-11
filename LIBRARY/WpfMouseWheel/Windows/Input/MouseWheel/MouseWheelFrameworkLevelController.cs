namespace WpfMouseWheel.Windows.Input
{
    using System.Windows;

    public class MouseWheelFrameworkLevelController : MouseWheelController
    {
        public MouseWheelFrameworkLevelController(IFrameworkLevelElement frameworkLevelElement)
          : base(frameworkLevelElement)
        {
            frameworkLevelElement.Unloaded += this.OnElementUnloaded;
        }

        public IFrameworkLevelElement FrameworkLevelElement => this.InputLevelElement as IFrameworkLevelElement;

        public override void Dispose()
        {
            this.FrameworkLevelElement.Unloaded -= this.OnElementUnloaded;
            base.Dispose();
        }

        private void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unload();
        }
    }
}
