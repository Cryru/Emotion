#nullable enable

using Emotion.Primitives.DataStructures;

namespace Emotion.Editor.EditorUI.Components.One;

public class OneTreeView<TBranchName, TLeafType> : UIBaseWindow
{
    private const int RowHeight = 38;
    private const int RowSpacing = 4;
    private const int BranchIndent = 42;
    private const int RowTextPaddingLeft = 38;
    private const int RowTextPaddingRight = 12;
    private const int ChevronCenterX = 20;
    private const int ChevronSize = 8;
    private const int ConnectorInset = 23;
    private const int ConnectorEndInset = 4;

    public NTree<TBranchName, TLeafType> Tree;

    public bool BranchesSelectable = true;
    public bool LeavesSelectable = true;

    public Action<NTree<TBranchName, TLeafType>>? OnBranchSelected;
    public Action<TLeafType>? OnLeafSelected;

    private OneTreeViewBranch? _selectedBranch;
    private OneTreeViewItem? _selectedLeaf;

    public OneTreeView(NTree<TBranchName, TLeafType> tree)
    {
        Layout.LayoutMethod = UILayoutMethod.VerticalList(RowSpacing);
        Layout.OverflowX = UIOverflow.Scroll;
        Layout.OverflowY = UIOverflow.Scroll;
        Layout.Padding = new UISpacing(4, 4, 4, 4);

        Tree = tree;

        var branchWindows = new Dictionary<NTree<TBranchName, TLeafType>, OneTreeViewBranch>();

        foreach ((TLeafType leaf, NTree<TBranchName, TLeafType> branch) in Tree.ForEachLeafWithBranch())
        {
            UIBaseWindow leafParent = this;

            if (branch != Tree)
            {
                var parentStack = new Stack<NTree<TBranchName, TLeafType>>();
                NTree<TBranchName, TLeafType>? current = branch;
                while (current != null && current != Tree)
                {
                    parentStack.Push(current);
                    current = current.Parent;
                }

                while (parentStack.TryPop(out NTree<TBranchName, TLeafType>? branchToSpawn))
                {
                    if (!branchWindows.TryGetValue(branchToSpawn, out OneTreeViewBranch? branchWindow))
                    {
                        var newBranch = new OneTreeViewBranch(branchToSpawn.Name?.ToString() ?? string.Empty);
                        newBranch.Row.OnClicked = (_) =>
                        {
                            newBranch.ToggleExpanded();

                            if (!BranchesSelectable) return;

                            _selectedBranch?.Row.SetSelected(false);
                            _selectedLeaf?.SetSelected(false);
                            _selectedBranch = newBranch;
                            _selectedLeaf = null;
                            newBranch.Row.SetSelected(true);
                            OnBranchSelected?.Invoke(branchToSpawn);
                        };

                        leafParent.AddChild(newBranch);
                        branchWindows.Add(branchToSpawn, newBranch);
                        branchWindow = newBranch;
                    }

                    leafParent = branchWindow.ChildrenContainer;
                }
            }

            var leafRow = new OneTreeViewItem(leaf?.ToString() ?? string.Empty, false);
            leafRow.OnClicked = (_) =>
            {
                if (!LeavesSelectable) return;

                _selectedBranch?.Row.SetSelected(false);
                _selectedLeaf?.SetSelected(false);
                _selectedBranch = null;
                _selectedLeaf = leafRow;
                leafRow.SetSelected(true);
                OnLeafSelected?.Invoke(leaf);
            };
            leafParent.AddChild(leafRow);
        }
    }

    private class OneTreeViewBranch : UIBaseWindow
    {
        public readonly OneTreeViewItem Row;
        public readonly OneTreeViewChildrenContainer ChildrenContainer;

