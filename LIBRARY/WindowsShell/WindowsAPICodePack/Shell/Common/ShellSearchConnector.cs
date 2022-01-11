// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// A Serch Connector folder in the Shell Namespace
    /// </summary>
    public sealed class ShellSearchConnector : ShellSearchCollection
    {

        #region Internal Constructor

        internal ShellSearchConnector()
        {
            CoreHelpers.ThrowIfNotWin7();
        }

        internal ShellSearchConnector(IShellItem2 shellItem)
            : this()
        {
            this.nativeShellItem = shellItem;
        }

        #endregion

        /// <summary>
        /// Indicates whether this feature is supported on the current platform.
        /// </summary>
        new public static bool IsPlatformSupported =>
                // We need Windows 7 onwards ...
                CoreHelpers.RunningOnWin7;
    }
}
