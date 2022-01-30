namespace TMP.WORK.AramisChetchiki.Charts
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Selects a colour purely based on its location within a collection.
    /// </summary>
    public class IndexedColourSelector : DependencyObject, IColorSelector
    {
        /// <summary>
        /// An array of brushes.
        /// </summary>
        public ObservableCollection<Brush> Brushes
        {
            get => (ObservableCollection<Brush>)this.GetValue(BrushesProperty);
            set => this.SetValue(BrushesProperty, value);
        }

        public static readonly DependencyProperty BrushesProperty =
                       DependencyProperty.Register("BrushesProperty", typeof(ObservableCollection<Brush>), typeof(IndexedColourSelector),
                           new PropertyMetadata(new ObservableCollection<Brush>()));

        public Brush SelectBrush(object item, int index)
        {
            if (this.Brushes == null || this.Brushes.Count == 0)
            {
                Brush result = System.Windows.Media.Brushes.LightGray;

                Random rnd = new();

                Type brushesType = typeof(Brushes);

                PropertyInfo[] properties = brushesType.GetProperties();

                int random = rnd.Next(properties.Length);
                result = (Brush)properties[random].GetValue(null, null);

                return result;
            }

            return this.Brushes[index % this.Brushes.Count];
        }
    }
}
