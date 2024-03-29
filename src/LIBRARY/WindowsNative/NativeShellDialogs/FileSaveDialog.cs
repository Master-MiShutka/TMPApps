﻿namespace WindowsNative
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public static class FileSaveDialog
    {
        /// <param name="selectedFilterIndex">0-based index of the filter to select.</param>
        public static string ShowDialog(IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex = -1)
        {
            NativeFileSaveDialog nfod = new NativeFileSaveDialog();
            try
            {
                return ShowDialogInner(nfod, parentHWnd, title, initialDirectory, defaultFileName, filters);
            }
            finally
            {
                Marshal.ReleaseComObject(nfod);
            }
        }

        private static string ShowDialogInner(IFileSaveDialog dialog, IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex = -1)
        {
            FileOpenOptions flags =
                FileOpenOptions.NoTestFileCreate |
                FileOpenOptions.PathMustExist |
                FileOpenOptions.ForceFilesystem |
                FileOpenOptions.OverwritePrompt;

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

            // if( initialSaveAsItem != null )
            // {
            // IShellItem2 initialSaveAsItemShellItem = Utility.ParseShellItem2Name( initialDirectory );
            // if( initialSaveAsItemShellItem != null )
            // {
            // dialog.SetSaveAsItem( initialSaveAsItemShellItem );
            // }
            // }
            if (defaultFileName != null)
            {
                dialog.SetFileName(defaultFileName);
            }

            Utility.SetFilters(dialog, filters, selectedFilterZeroBasedIndex);

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
                IShellItem selectedItem;
                dialog.GetResult(out selectedItem);

                if (selectedItem != null)
                {
                    return Utility.GetFileNameFromShellItem(selectedItem);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
