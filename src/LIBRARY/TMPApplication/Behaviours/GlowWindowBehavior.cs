namespace TMPApplication.Behaviours
{
    using System;
    using System.Windows;
    using System.Windows.Threading;
    using Interactivity;
    using TMPApplication.CustomWpfWindow;

    public class GlowWindowBehavior : Behavior<Window>
    {
        private static readonly TimeSpan GlowTimerDelay = TimeSpan.FromMilliseconds(200); // 200 ms delay, the same as VS2013
        private GlowWindow left;
        private GlowWindow right;
        private GlowWindow top;
        private GlowWindow bottom;
        private DispatcherTimer makeGlowVisibleTimer;
        private bool prevTopmost;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += this.AssociatedObjectOnLoaded;
            this.AssociatedObject.Unloaded += this.AssociatedObjectUnloaded;
            this.AssociatedObject.StateChanged += this.AssociatedObjectStateChanged;
        }

        private void AssociatedObjectStateChanged(object sender, EventArgs e)
        {
            if (this.AssociatedObject.WindowState == WindowState.Minimized)
            {
                this.prevTopmost = this.AssociatedObject.Topmost;
                this.AssociatedObject.Topmost = true;
            }
            else
            {
                this.AssociatedObject.Topmost = this.prevTopmost;
            }

            if (this.makeGlowVisibleTimer != null)
            {
                this.makeGlowVisibleTimer.Stop();
            }

            if (this.AssociatedObject.WindowState != WindowState.Minimized)
            {
                var tmpWindow = this.AssociatedObject as WindowWithDialogs;
                var ignoreTaskBar = tmpWindow != null && tmpWindow.IgnoreTaskbarOnMaximize;
                if (this.makeGlowVisibleTimer != null && SystemParameters.MinimizeAnimation && !ignoreTaskBar)
                {
                    this.makeGlowVisibleTimer.Start();
                }
                else
                {
                    this.RestoreGlow();
                }
            }
            else
            {
                this.HideGlow();
            }
        }

        private void AssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.makeGlowVisibleTimer != null)
            {
                this.makeGlowVisibleTimer.Stop();
                this.makeGlowVisibleTimer.Tick -= this.makeGlowVisibleTimer_Tick;
                this.makeGlowVisibleTimer = null;
            }
        }

        private void makeGlowVisibleTimer_Tick(object sender, EventArgs e)
        {
            if (this.makeGlowVisibleTimer != null)
            {
                this.makeGlowVisibleTimer.Stop();
            }

            this.RestoreGlow();
        }

        private void RestoreGlow()
        {
            if (this.left != null && this.top != null && this.right != null && this.bottom != null)
            {
                this.left.IsGlowing = this.top.IsGlowing = this.right.IsGlowing = this.bottom.IsGlowing = true;
                this.Update();
            }
        }

        private void HideGlow()
        {
            if (this.left != null && this.top != null && this.right != null && this.bottom != null)
            {
                this.left.IsGlowing = this.top.IsGlowing = this.right.IsGlowing = this.bottom.IsGlowing = false;
                this.Update();
            }
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // No glow effect if UseNoneWindowStyle is true or GlowBrush not set.
            var tmpWindow = this.AssociatedObject as WindowWithDialogs;
            if (tmpWindow != null && (tmpWindow.UseNoneWindowStyle || tmpWindow.GlowBrush == null))
            {
                return;
            }

            if (this.makeGlowVisibleTimer == null)
            {
                this.makeGlowVisibleTimer = new DispatcherTimer { Interval = GlowTimerDelay };
                this.makeGlowVisibleTimer.Tick += this.makeGlowVisibleTimer_Tick;
            }

            this.left = new GlowWindow(this.AssociatedObject, GlowDirection.Left);
            this.right = new GlowWindow(this.AssociatedObject, GlowDirection.Right);
            this.top = new GlowWindow(this.AssociatedObject, GlowDirection.Top);
            this.bottom = new GlowWindow(this.AssociatedObject, GlowDirection.Bottom);

            this.Show();
            this.Update();

            var windowTransitionsEnabled = tmpWindow != null;
            if (!windowTransitionsEnabled)
            {
                // no storyboard so set opacity to 1
                this.SetOpacityTo(1);
            }
            else
            {
                // start the opacity storyboard 0->1
                this.StartOpacityStoryboard();

                // hide the glows if window get invisible state
                this.AssociatedObject.IsVisibleChanged += this.AssociatedObjectIsVisibleChanged;

                // closing always handled
                this.AssociatedObject.Closing += (o, args) =>
                {
                    if (!args.Cancel)
                    {
                        this.AssociatedObject.IsVisibleChanged -= this.AssociatedObjectIsVisibleChanged;
                    }
                };
            }
        }

        private void AssociatedObjectIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.AssociatedObject.IsVisible)
            {
                // the associated owner got invisible so set opacity to 0 to start the storyboard by 0 for the next visible state
                this.SetOpacityTo(0);
            }
            else
            {
                this.StartOpacityStoryboard();
            }
        }

        /// <summary>
        /// Updates all glow windows (visible, hidden, collapsed)
        /// </summary>
        private void Update()
        {
            if (this.left != null
                && this.right != null
                && this.top != null
                && this.bottom != null)
            {
                this.left.Update();
                this.right.Update();
                this.top.Update();
                this.bottom.Update();
            }
        }

        /// <summary>
        /// Sets the opacity to all glow windows
        /// </summary>
        private void SetOpacityTo(double newOpacity)
        {
            if (this.left != null
                && this.right != null
                && this.top != null
                && this.bottom != null)
            {
                this.left.Opacity = newOpacity;
                this.right.Opacity = newOpacity;
                this.top.Opacity = newOpacity;
                this.bottom.Opacity = newOpacity;
            }
        }

        /// <summary>
        /// Starts the opacity storyboard 0 -> 1
        /// </summary>
        private void StartOpacityStoryboard()
        {
            if (this.left != null && this.left.OpacityStoryboard != null
                && this.right != null && this.right.OpacityStoryboard != null
                && this.top != null && this.top.OpacityStoryboard != null
                && this.bottom != null && this.bottom.OpacityStoryboard != null)
            {
                this.left.BeginStoryboard(this.left.OpacityStoryboard);
                this.right.BeginStoryboard(this.right.OpacityStoryboard);
                this.top.BeginStoryboard(this.top.OpacityStoryboard);
                this.bottom.BeginStoryboard(this.bottom.OpacityStoryboard);
            }
        }

        /// <summary>
        /// Shows all glow windows
        /// </summary>
        private void Show()
        {
            this.left.Show();
            this.right.Show();
            this.top.Show();
            this.bottom.Show();
        }
    }
}
