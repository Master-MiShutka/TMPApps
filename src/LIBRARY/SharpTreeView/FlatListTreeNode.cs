// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    // This part of SharpTreeNode controls the 'flat list' data structure, which emulates
    // a big flat list containing the whole tree; allowing access by visible index.
    partial class SharpTreeNode
    {
        /// <summary>The parent in the flat list</summary>
        internal SharpTreeNode listParent;

        /// <summary>Left/right nodes in the flat list</summary>
        private SharpTreeNode left;
        /// <summary>Left/right nodes in the flat list</summary>
        SharpTreeNode right;

        internal TreeFlattener treeFlattener;

        /// <summary>Subtree height in the flat list tree</summary>
        byte height = 1;

        /// <summary>Length in the flat list, including children (children within the flat list). -1 = invalidated</summary>
        int totalListLength = -1;

        int Balance => Height(this.right) - Height(this.left);

        static int Height(SharpTreeNode node)
        {
            return node != null ? node.height : 0;
        }

        internal SharpTreeNode GetListRoot()
        {
            SharpTreeNode node = this;
            while (node.listParent != null)
            {
                node = node.listParent;
            }

            return node;
        }

        #region Debugging
        [Conditional("DEBUG")]
        private void CheckRootInvariants()
        {
            this.GetListRoot().CheckInvariants();
        }

        [Conditional("DATACONSISTENCYCHECK")]
        private void CheckInvariants()
        {
            Debug.Assert(this.left == null || this.left.listParent == this);
            Debug.Assert(this.right == null || this.right.listParent == this);
            Debug.Assert(this.height == 1 + Math.Max(Height(this.left), Height(this.right)));
            Debug.Assert(Math.Abs(this.Balance) <= 1);
            Debug.Assert(this.totalListLength == -1 || this.totalListLength == (this.left != null ? this.left.totalListLength : 0) + (this.isVisible ? 1 : 0) + (this.right != null ? this.right.totalListLength : 0));
            if (this.left != null)
            {
                this.left.CheckInvariants();
            }

            if (this.right != null)
            {
                this.right.CheckInvariants();
            }
        }

        [Conditional("DEBUG")]
        private static void DumpTree(SharpTreeNode node)
        {
            node.GetListRoot().DumpTree();
        }

        [Conditional("DEBUG")]
        private void DumpTree()
        {
            Debug.Indent();
            if (this.left != null)
            {
                this.left.DumpTree();
            }

            Debug.Unindent();
            Debug.WriteLine("{0}, totalListLength={1}, height={2}, Balance={3}, isVisible={4}", this.ToString(), this.totalListLength, this.height, this.Balance, this.isVisible);
            Debug.Indent();
            if (this.right != null)
            {
                this.right.DumpTree();
            }

            Debug.Unindent();
        }
        #endregion

        #region GetNodeByVisibleIndex / GetVisibleIndexForNode
        internal static SharpTreeNode GetNodeByVisibleIndex(SharpTreeNode root, int index)
        {
            root.GetTotalListLength(); // ensure all list lengths are calculated
            Debug.Assert(index >= 0);
            Debug.Assert(index < root.totalListLength);
            SharpTreeNode node = root;
            while (true)
            {
                if (node.left != null && index < node.left.totalListLength)
                {
                    node = node.left;
                }
                else
                {
                    if (node.left != null)
                    {
                        index -= node.left.totalListLength;
                    }

                    if (node.isVisible)
                    {
                        if (index == 0)
                        {
                            return node;
                        }

                        index--;
                    }

                    node = node.right;
                }
            }
        }

        internal static int GetVisibleIndexForNode(SharpTreeNode node)
        {
            int index = node.left != null ? node.left.GetTotalListLength() : 0;
            while (node.listParent != null)
            {
                if (node == node.listParent.right)
                {
                    if (node.listParent.left != null)
                    {
                        index += node.listParent.left.GetTotalListLength();
                    }

                    if (node.listParent.isVisible)
                    {
                        index++;
                    }
                }

                node = node.listParent;
            }

            return index;
        }
        #endregion

        #region Balancing

        /// <summary>
        /// Balances the subtree rooted in <paramref name="node"/> and recomputes the 'height' field.
        /// This method assumes that the children of this node are already balanced and have an up-to-date 'height' value.
        /// </summary>
        /// <returns>The new root node</returns>
        private static SharpTreeNode Rebalance(SharpTreeNode node)
        {
            Debug.Assert(node.left == null || Math.Abs(node.left.Balance) <= 1);
            Debug.Assert(node.right == null || Math.Abs(node.right.Balance) <= 1);

            // Keep looping until it's balanced. Not sure if this is stricly required; this is based on
            // the Rope code where node merging made this necessary.
            while (Math.Abs(node.Balance) > 1)
            {
                // AVL balancing
                // note: because we don't care about the identity of concat nodes, this works a little different than usual
                // tree rotations: in our implementation, the "this" node will stay at the top, only its children are rearranged
                if (node.Balance > 1)
                {
                    if (node.right.Balance < 0)
                    {
                        node.right = node.right.RotateRight();
                    }

                    node = node.RotateLeft();

                    // If 'node' was unbalanced by more than 2, we've shifted some of the inbalance to the left node; so rebalance that.
                    node.left = Rebalance(node.left);
                }
                else if (node.Balance < -1)
                {
                    if (node.left.Balance > 0)
                    {
                        node.left = node.left.RotateLeft();
                    }

                    node = node.RotateRight();

                    // If 'node' was unbalanced by more than 2, we've shifted some of the inbalance to the right node; so rebalance that.
                    node.right = Rebalance(node.right);
                }
            }

            Debug.Assert(Math.Abs(node.Balance) <= 1);
            node.height = (byte)(1 + Math.Max(Height(node.left), Height(node.right)));
            node.totalListLength = -1; // mark for recalculation
                                       // since balancing checks the whole tree up to the root, the whole path will get marked as invalid
            return node;
        }

        internal int GetTotalListLength()
        {
            if (this.totalListLength >= 0)
            {
                return this.totalListLength;
            }

            int length = this.isVisible ? 1 : 0;
            if (this.left != null)
            {
                length += this.left.GetTotalListLength();
            }

            if (this.right != null)
            {
                length += this.right.GetTotalListLength();
            }

            return this.totalListLength = length;
        }

        private SharpTreeNode RotateLeft()
        {
            /* Rotate tree to the left
			 *
			 *       this               right
			 *       /  \               /  \
			 *      A   right   ===>  this  C
			 *           / \          / \
			 *          B   C        A   B
			 */
            SharpTreeNode b = this.right.left;
            SharpTreeNode newTop = this.right;

            if (b != null)
            {
                b.listParent = this;
            }

            this.right = b;
            newTop.left = this;
            newTop.listParent = this.listParent;
            this.listParent = newTop;

            // rebalance the 'this' node - this is necessary in some bulk insertion cases:
            newTop.left = Rebalance(this);
            return newTop;
        }

        private SharpTreeNode RotateRight()
        {
            /* Rotate tree to the right
			 *
			 *       this             left
			 *       /  \             /  \
			 *     left  C   ===>    A   this
			 *     / \                   /  \
			 *    A   B                 B    C
			 */
            SharpTreeNode b = this.left.right;
            SharpTreeNode newTop = this.left;

            if (b != null)
            {
                b.listParent = this;
            }

            this.left = b;
            newTop.right = this;
            newTop.listParent = this.listParent;
            this.listParent = newTop;
            newTop.right = Rebalance(this);
            return newTop;
        }

        private static void RebalanceUntilRoot(SharpTreeNode pos)
        {
            while (pos.listParent != null)
            {
                if (pos == pos.listParent.left)
                {
                    pos = pos.listParent.left = Rebalance(pos);
                }
                else
                {
                    Debug.Assert(pos == pos.listParent.right);
                    pos = pos.listParent.right = Rebalance(pos);
                }

                pos = pos.listParent;
            }

            SharpTreeNode newRoot = Rebalance(pos);
            if (newRoot != pos && pos.treeFlattener != null)
            {
                Debug.Assert(newRoot.treeFlattener == null);
                newRoot.treeFlattener = pos.treeFlattener;
                pos.treeFlattener = null;
                newRoot.treeFlattener.root = newRoot;
            }

            Debug.Assert(newRoot.listParent == null);
            newRoot.CheckInvariants();
        }
        #endregion

        #region Insertion
        private static void InsertNodeAfter(SharpTreeNode pos, SharpTreeNode newNode)
        {
            // newNode might be the model root of a whole subtree, so go to the list root of that subtree:
            newNode = newNode.GetListRoot();
            if (pos.right == null)
            {
                pos.right = newNode;
                newNode.listParent = pos;
            }
            else
            {
                // insert before pos.right's leftmost:
                pos = pos.right;
                while (pos.left != null)
                {
                    pos = pos.left;
                }

                Debug.Assert(pos.left == null);
                pos.left = newNode;
                newNode.listParent = pos;
            }

            RebalanceUntilRoot(pos);
        }
        #endregion

        #region Removal
        private void RemoveNodes(SharpTreeNode start, SharpTreeNode end)
        {
            // Removes all nodes from start to end (inclusive)
            // All removed nodes will be reorganized in a separate tree, do not delete
            // regions that don't belong together in the tree model!
            List<SharpTreeNode> removedSubtrees = new List<SharpTreeNode>();
            SharpTreeNode oldPos;
            SharpTreeNode pos = start;
            do
            {
                // recalculate the endAncestors every time, because the tree might have been rebalanced
                HashSet<SharpTreeNode> endAncestors = new HashSet<SharpTreeNode>();
                for (SharpTreeNode tmp = end; tmp != null; tmp = tmp.listParent)
                {
                    endAncestors.Add(tmp);
                }

                removedSubtrees.Add(pos);
                if (!endAncestors.Contains(pos))
                {
                    // we can remove pos' right subtree in a single step:
                    if (pos.right != null)
                    {
                        removedSubtrees.Add(pos.right);
                        pos.right.listParent = null;
                        pos.right = null;
                    }
                }

                SharpTreeNode succ = pos.Successor();
                DeleteNode(pos); // this will also rebalance out the deletion of the right subtree

                oldPos = pos;
                pos = succ;
            }
            while (oldPos != end);

            // merge back together the removed subtrees:
            SharpTreeNode removed = removedSubtrees[0];
            for (int i = 1; i < removedSubtrees.Count; i++)
            {
                removed = ConcatTrees(removed, removedSubtrees[i]);
            }
        }

        private static SharpTreeNode ConcatTrees(SharpTreeNode first, SharpTreeNode second)
        {
            SharpTreeNode tmp = first;
            while (tmp.right != null)
            {
                tmp = tmp.right;
            }

            InsertNodeAfter(tmp, second);
            return tmp.GetListRoot();
        }

        private SharpTreeNode Successor()
        {
            if (this.right != null)
            {
                SharpTreeNode node = this.right;
                while (node.left != null)
                {
                    node = node.left;
                }

                return node;
            }
            else
            {
                SharpTreeNode node = this;
                SharpTreeNode oldNode;
                do
                {
                    oldNode = node;
                    node = node.listParent;

                    // loop while we are on the way up from the right part
                }
                while (node != null && node.right == oldNode);
                return node;
            }
        }

        private static void DeleteNode(SharpTreeNode node)
        {
            SharpTreeNode balancingNode;
            if (node.left == null)
            {
                balancingNode = node.listParent;
                node.ReplaceWith(node.right);
                node.right = null;
            }
            else if (node.right == null)
            {
                balancingNode = node.listParent;
                node.ReplaceWith(node.left);
                node.left = null;
            }
            else
            {
                SharpTreeNode tmp = node.right;
                while (tmp.left != null)
                {
                    tmp = tmp.left;
                }

                // First replace tmp with tmp.right
                balancingNode = tmp.listParent;
                tmp.ReplaceWith(tmp.right);
                tmp.right = null;
                Debug.Assert(tmp.left == null);
                Debug.Assert(tmp.listParent == null);

                // Now move node's children to tmp:
                tmp.left = node.left;
                node.left = null;
                tmp.right = node.right;
                node.right = null;
                if (tmp.left != null)
                {
                    tmp.left.listParent = tmp;
                }

                if (tmp.right != null)
                {
                    tmp.right.listParent = tmp;
                }

                // Then replace node with tmp
                node.ReplaceWith(tmp);
                if (balancingNode == node)
                {
                    balancingNode = tmp;
                }
            }

            Debug.Assert(node.listParent == null);
            Debug.Assert(node.left == null);
            Debug.Assert(node.right == null);
            node.height = 1;
            node.totalListLength = -1;
            if (balancingNode != null)
            {
                RebalanceUntilRoot(balancingNode);
            }
        }

        private void ReplaceWith(SharpTreeNode node)
        {
            if (this.listParent != null)
            {
                if (this.listParent.left == this)
                {
                    this.listParent.left = node;
                }
                else
                {
                    Debug.Assert(this.listParent.right == this);
                    this.listParent.right = node;
                }

                if (node != null)
                {
                    node.listParent = this.listParent;
                }

                this.listParent = null;
            }
            else
            {
                // this was a root node
                Debug.Assert(node != null); // cannot delete the only node in the tree
                node.listParent = null;
                if (this.treeFlattener != null)
                {
                    Debug.Assert(node.treeFlattener == null);
                    node.treeFlattener = this.treeFlattener;
                    this.treeFlattener = null;
                    node.treeFlattener.root = node;
                }
            }
        }
        #endregion
    }
}
