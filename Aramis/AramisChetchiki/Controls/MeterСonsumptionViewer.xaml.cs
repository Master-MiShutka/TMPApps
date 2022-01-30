namespace TMP.WORK.AramisChetchiki.Controls
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

        public IEnumerable<Model.MeterEvent> MeterEvents
        {
            get => (IEnumerable<Model.MeterEvent>)this.GetValue(MeterEventsProperty);
            set => this.SetValue(MeterEventsProperty, value);
        }

        public static readonly DependencyProperty MeterEventsProperty =
            DependencyProperty.Register(nameof(MeterEvents), typeof(IEnumerable<Model.MeterEvent>), typeof(MeterСonsumptionViewer), new PropertyMetadata(null, OnMeterEventsPropertyChanged));

        private static void OnMeterEventsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MeterСonsumptionViewer viewer = (MeterСonsumptionViewer)d;

            IEnumerable<Model.MeterEvent> meterEvents = (IEnumerable<Model.MeterEvent>)e.NewValue;

            viewer.Dates = new StringCollection();

            if (meterEvents == null || meterEvents.Any() == false)
            {
                return;
            }

            uint max = meterEvents.Max(i => i.Сonsumption);
            ItemHeightValueConverter.MaxValue = max;

            viewer.Dates = new StringCollection();
            viewer.Dates.AddRange(meterEvents.Select(i => i.Date.ToString("MM-yyyy")).ToArray());
        }

        public StringCollection Dates
        {
            get => (StringCollection)this.GetValue(DatesProperty);
            set => this.SetValue(DatesProperty, value);
        }

        public static readonly DependencyProperty DatesProperty =
            DependencyProperty.Register(nameof(Dates), typeof(StringCollection), typeof(MeterСonsumptionViewer), new PropertyMetadata(null));
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
