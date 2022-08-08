namespace WpfMouseWheel.Windows.Data
{
    using System;
    using System.Windows.Data;
    using System.Windows.Input;

    public class BooleanToWaitCursorConverter : IValueConverter
    {
        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && ((bool)value))
                return Cursors.Wait;
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
