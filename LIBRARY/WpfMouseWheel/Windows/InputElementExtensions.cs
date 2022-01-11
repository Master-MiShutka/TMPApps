namespace WpfMouseWheel.Windows
{
    using System.Collections.Generic;
    using System.Windows;

    public static class InputElementExtensions
    {
        public static void RaiseEvent(this IInputElement source, IEnumerable<RoutedEventArgs> args)
        {
            foreach (var e in args)
            {
                source.RaiseEvent(e);
            }
        }
    }
}
