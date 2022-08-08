// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    ///<summary>
    /// Encapsulates additional configuration needed by NativeTaskDialog
    /// that it can't get from the TASKDIALOGCONFIG struct.
    ///</summary>
    internal class NativeTaskDialogSettings
    {
        internal NativeTaskDialogSettings()
        {
            this.NativeConfiguration = new TaskDialogNativeMethods.TaskDialogConfiguration();

            // Apply standard settings.
            this.NativeConfiguration.size = (uint)Marshal.SizeOf(this.NativeConfiguration);
            this.NativeConfiguration.parentHandle = IntPtr.Zero;
            this.NativeConfiguration.instance = IntPtr.Zero;
            this.NativeConfiguration.taskDialogFlags = TaskDialogNativeMethods.TaskDialogOptions.AllowCancel;
            this.NativeConfiguration.commonButtons = TaskDialogNativeMethods.TaskDialogCommonButtons.Ok;
            this.NativeConfiguration.mainIcon = new TaskDialogNativeMethods.IconUnion(0);
            this.NativeConfiguration.footerIcon = new TaskDialogNativeMethods.IconUnion(0);
            this.NativeConfiguration.width = TaskDialogDefaults.IdealWidth;

            // Zero out all the custom button fields.
            this.NativeConfiguration.buttonCount = 0;
            this.NativeConfiguration.radioButtonCount = 0;
            this.NativeConfiguration.buttons = IntPtr.Zero;
            this.NativeConfiguration.radioButtons = IntPtr.Zero;
            this.NativeConfiguration.defaultButtonIndex = 0;
            this.NativeConfiguration.defaultRadioButtonIndex = 0;

            // Various text defaults.
            this.NativeConfiguration.windowTitle = TaskDialogDefaults.Caption;
            this.NativeConfiguration.mainInstruction = TaskDialogDefaults.MainInstruction;
            this.NativeConfiguration.content = TaskDialogDefaults.Content;
            this.NativeConfiguration.verificationText = null;
            this.NativeConfiguration.expandedInformation = null;
            this.NativeConfiguration.expandedControlText = null;
            this.NativeConfiguration.collapsedControlText = null;
            this.NativeConfiguration.footerText = null;
        }

        public int ProgressBarMinimum { get; set; }

        public int ProgressBarMaximum { get; set; }

        public int ProgressBarValue { get; set; }

        public TaskDialogProgressBarState ProgressBarState { get; set; }

        public bool InvokeHelp { get; set; }

        public TaskDialogNativeMethods.TaskDialogConfiguration NativeConfiguration { get; private set; }

        public TaskDialogNativeMethods.TaskDialogButton[] Buttons { get; set; }

        public TaskDialogNativeMethods.TaskDialogButton[] RadioButtons { get; set; }

        public List<int> ElevatedButtons { get; set; }
    }
}
