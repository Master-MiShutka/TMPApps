namespace TMP.WORK.AramisChetchiki.Controls.SettingsPages
{
    using System;
    using System.Windows.Controls;

    public class SettingsPage : ContentControl
    {
        public SettingsPage()
        {
        }

        public string Header { get; set; }

        public Action CloseAction { get; set; }
    }
}
