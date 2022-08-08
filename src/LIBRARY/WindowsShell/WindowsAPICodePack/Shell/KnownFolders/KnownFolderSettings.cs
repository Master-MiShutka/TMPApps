// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using MS.WindowsAPICodePack.Internal;

    /// <summary>
    /// Internal class to represent the KnownFolder settings/properties
    /// </summary>
    internal class KnownFolderSettings
    {
        private FolderProperties knownFolderProperties;

        internal KnownFolderSettings(IKnownFolderNative knownFolderNative)
        {
            this.GetFolderProperties(knownFolderNative);
        }

        #region Private Methods

        /// <summary>
        /// Populates a structure that contains
        /// this known folder's properties.
        /// </summary>
        private void GetFolderProperties(IKnownFolderNative knownFolderNative)
        {
            Debug.Assert(knownFolderNative != null);

            KnownFoldersSafeNativeMethods.NativeFolderDefinition nativeFolderDefinition;
            knownFolderNative.GetFolderDefinition(out nativeFolderDefinition);

            try
            {
                this.knownFolderProperties.category = nativeFolderDefinition.category;
                this.knownFolderProperties.canonicalName = Marshal.PtrToStringUni(nativeFolderDefinition.name);
                this.knownFolderProperties.description = Marshal.PtrToStringUni(nativeFolderDefinition.description);
                this.knownFolderProperties.parentId = nativeFolderDefinition.parentId;
                this.knownFolderProperties.relativePath = Marshal.PtrToStringUni(nativeFolderDefinition.relativePath);
                this.knownFolderProperties.parsingName = Marshal.PtrToStringUni(nativeFolderDefinition.parsingName);
                this.knownFolderProperties.tooltipResourceId = Marshal.PtrToStringUni(nativeFolderDefinition.tooltip);
                this.knownFolderProperties.localizedNameResourceId = Marshal.PtrToStringUni(nativeFolderDefinition.localizedName);
                this.knownFolderProperties.iconResourceId = Marshal.PtrToStringUni(nativeFolderDefinition.icon);
                this.knownFolderProperties.security = Marshal.PtrToStringUni(nativeFolderDefinition.security);
                this.knownFolderProperties.fileAttributes = (System.IO.FileAttributes)nativeFolderDefinition.attributes;
                this.knownFolderProperties.definitionOptions = nativeFolderDefinition.definitionOptions;
                this.knownFolderProperties.folderTypeId = nativeFolderDefinition.folderTypeId;
                this.knownFolderProperties.folderType = FolderTypes.GetFolderType(this.knownFolderProperties.folderTypeId);

                bool pathExists;
                this.knownFolderProperties.path = this.GetPath(out pathExists, knownFolderNative);
                this.knownFolderProperties.pathExists = pathExists;

                this.knownFolderProperties.redirection = knownFolderNative.GetRedirectionCapabilities();

                // Turn tooltip, localized name and icon resource IDs
                // into the actual resources.
                this.knownFolderProperties.tooltip = CoreHelpers.GetStringResource(this.knownFolderProperties.tooltipResourceId);
                this.knownFolderProperties.localizedName = CoreHelpers.GetStringResource(this.knownFolderProperties.localizedNameResourceId);

                this.knownFolderProperties.folderId = knownFolderNative.GetId();
            }
            finally
            {
                // Clean up memory.
                Marshal.FreeCoTaskMem(nativeFolderDefinition.name);
                Marshal.FreeCoTaskMem(nativeFolderDefinition.description);
                Marshal.FreeCoTaskMem(nativeFolderDefinition.relativePath);
                Marshal.FreeCoTaskMem(nativeFolderDefinition.parsingName);
                Marshal.FreeCoTaskMem(nativeFolderDefinition.tooltip);
                Marshal.FreeCoTaskMem(nativeFolderDefinition.localizedName);
                Marshal.FreeCoTaskMem(nativeFolderDefinition.icon);
                Marshal.FreeCoTaskMem(nativeFolderDefinition.security);
            }
        }

        /// <summary>
        /// Gets the path of this this known folder.
        /// </summary>
        /// <param name="fileExists">
        /// Returns false if the folder is virtual, or a boolean
        /// value that indicates whether this known folder exists.
        /// </param>
        /// <param name="knownFolderNative">Native IKnownFolder reference</param>
        /// <returns>
        /// A <see cref="string"/> containing the path, or <see cref="string.Empty"/> if this known folder does not exist.
        /// </returns>
        private string GetPath(out bool fileExists, IKnownFolderNative knownFolderNative)
        {
            Debug.Assert(knownFolderNative != null);

            string kfPath = string.Empty;
            fileExists = true;

            // Virtual folders do not have path.
            if (this.knownFolderProperties.category == FolderCategory.Virtual)
            {
                fileExists = false;
                return kfPath;
            }

            try
            {
                kfPath = knownFolderNative.GetPath(0);
            }
            catch (System.IO.FileNotFoundException)
            {
                fileExists = false;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                fileExists = false;
            }

            return kfPath;
        }

        #endregion

        #region KnownFolder Properties

        /// <summary>
        /// Gets the path for this known folder.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string Path => this.knownFolderProperties.path;

        /// <summary>
        /// Gets the category designation for this known folder.
        /// </summary>
        /// <value>A <see cref="FolderCategory"/> value.</value>
        public FolderCategory Category => this.knownFolderProperties.category;

        /// <summary>
        /// Gets this known folder's canonical name.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string CanonicalName => this.knownFolderProperties.canonicalName;

        /// <summary>
        /// Gets this known folder's description.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string Description => this.knownFolderProperties.description;

        /// <summary>
        /// Gets the unique identifier for this known folder's parent folder.
        /// </summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid ParentId => this.knownFolderProperties.parentId;

        /// <summary>
        /// Gets this known folder's relative path.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string RelativePath => this.knownFolderProperties.relativePath;

        /// <summary>
        /// Gets this known folder's tool tip text.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string Tooltip => this.knownFolderProperties.tooltip;

        /// <summary>
        /// Gets the resource identifier for this
        /// known folder's tool tip text.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string TooltipResourceId => this.knownFolderProperties.tooltipResourceId;

        /// <summary>
        /// Gets this known folder's localized name.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string LocalizedName => this.knownFolderProperties.localizedName;

        /// <summary>
        /// Gets the resource identifier for this
        /// known folder's localized name.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string LocalizedNameResourceId => this.knownFolderProperties.localizedNameResourceId;

        /// <summary>
        /// Gets this known folder's security attributes.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string Security => this.knownFolderProperties.security;

        /// <summary>
        /// Gets this known folder's file attributes,
        /// such as "read-only".
        /// </summary>
        /// <value>A <see cref="System.IO.FileAttributes"/> value.</value>
        public System.IO.FileAttributes FileAttributes => this.knownFolderProperties.fileAttributes;

        /// <summary>
        /// Gets an value that describes this known folder's behaviors.
        /// </summary>
        /// <value>A <see cref="DefinitionOptions"/> value.</value>
        public DefinitionOptions DefinitionOptions => this.knownFolderProperties.definitionOptions;

        /// <summary>
        /// Gets the unique identifier for this known folder's type.
        /// </summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderTypeId => this.knownFolderProperties.folderTypeId;

        /// <summary>
        /// Gets a string representation of this known folder's type.
        /// </summary>
        /// <value>A <see cref="string"/> object.</value>
        public string FolderType => this.knownFolderProperties.folderType;

        /// <summary>
        /// Gets the unique identifier for this known folder.
        /// </summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderId => this.knownFolderProperties.folderId;

        /// <summary>
        /// Gets a value that indicates whether this known folder's path exists on the computer.
        /// </summary>
        /// <value>A bool<see cref="bool"/> value.</value>
        /// <remarks>If this property value is <b>false</b>,
        /// the folder might be a virtual folder (<see cref="Category"/> property will
        /// be <see cref="FolderCategory.Virtual"/> for virtual folders)</remarks>
        public bool PathExists => this.knownFolderProperties.pathExists;

        /// <summary>
        /// Gets a value that states whether this known folder
        /// can have its path set to a new value,
        /// including any restrictions on the redirection.
        /// </summary>
        /// <value>A <see cref="RedirectionCapability"/> value.</value>
        public RedirectionCapability Redirection => this.knownFolderProperties.redirection;

        #endregion
    }
}
