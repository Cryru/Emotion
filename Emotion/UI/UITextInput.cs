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
                if (_selectionEnd != _selectionStart)
                {
                    int smaller = Math.Min(_selectionEnd, _selectionStart);
                    _selectionEnd = smaller;
                    _selectionStart = smaller;
                }
                else
                {
                    _selectionEnd--;
                    EnsureSelectionRight();
                    _selectionStart = _selectionEnd;
                }
                ResetBlinkingCursor();
            }

            if (key == Key.RightArrow && status == KeyStatus.Down)
            {
                if (_selectionEnd != _selectionStart)
                {
                    int larger = Math.Max(_selectionEnd, _selectionStart);
                    _selectionEnd = larger;
                    _selectionStart = larger;
                }
                else
                {
                    _selectionEnd++;
                    EnsureSelectionRight();
                    _selectionStart = _selectionEnd;
                }
                ResetBlinkingCursor();
            }

            if (key == Key.UpArrow && status == KeyStatus.Down)
            {
                var posOfCurrent = GetPositonOfSelection(_selectionStart);
                posOfCurrent.Y -= _atlas.FontHeight / 2;
                var closestSepIdx = GetSelectionIndexUnderCursor(posOfCurrent);
                if (closestSepIdx != -1)
                {
                    _selectionStart = closestSepIdx;
                    _selectionEnd = _selectionStart;
                    EnsureSelectionRight();
                }
                ResetBlinkingCursor();
            }

            if (key == Key.DownArrow && status == KeyStatus.Down)
            {
                var posOfCurrent = GetPositonOfSelection(_selectionStart);
                posOfCurrent.Y += _atlas.FontHeight * 2;
                var closestSepIdx = GetSelectionIndexUnderCursor(posOfCurrent);
                if (closestSepIdx != -1)
                {
                    _selectionStart = closestSepIdx;
                    _selectionEnd = _selectionStart;
                    EnsureSelectionRight();
                }
                ResetBlinkingCursor();
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
                        Engine.Host.SetClipboard(new string(selection));
                    }

                    return false;
                }
                else if (key == Key.V) // Paste
                {
                    var clipboardStr = Engine.Host.GetClipboard();
                    if (!string.IsNullOrEmpty(clipboardStr))
                    {
                        InsertString(clipboardStr);
                    }

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

        protected void InsertString(string str)
        {
            EnsureSelectionRight();
            int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
            int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);

            ReadOnlySpan<char> leftOfSelection = Text.AsSpan(0, smallerSelIdx);
            ReadOnlySpan<char> rightOfSelection = Text.AsSpan(largerSelIdx, Text.Length - largerSelIdx);

            StringBuilder b = new StringBuilder();
            b.Append(leftOfSelection);

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (CanAddCharacter(c))
                {
                    b.Append(c);
                }
            }

            b.Append(rightOfSelection);
            Text = b.ToString();

            _selectionEnd = leftOfSelection.Length + 1;
            _selectionStart = _selectionEnd;
            EnsureSelectionRight();

            TextChanged();
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

            // Maybe recalculate selection box only when text/selection changes?
            // On the other hand only one can be focused at a time soooo
            bool focused = Controller?.InputFocus == this;
            if (focused && _layouter != null)
            {
                _layouter.RestartPen();

                List<Rectangle> selectionRects = new List<Rectangle>(1); // maybe cache this per text+selection change?

                int idx = 0;
                for (int i = 0; i < Text.Length + 1; i++)
                {
                    Vector2 gPos;
                    Vector2 gPosPreWrap = Vector2.Zero;
                    if (i < Text.Length)
                    {
                        char ch = Text[i];

                        bool willWrapNext = _layouter.IsNextCharacterGoingToWrap();
                        if (willWrapNext)
                        {
                            gPosPreWrap = _layouter.GetNextGlyphPosition(_layouter.GetPenLocation(), ch, out Vector2 _, out DrawableGlyph _);
                        }

                        gPos = _layouter.AddLetter(ch, out DrawableGlyph _);
                    }
                    else
                    {
                        gPos = _layouter.GetPenLocation();
                    }

                    if (_cursorOn && idx == _selectionEnd)
                    {
                        Vector2 cursorDrawStart = gPos;

                        // Doesn't cover all cases, but good enough.
                        if (gPosPreWrap != Vector2.Zero && _selectionEnd != _selectionStart)
                        {
                            cursorDrawStart = gPosPreWrap;
                        }

                        var top = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y, Z);
                        var bottom = new Vector3(X + cursorDrawStart.X, Y + cursorDrawStart.Y + _atlas.FontHeight, Z);

                        c.RenderLine(top, bottom, _calculatedColor);
                    }

                    // Fill selection boxes for every line.
                    if (_selectionStart != _selectionEnd && 
                        ((idx >= _selectionStart && idx <= _selectionEnd) || (idx >= _selectionEnd && idx <= _selectionStart)))
                    {
                        var currentPosRect = new Rectangle(gPos.X, gPos.Y, 0, _atlas.FontHeight);
                        if (selectionRects.Count == 0)
                        {
                            selectionRects.Add(currentPosRect);
                        }
                        else
                        {
                            if (gPosPreWrap != Vector2.Zero)
                            {
                                var altGlyphPosRect = new Rectangle(gPosPreWrap.X, gPosPreWrap.Y, 0, _atlas.FontHeight);
                                selectionRects[^1] = Rectangle.Union(selectionRects[^1], altGlyphPosRect);
                            }

                            // Check if new line
                            var currentRect = selectionRects[^1];
                            if (gPos.Y != currentRect.Y)
                                selectionRects.Add(currentPosRect);
                            else
                                selectionRects[^1] = Rectangle.Union(currentRect, currentPosRect);
                        }
                    }

                    idx++;
                }

                for (int i = 0; i < selectionRects.Count; i++)
                {
                    var rect = selectionRects[i];
                    c.RenderSprite(
                       new Vector2(X + rect.X, Y + rect.Y).IntCastRound().ToVec3(Z - 1),
                       rect.Size, Color.PrettyBlue * 0.3f);
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
                Vector2 gPosPreWrap = Vector2.Zero;
                if (i < Text.Length)
                {
                    char ch = Text[i];

                    bool willWrapNext = _layouter.IsNextCharacterGoingToWrap();
                    if (willWrapNext)
                    {
                        gPosPreWrap = _layouter.GetNextGlyphPosition(_layouter.GetPenLocation(), ch, out Vector2 _, out DrawableGlyph _);
                    }

                    gPos = _layouter.AddLetter(ch, out DrawableGlyph g);
                }
                else
                {
                    gPos = _layouter.GetPenLocation();
                }

                gPos.Y += _atlas.FontHeight / 2f;

                float dist = Vector2.Distance(Position2 + gPos, mousePos);
                if (dist < closestSepDist)
                {
                    closestSepDist = dist;
                    closestSepIdx = idx;
                }

                // Also check the cursor position of the last character on the previous line
                // prior to it wrapping for this line.
                if (gPosPreWrap != Vector2.Zero)
                {
                    gPosPreWrap.Y += _atlas.FontHeight / 2f;
                    dist = Vector2.Distance(Position2 + gPosPreWrap, mousePos);
                    if (dist < closestSepDist)
                    {
                        closestSepDist = dist;
                        closestSepIdx = idx;
                    }
                }

                idx++;
            }

            return closestSepIdx;
        }

        private Vector2 GetPositonOfSelection(int selId)
        {
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

                if (selId == idx)
                {
                    return Position2 + gPos;
                }

                idx++;
            }

            return Vector2.Zero;
        }

        private ReadOnlySpan<char> GetSelectedText()
        {
            int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
            int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);
            return Text.AsSpan(smallerSelIdx, largerSelIdx - smallerSelIdx);
        }

        private void ResetBlinkingCursor()
        {
            _cursorOn = true;
            _blinkingTimer.Restart();
        }

        #endregion
    }
}