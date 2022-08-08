namespace WPFHexaEditor.Control
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using WPFHexaEditor.Core;
    using WPFHexaEditor.Core.Bytes;
    using WPFHexaEditor.Core.ROMTable;

    /// <summary>
    /// Interaction logic for StringByteControl.xaml
    /// </summary>
    public partial class StringByteControl : UserControl
    {
        // private bool _isByteModified = false;
        private bool readOnlyMode;
        private TBLStream tBLCharacterTable = null;

        public event EventHandler Click;

        public event EventHandler RightClick;

        public event EventHandler MouseSelection;

        public event EventHandler StringByteModified;

        public event EventHandler MoveNext;

        public event EventHandler MovePrevious;

        public event EventHandler MoveRight;

        public event EventHandler MoveLeft;

        public event EventHandler MoveUp;

        public event EventHandler MoveDown;

        public event EventHandler MovePageDown;

        public event EventHandler MovePageUp;

        public event EventHandler ByteDeleted;

        public event EventHandler EscapeKey;

        public StringByteControl()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        #region DependencyProperty

        /// <summary>
        /// Position in file
        /// </summary>
        public long BytePositionInFile
        {
            get => (long)this.GetValue(BytePositionInFileProperty);
            set => this.SetValue(BytePositionInFileProperty, value);
        }

        public static readonly DependencyProperty BytePositionInFileProperty =
            DependencyProperty.Register("BytePositionInFile", typeof(long), typeof(StringByteControl), new PropertyMetadata(-1L));

        /// <summary>
        /// Used for selection coloring
        /// </summary>
        public bool StringByteFirstSelected
        {
            get => (bool)this.GetValue(StringByteFirstSelectedProperty);
            set => this.SetValue(StringByteFirstSelectedProperty, value);
        }

        public static readonly DependencyProperty StringByteFirstSelectedProperty =
            DependencyProperty.Register("StringByteFirstSelected", typeof(bool), typeof(StringByteControl), new PropertyMetadata(true));

        /// <summary>
        /// Byte used for this instance
        /// </summary>
        public byte? Byte
        {
            get => (byte?)this.GetValue(ByteProperty);
            set => this.SetValue(ByteProperty, value);
        }

        public static readonly DependencyProperty ByteProperty =
            DependencyProperty.Register("Byte", typeof(byte?), typeof(StringByteControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(Byte_PropertyChanged)));

        private static void Byte_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringByteControl ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
            {
                if (ctrl.Action != ByteAction.Nothing && ctrl.InternalChange == false)
                {
                    ctrl.StringByteModified?.Invoke(ctrl, new EventArgs());
                }

                ctrl.UpdateLabelFromByte();
                ctrl.UpdateHexString();

                ctrl.UpdateBackGround();
            }
        }

        /// <summary>
        /// Next Byte of this instance (used for TBL/MTE decoding)
        /// </summary>
        public byte? ByteNext
        {
            get => (byte?)this.GetValue(ByteNextProperty);
            set => this.SetValue(ByteNextProperty, value);
        }

        public static readonly DependencyProperty ByteNextProperty =
            DependencyProperty.Register("ByteNext", typeof(byte?), typeof(StringByteControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ByteNext_PropertyChanged)));

        private static void ByteNext_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringByteControl ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
            {
                // if (ctrl.Action != ByteAction.Nothing && ctrl.InternalChange == false)
                //    ctrl.StringByteModified?.Invoke(ctrl, new EventArgs());
                ctrl.UpdateLabelFromByte();

                // ctrl.UpdateHexString();
                ctrl.UpdateBackGround();
            }
        }

        /// <summary>
        /// Get or set if control as selected
        /// </summary>
        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(StringByteControl),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsSelected_PropertyChangedCallBack)));

        private static void IsSelected_PropertyChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringByteControl ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateBackGround();
            }
        }

        /// <summary>
        /// Get the hex string {00} representation of this byte
        /// </summary>
        public string HexString
        {
            get => (string)this.GetValue(HexStringProperty);
            internal set => this.SetValue(HexStringProperty, value);
        }

        public static readonly DependencyProperty HexStringProperty =
            DependencyProperty.Register("HexString", typeof(string), typeof(StringByteControl),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Get of Set if control as marked as highlighted
        /// </summary>
        public bool IsHighLight
        {
            get => (bool)this.GetValue(IsHighLightProperty);
            set => this.SetValue(IsHighLightProperty, value);
        }

        public static readonly DependencyProperty IsHighLightProperty =
            DependencyProperty.Register("IsHighLight", typeof(bool), typeof(StringByteControl),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(IsHighLight_PropertyChanged)));

        private static void IsHighLight_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringByteControl ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateBackGround();
            }
        }

        /// <summary>
        /// Used to prevent StringByteModified event occurc when we dont want!
        /// </summary>
        public bool InternalChange
        {
            get => (bool)this.GetValue(InternalChangeProperty);
            set => this.SetValue(InternalChangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalChangeProperty =
            DependencyProperty.Register("InternalChange", typeof(bool), typeof(StringByteControl), new PropertyMetadata(false));

        /// <summary>
        /// Action with this byte
        /// </summary>
        public ByteAction Action
        {
            get => (ByteAction)this.GetValue(ActionProperty);
            set => this.SetValue(ActionProperty, value);
        }

        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register("Action", typeof(ByteAction), typeof(StringByteControl),
                new FrameworkPropertyMetadata(ByteAction.Nothing,
                    new PropertyChangedCallback(Action_ValueChanged),
                    new CoerceValueCallback(Action_CoerceValue)));

        private static object Action_CoerceValue(DependencyObject d, object baseValue)
        {
            ByteAction value = (ByteAction)baseValue;

            if (value != ByteAction.All)
            {
                return baseValue;
            }
            else
            {
                return ByteAction.Nothing;
            }
        }

        private static void Action_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringByteControl ctrl = d as StringByteControl;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateBackGround();
            }
        }
        #endregion

        #region Characters tables

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
            DependencyProperty.Register("TypeOfCharacterTable", typeof(CharacterTable), typeof(StringByteControl),
                new FrameworkPropertyMetadata(CharacterTable.ASCII,
                    new PropertyChangedCallback(TypeOfCharacterTable_PropertyChanged)));

        private static void TypeOfCharacterTable_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringByteControl ctrl = d as StringByteControl;

            ctrl.UpdateLabelFromByte();
            ctrl.UpdateHexString();
        }

        public TBLStream TBLCharacterTable
        {
            get => this.tBLCharacterTable;

            set => this.tBLCharacterTable = value;
        }
        #endregion Characters tables

        /// <summary>
        /// Update control label from byte property
        /// </summary>
        private void UpdateLabelFromByte()
        {
            if (this.Byte != null)
            {
                switch (this.TypeOfCharacterTable)
                {
                    case CharacterTable.ASCII:
                        this.StringByteLabel.Content = ByteConverters.ByteToChar(this.Byte.Value);
                        this.Width = 12;
                        break;
                    case CharacterTable.TBLFile:
                        this.ReadOnlyMode = true;

                        if (this.TBLCharacterTable != null)
                        {
                            string content = "#";
                            string MTE = (ByteConverters.ByteToHex(this.Byte.Value) + ByteConverters.ByteToHex(this.ByteNext.Value)).ToUpper();
                            content = this.TBLCharacterTable.FindTBLMatch(MTE, true);

                            if (content == "#")
                            {
                                content = this.TBLCharacterTable.FindTBLMatch(ByteConverters.ByteToHex(this.Byte.Value).ToUpper().ToUpper(), true);
                            }

                            this.StringByteLabel.Content = content;

                            // Adjuste wight
                            if (content.Length == 1)
                            {
                                this.Width = 12;
                            }
                            else if (content.Length == 2)
                            {
                                this.Width = 12 + (content.Length * 2D);
                            }
                            else if (content.Length > 2)
                            {
                                this.Width = 12 + (content.Length * 3.8D);
                            }
                        }
                        else
                        {
                            goto case CharacterTable.ASCII;
                        }

                        break;
                }
            }
            else
            {
                this.StringByteLabel.Content = string.Empty;
            }
        }

        private void UpdateHexString()
        {
            if (this.Byte != null)
            {
                this.HexString = ByteConverters.ByteToHex(this.Byte.Value);
            }
            else
            {
                this.HexString = string.Empty;
            }
        }

        /// <summary>
        /// Update Background
        /// </summary>
        private void UpdateBackGround()
        {
            if (this.IsSelected)
            {
                this.FontWeight = (FontWeight)this.TryFindResource("NormalFontWeight");
                this.StringByteLabel.Foreground = Brushes.White;

                if (this.StringByteFirstSelected)
                {
                    this.Background = (SolidColorBrush)this.TryFindResource("FirstColor");
                }
                else
                {
                    this.Background = (SolidColorBrush)this.TryFindResource("SecondColor");
                }

                return;
            }
            else if (this.IsHighLight)
            {
                this.FontWeight = (FontWeight)this.TryFindResource("NormalFontWeight");
                this.StringByteLabel.Foreground = Brushes.Black;

                this.Background = (SolidColorBrush)this.TryFindResource("HighLightColor");

                return;
            }
            else if (this.Action != ByteAction.Nothing)
            {
                switch (this.Action)
                {
                    case ByteAction.Modified:
                        this.FontWeight = (FontWeight)this.TryFindResource("BoldFontWeight");
                        this.Background = (SolidColorBrush)this.TryFindResource("ByteModifiedColor");
                        this.StringByteLabel.Foreground = Brushes.Black;
                        break;
                    case ByteAction.Deleted:
                        this.FontWeight = (FontWeight)this.TryFindResource("BoldFontWeight");
                        this.Background = (SolidColorBrush)this.TryFindResource("ByteDeletedColor");
                        this.StringByteLabel.Foreground = Brushes.Black;
                        break;
                }

                return;
            }
            else
            {
                this.FontWeight = (FontWeight)this.TryFindResource("NormalFontWeight");
                this.Background = Brushes.Transparent;
                this.StringByteLabel.Foreground = Brushes.Black;

                if (this.TypeOfCharacterTable == CharacterTable.TBLFile)
                {
                    switch (DTE.TypeDTE((string)this.StringByteLabel.Content))
                    {
                        case DTEType.DualTitleEncoding:
                            this.StringByteLabel.Foreground = Brushes.Red;
                            break;
                        case DTEType.MultipleTitleEncoding:
                            this.StringByteLabel.Foreground = Brushes.Blue;
                            break;
                        default:
                            this.StringByteLabel.Foreground = Brushes.Black;
                            break;
                    }
                }
            }
        }

        public bool ReadOnlyMode
        {
            get => this.readOnlyMode;

            set => this.readOnlyMode = value;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyValidator.IsIgnoredKey(e.Key))
            {
                e.Handled = true;
                return;
            }
            else if (KeyValidator.IsUpKey(e.Key))
            {
                e.Handled = true;
                this.MoveUp?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsDownKey(e.Key))
            {
                e.Handled = true;
                this.MoveDown?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsLeftKey(e.Key))
            {
                e.Handled = true;
                this.MoveLeft?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsRightKey(e.Key))
            {
                e.Handled = true;
                this.MoveRight?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsPageDownKey(e.Key))
            {
                e.Handled = true;
                this.MovePageDown?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsPageUpKey(e.Key))
            {
                e.Handled = true;
                this.MovePageUp?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsDeleteKey(e.Key))
            {
                if (!this.ReadOnlyMode)
                {
                    e.Handled = true;
                    this.ByteDeleted?.Invoke(this, new EventArgs());

                    return;
                }
            }
            else if (KeyValidator.IsBackspaceKey(e.Key))
            {
                if (!this.ReadOnlyMode)
                {
                    e.Handled = true;
                    this.ByteDeleted?.Invoke(this, new EventArgs());

                    this.MovePrevious?.Invoke(this, new EventArgs());

                    return;
                }
            }
            else if (KeyValidator.IsEscapeKey(e.Key))
            {
                e.Handled = true;
                this.EscapeKey?.Invoke(this, new EventArgs());
                return;
            }

            // MODIFY ASCII...
            // TODO : MAKE BETTER KEYDETECTION AND EXPORT IN KEYVALIDATOR
            if (!this.ReadOnlyMode)
            {
                bool isok = false;

                if (Keyboard.GetKeyStates(Key.CapsLock) == KeyStates.Toggled)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        this.StringByteLabel.Content = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key));
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        isok = true;
                        this.StringByteLabel.Content = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString().ToLower();
                    }
                }
                else
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        this.StringByteLabel.Content = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key)).ToString().ToLower();
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        isok = true;
                        this.StringByteLabel.Content = ByteConverters.ByteToChar((byte)KeyInterop.VirtualKeyFromKey(e.Key));
                    }
                }

                // Move focus event
                if (isok)
                {
                    if (this.MoveNext != null)
                    {
                        this.Action = ByteAction.Modified;
                        this.Byte = ByteConverters.CharToByte(this.StringByteLabel.Content.ToString()[0]);

                        this.MoveNext(this, new EventArgs());
                    }
                }
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.Byte != null)
            {
                if (this.Action != ByteAction.Modified &&
                    this.Action != ByteAction.Deleted &&
                    this.Action != ByteAction.Added &&
                    !this.IsSelected && !this.IsHighLight)
                {
                    this.Background = (SolidColorBrush)this.TryFindResource("MouseOverColor");
                }
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.MouseSelection?.Invoke(this, e);
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.Byte != null)
            {
                if (this.Action != ByteAction.Modified &&
                    this.Action != ByteAction.Deleted &&
                    this.Action != ByteAction.Added &&
                    !this.IsSelected && !this.IsHighLight)
                {
                    this.Background = Brushes.Transparent;
                }
            }
        }

        private void StringByteLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Focus();

                this.Click?.Invoke(this, e);
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                this.RightClick?.Invoke(this, e);
            }
        }
    }
}
