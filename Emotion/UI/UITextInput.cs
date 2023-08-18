#region Using

using System;
using System.Numerics;
using System.Text;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Game.Text;
using Emotion.Game.Time;
using Emotion.Graphics;
using Emotion.Graphics.Text;
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

        private int _selectionStart = -1;
        private int _selectionEnd = -1;
        private bool _selectionHeld = false;

        public UITextInput()
        {
            HandleInput = true;
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
            if (SubmitOnEnter && key == Key.Enter && status == KeyStatus.Down)
            {
                OnSubmit?.Invoke(Text);
                return false;
            }

            if (key == Key.LeftArrow && status == KeyStatus.Down)
            {
                _selectionEnd--;
                EnsureSelectionRight();
                _selectionStart = _selectionEnd;
            }

            if (key == Key.RightArrow && status == KeyStatus.Down)
            {
                _selectionEnd++;
                EnsureSelectionRight();
                _selectionStart = _selectionEnd;
            }

            // Selection drag
            if (key == Key.MouseKeyLeft)
            {
                if (status == KeyStatus.Down)
                {
                    _selectionHeld = true;

                    var closestSepIdx = GetSelectionIndexUnderCursor(mousePos);
                    if (closestSepIdx != -1)
                    {
                        _selectionStart = closestSepIdx;
                        _selectionEnd = _selectionStart;
                        EnsureSelectionRight();
                    }
                }
                else if (status == KeyStatus.Up)
                {
                    _selectionHeld = false;
                }
            }

            if (status == KeyStatus.Down && Engine.Host.IsCtrlModifierHeld())
            {
                if (key == Key.A) // Select All
                {
                    _selectionStart = 0;
                    _selectionEnd = Text.Length;
                    EnsureSelectionRight();

                    return false;
                }
                else if (key == Key.C) // Copy
                {
                    var selection = GetSelectedText();
                    if (selection.Length != 0)
                    {

                    }

                    return false;
                }
                else if (key == Key.V) // Paste
                {
                    return false;
                }
                else if (key == Key.Z) // Undo
                {
                    return false;
                }
                else if (key == Key.Y) // Redo
                {
                    return false;
                }
            }

            // Block input leaks to parents since TextInputEvents are separate and we will receive duplicate keys.
            // But not mouse inputs - direct those upward to allow things such as scrolling in a scroll area of text inputs.
            if (key is > Key.MouseKeyStart and < Key.MouseKeyEnd)
                return Parent!.OnKey(key, status, mousePos);

            return false;
        }

        public override void OnMouseMove(Vector2 mousePos)
        {
            if (_selectionHeld)
            {
                var newPosIdx = GetSelectionIndexUnderCursor(mousePos);
                if (newPosIdx != -1)
                {
                    _selectionEnd = newPosIdx;
                }
            }

            base.OnMouseMove(mousePos);
        }

        public override void InputFocusChanged(bool haveFocus)
        {
            _blinkingTimer.Restart();
            _cursorOn = true;

            if (!haveFocus && SubmitOnFocusLoss)
            {
                OnSubmit?.Invoke(Text);
            }

            if (haveFocus && Text != null)
            {
                _selectionStart = Text.Length;
                _selectionEnd = _selectionStart;
            }
            EnsureSelectionRight();
        }

        private void TextInputEventHandler(char c)
        {
            bool focused = Controller?.InputFocus == this;
            if (focused)
            {
                if (c == '\r') c = '\n';

                EnsureSelectionRight();
                int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
                int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);

                ReadOnlySpan<char> leftOfSelection = Text.AsSpan(0, smallerSelIdx);
                ReadOnlySpan<char> selection = Text.AsSpan(smallerSelIdx, largerSelIdx - smallerSelIdx);
                ReadOnlySpan<char> rightOfSelection = Text.AsSpan(largerSelIdx, Text.Length - largerSelIdx);

                switch (c)
                {
                    case '\b':
                        {
                            StringBuilder b = new StringBuilder();
                            // Only delete the character left of selection if nothing is selected.
                            // Otherwise we are deleting the selection itself.
                            if (selection.Length == 0 && leftOfSelection.Length != 0)
                                leftOfSelection = leftOfSelection.Slice(0, leftOfSelection.Length - 1);
                            b.Append(leftOfSelection);
                            b.Append(rightOfSelection);
                            Text = b.ToString();

                            _selectionEnd = leftOfSelection.Length;
                            _selectionStart = _selectionEnd;
                            EnsureSelectionRight();

                            TextChanged();
                            break;
                        }
                    default:
                        {
                            if (CanAddCharacter(c))
                            {
                                StringBuilder b = new StringBuilder();
                                b.Append(leftOfSelection);
                                b.Append(c);
                                b.Append(rightOfSelection);
                                Text = b.ToString();

                                _selectionEnd = leftOfSelection.Length + 1;
                                _selectionStart = _selectionEnd;
                                EnsureSelectionRight();

                                TextChanged();
                            }
                            break;
                        }
                }
            }
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
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
            if (focused && _layouter != null)
            {
                _layouter.RestartPen();

                Vector2 selectionStartPos = Vector2.Zero;
                Vector2 selectionEndPos = Vector2.Zero;

                int idx = 0;
                for (int i = 0; i < Text.Length + 1; i++)
                {
                    Vector2 gPos;
                    float glyphWidth = 0;
                    if (i < Text.Length)
                    {
                        char ch = Text[i];
                        gPos = _layouter.AddLetter(ch, out DrawableGlyph g);
                    }
                    else
                    {
                        gPos = _layouter.GetPenLocation();
                    }

                    //if (g == null || g.GlyphUV == Rectangle.Empty) continue;

                    if (_cursorOn && idx == _selectionEnd)
                    {
                        Vector2 cursorDrawStart = gPos;
                        var top = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y, Z);
                        var bottom = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y + _atlas.FontHeight, Z);

                        c.RenderLine(top, bottom, _calculatedColor);
                    }

                    if (idx == _selectionStart) selectionStartPos = gPos;
                    if (idx == _selectionEnd) selectionEndPos = gPos;

                    idx++;
                }

                if (selectionStartPos != Vector2.Zero && selectionEndPos != Vector2.Zero)
                {
                    c.RenderSprite(
                        new Vector3(X + selectionStartPos.X, Y + selectionStartPos.Y, Z - 1),
                        new Vector2(selectionEndPos.X - selectionStartPos.X, _atlas.FontHeight), Color.PrettyBlue * 0.3f);
                }
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

        #region Selection

        private void EnsureSelectionRight()
        {
            if (Text == null)
            {
                _selectionStart = 0;
                _selectionEnd = 0;
                return;
            }

            if (_selectionStart < 0) _selectionStart = 0;
            if (_selectionEnd < 0) _selectionEnd = 0;

            if (_selectionStart > Text.Length) _selectionStart = Text.Length;
            if (_selectionEnd > Text.Length) _selectionEnd = _selectionStart;
        }

        private int GetSelectionIndexUnderCursor(Vector2 mousePos)
        {
            int closestSepIdx = -1;
            float closestSepDist = float.MaxValue;

            _layouter.RestartPen();

            int idx = 0;
            for (int i = 0; i < Text.Length + 1; i++)
            {
                Vector2 gPos;
                if (i < Text.Length)
                {
                    char ch = Text[i];
                    gPos = _layouter.AddLetter(ch, out DrawableGlyph g);
                }
                else
                {
                    gPos = _layouter.GetPenLocation();
                }

                float dist = Vector2.Distance(Position2 + gPos, mousePos);
                if (dist < closestSepDist)
                {
                    closestSepDist = dist;
                    closestSepIdx = idx;
                }

                idx++;
            }

            return closestSepIdx;
        }

        private ReadOnlySpan<char> GetSelectedText()
        {
            int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
            int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);
            return Text.AsSpan(smallerSelIdx, largerSelIdx - smallerSelIdx);
        }

        #endregion
    }
}