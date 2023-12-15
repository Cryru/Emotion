#region Using

#nullable enable

using Emotion.Common.Serialization;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Utility;

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

        private Color _selectorColor;
        private Rectangle _selectorRect;
        private Vector2 _dragging;

        public float TotalArea;
        public float PageArea;
        public float Current;

        public Color? OutlineColor;
        public float OutlineSize;

        private Vector2 _scaledOutline;

        public UIScrollbar()
        {
            HandleInput = true;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
            _selectorColor = DefaultSelectorColor;
        }

        public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
            if (key == Key.MouseKeyLeft)
            {
                if (status == KeyStatus.Down)
                {
                    if (_selectorRect.ContainsInclusive(mousePos))
                        _dragging = mousePos - _selectorRect.Position;
                    else
                        _dragging = Vector2.One;

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
            if (_dragging != Vector2.Zero || _selectorRect.ContainsInclusive(mousePos))
                _selectorColor = SelectorMouseInColor;
            else
                _selectorColor = DefaultSelectorColor;

            if (_dragging == Vector2.Zero) return;


            float progress = Maths.Map(mousePos.Y - _dragging.Y, Y, Y + Height, 0, TotalArea);
            progress = MathF.Round(progress);
            var list = (UICallbackListNavigator?) ScrollParent;
            list?.ScrollByAbsolutePos(progress);

            base.OnMouseMove(mousePos);
        }

        public override void OnMouseLeft(Vector2 mousePos)
        {
            _selectorColor = DefaultSelectorColor;

            base.OnMouseLeft(mousePos);
        }

        public void UpdateScrollbar()
        {
            if (ScrollParent != null)
            {
                float spaceTaken = TotalArea > PageArea ? PageArea : TotalArea;
                _measuredSize.Y = spaceTaken;
                Height = spaceTaken;
            }

            if (Horizontal)
            {
                float progress = Maths.Map(Current, 0, TotalArea, 0, Width);
                progress = MathF.Round(progress);
                float size = Maths.Map(PageArea, 0, TotalArea, 0, Width);
                size = MathF.Round(size);
                _selectorRect = new Rectangle(X + progress, Y, size, Height);
            }
            else
            {
                float progress = Maths.Map(Current, 0, TotalArea, 0, Height);
                progress = MathF.Round(progress);
                float size = Maths.Map(PageArea, 0, TotalArea, 0, Height);
                size = MathF.Round(size);
                _selectorRect = new Rectangle(X, Y + progress, Width, size);
            }
        }

        protected override void AfterMeasure(Vector2 contentSize)
        {
            _scaledOutline = new Vector2(OutlineSize * GetScale());
            base.AfterMeasure(contentSize);
        }

        protected override void AfterLayout()
        {
            base.AfterLayout();
            UpdateScrollbar();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            if (OutlineColor != null)
                c.RenderSprite(Position - _scaledOutline.ToVec3(), Size + _scaledOutline * 2, OutlineColor.Value);
            c.RenderSprite(Bounds, _calculatedColor);

            c.RenderSprite(_selectorRect, _selectorColor);

            return true;
        }
    }
}