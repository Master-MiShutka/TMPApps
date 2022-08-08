namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;
    using System.Windows;

    public delegate void MotionTransferEventHandler(object sender, MotionTransferEventArgs e);

    public class MotionTransferEventArgs : RoutedEventArgs
    {
        public MotionTransferEventArgs(IMotionInfo info, double delta)
        {
            this.RoutedEvent = MotionTransfer.TransferedEvent;
            this.MotionInfo = info;
            this.Delta = delta;
        }

        public IMotionInfo MotionInfo
        {
            get; private set;
        }

        public double Delta
        {
            get; private set;
        }

        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            var handler = (MotionTransferEventHandler)genericHandler;
            handler(genericTarget, this);
        }
    }

    public delegate void NativeMotionTransferEventHandler(object sender, NativeMotionTransferEventArgs e);

    public class NativeMotionTransferEventArgs : RoutedEventArgs
    {
        public NativeMotionTransferEventArgs(IMotionInfo info, int nativeDelta)
        {
            this.MotionInfo = info;
            this.NativeDelta = nativeDelta;
        }

        public IMotionInfo MotionInfo
        {
            get; private set;
        }

        public int NativeDelta
        {
            get; private set;
        }

        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            var handler = (NativeMotionTransferEventHandler)genericHandler;
            handler(genericTarget, this);
        }
    }
}
