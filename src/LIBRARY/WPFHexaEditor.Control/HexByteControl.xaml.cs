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

    /// <summary>
    /// Interaction logic for HexControl.xaml
    /// </summary>
    public partial class HexByteControl : UserControl
    {
        private bool readOnlyMode = false;
        private KeyDownLabel keyDownLabel = KeyDownLabel.FirstChar;

        public event EventHandler ByteModified;

        public event EventHandler MouseSelection;

        public event EventHandler Click;

        public event EventHandler RightClick;

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

        public HexByteControl()
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
            DependencyProperty.Register("BytePositionInFile", typeof(long), typeof(HexByteControl), new PropertyMetadata(-1L));

        /// <summary>
        /// Action with this byte
        /// </summary>
        public ByteAction Action
        {
            get => (ByteAction)this.GetValue(ActionProperty);
            set => this.SetValue(ActionProperty, value);
        }

        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register("Action", typeof(ByteAction), typeof(HexByteControl),
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
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateBackGround();
            }
        }

        /// <summary>
        /// Used for selection coloring
        /// </summary>
        public bool HexByteFirstSelected
        {
            get => (bool)this.GetValue(HexByteFirstSelectedProperty);
            set => this.SetValue(HexByteFirstSelectedProperty, value);
        }

        public static readonly DependencyProperty HexByteFirstSelectedProperty =
            DependencyProperty.Register("HexByteFirstSelected", typeof(bool), typeof(HexByteControl), new PropertyMetadata(true));

        /// <summary>
        /// Byte used for this instance
        /// </summary>
        public byte? Byte
        {
            get => (byte?)this.GetValue(ByteProperty);
            set => this.SetValue(ByteProperty, value);
        }

        public static readonly DependencyProperty ByteProperty =
            DependencyProperty.Register("Byte", typeof(byte?), typeof(HexByteControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(Byte_PropertyChanged)));

        private static void Byte_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue)
            {
                if (ctrl.Action != ByteAction.Nothing && ctrl.InternalChange == false)
                {
                    ctrl.ByteModified?.Invoke(ctrl, new EventArgs());
                }

                ctrl.UpdateLabelFromByte();
                ctrl.UpdateHexString();
            }
        }

        /// <summary>
        /// Used to prevent ByteModified event occurc when we dont want!
        /// </summary>
        public bool InternalChange
        {
            get => (bool)this.GetValue(InternalChangeProperty);
            set => this.SetValue(InternalChangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalChangeProperty =
            DependencyProperty.Register("InternalChange", typeof(bool), typeof(HexByteControl), new PropertyMetadata(false));

        #endregion

        public bool ReadOnlyMode
        {
            get => this.readOnlyMode;

            set => this.readOnlyMode = value;
        }

        /// <summary>
        /// Get the hex string representation of this byte
        /// </summary>
        public string HexString
        {
            get => (string)this.GetValue(HexStringProperty);
            internal set => this.SetValue(HexStringProperty, value);
        }

        public static readonly DependencyProperty HexStringProperty =
            DependencyProperty.Register("HexString", typeof(string), typeof(HexByteControl),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// Get or Set if control as selected
        /// </summary>
        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(HexByteControl),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(IsSelected_PropertyChange)));

        private static void IsSelected_PropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue)
            {
                ctrl.keyDownLabel = KeyDownLabel.FirstChar;
                ctrl.UpdateBackGround();
            }
        }

        /// <summary>
        /// Get of Set if control as marked as highlighted
        /// </summary>
        public bool IsHighLight
        {
            get => (bool)this.GetValue(IsHighLightProperty);
            set => this.SetValue(IsHighLightProperty, value);
        }

        public static readonly DependencyProperty IsHighLightProperty =
            DependencyProperty.Register("IsHighLight", typeof(bool), typeof(HexByteControl),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(IsHighLight_PropertyChanged)));

        private static void IsHighLight_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexByteControl ctrl = d as HexByteControl;

            if (e.NewValue != e.OldValue)
            {
                ctrl.keyDownLabel = KeyDownLabel.FirstChar;
                ctrl.UpdateBackGround();
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
                this.FirstHexChar.Foreground = Brushes.White;
                this.SecondHexChar.Foreground = Brushes.White;

                if (this.HexByteFirstSelected)
                {
                    this.Background = (SolidColorBrush)this.TryFindResource("FirstColor");
                }
                else
                {
                    this.Background = (SolidColorBrush)this.TryFindResource("SecondColor");
                }
            }
            else if (this.IsHighLight)
            {
                this.FontWeight = (FontWeight)this.TryFindResource("NormalFontWeight");
                this.FirstHexChar.Foreground = Brushes.Black;
                this.SecondHexChar.Foreground = Brushes.Black;

                this.Background = (SolidColorBrush)this.TryFindResource("HighLightColor");
            }
            else if (this.Action != ByteAction.Nothing)
            {
                switch (this.Action)
                {
                    case ByteAction.Modified:
                        this.FontWeight = (FontWeight)this.TryFindResource("BoldFontWeight");
                        this.Background = (SolidColorBrush)this.TryFindResource("ByteModifiedColor");
                        this.FirstHexChar.Foreground = Brushes.Black;
                        this.SecondHexChar.Foreground = Brushes.Black;
                        break;
                    case ByteAction.Deleted:
                        this.FontWeight = (FontWeight)this.TryFindResource("BoldFontWeight");
                        this.Background = (SolidColorBrush)this.TryFindResource("ByteDeletedColor");
                        this.FirstHexChar.Foreground = Brushes.Black;
                        this.SecondHexChar.Foreground = Brushes.Black;
                        break;
                }
            }
            else
            {
                this.FontWeight = (FontWeight)this.TryFindResource("NormalFontWeight");
                this.Background = Brushes.Transparent;
                this.FirstHexChar.Foreground = Brushes.Black;
                this.SecondHexChar.Foreground = Brushes.Black;
            }
        }

        private void UpdateLabelFromByte()
        {
            if (this.Byte != null)
            {
                string hexabyte = ByteConverters.ByteToHex(this.Byte.Value);

                this.FirstHexChar.Content = hexabyte.Substring(0, 1);
                this.SecondHexChar.Content = hexabyte.Substring(1, 1);
            }
            else
            {
                this.FirstHexChar.Content = string.Empty;
                this.SecondHexChar.Content = string.Empty;
            }
        }

        private void HexChar_MouseDown(object sender, MouseButtonEventArgs e)
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

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyValidator.IsUpKey(e.Key))
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
                e.Handled = true;
                this.ByteDeleted?.Invoke(this, new EventArgs());

                this.MovePrevious?.Invoke(this, new EventArgs());

                return;
            }
            else if (KeyValidator.IsEscapeKey(e.Key))
            {
                e.Handled = true;
                this.EscapeKey?.Invoke(this, new EventArgs());
                return;
            }

            // MODIFY BYTE
            if (!this.ReadOnlyMode)
            {
                if (KeyValidator.IsHexKey(e.Key))
                {
                    string key;
                    if (KeyValidator.IsNumericKey(e.Key))
                    {
                        key = KeyValidator.GetDigitFromKey(e.Key).ToString();
                    }
                    else
                    {
                        key = e.Key.ToString().ToLower();
                    }

                    switch (this.keyDownLabel)
                    {
                        case KeyDownLabel.FirstChar:
                            this.FirstHexChar.Content = key;
                            this.keyDownLabel = KeyDownLabel.SecondChar;
                            this.Action = ByteAction.Modified;
                            this.Byte = ByteConverters.HexToByte(this.FirstHexChar.Content.ToString() + this.SecondHexChar.Content.ToString())[0];
                            break;
                        case KeyDownLabel.SecondChar:
                            this.SecondHexChar.Content = key;
                            this.keyDownLabel = KeyDownLabel.NextPosition;

                            this.Action = ByteAction.Modified;
                            this.Byte = ByteConverters.HexToByte(this.FirstHexChar.Content.ToString() + this.SecondHexChar.Content.ToString())[0];

                            // Move focus event
                            this.MoveNext?.Invoke(this, new EventArgs());
                            break;
                    }
                }
            }
        }

        private void UpdateHexString()
        {
            this.HexString = (string)this.FirstHexChar.Content + (string)this.SecondHexChar.Content; // .ToString();
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
    }
}
