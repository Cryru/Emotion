#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Graphics;

#endregion

namespace Emotion.UI
{
    public class UITextInput : UIText
    {
        public bool MultiLine = false;
        public int MaxCharacters = -1;

        private bool _cursorOn;
        private Every _blinkingTimer;
        private float _scaledCursorDistance;

        public UITextInput()
        {
            InputTransparent = false;
            _blinkingTimer = new Every(650, () => { _cursorOn = !_cursorOn; });
        }

        public override void AttachedToController(UIController controller)
        {
            Engine.Host.OnTextInput += TextInputEventHandler;
            base.AttachedToController(controller);
        }

        public override void DetachedFromController(UIController controller)
        {
            Engine.Host.OnTextInput -= TextInputEventHandler;
            base.DetachedFromController(controller);
        }

        private void TextInputEventHandler(char c)
        {
            bool focused = Controller?.InputFocus == this;
            if (focused)
            {
                if (c == '\r') c = '\n';

                switch (c)
                {
                    case '\b':
                        if (Text.Length == 0) return;
                        Text = Text.Substring(0, Text.Length - 1);
                        TextChanged();
                        break;
                    default:
                        if (CanAddCharacter(c))
                        {
                            Text += c;
                            TextChanged();
                        }

                        break;
                }
            }
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            if (_layouter != null) _layouter.MeasureTrailingWhiteSpace = true;

            float scale = GetScale();
            _scaledCursorDistance = 3 * scale;

            base.InternalMeasure(space);

            if (!MultiLine) return new Vector2(space.X, _atlas.FontHeight);
            return space;
        }

        protected override bool UpdateInternal()
        {
            _blinkingTimer.Update(Engine.DeltaTime);
            return base.UpdateInternal();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            base.RenderInternal(c);

            bool focused = Controller?.InputFocus == this;
            if (focused && _cursorOn)
            {
                Vector2 cursorDrawStart = _layouter.GetPenLocation() + new Vector2(_scaledCursorDistance, 0);
                var top = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y, Z);
                var bottom = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y + _atlas.FontHeight + _atlas.Font.Descender * _atlas.RenderScale, Z);

                c.RenderLine(top, bottom, _calculatedColor);
            }

            return true;
        }

        protected virtual bool CanAddCharacter(char c)
        {
            if (MaxCharacters != -1 && Text.Length >= MaxCharacters) return false;
            if (c == '\n' && !MultiLine) return false;

            return true;
        }

        protected virtual void TextChanged()
        {
        }
    }
}