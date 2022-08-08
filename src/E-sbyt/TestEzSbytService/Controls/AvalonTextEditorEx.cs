using ICSharpCode.AvalonEdit;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TMP.Work.AmperM.TestApp.Controls
{
    /// <summary>
    /// Class that inherits from the AvalonEdit TextEditor control to 
    /// enable MVVM interaction. 
    /// </summary>
    public class AvalonTextEditorEx : TextEditor, INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor to set up event handlers.
        /// </summary>
        public AvalonTextEditorEx()
        {
            TextArea.SelectionChanged += TextArea_SelectionChanged;
            TextArea.Caret.PositionChanged += Caret_PositionChanged;
            this.PreviewMouseWheel += AvalonTextEditorEx_PreviewMouseWheel;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("CaretOffset");
        }

        /// <summary>
        /// Event handler to update properties based upon the selection changed event.
        /// </summary>
        void TextArea_SelectionChanged(object sender, EventArgs e)
        {
            this.SelectionStart = SelectionStart;
            this.SelectionLength = SelectionLength;
        }

        private void AvalonTextEditorEx_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            bool handle = (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) > 0;
            if (!handle)
                return;

            if (e.Delta > 0 && this.TextArea.FontSize < 40.0) this.TextArea.FontSize++;

            if (e.Delta < 0 && this.TextArea.FontSize > 5.0) this.TextArea.FontSize--;
        }

        #region Text
        /// <summary>
        /// Dependancy property for the editor text property binding.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
             DependencyProperty.Register("Text", typeof(string), typeof(AvalonTextEditorEx),
             new PropertyMetadata((obj, args) =>
             {
                 AvalonTextEditorEx target = (AvalonTextEditorEx)obj;
                 target.Text = (string)args.NewValue;
             }));

        /// <summary>
        /// Provide access to the Text.
        /// </summary>
        public new string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// Return the current text length.
        /// </summary>
        public int Length
        {
            get { return base.Text.Length; }
        }

        /// <summary>
        /// Override of OnTextChanged event.
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            RaisePropertyChanged("Length");            
            base.OnTextChanged(e);
        }
        #endregion // Text.

        #region Caret Offset
        /// <summary>
        /// Return the CaretOffset
        /// </summary>
        public new int CaretOffset
        {
            get { return base.CaretOffset; }
        }
        #endregion // Caret Offset.

        #region Selection
        /// <summary>
        /// DependencyProperty for the TextEditor SelectionLength property. 
        /// </summary>
        public static readonly DependencyProperty SelectionLengthProperty =
             DependencyProperty.Register("SelectionLength", typeof(int), typeof(AvalonTextEditorEx),
             new PropertyMetadata((obj, args) =>
             {
                 AvalonTextEditorEx target = (AvalonTextEditorEx)obj;
                 target.SelectionLength = (int)args.NewValue;
             }));

        /// <summary>
        /// Access to the SelectionLength property.
        /// </summary>
        public new int SelectionLength
        {
            get { return base.SelectionLength; }
            set { SetValue(SelectionLengthProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for the TextEditor SelectionStart property. 
        /// </summary>
        public static readonly DependencyProperty SelectionStartProperty =
             DependencyProperty.Register("SelectionStart", typeof(int), typeof(AvalonTextEditorEx),
             new PropertyMetadata((obj, args) =>
             {
                 AvalonTextEditorEx target = (AvalonTextEditorEx)obj;
                 target.SelectionStart = (int)args.NewValue;
             }));

        /// <summary>
        /// Access to the SelectionStart property.
        /// </summary>
        public new int SelectionStart
        {
            get { return base.SelectionStart; }
            set { SetValue(SelectionStartProperty, value); }
        }
        #endregion // Selection.

        /// <summary>
        /// Implement the INotifyPropertyChanged event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string caller = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }
    }
}
