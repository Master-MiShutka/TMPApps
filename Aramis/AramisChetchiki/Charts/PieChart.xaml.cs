namespace TMP.WORK.AramisChetchiki.Charts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Круговая диаграмма
    /// </summary>
    public partial class PieChart : UserControl
    {
        #region dependency properties

        /// <summary>
        /// The property of the bound object that will be plotted
        /// </summary>
        public string ObjectProperty
        {
            get => (string)this.GetValue(ObjectPropertyProperty);
            set => this.SetValue(ObjectPropertyProperty, value);
        }

        // ObjectProperty dependency property
        public static readonly DependencyProperty ObjectPropertyProperty =
                       DependencyProperty.Register("ObjectProperty", typeof(string), typeof(PieChart),
                           new FrameworkPropertyMetadata(default(string), ObjectPropertyChanged));

        /// <summary>
        /// Handles changes to the PlottedProperty property.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ObjectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PieChart chart = d as PieChart;
            if (chart == null)
            {
                return;
            }

            chart.ConstructPiePieces();
        }

        /// <summary>
        /// A class which selects a color based on the item being rendered.
        /// </summary>
        public IColorSelector ColorSelector
        {
            get => (IColorSelector)this.GetValue(ColorSelectorProperty);
            set => this.SetValue(ColorSelectorProperty, value);
        }

        public static readonly DependencyProperty ColorSelectorProperty =
               DependencyProperty.Register("ColorSelector", typeof(IColorSelector), typeof(PieChart), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// The size of the hole in the centre of circle (as a percentage)
        /// </summary>
        public double HoleSize
        {
            get => (double)this.GetValue(HoleSizeProperty);
            set
            {
                this.SetValue(HoleSizeProperty, value);
                this.ConstructPiePieces();
            }
        }

        public static readonly DependencyProperty HoleSizeProperty =
                       DependencyProperty.Register("HoleSize", typeof(double), typeof(PieChart), new UIPropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// A list which contains the current piece pieces, where the piece index
        /// is the same as the index of the item within the collection view which
        /// it represents.
        /// </summary>
        private List<PiePiece> piePieces = new ();

        public PieChart()
        {
            this.InitializeComponent();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.DataContextChangedHandler);
        }

        #region property change handlers

        /// <summary>
        /// Handle changes in the datacontext. When a change occurs handlers are registered for events which
        /// occur when the collection changes or any items within teh collection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataContextChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            if (this.DataContext == null)
            {
                return;
            }

            // handle the events that occur when the bound collection changes
            if (this.DataContext is INotifyCollectionChanged)
            {
                INotifyCollectionChanged observable = (INotifyCollectionChanged)this.DataContext;
                if (observable != null)
                {
                    observable.CollectionChanged += new NotifyCollectionChangedEventHandler(this.BoundCollectionChanged);
                }
            }

            // handle the selection change events
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            collectionView.CurrentChanged += new EventHandler(this.CollectionViewCurrentChanged);
            collectionView.CurrentChanging += new CurrentChangingEventHandler(this.CollectionViewCurrentChanging);

            this.ConstructPiePieces();
            this.ObserveBoundCollectionChanges();
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Handles the MouseUp event from the individual Pie Pieces
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PiePieceMouseUp(object sender, MouseButtonEventArgs e)
        {
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            if (collectionView == null)
            {
                return;
            }

            PiePiece piece = sender as PiePiece;
            if (piece == null)
            {
                return;
            }

            // select the item which this pie piece represents
            int index = (int)piece.Tag;
            collectionView.MoveCurrentToPosition(index);
        }

        /// <summary>
        /// Handles the event which occurs when the selected item is about to change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionViewCurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            CollectionView collectionView = (CollectionView)sender;

            if (collectionView != null && collectionView.CurrentPosition >= 0 && collectionView.CurrentPosition <= this.piePieces.Count)
            {
                PiePiece piece = this.piePieces[collectionView.CurrentPosition];

                DoubleAnimation a = new ();
                a.To = 0;
                a.Duration = new Duration(TimeSpan.FromMilliseconds(200));

                piece.BeginAnimation(PiePiece.PushOutProperty, a);
            }
        }

        /// <summary>
        /// Handles the event which occurs when the selected item has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionViewCurrentChanged(object sender, EventArgs e)
        {
            CollectionView collectionView = (CollectionView)sender;

            if (collectionView != null && collectionView.CurrentPosition >= 0 && collectionView.CurrentPosition <= this.piePieces.Count)
            {
                PiePiece piece = this.piePieces[collectionView.CurrentPosition];

                DoubleAnimation a = new ();
                a.To = 10;
                a.Duration = new Duration(TimeSpan.FromMilliseconds(200));

                piece.BeginAnimation(PiePiece.PushOutProperty, a);
            }
        }

        /// <summary>
        /// Handles events which are raised when the bound collection changes (i.e. items added/removed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoundCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ConstructPiePieces();
            this.ObserveBoundCollectionChanges();
        }

        /// <summary>
        /// Iterates over the items inthe bound collection, adding handlers for PropertyChanged events
        /// </summary>
        private void ObserveBoundCollectionChanges()
        {
            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);

            foreach (object item in myCollectionView)
            {
                if (item is INotifyPropertyChanged)
                {
                    INotifyPropertyChanged observable = (INotifyPropertyChanged)item;
                    observable.PropertyChanged += new PropertyChangedEventHandler(this.ItemPropertyChanged);
                }
            }
        }

        /// <summary>
        /// Handles events which occur when the properties of bound items change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // if the property which this pie chart represents has changed, re-construct the pie
            if (string.Equals(e.PropertyName, this.ObjectProperty, AppSettings.StringComparisonMethod))
            {
                this.ConstructPiePieces();
            }
        }

        #endregion

        private double GetPropertyValue(object item)
        {
            double result = 0d;
            if (string.IsNullOrWhiteSpace(this.ObjectProperty))
            {
                return result;
            }

            PropertyDescriptorCollection filterPropDesc = TypeDescriptor.GetProperties(item);
            var prop = filterPropDesc[this.ObjectProperty];
            if (prop != null)
            {
                object value = prop.GetValue(item);
                if (value != null)
                {
                    _ = double.TryParse(value.ToString(), out result);
                }
            }

            return result;
        }

        /// <summary>
        /// Constructs pie pieces and adds them to the visual tree for this control's canvas
        /// </summary>
        private void ConstructPiePieces()
        {
            if (string.IsNullOrWhiteSpace(this.ObjectProperty))
            {
                return;
            }

            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            if (myCollectionView == null)
            {
                return;
            }

            double halfWidth = (Math.Min(this.ActualWidth, this.ActualHeight) - (15 * 2)) / 2;
            halfWidth = Math.Max(0, halfWidth);
            double innerRadius = halfWidth * this.HoleSize;

            // compute the total for the property which is being plotted
            double total = 0;
            foreach (object item in myCollectionView)
            {
                total += this.GetPropertyValue(item);
            }

            // add the pie pieces
            this.canvas.Children.Clear();
            this.piePieces.Clear();

            double accumulativeAngle = 0;
            foreach (object item in myCollectionView)
            {
                bool selectedItem = item == myCollectionView.CurrentItem;

                double wedgeAngle = this.GetPropertyValue(item) * 360 / total;

                PiePiece piece = new ()
                {
                    Radius = halfWidth,
                    InnerRadius = innerRadius,
                    CentreX = halfWidth,
                    CentreY = halfWidth,
                    PushOut = selectedItem ? 10.0 : 0,
                    WedgeAngle = wedgeAngle,
                    PieceValue = this.GetPropertyValue(item),
                    RotationAngle = accumulativeAngle,
                    Fill = this.ColorSelector != null ? this.ColorSelector.SelectBrush(item, myCollectionView.IndexOf(item)) : Brushes.Black,

                    // record the index of the item which this pie slice represents
                    Tag = myCollectionView.IndexOf(item),
                    ToolTip = new ToolTip(),
                };

                piece.ToolTipOpening += new ToolTipEventHandler(this.PiePieceToolTipOpening);
                piece.MouseUp += new MouseButtonEventHandler(this.PiePieceMouseUp);

                this.piePieces.Add(piece);
                this.canvas.Children.Insert(0, piece);

                accumulativeAngle += wedgeAngle;
            }
        }

        /// <summary>
        /// Handles the event which occurs just before a pie piece tooltip opens
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PiePieceToolTipOpening(object sender, ToolTipEventArgs e)
        {
            PiePiece piece = (PiePiece)sender;

            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            if (collectionView == null)
            {
                return;
            }

            // select the item which this pie piece represents
            int index = (int)piece.Tag;
            if (piece.ToolTip != null)
            {
                ToolTip tip = (ToolTip)piece.ToolTip;
                tip.DataContext = collectionView.GetItemAt(index);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ConstructPiePieces();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ConstructPiePieces();
        }
    }
}
