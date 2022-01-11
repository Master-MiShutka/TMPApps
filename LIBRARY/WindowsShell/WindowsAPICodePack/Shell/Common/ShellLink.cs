// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;

    /// <summary>
    /// Represents a link to existing FileSystem or Virtual item.
    /// </summary>
    public class ShellLink : ShellObject
    {
        /// <summary>
        /// Path for this file e.g. c:\Windows\file.txt,
        /// </summary>
        private string internalPath;

        #region Internal Constructors

        internal ShellLink(IShellItem2 shellItem)
        {
            this.nativeShellItem = shellItem;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path for this link
        /// </summary>
        virtual public string Path
        {
            get
            {
                if (this.internalPath == null && this.NativeShellItem != null)
                {
                    this.internalPath = base.ParsingName;
                }

                return this.internalPath;
            }

            protected set => this.internalPath = value;
        }

        private string internalTargetLocation;

        /// <summary>
        /// Gets the location to which this link points to.
        /// </summary>
        public string TargetLocation
        {
            get
            {
                if (string.IsNullOrEmpty(this.internalTargetLocation) && this.NativeShellItem2 != null)
                {
                    this.internalTargetLocation = this.Properties.System.Link.TargetParsingPath.Value;
                }

                return this.internalTargetLocation;
            }

            set
            {
                if (value == null)
                {
                    return;
                }

                this.internalTargetLocation = value;

                if (this.NativeShellItem2 != null)
                {
                    this.Properties.System.Link.TargetParsingPath.Value = this.internalTargetLocation;
                }
            }
        }

        /// <summary>
        /// Gets the ShellObject to which this link points to.
        /// </summary>
        public ShellObject TargetShellObject => ShellObjectFactory.Create(this.TargetLocation);

        /// <summary>
        /// Gets or sets the link's title
        /// </summary>
        public string Title
        {
            get
            {
                if (this.NativeShellItem2 != null)
                {
                    return this.Properties.System.Title.Value;
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (this.NativeShellItem2 != null)
                {
                    this.Properties.System.Title.Value = value;
                }
            }
        }

        private string internalArguments;

        /// <summary>
        /// Gets the arguments associated with this link.
        /// </summary>
        public string Arguments
        {
            get
            {
                if (string.IsNullOrEmpty(this.internalArguments) && this.NativeShellItem2 != null)
                {
                    this.internalArguments = this.Properties.System.Link.Arguments.Value;
                }

                return this.internalArguments;
            }
        }

        private string internalComments;

        /// <summary>
        /// Gets the comments associated with this link.
        /// </summary>
        public string Comments
        {
            get
            {
                if (string.IsNullOrEmpty(this.internalComments) && this.NativeShellItem2 != null)
                {
                    this.internalComments = this.Properties.System.Comment.Value;
                }

                return this.internalComments;
            }
        }

        #endregion
    }
}
