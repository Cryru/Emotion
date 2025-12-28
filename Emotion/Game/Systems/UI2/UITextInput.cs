#nullable enable

#region Using

using Emotion;
using Emotion.Core.Systems.Input;
using Emotion.Core.Utility.Time;
using Emotion.Graphics.Text;
using System.Text;


#endregion

namespace Emotion.Game.Systems.UI2;

public class UITextInput : UIText
{
    public bool MultiLine = false;
    public int MaxCharacters = -1;

    public bool SubmitOnEnter = true;
    public bool SubmitOnFocusLoss;

    [DontSerialize] public Action<string>? OnSubmit;

    private bool _cursorOn;
    private Every _blinkingTimer;

    private int _selectionStart = -1;
    private int _selectionEnd = -1;
    private bool _selectionHeld;

    private Key? _arrowHeld;
    private Every _arrowHeldTimer;

    public UITextInput()
    {
        HandleInput = true;
        _blinkingTimer = new Every(650, () => { _cursorOn = !_cursorOn; });
        _arrowHeldTimer = new Every(50, ArrowHeldProc);
        WrapText = false;

        Layout.SizingX = UISizing.Grow();
        Layout.SizingY = UISizing.Grow();
    }

    protected override TextLayouter CreateTextLayouter()
    {
        return new TextLayouter(false);
    }

    protected override void OnOpen()
    {
        // We check focused so we can hook to the text input function that isnt blocked by anything.
        Engine.Host.OnTextInputAll += TextInputEventHandler;
        base.OnOpen();
    }

    protected override void OnClose()
    {
        Engine.Host.OnTextInputAll -= TextInputEventHandler;
        base.OnClose();
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (SubmitOnEnter && key == Key.Enter && status == KeyState.Down && !MultiLine)
        {
            OnSubmit?.Invoke(_text);
            return false;
        }

        if (key == Key.LeftArrow || key == Key.RightArrow || key == Key.UpArrow || key == Key.DownArrow)
        {
            if (status == KeyState.Down)
            {
                _arrowHeld = key;
            }
            else if(status == KeyState.Up && _arrowHeld == key)
            {
                _arrowHeld = null;
            }
            _arrowHeldTimer.Restart();
            return false;
        }

        // Selection drag
        if (key == Key.MouseKeyLeft)
        {
            if (status == KeyState.Down)
            {
                _selectionHeld = true;

                int closestSepIdx = GetSelectionIndexUnderCursor(mousePos);
                if (closestSepIdx != -1)
                {
                    _selectionStart = closestSepIdx;
                    _selectionEnd = _selectionStart;
                    EnsureSelectionValid();
                }
            }
            else if (status == KeyState.Up)
            {
                _selectionHeld = false;
            }

            return false;
        }

        if (status == KeyState.Down && Engine.Host.IsCtrlModifierHeld())
        {
            if (key == Key.A) // Select All
            {
                _selectionStart = 0;
                _selectionEnd = _layouter.GetSelectionIndexMax();
                EnsureSelectionValid();

                return false;
            }

            if (key == Key.C) // Copy
            {
                ReadOnlySpan<char> selection = GetSelectedText();
                if (selection.Length != 0) Engine.Host.SetClipboard(new string(selection));

                return false;
            }

            if (key == Key.V) // Paste
            {
                string? clipboardStr = Engine.Host.GetClipboard();
                if (!string.IsNullOrEmpty(clipboardStr)) InsertString(clipboardStr);

                return false;
            }

            if (key == Key.Z) // Undo
                return false;
            if (key == Key.Y) // Redo
                return false;
        }

        if (key == Key.Delete && status == KeyState.Down)
        {
            TextInputEventHandler((char) 127);
            return false;
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
            int newPosIdx = GetSelectionIndexUnderCursor(mousePos);
            if (newPosIdx != -1) _selectionEnd = newPosIdx;
        }

        base.OnMouseMove(mousePos);
    }

    public override void InputFocusChanged(bool haveFocus)
    {
        _blinkingTimer.Restart();
        _cursorOn = true;

        if (!haveFocus && SubmitOnFocusLoss) OnSubmit?.Invoke(_text);

        if (haveFocus && _text != null)
        {
            _selectionStart = _text.Length;
            _selectionEnd = _selectionStart;
        }

        EnsureSelectionValid();
    }

