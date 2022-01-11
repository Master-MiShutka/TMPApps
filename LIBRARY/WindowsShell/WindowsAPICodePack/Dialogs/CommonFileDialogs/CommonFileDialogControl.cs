// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs.Controls
{
    /// <summary>
    /// Defines an abstract class that supports shared functionality for the
    /// common file dialog controls.
    /// </summary>
    public abstract class CommonFileDialogControl : DialogControl
    {
        /// <summary>
        /// Holds the text that is displayed for this control.
        /// </summary>
        private string textValue;

        /// <summary>
        /// Gets or sets the text string that is displayed on the control.
        /// </summary>
        public virtual string Text
        {
            get => this.textValue;

            set
            {
                // Don't update this property if it hasn't changed
                if (value != this.textValue)
                {
                    this.textValue = value;
                    this.ApplyPropertyChange("Text");
                }
            }
        }

        private bool enabled = true;

        /// <summary>
        /// Gets or sets a value that determines if this control is enabled.
        /// </summary>
        public bool Enabled
        {
            get => this.enabled;

            set
            {
                // Don't update this property if it hasn't changed
                if (value == this.enabled)
                {
                    return;
                }

                this.enabled = value;
                this.ApplyPropertyChange("Enabled");
            }
        }

        private bool visible = true;

        /// <summary>
        /// Gets or sets a boolean value that indicates whether
        /// this control is visible.
        /// </summary>
        public bool Visible
        {
            get => this.visible;

            set
            {
                // Don't update this property if it hasn't changed
                if (value == this.visible)
                {
                    return;
                }

                this.visible = value;
                this.ApplyPropertyChange("Visible");
            }
        }

        private bool isAdded;

        /// <summary>
        /// Has this control been added to the dialog
        /// </summary>
        internal bool IsAdded
        {
            get => this.isAdded;
            set => this.isAdded = value;
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        protected CommonFileDialogControl()
        {
        }

        /// <summary>
        /// Creates a new instance of this class with the text.
        /// </summary>
        /// <param name="text">The text of the common file dialog control.</param>
        protected CommonFileDialogControl(string text)
            : base()
        {
            this.textValue = text;
        }

        /// <summary>
        /// Creates a new instance of this class with the specified name and text.
        /// </summary>
        /// <param name="name">The name of the common file dialog control.</param>
        /// <param name="text">The text of the common file dialog control.</param>
        protected CommonFileDialogControl(string name, string text)
            : base(name)
        {
            this.textValue = text;
        }

        /// <summary>
        /// Attach the custom control itself to the specified dialog
        /// </summary>
        /// <param name="dialog">the target dialog</param>
        internal abstract void Attach(IFileDialogCustomize dialog);

        internal virtual void SyncUnmanagedProperties()
        {
            this.ApplyPropertyChange("Enabled");
            this.ApplyPropertyChange("Visible");
        }
    }
}
