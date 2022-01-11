// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Dialogs
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Microsoft.WindowsAPICodePack.Shell;

    /// <summary>
    /// Creates a Vista or Windows 7 Common File Dialog, allowing the user to select one or more files.
    /// </summary>
    ///
    public sealed class CommonOpenFileDialog : CommonFileDialog
    {
        private NativeFileOpenDialog openDialogCoClass;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public CommonOpenFileDialog()
            : base()
        {
            // For Open file dialog, allow read only files.
            base.EnsureReadOnly = true;
        }

        /// <summary>
        /// Creates a new instance of this class with the specified name.
        /// </summary>
        /// <param name="name">The name of this dialog.</param>
        public CommonOpenFileDialog(string name)
            : base(name)
        {
            // For Open file dialog, allow read only files.
            base.EnsureReadOnly = true;
        }

        #region Public API specific to Open

        /// <summary>
        /// Gets a collection of the selected file names.
        /// </summary>
        /// <remarks>This property should only be used when the
        /// <see cref="CommonOpenFileDialog.Multiselect"/>
        /// property is <b>true</b>.</remarks>
        public IEnumerable<string> FileNames
        {
            get
            {
                this.CheckFileNamesAvailable();
                return base.FileNameCollection;
            }
        }

        /// <summary>
        /// Gets a collection of the selected items as ShellObject objects.
        /// </summary>
        /// <remarks>This property should only be used when the
        /// <see cref="CommonOpenFileDialog.Multiselect"/>
        /// property is <b>true</b>.</remarks>
        public ICollection<ShellObject> FilesAsShellObject
        {
            get
            {
                // Check if we have selected files from the user.
                this.CheckFileItemsAvailable();

                // temp collection to hold our shellobjects
                ICollection<ShellObject> resultItems = new Collection<ShellObject>();

                // Loop through our existing list of filenames, and try to create a concrete type of
                // ShellObject (e.g. ShellLibrary, FileSystemFolder, ShellFile, etc)
                foreach (IShellItem si in this.items)
                {
                    resultItems.Add(ShellObjectFactory.Create(si));
                }

                return resultItems;
            }
        }

        private bool multiselect;

        /// <summary>
        /// Gets or sets a value that determines whether the user can select more than one file.
        /// </summary>
        public bool Multiselect
        {
            get => this.multiselect;
            set => this.multiselect = value;
        }

        private bool isFolderPicker;

        /// <summary>
        /// Gets or sets a value that determines whether the user can select folders or files.
        /// Default value is false.
        /// </summary>
        public bool IsFolderPicker
        {
            get => this.isFolderPicker;
            set => this.isFolderPicker = value;
        }

        private bool allowNonFileSystem;

        /// <summary>
        /// Gets or sets a value that determines whether the user can select non-filesystem items,
        /// such as <b>Library</b>, <b>Search Connectors</b>, or <b>Known Folders</b>.
        /// </summary>
        public bool AllowNonFileSystemItems
        {
            get => this.allowNonFileSystem;
            set => this.allowNonFileSystem = value;
        }
        #endregion

        internal override IFileDialog GetNativeFileDialog()
        {
            Debug.Assert(this.openDialogCoClass != null, "Must call Initialize() before fetching dialog interface");

            return (IFileDialog)this.openDialogCoClass;
        }

        internal override void InitializeNativeFileDialog()
        {
            if (this.openDialogCoClass == null)
            {
                this.openDialogCoClass = new NativeFileOpenDialog();
            }
        }

        internal override void CleanUpNativeFileDialog()
        {
            if (this.openDialogCoClass != null)
            {
                Marshal.ReleaseComObject(this.openDialogCoClass);
            }
        }

        internal override void PopulateWithFileNames(Collection<string> names)
        {
            IShellItemArray resultsArray;
            uint count;

            this.openDialogCoClass.GetResults(out resultsArray);
            resultsArray.GetCount(out count);
            names.Clear();
            for (int i = 0; i < count; i++)
            {
                names.Add(GetFileNameFromShellItem(GetShellItemAt(resultsArray, i)));
            }
        }

        internal override void PopulateWithIShellItems(Collection<IShellItem> items)
        {
            IShellItemArray resultsArray;
            uint count;

            this.openDialogCoClass.GetResults(out resultsArray);
            resultsArray.GetCount(out count);
            items.Clear();
            for (int i = 0; i < count; i++)
            {
                items.Add(GetShellItemAt(resultsArray, i));
            }
        }

        internal override ShellNativeMethods.FileOpenOptions GetDerivedOptionFlags(ShellNativeMethods.FileOpenOptions flags)
        {
            if (this.multiselect)
            {
                flags |= ShellNativeMethods.FileOpenOptions.AllowMultiSelect;
            }

            if (this.isFolderPicker)
            {
                flags |= ShellNativeMethods.FileOpenOptions.PickFolders;
            }

            if (!this.allowNonFileSystem)
            {
                flags |= ShellNativeMethods.FileOpenOptions.ForceFilesystem;
            }
            else if (this.allowNonFileSystem)
            {
                flags |= ShellNativeMethods.FileOpenOptions.AllNonStorageItems;
            }

            return flags;
        }
    }
}
