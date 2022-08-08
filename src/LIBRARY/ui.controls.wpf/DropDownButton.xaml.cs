namespace TMP.UI.WPF.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ContentProperty(nameof(ItemsSource))]
    [TemplatePart(Name = "PART_Button", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ButtonContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_Menu", Type = typeof(ContextMenu))]
    public class DropDownButton : ItemsControl
    {
        public static readonly RoutedEvent ClickEvent =
                    EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
                        typeof(RoutedEventHandler), typeof(DropDownButton));

        public event RoutedEventHandler Click
        {
            add { this.AddHandler(ClickEvent, value); }
            remove { this.RemoveHandler(ClickEvent, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(DropDownButton), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsExpandedChanged)));

        private static void OnIsExpandedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            DropDownButton button = (DropDownButton)dependencyObject;
            if (button.clickButton != null)
            {
                button.menu.Placement = PlacementMode.Bottom;
                button.menu.PlacementTarget = button.clickButton;
            }
        }

        public static readonly DependencyProperty StayMenuOpenProperty = DependencyProperty.Register(nameof(StayMenuOpen), typeof(bool), typeof(DropDownButton));

        public static readonly DependencyProperty ExtraTagProperty = DependencyProperty.Register(nameof(ExtraTag), typeof(object), typeof(DropDownButton));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(DropDownButton), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(object), typeof(DropDownButton));
        public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(DropDownButton));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DropDownButton));
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(DropDownButton));
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(DropDownButton));

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(DropDownButton));

        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(nameof(ButtonStyle), typeof(Style), typeof(DropDownButton), new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty MenuStyleProperty = DependencyProperty.Register(nameof(MenuStyle), typeof(Style), typeof(DropDownButton), new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty ArrowBrushProperty = DependencyProperty.Register(nameof(ArrowBrush), typeof(Brush), typeof(DropDownButton), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ArrowVisibilityProperty = DependencyProperty.Register(nameof(ArrowVisibility), typeof(Visibility), typeof(DropDownButton), new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the IsStayOpen dropdown menu
        /// </summary>
        public bool StayMenuOpen
        {
            get => (bool)this.GetValue(StayMenuOpenProperty);
            set => this.SetValue(StayMenuOpenProperty, value);
        }

        /// <summary>
        /// Gets or sets the Content of this control..
        /// </summary>
        public object Content
        {
            get => (object)this.GetValue(ContentProperty);
            set => this.SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Reflects the parameter to pass to the CommandProperty upon execution.
        /// </summary>
        public object CommandParameter
        {
            get => (object)this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Gets or sets the target element on which to fire the command.
        /// </summary>
        public IInputElement CommandTarget
        {
            get => (IInputElement)this.GetValue(CommandTargetProperty);
            set => this.SetValue(CommandTargetProperty, value);
        }

        /// <summary>
        /// Get or sets the Command property.
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Indicates whether the Menu is visible.
        /// </summary>
        public bool IsExpanded
        {
            get => (bool)this.GetValue(IsExpandedProperty);
            set => this.SetValue(IsExpandedProperty, value);
        }

        /// <summary>
        /// Gets or sets an extra tag.
        /// </summary>
        public object ExtraTag
        {
            get => this.GetValue(ExtraTagProperty);
            set => this.SetValue(ExtraTagProperty, value);
        }

        /// <summary>
        /// Gets or sets the dimension of children stacking.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        /// <summary>
        ///  Gets or sets the Content used to generate the icon part.
        /// </summary>
        [Bindable(true)]
        public object Icon
        {
            get => this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        /// <summary>
        /// Gets or sets the ContentTemplate used to display the content of the icon part.
        /// </summary>
        [Bindable(true)]
        public DataTemplate IconTemplate
        {
            get => (DataTemplate)this.GetValue(IconTemplateProperty);
            set => this.SetValue(IconTemplateProperty, value);
        }

        /// <summary>
        /// Gets/sets the button style.
        /// </summary>
        public Style ButtonStyle
        {
            get => (Style)this.GetValue(ButtonStyleProperty);
            set => this.SetValue(ButtonStyleProperty, value);
        }

        /// <summary>
        /// Gets/sets the menu style.
        /// </summary>
        public Style MenuStyle
        {
            get => (Style)this.GetValue(MenuStyleProperty);
            set => this.SetValue(MenuStyleProperty, value);
        }

        /// <summary>
        /// Gets/sets the brush of the button arrow icon.
        /// </summary>
        public Brush ArrowBrush
        {
            get => (Brush)this.GetValue(ArrowBrushProperty);
            set => this.SetValue(ArrowBrushProperty, value);
        }

        /// <summary>
        /// Gets/sets the visibility of the button arrow icon.
        /// </summary>
        public Visibility ArrowVisibility
        {
            get => (Visibility)this.GetValue(ArrowVisibilityProperty);
            set => this.SetValue(ArrowVisibilityProperty, value);
        }

        private Button clickButton;
        private ContextMenu menu;

        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            this.IsExpanded = true;
            e.RoutedEvent = ClickEvent;
            this.RaiseEvent(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.clickButton = this.EnforceInstance<Button>("PART_Button");
            this.menu = this.EnforceInstance<ContextMenu>("PART_Menu");
            this.InitializeVisualElementsContainer();
        }

        // Get element from name. If it exist then element instance return, if not, new will be created
        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            T element = this.GetTemplateChild(partName) as T ?? new T();
            return element;
        }

        private void InitializeVisualElementsContainer()
        {
            this.MouseRightButtonUp -= this.DropDownButton_MouseRightButtonUp;
            this.clickButton.Click -= this.ButtonClick;
            this.MouseRightButtonUp += this.DropDownButton_MouseRightButtonUp;
            this.clickButton.Click += this.ButtonClick;
        }

        private void DropDownButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
