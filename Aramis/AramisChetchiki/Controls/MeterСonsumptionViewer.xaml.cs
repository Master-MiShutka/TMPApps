using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.WORK.AramisChetchiki.Controls
{
    /// <summary>
    /// Interaction logic for MeterСonsumptionViewer.xaml
    /// </summary>
    public partial class MeterСonsumptionViewer : UserControl
    {
        public MeterСonsumptionViewer()
        {
            InitializeComponent();
        }

        public IEnumerable<Model.MeterEvent> MeterEvents
        {
            get { return (IEnumerable<Model.MeterEvent>)GetValue(MeterEventsProperty); }
            set { SetValue(MeterEventsProperty, value); }
        }

        public static readonly DependencyProperty MeterEventsProperty =
            DependencyProperty.Register(nameof(MeterEvents), typeof(IEnumerable<Model.MeterEvent>), typeof(MeterСonsumptionViewer), new PropertyMetadata(null, OnMeterEventsPropertyChanged));

        private static void OnMeterEventsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MeterСonsumptionViewer viewer = (MeterСonsumptionViewer)d;

            IEnumerable<Model.MeterEvent> meterEvents = (IEnumerable<Model.MeterEvent>)e.NewValue;

            int max = meterEvents.Max(i => i.Сonsumption);
            ItemHeightValueConverter.MaxValue = max;

            viewer.Dates = new StringCollection();
            viewer.Dates.AddRange(meterEvents.Select(i => i.Date.ToString("mm-yyyy")).ToArray());
        }

        public StringCollection Dates
        {
            get { return (StringCollection)GetValue(DatesProperty); }
            set { SetValue(DatesProperty, value); }
        }

        public static readonly DependencyProperty DatesProperty =
            DependencyProperty.Register(nameof(Dates), typeof(StringCollection), typeof(MeterСonsumptionViewer), new PropertyMetadata(null));
    }

    public class ItemHeightValueConverter : IMultiValueConverter
    {
        public static int MaxValue { get; internal set; }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var d = new Nullable<double>((double)values[0]);
            double value = ((d != null && Double.IsNaN(d.Value) == false) ? d.Value : 0.0);
            double parentHeight = (values[1] != DependencyProperty.UnsetValue ? (double)values[1] : 0.0);

            return (value / MaxValue == 0 ? 1 : MaxValue) * (parentHeight - 7);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
