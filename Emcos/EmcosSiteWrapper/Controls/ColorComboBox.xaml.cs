using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.Work.Emcos.Controls
{
    /// <summary>
    /// Interaction logic for ColorComboBox.xaml
    /// </summary>
    public partial class ColorComboBox : ComboBox
    {
        public ColorComboBox()
        {
            InitializeComponent();

            var list = new List<System.Windows.Media.Brush>();
            foreach (var item in typeof(System.Windows.Media.Brushes).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                if (item.PropertyType == typeof(System.Windows.Media.SolidColorBrush))
                    list.Add((System.Windows.Media.Brush)item.GetValue(null, null));
            ItemsSource = list;
        }
        private void cmbMarkColorGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;

            const int COLUMNS = 5;

            if (grid != null)
            {
                if (grid.RowDefinitions != null && grid.RowDefinitions.Count == 0)
                {
                    for (int r = 0; r < grid.Children.Count / COLUMNS; r++)
                        grid.RowDefinitions.Add(new RowDefinition());
                }
                if (grid.ColumnDefinitions != null && grid.ColumnDefinitions.Count == 0)
                {
                    for (int c = 0; c < Math.Min(grid.Children.Count, COLUMNS); c++)
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                for (int i = 0; i < grid.Children.Count; i++)
                {
                    Grid.SetRow(grid.Children[i], i / COLUMNS);
                    Grid.SetColumn(grid.Children[i], i % COLUMNS);
                }
            }
        }
    }
}
