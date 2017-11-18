using MS.Windows.Shell;
using System;
using System.Windows;

namespace TMPApplication.CustomWpfWindow
{
    internal static class WindowWithDialogsHelper
    {
        /// <summary>
        /// Sets the IsHitTestVisibleInChromeProperty to a TMPWindow template child
        /// </summary>
        /// <param name="window">The TMPWindow</param>
        /// <param name="name">The name of the template child</param>
        public static void SetIsHitTestVisibleInChromeProperty<T>(this WindowWithDialogs window, string name) where T : DependencyObject
        {
            if (window == null)
            {
                return;
            }
            var elementPart = window.GetPart<T>(name);
            if (elementPart != null)
            {
                elementPart.SetValue(WindowChrome.IsHitTestVisibleInChromeProperty, true);
            }
        }

        /// <summary>
        /// Sets the WindowChrome ResizeGripDirection to a MetroWindow template child.
        /// </summary>
        /// <param name="window">The MetroWindow.</param>
        /// <param name="name">The name of the template child.</param>
        /// <param name="direction">The direction.</param>
        public static void SetWindowChromeResizeGripDirection(this WindowWithDialogs window, string name, ResizeGripDirection direction)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }
            var inputElement = window.GetPart(name) as IInputElement;
            System.Diagnostics.Debug.Assert(inputElement != null, $"{name} is not a IInputElement");
            if (WindowChrome.GetResizeGripDirection(inputElement) != direction)
            {
                WindowChrome.SetResizeGripDirection(inputElement, direction);
            }
        }
    }
}
