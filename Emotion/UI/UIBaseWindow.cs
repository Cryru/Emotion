#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Standard.XML;

#endregion

#nullable enable

namespace Emotion.UI
{
    [DontSerializeMembers("Position", "Size")]
    public class UIBaseWindow : Transform, IRenderable, IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
    {
        #region Properties

        public string? Id { get; set; }

        /// <summary>
        /// The point in the parent to anchor the window to.
        /// </summary>
        public UIAnchor ParentAnchor { get; set; } = UIAnchor.TopLeft;

        /// <summary>
        /// Where the window should anchor to relative to the alignment in its parent.
        /// </summary>
        public UIAnchor Anchor { get; set; } = UIAnchor.TopLeft;

        /// <summary>
        /// How to layout the children of this window.
        /// </summary>
        public LayoutMode LayoutMode { get; set; } = LayoutMode.Free;

        /// <summary>
        /// Spacing if the LayoutMode is a list.
        /// </summary>
        public Vector2 ListSpacing { get; set; }

        public bool Visible { get; set; } = true;

        public bool DontTakeSpaceWhenHidden { get; set; }

        public Color Color { get; set; } = Color.White;
        public List<UIBaseWindow>? Children { get; set; }
        public Vector2 MinSize { get; set; }
        public Vector2 MaxSize { get; set; } = new(9999, 9999);
        public UIScaleMode ScaleMode { get; set; } = UIScaleMode.FloatScale;
        public bool StretchX { get; set; }
        public bool StretchY { get; set; }

        #endregion

        #region State

        public UIBaseWindow? Parent { get; protected set; }
        public UIDebugger? Debugger { get; protected set; }

        #endregion

        protected bool _updateLayout = true;
        protected bool _updateColor = true;
        protected Color _calculatedColor;

        public virtual async Task Preload()
        {
            if (Children != null)
                for (var i = 0; i < Children.Count; i++)
                {
                    UIBaseWindow child = Children[i];
                    await child.Preload();
                }
        }

        public virtual void AddChild(UIBaseWindow child, int index = -1)
        {
            Children ??= new List<UIBaseWindow>();
            if (index != -1)
                Children.Insert(index, child);
            else
                Children.Add(child);

            child.Parent = this;
            child.InvalidateLayout();
            if (Debugger != null) child.Debugger = Debugger;
        }

        public void RemoveChild(UIBaseWindow win, bool evict = true)
        {
            if (Children == null) return;
            Debug.Assert(win.Parent == this);

            if (evict) Children.Remove(win);
        }

        public virtual void ClearChildren()
        {
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                RemoveChild(child, false);
            }

            Children = null;
        }

        public void InvalidateLayout()
        {
            _updateLayout = true;
            Parent?.InvalidateLayout();
        }

