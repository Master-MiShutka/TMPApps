namespace WPFHexaEditor.Core.Bytes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ByteModified
    {
        private byte? @byte = null;
        private ByteAction action = ByteAction.Nothing;
        private long position = -1;
        private long undoLenght = 1;

        /// <summary>
        /// Byte mofidied
        /// </summary>
        public byte? Byte
        {
            get => this.@byte;

            set => this.@byte = value;
        }

        /// <summary>
        /// Action have made in this byte
        /// </summary>
        public ByteAction Action
        {
            get => this.action;

            set => this.action = value;
        }

        /// <summary>
        /// Get of Set te position in file
        /// </summary>
        public long BytePositionInFile
        {
            get => this.position;

            set => this.position = value;
        }

        /// <summary>
        /// Check if the object is valid and data can be used for action
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (this.BytePositionInFile > -1 && this.Action != ByteAction.Nothing && this.Byte != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Number of undo todo when this byte is reach
        /// </summary>
        public long UndoLenght
        {
            get => this.undoLenght;

            set => this.undoLenght = value;
        }

        /// <summary>
        /// Clear object
        /// </summary>
        public void Clear()
        {
            this.@byte = null;
            this.action = ByteAction.Nothing;
            this.position = -1;
        }

        /// <summary>
        /// Copy Current instance to another
        /// </summary>
        /// <returns></returns>
        public ByteModified GetCopy()
        {
            ByteModified newByteModified = new ByteModified();
            object copied = null;

            newByteModified.Action = this.Action;
            newByteModified.Byte = this.Byte; // .Value;
            newByteModified.BytePositionInFile = this.BytePositionInFile;

            copied = newByteModified;

            return (ByteModified)copied;
        }

        public override string ToString()
        {
            return $"ByteModified - Action:{this.Action} Position:{this.BytePositionInFile} Byte:{this.Byte}";
        }

        /// <summary>
        /// Get if file is open
        /// </summary>
        public static bool CheckIsValid(ByteModified byteModified)
        {
            if (byteModified != null)
            {
                if (byteModified.IsValid)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
