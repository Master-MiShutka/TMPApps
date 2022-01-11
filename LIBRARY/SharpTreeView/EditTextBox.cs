// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    internal class EditTextBox : TextBox
    {
        static EditTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditTextBox),
                new FrameworkPropertyMetadata(typeof(EditTextBox)));
        }

        public EditTextBox()
        {
            this.Loaded += delegate { this.Init(); };
        }

        public SharpTreeViewItem Item { get; set; }

        public SharpTreeNode Node => this.Item.Node;

        private void Init()
        {
            this.Text = this.Node.LoadEditText();
            this.Focus();
            this.SelectAll();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Commit();
            }
            else if (e.Key == Key.Escape)
            {
                this.Node.IsEditing = false;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (this.Node.IsEditing)
            {
                this.Commit();
            }
        }

        private bool commiting;

        private void Commit()
        {
            if (!this.commiting)
            {
                this.commiting = true;

                this.Node.IsEditing = false;
                if (!this.Node.SaveEditText(this.Text))
                {
                    this.Item.Focus();
                }

                this.Node.RaisePropertyChanged("Text");

                // if (Node.SaveEditText(Text)) {
                //    Node.IsEditing = false;
                //    Node.RaisePropertyChanged("Text");
                // }
                // else {
                //    Init();
                // }
                this.commiting = false;
            }
        }
    }
}
