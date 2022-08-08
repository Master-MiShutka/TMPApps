namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Win32;
    using WpfMouseWheel.ComponentModel;

    #region ScrollIncrementScaleType
    public enum ScrollIncrementScaleType
    {
        Star,
        Unity,
    }
    #endregion

    #region MouseWheelScrollIncrement
    [TypeConverter(typeof(MouseWheelScrollIncrementConverter))]
    public sealed class MouseWheelScrollIncrement : ObservableObject, IWeakEventListener, IEquatable<MouseWheelScrollIncrement>, ICloneable
    {
        // MouseWheelUp native behavior and increments
        //   DocumentGrid           : CanVerticalScroll ? 16* : PageUp
        //   FlowDocumentView       : ScrollData.MouseWheelUp
        //   ScrollContentPresenter : 48
        //   ScrollData             : 48
        //   TextBoxView            : ScrollData.MouseWheelUp
        //   StackPanel             : Vertical            ? 1* : 16*
        //   VirtualizingStackPanel : Vertical && Logical ? 1* : 16*

        #region Constants

        #region Logical Increments
        public static readonly MouseWheelScrollIncrement StarLogicalDefault = new MouseWheelScrollIncrement(1, ScrollIncrementScaleType.Star);
        public static readonly MouseWheelScrollIncrement StarLogicalDefaultV = new MouseWheelScrollIncrement(1, ScrollIncrementScaleType.Star, Orientation.Vertical);
        public static readonly MouseWheelScrollIncrement StarLogicalDefaultH = new MouseWheelScrollIncrement(1, ScrollIncrementScaleType.Star, Orientation.Horizontal);

        public static readonly MouseWheelScrollIncrement LogicalDefault = new MouseWheelScrollIncrement(3, ScrollIncrementScaleType.Unity);
        public static readonly MouseWheelScrollIncrement LogicalDefaultV = new MouseWheelScrollIncrement(3, ScrollIncrementScaleType.Unity, Orientation.Vertical);
        public static readonly MouseWheelScrollIncrement LogicalDefaultH = new MouseWheelScrollIncrement(1, ScrollIncrementScaleType.Unity, Orientation.Horizontal);
        #endregion

        #region Physical Increments
        public static readonly MouseWheelScrollIncrement StarPhysicalDefault = new MouseWheelScrollIncrement(16, ScrollIncrementScaleType.Star);
        public static readonly MouseWheelScrollIncrement StarPhysicalDefaultV = new MouseWheelScrollIncrement(16, ScrollIncrementScaleType.Star, Orientation.Vertical);
        public static readonly MouseWheelScrollIncrement StarPhysicalDefaultH = new MouseWheelScrollIncrement(16, ScrollIncrementScaleType.Star, Orientation.Horizontal);

        public static readonly MouseWheelScrollIncrement PhysicalDefault = new MouseWheelScrollIncrement(48, ScrollIncrementScaleType.Unity);
        public static readonly MouseWheelScrollIncrement PhysicalDefaultV = new MouseWheelScrollIncrement(48, ScrollIncrementScaleType.Unity, Orientation.Vertical);
        public static readonly MouseWheelScrollIncrement PhysicalDefaultH = new MouseWheelScrollIncrement(48, ScrollIncrementScaleType.Unity, Orientation.Horizontal);
        #endregion

        #endregion

        #region Static
        public static MouseWheelScrollIncrement Parse(string s)
        {
            s = s.Trim();
            ScrollIncrementScaleType factorType = ScrollIncrementScaleType.Unity;
            double increment = 1.0;
            if (s.EndsWith("*"))
            {
                factorType = ScrollIncrementScaleType.Star;
                s = s.Substring(0, s.Length - 1).TrimEnd();
            }

            if (s.Length > 0 && !double.TryParse(s, out increment))
                throw new FormatException("ScrollIncrement format examples: 48, 16*, *, 0.5*");
            return new MouseWheelScrollIncrement(increment, factorType);
        }
        #endregion

        #region Instance

        #region Fields
        private readonly ScrollIncrementScaleType _scaleType;
        private int _scale = int.MinValue;
        private double _increment;
        private Func<int> _fetchScale;
        #endregion

        #region Initialization
        public MouseWheelScrollIncrement(double value, ScrollIncrementScaleType scaleType)
        {
            this._scaleType = scaleType;
            this._increment = value;
        }

        private MouseWheelScrollIncrement(double value, ScrollIncrementScaleType scaleType, Orientation orientation)
        {
            this._scaleType = scaleType;
            this._increment = value;
            this.Orientation = orientation;
        }
        #endregion

        #region Object
        public override string ToString()
        {
            if (this.ScaleType == ScrollIncrementScaleType.Star)
                return string.Format("{0}*", this.Increment);
            else
                return this.Increment.ToString();
        }

        public override int GetHashCode() { return this.ScaleType.GetHashCode() ^ this.Increment.GetHashCode() ^ this.Orientation.GetHashCode(); }

        public override sealed bool Equals(object obj) { return this.Equals(obj as MouseWheelScrollIncrement); }
        #endregion

        #region IEquatable<MouseWheelScrollIncrement>
        public bool Equals(MouseWheelScrollIncrement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return this.ScaleType == other.ScaleType && DoubleEx.AreClose(this.Increment, other.Increment) && this.Orientation == other.Orientation;
        }
        #endregion

        #region ICloneable
        object ICloneable.Clone() { return this.Clone(); }
        #endregion

        #region IWeakEventListener
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(UserPreferenceChangedEventManager))
                this.OnUserPreferenceChanged(sender, (UserPreferenceChangedEventArgs)e);
            else
                return false;
            return true;
        }
        #endregion

        #region Queries
        public ScrollIncrementScaleType ScaleType => this._scaleType;

        public double Value => this.Increment * this.Scale;
        #endregion

        #region Properties
        public int Scale
        {
            get
            {
                if (this._scale < 0)
                {
                    if (this.ScaleType == ScrollIncrementScaleType.Star)
                    {
                        if (this._fetchScale == null)
                            throw new InvalidOperationException("Orientation should be called first");
                        this._scale = this._fetchScale();
                        UserPreferenceChangedEventManager.AddListener(this);
                    }
                    else
                    {
                        this._scale = 1;
                    }
                }

                return this._scale;
            }

            private set
            {
                Debug.Assert(value > 0);
                if (this._scale == value) return;
                this._scale = value;
                this.OnPropertyChanged("Scale", "Value");
            }
        }

        public double Increment
        {
            get => this._increment;
            set
            {
                if (DoubleEx.AreClose(this._increment, value)) return;
                this._increment = value;
                this.OnPropertyChanged("Increment", "Value");
            }
        }

        internal Orientation Orientation
        {
            get
            {
                if (this._fetchScale == this.FetchVerticalFactor)
                    return Orientation.Vertical;
                else if (this._fetchScale == this.FetchHorizontalFactor)
                    return Orientation.Horizontal;
                else
                    throw new InvalidOperationException("Orientation still not initialized");
            }

            set
            {
                if (this._fetchScale != null)
                {
                    if (this.Orientation == value) return;
                    throw new InvalidOperationException("Orientation can only be set once");
                }

                if (value == Orientation.Vertical)
                    this._fetchScale = this.FetchVerticalFactor;
                else
                    this._fetchScale = this.FetchHorizontalFactor;
            }
        }
        #endregion

        #region Operators
        public static implicit operator double(MouseWheelScrollIncrement source) { return source.Value; }
        #endregion

        #region Methods
        public MouseWheelScrollIncrement Clone() { return this.Clone(this.Orientation); }

        public MouseWheelScrollIncrement Clone(Orientation orientation) { return new MouseWheelScrollIncrement(this._increment, this._scaleType, orientation); }

        public MouseWheelScrollIncrement SetOrientation(Orientation orientation) { this.Orientation = orientation; return this; }
        #endregion

        #region Helpers
        private int GetScale()
        {
            if (this.ScaleType == ScrollIncrementScaleType.Unity)
                return 1;
            if (this._fetchScale == null)
                throw new InvalidOperationException("Orientation should be set first");
            return this._fetchScale();
        }

        private int FetchVerticalFactor() { return SystemParameters.WheelScrollLines; }

        private int FetchHorizontalFactor() { return SystemParametersEx.WheelScrollChars; }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Mouse)
                this.Scale = this.GetScale();
        }
        #endregion

        #endregion
    }
    #endregion

    #region MouseWheelScrollIncrementConverter
    public class MouseWheelScrollIncrementConverter : StringableConverter<MouseWheelScrollIncrement>
    {
        #region StringableConverter<MouseWheelScrollIncrement>
        protected override MouseWheelScrollIncrement Parse(string value) { return MouseWheelScrollIncrement.Parse(value); }
        #endregion
    }
    #endregion
}
