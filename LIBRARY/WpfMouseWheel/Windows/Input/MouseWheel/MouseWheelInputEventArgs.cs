namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using WpfMouseWheel.Windows.MotionFlow;

    public class MouseWheelInputEventArgs : MouseWheelEventArgs
    {
        public MouseWheelInputEventArgs(IMouseWheelController controller, MouseWheel wheel, int timestamp, int delta, Orientation orientation)
          : base(wheel.MouseDevice, timestamp, delta)
        {
            this.Controller = controller;
            this.Wheel = wheel;
            this.Orientation = orientation;
        }

        public MouseWheel Wheel
        {
            get;
        }

        public Orientation Orientation
        {
            get;
        }

        public IMouseWheelController Controller
        {
            get;
            set;
        }

        public void RaiseNativeEvent(int nativeDelta)
        {
            this.Controller.InputElement.RaiseEvent(this.NativeDeltaToNativeEventArgs(nativeDelta));
        }

        public void EndCommand()
        {
            if (this.Handled)
                this.Controller.ExitElement.RaiseEvent(this.CreateNativeEventArgs(this.Timestamp, this.Delta));
            else
                this.Handled = this.Wheel.Transfer(MouseWheelNativeMotionTarget.Current, this);
        }

        public override string ToString()
        {
            return $"{this.Orientation}, Delta = {this.Delta}, Timestamp = {this.Timestamp}";
        }

        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            var handler = (MouseWheelInputEventHandler)genericHandler;
            handler(genericTarget, this);
        }

        private MouseWheelEventArgs CreateNativeEventArgs(int timestamp, int delta)
        {
            return new MouseWheelEventArgs(this.Wheel.MouseDevice, timestamp, delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Handled = this.Handled
            };
        }

        private IEnumerable<MouseWheelEventArgs> NormalizedDeltaToNativeEventArgs(int normalizedDelta)
        {
            if (normalizedDelta == 0)
                yield break;
            int clickDisplacement = this.Wheel.NormalizedToNative(Math.Sign(normalizedDelta));
            for (int i = 0; i < Math.Abs(normalizedDelta); ++i)
                yield return this.CreateNativeEventArgs(this.Timestamp + (i * 10), clickDisplacement);
        }

        private MouseWheelEventArgs NativeDeltaToNativeEventArgs(int nativeDelta)
        {
            return nativeDelta == 0 ? null : this.CreateNativeEventArgs(this.Timestamp, nativeDelta);
        }

        private class MouseWheelNativeMotionTarget : INativeMotionTarget
        {

            public static INativeMotionTarget Current => _current;

            public bool CanMove(IMotionInfo info, object context)
            {
                return true;
            }

            public int Coerce(IMotionInfo info, object context, int nativeDelta)
            {
                return nativeDelta;
            }

            public void Move(IMotionInfo info, object context, int nativeDelta)
            {
                var e = context as MouseWheelInputEventArgs;
                var args = e.NativeDeltaToNativeEventArgs(nativeDelta);
                if (args != null)
                    e.Controller.ExitElement.RaiseEvent(args);
            }

            private static readonly INativeMotionTarget _current = new MouseWheelNativeMotionTarget();
        }
    }

    public delegate void MouseWheelInputEventHandler(object sender, MouseWheelInputEventArgs e);
}
