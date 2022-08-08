namespace TMP.UI.WPF.Controls.Behaviours
{
    using System;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using Microsoft.Xaml.Behaviors;

    /// <summary>
    /// When attached to a framework element, the <see cref="P:System.Windows.Documents.TextElement.FontSize"/> property
    /// will be changed upon Ctrl+MouseWheel events.
    /// </summary>
    public class ZoomFontSizeOnMouseWheelBehavior : Behavior<FrameworkElement>
    {
        private double? initialFontSize;
        private int zoomOffset;

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.PreviewMouseWheel += this.AssociatedObject_PreviewMouseWheel;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>
        /// Override this to unhook functionality from the AssociatedObject.
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.PreviewMouseWheel -= this.AssociatedObject_PreviewMouseWheel;
        }

        private void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) || (e.Delta == 0))
            {
                return;
            }

            e.Handled = true;

            var newZoomOffset = this.zoomOffset + Math.Sign(e.Delta);

            var frameworkElement = this.AssociatedObject;
            if (frameworkElement == null)
            {
                return;
            }

            if (!this.initialFontSize.HasValue)
            {
                this.initialFontSize = TextElement.GetFontSize(frameworkElement);
            }

            var newFontSize = this.initialFontSize.Value + newZoomOffset;

            if ((newFontSize < 4) || (newFontSize >= 48))
            {
                return;
            }

            this.zoomOffset = newZoomOffset;
            TextElement.SetFontSize(frameworkElement, newFontSize);

            if (newZoomOffset == 0)
            {
                this.initialFontSize = null;
            }
        }
    }
}
