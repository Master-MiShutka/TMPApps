namespace TMP.UI.Controls.WPF.NativeShellDialogs
{
    using System;
    using System.Runtime.InteropServices;

    public static class FolderBrowserDialog
    {
        public static string ShowDialog(System.Windows.Window parentWindow, string title, string initialDirectory)
        {
            IntPtr parentHWnd = new System.Windows.Interop.WindowInteropHelper(parentWindow).Handle;

            return ShowDialog(parentHWnd, title, initialDirectory);
        }

        /// <summary>Shows the folder browser dialog. Returns null if the dialog cancelled. Otherwise returns the selected path.</summary>
        public static string ShowDialog(IntPtr parentHWnd, string title, string initialDirectory)
        {
            NativeFileOpenDialog nfod = new NativeFileOpenDialog();
            try
            {
                return ShowDialogInner(nfod, parentHWnd, title, initialDirectory);
            }
            finally
            {
                Marshal.ReleaseComObject(nfod);
            }
        }

        private static string ShowDialogInner(IFileOpenDialog dialog, IntPtr parentHWnd, string title, string initialDirectory)
        {
            // IFileDialog ifd = dialog;
            FileOpenOptions flags =
                FileOpenOptions.NoTestFileCreate |
                FileOpenOptions.PathMustExist |
                FileOpenOptions.PickFolders |
                FileOpenOptions.ForceFilesystem;

            dialog.SetOptions(flags);

            if (title != null)
            {
                dialog.SetTitle(title);
            }

            if (initialDirectory != null)
            {
                IShellItem2 initialDirectoryShellItem = Utility.ParseShellItem2Name(initialDirectory);
                if (initialDirectoryShellItem != null)
                {
                    dialog.SetFolder(initialDirectoryShellItem);
                }
            }

            HResult result = dialog.Show(parentHWnd);

            HResult cancelledAsHResult = Utility.HResultFromWin32((int)HResult.Win32ErrorCanceled);
            if (result == cancelledAsHResult)
            {
                // Cancelled
                return null;
            }
            else
            {
                // OK
                IShellItemArray resultsArray;
                dialog.GetResults(out resultsArray);

                string[] fileNames = Utility.GetFileNames(resultsArray);
                return fileNames.Length == 0 ? null : fileNames[0];
            }
        }
    }
}
