#region Using

using System;
using System.Numerics;
using Emotion.Common.Serialization;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Utility;

#nullable enable

#endregion

namespace Emotion.UI
{
    public class UIScrollbar : UIBaseWindow
    {
        /// <summary>
        /// Whether the scrollbar scrolls horizontally.
        /// </summary>
        public bool Horizontal;

        public Color DefaultSelectorColor = new Color(125, 0, 0);
        public Color SelectorMouseInColor = new Color(200, 0, 0);

        [DontSerialize] public UIBaseWindow? ScrollParent = null;

        private UIBaseWindow? _selector;
        private Vector2 _dragging;

        public float TotalArea;
        public float PageArea;
        public float Current;

        public UIScrollbar()
        {
            InputTransparent = false;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
            UIBaseWindow? scroll = GetWindowById("Selector");
            if (scroll == null)
            {
                scroll = new UISolidColor {WindowColor = DefaultSelectorColor, Id = "Selector", CodeGenerated = true};
                AddChild(scroll);
            }

            _selector = scroll;
        }

        public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
            if (key == Key.MouseKeyLeft)
            {
                if (status == KeyStatus.Down)
                {
                    if (_selector?.Bounds.ContainsInclusive(mousePos) ?? false)
                    {
                        _dragging = mousePos - _selector.Position2;
                    }
                    else
                    {
                        _dragging = Vector2.One;
                    }

                    OnMouseMove(mousePos);
                }
                else if (status == KeyStatus.Up)
                {
                    _dragging = Vector2.Zero;
                }
            }

            if (ScrollParent != null) return ScrollParent.OnKey(key, status, mousePos);

            return base.OnKey(key, status, mousePos);
        }

        public override void OnMouseMove(Vector2 mousePos)
        {
            if (_selector != null)
            {
                if (_dragging != Vector2.Zero || _selector.Bounds.ContainsInclusive(mousePos))
                {
                    _selector.WindowColor = SelectorMouseInColor;
                }
                else
                {
                    _selector.WindowColor = DefaultSelectorColor;
                }
            }

            if (_dragging == Vector2.Zero) return;

            float f = Y + Height;

            float progress = Maths.Map(mousePos.Y - _dragging.Y, Y, Y + Height, 0, TotalArea);
            var list = (UICallbackListNavigator?) ScrollParent;
            list?.ScrollByAbsolutePos(progress);

            base.OnMouseMove(mousePos);
        }

        public override void OnMouseScroll(float scroll)
        {
            ScrollParent?.OnMouseScroll(scroll);
        }

        public override void OnMouseLeft(Vector2 mousePos)
        {
            if (_selector != null)
            {
                _selector.WindowColor = DefaultSelectorColor;
            }

            base.OnMouseLeft(mousePos);
        }

        public void UpdateScrollbar()
        {
            if (_selector == null) return;

            if (Horizontal)
            {
                float progress = Maths.Map(Current, 0, TotalArea, 0, Width);
                progress /= GetScale();
                progress = MathF.Round(progress);

                float size = Maths.Map(PageArea, 0, TotalArea, 0, Width);
                size /= GetScale();
                size = MathF.Round(size);

                _selector.Offset = new Vector2(progress, 0);
                if (_selector.MaxSize.X != size)
                {
                    _selector.MaxSize = new Vector2(size, DefaultMaxSize.Y);
                    InvalidateLayout();
                }
                else
                {
                    _selector.X = X + _selector.Offset.X * GetScale();
                }
            }
            else
            {
                float progress = Maths.Map(Current, 0, TotalArea, 0, Height);
                progress /= GetScale();
                progress = MathF.Round(progress);

                float size = Maths.Map(PageArea, 0, TotalArea, 0, Height);
                size /= GetScale();
                size = MathF.Round(size);

                _selector.Offset = new Vector2(0, progress);
                if (_selector.MaxSize.Y != size)
                {
                    _selector.MaxSize = new Vector2(DefaultMaxSize.X, size);
                    InvalidateLayout();
                }
                else
                {
                    _selector.Y = Y + _selector.Offset.Y * GetScale();
                }
            }
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Bounds, _calculatedColor);
            return true;
        }
    }
}