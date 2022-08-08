using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Wpf.Common.Controls
{
    using System.Windows;
    public class NumericUpDownChangedRoutedEventArgs : RoutedEventArgs
    {
        public double Interval { get; set; }

        public NumericUpDownChangedRoutedEventArgs(RoutedEvent routedEvent, double interval) : base(routedEvent)
        {
            Interval = interval;
        }
    }
}
