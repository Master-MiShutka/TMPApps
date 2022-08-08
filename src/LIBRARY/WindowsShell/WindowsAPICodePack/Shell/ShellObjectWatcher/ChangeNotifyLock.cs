namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.WindowsAPICodePack.Shell.Interop;
    using MS.WindowsAPICodePack.Internal;

    internal class ChangeNotifyLock
    {
        private uint @event = 0;

        internal ChangeNotifyLock(Message message)
        {
            IntPtr pidl;
            IntPtr lockId = ShellNativeMethods.SHChangeNotification_Lock(
                    message.WParam, (int)message.LParam, out pidl, out this.@event);
            try
            {
                Trace.TraceInformation("Message: {0}", (ShellObjectChangeTypes)this.@event);

                var notifyStruct = pidl.MarshalAs<ShellNativeMethods.ShellNotifyStruct>();

                Guid guid = new Guid(ShellIIDGuid.IShellItem2);
                if (notifyStruct.item1 != IntPtr.Zero &&
                    (((ShellObjectChangeTypes)this.@event) & ShellObjectChangeTypes.SystemImageUpdate) == ShellObjectChangeTypes.None)
                {
                    IShellItem2 nativeShellItem;
                    if (CoreErrorHelper.Succeeded(ShellNativeMethods.SHCreateItemFromIDList(
                        notifyStruct.item1, ref guid, out nativeShellItem)))
                    {
                        string name;
                        nativeShellItem.GetDisplayName(ShellNativeMethods.ShellItemDesignNameOptions.FileSystemPath,
                            out name);
                        this.ItemName = name;

                        Trace.TraceInformation("Item1: {0}", this.ItemName);
                    }
                }
                else
                {
                    this.ImageIndex = notifyStruct.item1.ToInt32();
                }

                if (notifyStruct.item2 != IntPtr.Zero)
                {
                    IShellItem2 nativeShellItem;
                    if (CoreErrorHelper.Succeeded(ShellNativeMethods.SHCreateItemFromIDList(
                        notifyStruct.item2, ref guid, out nativeShellItem)))
                    {
                        string name;
                        nativeShellItem.GetDisplayName(ShellNativeMethods.ShellItemDesignNameOptions.FileSystemPath,
                            out name);
                        this.ItemName2 = name;

                        Trace.TraceInformation("Item2: {0}", this.ItemName2);
                    }
                }
            }
            finally
            {
                if (lockId != IntPtr.Zero)
                {
                    ShellNativeMethods.SHChangeNotification_Unlock(lockId);
                }
            }
        }

        public bool FromSystemInterrupt => ((ShellObjectChangeTypes)this.@event & ShellObjectChangeTypes.FromInterrupt)
                    != ShellObjectChangeTypes.None;

        public int ImageIndex { get; private set; }

        public string ItemName { get; private set; }

        public string ItemName2 { get; private set; }

        public ShellObjectChangeTypes ChangeType => (ShellObjectChangeTypes)this.@event;
    }
}
