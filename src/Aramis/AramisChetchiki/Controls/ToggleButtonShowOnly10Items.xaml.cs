﻿namespace TMP.WORK.AramisChetchiki.Controls
{
    using System;
    using System.Globalization;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for ToggleButtonShowOnly10Items.xaml
    /// </summary>
    public partial class ToggleButtonShowOnly10Items : ToggleButton
    {
        public ToggleButtonShowOnly10Items()
        {
            this.InitializeComponent();
        }
    }

    [ValueConversion(typeof(int), typeof(bool))]
    public class ItemsCountGreaterThan11ToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int count);
            return count > 11;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
