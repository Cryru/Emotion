#region Using

using System.Text;
using Emotion.Common.Serialization;
using Emotion.Game.Time;
using Emotion.Graphics.Text;
using Emotion.Platform.Input;

#endregion

#nullable enable

namespace Emotion.UI;

public class UITextInput2 : UIRichText
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

    public UITextInput2()
    {
        HandleInput = true;
        _blinkingTimer = new Every(650, () => { _cursorOn = !_cursorOn; });
        _arrowHeldTimer = new Every(50, ArrowHeldProc);
        _layoutEngine.ResolveTags = false;
        WrapText = false;
        UseNewLayoutSystem = true;

        FillX = true;
        FillY = true;
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
        if (SubmitOnEnter && key == Key.Enter && status == KeyStatus.Down && !MultiLine)
        {
            OnSubmit?.Invoke(_text);
            return false;
        }

        if (key == Key.LeftArrow || key == Key.RightArrow || key == Key.UpArrow || key == Key.DownArrow)
        {
            if (status == KeyStatus.Down)
            {
                _arrowHeld = key;
            }
            else if(status == KeyStatus.Up && _arrowHeld == key)
            {
                _arrowHeld = null;
            }
            _arrowHeldTimer.Restart();
            return false;
        }

        // Selection drag
        if (key == Key.MouseKeyLeft)
        {
            if (status == KeyStatus.Down)
            {
                _selectionHeld = true;

                int closestSepIdx = GetSelectionIndexUnderCursor(mousePos);
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

            return false;
        }

        if (status == KeyStatus.Down && Engine.Host.IsCtrlModifierHeld())
        {
            if (key == Key.A) // Select All
            {
                _selectionStart = 0;
                _selectionEnd = _layoutEngine.GetSelectionIndexMax();
                EnsureSelectionRight();

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

        if (key == Key.Delete && status == KeyStatus.Down)
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

        EnsureSelectionRight();
    }

    private void TextInputEventHandler(char c)
    {
        bool focused = Controller?.InputFocus == this;
        if (!focused) return;

        if (c == '\r') c = '\n';

        EnsureSelectionRight();

        int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
        int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);

        int smallerTextIdx = _layoutEngine.GetStringIndexFromSelectionIndex(smallerSelIdx);
        int largerTextIdx = _layoutEngine.GetStringIndexFromSelectionIndex(largerSelIdx);

        ReadOnlySpan<char> leftOfSelection = _text.AsSpan(0, smallerTextIdx);
        ReadOnlySpan<char> selection = _text.AsSpan(smallerTextIdx, largerTextIdx - smallerTextIdx);
        ReadOnlySpan<char> rightOfSelection = _text.AsSpan(largerTextIdx, _text.Length - largerTextIdx);

        switch (c)
        {
            case '\u007F':
            case '\b':
                {
                    var b = new StringBuilder();

                    int oldStringIndex = _layoutEngine.GetStringIndexFromSelectionIndex(largerSelIdx);

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
                    int newSelectionIndex = _layoutEngine.GetSelectionIndexFromStringIndex(oldStringIndex);

                    _selectionEnd = newSelectionIndex;
                    _selectionStart = _selectionEnd;
                    EnsureSelectionRight();
                    break;
                }
            default:
                {
                    if (CanAddCharacter(c, _text.Length))
                    {
                        int oldStringIndex = _layoutEngine.GetStringIndexFromSelectionIndex(smallerSelIdx);

                        var b = new StringBuilder();
                        b.Append(leftOfSelection);
                        b.Append(c);
                        b.Append(rightOfSelection);
                        Text = b.ToString();
                        UpdateText();

                        oldStringIndex++;
                        int newSelectionIndex = _layoutEngine.GetSelectionIndexFromStringIndex(oldStringIndex);

                        _selectionEnd = newSelectionIndex;
                        _selectionStart = _selectionEnd;
                        EnsureSelectionRight();
                    }

                    break;
                }
        }
    }

    protected void InsertString(string str)
    {
        EnsureSelectionRight();

        int smallerSelIdx = Math.Min(_selectionStart, _selectionEnd);
        int largerSelIdx = Math.Max(_selectionStart, _selectionEnd);

        int oldStringIndex = _layoutEngine.GetStringIndexFromSelectionIndex(smallerSelIdx);

        int smallerTextIdx = _layoutEngine.GetStringIndexFromSelectionIndex(smallerSelIdx);
        int largerTextIdx = _layoutEngine.GetStringIndexFromSelectionIndex(largerSelIdx);

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

        int newSelectionIndex = _layoutEngine.GetSelectionIndexFromStringIndex(oldStringIndex);
        _selectionEnd = newSelectionIndex;
        _selectionStart = _selectionEnd;
        EnsureSelectionRight();
    }

    protected override Vector2 InternalMeasure(Vector2 space)
    {
        Vector2 textSize = base.InternalMeasure(space);
        return new Vector2(space.X, MultiLine ? space.Y : textSize.Y);
    }

    protected override bool UpdateInternal()
    {
        _blinkingTimer.Update(Engine.DeltaTime);
        _arrowHeldTimer.Update(Engine.DeltaTime);
        return base.UpdateInternal();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.SetClipRect(Bounds);
        base.RenderInternal(c);
        c.SetClipRect(null);

        // Maybe recalculate selection box only when text/selection changes?
        // On the other hand only one can be focused at a time soooo
        bool focused = Controller?.InputFocus == this;
        if (focused && _layouter != null)
        {
            if (_cursorOn)
            {
                Rectangle cursorCharRect = _layoutEngine.GetBoundOfSelectionIndex(_selectionEnd);
                var top = new Vector3(cursorCharRect.X + X, cursorCharRect.Y + Y, Z);
                var bottom = new Vector3(cursorCharRect.X + X, cursorCharRect.Y + Y + cursorCharRect.Height, Z);
                c.RenderLine(top, bottom, _calculatedColor);
            }

            if (_selectionStart != _selectionEnd)
            {
                static void DrawSelectionRectangle(Rectangle lineBound, (Vector3 offset, RenderComposer composer) args)
                {
                    args.composer.RenderSprite(args.offset + lineBound.PositionZ(0), lineBound.Size, Color.PrettyBlue * 0.3f);
                }

                int selStart = _selectionStart;
                int selEnd = _selectionEnd;
                if (selEnd > selStart) selEnd--;
                else selStart--;

                _layoutEngine.ForEachLineBetweenSelectionIndices(selStart, selEnd, DrawSelectionRectangle, (Position, c));               
            }
        }

        return true;
    }

    protected virtual bool CanAddCharacter(char c, int stringLength)
    {
        if (MaxCharacters != -1 && stringLength >= MaxCharacters) return false;
        if (c == '\n' && !MultiLine) return false;

        return true;
    }

    protected void UpdateText()
    {
        _layoutEngine.InitializeLayout(Text ?? "", TextHeightMode);
        _layoutEngine.Run();
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
                EnsureSelectionRight();
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
                EnsureSelectionRight();
                _selectionStart = _selectionEnd;
            }

            ResetBlinkingCursor();
        }

        if (_arrowHeld == Key.UpArrow)
        {
            int closestSepIdx = _layoutEngine.GetSelectionIndexOnOtherLine(_selectionStart, -1);
            if (closestSepIdx != -1)
            {
                _selectionStart = closestSepIdx;
                _selectionEnd = _selectionStart;
                EnsureSelectionRight();
            }

            ResetBlinkingCursor();
        }

        if (_arrowHeld == Key.DownArrow)
        {
            int closestSepIdx = _layoutEngine.GetSelectionIndexOnOtherLine(_selectionStart, 1);
            if (closestSepIdx != -1)
            {
                _selectionStart = closestSepIdx;
                _selectionEnd = _selectionStart;
                EnsureSelectionRight();
            }

            ResetBlinkingCursor();
        }
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

        int selMax = _layoutEngine.GetSelectionIndexMax();
        if (_selectionStart > selMax) _selectionStart = selMax;
        if (_selectionEnd > selMax) _selectionEnd = _selectionStart;
    }

    private int GetSelectionIndexUnderCursor(Vector2 mousePos)
    {
        (int selIndex, int _, float __) = _layoutEngine.GetSelectionIndexFromPosition(mousePos - Position2);
        return selIndex;
    }

    private ReadOnlySpan<char> GetSelectedText()
    {
        int startTextIdx = _layoutEngine.GetStringIndexFromSelectionIndex(_selectionStart);
        int endTextIdx = _layoutEngine.GetStringIndexFromSelectionIndex(_selectionEnd);

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