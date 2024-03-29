﻿namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.WindowsAPICodePack.Shell.Resources;
    using MS.WindowsAPICodePack.Internal;

    internal static class MessageListenerFilter
    {
        private static readonly object registerLock = new object();
        private static List<RegisteredListener> packages = new List<RegisteredListener>();

        public static MessageListenerFilterRegistrationResult Register(Action<WindowMessageEventArgs> callback)
        {
            lock (registerLock)
            {
                uint message = 0;
                var package = packages.FirstOrDefault(x => x.TryRegister(callback, out message));
                if (package == null)
                {
                    package = new RegisteredListener();
                    if (!package.TryRegister(callback, out message))
                    {   // this should never happen
                        throw new ShellException(LocalizedMessages.MessageListenerFilterUnableToRegister);
                    }

                    packages.Add(package);
                }

                return new MessageListenerFilterRegistrationResult(
                    package.Listener.WindowHandle,
                    message);
            }
        }

        public static void Unregister(IntPtr listenerHandle, uint message)
        {
            lock (registerLock)
            {
                var package = packages.FirstOrDefault(x => x.Listener.WindowHandle == listenerHandle);
                if (package == null || !package.Callbacks.Remove(message))
                {
                    throw new ArgumentException(LocalizedMessages.MessageListenerFilterUnknownListenerHandle);
                }

                if (package.Callbacks.Count == 0)
                {
                    package.Listener.Dispose();
                    packages.Remove(package);
                }
            }
        }

        private class RegisteredListener
        {
            public Dictionary<uint, Action<WindowMessageEventArgs>> Callbacks { get; private set; }

            public MessageListener Listener { get; private set; }

            public RegisteredListener()
            {
                this.Callbacks = new Dictionary<uint, Action<WindowMessageEventArgs>>();
                this.Listener = new MessageListener();
                this.Listener.MessageReceived += this.MessageReceived;
            }

            private void MessageReceived(object sender, WindowMessageEventArgs e)
            {
                Action<WindowMessageEventArgs> action;
                if (this.Callbacks.TryGetValue(e.Message.Msg, out action))
                {
                    action(e);
                }
            }

            private uint lastMessage = MessageListener.BaseUserMessage;

            public bool TryRegister(Action<WindowMessageEventArgs> callback, out uint message)
            {
                message = 0;
                if (this.Callbacks.Count < ushort.MaxValue - MessageListener.BaseUserMessage)
                {
                    uint i = this.lastMessage + 1;
                    while (i != this.lastMessage)
                    {
                        if (i > ushort.MaxValue)
                        {
                            i = MessageListener.BaseUserMessage;
                        }

                        if (!this.Callbacks.ContainsKey(i))
                        {
                            this.lastMessage = message = i;
                            this.Callbacks.Add(i, callback);
                            return true;
                        }

                        i++;
                    }
                }

                return false;
            }
        }
    }

    /// <summary>
    /// The result of registering with the MessageListenerFilter
    /// </summary>
    internal class MessageListenerFilterRegistrationResult
    {
        internal MessageListenerFilterRegistrationResult(IntPtr handle, uint msg)
        {
            this.WindowHandle = handle;
            this.Message = msg;
        }

        /// <summary>
        /// Gets the window handle to which the callback was registered.
        /// </summary>
        public IntPtr WindowHandle { get; private set; }

        /// <summary>
        /// Gets the message for which the callback was registered.
        /// </summary>
        public uint Message { get; private set; }
    }
}
