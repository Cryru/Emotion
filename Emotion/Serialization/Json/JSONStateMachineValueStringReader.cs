#if PROTOTYPE
#nullable enable

namespace Emotion.Standard.OptimizedStringReadWrite;

/// <summary>
/// This is a branch of the ValueStringReader for this proof of concept.
/// It only supports UTF16
/// </summary>
public ref struct JSONStateMachineValueStringReader
{
    private ReadOnlySpan<char> _dataUTF16;
    private int _position;
    private int _charsTotal;

    public JSONStateMachineValueStringReader(ReadOnlySpan<char> utf16)
    {
        _dataUTF16 = utf16;
    }

    public int GetCurrentPosition()
    {
        return _position;
    }

    public void AdvancePosition(int p)
    {
        _position += p;
    }

    public void SetPosition(int pos)
    {
        _position = pos;
    }

    public SimpleSpanRange ReadToNextOccuranceOf(char c, bool moveCursorToAfterIt = false)
    {
        if (_position >= _charsTotal) return SimpleSpanRange.Invalid;

        ReadOnlySpan<char> afterPosition = _dataUTF16.Slice(_position);
        int indexOfNextSuchChar = afterPosition.IndexOf(c);
        if (indexOfNextSuchChar == -1)
        {
            _position = _charsTotal;
            return SimpleSpanRange.Invalid;
        }

        SimpleSpanRange returnVal = new(_position, indexOfNextSuchChar);

        _position += indexOfNextSuchChar;
        if (moveCursorToAfterIt) _position++;

        return returnVal;
    }

    public ReadOnlySpan<char> GetSpanSlice(SimpleSpanRange range)
    {
        return _dataUTF16.Slice(range.Start, range.Length);
    }

    public SimpleSpanRange ReadToNextOccuranceOfAnyOf(Span<char> chars, bool moveCursorToAfterIt = false)
    {
        if (_position >= _charsTotal) return SimpleSpanRange.Invalid;

        ReadOnlySpan<char> afterPosition = _dataUTF16.Slice(_position);
        int indexOfNextSuchChar = afterPosition.IndexOfAny(chars);
        if (indexOfNextSuchChar == -1)
        {
            _position = _charsTotal;
            return SimpleSpanRange.Invalid;
        }

        SimpleSpanRange returnVal = new(_position, indexOfNextSuchChar);

        _position += indexOfNextSuchChar;
        if (moveCursorToAfterIt) _position++;

        return returnVal;
    }

    /// <summary>
    /// Moves the cursor to the next character that isn't any of the provided characters.
    /// Leaves the cursor at that character (or if the provided argument is true, to the character after it)
    /// </summary>
    public char ReadToNextOccuranceOfAnyExcept(Span<char> anyExcept, bool moveCursorToAfterIt = false)
    {
        if (_position >= _charsTotal) return '\0';

        ReadOnlySpan<char> afterPosition = _dataUTF16.Slice(_position);
        int indexOfNextSuchChar = afterPosition.IndexOfAnyExcept(anyExcept);
        if (indexOfNextSuchChar == -1)
        {
            _position = _charsTotal;
            return '\0';
        }

        _position += indexOfNextSuchChar;
        if (moveCursorToAfterIt) _position++;

        return afterPosition[indexOfNextSuchChar];
    }

    /// <summary>
    /// Reads the next string of characters that are between two quotation marks (").
    /// Leaves the cursor at the character after the closing quotation mark.
    /// If the readChars array is not enough to accomodate the full string - the cursor will still be moved.
    /// </summary>
    public unsafe SimpleSpanRange ReadNextQuotedString(bool moveCursorToAfterIt = false)
    {
        const char STRING_OPEN_CLOSE = '\"';

        if (_position >= _charsTotal) return SimpleSpanRange.Invalid;

        ReadOnlySpan<char> afterPosition = _dataUTF16.Slice(_position);
        int nextOpen = afterPosition.IndexOf(STRING_OPEN_CLOSE);
        if (nextOpen == -1 || nextOpen == _charsTotal)
        {
            _position = _charsTotal;
            return SimpleSpanRange.Invalid;
        }

        ReadOnlySpan<char> afterOpen = afterPosition.Slice(nextOpen + 1);
        int nextClose = afterOpen.IndexOf(STRING_OPEN_CLOSE);
        int stringLength = nextClose;

        var result = new SimpleSpanRange(_position + nextOpen + 1, stringLength);

        _position += stringLength + nextOpen + 1;
        if (moveCursorToAfterIt) _position++;

        return result;
    }
}
#endif