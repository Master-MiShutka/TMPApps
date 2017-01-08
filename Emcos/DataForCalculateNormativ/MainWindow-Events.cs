using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public partial class MainWindow : Window
    {
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var list = LoadList();
            if (list != null)
            {
                _pointsList = new ObservableCollection<ListPoint>(list);
                tree.ItemsSource = _pointsList;
            }
            else
            {
                UpdatePoints();
            }
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_backgroudTask != null)
            {
                if (_backgroudTask.IsCompleted == false)
                {
                    e.Cancel = true;
                    if (cts != null) cts.Cancel();
                    ShowProgress(Strings.TerminatingMessage);
                    await _backgroudTask;
                    this.Close();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_pointsList != null)
                SaveList(_pointsList.ToList());
        }
        /*private void Group_checked(object sender, RoutedEventArgs e)
        {
            var list = HierarchicalListToFlatList(_pointsList.ToList());
            var selected = list
                .Where(i => i.Checked)
                .OrderBy(i => i.Name).ToList<ListPoint>();
            if (selected == null || selected.Count == 0)
            {
                selection.Text = "Ничего не выбрано.";
            }
            else
            {
                selection.Text = String.Format("Выбрано {0} групп.", selected.Count);
            }
        }*/
    }
}