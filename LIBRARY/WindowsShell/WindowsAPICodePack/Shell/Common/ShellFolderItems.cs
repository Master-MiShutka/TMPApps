// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using MS.WindowsAPICodePack.Internal;

    internal class ShellFolderItems : IEnumerator<ShellObject>
    {
        #region Private Fields

        private IEnumIDList nativeEnumIdList;
        private ShellObject currentItem;
        private ShellContainer nativeShellFolder;

        #endregion

        #region Internal Constructor

        internal ShellFolderItems(ShellContainer nativeShellFolder)
        {
            this.nativeShellFolder = nativeShellFolder;

            HResult hr = nativeShellFolder.NativeShellFolder.EnumObjects(
                IntPtr.Zero,
                ShellNativeMethods.ShellFolderEnumerationOptions.Folders | ShellNativeMethods.ShellFolderEnumerationOptions.NonFolders,
                out this.nativeEnumIdList);

            if (!CoreErrorHelper.Succeeded(hr))
            {
                if (hr == HResult.Canceled)
                {
                    throw new System.IO.FileNotFoundException();
                }
                else
                {
                    throw new ShellException(hr);
                }
            }
        }

        #endregion

        #region IEnumerator<ShellObject> Members

        public ShellObject Current => this.currentItem;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this.nativeEnumIdList != null)
            {
                Marshal.ReleaseComObject(this.nativeEnumIdList);
                this.nativeEnumIdList = null;
            }
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current => this.currentItem;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (this.nativeEnumIdList == null)
            {
                return false;
            }

            IntPtr item;
            uint numItemsReturned;
            uint itemsRequested = 1;
            HResult hr = this.nativeEnumIdList.Next(itemsRequested, out item, out numItemsReturned);

            if (numItemsReturned < itemsRequested || hr != HResult.Ok)
            {
                return false;
            }

            this.currentItem = ShellObjectFactory.Create(item, this.nativeShellFolder);

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        public void Reset()
        {
            if (this.nativeEnumIdList != null)
            {
                this.nativeEnumIdList.Reset();
            }
        }

        #endregion
    }
}
