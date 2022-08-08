namespace TMP.UI.WPF.Controls
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;

    [TemplatePart(Name = "rtxt", Type = typeof(RichTextBox))]
    [TemplatePart(Name = "popup", Type = typeof(Popup))]
    [TemplatePart(Name = "lstSuggestion", Type = typeof(ListBox))]

    public sealed class MultiSelectCombobox : MultiSelector
    {
        #region Members
        private bool isHandlerRegistered = true;
        private readonly object handlerLock = new object();
        #endregion

        #region Constructor
        static MultiSelectCombobox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectCombobox), new FrameworkPropertyMetadata(typeof(MultiSelectCombobox)));
        }

        public MultiSelectCombobox()
        {
            this.PreviewKeyDown += this.MultiSelectCombobox_PreviewKeyDown;
            this.LostFocus += this.MultiSelectCombobox_LostFocus;

            this.SelectionChanged += this.MultiSelectCombobox_SelectionChanged;
        }

        private void MultiSelectCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.richTextBoxElement != null)
            {
                if (e.RemovedItems != null)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        this.richTextBoxElement.RemoveFromParagraph(item);
                    }
                }

                if (e.AddedItems != null)
                {
                    foreach (var item in e.AddedItems)
                    {
                        if (this.SelectedItems?.Contains(item) == false)
                        {
                            this.richTextBoxElement.AddToParagraph(item, this.CreateInlineUIElement);
                        }
                    }
                }
            }
        }
        #endregion

        #region Template Parts
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.RichTextBoxElement = this.GetTemplateChild("rtxt") as RichTextBox;
            this.PopupElement = this.GetTemplateChild("popup") as Popup;
            this.SuggestionElement = this.GetTemplateChild("lstSuggestion") as ListBox;
        }

        private RichTextBox richTextBoxElement;

        private RichTextBox RichTextBoxElement
        {
            get => this.richTextBoxElement;
            set
            {
                if (this.richTextBoxElement != null)
                {
                    this.richTextBoxElement.TextChanged -= this.RichTextBoxElement_TextChanged;
                }

                this.richTextBoxElement = value;

                if (this.richTextBoxElement != null)
                {
                    this.richTextBoxElement.SetParagraphAsFirstBlock();

                    // Add all selected items
                    foreach (object item in this.SelectedItems)
                    {
                        this.richTextBoxElement.AddToParagraph(item, this.CreateInlineUIElement);
                    }

                    this.richTextBoxElement.TextChanged += this.RichTextBoxElement_TextChanged;
                }
            }
        }

        private Popup PopupElement { get; set; }

        private ListBox suggestionElement;

        private ListBox SuggestionElement
        {
            get => this.suggestionElement;
            set
            {
                if (this.suggestionElement != null)
                {
                    this.suggestionElement.PreviewMouseUp -= this.SuggestionDropdown_PreviewMouseUp;
                    this.suggestionElement.PreviewKeyUp -= this.SuggestionElement_PreviewKeyUp;
                    this.suggestionElement.PreviewMouseDown -= this.SuggestionDropdown_PreviewMouseDown;
                }

                this.suggestionElement = value;
                this.suggestionElement.DisplayMemberPath = this.DisplayMemberPath;
                this.suggestionElement.ItemsSource = this.ItemsSource;

                if (this.suggestionElement != null)
                {
                    this.suggestionElement.PreviewMouseUp += this.SuggestionDropdown_PreviewMouseUp;
                    this.suggestionElement.PreviewKeyUp += this.SuggestionElement_PreviewKeyUp;
                    this.suggestionElement.PreviewMouseDown += this.SuggestionDropdown_PreviewMouseDown;

                    this.suggestionElement.SelectionChanged += SuggestionElement_SelectionChanged;
                }
            }
        }

        private void SuggestionElement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Char value that separates two selected items. Default value is ';'
        /// </summary>
        public static readonly DependencyProperty ItemSeparatorProperty =
            DependencyProperty.Register(nameof(ItemSeparator), typeof(char), typeof(MultiSelectCombobox), new PropertyMetadata(';'));

        public char ItemSeparator
        {
            get => (char)this.GetValue(ItemSeparatorProperty);
            set => this.SetValue(ItemSeparatorProperty, value);
        }

        /// <summary>
        /// ILookUpContract - implementation for custom behavior of Look-up and create. 
        /// If not set, default behavior will be set.
        /// </summary>
        public static readonly DependencyProperty LookUpContractProperty =
            DependencyProperty.Register(nameof(LookUpContract), typeof(ILookUpContract), typeof(MultiSelectCombobox), new PropertyMetadata(new DefaultLookUpContract()));

        public ILookUpContract LookUpContract
        {
            get => (ILookUpContract)this.GetValue(LookUpContractProperty);
            set => this.SetValue(LookUpContractProperty, value);
        }
        #endregion  

        #region Property changed callback

        /* /// <summary>
        /// When selected item property is changed
        /// </summary>
        /// <param name="d">control</param>
        /// <param name="e">arg</param>
        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is MultiSelectCombobox multiChoiceControl
                && e.NewValue is IList selectedItems && selectedItems != null))
            {
                return;
            }

            // Clear everything in RichTextBox
            multiChoiceControl.RichTextBoxElement?.ClearParagraph();

            // Add all selected items
            foreach (object item in selectedItems)
            {
                multiChoiceControl?.RichTextBoxElement?.AddToParagraph(item, multiChoiceControl.CreateInlineUIElement);
            }
        } */

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (this.SuggestionElement == null)
            {
                return;
            }

            this.SuggestionElement.ItemsSource = newValue?.Cast<object>();
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);

            if (this.SuggestionElement == null)
            {
                return;
            }

            this.SuggestionElement.DisplayMemberPath = newDisplayMemberPath;
        }

        #endregion

        #region Routed Event

        /// <summary>
        /// Raise SelectionChanged event
        /// </summary>
        /// <param name="removedItems">removed items</param>
        /// <param name="addedItems">added items</param>
        private void RaiseSelectionChangedEvent(IList removedItems, IList addedItems) => this.RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems));
        #endregion

        #region Control Event Handlers

        /// <summary>
        /// Suggestion drop down - key board key up
        /// Forces control to update selected item based on selection in suggestion drop down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuggestionElement_PreviewKeyUp(object sender, KeyEventArgs e) => this.UpdateSelectedItemIfSelectionIsDone(e.Key);

        private void SuggestionDropdown_PreviewMouseDown(object sender, MouseButtonEventArgs e) => this.SuggestionElement.ClearSelection(this.IsSelectionProcessCompleted);

        /// <summary>
        /// Suggestion drop down - mouse key up
        /// Forces control to update selected item based on selection in suggestion drop down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuggestionDropdown_PreviewMouseUp(object sender, MouseButtonEventArgs e) => this.UpdateSelectedItemIfSelectionIsDone();

        private void MultiSelectCombobox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // If DropDown has focus, return
                if (this.PopupElement.IsKeyboardFocusWithin)
                {
                    return;
                }

                // Remove all invalid texts from
                this.RemoveInvalidTexts();

                // Hide drop-down
                this.HideSuggestions(EntensionMethods.SuggestionCleanupOperation.ResetIndex | EntensionMethods.SuggestionCleanupOperation.ClearSelection);
            }
            catch { }
        }

        private void MultiSelectCombobox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // User can remove paragraph reference by 'Select all & delete' in RichTextBox
                // Following method call with make sure local paragraph remains part of RichTextBox
                this.RichTextBoxElement.SetParagraphAsFirstBlock();

                switch (e.Key)
                {
                    case Key.Down:
                        {
                            e.Handled = true;
                            this.HandleKeyboardDownKeyPress();
                        }
                        break;
                    case Key.Up:
                        {
                            e.Handled = true;
                            this.HandleKeyboardUpKeyPress();
                        }
                        break;
                    case Key.Enter:
                        {
                            e.Handled = true;
                            this.UpdateSelectedItemsFromSuggestionDropdown();
                        }
                        break;
                    case Key.Escape:
                        {
                            e.Handled = true;
                            this.HideSuggestions(EntensionMethods.SuggestionCleanupOperation.ResetIndex | EntensionMethods.SuggestionCleanupOperation.ClearSelection);
                            this.RichTextBoxElement.TryFocus();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        private void RichTextBoxElement_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // All text entered in Control goes to Run element of RichTextBox
                string userEnteredText = this.RichTextBoxElement.GetCurrentText();
                if (!this.IsEndOfTextDetected(userEnteredText))
                {
                    this.UpdateSuggestionAndShowHideDropDown(userEnteredText);
                    return;
                }

                if (!this.UnsubscribeHandler())
                {
                    return;
                }

                // Hide suggestion drop-down
                // Reset suggestion drop down list
                this.HideSuggestions(EntensionMethods.SuggestionCleanupOperation.ResetIndex | EntensionMethods.SuggestionCleanupOperation.ResetItemSource);

                // User is expecting to complete item selection
                if (this.IsBlankTextWithItemSeparator(userEnteredText))
                {
                    // there's nothing to select
                    // set current text to empty
                    this.RichTextBoxElement.ResetCurrentText();
                    return;
                }

                // User has entered valid text + separator
                this.RichTextBoxElement.RemoveRunBlocks();
                // Try select item from source based on current entered text
                this.UpdateSelectedItemsFromEnteredText(userEnteredText);
            }
            catch { }
            finally
            {
                // Subscribe back
                this.SubsribeHandler();
            }
        }
        #endregion

        #region Methods

        #region event handler helper methods
        private bool IsBlankTextWithItemSeparator(string userEnteredText) => string.IsNullOrWhiteSpace(userEnteredText.Trim(this.ItemSeparator));

        private bool IsEndOfTextDetected(string userEnteredText) => !string.IsNullOrEmpty(userEnteredText) && userEnteredText.EndsWith(this.ItemSeparator.ToString());

        private void UpdateSuggestionAndShowHideDropDown(string userEnteredText)
        {
            bool hasAnySuggestionToShow = this.UpdateSuggestions(userEnteredText);
            if (hasAnySuggestionToShow)
            {
                this.ShowSuggestions();
                return;
            }

            this.HideSuggestions(EntensionMethods.SuggestionCleanupOperation.ResetIndex | EntensionMethods.SuggestionCleanupOperation.ClearSelection);
            return;
        }

        private void UpdateSelectedItemIfSelectionIsDone(Key? key = null)
        {
            if (this.IsSelectionProcessInProgress(key))
            {
                return;
            }

            this.UpdateSelectedItemsFromSuggestionDropdown();
        }

        private bool IsSelectionProcessCompleted() => !this.IsSelectionProcessInProgress();

        private bool IsSelectionProcessInProgress(Key? keyUp = null)
        {
            return !keyUp.HasValue
                ? Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Shift
                : keyUp != Key.LeftCtrl && keyUp != Key.RightCtrl;
        }

        /// <summary>
        /// Removes all invalid texts from RichTextBox except selected item
        /// </summary>
        private void RemoveInvalidTexts()
        {
            try
            {
                // Unsubscribe handlers first
                if (!this.UnsubscribeHandler())
                {
                    // Failed to unsubscribe, return
                    return;
                }

                this.RichTextBoxElement.RemoveRunBlocks();
            }
            finally
            {
                // Subscribe back
                this.SubsribeHandler();
            }
        }
        #endregion

        #region Handler subscribe/unsubscribe

        /// <summary>
        /// Subscribes to events for controls
        /// </summary>
        private void SubsribeHandler()
        {
            // Check handler registration
            if (this.isHandlerRegistered)
            {
                // if already registered, return
                return;
            }

            // acquire a lock
            lock (this.handlerLock)
            {
                // double check registration
                if (this.isHandlerRegistered)
                {
                    // race condition, return
                    return;
                }

                // set handler flag to true
                this.isHandlerRegistered = true;

                // subscribe
                this.RichTextBoxElement.TextChanged += this.RichTextBoxElement_TextChanged;
            }
        }

        /// <summary>
        /// Unsubscribes to events for controls
        /// </summary>
        private bool UnsubscribeHandler()
        {
            // Check handler registration
            if (!this.isHandlerRegistered)
            {
                // If already unsubscribed, return
                return false;
            }

            // acquire a lock
            lock (this.handlerLock)
            {
                // double check registration
                if (!this.isHandlerRegistered)
                {
                    // race condition, return
                    return false;
                }

                // set handler registration flag
                this.isHandlerRegistered = false;

                // unsubscribe
                this.RichTextBoxElement.TextChanged -= this.RichTextBoxElement_TextChanged;

                return true;
            }
        }
        #endregion

        #region Selection and Index
        private void HandleKeyboardUpKeyPress()
        {
            if (!this.HasAnySuggestion())
            {
                return;
            }

            this.ShowSuggestions();

            // If multi-selection
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                this.SuggestionElement.SelectMultiplePreviousItem();
                return;
            }

            this.SuggestionElement.SelectPreviousItem();
        }

        private void HandleKeyboardDownKeyPress()
        {
            if (!this.HasAnySuggestion())
            {
                return;
            }

            this.ShowSuggestions();

            // If multi-selection
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                this.SuggestionElement.SelectMultipleNextItem();
                return;
            }

            // Increment selected item index in drop-down
            this.SuggestionElement.SelectNextItem();
        }

        /// <summary>
        /// Tries to set item from entered text in RichTextBox
        /// </summary>
        /// <param name="itemString">entered text</param>
        /// <param name="forceAdd">Allows creation of new item</param>
        private void UpdateSelectedItemsFromEnteredText(string itemString)
        {
            itemString = itemString.Trim(this.ItemSeparator, ' ');

            if (this.IsItemAlreadySelected(itemString))
            {
                return;
            }

            object itemToAdd = this.GetItemToAdd(itemString);
            // If item is not available
            if (itemToAdd == null)
            {
                return;
            }

            this.AddToSelectedItems(itemToAdd);
            this.RaiseSelectionChangedEvent(new ArrayList(0), new[] { itemToAdd });
        }

        private bool IsItemAlreadySelected(string itemString) => this.SelectedItems?.Cast<object>()?.HasAnyExactMatch(itemString, this.LookUpContract, this) == true;

        private object GetItemToAdd(string itemString)
        {
            IEnumerable<object> controlItemSource = this.ItemsSource?.Cast<object>();

            bool hasAnyMatch = controlItemSource.HasAnyExactMatch(itemString, this.LookUpContract, this);
            object itemToAdd = hasAnyMatch  // Check if any match
                                ? controlItemSource.GetExactMatch(itemString, this.LookUpContract, this) // Exact match is found
                                : (this.LookUpContract?.SupportsNewObjectCreation == true) // Check if new object creation is supported by LookUpContract
                                    ? this.LookUpContract.CreateObject(this, itemString) // Create new object using LookUpContract
                                    : null;                                             // cant create new item. return
            return itemToAdd;
        }

        private void AddToSelectedItems(object itemToAdd)
        {
            if (this.SelectedItems?.Contains(itemToAdd) == true)
            {
                return;
            }

            // Add item to Selected Item list
            this.SelectionChanged -= this.MultiSelectCombobox_SelectionChanged;
            this.SelectedItems?.Add(itemToAdd);
            this.SelectionChanged += this.MultiSelectCombobox_SelectionChanged;

            // Add item in RichTextBox UI
            this.RichTextBoxElement.AddToParagraph(itemToAdd, this.CreateInlineUIElement);
        }

        private void AddSuggestionsToSelectedItems(IList itemsToAdd)
        {
            foreach (object item in itemsToAdd)
            {
                this.AddToSelectedItems(item);
            }

            this.RaiseSelectionChangedEvent(new ArrayList(0), new ArrayList(itemsToAdd));
        }

        /// <summary>
        /// Tries to set item from suggestion drop-down
        /// </summary>
        /// <param name="runTagToRemove"></param>
        /// <param name="itemObject"></param>
        private void UpdateSelectedItemsFromSuggestionDropdown()
        {
            try
            {
                // Unsubscribe handlers first
                if (!this.UnsubscribeHandler())
                {
                    // Failed to unsubscribe, return
                    return;
                }

                // Check if drop down is open or has any item selected
                if (!this.PopupElement.IsOpen || this.SuggestionElement.SelectedItems.Count < 1)
                {
                    return;
                }

                // Remove any user entered text if any
                this.RichTextBoxElement.RemoveRunBlocks();

                this.AddSuggestionsToSelectedItems(this.SuggestionElement.SelectedItems);

                // Hide drop-down
                this.HideSuggestions(EntensionMethods.SuggestionCleanupOperation.ResetIndex | EntensionMethods.SuggestionCleanupOperation.ClearSelection | EntensionMethods.SuggestionCleanupOperation.ResetItemSource);
            }
            finally
            {
                // Subscribe back
                this.SubsribeHandler();
            }

            this.RichTextBoxElement.TryFocus();
        }
        #endregion

        #region Suggestion related methods

        /// <summary>
        /// Shows suggestion drop-down
        /// </summary>
        private void ShowSuggestions() => this.PopupElement.Show(this.HasAnySuggestion, () => this.SuggestionElement.CleanOperation(EntensionMethods.SuggestionCleanupOperation.ResetIndex, this.ItemsSource));

        private void HideSuggestions(EntensionMethods.SuggestionCleanupOperation cleanupOperation) => this.PopupElement.Hide(null, () => this.SuggestionElement.CleanOperation(cleanupOperation, this.ItemsSource));

        private bool HasAnySuggestion() => this.SuggestionElement.Items.Count > 0;

        private bool UpdateSuggestions(string userEnteredText)
        {
            // Get Items to be shown in suggestion drop-down for current text
            IEnumerable<object> itemsToAdd = this.ItemsSource?.Cast<object>().GetSuggestions(userEnteredText, this.LookUpContract, this);

            // Add suggestion items to suggestion drop-down
            this.SuggestionElement.ItemsSource = itemsToAdd;

            return itemsToAdd?.Any() == true;
        }
        #endregion

        #region UI Element creation

        /// <summary>
        /// Create RichTextBox document element for given object
        /// </summary>
        /// <param name="objectToDisplay"></param>
        /// <returns></returns>
        private InlineUIContainer CreateInlineUIElement(object objectToDisplay)
        {
            var tb = new TextBlock()
            {
                // Text based on Display member path
                Text = objectToDisplay.GetPropertyValue(this.DisplayMemberPath)?.ToString() + this.ItemSeparator,
                // Set object in Tag for easy access for future operations
                Tag = objectToDisplay,
            };

            tb.Unloaded += this.Tb_Unloaded;
            return new InlineUIContainer(tb);
        }

        /// <summary>
        /// Event to handle scenario where User removes selected item from UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!this.IsLoaded
                    || !(sender is TextBlock tb))
                {
                    return;
                }

                tb.Unloaded -= this.Tb_Unloaded;
                this.SelectedItems?.Remove(tb.Tag);
                this.RaiseSelectionChangedEvent(new[] { tb.Tag }, new ArrayList(0));
            }
            catch { }
        }
        #endregion
        #endregion
    }
}
