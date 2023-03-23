#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Game.Time;
using Emotion.Graphics;
using Emotion.Platform.Input;

#endregion

#nullable enable

namespace Emotion.UI
{
    public class UITextInput : UIText
    {
        public bool MultiLine = false;
        public int MaxCharacters = -1;
        public bool SizeOfText = false;

        public bool SubmitOnEnter = true;
        public bool SubmitOnFocusLoss;

        [DontSerialize]
        public Action<string>? OnSubmit;

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
            // We check focused so we can hook to the text input function that isnt blocked by anything.
            Engine.Host.OnTextInputAll += TextInputEventHandler;
            base.AttachedToController(controller);
        }

        public override void DetachedFromController(UIController controller)
        {
            Engine.Host.OnTextInputAll -= TextInputEventHandler;
            base.DetachedFromController(controller);
        }

        public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
	        if (key == Key.MouseKeyLeft && status == KeyStatus.Down)
	        {
		        Controller?.SetInputFocus(this);
                return false;
	        }

	        if (SubmitOnEnter && key == Key.Enter && status == KeyStatus.Down)
	        {
		        OnSubmit?.Invoke(Text);
		        return false;
	        }

	        // Block input leaks to parents since TextInputEvents are separate and we will receive duplicate keys.
            // But not mouse inputs - direct those upward to allow things such as scrolling in a scroll area of text inputs.
            if (key is > Key.MouseKeyStart and < Key.MouseKeyEnd) return Parent!.OnKey(key, status, mousePos);

            return false;
        }

		public override void InputFocusChanged(bool haveFocus)
		{
			_blinkingTimer.Restart();
			_cursorOn = true;

			if (!haveFocus && SubmitOnFocusLoss)
			{
				OnSubmit?.Invoke(Text);
			}
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
            _scaledCursorDistance = 1 * scale;

            Vector2 textSize = base.InternalMeasure(space);
            if (SizeOfText) return textSize;

            return new Vector2(space.X, MultiLine ? space.Y : textSize.Y);
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
            if (focused && _cursorOn && _layouter != null)
            {
                Vector2 cursorDrawStart = _layouter.GetPenLocation() + new Vector2(_scaledCursorDistance, 0);
                var top = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y, Z);
                var bottom = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y + _atlas.FontHeight, Z);

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