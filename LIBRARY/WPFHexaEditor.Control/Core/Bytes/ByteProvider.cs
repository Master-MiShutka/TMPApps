namespace WPFHexaEditor.Core.Bytes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using WPFHexaEditor.Core.MethodExtention;

    /// <summary>
    /// Used for interaction with file
    /// </summary>
    public class ByteProvider
    {
        // Global variable
        private List<ByteModified> byteModifiedList = new List<ByteModified>();
        private string fileName = string.Empty;
        private Stream _stream = null;
        private bool _readOnlyMode = false;
        private bool _isUndoEnabled = true;
        private double _longProcessProgress = 0;
        private bool _isOnLongProcess = false;
        private ByteProviderStreamType _streamType = ByteProviderStreamType.Nothing;

        // Event
        public event EventHandler DataCopiedToClipboard;

        public event EventHandler ReadOnlyChanged;

        public event EventHandler Closed;

        public event EventHandler StreamOpened;

        public event EventHandler PositionChanged;

        public event EventHandler Undone;

        public event EventHandler DataCopiedToStream;

        public event EventHandler ChangesSubmited;

        public event EventHandler LongProcessProgressChanged;

        public event EventHandler LongProcessProgressStarted;

        public event EventHandler LongProcessProgressCompleted;

        public event EventHandler DataPastedNotInserted;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ByteProvider()
        {
        }

        /// <summary>
        /// Construct new ByteProvider with filename and try to open file
        /// </summary>
        public ByteProvider(string filename)
        {
            this.FileName = filename;
        }

        /// <summary>
        /// Constuct new ByteProvider with stream
        /// </summary>
        /// <param name="stream"></param>
        public ByteProvider(MemoryStream stream)
        {
            this.Stream = stream;
        }

        /// <summary>
        /// Set or Get the file with the control will show hex
        /// </summary>
        public string FileName
        {
            get => this.fileName;

            set
            {
                this.fileName = value;

                this.OpenFile();
            }
        }

        /// <summary>
        /// Get the type of stream are opened in byteprovider.
        /// </summary>
        public ByteProviderStreamType StreamType => this._streamType;

        /// <summary>
        /// Get or set a MemoryStream for use with byteProvider
        /// </summary>
        public MemoryStream Stream
        {
            get => (MemoryStream)this._stream;

            set
            {
                var readonlymode = this._readOnlyMode;
                this.Close();
                this._readOnlyMode = readonlymode;

                this._streamType = ByteProviderStreamType.MemoryStream;

                this._stream = value;

                this.StreamOpened?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Open file are set in FileName property
        /// </summary>
        public void OpenFile()
        {
            if (File.Exists(this.FileName))
            {
                this.Close();

                bool readOnlyMode = false;

                try
                {
                    this._stream = File.Open(this.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                }
                catch
                {
                    if (MessageBox.Show("The file is locked. Do you want to open it in read-only mode?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        this._stream = File.Open(this.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                        readOnlyMode = true;
                    }
                }

                if (readOnlyMode)
                {
                    this.ReadOnlyMode = true;
                }

                this._streamType = ByteProviderStreamType.File;

                this.StreamOpened?.Invoke(this, new EventArgs());
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// Put the control on readonly mode.
        /// </summary>
        public bool ReadOnlyMode
        {
            get => this._readOnlyMode;

            set
            {
                this._readOnlyMode = value;

                // Launch event
                this.ReadOnlyChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Close stream
        /// ReadOnlyMode is reset to false
        /// </summary>
        public void Close()
        {
            if (this.IsOpen)
            {
                this._stream.Close();
                this._stream = null;
                this.ReadOnlyMode = false;
                this.IsOnLongProcess = false;
                this.LongProcessProgress = 0;

                this._streamType = ByteProviderStreamType.Nothing;

                this.Closed?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Get the lenght of file. Return -1 if file is close.
        /// </summary>
        public long Length
        {
            get
            {
                if (this.IsOpen)
                {
                    return this._stream.Length;
                }

                return -1;
            }
        }

        /// <summary>
        /// Check if position as at EOF.
        /// </summary>
        public bool EOF => (this._stream.Position == this._stream.Length) || (this._stream.Position > this._stream.Length);

        /// <summary>
        /// Get or Set position in file. Return -1 when file is closed
        /// </summary>
        public long Position
        {
            get
            {
                if (this.IsOpen)
                {
                    return this._stream.Position <= this._stream.Length ? this._stream.Position : this._stream.Length;
                }

                return -1;
            }

            set
            {
                if (this.IsOpen)
                {
                    this._stream.Position = value;

                    this.PositionChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Get if file is open
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (this._stream != null)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Get if file is open
        /// </summary>
        public static bool CheckIsOpen(ByteProvider provider)
        {
            if (provider != null)
            {
                if (provider.IsOpen)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Readbyte at position if file CanRead. Return -1 is file is closed of EOF.
        /// </summary>
        /// <returns></returns>
        public int ReadByte()
        {
            if (this.IsOpen)
            {
                if (this._stream.CanRead)
                {
                    return this._stream.ReadByte();
                }
            }

            return -1;
        }

        #region SubmitChanges to file/stream

        /// <summary>
        /// Submit change to files/stream
        /// TODO : NEED OPTIMISATION FOR LARGE FILE... IT'S AS BEGINING :) USE TEMPS FILE ?
        /// TODO : USE TEMPS FILE FOR LARGE FILE
        /// </summary>
        public void SubmitChanges()
        {
            if (this.CanWrite)
            {
                // Set percent of progress to zero and create and iterator for help mesure progress
                this.LongProcessProgress = 0;
                int i = 0;

                // Create appropriate temp stream for new file.
                Stream NewStream = null;

                if (this.Length < ConstantReadOnly.LARGE_FILE_LENGTH)
                {
                    NewStream = new MemoryStream();
                }
                else
                {
                    NewStream = File.Open(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite);
                }

                // Fast change only nothing byte deleted or added
                if (this.ByteModifieds(ByteAction.Deleted).Count() == 0 &&
                    this.ByteModifieds(ByteAction.Added).Count() == 0)
                {
                    // Launch event at process strated
                    this.IsOnLongProcess = true;
                    this.LongProcessProgressStarted?.Invoke(this, new EventArgs());

                    var bytemodifiedList = this.ByteModifieds(ByteAction.Modified);
                    double countChange = bytemodifiedList.Count();
                    i = 0;

                    // Fast save. only save byteaction=modified
                    foreach (ByteModified bm in bytemodifiedList)
                    {
                        if (bm.IsValid)
                        {
                            // Set percent of progress
                            this.LongProcessProgress = i++ / countChange;

                            // Break process?
                            if (!this.IsOnLongProcess)
                            {
                                break;
                            }

                            this._stream.Position = bm.BytePositionInFile;
                            this._stream.WriteByte(bm.Byte.Value);
                        }
                    }

                    // Launch event at process completed
                    this.IsOnLongProcess = false;
                    this.LongProcessProgressCompleted?.Invoke(this, new EventArgs());
                }
                else
                {
                    // Launch event at process strated
                    this.IsOnLongProcess = true;
                    this.LongProcessProgressStarted?.Invoke(this, new EventArgs());

                    byte[] buffer = new byte[ConstantReadOnly.COPY_BLOCK_SIZE];
                    long bufferlength = 0;
                    var SortedBM = this.ByteModifieds(ByteAction.All).OrderBy(b => b.BytePositionInFile);
                    double countChange = SortedBM.Count();
                    i = 0;

                    // Set position
                    this.Position = 0;

                    ////Start update and rewrite file.
                    foreach (ByteModified nextByteModified in SortedBM)
                    {
                        // Set percent of progress
                        this.LongProcessProgress = i++ / countChange;

                        // Break process?
                        if (!this.IsOnLongProcess)
                        {
                            break;
                        }

                        // Reset buffer
                        buffer = new byte[ConstantReadOnly.COPY_BLOCK_SIZE];

                        // start read/write / use little block for optimize memory
                        while (this.Position != nextByteModified.BytePositionInFile)
                        {
                            bufferlength = nextByteModified.BytePositionInFile - this.Position;

                            // TEMPS
                            if (bufferlength < 0)
                            {
                                bufferlength = 1;
                            }

                            // EOF
                            if (bufferlength < ConstantReadOnly.COPY_BLOCK_SIZE)
                            {
                                buffer = new byte[bufferlength];
                            }

                            this._stream.Read(buffer, 0, buffer.Length);
                            NewStream.Write(buffer, 0, buffer.Length);
                        }

                        // Apply ByteAction!
                        switch (nextByteModified.Action)
                        {
                            case ByteAction.Added:
                                // TODO : IMPLEMENTING ADD BYTE
                                break;
                            case ByteAction.Deleted:
                                // NOTHING TODO we dont want to add deleted byte
                                this.Position++;
                                break;
                            case ByteAction.Modified:
                                this.Position++;
                                NewStream.WriteByte(nextByteModified.Byte.Value);
                                break;
                        }

                        // Read/Write the last section of file
                        if (nextByteModified.BytePositionInFile == SortedBM.Last().BytePositionInFile)
                        {
                            while (!this.EOF)
                            {
                                bufferlength = this._stream.Length - this.Position;

                                // EOF
                                if (bufferlength < ConstantReadOnly.COPY_BLOCK_SIZE)
                                {
                                    buffer = new byte[bufferlength];
                                }

                                this._stream.Read(buffer, 0, buffer.Length);
                                NewStream.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }

                    // Write new data to current stream
                    this.Position = 0;
                    NewStream.Position = 0;
                    buffer = new byte[ConstantReadOnly.COPY_BLOCK_SIZE];

                    while (!this.EOF)
                    {
                        // Set percent of progress
                        this.LongProcessProgress = (double)this.Position / (double)this.Length;

                        // Break process?
                        if (!this.IsOnLongProcess)
                        {
                            break;
                        }

                        bufferlength = this._stream.Length - this.Position;

                        // EOF
                        if (bufferlength < ConstantReadOnly.COPY_BLOCK_SIZE)
                        {
                            buffer = new byte[bufferlength];
                        }

                        NewStream.Read(buffer, 0, buffer.Length);
                        this._stream.Write(buffer, 0, buffer.Length);
                    }

                    this._stream.SetLength(NewStream.Length);

                    // dispose resource
                    NewStream.Close();
                    buffer = null;

                    // Launch event at process completed
                    this.IsOnLongProcess = false;
                    this.LongProcessProgressCompleted?.Invoke(this, new EventArgs());
                }

                // Launch event
                this.ChangesSubmited?.Invoke(this, new EventArgs());
            }
            else
            {
                throw new Exception("Cannot write to file.");
            }
        }
        #endregion SubmitChanges to file/stream

        #region Bytes modifications methods

        /// <summary>
        /// Clear changes and undo
        /// </summary>
        public void ClearUndoChange()
        {
            if (this.byteModifiedList != null)
            {
                this.byteModifiedList.Clear();
            }
        }

        /// <summary>
        /// Check if the byte in parameter are modified and return original Bytemodified from list
        /// </summary>
        public ByteModified CheckIfIsByteModified(long bytePositionInFile, ByteAction action = ByteAction.Modified)
        {
            foreach (ByteModified byteModified in this.byteModifiedList)
            {
                if (action != ByteAction.All)
                {
                    if (byteModified.BytePositionInFile == bytePositionInFile &&
                        byteModified.IsValid == true &&
                        byteModified.Action == action)
                    {
                        return byteModified;
                    }
                }
                else
                {
                    if (byteModified.BytePositionInFile == bytePositionInFile &&
                        byteModified.IsValid == true)
                    {
                        return byteModified;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Add/Modifiy a ByteModifed in the list of byte have changed
        /// </summary>
        public void AddByteModified(byte? @byte, long bytePositionInFile, long undoLenght = 1)
        {
            ByteModified bytemodifiedOriginal = this.CheckIfIsByteModified(bytePositionInFile, ByteAction.Modified);

            if (bytemodifiedOriginal != null)
            {
                this.byteModifiedList.Remove(bytemodifiedOriginal);
            }

            ByteModified byteModified = new ByteModified();

            byteModified.Byte = @byte;
            byteModified.UndoLenght = undoLenght;
            byteModified.BytePositionInFile = bytePositionInFile;
            byteModified.Action = ByteAction.Modified;

            this.byteModifiedList.Add(byteModified);
        }

        /// <summary>
        /// Add/Modifiy a ByteModifed in the list of byte have deleted
        /// </summary>
        public void AddByteDeleted(long bytePositionInFile, long length)
        {
            long position = bytePositionInFile;

            for (int i = 0; i < length; i++)
            {
                if (i % 100 == 0)
                {
                    Application.Current.DoEvents();
                }

                ByteModified bytemodifiedOriginal = this.CheckIfIsByteModified(position, ByteAction.All);

                if (bytemodifiedOriginal != null)
                {
                    this.byteModifiedList.Remove(bytemodifiedOriginal);
                }

                ByteModified byteModified = new ByteModified();

                byteModified.Byte = new byte();
                byteModified.UndoLenght = length;
                byteModified.BytePositionInFile = position;
                byteModified.Action = ByteAction.Deleted;

                this.byteModifiedList.Add(byteModified);

                position++;
            }
        }

        /// <summary>
        /// Return an IEnumerable ByteModified have action set to Modified
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ByteModified> ByteModifieds(ByteAction action)
        {
            foreach (ByteModified byteModified in this.byteModifiedList)
            {
                if (action != ByteAction.All)
                {
                    if (byteModified.Action == action)
                    {
                        yield return byteModified;
                    }
                }
                else
                {
                    yield return byteModified;
                }
            }
        }
        #endregion Bytes modifications methods

        #region Copy/Paste/Cut Methods

        /// <summary>
        /// Get the lenght of byte are selected (base 1)
        /// </summary>
        public long GetSelectionLenght(long selectionStart, long selectionStop)
        {
            if (selectionStop == -1 || selectionStop == -1)
            {
                return 0;
            }
            else if (selectionStart == selectionStop)
            {
                return 1;
            }
            else if (selectionStart > selectionStop)
            {
                return selectionStart - selectionStop + 1;
            }
            else
            {
                return selectionStop - selectionStart + 1;
            }
        }

        /// <summary>
        /// Copies the current selection in the hex box to the Clipboard.
        /// </summary>
        /// <param name="copyChange">Set tu true if you want onclude change in your copy. Set to false to copy directly from source</param>
        private byte[] GetCopyData(long selectionStart, long selectionStop, bool copyChange)
        {
            // Validation
            if (!this.CanCopy(selectionStart, selectionStop))
            {
                return new byte[0];
            }

            if (selectionStop == -1 || selectionStop == -1)
            {
                return new byte[0];
            }

            // Variable
            long byteStartPosition = -1;
            List<byte> bufferList = new List<byte>();

            // Set start position
            if (selectionStart == selectionStop)
            {
                byteStartPosition = selectionStart;
            }
            else if (selectionStart > selectionStop)
            {
                byteStartPosition = selectionStop;
            }
            else
            {
                byteStartPosition = selectionStart;
            }

            // set position
            this._stream.Position = byteStartPosition;

            // Exclude byte deleted from copy
            if (!copyChange)
            {
                byte[] buffer = new byte[this.GetSelectionLenght(selectionStart, selectionStop)];
                this._stream.Read(buffer, 0, Convert.ToInt32(this.GetSelectionLenght(selectionStart, selectionStop)));
                return buffer;
            }
            else
            {
                for (int i = 0; i < this.GetSelectionLenght(selectionStart, selectionStop); i++)
                {
                    ByteModified byteModified = this.CheckIfIsByteModified(this._stream.Position, ByteAction.All);

                    if (byteModified == null)
                    {
                        bufferList.Add((byte)this._stream.ReadByte());
                        continue;
                    }
                    else
                    {
                        switch (byteModified.Action)
                        {
                            case ByteAction.Added:
                                // TODO : IMPLEMENTING ADD BYTE
                                break;
                            case ByteAction.Deleted:
                                // NOTHING TODO we dont want to add deleted byte
                                break;
                            case ByteAction.Modified:
                                if (byteModified.IsValid)
                                {
                                    bufferList.Add(byteModified.Byte.Value);
                                }

                                break;
                        }
                    }

                    this._stream.Position++;
                }
            }

            return bufferList.ToArray();
        }

        /// <summary>
        /// Copy selection of byte to clipboard
        /// </summary>
        /// <param name="copyChange">Set tu true if you want onclude change in your copy. Set to false to copy directly from source</param>
        public void CopyToClipboard(CopyPasteMode copypastemode, long selectionStart, long selectionStop, bool copyChange = true)
        {
            if (!this.CanCopy(selectionStart, selectionStop))
            {
                return;
            }

            // Variables
            byte[] buffer = this.GetCopyData(selectionStart, selectionStop, copyChange);
            string sBuffer = string.Empty;

            DataObject da = new DataObject();

            switch (copypastemode)
            {
                case CopyPasteMode.ASCIIString:
                    sBuffer = ByteConverters.BytesToString(buffer);
                    da.SetText(sBuffer, TextDataFormat.Text);
                    break;
                case CopyPasteMode.HexaString:
                    sBuffer = ByteConverters.ByteToHex(buffer);
                    da.SetText(sBuffer, TextDataFormat.Text);
                    break;
                case CopyPasteMode.Byte:
                    throw new NotImplementedException();
            }

            // set memorystream (BinaryData) clipboard data
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer, 0, buffer.Length, false, true);
            da.SetData("BinaryData", ms);

            Clipboard.SetDataObject(da, true);

            this.DataCopiedToClipboard?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Copy selection of byte to a stream
        /// </summary>
        /// <param name="output">Output stream. Data will be copied at end of stream</param>
        /// <param name="copyChange">Set tu true if you want onclude change in your copy. Set to false to copy directly from source</param>
        public void CopyToStream(Stream output, long selectionStart, long selectionStop, bool copyChange = true)
        {
            if (!this.CanCopy(selectionStart, selectionStop))
            {
                return;
            }

            // Variables
            byte[] buffer = this.GetCopyData(selectionStart, selectionStop, copyChange);

            if (output.CanWrite)
            {
                output.Write(buffer, (int)output.Length, buffer.Length);
            }
            else
            {
                throw new Exception("An error is occurs when writing");
            }

            this.DataCopiedToStream?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Paste the string at position
        /// </summary>
        /// <param name="pasteString">The string to paste</param>
        /// <param name="startPosition">The position to start pasting</param>
        public void PasteNotInsert(long startPosition, string pasteString)
        {
            long lenght = pasteString.Length;
            this.Position = startPosition;

            foreach (char chr in pasteString)
            {
                if (!this.EOF)
                {
                    this.AddByteModified(ByteConverters.CharToByte(chr), this.Position, lenght);

                    this.Position++;
                }
                else
                {
                    break;
                }
            }

            this.DataPastedNotInserted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Paste the string at position
        /// </summary>
        /// <param name="pasteString">The string to paste</param>
        /// <param name="startPosition">The position to start pasting</param>
        public void PasteNotInsert(string pasteString)
        {
            this.PasteNotInsert(this.Position, pasteString);
        }

        #endregion Copy/Paste/Cut Methods

        #region Undo / Redo

        /// <summary>
        /// Undo last byteaction
        /// </summary>
        public void Undo()
        {
            if (this.CanUndo)
            {
                ByteModified last = this.byteModifiedList.Last<ByteModified>();

                for (int i = 0; i < last.UndoLenght; i++)
                {
                    this.byteModifiedList.RemoveAt(this.byteModifiedList.Count - 1);
                }

                this.Undone?.Invoke(this, new EventArgs());
            }
        }

        public long UndoCount => this.byteModifiedList.Count;

        public List<ByteModified> UndosList => this.byteModifiedList;

        /// <summary>
        /// Get or set for indicate if control CanUndo
        /// </summary>
        public bool IsUndoEnabled
        {
            get => this._isUndoEnabled;
            set => this._isUndoEnabled = value;
        }

        /// <summary>
        /// Check if the control can undone to a previous value
        /// </summary>
        /// <returns></returns>
        public bool CanUndo
        {
            get
            {
                if (this.IsUndoEnabled)
                {
                    return this.byteModifiedList.Count > 0;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion Undo / Redo

        #region Can do property...

        /// <summary>
        /// Return true if Copy method could be invoked.
        /// </summary>
        public bool CanCopy(long selectionStart, long selectionStop)
        {
            if (this.GetSelectionLenght(selectionStart, selectionStop) < 1 || !this.IsOpen)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update a value indicating whether the current stream is supporting writing.
        /// </summary>
        public bool CanWrite
        {
            get
            {
                if (this._stream != null)
                {
                    if (!this.ReadOnlyMode)
                    {
                        return this._stream.CanWrite;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Update a value indicating  whether the current stream is supporting reading.
        /// </summary>
        public bool CanRead
        {
            get
            {
                if (this._stream != null)
                {
                    return this._stream.CanRead;
                }

                return false;
            }
        }

        /// <summary>
        /// Update a value indicating  whether the current stream is supporting seeking.
        /// </summary>
        public bool CanSeek
        {
            get
            {
                if (this._stream != null)
                {
                    return this._stream.CanSeek;
                }

                return false;
            }
        }

        #endregion Can do Property...

        #region Find methods

        /// <summary>
        /// Find all occurance of string in stream and return an IEnumerable contening index when is find.
        /// </summary>
        public IEnumerable<long> FindIndexOf(string stringToFind, long startPosition = 0)
        {
            return this.FindIndexOf(ByteConverters.StringToByte(stringToFind), startPosition);
        }

        /// <summary>
        /// Find all occurance of byte[] in stream and return an IEnumerable contening index when is find.
        /// </summary>
        /// <param name="findtest"></param>
        public IEnumerable<long> FindIndexOf(byte[] bytesTofind, long startPosition = 0)
        {
            // start position checkup
            if (startPosition > this.Length)
            {
                startPosition = this.Length;
            }
            else if (startPosition < 0)
            {
                startPosition = 0;
            }

            // var
            this.Position = startPosition;
            byte[] buffer = new byte[ConstantReadOnly.FIND_BLOCK_SIZE];
            IEnumerable<long> findindex;
            List<long> indexList = new List<long>();

            // Launch event at process strated
            this.IsOnLongProcess = true;
            this.LongProcessProgressStarted?.Invoke(this, new EventArgs());

            // start find
            for (long i = startPosition; i < this.Length; i++)
            {
                // Do not freeze UI...
                if (i % 2000 == 0)
                {
                    this.LongProcessProgress = (double)this.Position / this.Length;
                }

                // Break long process if needed
                if (!this.IsOnLongProcess)
                {
                    break;
                }

                if ((byte)this.ReadByte() == bytesTofind[0])
                {
                    // position correction after read one byte
                    this.Position--;
                    i--;

                    if (buffer.Length > this.Length - this.Position)
                    {
                        buffer = new byte[this.Length - this.Position];
                    }

                    // read buffer and find
                    this._stream.Read(buffer, 0, buffer.Length);
                    findindex = buffer.FindIndexOf(bytesTofind);

                    // if byte if find add to list
                    if (findindex.Count() > 0)
                    {
                        foreach (long index in findindex)
                        {
                            indexList.Add(index + i + 1);
                        }
                    }

                    // position correction
                    i += buffer.Length;
                }
            }

            // Yield return all finded occurence
            foreach (long index in indexList)
            {
                yield return index;
            }

            // Launch event at process completed
            this.IsOnLongProcess = false;
            this.LongProcessProgressCompleted?.Invoke(this, new EventArgs());
        }

        #endregion Find methods

        #region Long process progress

        /// <summary>
        /// Get if byteprovider is on a long process. Set to false to cancel all process.
        /// </summary>
        public bool IsOnLongProcess
        {
            get => this._isOnLongProcess;

            set => this._isOnLongProcess = value;
        }

        /// <summary>
        /// Get the long progress percent of job.
        /// When set (internal) launch event LongProcessProgressChanged
        /// </summary>
        public double LongProcessProgress
        {
            get => this._longProcessProgress;

            internal set
            {
                this._longProcessProgress = value;

                this.LongProcessProgressChanged?.Invoke(value, new EventArgs());
            }
        }
        #endregion Long process progress

    }
}