        public OneTreeViewBranch(string label)
        {
            Layout.LayoutMethod = UILayoutMethod.VerticalList(RowSpacing);
            Layout.SizingX = UISizing.Grow();
            Layout.SizingY = UISizing.Fit();

            Row = new OneTreeViewItem(label, true);
            AddChild(Row);

            ChildrenContainer = new OneTreeViewChildrenContainer(Row)
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.VerticalList(RowSpacing),
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Fit(),
                    Padding = new UISpacing(BranchIndent, 0, 0, 0)
                },
                Visuals =
                {
                    DontTakeSpaceWhenHidden = true
                }
            };
            AddChild(ChildrenContainer);
        }

        public void ToggleExpanded()
        {
            SetExpanded(!ChildrenContainer.Visuals.Visible);
        }

        private void SetExpanded(bool expanded)
        {
            ChildrenContainer.Visuals.Visible = expanded;
            Row.SetExpanded(expanded);
        }
    }

    private class OneTreeViewChildrenContainer : UIBaseWindow
    {
        private readonly OneTreeViewItem _parentRow;

        public OneTreeViewChildrenContainer(OneTreeViewItem parentRow)
        {
            _parentRow = parentRow;
        }

        protected override void InternalBeforeRenderChildren(Renderer r)
        {
            base.InternalBeforeRenderChildren(r);
            if (Children.Count == 0) return;

            float scale = CalculatedMetrics.ScaleF;
            float lineExtension = RowSpacing * scale / 2f;
            float thickness = MathF.Max(1, scale);
            float lineX = 0;
            float firstCenterY = 0;
            float lastCenterY = 0;
            bool foundChild = false;

            for (int i = 0; i < Children.Count; i++)
            {
                OneTreeViewItem? row = Children[i] switch
                {
                    OneTreeViewBranch branch => branch.Row,
                    OneTreeViewItem item => item,
                    _ => null
                };
                if (row == null || !row.Visuals.Visible) continue;

                Vector2 rowPosition = row.CalculatedMetrics.Position.ToVec2();
                float centerY = rowPosition.Y + row.CalculatedMetrics.Size.Y / 2f;
                float currentLineX = rowPosition.X - ConnectorInset * scale;

                if (!foundChild)
                {
                    lineX = currentLineX;
                    firstCenterY = centerY;
                    foundChild = true;
                }

                lastCenterY = centerY;
                r.RenderLine(
                    new Vector2(currentLineX, centerY),
                    new Vector2(rowPosition.X - ConnectorEndInset * scale, centerY),
                    OnePalette.PRIMARY_1,
                    thickness
                );
            }

            if (!foundChild) return;

            float parentCenterY = _parentRow.CalculatedMetrics.Position.Y + _parentRow.CalculatedMetrics.Size.Y / 2f;
            r.RenderLine(
                new Vector2(lineX, parentCenterY),
                new Vector2(lineX, firstCenterY + lineExtension),
                OnePalette.PRIMARY_1,
                thickness
            );

            if (lastCenterY != firstCenterY)
            {
                r.RenderLine(
                    new Vector2(lineX, firstCenterY),
                    new Vector2(lineX, lastCenterY),
                    OnePalette.PRIMARY_1,
                    thickness
                );
            }
        }
    }

    private class OneTreeViewItem : UIBaseButton
    {
        private readonly UIText _label;
        private readonly bool _branch;
        private bool _expanded = true;
        private bool _selected;

        public OneTreeViewItem(string label, bool branch)
        {
            _branch = branch;

            Layout = new UIWindowLayoutConfig()
            {
                SizingX = UISizing.Grow(),
                SizingY = UISizing.Fixed(RowHeight),
                Padding = new UISpacing(RowTextPaddingLeft, 0, RowTextPaddingRight, 0)
            };
            Visuals = new UIWindowVisualConfig()
            {
                RoundRadius = 4
            };

            var text = new UIText()
            {
                Text = label,
                TextColor = OnePalette.PRIMARY_1,
                FontSize = OnePalette.FONT_SIZE,
                Font = OnePalette.FONT,
                WrapText = false,
                Layout =
                {
                    AnchorAndParentAnchor = UIAnchor.CenterLeft
                }
            };
            AddChild(text);
            _label = text;

            ApplyState();
        }

        public void SetExpanded(bool expanded)
        {
            if (_expanded == expanded) return;
            _expanded = expanded;
        }

        public void SetSelected(bool selected)
        {
            if (_selected == selected) return;
            _selected = selected;
            ApplyState();
        }

        public override void OnMouseEnter(Vector2 mousePos)
        {
            base.OnMouseEnter(mousePos);
            ApplyState();
        }

        public override void OnMouseLeft(Vector2 mousePos)
        {
            base.OnMouseLeft(mousePos);
            ApplyState();
        }

        private void ApplyState()
        {
            Visuals.BackgroundColor = _selected ? OnePalette.PRIMARY_7 : MouseInside ? OnePalette.PRIMARY_5 : OnePalette.PRIMARY_6;
            _label.TextColor = OnePalette.PRIMARY_1;
        }

        protected override void InternalRender(Renderer r)
        {
            base.InternalRender(r);
            if (!_branch) return;

            float scale = CalculatedMetrics.ScaleF;
            float size = ChevronSize * scale;
            float thickness = MathF.Max(2, 2 * scale);
            Vector2 center = CalculatedMetrics.Position.ToVec2() + new Vector2(ChevronCenterX * scale, CalculatedMetrics.Size.Y / 2f);

            if (_expanded)
            {
                r.RenderLine(center + new Vector2(-size, -size / 2f), center, OnePalette.PRIMARY_1, thickness);
                r.RenderLine(center, center + new Vector2(size, -size / 2f), OnePalette.PRIMARY_1, thickness);
            }
            else
            {
                r.RenderLine(center + new Vector2(-size / 2f, -size), center + new Vector2(size / 2f, 0), OnePalette.PRIMARY_1, thickness);
                r.RenderLine(center + new Vector2(size / 2f, 0), center + new Vector2(-size / 2f, size), OnePalette.PRIMARY_1, thickness);
            }
        }
    }
}
