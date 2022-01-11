﻿namespace TMP.UI.Controls.WPF.NativeShellDialogs
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport]
    [Guid(ShellIIDGuid.IFileSaveDialog)]
    [CoClass(typeof(FileSaveDialogRCW))]
    internal interface NativeFileSaveDialog : IFileSaveDialog
    {
    }
}
