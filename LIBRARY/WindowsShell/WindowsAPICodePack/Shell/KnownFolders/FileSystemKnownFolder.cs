// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a registered file system Known Folder
    /// </summary>
    public class FileSystemKnownFolder : ShellFileSystemFolder, IKnownFolder, IDisposable
    {
        #region Private Fields

        private IKnownFolderNative knownFolderNative;
        private KnownFolderSettings knownFolderSettings;

        #endregion

        #region Internal Constructors

        internal FileSystemKnownFolder(IShellItem2 shellItem) : base(shellItem)
        {
        }

        internal FileSystemKnownFolder(IKnownFolderNative kf)
        {
            Debug.Assert(kf != null);
            this.knownFolderNative = kf;

            // Set the native shell item
            // and set it on the base class (ShellObject)
            Guid guid = new Guid(ShellIIDGuid.IShellItem2);
            this.knownFolderNative.GetShellItem(0, ref guid, out this.nativeShellItem);
        }

        #endregion

        #region Private Members

        private KnownFolderSettings KnownFolderSettings
        {
            get
            {
                if (this.knownFolderNative == null)
                {
                    // We need to get the PIDL either from the NativeShellItem,
                    // or from base class's property (if someone already set it on us).
                    // Need to use the PIDL to get the native IKnownFolder interface.

                    // Get the PIDL for the ShellItem
                    if (this.nativeShellItem != null && base.PIDL == IntPtr.Zero)
                    {
                        base.PIDL = ShellHelper.PidlFromShellItem(this.nativeShellItem);
                    }

                    // If we have a valid PIDL, get the native IKnownFolder
                    if (base.PIDL != IntPtr.Zero)
                    {
                        this.knownFolderNative = KnownFolderHelper.FromPIDL(base.PIDL);
                    }

                    Debug.Assert(this.knownFolderNative != null);
                }

                // If this is the first time this property is being called,
                // get the native Folder Defination (KnownFolder properties)
                if (this.knownFolderSettings == null)
                {
                    this.knownFolderSettings = new KnownFolderSettings(this.knownFolderNative);
                }

                return this.knownFolderSettings;
            }
        }

        #endregion

        #region IKnownFolder Members

        /// <summary>
        /// Gets the path for this known folder.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public override string Path => this.KnownFolderSettings.Path;

        /// <summary>
        /// Gets the category designation for this known folder.
        /// </summary>
        /// <value>A <see cref="FolderCategory"/> value.</value>
        public FolderCategory Category => this.KnownFolderSettings.Category;

        /// <summary>
        /// Gets this known folder's canonical name.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string CanonicalName => this.KnownFolderSettings.CanonicalName;

        /// <summary>
        /// Gets this known folder's description.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string Description => this.KnownFolderSettings.Description;

        /// <summary>
        /// Gets the unique identifier for this known folder's parent folder.
        /// </summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid ParentId => this.KnownFolderSettings.ParentId;

        /// <summary>
        /// Gets this known folder's relative path.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string RelativePath => this.KnownFolderSettings.RelativePath;

        /// <summary>
        /// Gets this known folder's parsing name.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public override string ParsingName => base.ParsingName;

        /// <summary>
        /// Gets this known folder's tool tip text.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string Tooltip => this.KnownFolderSettings.Tooltip;

        /// <summary>
        /// Gets the resource identifier for this
        /// known folder's tool tip text.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string TooltipResourceId => this.KnownFolderSettings.TooltipResourceId;

        /// <summary>
        /// Gets this known folder's localized name.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string LocalizedName => this.KnownFolderSettings.LocalizedName;

        /// <summary>
        /// Gets the resource identifier for this
        /// known folder's localized name.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string LocalizedNameResourceId => this.KnownFolderSettings.LocalizedNameResourceId;

        /// <summary>
        /// Gets this known folder's security attributes.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string Security => this.KnownFolderSettings.Security;

        /// <summary>
        /// Gets this known folder's file attributes,
        /// such as "read-only".
        /// </summary>
        /// <value>A <see cref="System.IO.FileAttributes"/> value.</value>
        public System.IO.FileAttributes FileAttributes => this.KnownFolderSettings.FileAttributes;

        /// <summary>
        /// Gets an value that describes this known folder's behaviors.
        /// </summary>
        /// <value>A <see cref="DefinitionOptions"/> value.</value>
        public DefinitionOptions DefinitionOptions => this.KnownFolderSettings.DefinitionOptions;

        /// <summary>
        /// Gets the unique identifier for this known folder's type.
        /// </summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderTypeId => this.KnownFolderSettings.FolderTypeId;

        /// <summary>
        /// Gets a string representation of this known folder's type.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string FolderType => this.KnownFolderSettings.FolderType;

        /// <summary>
        /// Gets the unique identifier for this known folder.
        /// </summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderId => this.KnownFolderSettings.FolderId;

        /// <summary>
        /// Gets a value that indicates whether this known folder's path exists on the computer.
        /// </summary>
        /// <value>A bool<see cref="bool"/> value.</value>
        /// <remarks>If this property value is <b>false</b>,
        /// the folder might be a virtual folder (<see cref="Category"/> property will
        /// be <see cref="FolderCategory.Virtual"/> for virtual folders)</remarks>
        public bool PathExists => this.KnownFolderSettings.PathExists;

        /// <summary>
        /// Gets a value that states whether this known folder
        /// can have its path set to a new value,
        /// including any restrictions on the redirection.
        /// </summary>
        /// <value>A <see cref="RedirectionCapability"/> value.</value>
        public RedirectionCapability Redirection => this.KnownFolderSettings.Redirection;

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Release resources
        /// </summary>
        /// <param name="disposing">Indicates that this mothod is being called from Dispose() rather than the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.knownFolderSettings = null;
            }

            if (this.knownFolderNative != null)
            {
                Marshal.ReleaseComObject(this.knownFolderNative);
                this.knownFolderNative = null;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
