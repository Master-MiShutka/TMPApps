namespace TMP.UI.Controls.WPF.NativeShellDialogs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public static class FileOpenDialog
    {
        /// <summary>Shows the file open dialog for multiple filename selections. Returns null if the dialog cancelled. Otherwise returns all selected paths.</summary>
        /// <param name="selectedFilterIndex">0-based index of the filter to select.</param>
        public static string[] ShowMultiSelectDialog(IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex)
        {
            return ShowDialog(parentHWnd, title, initialDirectory, defaultFileName, filters, selectedFilterZeroBasedIndex, FileOpenOptions.AllowMultiSelect);
        }

        /// <summary>Shows the file open dialog for a single filename selection. Returns null if the dialog cancelled. Otherwise returns the selected path.</summary>
        /// <param name="selectedFilterIndex">0-based index of the filter to select.</param>
        public static string ShowSingleSelectDialog(IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex)
        {
            string[] fileNames = ShowDialog(parentHWnd, title, initialDirectory, defaultFileName, filters, selectedFilterZeroBasedIndex, FileOpenOptions.None);
            if (fileNames != null && fileNames.Length > 0)
            {
                return fileNames[0];
            }
            else
            {
                return null;
            }
        }

        private static string[] ShowDialog(IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex, FileOpenOptions flags)
        {
            NativeFileOpenDialog nfod = new NativeFileOpenDialog();
            try
            {
                return ShowDialogInner(nfod, parentHWnd, title, initialDirectory, defaultFileName, filters, selectedFilterZeroBasedIndex, flags);
            }
            finally
            {
                Marshal.ReleaseComObject(nfod);
            }
        }

        private static string[] ShowDialogInner(IFileOpenDialog dialog, IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex, FileOpenOptions flags)
        {
            flags = flags |
                FileOpenOptions.NoTestFileCreate |
                FileOpenOptions.PathMustExist |
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
                IShellItemArray resultsArray;
                dialog.GetResults(out resultsArray);

                string[] fileNames = Utility.GetFileNames(resultsArray);
                return fileNames;
            }
        }
    }
}
