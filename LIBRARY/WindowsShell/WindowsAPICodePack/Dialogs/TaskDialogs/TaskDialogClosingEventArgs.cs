// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using System.ComponentModel;

    /// <summary>
    /// Data associated with <see cref="TaskDialog.Closing"/> event.
    /// </summary>
    public class TaskDialogClosingEventArgs : CancelEventArgs
    {
        private TaskDialogResult taskDialogResult;

        /// <summary>
        /// Gets or sets the standard button that was clicked.
        /// </summary>
        public TaskDialogResult TaskDialogResult
        {
            get => this.taskDialogResult;
            set => this.taskDialogResult = value;
        }

        private string customButton;

        /// <summary>
        /// Gets or sets the text of the custom button that was clicked.
        /// </summary>
        public string CustomButton
        {
            get => this.customButton;
            set => this.customButton = value;
        }
    }
}
