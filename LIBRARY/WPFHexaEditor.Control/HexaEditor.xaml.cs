namespace WPFHexaEditor.Control
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using WPFHexaEditor.Core;
    using WPFHexaEditor.Core.Bytes;
    using WPFHexaEditor.Core.MethodExtention;
    using WPFHexaEditor.Core.ROMTable;

    /// <summary>
    /// 2016 - Derek Tremblay (derektremblay666@gmail.com)
    /// WPF Hexadecimal editor control
    /// </summary>
    public partial class HexaEditor : UserControl
    {
        private const double lineInfoHeight = 22;
        private ByteProvider _provider = null;
        private double _scrollLargeChange = 100;
        private List<long> markedPositionList = new List<long>();
        private long _rightClickBytePosition = -1;
        private TBLStream _TBLCharacterTable = null;

        // Event
        public event EventHandler SelectionStartChanged;

        public event EventHandler SelectionStopChanged;

        public event EventHandler SelectionLenghtChanged;

        public event EventHandler DataCopied;

        public HexaEditor()
        {
            this.InitializeComponent();

            this.RefreshView(true);

            this.StatusBarGrid.DataContext = this;
        }

        #region Miscellaneous property/methods
        public double ScrollLargeChange
        {
            get => this._scrollLargeChange;

            set
            {
                this._scrollLargeChange = value;

                this.UpdateVerticalScroll();
            }
        }

        #endregion Miscellaneous property/methods

        #region Characters tables property/methods

        /// <summary>
        /// Type of caracter table are used un hexacontrol.
        /// For now, somes character table can be readonly but will change in future
        /// </summary>
        public CharacterTable TypeOfCharacterTable
        {
            get => (CharacterTable)this.GetValue(TypeOfCharacterTableProperty);
            set => this.SetValue(TypeOfCharacterTableProperty, value);
        }

        // Using a DependencyProperty as the backing store for TypeOfCharacterTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeOfCharacterTableProperty =
            DependencyProperty.Register("TypeOfCharacterTable", typeof(CharacterTable), typeof(HexaEditor),
                new FrameworkPropertyMetadata(CharacterTable.ASCII,
                    new PropertyChangedCallback(TypeOfCharacterTable_PropertyChanged)));

        private static void TypeOfCharacterTable_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            ctrl.RefreshView();
        }

        /// <summary>
        /// Load TBL Character table file in control. (Used for ROM reverse engineering)
        /// Change CharacterTable property for use.
        /// </summary>
        public void LoadTBLFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                this._TBLCharacterTable = new TBLStream();
                this._TBLCharacterTable.Load(fileName);

                this.TBLLabel.Visibility = Visibility.Visible;
                this.TBLLabel.ToolTip = $"TBL file : {fileName}";

                this.RefreshView();
            }
        }
        #endregion Characters tables

        #region ReadOnly property/event

        /// <summary>
        /// Put the control on readonly mode.
        /// </summary>
        public bool ReadOnlyMode
        {
            get => (bool)this.GetValue(ReadOnlyModeProperty);
            set => this.SetValue(ReadOnlyModeProperty, value);
        }

        public static readonly DependencyProperty ReadOnlyModeProperty =
            DependencyProperty.Register("ReadOnlyMode", typeof(bool), typeof(HexaEditor),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(ReadOnlyMode_PropertyChanged)));

        private static void ReadOnlyMode_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.RefreshView(false);

                // TODO: ADD VISIBILITY CONVERTER FOR BINDING READONLY PROPERTY
                if (ctrl.ReadOnlyMode)
                {
                    ctrl.ReadOnlyLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    ctrl.ReadOnlyLabel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Provider_ReadOnlyChanged(object sender, EventArgs e)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this.ReadOnlyMode = this._provider.ReadOnlyMode;
            }
        }
        #endregion ReadOnly property/event

        #region Add modify delete bytes methods/event
        private void Control_ByteModified(object sender, EventArgs e)
        {
            HexByteControl ctrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
            {
                this._provider.AddByteModified(sbCtrl.Byte, sbCtrl.BytePositionInFile);
                this.SetScrollMarker(sbCtrl.BytePositionInFile, ScrollMarker.ByteModified);
            }
            else if (ctrl != null)
            {
                this._provider.AddByteModified(ctrl.Byte, ctrl.BytePositionInFile);
                this.SetScrollMarker(ctrl.BytePositionInFile, ScrollMarker.ByteModified);
            }

            this.UpdateStatusBar();
        }

        /// <summary>
        /// Delete selection, add scroll marker and update control
        /// </summary>
        public void DeleteSelection()
        {
            if (!this.CanDelete())
            {
                return;
            }

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                long position = -1;

                if (this.SelectionStart > this.SelectionStop)
                {
                    position = this.SelectionStop;
                }
                else
                {
                    position = this.SelectionStart;
                }

                this._provider.AddByteDeleted(position, this.SelectionLenght);

                this.SetScrollMarker(position, ScrollMarker.ByteDeleted);

                this.UpdateByteModified();
                this.UpdateSelection();
                this.UpdateStatusBar();
            }
        }
        #endregion Add modify delete bytes methods/event

        #region Lines methods

        /// <summary>
        /// Obtain the max line for verticalscrollbar
        /// </summary>
        public long GetMaxLine()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                return this._provider.Length / this.BytePerLine;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the number of row visible in control
        /// </summary>
        public long GetMaxVisibleLine()
        {
            return (long)(this.StringDataStackPanel.ActualHeight / lineInfoHeight); // + 1; //TEST
        }
        #endregion Lines methods

        #region Selection Property/Methods/Event

        /// <summary>
        /// Get the selected line of focus control
        /// </summary>
        public long SelectionLine
        {
            get => (long)this.GetValue(SelectionLineProperty);
            internal set => this.SetValue(SelectionLineProperty, value);
        }

        public static readonly DependencyProperty SelectionLineProperty =
            DependencyProperty.Register("SelectionLine", typeof(long), typeof(HexaEditor),
                new FrameworkPropertyMetadata(0L));

        private void LineInfoLabel_MouseMove(object sender, MouseEventArgs e)
        {
            Label line = sender as Label;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.SelectionStop = ByteConverters.HexLiteralToLong(line.Content.ToString()) + this.BytePerLine - 1;
            }
        }

        private void LineInfoLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label line = sender as Label;

            this.SelectionStart = ByteConverters.HexLiteralToLong(line.Content.ToString());
            this.SelectionStop = this.SelectionStart + this.BytePerLine - 1;
        }

        private void Control_MovePageDown(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long byteToMove = this.BytePerLine * this.GetMaxVisibleLine();
            long test = this.SelectionStart + byteToMove;

            // TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test < this._provider.Length)
                {
                    this.SelectionStart += byteToMove;
                }
                else
                {
                    this.SelectionStart = this._provider.Length;
                }
            }
            else
            {
                if (this.SelectionStart > this.SelectionStop)
                {
                    this.SelectionStart = this.SelectionStop;
                }
                else
                {
                    this.SelectionStop = this.SelectionStart;
                }

                if (test < this._provider.Length)
                {
                    this.SelectionStart += byteToMove;
                    this.SelectionStop += byteToMove;
                }
            }

            if (this.SelectionStart > this.GetLastVisibleBytePosition())
            {
                this.VerticalScrollBar.Value++;
            }

            if (hbCtrl != null || sbCtrl != null)
            {
                this.VerticalScrollBar.Value += this.GetMaxVisibleLine() - 1;
                this.SetFocusHexDataPanel(this.SelectionStart);
            }
        }

        private void Control_ByteDeleted(object sender, EventArgs e)
        {
            HexByteControl ctrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            this.DeleteSelection();
        }

        private void Control_EscapeKey(object sender, EventArgs e)
        {
            this.UnSelectAll();
            this.UnHighLightAll();
        }

        private void Control_MovePageUp(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long byteToMove = this.BytePerLine * this.GetMaxVisibleLine();
            long test = this.SelectionStart - byteToMove;

            // TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                {
                    this.SelectionStart -= byteToMove;
                }
                else
                {
                    this.SelectionStart = 0;
                }
            }
            else
            {
                if (this.SelectionStart > this.SelectionStop)
                {
                    this.SelectionStart = this.SelectionStop;
                }
                else
                {
                    this.SelectionStop = this.SelectionStart;
                }

                if (test > -1)
                {
                    this.SelectionStart -= byteToMove;
                    this.SelectionStop -= byteToMove;
                }
            }

            if (this.SelectionStart < this.GetFirstVisibleBytePosition())
            {
                this.VerticalScrollBar.Value--;
            }

            if (hbCtrl != null || sbCtrl != null)
            {
                this.VerticalScrollBar.Value -= this.GetMaxVisibleLine() - 1;
                this.SetFocusHexDataPanel(this.SelectionStart);
            }
        }

        private void Control_MoveDown(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = this.SelectionStart + this.BytePerLine;

            // TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test < this._provider.Length)
                {
                    this.SelectionStart += this.BytePerLine;
                }
                else
                {
                    this.SelectionStart = this._provider.Length;
                }
            }
            else
            {
                if (this.SelectionStart > this.SelectionStop)
                {
                    this.SelectionStart = this.SelectionStop;
                }
                else
                {
                    this.SelectionStop = this.SelectionStart;
                }

                if (test < this._provider.Length)
                {
                    this.SelectionStart += this.BytePerLine;
                    this.SelectionStop += this.BytePerLine;
                }
            }

            if (this.SelectionStart > this.GetLastVisibleBytePosition())
            {
                this.VerticalScrollBar.Value++;
            }

            if (hbCtrl != null)
            {
                this.SetFocusHexDataPanel(this.SelectionStart);
            }

            if (sbCtrl != null)
            {
                this.SetFocusStringDataPanel(this.SelectionStart);
            }
        }

        private void Control_MoveUp(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = this.SelectionStart - this.BytePerLine;

            // TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                {
                    this.SelectionStart -= this.BytePerLine;
                }
                else
                {
                    this.SelectionStart = 0;
                }
            }
            else
            {
                if (this.SelectionStart > this.SelectionStop)
                {
                    this.SelectionStart = this.SelectionStop;
                }
                else
                {
                    this.SelectionStop = this.SelectionStart;
                }

                if (test > -1)
                {
                    this.SelectionStart -= this.BytePerLine;
                    this.SelectionStop -= this.BytePerLine;
                }
            }

            if (this.SelectionStart < this.GetFirstVisibleBytePosition())
            {
                this.VerticalScrollBar.Value--;
            }

            if (hbCtrl != null)
            {
                this.SetFocusHexDataPanel(this.SelectionStart);
            }

            if (sbCtrl != null)
            {
                this.SetFocusStringDataPanel(this.SelectionStart);
            }
        }

        private void Control_Click(object sender, EventArgs e)
        {
            StringByteControl sbCtrl = sender as StringByteControl;
            HexByteControl ctrl = sender as HexByteControl;

            if (ctrl != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    this.SelectionStop = ctrl.BytePositionInFile;
                }
                else
                {
                    this.SelectionStart = ctrl.BytePositionInFile;
                    this.SelectionStop = ctrl.BytePositionInFile;
                }

                this.UpdateSelectionColorMode(FirstColor.HexByteData);
            }

            if (sbCtrl != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    this.SelectionStop = sbCtrl.BytePositionInFile;
                }
                else
                {
                    this.SelectionStart = sbCtrl.BytePositionInFile;
                    this.SelectionStop = sbCtrl.BytePositionInFile;
                }

                this.UpdateSelectionColorMode(FirstColor.StringByteData);
            }
        }

        private void Control_MouseSelection(object sender, EventArgs e)
        {
            // Prevent false mouse selection on file open
            if (this.SelectionStart == -1)
            {
                return;
            }

            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (hbCtrl != null)
            {
                this.UpdateSelectionColorMode(FirstColor.HexByteData);

                if (hbCtrl.BytePositionInFile != -1)
                {
                    this.SelectionStop = hbCtrl.BytePositionInFile;
                }
                else
                {
                    this.SelectionStop = this.GetLastVisibleBytePosition();
                }
            }

            if (sbCtrl != null)
            {
                this.UpdateSelectionColorMode(FirstColor.StringByteData);

                if (sbCtrl.BytePositionInFile != -1)
                {
                    this.SelectionStop = sbCtrl.BytePositionInFile;
                }
                else
                {
                    this.SelectionStop = this.GetLastVisibleBytePosition();
                }
            }

            this.UpdateSelection();
        }

        private void BottomRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.VerticalScrollBar.Value += 5;
            }
        }

        private void TopRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.VerticalScrollBar.Value -= 5;
            }
        }

        /// <summary>
        /// Un highlight all byte as highlighted with find all methods
        /// </summary>
        public void UnHighLightAll()
        {
            this.markedPositionList.Clear();
            this.UpdateHighLightByte();
            this.ClearScrollMarker(ScrollMarker.SearchHighLight);
        }

        /// <summary>
        /// Set the start byte position of selection
        /// </summary>
        public long SelectionStart
        {
            get => (long)this.GetValue(SelectionStartProperty);
            set => this.SetValue(SelectionStartProperty, value);
        }

        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.Register("SelectionStart", typeof(long), typeof(HexaEditor),
                new FrameworkPropertyMetadata(-1L, new PropertyChangedCallback(SelectionStart_ChangedCallBack),
                    new CoerceValueCallback(SelectionStart_CoerceValueCallBack)));

        private static object SelectionStart_CoerceValueCallBack(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            long value = (long)baseValue;

            if (value < -1)
            {
                return -1L;
            }

            if (!ByteProvider.CheckIsOpen(ctrl._provider))
            {
                return -1L;
            }
            else
            {
                return baseValue;
            }
        }

        private static void SelectionStart_ChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateSelection();
                ctrl.UpdateSelectionLine();
                ctrl.SetScrollMarker(0, ScrollMarker.SelectionStart);

                ctrl.SelectionStartChanged?.Invoke(ctrl, new EventArgs());

                ctrl.SelectionLenghtChanged?.Invoke(ctrl, new EventArgs());
            }
        }

        /// <summary>
        /// Set the start byte position of selection
        /// </summary>
        public long SelectionStop
        {
            get => (long)this.GetValue(SelectionStopProperty);
            set => this.SetValue(SelectionStopProperty, value);
        }

        public static readonly DependencyProperty SelectionStopProperty =
            DependencyProperty.Register("SelectionStop", typeof(long), typeof(HexaEditor),
                new FrameworkPropertyMetadata(-1L, new PropertyChangedCallback(SelectionStop_ChangedCallBack),
                    new CoerceValueCallback(SelectionStop_CoerceValueCallBack)));

        private static object SelectionStop_CoerceValueCallBack(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            long value = (long)baseValue;

            // Debug.Print($"SelectionStop : {value.ToString()}");
            if (value < -1)
            {
                return -1L;
            }

            if (!ByteProvider.CheckIsOpen(ctrl._provider))
            {
                return -1L;
            }

            if (value >= ctrl._provider.Length)
            {
                return ctrl._provider.Length;
            }

            return baseValue;
        }

        private static void SelectionStop_ChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateSelection();
                ctrl.UpdateSelectionLine();

                ctrl.SelectionStopChanged?.Invoke(ctrl, new EventArgs());

                ctrl.SelectionLenghtChanged?.Invoke(ctrl, new EventArgs());
            }
        }

        /// <summary>
        /// Reset selection to -1
        /// </summary>
        public void UnSelectAll()
        {
            this.SelectionStart = -1;
            this.SelectionStop = -1;
        }

        /// <summary>
        /// Select the entire file
        /// If file are closed the selection will be set to -1
        /// </summary>
        public void SelectAll()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this.SelectionStart = 0;
                this.SelectionStop = this._provider.Length;
            }
            else
            {
                this.SelectionStart = -1;
                this.SelectionStop = -1;
            }

            this.UpdateSelection();
        }

        /// <summary>
        /// Get the lenght of byte are selected (base 1)
        /// </summary>
        public long SelectionLenght
        {
            get
            {
                if (this.SelectionStop == -1 || this.SelectionStop == -1)
                {
                    return 0;
                }
                else if (this.SelectionStart == this.SelectionStop)
                {
                    return 1;
                }
                else if (this.SelectionStart > this.SelectionStop)
                {
                    return this.SelectionStart - this.SelectionStop + 1;
                }
                else
                {
                    return this.SelectionStop - this.SelectionStart + 1;
                }
            }
        }

        /// <summary>
        /// Get byte array from current selection
        /// </summary>
        public byte[] SelectionByteArray
        {
            get
            {
                MemoryStream ms = new MemoryStream();

                this.CopyToStream(ms, true);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get string from current selection
        /// </summary>
        public string SelectionString
        {
            get
            {
                MemoryStream ms = new MemoryStream();

                this.CopyToStream(ms, true);

                return ByteConverters.BytesToString(ms.ToArray());
            }
        }

        /// <summary>
        /// Get Hexadecimal from current selection
        /// </summary>
        public string SelectionHexa
        {
            get
            {
                MemoryStream ms = new MemoryStream();

                this.CopyToStream(ms, true);

                return ByteConverters.ByteToHex(ms.ToArray());
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) // UP
            {
                this.VerticalScrollBar.Value--;
            }

            if (e.Delta < 0) // Down
            {
                this.VerticalScrollBar.Value++;
            }
        }

        private void Control_MoveRight(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = this.SelectionStart + 1;

            // TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test <= this._provider.Length)
                {
                    this.SelectionStart++;
                }
                else
                {
                    this.SelectionStart = this._provider.Length;
                }
            }
            else
            {
                if (this.SelectionStart > this.SelectionStop)
                {
                    this.SelectionStart = this.SelectionStop;
                }
                else
                {
                    this.SelectionStop = this.SelectionStart;
                }

                if (test < this._provider.Length)
                {
                    this.SelectionStart++;
                    this.SelectionStop++;
                }
            }

            // Validation and refresh
            if (this.SelectionStart >= this._provider.Length)
            {
                this.SelectionStart = this._provider.Length;
            }

            if (this.SelectionStart > this.GetLastVisibleBytePosition())
            {
                this.VerticalScrollBar.Value++;
            }

            if (hbCtrl != null)
            {
                this.SetFocusHexDataPanel(this.SelectionStart);
            }

            if (sbCtrl != null)
            {
                this.SetFocusStringDataPanel(this.SelectionStart);
            }
        }

        private void Control_MoveLeft(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = this.SelectionStart - 1;

            // TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                {
                    this.SelectionStart--;
                }
                else
                {
                    this.SelectionStart = 0;
                }
            }
            else
            {
                if (this.SelectionStart > this.SelectionStop)
                {
                    this.SelectionStart = this.SelectionStop;
                }
                else
                {
                    this.SelectionStop = this.SelectionStart;
                }

                if (test > -1)
                {
                    this.SelectionStart--;
                    this.SelectionStop--;
                }
            }

            // Validation and refresh
            if (this.SelectionStart < 0)
            {
                this.SelectionStart = 0;
            }

            if (this.SelectionStart < this.GetFirstVisibleBytePosition())
            {
                this.VerticalScrollBar.Value--;
            }

            if (hbCtrl != null)
            {
                this.SetFocusHexDataPanel(this.SelectionStart);
            }

            if (sbCtrl != null)
            {
                this.SetFocusStringDataPanel(this.SelectionStart);
            }
        }

        private void Control_MovePrevious(object sender, EventArgs e)
        {
            HexByteControl hexByteCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
            {
                sbCtrl.IsSelected = false;
                this.SetFocusStringDataPanel(sbCtrl.BytePositionInFile - 1);
            }

            if (hexByteCtrl != null)
            {
                hexByteCtrl.IsSelected = false;
                this.SetFocusHexDataPanel(hexByteCtrl.BytePositionInFile - 1);
            }

            if (hexByteCtrl != null || sbCtrl != null)
            {
                this.SelectionStart--;
                this.SelectionStop--;
                this.UpdateByteModified();
            }
        }

        private void Control_MoveNext(object sender, EventArgs e)
        {
            HexByteControl hexByteCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
            {
                sbCtrl.IsSelected = false;
                this.SetFocusStringDataPanel(sbCtrl.BytePositionInFile + 1);
            }

            if (hexByteCtrl != null)
            {
                hexByteCtrl.IsSelected = false;
                this.SetFocusHexDataPanel(hexByteCtrl.BytePositionInFile + 1);
            }

            if (hexByteCtrl != null || sbCtrl != null)
            {
                this.SelectionStart++;
                this.SelectionStop++;
                this.UpdateByteModified();
            }
        }
        #endregion Selection Property/Methods

        #region Copy/Paste/Cut Methods

        /// <summary>
        /// Return true if Copy method could be invoked.
        /// </summary>
        public bool CanCopy()
        {
            if (this.SelectionLenght < 1 || !ByteProvider.CheckIsOpen(this._provider))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return true if delete method could be invoked.
        /// </summary>
        public bool CanDelete()
        {
            return this.CanCopy() && !this.ReadOnlyMode;
        }

        /// <summary>
        /// Copy to clipboard with default CopyPasteMode.ASCIIString
        /// </summary>
        public void CopyToClipboard()
        {
            this.CopyToClipboard(CopyPasteMode.ASCIIString);
        }

        /// <summary>
        /// Copy to clipboard the current selection with actual change in control
        /// </summary>
        public void CopyToClipboard(CopyPasteMode copypastemode)
        {
            this.CopyToClipboard(copypastemode, this.SelectionStart, this.SelectionStop, true);
        }

        /// <summary>
        /// Copy to clipboard
        /// </summary>
        public void CopyToClipboard(CopyPasteMode copypastemode, long selectionStart, long selectionStop, bool copyChange)
        {
            if (!this.CanCopy())
            {
                return;
            }

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this._provider.CopyToClipboard(copypastemode, this.SelectionStart, this.SelectionStop, copyChange);
            }
        }

        /// <summary>
        /// Copy selection to a stream
        /// </summary>
        /// <param name="output">Output stream is not closed after copy</param>
        public void CopyToStream(Stream output, bool copyChange)
        {
            this.CopyToStream(output, this.SelectionStart, this.SelectionStop, copyChange);
        }

        /// <summary>
        /// Copy selection to a stream
        /// </summary>
        /// <param name="output">Output stream is not closed after copy</param>
        public void CopyToStream(Stream output, long selectionStart, long selectionStop, bool copyChange)
        {
            if (!this.CanCopy())
            {
                return;
            }

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this._provider.CopyToStream(output, selectionStart, selectionStop, copyChange);
            }
        }

        /// <summary>
        /// Occurs when data is copied in byteprovider instance
        /// </summary>
        private void Provider_DataCopied(object sender, EventArgs e)
        {
            this.DataCopied?.Invoke(sender, e);
        }

        #endregion Copy/Paste/Cut Methods

        #region Set position methods

        /// <summary>
        /// Set position of cursor
        /// </summary>
        public void SetPosition(long position, long byteLenght)
        {
            // TODO : selected hexbytecontrol
            this.SelectionStart = position;
            this.SelectionStop = position + byteLenght - 1;

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this.VerticalScrollBar.Value = this.GetLineNumber(position);
            }
            else
            {
                this.VerticalScrollBar.Value = 0;
            }

            // RefreshView(true);
        }

        /// <summary>
        /// Get the line number of position in parameter
        /// </summary>
        public double GetLineNumber(long position)
        {
            return position / this.BytePerLine;
        }

        /// <summary>
        /// Set position in control at position in parameter
        /// </summary>
        public void SetPosition(long position)
        {
            this.SetPosition(position, 0);
        }

        /// <summary>
        /// Set position in control at position in parameter
        /// </summary>
        public void SetPosition(string hexLiteralPosition)
        {
            try
            {
                this.SetPosition(ByteConverters.HexLiteralToLong(hexLiteralPosition));
            }
            catch
            {
                throw new InvalidCastException("Invalid hexa string");
            }
        }

        /// <summary>
        /// Set position in control at position in parameter with specified selected lenght
        /// </summary>
        public void SetPosition(string hexLiteralPosition, long byteLenght)
        {
            try
            {
                this.SetPosition(ByteConverters.HexLiteralToLong(hexLiteralPosition), byteLenght);
            }
            catch
            {
                throw new InvalidCastException("Invalid hexa string");
            }
        }
        #endregion Set position methods

        #region Visibility property

        /// <summary>
        /// Set or Get value for change visibility of hexadecimal panel
        /// </summary>
        public Visibility HexDataVisibility
        {
            get => (Visibility)this.GetValue(HexDataVisibilityProperty);
            set => this.SetValue(HexDataVisibilityProperty, value);
        }

        public static readonly DependencyProperty HexDataVisibilityProperty =
            DependencyProperty.Register("HexDataVisibility", typeof(Visibility), typeof(HexaEditor),
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(HexDataVisibility_PropertyChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static object Visibility_CoerceValue(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)baseValue;

            if (value == Visibility.Hidden)
            {
                return Visibility.Collapsed;
            }

            return value;
        }

        private static void HexDataVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.HexDataStackPanel.Visibility = Visibility.Visible;

                    if (ctrl.HeaderVisibility == Visibility.Visible)
                    {
                        ctrl.HexHeaderStackPanel.Visibility = Visibility.Visible;
                    }

                    break;
                case Visibility.Collapsed:
                    ctrl.HexDataStackPanel.Visibility = Visibility.Collapsed;
                    ctrl.HexHeaderStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of hexadecimal header
        /// </summary>
        public Visibility HeaderVisibility
        {
            get => (Visibility)this.GetValue(HeaderVisibilityProperty);
            set => this.SetValue(HeaderVisibilityProperty, value);
        }

        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(Visibility), typeof(HexaEditor),
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(HeaderVisibility_PropertyChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void HeaderVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    if (ctrl.HexDataVisibility == Visibility.Visible)
                    {
                        ctrl.HexHeaderStackPanel.Visibility = Visibility.Visible;
                    }

                    break;
                case Visibility.Collapsed:
                    ctrl.HexHeaderStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of string panel
        /// </summary>
        public Visibility StringDataVisibility
        {
            get => (Visibility)this.GetValue(StringDataVisibilityProperty);
            set => this.SetValue(StringDataVisibilityProperty, value);
        }

        public static readonly DependencyProperty StringDataVisibilityProperty =
            DependencyProperty.Register("StringDataVisibility", typeof(Visibility), typeof(HexaEditor),
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(StringDataVisibility_ValidateValue),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void StringDataVisibility_ValidateValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.StringDataStackPanel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.StringDataStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of vertical scroll bar
        /// </summary>
        public Visibility VerticalScrollBarVisibility
        {
            get => (Visibility)this.GetValue(VerticalScrollBarVisibilityProperty);
            set => this.SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof(Visibility), typeof(HexaEditor),
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(VerticalScrollBarVisibility_ValueChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void VerticalScrollBarVisibility_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.VerticalScrollBar.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.VerticalScrollBar.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of status bar
        /// </summary>
        public Visibility StatusBarVisibility
        {
            get => (Visibility)this.GetValue(StatusBarVisibilityProperty);
            set => this.SetValue(StatusBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty StatusBarVisibilityProperty =
            DependencyProperty.Register("StatusBarVisibility", typeof(Visibility), typeof(HexaEditor),
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(StatusBarVisibility_ValueChange),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void StatusBarVisibility_ValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.StatusBarGrid.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.StatusBarGrid.Visibility = Visibility.Collapsed;
                    break;
            }

            ctrl.RefreshView(false);
        }
        #endregion Visibility standard property

        #region Undo / Redo

        /// <summary>
        /// Clear undo and change
        /// </summary>
        public void ClearAllChange()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this._provider.ClearUndoChange();
            }
        }

        /// <summary>
        /// Make undo of last the last bytemodified
        /// </summary>
        public void Undo(int repeat = 1)
        {
            this.UnSelectAll();

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                for (int i = 0; i < repeat; i++)
                {
                    this._provider.Undo();
                }

                this.RefreshView(false, false);
            }
        }

        public long UndoCount
        {
            get
            {
                if (ByteProvider.CheckIsOpen(this._provider))
                {
                    return this._provider.UndoCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public List<ByteModified> UndosList
        {
            get
            {
                if (ByteProvider.CheckIsOpen(this._provider))
                {
                    return this._provider.UndosList;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion Undo / Redo

        #region Open, Close, Save... Methods/Property

        private void Provider_ChangesSubmited(object sender, EventArgs e)
        {
            // Refresh filename
            var filename = this.FileName;
            this.Close();
            this.FileName = filename;
        }

        private void ProviderStream_ChangesSubmited(object sender, EventArgs e)
        {
            // Refresh stream
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                MemoryStream stream = new MemoryStream(this._provider.Stream.ToArray());
                this.Close();
                this.OpenStream(stream);
            }
        }

        /// <summary>
        /// Set or Get the file with the control will show hex
        /// </summary>
        public string FileName
        {
            get => (string)this.GetValue(FileNameProperty);
            set => this.SetValue(FileNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for FileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(HexaEditor),
                new FrameworkPropertyMetadata(string.Empty,
                    new PropertyChangedCallback(FileName_PropertyChanged)));

        private static void FileName_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            ctrl.Close();
            ctrl.OpenFile((string)e.NewValue);
        }

        /// <summary>
        /// Set the MemoryStream are used by ByteProvider
        /// </summary>
        public MemoryStream Stream
        {
            get => (MemoryStream)this.GetValue(StreamProperty);
            set => this.SetValue(StreamProperty, value);
        }

        // Using a DependencyProperty as the backing store for Stream.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StreamProperty =
            DependencyProperty.Register("Stream", typeof(MemoryStream), typeof(HexaEditor),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(Stream_PropertyChanged)));

        private static void Stream_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            ctrl.Close();
            ctrl.OpenStream((MemoryStream)e.NewValue);
        }

        /// <summary>
        /// Close file and clear control
        /// ReadOnlyMode is reset to false
        /// </summary>
        public void Close()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this._provider.Close();

                try
                {
                    this.FileName = string.Empty;
                }
                catch
                {
                }

                this.ReadOnlyMode = false;
                this.VerticalScrollBar.Value = 0;

                // _TBLCharacterTable = null;
            }

            this.UnHighLightAll();
            this.ClearAllChange();
            this.ClearAllScrollMarker();
            this.UnSelectAll();
            this.RefreshView();
        }

        /// <summary>
        /// Save to the current stream
        /// TODO: Add save as another stream...
        /// </summary>
        public void SubmitChanges()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                if (!this._provider.ReadOnlyMode)
                {
                    this._provider.SubmitChanges();
                }
            }
        }

        /// <summary>
        /// Open file name
        /// </summary>
        /// <param name="filename"></param>
        private void OpenFile(string filename)
        {
            if (File.Exists(filename))
            {
                this.Close();

                this._provider = new ByteProvider(filename);
                this._provider.ReadOnlyChanged += this.Provider_ReadOnlyChanged;
                this._provider.DataCopiedToClipboard += this.Provider_DataCopied;
                this._provider.ChangesSubmited += this.Provider_ChangesSubmited;
                this._provider.LongProcessProgressChanged += this.Provider_LongProcessProgressChanged;
                this._provider.LongProcessProgressStarted += this.Provider_LongProcessProgressStarted;
                this._provider.LongProcessProgressCompleted += this.Provider_LongProcessProgressCompleted;

                this.UpdateVerticalScroll();
                this.UpdateHexHeader();

                this.RefreshView(true);

                this.UnSelectAll();

                this.UpdateSelectionColorMode(FirstColor.HexByteData);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// Open file name
        /// </summary>
        /// <param name="filename"></param>
        private void OpenStream(MemoryStream stream)
        {
            if (stream.CanRead)
            {
                this.Close();

                this._provider = new ByteProvider(stream);
                this._provider.ReadOnlyChanged += this.Provider_ReadOnlyChanged;
                this._provider.DataCopiedToClipboard += this.Provider_DataCopied;
                this._provider.ChangesSubmited += this.ProviderStream_ChangesSubmited;
                this._provider.LongProcessProgressChanged += this.Provider_LongProcessProgressChanged;
                this._provider.LongProcessProgressStarted += this.Provider_LongProcessProgressStarted;
                this._provider.LongProcessProgressCompleted += this.Provider_LongProcessProgressCompleted;

                this.UpdateVerticalScroll();
                this.UpdateHexHeader();

                this.RefreshView(true);

                this.UnSelectAll();

                this.UpdateSelectionColorMode(FirstColor.HexByteData);
            }
            else
            {
                throw new Exception("Can't read MemoryStream");
            }
        }

        private void Provider_LongProcessProgressCompleted(object sender, EventArgs e)
        {
            this.LongProgressProgressBar.Visibility = Visibility.Collapsed;
            this.CancelLongProcessButton.Visibility = Visibility.Collapsed;
        }

        private void Provider_LongProcessProgressStarted(object sender, EventArgs e)
        {
            this.LongProgressProgressBar.Visibility = Visibility.Visible;
            this.CancelLongProcessButton.Visibility = Visibility.Visible;
        }

        private void Provider_LongProcessProgressChanged(object sender, EventArgs e)
        {
            // Update progress bar
            this.LongProgressProgressBar.Value = (double)sender;
            Application.Current.DoEvents();
        }

        private void CancelLongProcessButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add messagebox confirmation
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this._provider.IsOnLongProcess = false;
            }
        }

        /// <summary>
        /// Check if byteprovider is on long progress and update control
        /// </summary>
        private void CheckProviderIsOnProgress()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                if (!this._provider.IsOnLongProcess)
                {
                    this.CancelLongProcessButton.Visibility = Visibility.Collapsed;
                    this.LongProgressProgressBar.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.CancelLongProcessButton.Visibility = Visibility.Collapsed;
                this.LongProgressProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Update/Refresh view methods/event

        /// <summary>
        /// Get or set the number of byte are show in control
        /// </summary>
        public int BytePerLine
        {
            get => (int)this.GetValue(BytePerLineProperty);
            set => this.SetValue(BytePerLineProperty, value);
        }

        public static readonly DependencyProperty BytePerLineProperty =
            DependencyProperty.Register("BytePerLine", typeof(int), typeof(HexaEditor),
                new FrameworkPropertyMetadata(16, new PropertyChangedCallback(BytePerLine_PropertyChanged),
                    new CoerceValueCallback(BytePerLine_CoerceValue)));

        private static object BytePerLine_CoerceValue(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;

            var value = (int)baseValue;

            if (value < 8)
            {
                return 8;
            }
            else if (value > 32)
            {
                return 32;
            }
            else
            {
                return baseValue;
            }
        }

        private static void BytePerLine_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.RefreshView(true);
                ctrl.UpdateHexHeader();
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > e.PreviousSize.Height ||
                e.NewSize.Height < e.PreviousSize.Height)
            {
                this.RefreshView(true);
            }
        }

        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.RefreshView(false);
        }

        /// <summary>
        /// Update vertical scrollbar with file info
        /// </summary>
        public void UpdateVerticalScroll()
        {
            this.VerticalScrollBar.Visibility = Visibility.Collapsed;

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                // TODO : check if need to show
                this.VerticalScrollBar.Visibility = Visibility.Visible;

                this.VerticalScrollBar.SmallChange = 1;
                this.VerticalScrollBar.LargeChange = this.ScrollLargeChange;
                this.VerticalScrollBar.Maximum = this.GetMaxLine() - this.GetMaxVisibleLine() + 1;
            }
        }

        /// <summary>
        /// Update de SelectionLine property
        /// </summary>
        private void UpdateSelectionLine()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this.SelectionLine = (this.SelectionStart / this.BytePerLine) + 1;
            }
            else
            {
                this.SelectionLine = 0;
            }
        }

        /// <summary>
        /// Refresh currentview of hexeditor
        /// </summary>
        /// <param name="controlResize"></param>
        public void RefreshView(bool controlResize = false, bool RefreshData = true)
        {
            this.UpdateLinesInfo();

            // UpdateVerticalScroll();
            // UpdateHexHeader();
            if (RefreshData)
            {
                this.UpdateStringDataViewer(controlResize);
                this.UpdateDataViewer(controlResize);
            }

            this.UpdateByteModified();
            this.UpdateSelection();
            this.UpdateHighLightByte();
            this.UpdateStatusBar();

            this.CheckProviderIsOnProgress();

            if (controlResize)
            {
                this.UpdateScrollMarkerPosition();
            }
        }

        /// <summary>
        /// Update the selection of byte in hexadecimal panel
        /// </summary>
        private void UpdateSelectionColorMode(FirstColor coloring)
        {
            int stackIndex = 0;

            switch (coloring)
            {
                case FirstColor.HexByteData:
                    stackIndex = 0;
                    foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                    {
                        foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            byteControl.HexByteFirstSelected = true;
                        }

                        foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                        {
                            byteControl.StringByteFirstSelected = false;
                        }

                        stackIndex++;
                    }

                    break;
                case FirstColor.StringByteData:
                    stackIndex = 0;
                    foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                    {
                        foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            byteControl.HexByteFirstSelected = false;
                        }

                        foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                        {
                            byteControl.StringByteFirstSelected = true;
                        }

                        stackIndex++;
                    }

                    break;
            }
        }

        /// <summary>
        /// Update the dataviewer stackpanel
        /// </summary>
        private void UpdateStringDataViewer(bool controlResize)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                if (controlResize)
                {
                    this.StringDataStackPanel.Children.Clear();

                    foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;

                        long position = ByteConverters.HexLiteralToLong(infolabel.Content.ToString());

                        for (int i = 0; i < this.BytePerLine; i++)
                        {
                            this._provider.Position = position + i;

                            if (this._provider.Position >= this._provider.Length)
                            {
                                break;
                            }

                            StringByteControl sbCtrl = new StringByteControl();

                            sbCtrl.BytePositionInFile = this._provider.Position;
                            sbCtrl.StringByteModified += this.Control_ByteModified;
                            sbCtrl.ReadOnlyMode = this.ReadOnlyMode;
                            sbCtrl.MoveNext += this.Control_MoveNext;
                            sbCtrl.MovePrevious += this.Control_MovePrevious;
                            sbCtrl.MouseSelection += this.Control_MouseSelection;
                            sbCtrl.Click += this.Control_Click;
                            sbCtrl.RightClick += this.Control_RightClick;
                            sbCtrl.MoveUp += this.Control_MoveUp;
                            sbCtrl.MoveDown += this.Control_MoveDown;
                            sbCtrl.MoveLeft += this.Control_MoveLeft;
                            sbCtrl.MoveRight += this.Control_MoveRight;
                            sbCtrl.ByteDeleted += this.Control_ByteDeleted;
                            sbCtrl.EscapeKey += this.Control_EscapeKey;

                            sbCtrl.InternalChange = true;
                            sbCtrl.TBLCharacterTable = this._TBLCharacterTable;
                            sbCtrl.TypeOfCharacterTable = this.TypeOfCharacterTable;
                            sbCtrl.Byte = (byte)this._provider.ReadByte();

                            // bool MTE = false;
                            // if (!_provider.EOF)
                            // {
                            // sbCtrl.ByteNext = (byte)_provider.ReadByte();

                            // if (_TBLCharacterTable != null)
                            //    if (DTE.TypeDTE(_TBLCharacterTable.FindTBLMatch(ByteConverters.ByteToHex(sbCtrl.Byte.Value).ToUpper() +
                            //                                                    ByteConverters.ByteToHex(sbCtrl.ByteNext.Value).ToUpper(), true)) != DTEType.MultipleTitleEncoding)
                            sbCtrl.ByteNext = (byte)this._provider.ReadByte();

                            this._provider.Position--;

                            // }
                            sbCtrl.InternalChange = false;

                            dataLineStack.Children.Add(sbCtrl);
                        }

                        this.StringDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else
                {
                    int stackIndex = 0;
                    foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                    {
                        long position = ByteConverters.HexLiteralToLong(infolabel.Content.ToString());

                        foreach (StringByteControl sbCtrl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                        {
                            this._provider.Position = position++;

                            sbCtrl.Action = ByteAction.Nothing;
                            sbCtrl.ReadOnlyMode = this.ReadOnlyMode;

                            sbCtrl.InternalChange = true;
                            sbCtrl.TBLCharacterTable = this._TBLCharacterTable;
                            sbCtrl.TypeOfCharacterTable = this.TypeOfCharacterTable;
                            if (this._provider.Position >= this._provider.Length)
                            {
                                sbCtrl.Byte = null;
                                sbCtrl.ByteNext = null;
                                sbCtrl.BytePositionInFile = -1;
                            }
                            else
                            {
                                sbCtrl.Byte = (byte)this._provider.ReadByte();
                                sbCtrl.BytePositionInFile = this._provider.Position - 1;

                                sbCtrl.ByteNext = (byte)this._provider.ReadByte();
                                this._provider.Position--;
                            }

                            sbCtrl.InternalChange = false;
                        }

                        stackIndex++;

                        // Prevent index out off range exception when resize at EOF
                        if (stackIndex == this.HexDataStackPanel.Children.Count && this.VerticalScrollBar.Value == this.VerticalScrollBar.Maximum)
                        {
                            stackIndex--;
                        }
                    }
                }
            }
            else
            {
                this.StringDataStackPanel.Children.Clear();
            }
        }

        /// <summary>
        /// Update byte are modified
        /// </summary>
        private void UpdateByteModified()
        {
            int stackIndex = 0;
            ByteModified byteModifiedCopy = null;

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                foreach (ByteModified byteModified in this._provider.ByteModifieds(ByteAction.All))
                {
                    stackIndex = 0;
                    byteModifiedCopy = byteModified.GetCopy();

                    foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                    {
                        foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                        {
                            if (byteModifiedCopy.BytePositionInFile == byteControl.BytePositionInFile)
                            {
                                byteControl.InternalChange = true;
                                byteControl.Byte = byteModifiedCopy.Byte;

                                switch (byteModifiedCopy.Action)
                                {
                                    case ByteAction.Modified:
                                        byteControl.Action = ByteAction.Modified;
                                        break;
                                    case ByteAction.Deleted:
                                        byteControl.Action = ByteAction.Deleted;
                                        break;
                                }

                                byteControl.InternalChange = false;
                            }
                        }

                        foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            if (byteModifiedCopy.BytePositionInFile == byteControl.BytePositionInFile)
                            {
                                byteControl.InternalChange = true;
                                byteControl.Byte = byteModifiedCopy.Byte;

                                switch (byteModifiedCopy.Action)
                                {
                                    case ByteAction.Modified:
                                        byteControl.Action = ByteAction.Modified;
                                        break;
                                    case ByteAction.Deleted:
                                        byteControl.Action = ByteAction.Deleted;
                                        break;
                                }

                                byteControl.InternalChange = false;
                            }
                        }

                        stackIndex++;

                        // Prevent index out off range exception when resize at EOF
                        if (stackIndex == this.HexDataStackPanel.Children.Count && this.VerticalScrollBar.Value == this.VerticalScrollBar.Maximum)
                        {
                            stackIndex--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the selection of byte
        /// </summary>
        private void UpdateSelection()
        {
            int stackIndex = 0;
            foreach (Label infolabel in this.LinesInfoStackPanel.Children)
            {
                if (this.SelectionStart <= this.SelectionStop)
                {
                    // Stringbyte panel
                    foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile >= this.SelectionStart &&
                            byteControl.BytePositionInFile <= this.SelectionStop &&
                            byteControl.BytePositionInFile > -1)
                        {
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        }
                        else
                        {
                            byteControl.IsSelected = false;
                        }
                    }

                    // HexByte panel
                    foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile >= this.SelectionStart &&
                            byteControl.BytePositionInFile <= this.SelectionStop &&
                            byteControl.BytePositionInFile > -1)
                        {
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        }
                        else
                        {
                            byteControl.IsSelected = false;
                        }
                    }
                }
                else
                {
                    // Stringbyte panel
                    foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile >= this.SelectionStop &&
                            byteControl.BytePositionInFile <= this.SelectionStart &&
                            byteControl.BytePositionInFile > -1)
                        {
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        }
                        else
                        {
                            byteControl.IsSelected = false;
                        }
                    }

                    // HexByte panel
                    foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile >= this.SelectionStop &&
                            byteControl.BytePositionInFile <= this.SelectionStart &&
                            byteControl.BytePositionInFile > -1)
                        {
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        }
                        else
                        {
                            byteControl.IsSelected = false;
                        }
                    }
                }

                stackIndex++;

                // Prevent index out off range exception when resize at EOF
                if (stackIndex == this.HexDataStackPanel.Children.Count && this.VerticalScrollBar.Value == this.VerticalScrollBar.Maximum)
                {
                    stackIndex--;
                }
            }
        }

        /// <summary>
        /// Update bytes as marked on findall()
        /// </summary>
        private void UpdateHighLightByte()
        {
            int stackIndex = 0;
            bool find = false;

            if (this.markedPositionList.Count > 0)
            {
                // var ByteList = from hlb in _markedPositionList
                //         where hlb >= GetFirstVisibleBytePosition() + BytePerLine && hlb <= GetLastVisibleBytePosition() + BytePerLine
                //         select hlb;
                foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                {
                    // Stringbyte panel
                    foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        find = false;

                        foreach (long position in this.markedPositionList)
                        {
                            if (position == byteControl.BytePositionInFile)
                            {
                                find = true;
                                break;
                            }
                        }

                        byteControl.IsHighLight = find;
                    }

                    // HexByte panel
                    foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        find = false;

                        foreach (long position in this.markedPositionList)
                        {
                            if (position == byteControl.BytePositionInFile)
                            {
                                find = true;
                                break;
                            }
                        }

                        byteControl.IsHighLight = find;
                    }

                    stackIndex++;

                    // Prevent index out off range exception when resize at EOF
                    if (stackIndex == this.HexDataStackPanel.Children.Count && this.VerticalScrollBar.Value == this.VerticalScrollBar.Maximum)
                    {
                        stackIndex--;
                    }
                }
            }
            else // Un highlight all
            {
                stackIndex = 0;

                foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                {
                    // Stringbyte panel
                    foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        byteControl.IsHighLight = false;
                    }

                    // HexByte panel
                    foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        byteControl.IsHighLight = false;
                    }

                    stackIndex++;

                    // Prevent index out off range exception when resize at EOF
                    if (stackIndex == this.HexDataStackPanel.Children.Count && this.VerticalScrollBar.Value == this.VerticalScrollBar.Maximum)
                    {
                        stackIndex--;
                    }
                }
            }
        }

        /// <summary>
        /// Update the dataviewer stackpanel
        /// </summary>
        private void UpdateDataViewer(bool controlResize)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                if (controlResize)
                {
                    this.HexDataStackPanel.Children.Clear();

                    foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;

                        long position = ByteConverters.HexLiteralToLong(infolabel.Content.ToString());

                        for (int i = 0; i < this.BytePerLine; i++)
                        {
                            this._provider.Position = position + i; // + correction;

                            if (this._provider.Position >= this._provider.Length)
                            {
                                break;
                            }

                            HexByteControl byteControl = new HexByteControl();

                            byteControl.BytePositionInFile = this._provider.Position;
                            byteControl.ReadOnlyMode = this.ReadOnlyMode;
                            byteControl.MouseSelection += this.Control_MouseSelection;
                            byteControl.Click += this.Control_Click;
                            byteControl.RightClick += this.Control_RightClick;
                            byteControl.MoveNext += this.Control_MoveNext;
                            byteControl.MovePrevious += this.Control_MovePrevious;
                            byteControl.ByteModified += this.Control_ByteModified;
                            byteControl.MoveUp += this.Control_MoveUp;
                            byteControl.MoveDown += this.Control_MoveDown;
                            byteControl.MoveLeft += this.Control_MoveLeft;
                            byteControl.MoveRight += this.Control_MoveRight;
                            byteControl.MovePageUp += this.Control_MovePageUp;
                            byteControl.MovePageDown += this.Control_MovePageDown;
                            byteControl.ByteDeleted += this.Control_ByteDeleted;
                            byteControl.EscapeKey += this.Control_EscapeKey;

                            byteControl.InternalChange = true;
                            byteControl.Byte = (byte)this._provider.ReadByte();
                            byteControl.InternalChange = false;

                            dataLineStack.Children.Add(byteControl);
                        }

                        this.HexDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else
                {
                    int stackIndex = 0;
                    foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                    {
                        long position = ByteConverters.HexLiteralToLong(infolabel.Content.ToString());

                        foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            this._provider.Position = position++;

                            byteControl.ReadOnlyMode = this.ReadOnlyMode;
                            byteControl.Action = ByteAction.Nothing;

                            byteControl.InternalChange = true;
                            if (this._provider.Position >= this._provider.Length)
                            {
                                byteControl.BytePositionInFile = -1;
                                byteControl.Byte = null;
                            }
                            else
                            {
                                byteControl.ReadOnlyMode = this.ReadOnlyMode;
                                byteControl.BytePositionInFile = this._provider.Position;
                                byteControl.Byte = (byte)this._provider.ReadByte();
                            }

                            byteControl.InternalChange = false;
                        }

                        stackIndex++;

                        // Prevent index out off range exception when resize at EOF
                        if (stackIndex == this.HexDataStackPanel.Children.Count && this.VerticalScrollBar.Value == this.VerticalScrollBar.Maximum)
                        {
                            stackIndex--;
                        }
                    }
                }
            }
            else
            {
                this.HexDataStackPanel.Children.Clear();
            }
        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateHexHeader()
        {
            this.HexHeaderStackPanel.Children.Clear();

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                for (int i = 0; i < this.BytePerLine; i++)
                {
                    // Create control
                    Label LineInfoLabel = new Label();
                    LineInfoLabel.Height = lineInfoHeight;
                    LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                    LineInfoLabel.Foreground = Brushes.Gray;
                    LineInfoLabel.Width = 25;
                    LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                    LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    LineInfoLabel.Content = ByteConverters.ByteToHex((byte)i);
                    LineInfoLabel.ToolTip = $"Column : {i.ToString()}";

                    this.HexHeaderStackPanel.Children.Add(LineInfoLabel);
                }
            }
        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateLinesInfo()
        {
            this.LinesInfoStackPanel.Children.Clear();

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                for (int i = 0; i < this.GetMaxVisibleLine(); i++)
                {
                    long fds = this.GetMaxVisibleLine();

                    // LineInfo
                    long firstLineByte = ((long)this.VerticalScrollBar.Value + i) * this.BytePerLine;
                    string info = "0x" + firstLineByte.ToString(ConstantReadOnly.HexLineInfoStringFormat, CultureInfo.InvariantCulture);

                    if (firstLineByte < this._provider.Length)
                    {
                        // Create control
                        Label LineInfoLabel = new Label();
                        LineInfoLabel.Height = lineInfoHeight;
                        LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                        LineInfoLabel.Foreground = Brushes.Gray;
                        LineInfoLabel.MouseDown += this.LineInfoLabel_MouseDown;
                        LineInfoLabel.MouseMove += this.LineInfoLabel_MouseMove;
                        LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                        LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                        LineInfoLabel.Content = info;
                        LineInfoLabel.ToolTip = $"Byte : {firstLineByte.ToString()}";

                        this.LinesInfoStackPanel.Children.Add(LineInfoLabel);
                    }
                }
            }
        }

        #endregion Update/Refresh view methods

        #region First/Last visible byte methods

        /// <summary>
        /// Get first visible byte position in control
        /// </summary>
        /// <returns>Return -1 of no file open</returns>
        private long GetFirstVisibleBytePosition()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                int stackIndex = 0;
                foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        return byteControl.BytePositionInFile;
                    }

                    stackIndex++;
                }

                return -1;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get last visible byte position in control
        /// </summary>
        /// <returns>Return -1 of no file open.</returns>
        private long GetLastVisibleBytePosition()
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                int stackIndex = 0;
                long byteposition = this.GetFirstVisibleBytePosition();
                foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        byteposition++;
                    }

                    stackIndex++;
                }

                return byteposition;
            }
            else
            {
                return -1;
            }
        }
        #endregion First/Last visible byte methods

        #region Focus Methods

        /// <summary>
        /// Set focus on byte
        /// </summary>
        private void SetFocusHexDataPanel(long bytePositionInFile)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                if (bytePositionInFile >= this._provider.Length)
                {
                    return;
                }

                int stackIndex = 0;
                foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)this.HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile == bytePositionInFile)
                        {
                            byteControl.Focus();
                            return;
                        }
                    }

                    stackIndex++;
                }

                if (this.VerticalScrollBar.Value < this.VerticalScrollBar.Maximum)
                {
                    this.VerticalScrollBar.Value++;
                }

                this.SetFocusHexDataPanel(bytePositionInFile);

                // SetPosition(bytePositionInFile);
            }
        }

        /// <summary>
        /// Set focus on byte
        /// </summary>
        private void SetFocusStringDataPanel(long bytePositionInFile)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                if (bytePositionInFile >= this._provider.Length)
                {
                    return;
                }

                int stackIndex = 0;
                foreach (Label infolabel in this.LinesInfoStackPanel.Children)
                {
                    foreach (StringByteControl byteControl in ((StackPanel)this.StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile == bytePositionInFile)
                        {
                            byteControl.Focus();
                            return;
                        }
                    }

                    stackIndex++;
                }

                if (this.VerticalScrollBar.Value < this.VerticalScrollBar.Maximum)
                {
                    this.VerticalScrollBar.Value++;
                }

                this.SetFocusStringDataPanel(bytePositionInFile);

                // SetPosition(bytePositionInFile);
            }
        }
        #endregion Focus Methods

        #region Find methods

        /// <summary>
        /// Find first occurence of string in stream. Search start as startPosition.
        /// </summary>
        public long FindFirst(string text, long startPosition = 0)
        {
            return this.FindFirst(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find first occurence of byte[] in stream. Search start as startPosition.
        /// </summary>
        public long FindFirst(byte[] bytes, long startPosition = 0)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                try
                {
                    var position = this._provider.FindIndexOf(bytes, startPosition).First();
                    this.SetPosition(position, bytes.Length);
                    return position;
                }
                catch
                {
                    this.UnSelectAll();
                    return -1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Find next occurence of string in stream search start at SelectionStart.
        /// </summary>
        public long FindNext(string text)
        {
            return this.FindNext(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find next occurence of byte[] in stream search start at SelectionStart.
        /// </summary>
        public long FindNext(byte[] bytes)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                try
                {
                    var position = this._provider.FindIndexOf(bytes, this.SelectionStart + 1).First();
                    this.SetPosition(position, bytes.Length);
                    return position;
                }
                catch
                {
                    this.UnSelectAll();
                    return -1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Find last occurence of string in stream search start at SelectionStart.
        /// </summary>
        public long FindLast(string text)
        {
            return this.FindLast(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find first occurence of byte[] in stream.
        /// </summary>
        public long FindLast(byte[] bytes)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                try
                {
                    var position = this._provider.FindIndexOf(bytes, this.SelectionStart + 1).Last();
                    this.SetPosition(position, bytes.Length);
                    return position;
                }
                catch
                {
                    this.UnSelectAll();
                    return -1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Find all occurence of string in stream.
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(string text)
        {
            return this.FindAll(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find all occurence of byte[] in stream.
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(byte[] bytes)
        {
            this.UnHighLightAll();

            if (ByteProvider.CheckIsOpen(this._provider))
            {
                return this._provider.FindIndexOf(bytes, 0);
            }

            return null;
        }

        /// <summary>
        /// Find all occurence of string in stream.
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(string text, bool highLight)
        {
            return this.FindAll(ByteConverters.StringToByte(text), highLight);
        }

        /// <summary>
        /// Find all occurence of string in stream. Highlight occurance in stream is MarcAll as true
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(byte[] bytes, bool highLight)
        {
            this.ClearScrollMarker(ScrollMarker.SearchHighLight);

            if (highLight)
            {
                var positions = this.FindAll(bytes);

                foreach (long position in positions)
                {
                    for (long i = position; i < position + bytes.Length; i++)
                    {
                        this.markedPositionList.Add(i);
                    }

                    this.SetScrollMarker(position, ScrollMarker.SearchHighLight);
                }

                this.UnSelectAll();
                this.UpdateHighLightByte();

                // Sort list
                this.markedPositionList.Sort();

                return positions;
            }
            else
            {
                return this.FindAll(bytes);
            }
        }

        /// <summary>
        /// Find all occurence of SelectionByteArray in stream. Highlight byte finded
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAllSelection(bool highLight)
        {
            if (this.SelectionLenght > 0)
            {
                return this.FindAll(this.SelectionByteArray, highLight);
            }
            else
            {
                return null;
            }
        }
        #endregion Find methods

        #region Statusbar

        /// <summary>
        /// Update statusbar for somes property dont support dependency property
        /// </summary>
        private void UpdateStatusBar()
        {
            if (this.StatusBarVisibility == Visibility.Visible)
            {
                if (ByteProvider.CheckIsOpen(this._provider))
                {
                    bool MB = false;
                    long deletedBytesCount = this._provider.ByteModifieds(ByteAction.Deleted).Count();

                    this.FileLengthLabel.Content = this._provider.Length - deletedBytesCount;

                    // is mega bytes ?
                    double lenght = (this._provider.Length - deletedBytesCount) / 1024;
                    if (lenght > 1024)
                    {
                        lenght = lenght / 1024;
                        MB = true;
                    }

                    this.FileLengthKBLabel.Content = Math.Round(lenght, 2) + (MB == true ? " MB" : " KB");
                }
                else
                {
                    this.FileLengthLabel.Content = 0;
                    this.FileLengthKBLabel.Content = 0;
                }
            }
        }
        #endregion Statusbar

        #region Bookmark and other scrollmarker

        /// <summary>
        /// Get all bookmark are currently set
        /// </summary>
        public IEnumerable<BookMark> BookMarks
        {
            get => (IEnumerable<BookMark>)this.GetValue(BookMarksProperty);
            internal set => this.SetValue(BookMarksProperty, value);
        }

        public static readonly DependencyProperty BookMarksProperty =
            DependencyProperty.Register("BookMarks", typeof(IEnumerable<BookMark>), typeof(HexaEditor),
                new FrameworkPropertyMetadata(new List<BookMark>()));

        /// <summary>
        /// Set bookmark at specified position
        /// </summary>
        /// <param name="position"></param>
        public void SetBookMark(long position)
        {
            this.SetScrollMarker(position, ScrollMarker.Bookmark);
        }

        /// <summary>
        /// Set bookmark at selection start
        /// </summary>
        public void SetBookMark()
        {
            this.SetScrollMarker(this.SelectionStart, ScrollMarker.Bookmark);
        }

        /// <summary>
        /// Set marker at position
        /// </summary>
        private void SetScrollMarker(long position, ScrollMarker marker)
        {
            Rectangle rect = new Rectangle();
            double topPosition = 0;
            double rightPosition = 0;

            // create bookmark
            var bookMark = new BookMark();
            bookMark.Marker = marker;
            bookMark.BytePositionInFile = position;

            // Remove selection start marker and set position
            if (marker == ScrollMarker.SelectionStart)
            {
                int i = 0;
                foreach (Rectangle ctrl in this.MarkerGrid.Children)
                {
                    if (((BookMark)ctrl.Tag).Marker == ScrollMarker.SelectionStart)
                    {
                        this.MarkerGrid.Children.RemoveAt(i);
                        break;
                    }

                    i++;
                }

                bookMark.BytePositionInFile = this.SelectionStart;
            }

            // Set position in scrollbar
            topPosition = (this.GetLineNumber(bookMark.BytePositionInFile) * this.VerticalScrollBar.Track.TickHeight(this.GetMaxLine())) - 1;

            if (topPosition == double.NaN)
            {
                topPosition = 0;
            }

            // Check if position already exist and exit if exist
            if (marker != ScrollMarker.SelectionStart)
            {
                foreach (Rectangle ctrl in this.MarkerGrid.Children)
                {
                    if (ctrl.Margin.Top == topPosition && ((BookMark)ctrl.Tag).Marker == marker)
                    {
                        return;
                    }
                }
            }

            // Somes general properties
            rect.MouseDown += this.Rect_MouseDown;
            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.Tag = bookMark;
            rect.Width = 5;
            rect.Height = 3;

            var byteinfo = new ByteModified();
            byteinfo.BytePositionInFile = position;
            rect.DataContext = byteinfo;

            // Set somes properties for different marker
            switch (marker)
            {
                case ScrollMarker.Bookmark:
                    rect.ToolTip = this.TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)this.TryFindResource("BookMarkColor");
                    break;
                case ScrollMarker.SearchHighLight:
                    rect.ToolTip = this.TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)this.TryFindResource("SearchBookMarkColor");
                    rect.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case ScrollMarker.SelectionStart:
                    rect.Fill = (SolidColorBrush)this.TryFindResource("SelectionStartBookMarkColor");
                    rect.Width = this.VerticalScrollBar.ActualWidth;
                    rect.Height = 2;
                    break;
                case ScrollMarker.ByteModified:
                    rect.ToolTip = this.TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)this.TryFindResource("ByteModifiedMarkColor");
                    rect.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case ScrollMarker.ByteDeleted:
                    rect.ToolTip = this.TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)this.TryFindResource("ByteDeletedMarkColor");
                    rect.HorizontalAlignment = HorizontalAlignment.Right;
                    rightPosition = 4;
                    break;
            }

            try
            {
                rect.Margin = new Thickness(0, topPosition, rightPosition, 0);
            }
            catch
            {
            }

            // Add to grid
            this.MarkerGrid.Children.Add(rect);

            // Update bookmarks properties
            this.UpdateBookMarkProperties();
        }

        /// <summary>
        /// Update the bookmark properties are currently set
        /// </summary>
        private void UpdateBookMarkProperties()
        {
            List<BookMark> bmList = new List<BookMark>();
            foreach (Rectangle rc in this.MarkerGrid.Children)
            {
                BookMark bm = rc.Tag as BookMark;

                if (bm != null)
                {
                    if (bm.Marker == ScrollMarker.Bookmark)
                    {
                        bmList.Add(bm);
                    }
                }
            }

            this.BookMarks = bmList;
        }

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;

            Debug.Print(rect.Tag.ToString());

            if (((BookMark)rect.Tag).Marker != ScrollMarker.SelectionStart)
            {
                this.SetPosition(((BookMark)rect.Tag).BytePositionInFile, 1);
            }
            else
            {
                this.SetPosition(this.SelectionStart, 1);
            }
        }

        /// <summary>
        /// Update all scroll marker position
        /// </summary>
        private void UpdateScrollMarkerPosition()
        {
            foreach (Rectangle rect in this.MarkerGrid.Children)
            {
                if (((BookMark)rect.Tag).Marker != ScrollMarker.SelectionStart)
                {
                    rect.Margin = new Thickness(0, (this.GetLineNumber(((BookMark)rect.Tag).BytePositionInFile) * this.VerticalScrollBar.Track.TickHeight(this.GetMaxLine())) - rect.ActualHeight, 0, 0);
                }
            }
        }

        /// <summary>
        /// Clear ScrollMarker
        /// </summary>
        public void ClearAllScrollMarker()
        {
            this.MarkerGrid.Children.Clear();
        }

        /// <summary>
        /// Clear ScrollMarker
        /// </summary>
        public void ClearScrollMarker(ScrollMarker marker)
        {
            for (int i = 0; i < this.MarkerGrid.Children.Count; i++)
            {
                BookMark mark = (BookMark)((Rectangle)this.MarkerGrid.Children[i]).Tag;

                if (mark.Marker == marker)
                {
                    this.MarkerGrid.Children.Remove(this.MarkerGrid.Children[i]);
                    i--;
                }
            }
        }

        #endregion Bookmark and other scrollmarker

        #region Context menu

        private void Control_RightClick(object sender, EventArgs e)
        {
            // position
            StringByteControl sbCtrl = sender as StringByteControl;
            HexByteControl ctrl = sender as HexByteControl;

            if (sbCtrl != null)
            {
                this._rightClickBytePosition = sbCtrl.BytePositionInFile;
            }
            else if (ctrl != null)
            {
                this._rightClickBytePosition = ctrl.BytePositionInFile;
            }

            // update ctrl
            this.CopyASCIICMenu.IsEnabled = false;
            this.FindAllCMenu.IsEnabled = false;
            this.CopyHexaCMenu.IsEnabled = false;
            this.UndoCMenu.IsEnabled = false;
            this.DeleteCMenu.IsEnabled = false;

            // BookMarkCMenu.IsEnabled = false;
            if (this.SelectionLenght > 0)
            {
                this.CopyASCIICMenu.IsEnabled = true;
                this.FindAllCMenu.IsEnabled = true;
                this.CopyHexaCMenu.IsEnabled = true;
                this.DeleteCMenu.IsEnabled = true;
            }

            if (this.UndoCount > 0)
            {
                this.UndoCMenu.IsEnabled = true;
            }

            // Show context menu
            this.Focus();
            this.CMenu.Visibility = Visibility.Visible;
        }

        private void FindAllCMenu_Click(object sender, RoutedEventArgs e)
        {
            this.FindAll(this.SelectionByteArray, true);
        }

        private void CopyHexaCMenu_Click(object sender, RoutedEventArgs e)
        {
            this.CopyToClipboard(CopyPasteMode.HexaString);
        }

        private void CopyASCIICMenu_Click(object sender, RoutedEventArgs e)
        {
            this.CopyToClipboard(CopyPasteMode.ASCIIString);
        }

        private void DeleteCMenu_Click(object sender, RoutedEventArgs e)
        {
            this.DeleteSelection();
        }

        private void UndoCMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Undo();
        }

        private void BookMarkCMenu_Click(object sender, RoutedEventArgs e)
        {
            this.SetBookMark(this._rightClickBytePosition);
        }

        private void ClearBookMarkCMenu_Click(object sender, RoutedEventArgs e)
        {
            this.ClearScrollMarker(ScrollMarker.Bookmark);
        }

        private void PasteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ByteProvider.CheckIsOpen(this._provider))
            {
                this._provider.PasteNotInsert(this._rightClickBytePosition, Clipboard.GetText());

                this.RefreshView();
            }
        }

        #endregion Bookmark and other scrollmarker

    }
}
