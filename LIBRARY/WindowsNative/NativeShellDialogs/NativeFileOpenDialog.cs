namespace WindowsNative
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
