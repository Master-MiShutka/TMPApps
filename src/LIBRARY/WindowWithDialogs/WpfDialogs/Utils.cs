using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System.Windows.Controls;

namespace WindowWithDialogs.WpfDialogs
{
    internal static class Utils
    {
        public static HorizontalAlignment FromUIInfrastructure(UIInfrastructure.HorizontalAlignment alignment) => alignment switch
        {
            UIInfrastructure.HorizontalAlignment.Left => HorizontalAlignment.Left,
            UIInfrastructure.HorizontalAlignment.Center => HorizontalAlignment.Center,
            UIInfrastructure.HorizontalAlignment.Right => HorizontalAlignment.Right,
            UIInfrastructure.HorizontalAlignment.Stretch => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Center,
        };

        public static VerticalAlignment FromUIInfrastructure(UIInfrastructure.VerticalAlignment alignment) => alignment switch
        {
            UIInfrastructure.VerticalAlignment.Top => VerticalAlignment.Top,
            UIInfrastructure.VerticalAlignment.Center => VerticalAlignment.Center,
            UIInfrastructure.VerticalAlignment.Bottom => VerticalAlignment.Bottom,
            UIInfrastructure.VerticalAlignment.Stretch => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Center,
        };

        public static UIInfrastructure.HorizontalAlignment ToUIInfrastructure(HorizontalAlignment alignment) => alignment switch
        {
            HorizontalAlignment.Left => UIInfrastructure.HorizontalAlignment.Left,
            HorizontalAlignment.Center => UIInfrastructure.HorizontalAlignment.Center,
            HorizontalAlignment.Right => UIInfrastructure.HorizontalAlignment.Right,
            HorizontalAlignment.Stretch => UIInfrastructure.HorizontalAlignment.Stretch,
            _ => UIInfrastructure.HorizontalAlignment.Center,
        };

        public static UIInfrastructure.VerticalAlignment ToUIInfrastructure(VerticalAlignment alignment) => alignment switch
        {
            VerticalAlignment.Top => UIInfrastructure.VerticalAlignment.Top,
            VerticalAlignment.Center => UIInfrastructure.VerticalAlignment.Center,
            VerticalAlignment.Bottom => UIInfrastructure.VerticalAlignment.Bottom,
            VerticalAlignment.Stretch => UIInfrastructure.VerticalAlignment.Stretch,
            _ => UIInfrastructure.VerticalAlignment.Center,
        };
    }
}
