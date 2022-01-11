// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using Microsoft.WindowsAPICodePack.Resources;

    /// <summary>
    /// Provides a visual representation of the progress of a long running operation.
    /// </summary>
    public class TaskDialogProgressBar : TaskDialogBar
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public TaskDialogProgressBar()
        {
        }

        /// <summary>
        /// Creates a new instance of this class with the specified name.
        /// And using the default values: Min = 0, Max = 100, Current = 0
        /// </summary>
        /// <param name="name">The name of the control.</param>
        public TaskDialogProgressBar(string name) : base(name)
        {
        }

        /// <summary>
        /// Creates a new instance of this class with the specified
        /// minimum, maximum and current values.
        /// </summary>
        /// <param name="minimum">The minimum value for this control.</param>
        /// <param name="maximum">The maximum value for this control.</param>
        /// <param name="value">The current value for this control.</param>
        public TaskDialogProgressBar(int minimum, int maximum, int value)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Value = value;
        }

        private int minimum;
        private int _value;
        private int maximum = TaskDialogDefaults.ProgressBarMaximumValue;

        /// <summary>
        /// Gets or sets the minimum value for the control.
        /// </summary>
        public int Minimum
        {
            get => this.minimum;

            set
            {
                this.CheckPropertyChangeAllowed("Minimum");

                // Check for positive numbers
                if (value < 0)
                {
                    throw new System.ArgumentException(LocalizedMessages.TaskDialogProgressBarMinValueGreaterThanZero, nameof(value));
                }

                // Check if min / max differ
                if (value >= this.Maximum)
                {
                    throw new System.ArgumentException(LocalizedMessages.TaskDialogProgressBarMinValueLessThanMax, nameof(value));
                }

                this.minimum = value;
                this.ApplyPropertyChange("Minimum");
            }
        }

        /// <summary>
        /// Gets or sets the maximum value for the control.
        /// </summary>
        public int Maximum
        {
            get => this.maximum;

            set
            {
                this.CheckPropertyChangeAllowed("Maximum");

                // Check if min / max differ
                if (value < this.Minimum)
                {
                    throw new System.ArgumentException(LocalizedMessages.TaskDialogProgressBarMaxValueGreaterThanMin, nameof(value));
                }

                this.maximum = value;
                this.ApplyPropertyChange("Maximum");
            }
        }

        /// <summary>
        /// Gets or sets the current value for the control.
        /// </summary>
        public int Value
        {
            get => this._value;

            set
            {
                this.CheckPropertyChangeAllowed("Value");

                // Check for positive numbers
                if (value < this.Minimum || value > this.Maximum)
                {
                    throw new System.ArgumentException(LocalizedMessages.TaskDialogProgressBarValueInRange, nameof(value));
                }

                this._value = value;
                this.ApplyPropertyChange("Value");
            }
        }

        /// <summary>
        /// Verifies that the progress bar's value is between its minimum and maximum.
        /// </summary>
        internal bool HasValidValues => this.minimum <= this._value && this._value <= this.maximum;

        /// <summary>
        /// Resets the control to its minimum value.
        /// </summary>
        protected internal override void Reset()
        {
            base.Reset();
            this._value = this.minimum;
        }
    }
}
