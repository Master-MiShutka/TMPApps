﻿namespace TMP.UI.WPF.Controls.Helpers
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    public enum ButtonsAlignment
    {
        Left,
        Right,
    }

    public class TextBoxHelper
    {
        public static readonly DependencyProperty IsMonitoringProperty = DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(TextBoxHelper), new UIPropertyMetadata(false, OnIsMonitoringChanged));

        public static bool GetIsMonitoring(DependencyObject obj, bool value)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox)
            {
                var txtBox = d as TextBox;

                if ((bool)e.NewValue)
                {
                    txtBox.TextChanged += TextChanged;
                    txtBox.GotFocus += TextBoxGotFocus;

                    txtBox.Dispatcher.BeginInvoke((Action)(() =>
                        TextChanged(txtBox, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None))));
                }
                else
                {
                    txtBox.TextChanged -= TextChanged;
                    txtBox.GotFocus -= TextBoxGotFocus;
                }
            }
            else if (d is PasswordBox)
            {
                var passBox = d as PasswordBox;

                if ((bool)e.NewValue)
                {
                    passBox.PasswordChanged += PasswordChanged;
                    passBox.GotFocus += PasswordGotFocus;

                    // Also fixes 1343, also triggers the show of the floating watermark if necessary
                    passBox.Dispatcher.BeginInvoke((Action)(() =>
                        PasswordChanged(passBox, new RoutedEventArgs(PasswordBox.PasswordChangedEvent, passBox))));
                }
                else
                {
                    passBox.PasswordChanged -= PasswordChanged;
                    passBox.GotFocus -= PasswordGotFocus;
                }
            }
        }

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(TextBoxHelper), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty UseFloatingWatermarkProperty = DependencyProperty.RegisterAttached("UseFloatingWatermark", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, ButtonCommandOrClearTextChanged));
        public static readonly DependencyProperty TextLengthProperty = DependencyProperty.RegisterAttached("TextLength", typeof(int), typeof(TextBoxHelper), new UIPropertyMetadata(0));
        public static readonly DependencyProperty ClearTextButtonProperty = DependencyProperty.RegisterAttached("ClearTextButton", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, ButtonCommandOrClearTextChanged));
        public static readonly DependencyProperty TextButtonProperty = DependencyProperty.RegisterAttached("TextButton", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, ButtonCommandOrClearTextChanged));
        public static readonly DependencyProperty ButtonsAlignmentProperty = DependencyProperty.RegisterAttached("ButtonsAlignment", typeof(ButtonsAlignment), typeof(TextBoxHelper), new FrameworkPropertyMetadata(ButtonsAlignment.Right, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty IsClearTextButtonBehaviorEnabledProperty = DependencyProperty.RegisterAttached("IsClearTextButtonBehaviorEnabled", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, IsClearTextButtonBehaviorEnabledChanged));

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.RegisterAttached("ButtonCommand", typeof(ICommand), typeof(TextBoxHelper), new FrameworkPropertyMetadata(null, ButtonCommandOrClearTextChanged));
        public static readonly DependencyProperty ButtonCommandParameterProperty = DependencyProperty.RegisterAttached("ButtonCommandParameter", typeof(object), typeof(TextBoxHelper), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.RegisterAttached("ButtonContent", typeof(object), typeof(TextBoxHelper), new FrameworkPropertyMetadata("r"));
        public static readonly DependencyProperty ButtonContentTemplateProperty = DependencyProperty.RegisterAttached("ButtonContentTemplate", typeof(DataTemplate), typeof(TextBoxHelper), new FrameworkPropertyMetadata((DataTemplate)null));
        public static readonly DependencyProperty ButtonTemplateProperty = DependencyProperty.RegisterAttached("ButtonTemplate", typeof(ControlTemplate), typeof(TextBoxHelper), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ButtonFontFamilyProperty = DependencyProperty.RegisterAttached("ButtonFontFamily", typeof(FontFamily), typeof(TextBoxHelper), new FrameworkPropertyMetadata(new FontFamilyConverter().ConvertFromString("Marlett")));

        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsWaitingForDataProperty = DependencyProperty.RegisterAttached("IsWaitingForData", typeof(bool), typeof(TextBoxHelper), new UIPropertyMetadata(false));

        public static readonly DependencyProperty HasTextProperty = DependencyProperty.RegisterAttached("HasText", typeof(bool), typeof(TextBoxHelper), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

        public static void SetIsWaitingForData(DependencyObject obj, bool value)
        {
            obj.SetValue(IsWaitingForDataProperty, value);
        }

        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static bool GetIsWaitingForData(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsWaitingForDataProperty);
        }

        public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static bool GetSelectAllOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectAllOnFocusProperty);
        }

        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }

        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static bool GetUseFloatingWatermark(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseFloatingWatermarkProperty);
        }

        public static void SetUseFloatingWatermark(DependencyObject obj, bool value)
        {
            obj.SetValue(UseFloatingWatermarkProperty, value);
        }

        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static bool GetHasText(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasTextProperty);
        }

        public static void SetHasText(DependencyObject obj, bool value)
        {
            obj.SetValue(HasTextProperty, value);
        }

        private static void SetTextLength<TDependencyObject>(TDependencyObject sender, Func<TDependencyObject, int> funcTextLength)
            where TDependencyObject : DependencyObject
        {
            if (sender != null)
            {
                var value = funcTextLength(sender);
                sender.SetValue(TextLengthProperty, value);
                sender.SetValue(HasTextProperty, value >= 1);
            }
        }

        private static void TextChanged(object sender, RoutedEventArgs e)
        {
            SetTextLength(sender as TextBox, textBox => textBox.Text.Length);
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            SetTextLength(sender as PasswordBox, passwordBox => passwordBox.Password.Length);
        }

        private static void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            ControlGotFocus(sender as TextBox, textBox => textBox.SelectAll());
        }

        private static void PasswordGotFocus(object sender, RoutedEventArgs e)
        {
            ControlGotFocus(sender as PasswordBox, passwordBox => passwordBox.SelectAll());
        }

        private static void ControlGotFocus<TDependencyObject>(TDependencyObject sender, Action<TDependencyObject> action)
            where TDependencyObject : DependencyObject
        {
            if (sender != null)
            {
                if (GetSelectAllOnFocus(sender))
                {
                    sender.Dispatcher.BeginInvoke(action, sender);
                }
            }
        }

        /// <summary>
        /// Gets the clear text button visibility / feature. Can be used to enable text deletion.
        /// </summary>
        [Category(AppName.TMPApps)]
        public static bool GetClearTextButton(DependencyObject d)
        {
            return (bool)d.GetValue(ClearTextButtonProperty);
        }

        /// <summary>
        /// Sets the clear text button visibility / feature. Can be used to enable text deletion.
        /// </summary>
        public static void SetClearTextButton(DependencyObject obj, bool value)
        {
            obj.SetValue(ClearTextButtonProperty, value);
        }

        /// <summary>
        /// Gets the text button visibility.
        /// </summary>
        [Category(AppName.TMPApps)]
        public static bool GetTextButton(DependencyObject d)
        {
            return (bool)d.GetValue(TextButtonProperty);
        }

        /// <summary>
        /// Sets the text button visibility.
        /// </summary>
        public static void SetTextButton(DependencyObject obj, bool value)
        {
            obj.SetValue(TextButtonProperty, value);
        }

        /// <summary>
        /// Gets the buttons placement variant.
        /// </summary>
        [Category(AppName.TMPApps)]
        public static ButtonsAlignment GetButtonsAlignment(DependencyObject d)
        {
            return (ButtonsAlignment)d.GetValue(ButtonsAlignmentProperty);
        }

        /// <summary>
        /// Sets the buttons placement variant.
        /// </summary>
        public static void SetButtonsAlignment(DependencyObject obj, ButtonsAlignment value)
        {
            obj.SetValue(ButtonsAlignmentProperty, value);
        }

        /// <summary>
        /// Gets the clear text button behavior.
        /// </summary>
        [Category(AppName.TMPApps)]
        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static bool GetIsClearTextButtonBehaviorEnabled(Button d)
        {
            return (bool)d.GetValue(IsClearTextButtonBehaviorEnabledProperty);
        }

        /// <summary>
        /// Sets the clear text button behavior.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static void SetIsClearTextButtonBehaviorEnabled(Button obj, bool value)
        {
            obj.SetValue(IsClearTextButtonBehaviorEnabledProperty, value);
        }

        [Category(AppName.TMPApps)]
        public static ICommand GetButtonCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(ButtonCommandProperty);
        }

        public static void SetButtonCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ButtonCommandProperty, value);
        }

        [Category(AppName.TMPApps)]
        public static object GetButtonCommandParameter(DependencyObject d)
        {
            return (object)d.GetValue(ButtonCommandParameterProperty);
        }

        public static void SetButtonCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(ButtonCommandParameterProperty, value);
        }

        [Category(AppName.TMPApps)]
        public static object GetButtonContent(DependencyObject d)
        {
            return (object)d.GetValue(ButtonContentProperty);
        }

        public static void SetButtonContent(DependencyObject obj, object value)
        {
            obj.SetValue(ButtonContentProperty, value);
        }

        /// <summary>
        /// ButtonContentTemplate is the template used to display the content of the ClearText button.
        /// </summary>
        [Category(AppName.TMPApps)]
        public static DataTemplate GetButtonContentTemplate(DependencyObject d)
        {
            return (DataTemplate)d.GetValue(ButtonContentTemplateProperty);
        }

        public static void SetButtonContentTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(ButtonContentTemplateProperty, value);
        }

        [Category(AppName.TMPApps)]
        public static ControlTemplate GetButtonTemplate(DependencyObject d)
        {
            return (ControlTemplate)d.GetValue(ButtonTemplateProperty);
        }

        public static void SetButtonTemplate(DependencyObject obj, ControlTemplate value)
        {
            obj.SetValue(ButtonTemplateProperty, value);
        }

        [Category(AppName.TMPApps)]
        public static FontFamily GetButtonFontFamily(DependencyObject d)
        {
            return (FontFamily)d.GetValue(ButtonFontFamilyProperty);
        }

        public static void SetButtonFontFamily(DependencyObject obj, FontFamily value)
        {
            obj.SetValue(ButtonFontFamilyProperty, value);
        }

        private static void IsClearTextButtonBehaviorEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as Button;
            if (e.OldValue != e.NewValue && button != null)
            {
                button.Click -= ButtonClicked;
                if ((bool)e.NewValue)
                {
                    button.Click += ButtonClicked;
                }
            }
        }

        public static void ButtonClicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var parent = VisualTreeHelper.GetParent(button);
            while (!(parent is TextBox || parent is PasswordBox || parent is ComboBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var command = GetButtonCommand(parent);
            var commandParameter = GetButtonCommandParameter(parent) ?? parent;
            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }

            if (GetClearTextButton(parent))
            {
                if (parent is TextBox)
                {
                    ((TextBox)parent).Text = null;
                }
                else if (parent is PasswordBox)
                {
                    ((PasswordBox)parent).Clear();
                }
                else if (parent is ComboBox)
                {
                    if (((ComboBox)parent).IsEditable)
                    {
                        ((ComboBox)parent).Text = string.Empty;
                    }

                    ((ComboBox)parent).SelectedItem = null;
                }
            }
        }

        private static void ButtonCommandOrClearTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textbox)
            {
                // only one loaded event
                textbox.Loaded -= TextChanged;
                textbox.Loaded += TextChanged;
                if (textbox.IsLoaded)
                {
                    TextChanged(textbox, new RoutedEventArgs());
                }
            }

            if (d is PasswordBox passbox)
            {
                // only one loaded event
                passbox.Loaded -= PasswordChanged;
                passbox.Loaded += PasswordChanged;
                if (passbox.IsLoaded)
                {
                    PasswordChanged(passbox, new RoutedEventArgs());
                }
            }

            if (d is ComboBox combobox)
            {
                // only one loaded event
                combobox.Loaded -= ComboBoxLoaded;
                combobox.Loaded += ComboBoxLoaded;
                if (combobox.IsLoaded)
                {
                    ComboBoxLoaded(combobox, new RoutedEventArgs());
                }
            }
        }

        private static void ComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.SetValue(HasTextProperty, !string.IsNullOrWhiteSpace(comboBox.Text) || comboBox.SelectedItem != null);
            }
        }
    }
}
