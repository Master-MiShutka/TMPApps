using System;

namespace TMPApplication.CustomWpfWindow
{
    public class ClosingWindowEventHandlerArgs : EventArgs
    {
        public bool Cancelled { get; set; }
    }
}