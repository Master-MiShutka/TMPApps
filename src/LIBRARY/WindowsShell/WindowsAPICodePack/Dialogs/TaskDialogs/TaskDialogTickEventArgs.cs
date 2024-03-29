// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using System;

    /// <summary>
    /// The event data for a TaskDialogTick event.
    /// </summary>
    public class TaskDialogTickEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes the data associated with the TaskDialog tick event.
        /// </summary>
        /// <param name="ticks">The total number of ticks since the control was activated.</param>
        public TaskDialogTickEventArgs(int ticks)
        {
            this.Ticks = ticks;
        }

        /// <summary>
        /// Gets a value that determines the current number of ticks.
        /// </summary>
        public int Ticks { get; private set; }
    }
}
