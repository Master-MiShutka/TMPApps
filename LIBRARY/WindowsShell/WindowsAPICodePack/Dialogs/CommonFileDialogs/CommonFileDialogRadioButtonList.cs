// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows.Markup;
    using Microsoft.WindowsAPICodePack.Shell.Resources;

    /// <summary>
    /// Represents a radio button list for the Common File Dialog.
    /// </summary>
    [ContentProperty("Items")]
    public class CommonFileDialogRadioButtonList : CommonFileDialogControl, ICommonFileDialogIndexedControls
    {
        private Collection<CommonFileDialogRadioButtonListItem> items = new Collection<CommonFileDialogRadioButtonListItem>();

        /// <summary>
        /// Gets the collection of CommonFileDialogRadioButtonListItem objects
        /// </summary>
        public Collection<CommonFileDialogRadioButtonListItem> Items => this.items;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public CommonFileDialogRadioButtonList()
        {
        }

        /// <summary>
        /// Creates a new instance of this class with the specified name.
        /// </summary>
        /// <param name="name">The name of this control.</param>
        public CommonFileDialogRadioButtonList(string name) : base(name, string.Empty)
        {
        }

        #region ICommonFileDialogIndexedControls Members

        private int selectedIndex = -1;

        /// <summary>
        /// Gets or sets the current index of the selected item.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "ToDo")]
        public int SelectedIndex
        {
            get => this.selectedIndex;

            set
            {
                // Don't update this property if it hasn't changed
                if (this.selectedIndex == value)
                {
                    return;
                }

                // If the native dialog has not been created yet
                if (this.HostingDialog == null)
                {
                    this.selectedIndex = value;
                }
                else if (value >= 0 && value < this.items.Count)
                {
                    this.selectedIndex = value;
                    this.ApplyPropertyChange("SelectedIndex");
                }
                else
                {
                    throw new IndexOutOfRangeException(LocalizedMessages.RadioButtonListIndexOutOfBounds);
                }
            }
        }

        /// <summary>
        /// Occurs when the user changes the SelectedIndex.
        /// </summary>
        ///
        /// <remarks>
        /// By initializing the SelectedIndexChanged event with an empty
        /// delegate, we can skip the test to determine
        /// if the SelectedIndexChanged is null.
        /// test.
        /// </remarks>
        public event EventHandler SelectedIndexChanged = delegate { };

        /// <summary>
        /// Occurs when the user changes the SelectedIndex.
        /// </summary>
        /// <remarks>Because this method is defined in an interface, we can either
        /// have it as public, or make it private and explicitly implement (like below).
        /// Making it public doesn't really help as its only internal (but can't have this
        /// internal because of the interface)
        /// </remarks>
        void ICommonFileDialogIndexedControls.RaiseSelectedIndexChangedEvent()
        {
            // Make sure that this control is enabled and has a specified delegate
            if (this.Enabled)
            {
                this.SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        /// <summary>
        /// Attach the RadioButtonList control to the dialog object
        /// </summary>
        /// <param name="dialog">The target dialog</param>
        internal override void Attach(IFileDialogCustomize dialog)
        {
            Debug.Assert(dialog != null, "CommonFileDialogRadioButtonList.Attach: dialog parameter can not be null");

            // Add the radio button list control
            dialog.AddRadioButtonList(this.Id);

            // Add the radio button list items
            for (int index = 0; index < this.items.Count; index++)
            {
                dialog.AddControlItem(this.Id, index, this.items[index].Text);
            }

            // Set the currently selected item
            if (this.selectedIndex >= 0 && this.selectedIndex < this.items.Count)
            {
                dialog.SetSelectedControlItem(this.Id, this.selectedIndex);
            }
            else if (this.selectedIndex != -1)
            {
                throw new IndexOutOfRangeException(LocalizedMessages.RadioButtonListIndexOutOfBounds);
            }

            // Sync unmanaged properties with managed properties
            this.SyncUnmanagedProperties();
        }
    }

    /// <summary>
    /// Represents a list item for the CommonFileDialogRadioButtonList object.
    /// </summary>
    public class CommonFileDialogRadioButtonListItem
    {
        /// <summary>
        /// Gets or sets the string that will be displayed for this list item.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public CommonFileDialogRadioButtonListItem() : this(string.Empty)
        {
        }

        /// <summary>
        /// Creates a new instance of this class with the specified text.
        /// </summary>
        /// <param name="text">The string that you want to display for this list item.</param>
        public CommonFileDialogRadioButtonListItem(string text)
        {
            this.Text = text;
        }
    }
}
