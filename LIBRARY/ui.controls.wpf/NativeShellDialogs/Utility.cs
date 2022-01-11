namespace TMP.UI.Controls.WPF.NativeShellDialogs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal static class Utility
    {
        public static string[] GetFileNames(IShellItemArray items)
        {
            uint count;
            HResult hresult = items.GetCount(out count);
            if (hresult != HResult.Ok)
            {
                throw new Exception("IShellItemArray.GetCount failed. HResult: " + hresult); // TODO: Will this ever happen?
            }

            string[] fileNames = new string[count];

            for (int i = 0; i < count; i++)
            {
                IShellItem shellItem = Utility.GetShellItemAt(items, i);
                string fileName = Utility.GetFileNameFromShellItem(shellItem);

                fileNames[i] = fileName;
            }

            return fileNames;
        }

        private static readonly Guid ishellItem2Guid = new Guid(ShellIIDGuid.IShellItem2);

        public static IShellItem2 ParseShellItem2Name(string value)
        {
            Guid ishellItem2GuidCopy = ishellItem2Guid;

            IShellItem2 shellItem;
            HResult hresult = ShellNativeMethods.SHCreateItemFromParsingName(value, IntPtr.Zero, ref ishellItem2GuidCopy, out shellItem);
            if (hresult == HResult.Ok)
            {
                return shellItem;
            }
            else
            {
                // TODO: Handle HRESULT error codes?
                return null;
            }
        }

        public static HResult HResultFromWin32(int win32ErrorCode)
        {
            const int FacilityWin32 = 7;

            if (win32ErrorCode > 0)
            {
                win32ErrorCode = (int)(((uint)win32ErrorCode & 0x0000FFFF) | (FacilityWin32 << 16) | 0x80000000);
            }

            return (HResult)win32ErrorCode;
        }

        public static string GetFileNameFromShellItem(IShellItem item)
        {
            string filename = null;
            IntPtr pszString = IntPtr.Zero;
            HResult hr = item.GetDisplayName(ShellItemDesignNameOptions.DesktopAbsoluteParsing, out pszString);
            if (hr == HResult.Ok && pszString != IntPtr.Zero)
            {
                filename = Marshal.PtrToStringAuto(pszString);
                Marshal.FreeCoTaskMem(pszString);
            }

            return filename;
        }

        public static IShellItem GetShellItemAt(IShellItemArray array, int i)
        {
            IShellItem result;
            uint index = (uint)i;
            array.GetItemAt(index, out result);
            return result;
        }

        public static void SetFilters(IFileDialog dialog, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex)
        {
            if (filters == null || filters.Count == 0)
            {
                return;
            }

            FilterSpec[] specs = Utility.CreateFilterSpec(filters);
            dialog.SetFileTypes((uint)specs.Length, specs);

            if (selectedFilterZeroBasedIndex > -1 && selectedFilterZeroBasedIndex < filters.Count)
            {
                dialog.SetFileTypeIndex(1 + (uint)selectedFilterZeroBasedIndex); // In the COM interface (like the other Windows OFD APIs), filter indexes are 1-based, not 0-based.
            }
        }

        public static FilterSpec[] CreateFilterSpec(IReadOnlyCollection<Filter> filters)
        {
            FilterSpec[] specs = new FilterSpec[filters.Count];
            int i = 0;
            foreach (Filter filter in filters)
            {
                specs[i] = filter.ToFilterSpec();
                i++;
            }

            return specs;
        }
    }
}
