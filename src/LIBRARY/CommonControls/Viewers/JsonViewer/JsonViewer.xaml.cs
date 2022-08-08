using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading;

namespace TMP.Wpf.CommonControls.Viewers
{
    /// <summary>
    /// Логика взаимодействия для JsonViewer.xaml
    /// </summary>
    public partial class JsonViewer : UserControl, IStringBasedViewer
    {
        private const GeneratorStatus Generated = GeneratorStatus.ContainersGenerated;
        private DispatcherTimer _timer;

        public JsonViewer()
        {
            InitializeComponent();
            JsonTreeView.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(TreeViewItemExpanded));
        }

        private void TreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            var prevCursor = Cursor;
            // we will only go through with this if our children haven't been populated
            TreeViewItem sourceItem = e.OriginalSource as TreeViewItem;
            if ((sourceItem != null)
                && (sourceItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated))
            {
                // create a handler that will check our children and reset the cursor when the ItemContainerGenerator has finished
                EventHandler itemsGenerated = null;
                DateTime before = DateTime.Now;
                itemsGenerated = delegate (object o, EventArgs args)
                {
                    // if the children are done being generated...
                    if ((o as ItemContainerGenerator).Status == GeneratorStatus.ContainersGenerated)
                    {
                        (o as ItemContainerGenerator).StatusChanged -= itemsGenerated;  // we're done, so remove the handler
                        sourceItem.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, (ThreadStart)delegate    // asynchronous reset of cursor
                        {
                            Cursor = prevCursor;
                            System.Diagnostics.Debug.WriteLine("Expanded in " + (DateTime.Now - before));
                        });
                    }
                };
                sourceItem.ItemContainerGenerator.StatusChanged += itemsGenerated;  // add the handler
                Cursor = Cursors.Wait;     // wait cursor
            }
            e.Handled = true;
        }

        public void Load(string json)
        {
            if (String.IsNullOrEmpty(json)) return;
            JsonTreeView.ItemsSource = null;
            JsonTreeView.Items.Clear();

            var children = new List<JToken>();

            try
            {
                var token = JToken.Parse(json);

                if (token != null)
                {
                    children.Add(token);
                }

                JsonTreeView.ItemsSource = children;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the JSON string:\r\n" + ex.Message);
            }
        }

        private void JValue_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            var tb = sender as TextBlock;
            if (tb != null)
            {
                Clipboard.SetText(tb.Text);
            }
        }

        private void ExpandAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(true);
        }

        private void CollapseAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(false);
        }

        private void ToggleItems(bool isExpanded)
        {
            if (JsonTreeView.Items.IsEmpty)
                return;

            //var prevCursor = Cursor;
            //System.Windows.Controls.DockPanel.Opacity = 0.2;
            //System.Windows.Controls.DockPanel.IsEnabled = false;
            Cursor = Cursors.Wait;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, delegate
            {
                ToggleItems(JsonTreeView, JsonTreeView.Items, isExpanded);
                //System.Windows.Controls.DockPanel.Opacity = 1.0;
                //System.Windows.Controls.DockPanel.IsEnabled = true;
                _timer.Stop();
                Cursor = Cursors.Arrow;
            }, Application.Current.Dispatcher);
            _timer.Start();
        }

        private void ToggleItems(ItemsControl parentContainer, ItemCollection items, bool isExpanded)
        {
            var itemGen = parentContainer.ItemContainerGenerator;
            if (itemGen.Status == Generated)
            {
                Recurse(items, isExpanded, itemGen);
            }
            else
            {
                itemGen.StatusChanged += delegate
                {
                    Recurse(items, isExpanded, itemGen);
                };
            }
        }

        private void Recurse(ItemCollection items, bool isExpanded, ItemContainerGenerator itemGen)
        {
            if (itemGen.Status != Generated)
                return;

            foreach (var item in items)
            {
                var tvi = itemGen.ContainerFromItem(item) as TreeViewItem;
                tvi.IsExpanded = isExpanded;
                ToggleItems(tvi, tvi.Items, isExpanded);
            }
        }

        public void SaveContent()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.DefaultExt = ".json";
                sfd.Filter = "Файлы JSON (*.json)|*.json";
                sfd.FilterIndex = 0;
                sfd.Title = "Сохранение данных";
                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();
                    List<JToken> list = JsonTreeView.ItemsSource as List<JToken>;
                    foreach (var i in list)
                        sb.Append(i.ToString());

                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                }
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show("Произошла ошибка при сохранении.\n" + ex.Message, "Сохранение", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveContent(object sender, RoutedEventArgs e)
        {
            SaveContent();
        }
    }
}
