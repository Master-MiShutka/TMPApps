/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

namespace DataGridWpf.Export
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    // Class used to format data into the CF_HTML Clipbard Format
    // http://msdn.microsoft.com/en-us/library/aa767917(VS.85).aspx
    internal sealed class CF_HtmlStream : Stream
    {
        public CF_HtmlStream(Stream innerStream)
        {
            if (innerStream == null)
            {
                throw new ArgumentNullException(nameof(innerStream));
            }

            if (innerStream.CanWrite == false)
            {
                throw new InvalidOperationException("An attempt was made to use a non-writable stream.");
            }

            if (innerStream.CanSeek == false)
            {
                throw new InvalidOperationException("An attempt was made to use a non-seekable stream.");
            }

            this._innerStream = innerStream;

            StringBuilder headerStringBuilder = new StringBuilder();
            headerStringBuilder.Append("Version:1.0");
            headerStringBuilder.Append(Environment.NewLine);
            headerStringBuilder.Append("StartHTML:-1"); // This is optional according to MSDN documentation
            headerStringBuilder.Append(Environment.NewLine);
            headerStringBuilder.Append("EndHTML:-1"); // This is optional according to MSDN documentation
            headerStringBuilder.Append(Environment.NewLine);
            headerStringBuilder.Append("StartFragment:0000000109"); // Always 109 bytes from start of Version to the end of <!--StartFragment--> tag
            headerStringBuilder.Append(Environment.NewLine);

            // Get the offset of the EndFragment: tag to be able to modify the 10 digits
            this.endFragmentOffset = headerStringBuilder.ToString().Length;

            Debug.Assert(this.endFragmentOffset == 65);

            headerStringBuilder.Append("EndFragment:0000000000"); // We write 0000000000 and we will update this field when the Stream is closed
            headerStringBuilder.Append(Environment.NewLine);
            headerStringBuilder.Append("<!--StartFragment-->");

            string headerString = headerStringBuilder.ToString();

            byte[] tempBuffer = Encoding.UTF8.GetBytes(headerString);

            this._headerBytesLength = headerStringBuilder.Length;

            Debug.Assert(this._headerBytesLength == tempBuffer.Length);
            Debug.Assert(tempBuffer.Length == 109);

            this._innerStream.Write(tempBuffer, 0, this._headerBytesLength);
            this._htmlContentByteCount = 0;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => this._headerBytesLength + this._htmlContentByteCount + _footerBytes.Length;

        public override long Position
        {
            get => throw new NotSupportedException();

            set => throw new NotSupportedException();
        }

        public override void Close()
        {
            // Already closed, nothing to do
            if (this.closed)
            {
                return;
            }

            // Update the value of EndFragment field in the header
            string endFragmentOffset = (this._htmlContentByteCount + this._headerBytesLength).ToString("0000000000", CultureInfo.InvariantCulture);

            this._innerStream.Seek(this.endFragmentOffset, SeekOrigin.Begin);

            byte[] tempBuffer = Encoding.UTF8.GetBytes("EndFragment:" + endFragmentOffset);
            Debug.Assert(tempBuffer.Length == 22);
            this._innerStream.Write(tempBuffer, 0, tempBuffer.Length);

            // Append the final end line and EndFragment tag
            this._innerStream.Seek(0, SeekOrigin.End);
            this._innerStream.Write(_footerBytes, 0, _footerBytes.Length);
            this._innerStream.Flush();

            this.closed = true;
        }

        protected override void Dispose(bool disposing)
        {
            this.Close();
        }

        public override void Flush()
        {
            this._innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {
            this.CheckIfClosed();
            this._htmlContentByteCount++;
            this._innerStream.WriteByte(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckIfClosed();
            this._htmlContentByteCount += count;
            this._innerStream.Write(buffer, offset, count);
        }

        private void CheckIfClosed()
        {
            if (this.closed)
            {
                throw new InvalidOperationException("An attempt was made to access a closed stream.");
            }
        }

        private bool closed; // = false;
        private int endFragmentOffset; // = 0;
        private int _headerBytesLength; // = 0;
        private long _htmlContentByteCount; // = 0;
        private Stream _innerStream; // = null;

        private static byte[] _footerBytes = Encoding.UTF8.GetBytes(Environment.NewLine + "<!--EndFragment-->");
    }
}
