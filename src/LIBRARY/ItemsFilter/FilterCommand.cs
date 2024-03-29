﻿namespace ItemsFilter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Define Filter commands.
    /// </summary>
    public static class FilterCommand
    {
        /// <summary>
        /// Gets the value that represents the Show filter routed command.
        /// The Items.Show default gesture KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Alt).
        /// </summary>
        public static readonly RoutedUICommand Show;

        /// <summary>
        /// Gets the value that represents the Clear filter routed command.
        /// </summary>
        public static readonly RoutedUICommand Clear;

        static FilterCommand()
        {
            Show = new RoutedUICommand(Resources.Strings.ShowCodemaidQuickFilterMenuText, "Show", typeof(FilterCommand));
            Show.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Alt));
            Clear = new RoutedUICommand(ItemsFilter.Resources.Strings.ClearFilterText, "Clear", typeof(FilterCommand));
            Clear.InputGestures.Add(new KeyGesture(Key.Delete, ModifierKeys.Control));
        }

        /// <summary>
        /// Seek first parent of type T in visual tree.
        /// </summary>
        /// <typeparam name="T">Type of the parent.</typeparam>
        /// <returns>First parent of type T in visual tree or null, if not exist. </returns>
        internal static T GetParent<T>(this DependencyObject obj)
        where T : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }

            var parent = VisualTreeHelper.GetParent(obj) as DependencyObject;
            if (parent is T)
            {
                return (T)parent;
            }

            return GetParent<T>(parent);
        }
    }
}
