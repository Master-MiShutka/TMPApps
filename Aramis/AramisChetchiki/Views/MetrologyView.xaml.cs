﻿namespace TMP.WORK.AramisChetchiki.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MetrologyView.xaml
    /// </summary>
    public partial class MetrologyView : UserControl
    {
        public MetrologyView()
        {
            this.InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollviewer)
            {
                if (e.Delta > 0)
                {
                    scrollviewer.PageLeft();
                }
                else
                {
                    scrollviewer.PageRight();
                }

                e.Handled = true;
            }
        }

        private void ScrollViewer_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

        }
    }
}
