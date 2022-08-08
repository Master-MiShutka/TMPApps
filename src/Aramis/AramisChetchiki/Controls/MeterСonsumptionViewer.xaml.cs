{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for MeterСonsumptionViewer.xaml
    /// </summary>
    public partial class MeterСonsumptionViewer : UserControl
    {
        public MeterСonsumptionViewer()
        {
            this.InitializeComponent();
        }

        public MeterEventsCollection MeterEvents
        {
            get => (MeterEventsCollection)this.GetValue(MeterEventsProperty);
            set => this.SetValue(MeterEventsProperty, value);
        }

        public static readonly DependencyProperty MeterEventsProperty =
            DependencyProperty.Register(nameof(MeterEvents), typeof(MeterEventsCollection), typeof(MeterСonsumptionViewer), new PropertyMetadata(null, OnMeterEventsChanged));

        private static void OnMeterEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            MeterСonsumptionViewer viewer = (MeterСonsumptionViewer)d;

            MeterEventsCollection meterEvents = new MeterEventsCollection((IEnumerable<MeterEvent>)e.NewValue);

            if (meterEvents == null || meterEvents.Any() == false)
            {
                return;
            }

            uint max = meterEvents.Max(i => i.Сonsumption);
            ItemHeightValueConverter.MaxValue = max;

            var r = meterEvents
                .Select(i => i.Date)
                .GroupBy(i => i.Year, element => element.Month, (key, result) => new { Header = key, Items = result });
            viewer.Dates = r.ToList();
        }

        public System.Collections.IList Dates
        {
            get => (System.Collections.IList)this.GetValue(DatesProperty);
            set => this.SetValue(DatesProperty, value);
        }

        public static readonly DependencyProperty DatesProperty =
            DependencyProperty.Register(nameof(Dates), typeof(System.Collections.IList), typeof(MeterСonsumptionViewer), new PropertyMetadata(null));

        public bool HasData => Dates?.Count > 0;
    }

    public class ItemHeightValueConverter : IMultiValueConverter
    {
        public static uint MaxValue { get; internal set; }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                double value = values[0] == null ? 0.0 : System.Convert.ToDouble(values[0]);
                double parentHeight = (values[1] == null || values[1] == DependencyProperty.UnsetValue) ? 0.0 : System.Convert.ToDouble(values[1]);

                if (parentHeight > SystemParameters.PrimaryScreenHeight)
                    return 0d;

                double result = (value / (MaxValue == 0 ? 1 : MaxValue)) * (parentHeight - 7);

                if (result < 0)
                {
                    System.Diagnostics.Debugger.Break();
                    return 0d;
                }

                return result;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
