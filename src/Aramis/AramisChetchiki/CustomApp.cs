namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class CustomApp : Application
    {
        public CustomApp()
        {
            this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"pack://application:,,,/DataGridWpf;component/themes/generic.xaml", UriKind.RelativeOrAbsolute) });

            // Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"pack://application:,,,/ui.wpf;component/themes/generic.xaml", UriKind.RelativeOrAbsolute) });
            // Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"Themes/Generic.xaml", UriKind.RelativeOrAbsolute) });
            this.EnableTheme("/PresentationFramework.Royale;component/themes/royale.normalcolor.xaml");

            Window window = new Window() { Title = "Test" };

            DataGridWpf.FilterDataGrid grid = new();
            grid.AutoGenerateColumns = true;

            grid.ItemsSource = new object[]
            {
                new Model.Address("Minsk", "Unknown", "5g", "4/2", "Belarus"),
                new Model.Address("Grodno", "Long", "89", "1-2", "Belarus"),
                new Model.Address("Hamburg", "Short", "3", "8", "Germany"),
                new Model.Address("Hamburg", "Short", "3", "6", "Belarus"),
                new Model.Address("Hamburg", "Short", "3", "4", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "1", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "3", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "5", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "7", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "9", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "10", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "11", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "12", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "13", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "14", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "15", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "16", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "17", "Russia"),
                new Model.Address("Hamburg", "Short", "3", "2", "Latvia"),
            };

            grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            grid.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            Button btn = new() { Content = "Show message" };
            btn.SetValue(DockPanel.DockProperty, Dock.Bottom);
            btn.Click += (o, e) => { MessageBox.Show(window, DateTime.Now.ToLongDateString()); };

            DockPanel dockPanel = new();
            dockPanel.LastChildFill = true;
            dockPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            dockPanel.VerticalAlignment = VerticalAlignment.Stretch;
            dockPanel.Margin = new Thickness(5);
            dockPanel.Children.Add(btn);
            dockPanel.Children.Add(grid);

            window.Content = dockPanel;

            this.MainWindow = window;

            // MessageBox.Show("Hi! Test.", "IO ghgh g");
        }

        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            CustomApp app = new();

            // MessageBox.Show(app.MainWindow, "Reaady", "IOIK");
            app.Run(app.MainWindow);

            MessageBox.Show("The end", "IOIK");
        }

        private void EnableTheme(string themePath)
        {
            List<Uri> dictionaries = null;
            if (Application.Current.Resources.MergedDictionaries != null)
            {
                dictionaries = Application.Current.Resources.MergedDictionaries
                    .Where(d => d.Source.OriginalString.StartsWith(@"/PresentationFramework", StringComparison.InvariantCulture) == false)
                    .Select(d => d.Source)
                    .ToList();

                // очищаем перед загрузкой темы
                this.Resources.MergedDictionaries.Clear();
            }

            // загружаем необходимые для работы ресурсы
            // загружаем тему
            try
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themePath, UriKind.RelativeOrAbsolute) });
                if (dictionaries != null)
                {
                    dictionaries.ForEach((uri) => Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri }));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось применить визуальную тему.");
            }
        }
    }
}
