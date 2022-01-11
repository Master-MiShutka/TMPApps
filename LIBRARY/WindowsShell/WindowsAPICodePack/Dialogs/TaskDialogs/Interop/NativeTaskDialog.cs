// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Microsoft.WindowsAPICodePack.Resources;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Encapsulates the native logic required to create,
    /// configure, and show a TaskDialog,
    /// via the TaskDialogIndirect() Win32 function.
    /// </summary>
    /// <remarks>A new instance of this class should
    /// be created for each messagebox show, as
    /// the HWNDs for TaskDialogs do not remain constant
    /// across calls to TaskDialogIndirect.
    /// </remarks>
    internal class NativeTaskDialog : IDisposable
    {
        private TaskDialogNativeMethods.TaskDialogConfiguration nativeDialogConfig;
        private NativeTaskDialogSettings settings;
        private IntPtr hWndDialog;
        private TaskDialog outerDialog;

        private IntPtr[] updatedStrings = new IntPtr[Enum.GetNames(typeof(TaskDialogNativeMethods.TaskDialogElements)).Length];
        private IntPtr buttonArray;
        private IntPtr radioButtonArray;

        // Flag tracks whether our first radio
        // button click event has come through.
        private bool firstRadioButtonClicked = true;

        #region Constructors

        // Configuration is applied at dialog creation time.
        internal NativeTaskDialog(NativeTaskDialogSettings settings, TaskDialog outerDialog)
        {
            this.nativeDialogConfig = settings.NativeConfiguration;
            this.settings = settings;

            // Wireup dialog proc message loop for this instance.
            this.nativeDialogConfig.callback = new TaskDialogNativeMethods.TaskDialogCallback(this.DialogProc);

            this.ShowState = DialogShowState.PreShow;

            // Keep a reference to the outer shell, so we can notify.
            this.outerDialog = outerDialog;
        }

        #endregion

        #region Public Properties

        public DialogShowState ShowState { get; private set; }

        public int SelectedButtonId { get; private set; }

        public int SelectedRadioButtonId { get; private set; }

        public bool CheckBoxChecked { get; private set; }

        #endregion

        internal void NativeShow()
        {
            // Applies config struct and other settings, then
            // calls main Win32 function.
            if (this.settings == null)
            {
                throw new InvalidOperationException(LocalizedMessages.NativeTaskDialogConfigurationError);
            }

            // Do a last-minute parse of the various dialog control lists,
            // and only allocate the memory at the last minute.
            this.MarshalDialogControlStructs();

            // Make the call and show the dialog.
            // NOTE: this call is BLOCKING, though the thread
            // WILL re-enter via the DialogProc.
            try
            {
                this.ShowState = DialogShowState.Showing;

                int selectedButtonId;
                int selectedRadioButtonId;
                bool checkBoxChecked;

                // Here is the way we use "vanilla" P/Invoke to call TaskDialogIndirect().
                HResult hresult = TaskDialogNativeMethods.TaskDialogIndirect(
                    this.nativeDialogConfig,
                    out selectedButtonId,
                    out selectedRadioButtonId,
                    out checkBoxChecked);

                if (CoreErrorHelper.Failed(hresult))
                {
                    string msg;
                    switch (hresult)
                    {
                        case HResult.InvalidArguments:
                            msg = LocalizedMessages.NativeTaskDialogInternalErrorArgs;
                            break;
                        case HResult.OutOfMemory:
                            msg = LocalizedMessages.NativeTaskDialogInternalErrorComplex;
                            break;
                        default:
                            msg = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                LocalizedMessages.NativeTaskDialogInternalErrorUnexpected,
                                hresult);
                            break;
                    }

                    Exception e = Marshal.GetExceptionForHR((int)hresult);
                    throw new Win32Exception(msg, e);
                }

                this.SelectedButtonId = selectedButtonId;
                this.SelectedRadioButtonId = selectedRadioButtonId;
                this.CheckBoxChecked = checkBoxChecked;
            }
            catch (EntryPointNotFoundException exc)
            {
                throw new NotSupportedException(LocalizedMessages.NativeTaskDialogVersionError, exc);
            }
            finally
            {
                this.ShowState = DialogShowState.Closed;
            }
        }

        // The new task dialog does not support the existing
        // Win32 functions for closing (e.g. EndDialog()); instead,
        // a "click button" message is sent. In this case, we're
        // abstracting out to say that the TaskDialog consumer can
        // simply call "Close" and we'll "click" the cancel button.
        // Note that the cancel button doesn't actually
        // have to exist for this to work.
        internal void NativeClose(TaskDialogResult result)
        {
            this.ShowState = DialogShowState.Closing;

            int id;
            switch (result)
            {
                case TaskDialogResult.Close:
                    id = (int)TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Close;
                    break;
                case TaskDialogResult.CustomButtonClicked:
                    id = DialogsDefaults.MinimumDialogControlId; // custom buttons
                    break;
                case TaskDialogResult.No:
                    id = (int)TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.No;
                    break;
                case TaskDialogResult.Ok:
                    id = (int)TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Ok;
                    break;
                case TaskDialogResult.Retry:
                    id = (int)TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Retry;
                    break;
                case TaskDialogResult.Yes:
                    id = (int)TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Yes;
                    break;
                default:
                    id = (int)TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Cancel;
                    break;
            }

            this.SendMessageHelper(TaskDialogNativeMethods.TaskDialogMessages.ClickButton, id, 0);
        }

        #region Main Dialog Proc

        private int DialogProc(
            IntPtr windowHandle,
            uint message,
            IntPtr wparam,
            IntPtr lparam,
            IntPtr referenceData)
        {
            // Fetch the HWND - it may be the first time we're getting it.
            this.hWndDialog = windowHandle;

            // Big switch on the various notifications the
            // dialog proc can get.
            switch ((TaskDialogNativeMethods.TaskDialogNotifications)message)
            {
                case TaskDialogNativeMethods.TaskDialogNotifications.Created:
                    int result = this.PerformDialogInitialization();
                    this.outerDialog.RaiseOpenedEvent();
                    return result;
                case TaskDialogNativeMethods.TaskDialogNotifications.ButtonClicked:
                    return this.HandleButtonClick((int)wparam);
                case TaskDialogNativeMethods.TaskDialogNotifications.RadioButtonClicked:
                    return this.HandleRadioButtonClick((int)wparam);
                case TaskDialogNativeMethods.TaskDialogNotifications.HyperlinkClicked:
                    return this.HandleHyperlinkClick(lparam);
                case TaskDialogNativeMethods.TaskDialogNotifications.Help:
                    return this.HandleHelpInvocation();
                case TaskDialogNativeMethods.TaskDialogNotifications.Timer:
                    return this.HandleTick((int)wparam);
                case TaskDialogNativeMethods.TaskDialogNotifications.Destroyed:
                    return this.PerformDialogCleanup();
                default:
                    break;
            }

            return (int)HResult.Ok;
        }

        // Once the task dialog HWND is open, we need to send
        // additional messages to configure it.
        private int PerformDialogInitialization()
        {
            // Initialize Progress or Marquee Bar.
            if (this.IsOptionSet(TaskDialogNativeMethods.TaskDialogOptions.ShowProgressBar))
            {
                this.UpdateProgressBarRange();

                // The order of the following is important -
                // state is more important than value,
                // and non-normal states turn off the bar value change
                // animation, which is likely the intended
                // and preferable behavior.
                this.UpdateProgressBarState(this.settings.ProgressBarState);
                this.UpdateProgressBarValue(this.settings.ProgressBarValue);

                // Due to a bug that wasn't fixed in time for RTM of Vista,
                // second SendMessage is required if the state is non-Normal.
                this.UpdateProgressBarValue(this.settings.ProgressBarValue);
            }
            else if (this.IsOptionSet(TaskDialogNativeMethods.TaskDialogOptions.ShowMarqueeProgressBar))
            {
                // TDM_SET_PROGRESS_BAR_MARQUEE is necessary
                // to cause the marquee to start animating.
                // Note that this internal task dialog setting is
                // round-tripped when the marquee is
                // is set to different states, so it never has to
                // be touched/sent again.
                this.SendMessageHelper(TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarMarquee, 1, 0);
                this.UpdateProgressBarState(this.settings.ProgressBarState);
            }

            if (this.settings.ElevatedButtons != null && this.settings.ElevatedButtons.Count > 0)
            {
                foreach (int id in this.settings.ElevatedButtons)
                {
                    this.UpdateElevationIcon(id, true);
                }
            }

            return CoreErrorHelper.Ignored;
        }

        private int HandleButtonClick(int id)
        {
            // First we raise a Click event, if there is a custom button
            // However, we implement Close() by sending a cancel button, so
            // we don't want to raise a click event in response to that.
            if (this.ShowState != DialogShowState.Closing)
            {
                this.outerDialog.RaiseButtonClickEvent(id);
            }

            // Once that returns, we raise a Closing event for the dialog
            // The Win32 API handles button clicking-and-closing
            // as an atomic action,
            // but it is more .NET friendly to split them up.
            // Unfortunately, we do NOT have the return values at this stage.
            if (id < DialogsDefaults.MinimumDialogControlId)
            {
                return this.outerDialog.RaiseClosingEvent(id);
            }

            return (int)HResult.False;
        }

        private int HandleRadioButtonClick(int id)
        {
            // When the dialog sets the radio button to default,
            // it (somewhat confusingly)issues a radio button clicked event
            //  - we mask that out - though ONLY if
            // we do have a default radio button
            if (this.firstRadioButtonClicked
                && !this.IsOptionSet(TaskDialogNativeMethods.TaskDialogOptions.NoDefaultRadioButton))
            {
                this.firstRadioButtonClicked = false;
            }
            else
            {
                this.outerDialog.RaiseButtonClickEvent(id);
            }

            // Note: we don't raise Closing, as radio
            // buttons are non-committing buttons
            return CoreErrorHelper.Ignored;
        }

        private int HandleHyperlinkClick(IntPtr href)
        {
            string link = Marshal.PtrToStringUni(href);
            this.outerDialog.RaiseHyperlinkClickEvent(link);

            return CoreErrorHelper.Ignored;
        }

        private int HandleTick(int ticks)
        {
            this.outerDialog.RaiseTickEvent(ticks);
            return CoreErrorHelper.Ignored;
        }

        private int HandleHelpInvocation()
        {
            this.outerDialog.RaiseHelpInvokedEvent();
            return CoreErrorHelper.Ignored;
        }

        // There should be little we need to do here,
        // as the use of the NativeTaskDialog is
        // that it is instantiated for a single show, then disposed of.
        private int PerformDialogCleanup()
        {
            this.firstRadioButtonClicked = true;

            return CoreErrorHelper.Ignored;
        }

        #endregion

        #region Update members

        internal void UpdateProgressBarValue(int i)
        {
            this.AssertCurrentlyShowing();
            this.SendMessageHelper(TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarPosition, i, 0);
        }

        internal void UpdateProgressBarRange()
        {
            this.AssertCurrentlyShowing();

            // Build range LPARAM - note it is in REVERSE intuitive order.
            long range = NativeTaskDialog.MakeLongLParam(
                this.settings.ProgressBarMaximum,
                this.settings.ProgressBarMinimum);

            this.SendMessageHelper(TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarRange, 0, range);
        }

        internal void UpdateProgressBarState(TaskDialogProgressBarState state)
        {
            this.AssertCurrentlyShowing();
            this.SendMessageHelper(TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarState, (int)state, 0);
        }

        internal void UpdateText(string text)
        {
            this.UpdateTextCore(text, TaskDialogNativeMethods.TaskDialogElements.Content);
        }

        internal void UpdateInstruction(string instruction)
        {
            this.UpdateTextCore(instruction, TaskDialogNativeMethods.TaskDialogElements.MainInstruction);
        }

        internal void UpdateFooterText(string footerText)
        {
            this.UpdateTextCore(footerText, TaskDialogNativeMethods.TaskDialogElements.Footer);
        }

        internal void UpdateExpandedText(string expandedText)
        {
            this.UpdateTextCore(expandedText, TaskDialogNativeMethods.TaskDialogElements.ExpandedInformation);
        }

        private void UpdateTextCore(string s, TaskDialogNativeMethods.TaskDialogElements element)
        {
            this.AssertCurrentlyShowing();

            this.FreeOldString(element);
            this.SendMessageHelper(
                TaskDialogNativeMethods.TaskDialogMessages.SetElementText,
                (int)element,
                (long)this.MakeNewString(s, element));
        }

        internal void UpdateMainIcon(TaskDialogStandardIcon mainIcon)
        {
            this.UpdateIconCore(mainIcon, TaskDialogNativeMethods.TaskDialogIconElement.Main);
        }

        internal void UpdateFooterIcon(TaskDialogStandardIcon footerIcon)
        {
            this.UpdateIconCore(footerIcon, TaskDialogNativeMethods.TaskDialogIconElement.Footer);
        }

        private void UpdateIconCore(TaskDialogStandardIcon icon, TaskDialogNativeMethods.TaskDialogIconElement element)
        {
            this.AssertCurrentlyShowing();
            this.SendMessageHelper(
                TaskDialogNativeMethods.TaskDialogMessages.UpdateIcon,
                (int)element,
                (long)icon);
        }

        internal void UpdateCheckBoxChecked(bool cbc)
        {
            this.AssertCurrentlyShowing();
            this.SendMessageHelper(
                TaskDialogNativeMethods.TaskDialogMessages.ClickVerification,
                cbc ? 1 : 0,
                1);
        }

        internal void UpdateElevationIcon(int buttonId, bool showIcon)
        {
            this.AssertCurrentlyShowing();
            this.SendMessageHelper(
                TaskDialogNativeMethods.TaskDialogMessages.SetButtonElevationRequiredState,
                buttonId,
                Convert.ToInt32(showIcon));
        }

        internal void UpdateButtonEnabled(int buttonID, bool enabled)
        {
            this.AssertCurrentlyShowing();
            this.SendMessageHelper(
                TaskDialogNativeMethods.TaskDialogMessages.EnableButton, buttonID, enabled == true ? 1 : 0);
        }

        internal void UpdateRadioButtonEnabled(int buttonID, bool enabled)
        {
            this.AssertCurrentlyShowing();
            this.SendMessageHelper(TaskDialogNativeMethods.TaskDialogMessages.EnableRadioButton,
                buttonID, enabled == true ? 1 : 0);
        }

        internal void AssertCurrentlyShowing()
        {
            Debug.Assert(this.ShowState == DialogShowState.Showing,
                "Update*() methods should only be called while native dialog is showing");
        }

        #endregion

        #region Helpers

        private int SendMessageHelper(TaskDialogNativeMethods.TaskDialogMessages message, int wparam, long lparam)
        {
            // Be sure to at least assert here -
            // messages to invalid handles often just disappear silently
            Debug.Assert(this.hWndDialog != null, "HWND for dialog is null during SendMessage");

            return (int)CoreNativeMethods.SendMessage(
                this.hWndDialog,
                (uint)message,
                (IntPtr)wparam,
                new IntPtr(lparam));
        }

        private bool IsOptionSet(TaskDialogNativeMethods.TaskDialogOptions flag)
        {
            return (this.nativeDialogConfig.taskDialogFlags & flag) == flag;
        }

        // Allocates a new string on the unmanaged heap,
        // and stores the pointer so we can free it later.
        private IntPtr MakeNewString(string text, TaskDialogNativeMethods.TaskDialogElements element)
        {
            IntPtr newStringPtr = Marshal.StringToHGlobalUni(text);
            this.updatedStrings[(int)element] = newStringPtr;
            return newStringPtr;
        }

        // Checks to see if the given element already has an
        // updated string, and if so,
        // frees it. This is done in preparation for a call to
        // MakeNewString(), to prevent
        // leaks from multiple updates calls on the same element
        // within a single native dialog lifetime.
        private void FreeOldString(TaskDialogNativeMethods.TaskDialogElements element)
        {
            int elementIndex = (int)element;
            if (this.updatedStrings[elementIndex] != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.updatedStrings[elementIndex]);
                this.updatedStrings[elementIndex] = IntPtr.Zero;
            }
        }

        // Based on the following defines in WinDef.h and WinUser.h:
        // #define MAKELPARAM(l, h) ((LPARAM)(DWORD)MAKELONG(l, h))
        // #define MAKELONG(a, b) ((LONG)(((WORD)(((DWORD_PTR)(a)) & 0xffff)) | ((DWORD)((WORD)(((DWORD_PTR)(b)) & 0xffff))) << 16))
        private static long MakeLongLParam(int a, int b)
        {
            return (a << 16) + b;
        }

        // Builds the actual configuration that the
        // NativeTaskDialog (and underlying Win32 API)
        // expects, by parsing the various control lists,
        // marshaling to the unmanaged heap, etc.
        private void MarshalDialogControlStructs()
        {
            if (this.settings.Buttons != null && this.settings.Buttons.Length > 0)
            {
                this.buttonArray = AllocateAndMarshalButtons(this.settings.Buttons);
                this.settings.NativeConfiguration.buttons = this.buttonArray;
                this.settings.NativeConfiguration.buttonCount = (uint)this.settings.Buttons.Length;
            }

            if (this.settings.RadioButtons != null && this.settings.RadioButtons.Length > 0)
            {
                this.radioButtonArray = AllocateAndMarshalButtons(this.settings.RadioButtons);
                this.settings.NativeConfiguration.radioButtons = this.radioButtonArray;
                this.settings.NativeConfiguration.radioButtonCount = (uint)this.settings.RadioButtons.Length;
            }
        }

        private static IntPtr AllocateAndMarshalButtons(TaskDialogNativeMethods.TaskDialogButton[] structs)
        {
            IntPtr initialPtr = Marshal.AllocHGlobal(
                Marshal.SizeOf(typeof(TaskDialogNativeMethods.TaskDialogButton)) * structs.Length);

            IntPtr currentPtr = initialPtr;
            foreach (TaskDialogNativeMethods.TaskDialogButton button in structs)
            {
                Marshal.StructureToPtr(button, currentPtr, false);
                currentPtr = (IntPtr)((int)currentPtr + Marshal.SizeOf(button));
            }

            return initialPtr;
        }

        #endregion

        #region IDispose Pattern

        private bool disposed;

        // Finalizer and IDisposable implementation.
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NativeTaskDialog()
        {
            this.Dispose(false);
        }

        // Core disposing logic.
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                // Single biggest resource - make sure the dialog
                // itself has been instructed to close.
                if (this.ShowState == DialogShowState.Showing)
                {
                    this.NativeClose(TaskDialogResult.Cancel);
                }

                // Clean up custom allocated strings that were updated
                // while the dialog was showing. Note that the strings
                // passed in the initial TaskDialogIndirect call will
                // be cleaned up automagically by the default
                // marshalling logic.
                if (this.updatedStrings != null)
                {
                    for (int i = 0; i < this.updatedStrings.Length; i++)
                    {
                        if (this.updatedStrings[i] != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(this.updatedStrings[i]);
                            this.updatedStrings[i] = IntPtr.Zero;
                        }
                    }
                }

                // Clean up the button and radio button arrays, if any.
                if (this.buttonArray != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.buttonArray);
                    this.buttonArray = IntPtr.Zero;
                }

                if (this.radioButtonArray != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.radioButtonArray);
                    this.radioButtonArray = IntPtr.Zero;
                }

                if (disposing)
                {
                    // Clean up managed resources - currently there are none
                    // that are interesting.
                }
            }
        }

        #endregion
    }
}
