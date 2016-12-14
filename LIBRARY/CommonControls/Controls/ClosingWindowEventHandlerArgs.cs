using System;

namespace TMP.Wpf.CommonControls
{
    public class ClosingWindowEventHandlerArgs : EventArgs
    {
        public bool Cancelled { get; set; }
    }
}