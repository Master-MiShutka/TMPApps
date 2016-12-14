using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TMP.Wpf.Common.Controls
{
    using System.ComponentModel;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// A helper class that provides various controls.
    /// </summary>
    public static class ControlsHelper
    {
        public static readonly DependencyProperty DisabledVisualElementVisibilityProperty = DependencyProperty.RegisterAttached("DisabledVisualElementVisibility", typeof(Visibility), typeof(ControlsHelper), new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets the value to handle the visibility of the DisabledVisualElement in the template.
        /// </summary>
        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
        public static Visibility GetDisabledVisualElementVisibility(UIElement element)
        {
            return (Visibility)element.GetValue(DisabledVisualElementVisibilityProperty);
        }

        /// <summary>
        /// Sets the value to handle the visibility of the DisabledVisualElement in the template.
        /// </summary>
        public static void SetDisabledVisualElementVisibility(UIElement element, Visibility value)
        {
            element.SetValue(DisabledVisualElementVisibilityProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the CharacterCasing property.
        /// Controls whether or not content is converted to upper or lower case
        /// </summary>
        public static readonly DependencyProperty ContentCharacterCasingProperty =
            DependencyProperty.RegisterAttached(
                "ContentCharacterCasing",
                typeof (CharacterCasing),
                typeof (ControlsHelper),
                new FrameworkPropertyMetadata(CharacterCasing.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure),
                new ValidateValueCallback(value => CharacterCasing.Normal <= (CharacterCasing) value && (CharacterCasing) value <= CharacterCasing.Upper));

        /// <summary>
        /// Gets the character casing of the control
        /// </summary>
        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(ContentControl))]
        [AttachedPropertyBrowsableForType(typeof(DropDownButton))]
        public static CharacterCasing GetContentCharacterCasing(UIElement element)
        {
            return (CharacterCasing)element.GetValue(ContentCharacterCasingProperty);
        }

        /// <summary>
        /// Sets the character casing of the control
        /// </summary>
        public static void SetContentCharacterCasing(UIElement element, CharacterCasing value)
        {
            element.SetValue(ContentCharacterCasingProperty, value);
        }


        public static readonly DependencyProperty HeaderFontStretchProperty =
            DependencyProperty.RegisterAttached("HeaderFontStretch", typeof(FontStretch), typeof(ControlsHelper), new UIPropertyMetadata(FontStretches.Normal));

        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TabItem))]
        [AttachedPropertyBrowsableForType(typeof(GroupBox))]
        public static FontStretch GetHeaderFontStretch(UIElement element)
        {
            return (FontStretch)element.GetValue(HeaderFontStretchProperty);
        }

        public static void SetHeaderFontStretch(UIElement element, FontStretch value)
        {
            element.SetValue(HeaderFontStretchProperty, value);
        }

        public static readonly DependencyProperty HeaderFontWeightProperty =
            DependencyProperty.RegisterAttached("HeaderFontWeight", typeof(FontWeight), typeof(ControlsHelper), new UIPropertyMetadata(FontWeights.Normal));

        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TabItem))]
        [AttachedPropertyBrowsableForType(typeof(GroupBox))]
        public static FontWeight GetHeaderFontWeight(UIElement element)
        {
            return (FontWeight)element.GetValue(HeaderFontWeightProperty);
        }

        public static void SetHeaderFontWeight(UIElement element, FontWeight value)
        {
            element.SetValue(HeaderFontWeightProperty, value);
        }

        /// <summary>
        /// This property can be used to set the button width (PART_ClearText) of TextBox, PasswordBox, ComboBox
        /// For multiline TextBox, PasswordBox is this the fallback for the clear text button! so it must set manually!
        /// For normal TextBox, PasswordBox the width is the height. 
        /// </summary>
        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.RegisterAttached("ButtonWidth", typeof(double), typeof(ControlsHelper),
                                                new FrameworkPropertyMetadata(22d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));

        [Category(AppName.TMPApps)]
        public static double GetButtonWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(ButtonWidthProperty);
        }

        public static void SetButtonWidth(DependencyObject obj, double value)
        {
            obj.SetValue(ButtonWidthProperty, value);
        }

        public static readonly DependencyProperty FocusBorderBrushProperty = DependencyProperty.RegisterAttached("FocusBorderBrush", typeof(Brush), typeof(ControlsHelper), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.RegisterAttached("MouseOverBorderBrush", typeof(Brush), typeof(ControlsHelper), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Sets the brush used to draw the focus border.
        /// </summary>
        public static void SetFocusBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(FocusBorderBrushProperty, value);
        }

        /// <summary>
        /// Gets the brush used to draw the focus border.
        /// </summary>
        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static Brush GetFocusBorderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(FocusBorderBrushProperty);
        }

        /// <summary>
        /// Sets the brush used to draw the mouse over brush.
        /// </summary>
        public static void SetMouseOverBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(MouseOverBorderBrushProperty, value);
        }

        /// <summary>
        /// Gets the brush used to draw the mouse over brush.
        /// </summary>
        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static Brush GetMouseOverBorderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(MouseOverBorderBrushProperty);
        }

        /// <summary>
        /// DependencyProperty for <see cref="CornerRadius" /> property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(ControlsHelper),
                                                  new FrameworkPropertyMetadata(
                                                      new CornerRadius(),
                                                      FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> 
        /// The CornerRadius property allows users to control the roundness of the button corners independently by 
        /// setting a radius value for each corner. Radius values that are too large are scaled so that they
        /// smoothly blend from corner to corner. (Can be used e.g. at MetroButton style)
        /// Description taken from original Microsoft description :-D
        /// </summary>
        [Category(AppName.TMPApps)]
        public static CornerRadius GetCornerRadius(UIElement element)
        {
            return (CornerRadius)element.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(UIElement element, CornerRadius value)
        {
            element.SetValue(CornerRadiusProperty, value);
        }
    }
}
