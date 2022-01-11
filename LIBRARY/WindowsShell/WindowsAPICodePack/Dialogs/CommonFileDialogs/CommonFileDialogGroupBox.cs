// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows.Markup;

    /// <summary>
    /// Represents a group box control for the Common File Dialog.
    /// </summary>note
    [ContentProperty("Items")]
    public class CommonFileDialogGroupBox : CommonFileDialogProminentControl
    {
        private Collection<DialogControl> items;

        /// <summary>
        /// Gets the collection of controls for this group box.
        /// </summary>
        public Collection<DialogControl> Items => this.items;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public CommonFileDialogGroupBox()
            : base(string.Empty)
        {
            this.Initialize();
        }

        /// <summary>
        /// Create a new instance of this class with the specified text.
        /// </summary>
        /// <param name="text">The text to display for this control.</param>
        public CommonFileDialogGroupBox(string text)
            : base(text)
        {
            this.Initialize();
        }

        /// <summary>
        /// Creates a new instance of this class with the specified name and text.
        /// </summary>
        /// <param name="name">The name of this control.</param>
        /// <param name="text">The text to display for this control.</param>
        public CommonFileDialogGroupBox(string name, string text)
            : base(name, text)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes the item collection for this class.
        /// </summary>
        private void Initialize()
        {
            this.items = new Collection<DialogControl>();
        }

        /// <summary>
        /// Attach the GroupBox control to the dialog object
        /// </summary>
        /// <param name="dialog">Target dialog</param>
        internal override void Attach(IFileDialogCustomize dialog)
        {
            Debug.Assert(dialog != null, "CommonFileDialogGroupBox.Attach: dialog parameter can not be null");

            // Start a visual group
            dialog.StartVisualGroup(this.Id, this.Text);

            // Add child controls
            foreach (CommonFileDialogControl item in this.items)
            {
                item.HostingDialog = this.HostingDialog;
                item.Attach(dialog);
            }

            // End visual group
            dialog.EndVisualGroup();

            // Make this control prominent if needed
            if (this.IsProminent)
            {
                dialog.MakeProminent(this.Id);
            }

            // Sync unmanaged properties with managed properties
            this.SyncUnmanagedProperties();
        }
    }
}
