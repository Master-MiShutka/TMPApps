namespace TMP.WORK.AramisChetchiki.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;

    public class TiltWheelHorizontalScroller
    {
        public static bool GetEnableTiltWheelScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableTiltWheelScrollProperty);
        }

        public static void SetEnableTiltWheelScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableTiltWheelScrollProperty, value);
        }

        public static readonly DependencyProperty EnableTiltWheelScrollProperty =
                DependencyProperty.RegisterAttached("EnableTiltWheelScroll", typeof(bool), typeof(TiltWheelHorizontalScroller), new UIPropertyMetadata(false, OnEnableTiltWheelScrollChanged));

        private static HashSet<int> controls = new HashSet<int>();

        private static void OnEnableTiltWheelScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is Control control && GetEnableTiltWheelScroll(d) && controls.Add(control.GetHashCode()))
            {
                control.MouseEnter += (sender, e) =>
                {
                    ScrollViewer scrollViewer = FindChildOfType<ScrollViewer>(d);
                    if (scrollViewer != null)
                    {
                        new TiltWheelMouseScrollHelper(scrollViewer, d);
                    }
                };
            }
        }

        /// <summary>
        /// Finds first child of provided type. If child not found, null is returned
        /// </summary>
        /// <typeparam name="T">Type of chiled to be found</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T FindChildOfType<T>(DependencyObject originalSource)
            where T : DependencyObject
        {
            T ret = originalSource as T;
            DependencyObject child = null;
            if (originalSource != null && ret == null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(originalSource); i++)
                {
                    child = System.Windows.Media.VisualTreeHelper.GetChild(originalSource, i);
                    if (child != null)
                    {
                        if (child is T)
                        {
                            ret = child as T;
                            break;
                        }
                        else
                        {
                            ret = FindChildOfType<T>(child);
                            if (ret != null)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return ret;
        }
    }

    internal class TiltWheelMouseScrollHelper
    {
        /// <summary>
        /// multiplier of how far to scroll horizontally. Change as desired.
        /// </summary>
        private const int ScrollFactor = 3;
        private const int WM_MOUSEHWHEEL = 0x20e;
        private const int WM_MOUSEWHEEL = 0x20a;
        private ScrollViewer scrollViewer;
        private HwndSource hwndSource;
        private HwndSourceHook hook;
        private static readonly HashSet<int> scrollViewers = new HashSet<int>();

        public TiltWheelMouseScrollHelper(ScrollViewer scrollViewer, DependencyObject d)
        {
            this.scrollViewer = scrollViewer;
            this.hwndSource = PresentationSource.FromDependencyObject(d) as HwndSource;
            this.hook = this.WindowProc;
            this.hwndSource?.AddHook(this.hook);
            if (scrollViewers.Add(scrollViewer.GetHashCode()))
            {
                scrollViewer.MouseLeave += (sender, e) =>
                {
                    this.hwndSource.RemoveHook(this.hook);
                };
            }
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_MOUSEHWHEEL:
                case WM_MOUSEWHEEL:
                    this.Scroll(wParam);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private void Scroll(IntPtr wParam)
        {
            int delta = (HIWORD(wParam) > 0 ? 1 : -1) * ScrollFactor;
            this.scrollViewer.ScrollToHorizontalOffset(this.scrollViewer.HorizontalOffset + delta);
        }

        private static int HIWORD(IntPtr ptr)
        {
            return (short)((((int)ptr.ToInt64()) >> 16) & 0xFFFF);
        }
    }
}
