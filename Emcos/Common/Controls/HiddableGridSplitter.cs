using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TMP.Wpf.Common.Controls
{
    /// <summary>
    /// Grid splitter that show or hides the following row when the visibility of the splitter is changed. 
    /// </summary>
    public class HiddableGridSplitter : GridSplitter
    {
        GridLength height;

        public HiddableGridSplitter()
        {
            this.IsVisibleChanged += HideableGridSplitter_IsVisibleChanged;
            this.Initialized += HideableGridSplitter_Initialized;
        }

        void HideableGridSplitter_Initialized(object sender, EventArgs e)
        {
            //Cache the initial RowDefinition height,
            //so it is not always assumed to be "Auto"
            Grid parent = base.Parent as Grid;
            if (parent == null) return;
            int rowIndex = Grid.GetRow(this);
            if (rowIndex + 1 >= parent.RowDefinitions.Count) return;
            var lastRow = parent.RowDefinitions[rowIndex + 1];
            height = lastRow.Height;
        }

        void HideableGridSplitter_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Grid parent = base.Parent as Grid;
            if (parent == null) return;

            int rowIndex = Grid.GetRow(this);

            if (rowIndex + 1 >= parent.RowDefinitions.Count) return;

            var lastRow = parent.RowDefinitions[rowIndex + 1];

            if (this.Visibility == Visibility.Visible)
            {
                lastRow.Height = height;
            }
            else
            {
                height = lastRow.Height;
                lastRow.Height = new GridLength(0);
            }

        }
    }
}
