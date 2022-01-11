// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace MS.WindowsAPICodePack.Internal
{
    using System.Security.Permissions;

    /// <summary>
    /// Safe Region Handle
    /// </summary>
    public class SafeRegionHandle : ZeroInvalidHandle
    {
        /// <summary>
        /// Release the handle
        /// </summary>
        /// <returns>true if handled is release successfully, false otherwise</returns>
        protected override bool ReleaseHandle()
        {
            if (CoreNativeMethods.DeleteObject(this.handle))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
