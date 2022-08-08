// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using Microsoft.WindowsAPICodePack.Shell.Resources;

    /// <summary>
    /// Listens for changes in/on a ShellObject and raises events when they occur.
    /// This class supports all items under the shell namespace including
    /// files, folders and virtual folders (libraries, search results and network items), etc.
    /// </summary>
    public class ShellObjectWatcher : IDisposable
    {
        private ShellObject shellObject;
        private bool recursive;

        private ChangeNotifyEventManager _manager = new ChangeNotifyEventManager();
        private IntPtr _listenerHandle;
        private uint _message;

        private uint _registrationId;
        private volatile bool _running;

        private SynchronizationContext _context = SynchronizationContext.Current;

        /// <summary>
        /// Creates the ShellObjectWatcher for the given ShellObject
        /// </summary>
        /// <param name="shellObject">The ShellObject to monitor</param>
        /// <param name="recursive">Whether to listen for changes recursively (for when monitoring a container)</param>
        public ShellObjectWatcher(ShellObject shellObject, bool recursive)
        {
            if (shellObject == null)
            {
                throw new ArgumentNullException(nameof(shellObject));
            }

            if (this._context == null)
            {
                this._context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(this._context);
            }

            this.shellObject = shellObject;
            this.recursive = recursive;

            var result = MessageListenerFilter.Register(this.OnWindowMessageReceived);
            this._listenerHandle = result.WindowHandle;
            this._message = result.Message;
        }

        /// <summary>
        /// Gets whether the watcher is currently running.
        /// </summary>
        public bool Running
        {
            get => this._running;
            private set => this._running = value;
        }

        /// <summary>
        /// Start the watcher and begin receiving change notifications.
        /// <remarks>
        /// If the watcher is running, has no effect.
        /// Registration for notifications should be done before this is called.
        /// </remarks>
        /// </summary>
        public void Start()
        {
            if (this.Running)
            {
                return;
            }

            #region Registration
            ShellNativeMethods.SHChangeNotifyEntry entry = new ShellNativeMethods.SHChangeNotifyEntry();
            entry.recursively = this.recursive;

            entry.pIdl = this.shellObject.PIDL;

            this._registrationId = ShellNativeMethods.SHChangeNotifyRegister(
                this._listenerHandle,
                ShellNativeMethods.ShellChangeNotifyEventSource.ShellLevel | ShellNativeMethods.ShellChangeNotifyEventSource.InterruptLevel | ShellNativeMethods.ShellChangeNotifyEventSource.NewDelivery,
                this._manager.RegisteredTypes, // ShellObjectChangeTypes.AllEventsMask,
                this._message,
                1,
                ref entry);

            if (this._registrationId == 0)
            {
                throw new Win32Exception(LocalizedMessages.ShellObjectWatcherRegisterFailed);
            }
            #endregion

            this.Running = true;
        }

        /// <summary>
        /// Stop the watcher and prevent further notifications from being received.
        /// <remarks>If the watcher is not running, this has no effect.</remarks>
        /// </summary>
        public void Stop()
        {
            if (!this.Running)
            {
                return;
            }

            if (this._registrationId > 0)
            {
                ShellNativeMethods.SHChangeNotifyDeregister(this._registrationId);
                this._registrationId = 0;
            }

            this.Running = false;
        }

        private void OnWindowMessageReceived(WindowMessageEventArgs e)
        {
            if (e.Message.Msg == this._message)
            {
                this._context.Send(x => this.ProcessChangeNotificationEvent(e), null);
            }
        }

        private void ThrowIfRunning()
        {
            if (this.Running)
            {
                throw new InvalidOperationException(LocalizedMessages.ShellObjectWatcherUnableToChangeEvents);
            }
        }

        /// <summary>
        /// Processes all change notifications sent by the Windows Shell.
        /// </summary>
        /// <param name="e">The windows message representing the notification event</param>
        protected virtual void ProcessChangeNotificationEvent(WindowMessageEventArgs e)
        {
            if (!this.Running)
            {
                return;
            }

            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            ChangeNotifyLock notifyLock = new ChangeNotifyLock(e.Message);

            ShellObjectNotificationEventArgs args = null;
            switch (notifyLock.ChangeType)
            {
                case ShellObjectChangeTypes.DirectoryRename:
                case ShellObjectChangeTypes.ItemRename:
                    args = new ShellObjectRenamedEventArgs(notifyLock);
                    break;
                case ShellObjectChangeTypes.SystemImageUpdate:
                    args = new SystemImageUpdatedEventArgs(notifyLock);
                    break;
                default:
                    args = new ShellObjectChangedEventArgs(notifyLock);
                    break;
            }

            this._manager.Invoke(this, notifyLock.ChangeType, args);
        }

        #region Change Events

        #region Mask Events

        /// <summary>
        /// Raised when any event occurs.
        /// </summary>
        public event EventHandler<ShellObjectNotificationEventArgs> AllEvents
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.AllEventsMask, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.AllEventsMask, value);
            }
        }

        /// <summary>
        /// Raised when global events occur.
        /// </summary>
        public event EventHandler<ShellObjectNotificationEventArgs> GlobalEvents
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.GlobalEventsMask, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.GlobalEventsMask, value);
            }
        }

        /// <summary>
        /// Raised when disk events occur.
        /// </summary>
        public event EventHandler<ShellObjectNotificationEventArgs> DiskEvents
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.DiskEventsMask, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.DiskEventsMask, value);
            }
        }
        #endregion

        #region Single Events

        /// <summary>
        /// Raised when an item is renamed.
        /// </summary>
        public event EventHandler<ShellObjectRenamedEventArgs> ItemRenamed
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.ItemRename, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.ItemRename, value);
            }
        }

        /// <summary>
        /// Raised when an item is created.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> ItemCreated
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.ItemCreate, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.ItemCreate, value);
            }
        }

        /// <summary>
        /// Raised when an item is deleted.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> ItemDeleted
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.ItemDelete, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.ItemDelete, value);
            }
        }

        /// <summary>
        /// Raised when an item is updated.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> Updated
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.Update, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.Update, value);
            }
        }

        /// <summary>
        /// Raised when a directory is updated.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> DirectoryUpdated
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.DirectoryContentsUpdate, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.DirectoryContentsUpdate, value);
            }
        }

        /// <summary>
        /// Raised when a directory is renamed.
        /// </summary>
        public event EventHandler<ShellObjectRenamedEventArgs> DirectoryRenamed
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.DirectoryRename, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.DirectoryRename, value);
            }
        }

        /// <summary>
        /// Raised when a directory is created.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> DirectoryCreated
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.DirectoryCreate, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.DirectoryCreate, value);
            }
        }

        /// <summary>
        /// Raised when a directory is deleted.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> DirectoryDeleted
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.DirectoryDelete, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.DirectoryDelete, value);
            }
        }

        /// <summary>
        /// Raised when media is inserted.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> MediaInserted
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.MediaInsert, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.MediaInsert, value);
            }
        }

        /// <summary>
        /// Raised when media is removed.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> MediaRemoved
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.MediaRemove, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.MediaRemove, value);
            }
        }

        /// <summary>
        /// Raised when a drive is added.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> DriveAdded
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.DriveAdd, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.DriveAdd, value);
            }
        }

        /// <summary>
        /// Raised when a drive is removed.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> DriveRemoved
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.DriveRemove, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.DriveRemove, value);
            }
        }

        /// <summary>
        /// Raised when a folder is shared on a network.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> FolderNetworkShared
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.NetShare, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.NetShare, value);
            }
        }

        /// <summary>
        /// Raised when a folder is unshared from the network.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> FolderNetworkUnshared
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.NetUnshare, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.NetUnshare, value);
            }
        }

        /// <summary>
        /// Raised when a server is disconnected.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> ServerDisconnected
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.ServerDisconnect, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.ServerDisconnect, value);
            }
        }

        /// <summary>
        /// Raised when a system image is changed.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> SystemImageChanged
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.SystemImageUpdate, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.SystemImageUpdate, value);
            }
        }

        /// <summary>
        /// Raised when free space changes.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> FreeSpaceChanged
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.FreeSpace, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.FreeSpace, value);
            }
        }

        /// <summary>
        /// Raised when a file type association changes.
        /// </summary>
        public event EventHandler<ShellObjectChangedEventArgs> FileTypeAssociationChanged
        {
            add
            {
                this.ThrowIfRunning();
                this._manager.Register(ShellObjectChangeTypes.AssociationChange, value);
            }

            remove
            {
                this.ThrowIfRunning();
                this._manager.Unregister(ShellObjectChangeTypes.AssociationChange, value);
            }
        }
        #endregion

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes ShellObjectWatcher
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            this.Stop();
            this._manager.UnregisterAll();

            if (this._listenerHandle != IntPtr.Zero)
            {
                MessageListenerFilter.Unregister(this._listenerHandle, this._message);
            }
        }

        /// <summary>
        /// Disposes ShellObjectWatcher.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer for ShellObjectWatcher
        /// </summary>
        ~ShellObjectWatcher()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
