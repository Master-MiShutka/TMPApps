namespace TMPApplication.CustomWpfWindow
{
    using System;

    public class ClosingWindowEventHandlerArgs : EventArgs
    {
        public bool Cancelled { get; set; }
    }
}