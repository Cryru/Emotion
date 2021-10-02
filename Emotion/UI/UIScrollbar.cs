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
        public int MinValue
        {
            get => _minVal;
            set
            {
                if (_minVal == value) return;
                _minVal = value;
                InvalidateLayout();
            }
        }

        private int _minVal;

        public int MaxValue
        {
            get => _maxVal;
            set
            {
                if (_maxVal == value) return;
                _maxVal = value;
                InvalidateLayout();
            }
        }

        private int _maxVal = 100;

        /// <summary>
        /// Whether the scrollbar scrolls vertically.
        /// </summary>
        public bool Vertical;

        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = Maths.Clamp(value, MinValue, MaxValue);
                OnValueChanged?.Invoke(_value);
                InvalidateLayout();
            }
        }

        private int _value;

        /// <summary>
        /// Whether to keep the selector inside the bar or to keep its center inside. By default the center is kept inside only.
        /// </summary>
        public bool KeepSelectorInside = true;

        /// <summary>
        /// How wide should the selector be compared to the total bar.
        /// </summary>
        public int SelectorRatio = 1;

        public Color DefaultSelectorColor = Color.Red;

        [DontSerialize] public Action<int>? OnValueChanged;

        private UIBaseWindow _selector = null!;
        private bool _dragging;

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
                    SetValueFromPos(mousePos);
                    _dragging = true;
                }
                else if (status == KeyStatus.Up)
                {
                    _dragging = false;
                }
            }

            return base.OnKey(key, status, mousePos);
        }

        public void SetValueFromPos(Vector2 pos)
        {
            Vector2 relativePos = pos - _renderBoundsWithChildren.Position;
            int range = MaxValue - MinValue;
            Vector2 size = Size;

            if (Vertical)
                Value = MinValue + (int) MathF.Ceiling(relativePos.X / size.X * range);
            else
                Value = MinValue + (int) MathF.Ceiling(relativePos.Y / size.Y * range);
        }

        public override void OnMouseMove(Vector2 mousePos)
        {
            if (!_dragging) return;
            SetValueFromPos(mousePos);
            base.OnMouseMove(mousePos);
        }

        public override void OnMouseScroll(float scroll)
        {
            Value += scroll > 0 ? -1 : 1;
        }

        protected override void AfterMeasure(Vector2 mySize)
        {
            mySize /= GetScale();
            int range = MaxValue - MinValue;
            Vector2 selectorSize;
            if (Vertical)
                selectorSize = new Vector2(mySize.X / range * SelectorRatio, DefaultMaxSize.Y);
            else
                selectorSize = new Vector2(DefaultMaxSize.X, mySize.Y / range * SelectorRatio);

            _selector.MaxSize = selectorSize;
        }

        protected override Vector2 BeforeLayout(Vector2 position)
        {
            Vector2 size = Size / GetScale();
            int range = MaxValue - MinValue;

            if (Vertical)
            {
                float selectorSize = _selector.Width / _selector.GetScale();
                float offset;
                if (!KeepSelectorInside)
                {
                    offset = size.X / range * Value;
                    offset -= selectorSize / 2;
                }
                else
                {
                    offset = (size.X - selectorSize) / range * Value;
                }

                _selector.Offset = new Vector2(offset, 0);
            }
            else
            {
                float selectorSize = _selector.Height / _selector.GetScale();
                float offset;
                if (!KeepSelectorInside)
                {
                    offset = size.Y / range * Value;
                    offset -= selectorSize / 2;
                }
                else
                {
                    offset = (size.Y - selectorSize) / range * Value;
                }

                _selector.Offset = new Vector2(0, float.IsNaN(offset) ? 0 : offset);
            }

            return base.BeforeLayout(position);
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Bounds, _calculatedColor);
            return true;
        }
    }
}