    private void TextInputEventHandler(char c)
    {
        bool focused = Engine.UI.InputFocus == this;
        if (!focused) return;

        if (c == '\r') c = '\n';

        EnsureSelectionValid();

        int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
        int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);

        int smallerTextIdx = _layouter.GetStringIndexFromSelectionIndex(smallerSelIdx);
        int largerTextIdx = _layouter.GetStringIndexFromSelectionIndex(largerSelIdx);

        ReadOnlySpan<char> leftOfSelection = _text.AsSpan(0, smallerTextIdx);
        ReadOnlySpan<char> selection = _text.AsSpan(smallerTextIdx, largerTextIdx - smallerTextIdx);
        ReadOnlySpan<char> rightOfSelection = _text.AsSpan(largerTextIdx, _text.Length - largerTextIdx);

        switch (c)
        {
            case '\u007F':
            case '\b':
                {
                    var b = new StringBuilder();

                    int oldStringIndex = _layouter.GetStringIndexFromSelectionIndex(largerSelIdx);

                    // Only delete the character left of selection if nothing is selected.
                    // Otherwise we are deleting the selection itself.
                    if (selection.Length == 0 && leftOfSelection.Length != 0)
                    {
                        oldStringIndex--;
                        leftOfSelection = _text.AsSpan(0, oldStringIndex);
                    }

                    b.Append(leftOfSelection);
                    b.Append(rightOfSelection);
                    Text = b.ToString();
                    UpdateText();

                    oldStringIndex -= selection.Length;
                    int newSelectionIndex = _layouter.GetSelectionIndexFromStringIndex(oldStringIndex);

                    _selectionEnd = newSelectionIndex;
                    _selectionStart = _selectionEnd;
                    EnsureSelectionValid();
                    break;
                }
            default:
                {
                    if (CanAddCharacter(c, _text.Length))
                    {
                        int oldStringIndex = _layouter.GetStringIndexFromSelectionIndex(smallerSelIdx);

                        var b = new StringBuilder();
                        b.Append(leftOfSelection);
                        b.Append(c);
                        b.Append(rightOfSelection);
                        Text = b.ToString();
                        UpdateText();

                        oldStringIndex++;
                        int newSelectionIndex = _layouter.GetSelectionIndexFromStringIndex(oldStringIndex);

                        _selectionEnd = newSelectionIndex;
                        _selectionStart = _selectionEnd;
                        EnsureSelectionValid();
                    }

                    break;
                }
        }
    }

    protected void InsertString(string str)
    {
        EnsureSelectionValid();

        int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
        int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);

        int oldStringIndex = _layouter.GetStringIndexFromSelectionIndex(smallerSelIdx);

        int smallerTextIdx = _layouter.GetStringIndexFromSelectionIndex(smallerSelIdx);
        int largerTextIdx = _layouter.GetStringIndexFromSelectionIndex(largerSelIdx);

        ReadOnlySpan<char> leftOfSelection = _text.AsSpan(0, smallerTextIdx);
        ReadOnlySpan<char> rightOfSelection = _text.AsSpan(largerTextIdx, _text.Length - largerTextIdx);

        var b = new StringBuilder();
        b.Append(leftOfSelection);

        for (var i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (CanAddCharacter(c, b.Length + rightOfSelection.Length))
            {
                b.Append(c);
                oldStringIndex++;
            }
        }

        b.Append(rightOfSelection);
        Text = b.ToString();
        UpdateText();

        int newSelectionIndex = _layouter.GetSelectionIndexFromStringIndex(oldStringIndex);
        _selectionEnd = newSelectionIndex;
        _selectionStart = _selectionEnd;
        EnsureSelectionValid();
    }

    protected override Vector2 InternalMeasure(Vector2 space)
    {
        Vector2 textSize = base.InternalMeasure(space);
        return textSize;// new Vector2(space.X, MultiLine ? space.Y : textSize.Y);
    }

    protected override bool UpdateInternal()
    {
        _blinkingTimer.Update(Engine.DeltaTime);
        _arrowHeldTimer.Update(Engine.DeltaTime);
        return base.UpdateInternal();
    }

    protected override void InternalRender(Renderer r)
    {
        Rectangle? prevClip = r.CurrentState.ClipRect;
        Rectangle clipRect = CalculatedMetrics.Bounds.ToRect().Offset(r.ModelMatrix.Translation.ToVec2());
        if (prevClip != null)
            clipRect = Rectangle.Clip(prevClip.Value, clipRect);

        r.SetClipRect(clipRect);
        base.InternalRender(r);
        r.SetClipRect(prevClip);

        // Maybe recalculate selection box only when text/selection changes?
        // On the other hand only one can be focused at a time soooo
        bool focused = Engine.UI.InputFocus == this;
        if (focused)
        {
            if (_cursorOn)
            {
                Rectangle cursorCharRect = _layouter.GetBoundOfSelectionIndex(_selectionEnd);

                var top = (cursorCharRect.TopLeft + CalculatedMetrics.Position.ToVec2()).ToVec3(Z);
                var bottom = (cursorCharRect.BottomLeft + CalculatedMetrics.Position.ToVec2()).ToVec3(Z);
                r.RenderLine(top, bottom, TextColor, 2);
            }

            if (_selectionStart != _selectionEnd)
            {
                static void DrawSelectionRectangle(Rectangle lineBound, (Vector3 offset, Renderer composer) args)
                {
                    args.composer.RenderSprite(args.offset + lineBound.PositionZ(0), lineBound.Size, Color.PrettyBlue * 0.3f);
                }

                int selStart = _selectionStart;
                int selEnd = _selectionEnd;
                if (selEnd > selStart) selEnd--;
                else selStart--;

                _layouter.ForEachLineBetweenSelectionIndices(selStart, selEnd, DrawSelectionRectangle, (CalculatedMetrics.Position.ToVec3(), r));
            }
        }
    }

    protected virtual bool CanAddCharacter(char c, int stringLength)
    {
        if (MaxCharacters != -1 && stringLength >= MaxCharacters) return false;
        if (c == '\n' && !MultiLine) return false;

        return true;
    }

    protected void UpdateText()
    {
        ReRunLayout();
        OnTextChanged();
    }

    protected virtual void OnTextChanged()
    {
    }

    private void ArrowHeldProc()
    {
        if (_arrowHeld == Key.LeftArrow)
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
                EnsureSelectionValid();
                _selectionStart = _selectionEnd;
            }

            ResetBlinkingCursor();
        }

        if (_arrowHeld == Key.RightArrow)
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
                EnsureSelectionValid();
                _selectionStart = _selectionEnd;
            }

            ResetBlinkingCursor();
        }

        if (_arrowHeld == Key.UpArrow)
        {
            int closestSepIdx = _layouter.GetSelectionIndexOnOtherLine(_selectionStart, -1);
            if (closestSepIdx != -1)
            {
                _selectionStart = closestSepIdx;
                _selectionEnd = _selectionStart;
                EnsureSelectionValid();
            }

            ResetBlinkingCursor();
        }

        if (_arrowHeld == Key.DownArrow)
        {
            int closestSepIdx = _layouter.GetSelectionIndexOnOtherLine(_selectionStart, 1);
            if (closestSepIdx != -1)
            {
                _selectionStart = closestSepIdx;
                _selectionEnd = _selectionStart;
                EnsureSelectionValid();
            }

            ResetBlinkingCursor();
        }
    }

    #region Selection

    private void EnsureSelectionValid()
    {
        if (Text == null)
        {
            _selectionStart = 0;
            _selectionEnd = 0;
            return;
        }

        if (_selectionStart < 0) _selectionStart = 0;
        if (_selectionEnd < 0) _selectionEnd = 0;

        int selMax = _layouter.GetSelectionIndexMax();
        if (_selectionStart > selMax) _selectionStart = selMax;
        if (_selectionEnd > selMax) _selectionEnd = _selectionStart;
    }

    private int GetSelectionIndexUnderCursor(Vector2 mousePos)
    {
        (int selIndex, int _, float __) = _layouter.GetSelectionIndexFromPosition(mousePos - CalculatedMetrics.Position.ToVec2());
        return selIndex;
    }

    private ReadOnlySpan<char> GetSelectedText()
    {
        int startTextIdx = _layouter.GetStringIndexFromSelectionIndex(_selectionStart);
        int endTextIdx = _layouter.GetStringIndexFromSelectionIndex(_selectionEnd);

        int smallerSelIdx = Math.Min(startTextIdx, endTextIdx);
        int largerSelIdx = Math.Max(startTextIdx, endTextIdx);
        return Text.AsSpan(smallerSelIdx, largerSelIdx - smallerSelIdx);
    }

    private void ResetBlinkingCursor()
    {
        _cursorOn = true;
        _blinkingTimer.Restart();
    }

    #endregion
}