namespace TMP.UI.Controls.WPF.NativeShellDialogs
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport]
    [Guid(ShellIIDGuid.IFileOpenDialog)]
    [CoClass(typeof(FileOpenDialogRCW))]
    internal interface NativeFileOpenDialog : IFileOpenDialog
    {
    }
}
