using MS.Windows.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TMP.Wpf.CommonControls
{
    internal static class TMPWindowHelpers
    {
        /// <summary>
        /// Sets the IsHitTestVisibleInChromeProperty to a TMPWindow template child
        /// </summary>
        /// <param name="window">The TMPWindow</param>
        /// <param name="name">The name of the template child</param>
        public static void SetIsHitTestVisibleInChromeProperty<T>(this TMPWindow window, string name) where T : DependencyObject
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

        public static void ResetAllWindowCommandsBrush(this TMPWindow window)
        {
            if (window.OverrideDefaultWindowCommandsBrush == null)
            {
                window.WindowButtonCommands.ClearValue(Control.ForegroundProperty);
            }
            else
            {
                window.ChangeAllWindowCommandsBrush(window.OverrideDefaultWindowCommandsBrush);
            }
        }

        private static void ChangeAllWindowCommandsBrush(this TMPWindow window, Brush brush)
        {
            window.WindowButtonCommands.SetValue(Control.ForegroundProperty, brush);
        }

        private static void ChangeAllWindowCommandsBrush(this TMPWindow window, Brush brush, Position position)
        {
            if (position == Position.Right || position == Position.Top)
            {
                window.WindowButtonCommands.SetValue(Control.ForegroundProperty, brush);
            }
        }       
    }
}