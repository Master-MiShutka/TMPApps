﻿// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using Microsoft.WindowsAPICodePack.Resources;

    internal static class DialogsDefaults
    {
        internal static string Caption => LocalizedMessages.DialogDefaultCaption;

        internal static string MainInstruction => LocalizedMessages.DialogDefaultMainInstruction;

        internal static string Content => LocalizedMessages.DialogDefaultContent;

        internal const int ProgressBarStartingValue = 0;
        internal const int ProgressBarMinimumValue = 0;
        internal const int ProgressBarMaximumValue = 100;

        internal const int IdealWidth = 0;

        // For generating control ID numbers that won't
        // collide with the standard button return IDs.
        internal const int MinimumDialogControlId =
            (int)TaskDialogNativeMethods.TaskDialogCommonButtonReturnIds.Close + 1;
    }
}
