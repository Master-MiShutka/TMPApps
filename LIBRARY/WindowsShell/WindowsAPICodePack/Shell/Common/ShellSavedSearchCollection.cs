// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Represents a saved search
    /// </summary>
    public class ShellSavedSearchCollection : ShellSearchCollection
    {
        internal ShellSavedSearchCollection(IShellItem2 shellItem)
            : base(shellItem)
        {
            CoreHelpers.ThrowIfNotVista();
        }
    }
}