        public void InvalidateColor()
        {
            _updateColor = true;
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.InvalidateColor();
            }
        }

        private Vector2 _measuredSize;

        protected virtual Vector2 InternalMeasure(Vector2 space)
        {
            return space;
        }

        protected Vector2 Measure(Vector2 space)
        {
            float scale = GetScale();
            Vector2 contentSize = InternalMeasure(space);
            Debugger?.RecordMetric(this, "InternalMeasure", contentSize);
            Vector2 usedSpace = Vector2.Zero;

            if (Children != null)
            {
                Vector2 freeSpace = StretchX || StretchY ? space : contentSize;
                Vector2 scaledSpacing = ListSpacing * scale;
                bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
                switch (LayoutMode)
                {
                    case LayoutMode.Free:
                        for (var i = 0; i < Children.Count; i++)
                        {
                            UIBaseWindow child = Children[i];
                            Vector2 childSize = child.Measure(freeSpace);
                            // if (AnchorsInsideParent(child.ParentAnchor, child.Anchor))
                            usedSpace = Vector2.Max(usedSpace, childSize);
                        }

                        break;
                    case LayoutMode.HorizontalListWrap:
                    case LayoutMode.HorizontalList:
                        for (var i = 0; i < Children.Count; i++)
                        {
                            UIBaseWindow child = Children[i];
                            Vector2 childSize = child.Measure(freeSpace);
                            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

                            float spaceTaken = childSize.X;
                            if (i != Children.Count - 1) spaceTaken += scaledSpacing.X;

                            freeSpace.X -= spaceTaken;
                            usedSpace.X += spaceTaken;
                            usedSpace.Y = MathF.Max(usedSpace.Y, childSize.Y);
                        }

                        break;
                    case LayoutMode.VerticalListWrap:
                    case LayoutMode.VerticalList:
                        for (var i = 0; i < Children.Count; i++)
                        {
                            UIBaseWindow child = Children[i];
                            Vector2 childSize = child.Measure(freeSpace);
                            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

                            float spaceTaken = childSize.Y;
                            if (i != Children.Count - 1) spaceTaken += scaledSpacing.Y;

                            freeSpace.Y -= spaceTaken;
                            usedSpace.Y += spaceTaken;
                            usedSpace.X = MathF.Max(usedSpace.X, childSize.X);
                        }

                        break;
                }
            }

            var size = new Vector2(StretchX ? usedSpace.X : contentSize.X, StretchY ? usedSpace.Y : contentSize.Y);
            _measuredSize = Vector2.Clamp(size, MinSize * scale, MaxSize * scale);
            Debugger?.RecordMetric(this, "InternalMeasure PostClamp", _measuredSize);

            return _measuredSize;
        }

        protected void Layout(Vector2 contentPos, Vector2 contentSize)
        {
            Debugger?.RecordMetric(this, "ContentPos", contentPos);
            Debugger?.RecordMetric(this, "ContentSize", contentSize);

            Size = _measuredSize;
            Position = contentPos.ToVec3(Z);

            if (Children == null) return;

            float scale = GetScale();
            bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
            Vector2 scaledSpacing = ListSpacing * scale;
            switch (LayoutMode)
            {
                case LayoutMode.Free:
                {
                    for (var i = 0; i < Children.Count; i++)
                    {
                        UIBaseWindow child = Children[i];
                        Vector2 pos = GetUIAnchorPosition(child.ParentAnchor, _measuredSize, child.Anchor, child._measuredSize);
                        child.Layout(contentPos + pos, Vector2.Zero);
                    }

                    break;
                }
                case LayoutMode.HorizontalListWrap:
                case LayoutMode.HorizontalList:
                {
                    Vector2 pen = contentPos;
                    Vector2 sizeLeft = _measuredSize;
                    for (var i = 0; i < Children.Count; i++)
                    {
                        UIBaseWindow child = Children[i];
                        Vector2 childSize = child._measuredSize;
                        Vector2 pos = GetUIAnchorPosition(child.ParentAnchor, sizeLeft, child.Anchor, childSize);
                        child.Layout(pen + pos, Vector2.Zero);
                        if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

                        float spaceTaken = childSize.X;
                        if (i != Children.Count - 1) spaceTaken += scaledSpacing.X;

                        pen.X += spaceTaken;
                        sizeLeft.X -= spaceTaken;
                    }

                    break;
                }
                case LayoutMode.VerticalListWrap:
                case LayoutMode.VerticalList:
                {
                    Vector2 pen = contentPos;
                    Vector2 sizeLeft = _measuredSize;
                    for (var i = 0; i < Children.Count; i++)
                    {
                        UIBaseWindow child = Children[i];
                        Vector2 childSize = child._measuredSize;
                        Vector2 pos = GetUIAnchorPosition(child.ParentAnchor, sizeLeft, child.Anchor, childSize);
                        child.Layout(pen + pos, Vector2.Zero);
                        if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

                        float spaceTaken = childSize.Y;
                        if (i != Children.Count - 1) spaceTaken += scaledSpacing.Y;

                        pen.Y += spaceTaken;
                        sizeLeft.Y -= spaceTaken;
                    }

                    break;
                }
            }

            _updateLayout = false;
        }

        protected void CalculateColor()
        {
            // todo;
            _calculatedColor = Color;
            _updateColor = true;

            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow? child = Children[i];
                child.CalculateColor();
            }
        }

        public void Render(RenderComposer c)
        {
            if (!Visible) return;
            if (!RenderInternal(c, ref _calculatedColor)) return;

            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                if (!child.Visible) continue;
                child.Render(c);
            }
        }

        protected virtual bool RenderInternal(RenderComposer c, ref Color windowColor)
        {
            c.RenderSprite(Position, Size, windowColor);
            return true;
        }

        #region Layout

        /// <summary>
        /// A very simple check for whether the anchors will land the window inside or outside the parent.
        /// This decides whether the window will layout within the parent or outside it.
        /// </summary>
        /// <returns></returns>
        protected static bool AnchorsInsideParent(UIAnchor parentAnchor, UIAnchor anchor)
        {
            bool parentIsTop = parentAnchor is UIAnchor.TopLeft or UIAnchor.TopCenter or UIAnchor.TopRight;
            bool parentIsVCenter = parentAnchor is UIAnchor.CenterLeft or UIAnchor.CenterCenter or UIAnchor.CenterRight;
            bool parentIsBottom = parentAnchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight;

            bool parentIsLeft = parentAnchor is UIAnchor.TopLeft or UIAnchor.CenterLeft or UIAnchor.BottomLeft;
            bool parentIsHCenter = parentAnchor is UIAnchor.TopCenter or UIAnchor.CenterCenter or UIAnchor.BottomCenter;
            bool parentIsRight = parentAnchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight;

            if (parentIsTop)
            {
                if (parentIsLeft && anchor == UIAnchor.TopLeft) return true;

                if (parentIsHCenter && anchor is UIAnchor.TopLeft or UIAnchor.TopCenter or UIAnchor.TopRight) return true;

                if (parentIsRight && anchor == UIAnchor.TopRight) return true;
            }
            else if (parentIsVCenter)
            {
                if (parentIsLeft && anchor is UIAnchor.TopLeft or UIAnchor.CenterLeft or UIAnchor.BottomLeft) return true;
                if (parentIsHCenter) return true;
                if (parentIsRight && anchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight) return true;
            }
            else if (parentIsBottom)
            {
                if (parentIsLeft && anchor == UIAnchor.BottomLeft) return true;
                if (parentIsHCenter && anchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight) return true;
                if (parentIsRight && anchor == UIAnchor.BottomRight) return true;
            }

            return false;
        }

        protected static Vector2 GetUIAnchorPosition(UIAnchor parentAnchor, Vector2 parentSize, UIAnchor anchor, Vector2 contentSize)
        {
            Vector2 offset = Vector2.Zero;

            switch (parentAnchor)
            {
                case UIAnchor.TopLeft:
                case UIAnchor.CenterLeft:
                case UIAnchor.BottomLeft:
                    offset.X += 0;
                    break;
                case UIAnchor.TopCenter:
                case UIAnchor.CenterCenter:
                case UIAnchor.BottomCenter:
                    offset.X += parentSize.X / 2;
                    break;
                case UIAnchor.TopRight:
                case UIAnchor.CenterRight:
                case UIAnchor.BottomRight:
                    offset.X += parentSize.X;
                    break;
            }

            switch (parentAnchor)
            {
                case UIAnchor.TopLeft:
                case UIAnchor.TopCenter:
                case UIAnchor.TopRight:
                    offset.Y += 0;
                    break;
                case UIAnchor.CenterLeft:
                case UIAnchor.CenterCenter:
                case UIAnchor.CenterRight:
                    offset.Y += parentSize.Y / 2;
                    break;
                case UIAnchor.BottomLeft:
                case UIAnchor.BottomCenter:
                case UIAnchor.BottomRight:
                    offset.Y += parentSize.Y;
                    break;
            }

            switch (anchor)
            {
                case UIAnchor.TopCenter:
                case UIAnchor.CenterCenter:
                case UIAnchor.BottomCenter:
                    offset.X -= contentSize.X / 2;
                    break;
                case UIAnchor.TopRight:
                case UIAnchor.CenterRight:
                case UIAnchor.BottomRight:
                    offset.X -= contentSize.X;
                    break;
            }

            switch (anchor)
            {
                case UIAnchor.CenterLeft:
                case UIAnchor.CenterCenter:
                case UIAnchor.CenterRight:
                    offset.Y -= contentSize.Y / 2;
                    break;
                case UIAnchor.BottomLeft:
                case UIAnchor.BottomCenter:
                case UIAnchor.BottomRight:
                    offset.Y -= contentSize.Y;
                    break;
            }

            return offset;
        }

        /// <summary>
        /// The scale factor applied on the UI.
        /// </summary>
        /// <returns></returns>
        public float GetScale()
        {
            return ScaleMode switch
            {
                UIScaleMode.FloatScale => Engine.Renderer.Scale,
                UIScaleMode.IntScale => Engine.Renderer.IntScale,
                _ => 1.0f
            };
        }

        #endregion

        /// <summary>
        /// Get a window with the specified id which is either a child of this window,
        /// or below it on the tree.
        /// </summary>
        /// <param name="id">The id of the window to look for.</param>
        /// <returns>The instance of the window.</returns>
        public virtual UIBaseWindow? GetWindowById(string id)
        {
            if (id == "Controller")
            {
                UIBaseWindow cur = this;
                while (cur.Parent != null)
                {
                    if (cur is UIController) return cur;
                    cur = cur.Parent;
                }
            }

            if (Children == null) return null;

            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i].Id == id) return Children[i];
            }

            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow? win = Children[i].GetWindowById(id);
                if (win != null) return win;
            }

            return null;
        }

        /// <summary>
        /// Clone the window. By default this performs a serialization clone.
        /// </summary>
        /// <returns></returns>
        public UIBaseWindow Clone()
        {
            string xml = XMLFormat.To(this);
            return XMLFormat.From<UIBaseWindow>(xml);
        }

        /// <summary>
        /// Compares the windows based on their Z order.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(UIBaseWindow? other)
        {
            if (other == null) return MathF.Sign(Z);
            return MathF.Sign(Z - other.Z);
        }

        public override string ToString()
        {
            return $"{GetType().ToString().Replace("Emotion.UI.", "")} {Id}";
        }

        #region Enum

        public IEnumerator<UIBaseWindow> GetEnumerator()
        {
            for (var i = 0; i < Children?.Count; i++)
            {
                UIBaseWindow cur = Children[i];

                // Get children.
                var childrenLists = new Queue<List<UIBaseWindow>>();
                if (cur.Children != null) childrenLists.Enqueue(cur.Children);
                yield return cur;

                while (childrenLists.Count > 0)
                {
                    List<UIBaseWindow> list = childrenLists.Dequeue();
                    for (var j = 0; j < list.Count; j++)
                    {
                        UIBaseWindow child = list[j];
                        // Get grandchildren if any.
                        if (child.Children?.Count > 0) childrenLists.Enqueue(child.Children);
                        yield return child;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}