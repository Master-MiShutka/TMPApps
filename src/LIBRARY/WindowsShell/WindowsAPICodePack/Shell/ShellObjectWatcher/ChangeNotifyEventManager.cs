namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class ChangeNotifyEventManager
    {
        #region Change order
        private static readonly ShellObjectChangeTypes[] changeOrder =
        {
            ShellObjectChangeTypes.ItemCreate,
            ShellObjectChangeTypes.ItemRename,
            ShellObjectChangeTypes.ItemDelete,

            ShellObjectChangeTypes.AttributesChange,

            ShellObjectChangeTypes.DirectoryCreate,
            ShellObjectChangeTypes.DirectoryDelete,
            ShellObjectChangeTypes.DirectoryContentsUpdate,
            ShellObjectChangeTypes.DirectoryRename,

            ShellObjectChangeTypes.Update,

            ShellObjectChangeTypes.MediaInsert,
            ShellObjectChangeTypes.MediaRemove,
            ShellObjectChangeTypes.DriveAdd,
            ShellObjectChangeTypes.DriveRemove,
            ShellObjectChangeTypes.NetShare,
            ShellObjectChangeTypes.NetUnshare,

            ShellObjectChangeTypes.ServerDisconnect,
            ShellObjectChangeTypes.SystemImageUpdate,

            ShellObjectChangeTypes.AssociationChange,
            ShellObjectChangeTypes.FreeSpace,

            ShellObjectChangeTypes.DiskEventsMask,
            ShellObjectChangeTypes.GlobalEventsMask,
            ShellObjectChangeTypes.AllEventsMask,
        };
        #endregion

        private Dictionary<ShellObjectChangeTypes, Delegate> events = new Dictionary<ShellObjectChangeTypes, Delegate>();

        public void Register(ShellObjectChangeTypes changeType, Delegate handler)
        {
            Delegate del;
            if (!this.events.TryGetValue(changeType, out del))
            {
                this.events.Add(changeType, handler);
            }
            else
            {
                del = MulticastDelegate.Combine(del, handler);
                this.events[changeType] = del;
            }
        }

        public void Unregister(ShellObjectChangeTypes changeType, Delegate handler)
        {
            Delegate del;
            if (this.events.TryGetValue(changeType, out del))
            {
                del = MulticastDelegate.Remove(del, handler);
                if (del == null) // It's a bug in .NET if del is non-null and has an empty invocation list.
                {
                    this.events.Remove(changeType);
                }
                else
                {
                    this.events[changeType] = del;
                }
            }
        }

        public void UnregisterAll()
        {
            this.events.Clear();
        }

        public void Invoke(object sender, ShellObjectChangeTypes changeType, EventArgs args)
        {
            // Removes FromInterrupt flag if pressent
            changeType = changeType & ~ShellObjectChangeTypes.FromInterrupt;

            Delegate del;
            foreach (var change in changeOrder.Where(x => (x & changeType) != 0))
            {
                if (this.events.TryGetValue(change, out del))
                {
                    del.DynamicInvoke(sender, args);
                }
            }
        }

        public ShellObjectChangeTypes RegisteredTypes => this.events.Keys.Aggregate<ShellObjectChangeTypes, ShellObjectChangeTypes>(
                    ShellObjectChangeTypes.None,
                    (accumulator, changeType) => changeType | accumulator);
    }
}
