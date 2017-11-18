using System.Windows;

namespace TMP.UI.Controls.WPF.Extensions
{
    public static class DependencyExtensions
    {
        /// <summary>
        /// Sets the value of the <paramref name="property"/> only if it hasn't been explicitly set.
        /// </summary>
        public static bool SetIfDefault<T>(this DependencyObject o, DependencyProperty property, T value)
        {
            if (DependencyPropertyHelper.GetValueSource(o, property).BaseValueSource == BaseValueSource.Default)
            {
                o.SetValue(property, value);

                return true;
            }

            return false;
        }
    }
}
