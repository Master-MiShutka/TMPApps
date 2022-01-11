using System.Windows;
using System.Windows.Controls;

namespace TMP.Wpf.CommonControls
{
    /// <summary>
    /// Originally from http://xamlcoder.com/blog/2010/11/04/creating-a-TMP-ui-style-control/
    /// </summary>
    public class TMPContentControl : ContentControl
    {
        public static readonly DependencyProperty ReverseTransitionProperty = DependencyProperty.Register("ReverseTransition", typeof(bool), typeof(TMPContentControl), new FrameworkPropertyMetadata(false));

        public bool ReverseTransition
        {
            get => (bool)GetValue(ReverseTransitionProperty);
            set => SetValue(ReverseTransitionProperty, value);
        }

        public static readonly DependencyProperty TransitionsEnabledProperty = DependencyProperty.Register("TransitionsEnabled", typeof(bool), typeof(TMPContentControl), new FrameworkPropertyMetadata(true));

        public bool TransitionsEnabled
        {
            get => (bool)GetValue(TransitionsEnabledProperty);
            set => SetValue(TransitionsEnabledProperty, value);
        }

        public static readonly DependencyProperty OnlyLoadTransitionProperty = DependencyProperty.Register("OnlyLoadTransition", typeof(bool), typeof(TMPContentControl), new FrameworkPropertyMetadata(false));

        public bool OnlyLoadTransition
        {
            get => (bool)GetValue(OnlyLoadTransitionProperty);
            set => SetValue(OnlyLoadTransitionProperty, value);
        }

        private bool transitionLoaded;

        public TMPContentControl()
        {
            DefaultStyleKey = typeof(TMPContentControl);

            Loaded += TMPContentControlLoaded;
            Unloaded += TMPContentControlUnloaded;
        }

        private void TMPContentControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (TransitionsEnabled && !transitionLoaded)
            {
                if (!IsVisible)
                    VisualStateManager.GoToState(this, ReverseTransition ? "AfterUnLoadedReverse" : "AfterUnLoaded", false);
                else
                    VisualStateManager.GoToState(this, ReverseTransition ? "AfterLoadedReverse" : "AfterLoaded", true);
            }
        }

        private void TMPContentControlUnloaded(object sender, RoutedEventArgs e)
        {
            if (TransitionsEnabled)
            {
                if (transitionLoaded)
                    VisualStateManager.GoToState(this, ReverseTransition ? "AfterUnLoadedReverse" : "AfterUnLoaded", false);
                IsVisibleChanged -= TMPContentControlIsVisibleChanged;
            }
        }

        private void TMPContentControlLoaded(object sender, RoutedEventArgs e)
        {
            if (TransitionsEnabled)
            {
                if (!transitionLoaded)
                {
                    transitionLoaded = this.OnlyLoadTransition;
                    VisualStateManager.GoToState(this, ReverseTransition ? "AfterLoadedReverse" : "AfterLoaded", true);
                }
                IsVisibleChanged -= TMPContentControlIsVisibleChanged;
                IsVisibleChanged += TMPContentControlIsVisibleChanged;
            }
            else
            {
                var root = ((Grid)GetTemplateChild("root"));
                root.Opacity = 1.0;
                var transform = ((System.Windows.Media.TranslateTransform)root.RenderTransform);
                if (transform.IsFrozen)
                {
                    var modifiedTransform = transform.Clone();
                    modifiedTransform.X = 0;
                    root.RenderTransform = modifiedTransform;
                }
                else
                {
                    transform.X = 0;
                }
            }
        }

        public void Reload()
        {
            if (!TransitionsEnabled || transitionLoaded) return;

            if (ReverseTransition)
            {
                VisualStateManager.GoToState(this, "BeforeLoaded", true);
                VisualStateManager.GoToState(this, "AfterUnLoadedReverse", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "BeforeLoaded", true);
                VisualStateManager.GoToState(this, "AfterLoaded", true);
            }
        }
    }
}