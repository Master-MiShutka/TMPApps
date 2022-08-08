using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TMP.Wpf.CommonControls.Behaviors
{
    public static class ReloadBehavior
    {
        public static DependencyProperty OnDataContextChangedProperty =
            DependencyProperty.RegisterAttached("OnDataContextChanged", typeof(bool), typeof(ReloadBehavior), new PropertyMetadata(OnDataContextChanged));

        public static bool GetOnDataContextChanged(TMPContentControl element)
        {
            return (bool)element.GetValue(OnDataContextChangedProperty);
        }

        public static void SetOnDataContextChanged(TMPContentControl element, bool value)
        {
            element.SetValue(OnDataContextChangedProperty, value);
        }

        private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TMPContentControl)d).DataContextChanged += ReloadDataContextChanged;
        }

        private static void ReloadDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((TMPContentControl)sender).Reload();
        }

        public static DependencyProperty OnSelectedTabChangedProperty =
            DependencyProperty.RegisterAttached("OnSelectedTabChanged", typeof(bool), typeof(ReloadBehavior), new PropertyMetadata(OnSelectedTabChanged));

        public static bool GetOnSelectedTabChanged(ContentControl element)
        {
            return (bool)element.GetValue(OnDataContextChangedProperty);
        }

        public static void SetOnSelectedTabChanged(ContentControl element, bool value)
        {
            element.SetValue(OnDataContextChangedProperty, value);
        }

        private static void OnSelectedTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContentControl)d).Loaded += ReloadLoaded;
        }

        private static void ReloadLoaded(object sender, RoutedEventArgs e)
        {
            var metroContentControl = ((ContentControl)sender);
            var tab = Ancestors(metroContentControl)
                .OfType<TabControl>()
                .FirstOrDefault();

            if (tab == null) return;

            SetTMPContentControl(tab, metroContentControl);
            tab.SelectionChanged -= ReloadSelectionChanged;
            tab.SelectionChanged += ReloadSelectionChanged;
        }

        private static IEnumerable<DependencyObject> Ancestors(DependencyObject obj)
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                yield return parent;
                obj = parent;
                parent = VisualTreeHelper.GetParent(obj);
            }
        }

        private static void ReloadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != sender)
                return;

            var contentControl = GetTMPContentControl((TabControl)sender);
            var metroContentControl = contentControl as TMPContentControl;
            if (metroContentControl != null)
            {
                metroContentControl.Reload();
            }
        }

        public static readonly DependencyProperty TMPContentControlProperty =
            DependencyProperty.RegisterAttached("TMPContentControl", typeof(ContentControl), typeof(ReloadBehavior), new PropertyMetadata(default(ContentControl)));

        public static void SetTMPContentControl(UIElement element, ContentControl value)
        {
            element.SetValue(TMPContentControlProperty, value);
        }

        public static ContentControl GetTMPContentControl(UIElement element)
        {
            return (ContentControl)element.GetValue(TMPContentControlProperty);
        }
    }
}