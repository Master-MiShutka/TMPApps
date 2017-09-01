using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;

namespace Native
{
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeLibraryHandle()
            : base(true)
        { }

        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.FreeLibrary(handle);
        }
    }
}
