using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Windows.Controls;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TMP.WORK.AramisChetchiki.Charts
{
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
            get { return (ObservableCollection<Brush>)GetValue(BrushesProperty); }
            set { SetValue(BrushesProperty, value); }
        }

        public static readonly DependencyProperty BrushesProperty =
                       DependencyProperty.Register("BrushesProperty", typeof(ObservableCollection<Brush>), typeof(IndexedColourSelector), 
                           new PropertyMetadata(new ObservableCollection<Brush>()));


        public Brush SelectBrush(object item, int index)
        {
            if (Brushes == null || Brushes.Count == 0)
            {
                Brush result = System.Windows.Media.Brushes.LightGray;

                Random rnd = new Random();

                Type brushesType = typeof(Brushes);

                PropertyInfo[] properties = brushesType.GetProperties();

                int random = rnd.Next(properties.Length);
                result = (Brush)properties[random].GetValue(null, null);

                return result;

            }
            return Brushes[index % Brushes.Count];
        }
    }
}
