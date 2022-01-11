// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Represents the base class for all types of Shell "containers". Any class deriving from this class
    /// can contain other ShellObjects (e.g. ShellFolder, FileSystemKnownFolder, ShellLibrary, etc)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This will complicate the class hierarchy and naming convention used in the Shell area")]
    public abstract class ShellContainer : ShellObject, IEnumerable<ShellObject>, IDisposable
    {

        #region Private Fields

        private IShellFolder desktopFolderEnumeration;
        private IShellFolder nativeShellFolder;

        #endregion

        #region Internal Properties

        internal IShellFolder NativeShellFolder
        {
            get
            {
                if (this.nativeShellFolder == null)
                {
                    Guid guid = new Guid(ShellIIDGuid.IShellFolder);
                    Guid handler = new Guid(ShellBHIDGuid.ShellFolderObject);

                    HResult hr = this.NativeShellItem.BindToHandler(
                        IntPtr.Zero, ref handler, ref guid, out this.nativeShellFolder);

                    if (CoreErrorHelper.Failed(hr))
                    {
                        string str = ShellHelper.GetParsingName(this.NativeShellItem);
                        if (str != null && str != Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                        {
                            throw new ShellException(hr);
                        }
                    }
                }

                return this.nativeShellFolder;
            }
        }

        #endregion

        #region Internal Constructor

        internal ShellContainer()
        {
        }

        internal ShellContainer(IShellItem2 shellItem) : base(shellItem)
        {
        }

        #endregion

        #region Disposable Pattern

        /// <summary>
        /// Release resources
        /// </summary>
        /// <param name="disposing"><B>True</B> indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (this.nativeShellFolder != null)
            {
                Marshal.ReleaseComObject(this.nativeShellFolder);
                this.nativeShellFolder = null;
            }

            if (this.desktopFolderEnumeration != null)
            {
                Marshal.ReleaseComObject(this.desktopFolderEnumeration);
                this.desktopFolderEnumeration = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region IEnumerable<ShellObject> Members

        /// <summary>
        /// Enumerates through contents of the ShellObjectContainer
        /// </summary>
        /// <returns>Enumerated contents</returns>
        public IEnumerator<ShellObject> GetEnumerator()
        {
            if (this.NativeShellFolder == null)
            {
                if (this.desktopFolderEnumeration == null)
                {
                    ShellNativeMethods.SHGetDesktopFolder(out this.desktopFolderEnumeration);
                }

                this.nativeShellFolder = this.desktopFolderEnumeration;
            }

            return new ShellFolderItems(this);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ShellFolderItems(this);
        }

        #endregion
    }
}
