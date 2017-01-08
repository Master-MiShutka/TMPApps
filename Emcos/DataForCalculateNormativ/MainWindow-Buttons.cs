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
using System.Diagnostics;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public partial class MainWindow : Window
    {
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = this;
            settings.Show();
        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAvailability() == false)
                return;

            var list = HierarchicalListToFlatList(_pointsList.ToList());

            var substations = list
                .Where(i => (i.TypeCode == "SUBSTATION" || i.TypeCode == "VOLTAGE") && i.Checked)
                .OrderBy(i => i.Name).ToList<ListPoint>();
            if (substations == null || substations.Count == 0)
            {
                App.ShowWarning(Strings.EmptyList);
                return;
            }

            btnPanel.IsEnabled = false;
            status.Text = Strings.GettingDataStatus;
            try
            {
                App.ShowInfo(Strings.OnGettingData);

                Action updateUI = () =>
                {
                    btnPanel.IsEnabled = true;
                    HideProgress();
                };
                Action completed = () =>
                {
                    updateUI();
                    status.Text = Strings.ReadyMessage;                    
                };
                Action canceled = () =>
                {
                    updateUI();
                    status.Text = Strings.ReadyMessage;
                    App.ShowInfo(Strings.Canceled);
                };

                GetDataControl get = new GetDataControl(substations, completed, canceled);
                dialogHost.Content = get;
            }
            catch (Exception ex)
            {
                btnPanel.IsEnabled = true;
                HideProgress();
                status.Text = Strings.ReadyMessage;
                App.ShowError(String.Format(Strings.Error, App.GetExceptionDetails(ex)));
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAvailability() == false)
                return;
            try
            {
                if (App.ShowInfo(Strings.OnUpdatingPointsList) == MessageBoxResult.OK)
                {
                    UpdatePoints();
                }
            }
            catch (Exception ex)
            {
                HideProgress();
                status.Text = Strings.ReadyMessage;
                App.ShowError(String.Format(Strings.Error, App.GetExceptionDetails(ex)));
            }
        }

        private void btnGetAuxiliaries_Click(object sender, RoutedEventArgs e)
        {
            ;
        }

        private void TreeSelectAll(object sender, RoutedEventArgs e)
        {
            if (_pointsList != null)
            {
                foreach (var item in _pointsList)
                    ForEachPointInTree(item, p => p.TypeCode == "SUBSTATION" || p.TypeCode == "VOLTAGE", p => p.Checked = true);
            }
        }

        private void TreeUnselectAll(object sender, RoutedEventArgs e)
        {
            if (_pointsList != null)
            {
                foreach (var item in _pointsList)
                    ForEachPointInTree(item, p => true, p => p.Checked = false);
            }
        }
        private void TreeCheckItems(object sender, RoutedEventArgs e)
        {
            if (tree.SelectedItem != null)
            {
                ListPoint point = (ListPoint)tree.SelectedItem;
                if (point != null && point.Items != null)
                {
                    foreach (var item in point.Items)
                        if (item.TypeCode == "FES" || item.TypeCode == "RES" || item.TypeCode == "SUBSTATION" || item.TypeCode == "VOLTAGE")
                            item.Checked = !item.Checked;
                }
            }
        }        

        private void ForEachPointInTree(ListPoint point, Func<ListPoint, bool> condition, Action<ListPoint> action)
        {
            if (condition(point))
                action(point);
            if (point.Items != null && point.Items.Count > 0)
                foreach (var item in point.Items)
                    ForEachPointInTree(item, condition, action);
        }
    }
}