﻿namespace TMP.WORK.AramisChetchiki.Controls.SettingsPages
{
    using System.Collections;

    /// <summary>
    /// Interaction logic for SummaryInfosSettings.xaml
    /// </summary>
    public partial class SummaryInfosSettings : SettingsPage
    {
        public SummaryInfosSettings()
        {
            this.InitializeComponent();
        }

        private void MoveUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void MoveDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.lstBox.SelectedIndex > 0)
            {
                int index = this.lstBox.SelectedIndex;
                IList collection = this.lstBox.ItemsSource as IList;
                collection.RemoveAt(index);
            }
        }
    }
}
