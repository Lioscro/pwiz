﻿/*
 * Original author: Brendan MacLean <brendanx .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using pwiz.Skyline.Controls.Graphs;
using pwiz.Skyline.Controls.SeqNode;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.Find;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Controls
{
    public enum ReplicateDisplay { all, single, best }

    /// <summary>
    /// Displays a <see cref="SrmDocument"/> as a tree of nodes.
    /// <para>
    /// Enhanced node label editing from:
    /// http://www.codeproject.com/KB/tree/CustomizedLabelEdit.aspx?display=Print
    /// </para>
    /// </summary>
    public class SequenceTree : TreeViewMS, ITipDisplayer
    {
        private Image _dropImage;
        private TreeNode _nodeCapture;
        private IdentityPath _nodeEditPath;
        private Timer _pickTimer;
        private NodeTip _nodeTip;
        private bool _focus;
        private bool _sawDoubleClick;
        private bool _triggerLabelEdit;
        private string _editedLabel;
        private int _resultsIndex;
        private int _ratioIndex;
        private StatementCompletionTextBox _editTextBox;

        private readonly MoveThreshold _moveThreshold = new MoveThreshold(5, 5);

        /// <summary>
        /// Identity type used to select the last "dummy" node in the tree.
        /// </summary>
        private class InsertId : Identity { }

        /// <summary>
        /// Identity instance used to select the last "dummy" node in the tree.
        /// </summary>
        public static readonly Identity NODE_INSERT_ID = new InsertId();

        /// <summary>
        /// Enum of images used in the tree, in index order.
        /// </summary>
        public enum ImageId
        {
            blank,
            protein,
            peptide,
            tran_group,
            fragment,
            peptide_lib,
            tran_group_lib,
            fragment_lib
        }

        public enum StateImageId
        {
            peak,
            keep,
            no_peak,
            peak_blank            
        }

        public void InitializeTree(IDocumentUIContainer documentUIContainer)
        {            
            DocumentContainer = documentUIContainer;
            DocumentContainer.ListenUI(OnDocumentChanged);

            // Activate double buffering (unsuccessful attempts to reduce flicker)
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            // For improved control of label edit handling
            SetStyle(ControlStyles.EnableNotifyMessage, true);
            LabelEdit = false;
            
            BeforeExpand += SequenceTree_BeforeExpand;

            ImageList = new ImageList();
            ImageList.Images.Add(Resources.Blank);
            ImageList.Images.Add(Resources.Protein);
            ImageList.Images.Add(Resources.Peptide);
            ImageList.Images.Add(Resources.TransitionGroup);
            ImageList.Images.Add(Resources.Fragment);
            ImageList.Images.Add(Resources.PeptideLib);
            ImageList.Images.Add(Resources.TransitionGroupLib);
            ImageList.Images.Add(Resources.FragmentLib);

            StateImageList = new ImageList();
            StateImageList.Images.Add(Resources.Peak);
            StateImageList.Images.Add(Resources.Keep);
            StateImageList.Images.Add(Resources.NoPeak);
            StateImageList.Images.Add(Resources.PeakBlank);

            // Add the editable node at the end
            Nodes.Add(new EmptyNode());

            // One millisecond timer for showing pick list after mouse click
            // to avoid deactivation from tree view setting focus back to itself.
            _pickTimer = new Timer { Interval = 1 };
            _pickTimer.Tick += tick_ShowPickList;

            _nodeTip = new NodeTip(this);

            OnTextZoomChanged();
        }

        [Browsable(true)]
        public event EventHandler<PickedChildrenEventArgs> PickedChildrenEvent;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModFontHolder ModFonts { get; private set; }

        /// <summary>
        /// For <see cref="SrmTreeNodeParent"/> to use when child picking completes
        /// successfully, and the child set was changed.
        /// </summary>
        /// <param name="node">The parent node for which children were picked</param>
        /// <param name="list">Access to the picked list and creation of new child nodes</param>
        /// <param name="synchSiblings">True if siblings of the node should be synchronized with it</param>
        public void FirePickedChildren(SrmTreeNodeParent node, IPickedList list, bool synchSiblings)
        {
            if (PickedChildrenEvent != null)
                PickedChildrenEvent(this, new PickedChildrenEventArgs(node, list, synchSiblings));
        }

        [Browsable(true)]
        public event TreeViewEventHandler SelectedNodeChanged;

        public void FireSelectedNodeChanged()
        {
            if (SelectedNodeChanged != null)
                SelectedNodeChanged(this, new TreeViewEventArgs(SelectedNode));
        }

        /// <summary>
        /// Shows the pick list for the current node, if it allows node picking.
        /// </summary>
        public void ShowPickList()
        {
            IShowPicker picker = GetPicker(SelectedNode);
            if (picker != null)
                picker.ShowPickList(GetPickerLocation(SelectedNode));            
        }

        private void tick_ShowPickList(object sender, EventArgs e)
        {
            _pickTimer.Stop();
            ShowPickList();
        }

        /// <summary>
        /// Use to determine whether a node supports child picking.
        /// </summary>
        /// <param name="node">The tree node in question</param>
        /// <returns></returns>
        public static bool CanPickChildren(TreeNode node)
        {
            return GetPicker(node) != null;
        }

        private static IShowPicker GetPicker(TreeNode node)
        {
            IShowPicker picker = node as IShowPicker;
            return (picker != null && picker.CanShow ? picker : null);
        }

        /// <summary>
        /// Property access to the underlying document being edited.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SrmDocument Document { get { return DocumentContainer.Document; } }

        /// <summary>
        /// Handler for a document changed event raised on the main document
        /// window.  Tip and capture are removed, and all nodes in the tree updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            SrmDocument document = DocumentContainer.DocumentUI;
            // If none of the children changed, then do nothing
            if (e.DocumentPrevious != null &&
                    ReferenceEquals(document.Children, e.DocumentPrevious.Children))
            {
                return;                
            }

            HideEffects();

            if (_editTextBox != null)
            {
                CommitEditBox(true);
            }

            Control cover = null;
            try
            {
                // Cover the tree with a transparent control during document changes,
                // since they can be large, BeginUpdate can fail to hide scrollbar updates.
                if (e.DocumentPrevious != null && !ReferenceEquals(e.DocumentPrevious.Id, document.Id))
                {
                    _resultsIndex = 0;
                    _ratioIndex = 0;

                    cover = new CoverControl(this);
                }
                else
                {
                    var settings = document.Settings;
                    _resultsIndex = settings.HasResults
                        ? Math.Min(_resultsIndex, settings.MeasuredResults.Chromatograms.Count - 1)
                        : 0;
                    var mods = settings.PeptideSettings.Modifications;
                    _ratioIndex = Math.Min(_ratioIndex, mods.InternalStandardTypes.Count-1);
                }

                BeginUpdateMS();

                SrmTreeNodeParent.UpdateNodes<PeptideGroupTreeNode>(this, Nodes, document.Children,
                    true, PeptideGroupTreeNode.CreateInstance);
            }
            finally
            {
                EndUpdateMS();
                if (cover != null)
                    cover.Dispose();
            }
        }

        [Browsable(true)]
        public bool AutoExpandSingleNodes { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ResultsIndex
        {
            get { return _resultsIndex; }
            set
            {
                if (_resultsIndex != value)
                {
                    _resultsIndex = value;

                    // If showing results based on the selected index, update
                    // the display.
                    if (ShowReplicate == ReplicateDisplay.single)
                    {
                        UpdateNodeStates();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the results index for which the results are displayed in the tree,
        /// which is either the currently active result, or the best result for the
        /// given peptide, if best results are being displayed.
        /// </summary>
        /// <param name="nodePepTree">The peptide tree node from which to retreive the best result</param>
        /// <returns>The best result index for the given peptide</returns>
        public int GetDisplayResultsIndex(PeptideTreeNode nodePepTree)
        {
            return GetDisplayResultsIndex(nodePepTree != null ? nodePepTree.DocNode : null);
        }

        public int GetDisplayResultsIndex(PeptideDocNode nodePep)
        {
            int i = -1;
            if (nodePep != null && ShowReplicate == ReplicateDisplay.best)
                i = nodePep.BestResult;
            if (i == -1)
                i = ResultsIndex;
            return i;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ReplicateDisplay ShowReplicate
        {
            get
            {
                return Helpers.ParseEnum(Settings.Default.ShowTreeReplicateEnum, ReplicateDisplay.single);
            }

            set
            {
                if (ShowReplicate != value)
                {
                    Settings.Default.ShowTreeReplicateEnum = value.ToString();
                    UpdateNodeStates();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RatioIndex
        {
            get { return _ratioIndex; }
            set
            {
                if (_ratioIndex != value)
                {
                    _ratioIndex = value;
                    UpdateNodeStates();
                }
            }
        }

        /// <summary>
        /// Update states of a list of nodes and all of their children.
        /// </summary>
        /// <param name="nodes">The list of nodes to update</param>
        private static void UpdateNodeStates(TreeNodeCollection nodes)
        {
            if (nodes == null)
                return;

            foreach (var nodeTree in nodes)
            {
                var node = nodeTree as SrmTreeNode;
                if (node == null)
                    continue;
                node.UpdateState();
                UpdateNodeStates(node.Nodes);
            }
        }

        private void UpdateNodeStates()
        {
            BeginUpdate();
            UpdateNodeStates(Nodes);
            EndUpdate();            
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<IdentityPath> SelectedPaths
        {
            get
            {
                IList<IdentityPath> treeSelections = new List<IdentityPath>();
                foreach (TreeNodeMS node in SelectedNodes)
                {
                    treeSelections.Add(GetNodePath(node));
                }
                return treeSelections;
            }

            set
            {
                if (value != null)
                {
                    SelectedNodes.Clear();
                    foreach (IdentityPath path in value)
                    {
                        AddSelectedNode(Nodes, new IdentityPathTraversal(path));
                    }
                }
            }
        }

        /// <summary>
        /// Property usable for saving and restoring selection based on the
        /// underlying document structure.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IdentityPath SelectedPath
        {
            get
            {
                return GetNodePath((TreeNodeMS) SelectedNode);
            }

            set
            {
                if (value != null)
                {
                    SelectNode(Nodes, new IdentityPathTraversal(value));    
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<DocNode> SelectedDocNodes
        {
            get
            {
                foreach (var nodeTree in SelectedNodes)
                {
                    var nodeSrmTree = nodeTree as SrmTreeNode;
                    if (nodeSrmTree != null)
                        yield return nodeSrmTree.Model;
                }
            }
        }

        public bool IsInsertPath(IdentityPath path)
        {
            return path != null && ReferenceEquals(path.GetIdentity(0), NODE_INSERT_ID);
        }

        public IdentityPath GetNodePath(TreeNodeMS node)
        {
            SrmTreeNode nodeTree = node as SrmTreeNode;
            if (nodeTree == null)
                return (node == null ? null : new IdentityPath(NODE_INSERT_ID));
            return nodeTree.Path;
        }

        private void SelectNode(TreeNodeCollection treeNodes, IdentityPathTraversal traversal)
        {
            SelectedNodes.Clear();
            AddSelectedNode(treeNodes, traversal);
            if (SelectedNodes.Count > 0)
                SelectedNode = SelectedNodes.First();
        }

        /// <summary>
        /// Recursive function for restoring a tree node selection based
        /// on a path of <see cref="Identity"/> references.
        /// </summary>
        /// <param name="treeNodes">Tree nodes for the current point in the <see cref="traversal"/></param>
        /// <param name="traversal">A recursive descent traversal of a <see cref="IdentityPath"/></param>
        private void AddSelectedNode(TreeNodeCollection treeNodes, IdentityPathTraversal traversal)
        {
            // Identity paths are not allowed to be empty, so we can get the
            // first value, and then check to make sure we never descend further when
            // no next value is available.
            Identity id = traversal.Next();

            // Check of the insert node, which is a special value
            if (ReferenceEquals(id, NODE_INSERT_ID))
            {
                SelectNode((TreeNodeMS)Nodes[Nodes.Count - 1], true);
                return;
            }

            // Look for the specified child
            foreach (TreeNode nodeTree in treeNodes)
            {
                SrmTreeNode node = nodeTree as SrmTreeNode;
                if (node != null && ReferenceEquals(id, node.Model.Id))
                {
                    // If traversal is complete, select the specified node
                    if (!traversal.HasNext)
                        SelectNode(node, true);
                    // Otherwise continue descending
                    else
                    {
                        // Make sure children have been materialized
                        var nodeParent = node as SrmTreeNodeParent;
                        if (nodeParent != null)
                            nodeParent.EnsureChildren();

                        // If no children are found, then select this node
                        if (node.Nodes.Count == 0)
                            SelectNode(node, true);
                        else
                            AddSelectedNode(node.Nodes, traversal);
                    }
                }
            }
        }

        public TreeNode FindAvailableNode(TreeNodeCollection treeNodes, IdentityPathTraversal traversal)
        {
            Identity id = traversal.Next();

            if (ReferenceEquals(id, NODE_INSERT_ID))
            {
                return Nodes[Nodes.Count - 1];
            }

            foreach(TreeNode nodeTree in treeNodes)
            {
                SrmTreeNode node = nodeTree as SrmTreeNode;
                if(node != null && ReferenceEquals(id, node.Model.Id))
                {
                    if (!traversal.HasNext)
                        return node;
                    if (node.Nodes.Count != 0)
                        return FindAvailableNode(node.Nodes, traversal);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a typed <see cref="PeptideGroupTreeNode"/> list of the root tree nodes
        /// in this <see cref="TreeView"/>.
        /// </summary>
        /// <returns>A list of root nodes</returns>
        public IEnumerable<PeptideGroupTreeNode> GetSequenceNodes()
        {
            foreach (TreeNode node in Nodes)
            {                
                PeptideGroupTreeNode seqNode = node as PeptideGroupTreeNode;
                if (seqNode != null)
                    yield return seqNode;
            }
        }

        /// <summary>
        /// Returns the start node, or an ancestor, if it is of a given type.
        /// </summary>
        /// <typeparam name="TNode">The type to look for</typeparam>
        /// <param name="nodeStart">The node to start from</param>
        /// <returns>The selected node or ancestor of the desired type</returns>
        public TNode GetNodeOfType<TNode>(TreeNode nodeStart)
            where TNode : TreeNode
        {
            for (TreeNode node = nodeStart; node != null; node = node.Parent)
            {
                if (node is TNode)
                    return (TNode)node;
            }
            return null;
        }

        /// <summary>
        /// Gets the selected node, or an ancestor, if it is of a given type.
        /// </summary>
        /// <typeparam name="TNode">The type to look for</typeparam>
        /// <returns>The selected node or ancestor of the desired type</returns>
        public TNode GetNodeOfType<TNode>()
            where TNode : SrmTreeNode
        {
            return GetNodeOfType<TNode>(SelectedNode);
        }
        

        /// <summary>
        /// Hides tool tip and drop-arrow.
        /// </summary>
        public void HideEffects()
        {
            // Clear capture node to hide drop arrow
            NodeCapture = null;

            // Hide tool tip
            if (_nodeTip != null)
                _nodeTip.HideTip();

            _moveThreshold.Location = PointToClient(Cursor.Position);
        }

        /// <summary>
        /// Use to freeze on screen updates of the tree, and show an hourglass
        /// cursor during operations expected to have significant impact on
        /// the tree contents.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> instance suitable for a using block</returns>
        public IDisposable BeginLargeUpdate()
        {
            return new LargeUpdate(this);
        }

        private class LargeUpdate : IDisposable
        {
            private readonly SequenceTree _tree;
            private readonly Control _coverControl;
            private readonly Cursor _cursorBegin;
            private readonly bool _autoExpandBegin;

            public LargeUpdate(SequenceTree tree)
            {
                _tree = tree;
                _coverControl = new CoverControl(_tree);
                _cursorBegin = _tree.Parent.Cursor;
                _tree.Parent.Cursor = Cursors.WaitCursor;
                _autoExpandBegin = _tree.AutoExpandSingleNodes;
                _tree.AutoExpandSingleNodes = false;
                _tree.BeginUpdate();
            }

            public void Dispose()
            {
                _tree.EndUpdate();
                _tree.AutoExpandSingleNodes = _autoExpandBegin;
                _coverControl.Dispose();
                _tree.Parent.Cursor = _cursorBegin;
            }
        }

        protected Rectangle DropRect
        {
            get
            {
                if (_nodeCapture == null)
                    return new Rectangle();
                return GetDropRect(_nodeCapture);
            }
        }

        protected Image DropImage
        {
            get { return _dropImage ?? (_dropImage = Resources.DropImage); }
        }

        protected TreeNode NodeCapture
        {
            get { return _nodeCapture; }
            set
            {
                if (value != _nodeCapture)
                {
                    Capture = (value != null);
                    InvalidateDropRect();
                    _nodeCapture = value;
                    PaintDropImage();
                }
            }
        }

        protected Rectangle GetNoteRect(TreeNode node)
        {
            Rectangle rectNode = ((TreeNodeMS)node).BoundsMS;
            int halfHeight = rectNode.Height - 2;
            return new Rectangle(rectNode.X + rectNode.Width - halfHeight, rectNode.Y, halfHeight, halfHeight);
        }

        protected Rectangle GetDropRect(TreeNode node)
        {
            // Size for arrowhead image, centered on the node
            Rectangle rectNode = ((TreeNodeMS)node).BoundsMS;
            Image img = DropImage;
            return new Rectangle(rectNode.X + rectNode.Width + 5,
                                 rectNode.Y,
                                 img.Width,
                                 rectNode.Height);
        }

        protected Point GetPickerLocation(TreeNode node)
        {
            return GetPickerLocation(GetDropRect(node));
        }

        protected Point GetPickerLocation(Rectangle rectDrop)
        {
            var screen = Screen.FromControl(this);
            Size size = PopupPickList.SizeAll;
            Point pt = PointToScreen(rectDrop.Location);
            int y = pt.Y + rectDrop.Height;
            if (y + size.Height > screen.WorkingArea.Height)
                y = pt.Y - size.Height;
            int x = Math.Min(pt.X, screen.WorkingArea.Width - size.Width);
            
            return new Point(x, y);
        }

        protected void InvalidateDropRect()
        {
            if (_nodeCapture != null)
                Invalidate(DropRect);
        }

        protected void PaintDropImage()
        {
            if (_nodeCapture != null)
            {
                using (Graphics g = CreateGraphics())
                {
                    Rectangle dropRect = DropRect;
                    Point pt = dropRect.Location;
                    // Center the image in the rectangle.
                    pt.Y += dropRect.Height/2 - DropImage.Height/2;
                    
                    g.DrawImage(DropImage, pt);
                }
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _focus = true;
            Refresh();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            _focus = false;
            HideEffects();
            base.OnLostFocus(e);
            Refresh();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point pt = e.Location;
            if (!_moveThreshold.Moved(pt))
                return;
            _moveThreshold.Location = null;

            // Calculate UI indications of picker popup and tool tip
            TreeNodeMS node = (TreeNodeMS) GetNodeAt(pt);
            var picker = GetPicker(node);
            var tipProvider = node as ITipProvider;
            if (tipProvider != null)
                ((SrmTreeNode) node).ShowAnnotationTipOnly = GetNoteRect(node).Contains(pt);
            if (tipProvider != null && !tipProvider.HasTip)
                tipProvider = null;
            if (_focus &&  (picker != null || tipProvider != null))
            {
                Rectangle rectCapture = node.BoundsMS;
                if (tipProvider == null || !rectCapture.Contains(pt))
                    _nodeTip.HideTip();
                else
                    _nodeTip.SetTipProvider(tipProvider, rectCapture, pt);
                Rectangle rectDrop = GetDropRect(node);
                rectCapture.Width = rectDrop.Left + rectDrop.Width - rectCapture.Left;
                if (picker == null || !rectCapture.Contains(pt))
                    node = null;
                Cursor = (picker != null && rectDrop.Contains(pt) ? Cursors.Hand : Cursors.Default);
            }
            else
            {
                node = null;
                _nodeTip.HideTip();                
            }
            NodeCapture = node;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (NodeCapture is IShowPicker && DropRect.Contains(e.Location))
            {
                if (!ReferenceEquals(SelectedNode, NodeCapture))
                    SelectedNode = NodeCapture;

                // Must be done on a timer, because otherwise tree view may
                // set focus back to itself, and deactivate the pick list prematurely.
                _pickTimer.Start();
            }
            else
            {
                _triggerLabelEdit = false;

                base.OnMouseClick(e);
            }
        }

        protected void RunUI(Action act)
        {
            Invoke(act);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode node = SelectedNode;
                if (node == GetNodeAt(0, e.Y))
                {
                    if (_sawDoubleClick)
                        _sawDoubleClick = false;
                    else if (IsEditableNode(node))
                        _triggerLabelEdit = true;
                }
            }
            base.OnMouseUp(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            _sawDoubleClick = true;
            base.OnDoubleClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Space:
                    ShowPickList();
                    break;

                case Keys.End:
                    if (e.Control && Nodes.Count > 0)
                        SelectedNode = Nodes[Nodes.Count - 1];
                    break;

                case Keys.Home:
                    if (e.Control && Nodes.Count > 0)
                        SelectedNode = Nodes[0];
                    break;

                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (IsEditableNode(SelectedNode) && !Char.IsControl(e.KeyChar))
            {
                BeginEdit(true);
                string keyChar = e.KeyChar.ToString(CultureInfo.CurrentCulture);
                if (IsKeyLocked(Keys.CapsLock))
                    keyChar = keyChar.ToLower();
                SendKeys.Send(keyChar);
                e.Handled = true;
            }
            else
            {
                base.OnKeyPress(e);                
            }
        }

        protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
        {
            // Statement completion UI now handles editing, so cancel the default
            // tree node label edit behavior.
            e.CancelEdit = true;
        }

        protected void OnAfterNodeEdit(NodeLabelEditEventArgs e)
        {
            if (e.CancelEdit)
            {
                if (AfterNodeEdit != null)
                    AfterNodeEdit(this, e);                
            }
            else
            {
                LabelEdit = false;
                e.CancelEdit = true;
                if (e.Label == null)
                    return;
                var ea = new ValidateLabelEditEventArgs(e.Label);
                OnValidateLabelEdit(ea);
                if (ea.Cancel)
                {
                    e.Node.Text = _editedLabel;
                    LabelEdit = true;
                    BeginEditNode(e.Node, true);
                }
                else
                {
                    e.CancelEdit = false;
                    if (AfterNodeEdit != null)
                        AfterNodeEdit(this, e);
                }                
            }
        }

        [Browsable(true)]
        public event EventHandler<ValidateLabelEditEventArgs> ValidateLabelEdit;

        [Browsable(true)]
        public event EventHandler<NodeLabelEditEventArgs> BeforeNodeEdit;

        [Browsable(true)]
        public event EventHandler<NodeLabelEditEventArgs> AfterNodeEdit;


        protected virtual void OnValidateLabelEdit(ValidateLabelEditEventArgs e)
        {
            if (ValidateLabelEdit != null)
                ValidateLabelEdit(this, e);
        }

        protected override void OnNotifyMessage(Message m)
        {
            // No erasebackground to reduce flicker
            //if (m.Msg == (int)WinMsg.WM_ERASEBKGND)
            //    return;

            if (_triggerLabelEdit)
            {
                if (m.Msg == (int) WinMsg.WM_TIMER)
                {
                    _triggerLabelEdit = false;
                    StartLabelEdit(true);
                }
            }
            base.OnNotifyMessage(m);
        }

        public bool IsEditableNode(TreeNode node)
        {
            if (SelectedNode is EmptyNode)
                return true;
            PeptideGroupTreeNode nodeTree = node as PeptideGroupTreeNode;
            return (nodeTree != null && nodeTree.DocNode.IsPeptideList);
        }

        public void BeginEdit(bool commitOnLoseFocus)
        {
            StartLabelEdit(commitOnLoseFocus);
        }

        private void BeginEditNode(TreeNode node, bool commitOnLoseFocus)
        {
            var textBox = new TextBox
            {
                Text = node.Text,
                Bounds = node is TreeNodeMS ? (node as TreeNodeMS).BoundsMS : node.Bounds,
                Font = Font
            };

            // Only allow statement completion on the new node. Statement
            // completion for an existing peptide list gets a bit strange. If this is
            // really what the user wants, they can delete the existing node and use statement
            // completion on the new node.
            bool disableCompletion = (node is SrmTreeNode);
            if (disableCompletion)
            {
                // But allow statement completion for an empty peptide list
                var nodeGroupTree = node as PeptideGroupTreeNode;
                if (nodeGroupTree != null &&
                    nodeGroupTree.DocNode.Children.Count == 0 &&
                    nodeGroupTree.DocNode.IsPeptideList)
                {
                    disableCompletion = false;
                }
            }
            _editTextBox = new StatementCompletionTextBox(DocumentContainer)
                {AutoSizeWidth = true, DisableCompletion = disableCompletion};
            _editTextBox.Attach(textBox);
            textBox.KeyDown += textBox_KeyDown;
            _editTextBox.SelectionMade += EditBox_SelectionMade;
            if (commitOnLoseFocus)
                _editTextBox.TextBoxLoseFocus += _editTextBox_TextBoxLoseFocus;
            RepositionEditTextBox();
            Parent.Controls.Add(textBox);
            Parent.Controls.SetChildIndex(textBox, 0);
            textBox.Focus();
        }

        void _editTextBox_TextBoxLoseFocus()
        {
            CommitEditBox(false);
        }

        void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    CommitEditBox(true);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Enter:
                    CommitEditBox(false);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
            }
        }

        private void RepositionEditTextBox()
        {
            var bounds = ((TreeNodeMS) SelectedNode).BoundsMS;
            _editTextBox.TextBox.Location = new Point(bounds.Location.X + Location.X, 
                bounds.Location.Y + Location.Y);
            _editTextBox.MinimumWidth = 80;
            _editTextBox.MaximumWidth = Bounds.Width - 1 - bounds.Left;
            _editTextBox.AutoSizeWidth = true;
            _editTextBox.AutoResize();
        }

        private void EditBox_SelectionMade(StatementCompletionItem statementCompletionItem)
        {
            if (_editTextBox == null)
                return;
            _editTextBox.TextBox.Text = statementCompletionItem.ToString();
            CommitEditBox(false);
        }
        
        public void CommitEditBox(bool wasCancelled)
        {
            if (_editTextBox == null)
            {
                return;
            }
            var editTextBox = _editTextBox;
            _editTextBox = null;
            String label = editTextBox.TextBox.Text;
            editTextBox.TextBox.Parent.Controls.Remove(editTextBox.TextBox);
            editTextBox.Detach();
            var node = FindAvailableNode(Nodes, new IdentityPathTraversal(_nodeEditPath));
            if(node == null)
                return;
            NodeLabelEditEventArgs nodeLabelEditEventArgs =
                new NodeLabelEditEventArgs(node, label) {CancelEdit = wasCancelled};
            OnAfterNodeEdit(nodeLabelEditEventArgs);
        }

        public void StartLabelEdit(bool commitOnLoseFocus)
        {
            TreeNode node = SelectedNode;
            if (BeforeNodeEdit != null)
                BeforeNodeEdit(this, new NodeLabelEditEventArgs(node));
            _editedLabel = node.Text;
            LabelEdit = true;
            _nodeEditPath = SelectedPath;
            BeginEditNode(node, commitOnLoseFocus);
        }

        public StatementCompletionTextBox StatementCompletionEditBox
        {
            get { return _editTextBox; }
        }

        private void SequenceTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (!IsInUpdate)
            {
               
                SrmTreeNodeParent nodeTree = e.Node as SrmTreeNodeParent;
                if (nodeTree != null)
                {
                    // Save and restore top node to keep from scrolling
                    TreeNode nodeTop = TopNode;

                    nodeTree.EnsureChildren();

                    // Do the Windows explorer thing of expanding single node children.
                    if (AutoExpandSingleNodes && nodeTree.ChildDocNodes.Count == 1)
                        nodeTree.Nodes[0].Expand();

                    TopNode = nodeTop;
                }
            }
        }

        public Rectangle ScreenRect
        {
            get { return Screen.GetBounds(this); }
        }

        public bool AllowDisplayTip
        {
            get { return Focused || ToolTipOwner != null; }
        }

        public DisplaySettings GetDisplaySettings(PeptideDocNode nodePep)
        {
            return new DisplaySettings(nodePep, ShowReplicate == ReplicateDisplay.best, ResultsIndex, RatioIndex); 
        }

        public Rectangle RectToScreen(Rectangle r)
        {
            return RectangleToScreen(r);
        }

        public override void OnTextZoomChanged()
        {
            base.OnTextZoomChanged();
            ModFonts = new ModFontHolder(this);   
        }

        private Control _toolTipOwner;
        Control ToolTipOwner
        {
            get
            {
                return _toolTipOwner;
            }
            set
            {
                if (ToolTipOwner == value)
                {
                    return;
                }
                if (ToolTipOwner != null)
                {
                    ToolTipOwner.LostFocus -= TooltipOwnerLostFocus;
                }
                _toolTipOwner = value;
                if (ToolTipOwner != null)
                {
                    ToolTipOwner.LostFocus += TooltipOwnerLostFocus;
                }
            }
        }

        /// <summary>
        /// If the FindMatch requires showing a tooltip, displays the Tooltip
        /// for the currently selected node.
        /// </summary>
        /// <param name="owner">If not null, the SequenceTree will continue to
        /// display the tooltip until owner loses focus.  If null, the SequenceTree
        /// sets focus to itself before displaying the tooltip.</param>
        /// <param name="findMatch">The match to highlight</param>
        public void HighlightFindMatch(Control owner, FindMatch findMatch)
        {
            ToolTipOwner = null;
            if (!findMatch.Note && findMatch.AnnotationName == null)
            {
                return;
            }
            if (owner != null)
            {
                if (!owner.Focused)
                {
                    return;
                }
                ToolTipOwner = owner;
            }
            else
            {
                Focus();
            }
            var selectedMsNode = SelectedNode as SrmTreeNode;
            var tipProvider = SelectedNode as ITipProvider;
            if (tipProvider == null || selectedMsNode == null)
            {
                _nodeTip.HideTip();
                return;
            }
            selectedMsNode.ShowAnnotationTipOnly = true;
            _nodeTip.SetTipProvider(tipProvider, selectedMsNode.BoundsMS, PointToClient(Cursor.Position));
        }
        private void TooltipOwnerLostFocus(object sender, EventArgs eventArgs)
        {
            ToolTipOwner = null;
            _nodeTip.HideTip();
        }
    }

    public class ValidateLabelEditEventArgs : CancelEventArgs
    {
        public ValidateLabelEditEventArgs(string label)
        {
            Label = label;
        }

        public string Label { get; set; }
    }

    public class ModFontHolder
    {
        private readonly Control _control;

        public ModFontHolder(Control control)
        {
            _control = control;

            Heavy = new Font(Plain, FontStyle.Bold);
            Light = new Font(Plain, FontStyle.Bold | FontStyle.Underline);
        }

        public Font Plain { get { return _control.Font; } }
        public Font Light { get; private set; }
        public Font Heavy { get; private set; }
        public Font LightAndHeavy { get { return Light; } }

        public Font GetModFont(IsotopeLabelType labelType)
        {
            return (labelType.IsLight ? Light : Heavy);
        }

        public static Color GetModColor(IsotopeLabelType labelType)
        {
            if (labelType.IsLight)
                return Color.Black;

            int indexColor = labelType.SortOrder % GraphChromatogram.COLORS_GROUPS.Length;
            return GraphChromatogram.COLORS_GROUPS[indexColor];
        }
    }
}
