// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using System;

    /// <summary>
    /// Defines event data associated with a HyperlinkClick event.
    /// </summary>
    public class TaskDialogHyperlinkClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of this class with the specified link text.
        /// </summary>
        /// <param name="linkText">The text of the hyperlink that was clicked.</param>
        public TaskDialogHyperlinkClickedEventArgs(string linkText)
        {
            this.LinkText = linkText;
        }

        /// <summary>
        /// Gets or sets the text of the hyperlink that was clicked.
        /// </summary>
        public string LinkText { get; set; }
    }
}
