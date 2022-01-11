// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.WindowsAPICodePack.Resources;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Encapsulates a new-to-Vista Win32 TaskDialog window
    /// - a powerful successor to the MessageBox available
    /// in previous versions of Windows.
    /// </summary>
    public sealed class TaskDialog : IDialogControlHost, IDisposable
    {
        // Global instance of TaskDialog, to be used by static Show() method.
        // As most parameters of a dialog created via static Show() will have
        // identical parameters, we'll create one TaskDialog and treat it
        // as a NativeTaskDialog generator for all static Show() calls.
        private static TaskDialog staticDialog;

        // Main current native dialog.
        private NativeTaskDialog nativeDialog;

        private List<TaskDialogButtonBase> buttons = new List<TaskDialogButtonBase>();
        private List<TaskDialogButtonBase> radioButtons = new List<TaskDialogButtonBase>();
        private List<TaskDialogButtonBase> commandLinks = new List<TaskDialogButtonBase>();
        private IntPtr ownerWindow;

        #region Public Properties

        /// <summary>
        /// Occurs when a progress bar changes.
        /// </summary>
        public event EventHandler<TaskDialogTickEventArgs> Tick;

        /// <summary>
        /// Occurs when a user clicks a hyperlink.
        /// </summary>
        public event EventHandler<TaskDialogHyperlinkClickedEventArgs> HyperlinkClick;

        /// <summary>
        /// Occurs when the TaskDialog is closing.
        /// </summary>
        public event EventHandler<TaskDialogClosingEventArgs> Closing;

        /// <summary>
        /// Occurs when a user clicks on Help.
        /// </summary>
        public event EventHandler HelpInvoked;

        /// <summary>
        /// Occurs when the TaskDialog is opened.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Gets or sets a value that contains the owner window's handle.
        /// </summary>
        public IntPtr OwnerWindowHandle
        {
            get => this.ownerWindow;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.OwnerCannotBeChanged);
                this.ownerWindow = value;
            }
        }

        // Main content (maps to MessageBox's "message").
        private string text;

        /// <summary>
        /// Gets or sets a value that contains the message text.
        /// </summary>
        public string Text
        {
            get => this.text;

            set
            {
                // Set local value, then update native dialog if showing.
                this.text = value;
                if (this.NativeDialogShowing)
                {
                    this.nativeDialog.UpdateText(this.text);
                }
            }
        }

        private string instructionText;

        /// <summary>
        /// Gets or sets a value that contains the instruction text.
        /// </summary>
        public string InstructionText
        {
            get => this.instructionText;

            set
            {
                // Set local value, then update native dialog if showing.
                this.instructionText = value;
                if (this.NativeDialogShowing)
                {
                    this.nativeDialog.UpdateInstruction(this.instructionText);
                }
            }
        }

        private string caption;

        /// <summary>
        /// Gets or sets a value that contains the caption text.
        /// </summary>
        public string Caption
        {
            get => this.caption;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.CaptionCannotBeChanged);
                this.caption = value;
            }
        }

        private string footerText;

        /// <summary>
        /// Gets or sets a value that contains the footer text.
        /// </summary>
        public string FooterText
        {
            get => this.footerText;

            set
            {
                // Set local value, then update native dialog if showing.
                this.footerText = value;
                if (this.NativeDialogShowing)
                {
                    this.nativeDialog.UpdateFooterText(this.footerText);
                }
            }
        }

        private string checkBoxText;

        /// <summary>
        /// Gets or sets a value that contains the footer check box text.
        /// </summary>
        public string FooterCheckBoxText
        {
            get => this.checkBoxText;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.CheckBoxCannotBeChanged);
                this.checkBoxText = value;
            }
        }

        private string detailsExpandedText;

        /// <summary>
        /// Gets or sets a value that contains the expanded text in the details section.
        /// </summary>
        public string DetailsExpandedText
        {
            get => this.detailsExpandedText;

            set
            {
                // Set local value, then update native dialog if showing.
                this.detailsExpandedText = value;
                if (this.NativeDialogShowing)
                {
                    this.nativeDialog.UpdateExpandedText(this.detailsExpandedText);
                }
            }
        }

        private bool detailsExpanded;

        /// <summary>
        /// Gets or sets a value that determines if the details section is expanded.
        /// </summary>
        public bool DetailsExpanded
        {
            get => this.detailsExpanded;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.ExpandingStateCannotBeChanged);
                this.detailsExpanded = value;
            }
        }

        private string detailsExpandedLabel;

        /// <summary>
        /// Gets or sets a value that contains the expanded control text.
        /// </summary>
        public string DetailsExpandedLabel
        {
            get => this.detailsExpandedLabel;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.ExpandedLabelCannotBeChanged);
                this.detailsExpandedLabel = value;
            }
        }

        private string detailsCollapsedLabel;

        /// <summary>
        /// Gets or sets a value that contains the collapsed control text.
        /// </summary>
        public string DetailsCollapsedLabel
        {
            get => this.detailsCollapsedLabel;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.CollapsedTextCannotBeChanged);
                this.detailsCollapsedLabel = value;
            }
        }

        private bool cancelable;

        /// <summary>
        /// Gets or sets a value that determines if Cancelable is set.
        /// </summary>
        public bool Cancelable
        {
            get => this.cancelable;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.CancelableCannotBeChanged);
                this.cancelable = value;
            }
        }

        private TaskDialogStandardIcon icon;

        /// <summary>
        /// Gets or sets a value that contains the TaskDialog main icon.
        /// </summary>
        public TaskDialogStandardIcon Icon
        {
            get => this.icon;

            set
            {
                // Set local value, then update native dialog if showing.
                this.icon = value;
                if (this.NativeDialogShowing)
                {
                    this.nativeDialog.UpdateMainIcon(this.icon);
                }
            }
        }

        private TaskDialogStandardIcon footerIcon;

        /// <summary>
        /// Gets or sets a value that contains the footer icon.
        /// </summary>
        public TaskDialogStandardIcon FooterIcon
        {
            get => this.footerIcon;

            set
            {
                // Set local value, then update native dialog if showing.
                this.footerIcon = value;
                if (this.NativeDialogShowing)
                {
                    this.nativeDialog.UpdateFooterIcon(this.footerIcon);
                }
            }
        }

        private TaskDialogStandardButtons standardButtons = TaskDialogStandardButtons.None;

        /// <summary>
        /// Gets or sets a value that contains the standard buttons.
        /// </summary>
        public TaskDialogStandardButtons StandardButtons
        {
            get => this.standardButtons;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.StandardButtonsCannotBeChanged);
                this.standardButtons = value;
            }
        }

        private DialogControlCollection<TaskDialogControl> controls;

        /// <summary>
        /// Gets a value that contains the TaskDialog controls.
        /// </summary>
        public DialogControlCollection<TaskDialogControl> Controls => this.controls;

        private bool hyperlinksEnabled;

        /// <summary>
        /// Gets or sets a value that determines if hyperlinks are enabled.
        /// </summary>
        public bool HyperlinksEnabled
        {
            get => this.hyperlinksEnabled;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.HyperlinksCannotBetSet);
                this.hyperlinksEnabled = value;
            }
        }

        private bool? footerCheckBoxChecked = null;

        /// <summary>
        /// Gets or sets a value that indicates if the footer checkbox is checked.
        /// </summary>
        public bool? FooterCheckBoxChecked
        {
            get => this.footerCheckBoxChecked.GetValueOrDefault(false);

            set
            {
                // Set local value, then update native dialog if showing.
                this.footerCheckBoxChecked = value;
                if (this.NativeDialogShowing)
                {
                    this.nativeDialog.UpdateCheckBoxChecked(this.footerCheckBoxChecked.Value);
                }
            }
        }

        private TaskDialogExpandedDetailsLocation expansionMode;

        /// <summary>
        /// Gets or sets a value that contains the expansion mode for this dialog.
        /// </summary>
        public TaskDialogExpandedDetailsLocation ExpansionMode
        {
            get => this.expansionMode;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.ExpandedDetailsCannotBeChanged);
                this.expansionMode = value;
            }
        }

        private TaskDialogStartupLocation startupLocation;

        /// <summary>
        /// Gets or sets a value that contains the startup location.
        /// </summary>
        public TaskDialogStartupLocation StartupLocation
        {
            get => this.startupLocation;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.StartupLocationCannotBeChanged);
                this.startupLocation = value;
            }
        }

        private TaskDialogProgressBar progressBar;

        /// <summary>
        /// Gets or sets the progress bar on the taskdialog. ProgressBar a visual representation
        /// of the progress of a long running operation.
        /// </summary>
        public TaskDialogProgressBar ProgressBar
        {
            get => this.progressBar;

            set
            {
                this.ThrowIfDialogShowing(LocalizedMessages.ProgressBarCannotBeChanged);
                if (value != null)
                {
                    if (value.HostingDialog != null)
                    {
                        throw new InvalidOperationException(LocalizedMessages.ProgressBarCannotBeHostedInMultipleDialogs);
                    }

                    value.HostingDialog = this;
                }

                this.progressBar = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a basic TaskDialog window
        /// </summary>
        public TaskDialog()
        {
            CoreHelpers.ThrowIfNotVista();

            // Initialize various data structs.
            this.controls = new DialogControlCollection<TaskDialogControl>(this);
        }

        #endregion

        #region Static Show Methods

        /// <summary>
        /// Creates and shows a task dialog with the specified message text.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The dialog result.</returns>
        public static TaskDialogResult Show(string text)
        {
            return ShowCoreStatic(
                text,
                TaskDialogDefaults.MainInstruction,
                TaskDialogDefaults.Caption);
        }

        /// <summary>
        /// Creates and shows a task dialog with the specified supporting text and main instruction.
        /// </summary>
        /// <param name="text">The supporting text to display.</param>
        /// <param name="instructionText">The main instruction text to display.</param>
        /// <returns>The dialog result.</returns>
        public static TaskDialogResult Show(string text, string instructionText)
        {
            return ShowCoreStatic(
                text,
                instructionText,
                TaskDialogDefaults.Caption);
        }

        /// <summary>
        /// Creates and shows a task dialog with the specified supporting text, main instruction, and dialog caption.
        /// </summary>
        /// <param name="text">The supporting text to display.</param>
        /// <param name="instructionText">The main instruction text to display.</param>
        /// <param name="caption">The caption for the dialog.</param>
        /// <returns>The dialog result.</returns>
        public static TaskDialogResult Show(string text, string instructionText, string caption)
        {
            return ShowCoreStatic(text, instructionText, caption);
        }
        #endregion

        #region Instance Show Methods

        /// <summary>
        /// Creates and shows a task dialog.
        /// </summary>
        /// <returns>The dialog result.</returns>
        public TaskDialogResult Show()
        {
            return this.ShowCore();
        }
        #endregion

        #region Core Show Logic

        // CORE SHOW METHODS:
        // All static Show() calls forward here -
        // it is responsible for retrieving
        // or creating our cached TaskDialog instance, getting it configured,
        // and in turn calling the appropriate instance Show.
        private static TaskDialogResult ShowCoreStatic(
            string text,
            string instructionText,
            string caption)
        {
            CoreHelpers.ThrowIfNotVista();

            // If no instance cached yet, create it.
            if (staticDialog == null)
            {
                // New TaskDialog will automatically pick up defaults when
                // a new config structure is created as part of ShowCore().
                staticDialog = new TaskDialog();
            }

            // Set the few relevant properties,
            // and go with the defaults for the others.
            staticDialog.text = text;
            staticDialog.instructionText = instructionText;
            staticDialog.caption = caption;

            return staticDialog.Show();
        }

        private TaskDialogResult ShowCore()
        {
            TaskDialogResult result;

            try
            {
                // Populate control lists, based on current
                // contents - note we are somewhat late-bound
                // on our control lists, to support XAML scenarios.
                this.SortDialogControls();

                // First, let's make sure it even makes
                // sense to try a show.
                this.ValidateCurrentDialogSettings();

                // Create settings object for new dialog,
                // based on current state.
                NativeTaskDialogSettings settings = new NativeTaskDialogSettings();
                this.ApplyCoreSettings(settings);
                this.ApplySupplementalSettings(settings);

                // Show the dialog.
                // NOTE: this is a BLOCKING call; the dialog proc callbacks
                // will be executed by the same thread as the
                // Show() call before the thread of execution
                // contines to the end of this method.
                this.nativeDialog = new NativeTaskDialog(settings, this);
                this.nativeDialog.NativeShow();

                // Build and return dialog result to public API - leaving it
                // null after an exception is thrown is fine in this case
                result = ConstructDialogResult(this.nativeDialog);
                this.footerCheckBoxChecked = this.nativeDialog.CheckBoxChecked;
            }
            finally
            {
                this.CleanUp();
                this.nativeDialog = null;
            }

            return result;
        }

        // Helper that looks at the current state of the TaskDialog and verifies
        // that there aren't any abberant combinations of properties.
        // NOTE that this method is designed to throw
        // rather than return a bool.
        private void ValidateCurrentDialogSettings()
        {
            if (this.footerCheckBoxChecked.HasValue &&
                this.footerCheckBoxChecked.Value == true &&
                string.IsNullOrEmpty(this.checkBoxText))
            {
                throw new InvalidOperationException(LocalizedMessages.TaskDialogCheckBoxTextRequiredToEnableCheckBox);
            }

            // Progress bar validation.
            // Make sure the progress bar values are valid.
            // the Win32 API will valiantly try to rationalize
            // bizarre min/max/value combinations, but we'll save
            // it the trouble by validating.
            if (this.progressBar != null && !this.progressBar.HasValidValues)
            {
                throw new InvalidOperationException(LocalizedMessages.TaskDialogProgressBarValueInRange);
            }

            // Validate Buttons collection.
            // Make sure we don't have buttons AND
            // command-links - the Win32 API treats them as different
            // flavors of a single button struct.
            if (this.buttons.Count > 0 && this.commandLinks.Count > 0)
            {
                throw new NotSupportedException(LocalizedMessages.TaskDialogSupportedButtonsAndLinks);
            }

            if (this.buttons.Count > 0 && this.standardButtons != TaskDialogStandardButtons.None)
            {
                throw new NotSupportedException(LocalizedMessages.TaskDialogSupportedButtonsAndButtons);
            }
        }

        // Analyzes the final state of the NativeTaskDialog instance and creates the
        // final TaskDialogResult that will be returned from the public API
        private static TaskDialogResult ConstructDialogResult(NativeTaskDialog native)
        {
            Debug.Assert(native.ShowState == DialogShowState.Closed, "dialog result being constructed for unshown dialog.");

            TaskDialogResult result = TaskDialogResult.Cancel;

            TaskDialogStandardButtons standardButton = MapButtonIdToStandardButton(native.SelectedButtonId);

            // If returned ID isn't a standard button, let's fetch
            if (standardButton == TaskDialogStandardButtons.None)
            {
                result = TaskDialogResult.CustomButtonClicked;
            }
            else
            {
                result = (TaskDialogResult)standardButton;
            }

            return result;
        }

        /// <summary>
        /// Close TaskDialog
        /// </summary>
        /// <exception cref="InvalidOperationException">if TaskDialog is not showing.</exception>
        public void Close()
        {
            if (!this.NativeDialogShowing)
            {
                throw new InvalidOperationException(LocalizedMessages.TaskDialogCloseNonShowing);
            }

            this.nativeDialog.NativeClose(TaskDialogResult.Cancel);

            // TaskDialog's own cleanup code -
            // which runs post show - will handle disposal of native dialog.
        }

        /// <summary>
        /// Close TaskDialog with a given TaskDialogResult
        /// </summary>
        /// <param name="closingResult">TaskDialogResult to return from the TaskDialog.Show() method</param>
        /// <exception cref="InvalidOperationException">if TaskDialog is not showing.</exception>
        public void Close(TaskDialogResult closingResult)
        {
            if (!this.NativeDialogShowing)
            {
                throw new InvalidOperationException(LocalizedMessages.TaskDialogCloseNonShowing);
            }

            this.nativeDialog.NativeClose(closingResult);

            // TaskDialog's own cleanup code -
            // which runs post show - will handle disposal of native dialog.
        }

        #endregion

        #region Configuration Construction

        private void ApplyCoreSettings(NativeTaskDialogSettings settings)
        {
            this.ApplyGeneralNativeConfiguration(settings.NativeConfiguration);
            this.ApplyTextConfiguration(settings.NativeConfiguration);
            this.ApplyOptionConfiguration(settings.NativeConfiguration);
            this.ApplyControlConfiguration(settings);
        }

        private void ApplyGeneralNativeConfiguration(TaskDialogNativeMethods.TaskDialogConfiguration dialogConfig)
        {
            // If an owner wasn't specifically specified,
            // we'll use the app's main window.
            if (this.ownerWindow != IntPtr.Zero)
            {
                dialogConfig.parentHandle = this.ownerWindow;
            }

            // Other miscellaneous sets.
            dialogConfig.mainIcon = new TaskDialogNativeMethods.IconUnion((int)this.icon);
            dialogConfig.footerIcon = new TaskDialogNativeMethods.IconUnion((int)this.footerIcon);
            dialogConfig.commonButtons = (TaskDialogNativeMethods.TaskDialogCommonButtons)this.standardButtons;
        }

        /// <summary>
        /// Sets important text properties.
        /// </summary>
        /// <param name="dialogConfig">An instance of a <see cref="TaskDialogNativeMethods.TaskDialogConfiguration"/> object.</param>
        private void ApplyTextConfiguration(TaskDialogNativeMethods.TaskDialogConfiguration dialogConfig)
        {
            // note that nulls or empty strings are fine here.
            dialogConfig.content = this.text;
            dialogConfig.windowTitle = this.caption;
            dialogConfig.mainInstruction = this.instructionText;
            dialogConfig.expandedInformation = this.detailsExpandedText;
            dialogConfig.expandedControlText = this.detailsExpandedLabel;
            dialogConfig.collapsedControlText = this.detailsCollapsedLabel;
            dialogConfig.footerText = this.footerText;
            dialogConfig.verificationText = this.checkBoxText;
        }

        private void ApplyOptionConfiguration(TaskDialogNativeMethods.TaskDialogConfiguration dialogConfig)
        {
            // Handle options - start with no options set.
            TaskDialogNativeMethods.TaskDialogOptions options = TaskDialogNativeMethods.TaskDialogOptions.None;
            if (this.cancelable)
            {
                options |= TaskDialogNativeMethods.TaskDialogOptions.AllowCancel;
            }

            if (this.footerCheckBoxChecked.HasValue && this.footerCheckBoxChecked.Value)
            {
                options |= TaskDialogNativeMethods.TaskDialogOptions.CheckVerificationFlag;
            }

            if (this.hyperlinksEnabled)
            {
                options |= TaskDialogNativeMethods.TaskDialogOptions.EnableHyperlinks;
            }

            if (this.detailsExpanded)
            {
                options |= TaskDialogNativeMethods.TaskDialogOptions.ExpandedByDefault;
            }

            if (this.Tick != null)
            {
                options |= TaskDialogNativeMethods.TaskDialogOptions.UseCallbackTimer;
            }

            if (this.startupLocation == TaskDialogStartupLocation.CenterOwner)
            {
                options |= TaskDialogNativeMethods.TaskDialogOptions.PositionRelativeToWindow;
            }

            // Note: no validation required, as we allow this to
            // be set even if there is no expanded information
            // text because that could be added later.
            // Default for Win32 API is to expand into (and after)
            // the content area.
            if (this.expansionMode == TaskDialogExpandedDetailsLocation.ExpandFooter)
            {
                options |= TaskDialogNativeMethods.TaskDialogOptions.ExpandFooterArea;
            }

            // Finally, apply options to config.
            dialogConfig.taskDialogFlags = options;
        }

        // Builds the actual configuration
        // that the NativeTaskDialog (and underlying Win32 API)
        // expects, by parsing the various control
        // lists, marshalling to the unmanaged heap, etc.
        private void ApplyControlConfiguration(NativeTaskDialogSettings settings)
        {
            // Deal with progress bars/marquees.
            if (this.progressBar != null)
            {
                if (this.progressBar.State == TaskDialogProgressBarState.Marquee)
                {
                    settings.NativeConfiguration.taskDialogFlags |= TaskDialogNativeMethods.TaskDialogOptions.ShowMarqueeProgressBar;
                }
                else
                {
                    settings.NativeConfiguration.taskDialogFlags |= TaskDialogNativeMethods.TaskDialogOptions.ShowProgressBar;
                }
            }

            // Build the native struct arrays that NativeTaskDialog
            // needs - though NTD will handle
            // the heavy lifting marshalling to make sure
            // all the cleanup is centralized there.
            if (this.buttons.Count > 0 || this.commandLinks.Count > 0)
            {
                // These are the actual arrays/lists of
                // the structs that we'll copy to the
                // unmanaged heap.
                List<TaskDialogButtonBase> sourceList = this.buttons.Count > 0 ? this.buttons : this.commandLinks;
                settings.Buttons = BuildButtonStructArray(sourceList);

                // Apply option flag that forces all
                // custom buttons to render as command links.
                if (this.commandLinks.Count > 0)
                {
                    settings.NativeConfiguration.taskDialogFlags |= TaskDialogNativeMethods.TaskDialogOptions.UseCommandLinks;
                }

                // Set default button and add elevation icons
                // to appropriate buttons.
                settings.NativeConfiguration.defaultButtonIndex = FindDefaultButtonId(sourceList);

                ApplyElevatedIcons(settings, sourceList);
            }

            if (this.radioButtons.Count > 0)
            {
                settings.RadioButtons = BuildButtonStructArray(this.radioButtons);

                // Set default radio button - radio buttons don't support.
                int defaultRadioButton = FindDefaultButtonId(this.radioButtons);
                settings.NativeConfiguration.defaultRadioButtonIndex = defaultRadioButton;

                if (defaultRadioButton == TaskDialogNativeMethods.NoDefaultButtonSpecified)
                {
                    settings.NativeConfiguration.taskDialogFlags |= TaskDialogNativeMethods.TaskDialogOptions.NoDefaultRadioButton;
                }
            }
        }

        private static TaskDialogNativeMethods.TaskDialogButton[] BuildButtonStructArray(List<TaskDialogButtonBase> controls)
        {
            TaskDialogNativeMethods.TaskDialogButton[] buttonStructs;
            TaskDialogButtonBase button;

            int totalButtons = controls.Count;
            buttonStructs = new TaskDialogNativeMethods.TaskDialogButton[totalButtons];
            for (int i = 0; i < totalButtons; i++)
            {
                button = controls[i];
                buttonStructs[i] = new TaskDialogNativeMethods.TaskDialogButton(button.Id, button.ToString());
            }

            return buttonStructs;
        }

        // Searches list of controls and returns the ID of
        // the default control, or 0 if no default was specified.
        private static int FindDefaultButtonId(List<TaskDialogButtonBase> controls)
        {
            var defaults = controls.FindAll(control => control.Default);

            if (defaults.Count == 1)
            {
                return defaults[0].Id;
            }
            else if (defaults.Count > 1)
            {
                throw new InvalidOperationException(LocalizedMessages.TaskDialogOnlyOneDefaultControl);
            }

            return TaskDialogNativeMethods.NoDefaultButtonSpecified;
        }

        private static void ApplyElevatedIcons(NativeTaskDialogSettings settings, List<TaskDialogButtonBase> controls)
        {
            foreach (TaskDialogButton control in controls)
            {
                if (control.UseElevationIcon)
                {
                    if (settings.ElevatedButtons == null)
                    {
                        settings.ElevatedButtons = new List<int>();
                    }

                    settings.ElevatedButtons.Add(control.Id);
                }
            }
        }

        private void ApplySupplementalSettings(NativeTaskDialogSettings settings)
        {
            if (this.progressBar != null)
            {
                if (this.progressBar.State != TaskDialogProgressBarState.Marquee)
                {
                    settings.ProgressBarMinimum = this.progressBar.Minimum;
                    settings.ProgressBarMaximum = this.progressBar.Maximum;
                    settings.ProgressBarValue = this.progressBar.Value;
                    settings.ProgressBarState = this.progressBar.State;
                }
            }

            if (this.HelpInvoked != null)
            {
                settings.InvokeHelp = true;
            }
        }

        // Here we walk our controls collection and
        // sort the various controls by type.
        private void SortDialogControls()
        {
            foreach (TaskDialogControl control in this.controls)
            {
                TaskDialogButtonBase buttonBase = control as TaskDialogButtonBase;
                TaskDialogCommandLink commandLink = control as TaskDialogCommandLink;

                if (buttonBase != null && string.IsNullOrEmpty(buttonBase.Text) &&
                    commandLink != null && string.IsNullOrEmpty(commandLink.Instruction))
                {
                    throw new InvalidOperationException(LocalizedMessages.TaskDialogButtonTextEmpty);
                }

                TaskDialogRadioButton radButton;
                TaskDialogProgressBar progBar;

                // Loop through child controls
                // and sort the controls based on type.
                if (commandLink != null)
                {
                    this.commandLinks.Add(commandLink);
                }
                else if ((radButton = control as TaskDialogRadioButton) != null)
                {
                    if (this.radioButtons == null)
                    {
                        this.radioButtons = new List<TaskDialogButtonBase>();
                    }

                    this.radioButtons.Add(radButton);
                }
                else if (buttonBase != null)
                {
                    if (this.buttons == null)
                    {
                        this.buttons = new List<TaskDialogButtonBase>();
                    }

                    this.buttons.Add(buttonBase);
                }
                else if ((progBar = control as TaskDialogProgressBar) != null)
                {
                    this.progressBar = progBar;
                }
                else
                {
                    throw new InvalidOperationException(LocalizedMessages.TaskDialogUnkownControl);
                }
            }
        }

        #endregion

        #region Helpers

        // Helper to map the standard button IDs returned by
        // TaskDialogIndirect to the standard button ID enum -
        // note that we can't just cast, as the Win32
        // typedefs differ incoming and outgoing.
        private static TaskDialogStandardButtons MapButtonIdToStandardButton(int id)
        {
            switch ((TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds)id)
            {
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Ok:
                    return TaskDialogStandardButtons.Ok;
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Cancel:
                    return TaskDialogStandardButtons.Cancel;
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Abort:
                    // Included for completeness in API -
                    // we can't pass in an Abort standard button.
                    return TaskDialogStandardButtons.None;
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Retry:
                    return TaskDialogStandardButtons.Retry;
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Ignore:
                    // Included for completeness in API -
                    // we can't pass in an Ignore standard button.
                    return TaskDialogStandardButtons.None;
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Yes:
                    return TaskDialogStandardButtons.Yes;
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.No:
                    return TaskDialogStandardButtons.No;
                case TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Close:
                    return TaskDialogStandardButtons.Close;
                default:
                    return TaskDialogStandardButtons.None;
            }
        }

        private void ThrowIfDialogShowing(string message)
        {
            if (this.NativeDialogShowing)
            {
                throw new NotSupportedException(message);
            }
        }

        private bool NativeDialogShowing => (this.nativeDialog != null)
                    && (this.nativeDialog.ShowState == DialogShowState.Showing
                    || this.nativeDialog.ShowState == DialogShowState.Closing);

        // NOTE: we are going to require names be unique
        // across both buttons and radio buttons,
        // even though the Win32 API allows them to be separate.
        private TaskDialogButtonBase GetButtonForId(int id)
        {
            return (TaskDialogButtonBase)this.controls.GetControlbyId(id);
        }

        #endregion

        #region IDialogControlHost Members

        // We're explicitly implementing this interface
        // as the user will never need to know about it
        // or use it directly - it is only for the internal
        // implementation of "pseudo controls" within
        // the dialogs.

        // Called whenever controls are being added
        // to or removed from the dialog control collection.
        bool IDialogControlHost.IsCollectionChangeAllowed()
        {
            // Only allow additions to collection if dialog is NOT showing.
            return !this.NativeDialogShowing;
        }

        // Called whenever controls have been added or removed.
        void IDialogControlHost.ApplyCollectionChanged()
        {
            // If we're showing, we should never get here -
            // the changing notification would have thrown and the
            // property would not have been changed.
            Debug.Assert(!this.NativeDialogShowing,
                "Collection changed notification received despite show state of dialog");
        }

        // Called when a control currently in the collection
        // has a property changing - this is
        // basically to screen out property changes that
        // cannot occur while the dialog is showing
        // because the Win32 API has no way for us to
        // propagate the changes until we re-invoke the Win32 call.
        bool IDialogControlHost.IsControlPropertyChangeAllowed(string propertyName, DialogControl control)
        {
            Debug.Assert(control is TaskDialogControl,
                "Property changing for a control that is not a TaskDialogControl-derived type");
            Debug.Assert(propertyName != "Name",
                "Name changes at any time are not supported - public API should have blocked this");

            bool canChange = false;

            if (!this.NativeDialogShowing)
            {
                // Certain properties can't be changed if the dialog is not showing
                // we need a handle created before we can set these...
                switch (propertyName)
                {
                    case "Enabled":
                        canChange = false;
                        break;
                    default:
                        canChange = true;
                        break;
                }
            }
            else
            {
                // If the dialog is showing, we can only
                // allow some properties to change.
                switch (propertyName)
                {
                    // Properties that CAN'T be changed while dialog is showing.
                    case "Text":
                    case "Default":
                        canChange = false;
                        break;

                    // Properties that CAN be changed while dialog is showing.
                    case "ShowElevationIcon":
                    case "Enabled":
                        canChange = true;
                        break;
                    default:
                        Debug.Assert(true, "Unknown property name coming through property changing handler");
                        break;
                }
            }

            return canChange;
        }

        // Called when a control currently in the collection
        // has a property changed - this handles propagating
        // the new property values to the Win32 API.
        // If there isn't a way to change the Win32 value, then we
        // should have already screened out the property set
        // in NotifyControlPropertyChanging.
        void IDialogControlHost.ApplyControlPropertyChange(string propertyName, DialogControl control)
        {
            // We only need to apply changes to the
            // native dialog when it actually exists.
            if (this.NativeDialogShowing)
            {
                TaskDialogButton button;
                TaskDialogRadioButton radioButton;
                if (control is TaskDialogProgressBar)
                {
                    if (!this.progressBar.HasValidValues)
                    {
                        throw new ArgumentException(LocalizedMessages.TaskDialogProgressBarValueInRange);
                    }

                    switch (propertyName)
                    {
                        case "State":
                            this.nativeDialog.UpdateProgressBarState(this.progressBar.State);
                            break;
                        case "Value":
                            this.nativeDialog.UpdateProgressBarValue(this.progressBar.Value);
                            break;
                        case "Minimum":
                        case "Maximum":
                            this.nativeDialog.UpdateProgressBarRange();
                            break;
                        default:
                            Debug.Assert(true, "Unknown property being set");
                            break;
                    }
                }
                else if ((button = control as TaskDialogButton) != null)
                {
                    switch (propertyName)
                    {
                        case "ShowElevationIcon":
                            this.nativeDialog.UpdateElevationIcon(button.Id, button.UseElevationIcon);
                            break;
                        case "Enabled":
                            this.nativeDialog.UpdateButtonEnabled(button.Id, button.Enabled);
                            break;
                        default:
                            Debug.Assert(true, "Unknown property being set");
                            break;
                    }
                }
                else if ((radioButton = control as TaskDialogRadioButton) != null)
                {
                    switch (propertyName)
                    {
                        case "Enabled":
                            this.nativeDialog.UpdateRadioButtonEnabled(radioButton.Id, radioButton.Enabled);
                            break;
                        default:
                            Debug.Assert(true, "Unknown property being set");
                            break;
                    }
                }
                else
                {
                    // Do nothing with property change -
                    // note that this shouldn't ever happen, we should have
                    // either thrown on the changing event, or we handle above.
                    Debug.Assert(true, "Control property changed notification not handled properly - being ignored");
                }
            }
        }

        #endregion

        #region Event Percolation Methods

        // All Raise*() methods are called by the
        // NativeTaskDialog when various pseudo-controls
        // are triggered.
        internal void RaiseButtonClickEvent(int id)
        {
            // First check to see if the ID matches a custom button.
            TaskDialogButtonBase button = this.GetButtonForId(id);

            // If a custom button was found,
            // raise the event - if not, it's a standard button, and
            // we don't support custom event handling for the standard buttons
            if (button != null)
            {
                button.RaiseClickEvent();
            }
        }

        internal void RaiseHyperlinkClickEvent(string link)
        {
            EventHandler<TaskDialogHyperlinkClickedEventArgs> handler = this.HyperlinkClick;
            if (handler != null)
            {
                handler(this, new TaskDialogHyperlinkClickedEventArgs(link));
            }
        }

        // Gives event subscriber a chance to prevent
        // the dialog from closing, based on
        // the current state of the app and the button
        // used to commit. Note that we don't
        // have full access at this stage to
        // the full dialog state.
        internal int RaiseClosingEvent(int id)
        {
            EventHandler<TaskDialogClosingEventArgs> handler = this.Closing;
            if (handler != null)
            {
                TaskDialogButtonBase customButton = null;
                TaskDialogClosingEventArgs e = new TaskDialogClosingEventArgs();

                // Try to identify the button - is it a standard one?
                TaskDialogStandardButtons buttonClicked = MapButtonIdToStandardButton(id);

                // If not, it had better be a custom button...
                if (buttonClicked == TaskDialogStandardButtons.None)
                {
                    customButton = this.GetButtonForId(id);

                    // ... or we have a problem.
                    if (customButton == null)
                    {
                        throw new InvalidOperationException(LocalizedMessages.TaskDialogBadButtonId);
                    }

                    e.CustomButton = customButton.Name;
                    e.TaskDialogResult = TaskDialogResult.CustomButtonClicked;
                }
                else
                {
                    e.TaskDialogResult = (TaskDialogResult)buttonClicked;
                }

                // Raise the event and determine how to proceed.
                handler(this, e);
                if (e.Cancel)
                {
                    return (int)HResult.False;
                }
            }

            // It's okay to let the dialog close.
            return (int)HResult.Ok;
        }

        internal void RaiseHelpInvokedEvent()
        {
            if (this.HelpInvoked != null)
            {
                this.HelpInvoked(this, EventArgs.Empty);
            }
        }

        internal void RaiseOpenedEvent()
        {
            if (this.Opened != null)
            {
                this.Opened(this, EventArgs.Empty);
            }
        }

        internal void RaiseTickEvent(int ticks)
        {
            if (this.Tick != null)
            {
                this.Tick(this, new TaskDialogTickEventArgs(ticks));
            }
        }

        #endregion

        #region Cleanup Code

        // Cleans up data and structs from a single
        // native dialog Show() invocation.
        private void CleanUp()
        {
            // Reset values that would be considered
            // 'volatile' in a given instance.
            if (this.progressBar != null)
            {
                this.progressBar.Reset();
            }

            // Clean out sorted control lists -
            // though we don't of course clear the main controls collection,
            // so the controls are still around; we'll
            // resort on next show, since the collection may have changed.
            if (this.buttons != null)
            {
                this.buttons.Clear();
            }

            if (this.commandLinks != null)
            {
                this.commandLinks.Clear();
            }

            if (this.radioButtons != null)
            {
                this.radioButtons.Clear();
            }

            this.progressBar = null;

            // Have the native dialog clean up the rest.
            if (this.nativeDialog != null)
            {
                this.nativeDialog.Dispose();
            }
        }

        // Dispose pattern - cleans up data and structs for
        // a) any native dialog currently showing, and
        // b) anything else that the outer TaskDialog has.
        private bool disposed;

        /// <summary>
        /// Dispose TaskDialog Resources
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// TaskDialog Finalizer
        /// </summary>
        ~TaskDialog()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose TaskDialog Resources
        /// </summary>
        /// <param name="disposing">If true, indicates that this is being called via Dispose rather than via the finalizer.</param>
        public void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    // Clean up managed resources.
                    if (this.nativeDialog != null && this.nativeDialog.ShowState == DialogShowState.Showing)
                    {
                        this.nativeDialog.NativeClose(TaskDialogResult.Cancel);
                    }

                    this.buttons = null;
                    this.radioButtons = null;
                    this.commandLinks = null;
                }

                // Clean up unmanaged resources SECOND, NTD counts on
                // being closed before being disposed.
                if (this.nativeDialog != null)
                {
                    this.nativeDialog.Dispose();
                    this.nativeDialog = null;
                }

                if (staticDialog != null)
                {
                    staticDialog.Dispose();
                    staticDialog = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Indicates whether this feature is supported on the current platform.
        /// </summary>
        public static bool IsPlatformSupported =>
                // We need Windows Vista onwards ...
                CoreHelpers.RunningOnVista;
    }
}
