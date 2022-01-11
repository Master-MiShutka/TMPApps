// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    public class SharpTreeViewItem : ListViewItem
    {
        static SharpTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SharpTreeViewItem),
                                                     new FrameworkPropertyMetadata(typeof(SharpTreeViewItem)));
        }

        public SharpTreeNode Node => this.DataContext as SharpTreeNode;

        public SharpTreeNodeView NodeView { get; internal set; }

        public SharpTreeView ParentTreeView { get; internal set; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    // if (SharpTreeNode.ActiveNodes.Count == 1 && Node.IsEditable) {
                    // Node.IsEditing = true;
                    // e.Handled = true;
                    // }
                    break;
                case Key.Escape:
                    this.Node.IsEditing = false;
                    break;
            }
        }

        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new SharpTreeViewItemAutomationPeer(this);
        }

        #region Mouse

        private Point startPoint;
        private bool wasSelected;
        private bool wasDoubleClick;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.wasSelected = this.IsSelected;
            if (!this.IsSelected)
            {
                base.OnMouseLeftButtonDown(e);
            }

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.startPoint = e.GetPosition(null);
                this.CaptureMouse();

                if (e.ClickCount == 2)
                {
                    this.wasDoubleClick = true;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                var currentPoint = e.GetPosition(null);
                if (Math.Abs(currentPoint.X - this.startPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - this.startPoint.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {

                    var selection = this.ParentTreeView.GetTopLevelSelection().ToArray();
                    if (this.Node.CanDrag(selection))
                    {
                        this.Node.StartDrag(this, selection);
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (this.wasDoubleClick)
            {
                this.wasDoubleClick = false;
                this.Node.ActivateItem(e);
                if (!e.Handled)
                {
                    if (!this.Node.IsRoot || this.ParentTreeView.ShowRootExpander)
                    {
                        this.Node.IsExpanded = !this.Node.IsExpanded;
                    }
                }
            }

            this.ReleaseMouseCapture();
            if (this.wasSelected)
            {
                base.OnMouseLeftButtonDown(e);
            }
        }

        #endregion

        #region Drag and Drop

        protected override void OnDragEnter(DragEventArgs e)
        {
            this.ParentTreeView.HandleDragEnter(this, e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            this.ParentTreeView.HandleDragOver(this, e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            this.ParentTreeView.HandleDrop(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            this.ParentTreeView.HandleDragLeave(this, e);
        }

        #endregion
    }
}
