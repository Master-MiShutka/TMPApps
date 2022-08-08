// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    public class SharpTreeNodeView : Control
    {
        static SharpTreeNodeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SharpTreeNodeView),
                                                     new FrameworkPropertyMetadata(typeof(SharpTreeNodeView)));
        }

        public static readonly DependencyProperty TextBackgroundProperty =
            DependencyProperty.Register("TextBackground", typeof(Brush), typeof(SharpTreeNodeView));

        public Brush TextBackground
        {
            get => (Brush)this.GetValue(TextBackgroundProperty);
            set => this.SetValue(TextBackgroundProperty, value);
        }

        public SharpTreeNode Node => this.DataContext as SharpTreeNode;

        public SharpTreeViewItem ParentItem { get; private set; }

        public static readonly DependencyProperty CellEditorProperty =
            DependencyProperty.Register("CellEditor", typeof(Control), typeof(SharpTreeNodeView),
                                        new FrameworkPropertyMetadata());

        public Control CellEditor
        {
            get => (Control)this.GetValue(CellEditorProperty);
            set => this.SetValue(CellEditorProperty, value);
        }

        public SharpTreeView ParentTreeView => this.ParentItem.ParentTreeView;

        internal LinesRenderer LinesRenderer { get; private set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.LinesRenderer = this.Template.FindName("linesRenderer", this) as LinesRenderer;
            this.UpdateTemplate();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            this.ParentItem = this.FindAncestor<SharpTreeViewItem>();
            this.ParentItem.NodeView = this;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty)
            {
                this.UpdateDataContext(e.OldValue as SharpTreeNode, e.NewValue as SharpTreeNode);
            }
        }

        private void UpdateDataContext(SharpTreeNode oldNode, SharpTreeNode newNode)
        {
            if (newNode != null)
            {
                newNode.PropertyChanged += this.Node_PropertyChanged;
                if (this.Template != null)
                {
                    this.UpdateTemplate();
                }
            }

            if (oldNode != null)
            {
                oldNode.PropertyChanged -= this.Node_PropertyChanged;
            }
        }

        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEditing")
            {
                this.OnIsEditingChanged();
            }
            else if (e.PropertyName == "IsLast")
            {
                if (this.ParentTreeView.ShowLines)
                {
                    foreach (var child in this.Node.VisibleDescendantsAndSelf())
                    {
                        if (this.ParentTreeView.ItemContainerGenerator.ContainerFromItem(child) is SharpTreeViewItem container)
                        {
                            container.NodeView.LinesRenderer.InvalidateVisual();
                        }
                    }
                }
            }
            else if (e.PropertyName == "IsExpanded")
            {
                if (this.Node.IsExpanded)
                {
                    this.ParentTreeView.HandleExpanding(this.Node);
                }
            }
        }

        private void OnIsEditingChanged()
        {
            var textEditorContainer = this.Template.FindName("textEditorContainer", this) as Border;
            if (this.Node.IsEditing)
            {
                if (this.CellEditor == null)
                {
                    textEditorContainer.Child = new EditTextBox() { Item = this.ParentItem };
                }
                else
                {
                    textEditorContainer.Child = this.CellEditor;
                }
            }
            else
            {
                textEditorContainer.Child = null;
            }
        }

        private void UpdateTemplate()
        {
            if (this.IsVisible)
            {
                var spacer = this.Template.FindName("spacer", this) as FrameworkElement;
                spacer.Width = this.CalculateIndent();

                var expander = this.Template.FindName("expander", this) as ToggleButton;
                if (this.ParentTreeView.Root == this.Node && !this.ParentTreeView.ShowRootExpander)
                {
                    expander.Visibility = Visibility.Collapsed;
                }
                else
                {
                    expander.ClearValue(VisibilityProperty);
                }
            }
        }

        internal double CalculateIndent()
        {
            var result = 19 * this.Node.Level;
            if (this.ParentTreeView.ShowRoot)
            {
                if (!this.ParentTreeView.ShowRootExpander)
                {
                    if (this.ParentTreeView.Root != this.Node)
                    {
                        result -= 15;
                    }
                }
            }
            else
            {
                result -= 19;
            }

            if (result < 0)
            {
                throw new InvalidOperationException();
            }

            return result;
        }
    }
}
