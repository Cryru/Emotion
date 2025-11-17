#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;

namespace Emotion.Standard.DataStructures.OptimizedStringReadWrite;

public ref struct ValueStringReader
{
    public StringType Type;

    private ReadOnlySpan<byte> _dataUTF8;
    private int _utf8ByteOffset;

    private ReadOnlySpan<char> _dataUTF16;
    private int _position;
    private int _charsTotal;

    public ValueStringReader(ReadOnlySpan<char> utf16)
    {
        _dataUTF16 = utf16;
        _charsTotal = utf16.Length;
        Type = StringType.DefaultUTF16;
    }

    public ValueStringReader(ReadOnlySpan<byte> utf8)
    {
        _dataUTF8 = utf8;
        _charsTotal = Encoding.UTF8.GetCharCount(utf8);
        Type = StringType.UTF8;
    }

    public void SwitchModeToUTF8()
    {
        if (Type == StringType.UTF8)
            return;

        Assert(Type == StringType.DefaultUTF16);
        _dataUTF8 = MemoryMarshal.Cast<char, byte>(_dataUTF16);
        _charsTotal = Encoding.UTF8.GetCharCount(_dataUTF8);
        Type = StringType.UTF8;

        _dataUTF16 = ReadOnlySpan<char>.Empty;
    }

    public void SwitchModeToUTF16()
    {
        Assert(Type == StringType.UTF8);
        _dataUTF16 = MemoryMarshal.Cast<byte, char>(_dataUTF8);
        _charsTotal = _dataUTF16.Length;
        Type = StringType.DefaultUTF16;

        _dataUTF8 = ReadOnlySpan<byte>.Empty;
    }

    public char ReadNextChar()
    {
        if (Type == StringType.DefaultUTF16)
        {
            if (_position >= _dataUTF16.Length) return '\0';
            char c = _dataUTF16[_position];
            _position++;
            return c;
        }

        // if (Type == StringType.UTF8)
        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        char curChar = '\0';
        utf8Decoder.Convert(_dataUTF8.Slice(_utf8ByteOffset), new Span<char>(ref curChar), true, out int bytesUsed, out int charsRead, out bool bufferFinished);
        _utf8ByteOffset += bytesUsed;
        _position++;
        return curChar;
    }

    public char PeekNextChar()
    {
        if (Type == StringType.DefaultUTF16)
        {
            if (_position + 1 >= _dataUTF16.Length) return '\0';
            return _dataUTF16[_position + 1];
        }

        // if (Type == StringType.UTF8)
        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        Span<char> chars = stackalloc char[2]; // current char and next char
        utf8Decoder.Convert(_dataUTF8.Slice(_utf8ByteOffset), chars, true, out int bytesUsed, out int charsRead, out bool bufferFinished);
        return chars[1];
    }

    public char PeekCurrentChar()
    {
        if (Type == StringType.DefaultUTF16)
        {
            if (_position >= _dataUTF16.Length) return '\0';
            return _dataUTF16[_position];
        }

        // if (Type == StringType.UTF8)
        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        Span<char> chars = stackalloc char[1]; // current char
        utf8Decoder.Convert(_dataUTF8.Slice(_utf8ByteOffset), chars, true, out int bytesUsed, out int charsRead, out bool bufferFinished);
        return chars[0];
    }

    public int ReadToNextOccuranceofChar(char c, Span<char> readChars)
    {
        char nextChar = '\0';
        int charactersWritten = 0;

        if (Type == StringType.DefaultUTF16)
        {
            while (_position < _charsTotal)
            {
                nextChar = _dataUTF16[_position];
                if (nextChar == c)
                    return charactersWritten;

                _position++;

                readChars[charactersWritten] = nextChar;
                charactersWritten++;

                if (charactersWritten >= readChars.Length)
                    return charactersWritten;
            }
            return 0;
        }

        Span<char> readSpan = new Span<char>(ref nextChar);

        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        while (_utf8ByteOffset < _dataUTF8.Length)
        {
            utf8Decoder.Convert(_dataUTF8.Slice(_utf8ByteOffset), readSpan, false, out int bytesUsed, out int charsRead, out bool bufferFinished);
            if (nextChar == c)
                return charactersWritten;

            _utf8ByteOffset += bytesUsed;
            _position++;

            readChars[charactersWritten] = nextChar;
            charactersWritten++;

            if (charactersWritten >= readChars.Length)
                return charactersWritten;

            if (bufferFinished || bytesUsed == 0)
                break;
        }

        return 0;
    }

    public char MoveCursorToNextOccuranceOfNotWhitespace()
    {
        char nextChar = '\0';
        if (Type == StringType.DefaultUTF16)
        {
            while (_position < _charsTotal)
            {
                nextChar = _dataUTF16[_position];
                if (!char.IsWhiteSpace(nextChar))
                    return nextChar;

                _position++;
            }
            return '\0';
        }

        // if (Type == StringType.UTF8)
        Span<char> readSpan = new Span<char>(ref nextChar);

        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        while (_utf8ByteOffset < _dataUTF8.Length)
        {
            utf8Decoder.Convert(_dataUTF8.Slice(_utf8ByteOffset), readSpan, false, out int bytesUsed, out int charsRead, out bool bufferFinished);
            if (!char.IsWhiteSpace(nextChar))
                return nextChar;

            _utf8ByteOffset += bytesUsed;
            _position++;

            if (bufferFinished || bytesUsed == 0)
                break;
        }
            
        return '\0';
    }

    public bool MoveCursorToNextOccuranceOfChar(char c)
    {
        char nextChar = '\0';
        if (Type == StringType.DefaultUTF16)
        {
            while (_position < _charsTotal)
            {
                nextChar = _dataUTF16[_position];
                if (nextChar == c)
                    return true;

                _position++;
            }
            return false;
        }

        // if (Type == StringType.UTF8)
        Span<char> readSpan = new Span<char>(ref nextChar);

        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        while (_utf8ByteOffset < _dataUTF8.Length)
        {
            utf8Decoder.Convert(_dataUTF8.Slice(_utf8ByteOffset), readSpan, false, out int bytesUsed, out int charsRead, out bool bufferFinished);
            if (nextChar == c)
                return true;

            _utf8ByteOffset += bytesUsed;
            _position++;

            if (bufferFinished || bytesUsed == 0)
                break;
        }

        return false;
    }

    #region Specializations

    public int ReadXMLTagIfNotClosing(Span<char> memory)
    {
        // Find start of tag
        if (!MoveCursorToNextOccuranceOfChar('<')) return 0;

        char nextChar = PeekNextChar();
        if (nextChar == '/') return 0; // Closing!

        // Read opening bracket
        {
            char c = ReadNextChar();
            if (c != '<') return 0;
        }

        // Read content
        int charsWritten = ReadToNextOccuranceofChar('>', memory);
        if (charsWritten == 0) return 0;

        // Close tag
        {
            char c = ReadNextChar();
            if (c != '>') return 0;
        }

        return charsWritten;
    }

    public int ReadXMLTag(Span<char> memory, out bool closing)
    {
        closing = false;

        // Find start of tag
        if (!MoveCursorToNextOccuranceOfChar('<')) return 0;

        // Read opening bracket
        {
            char c = ReadNextChar();
            if (c != '<') return 0;
        }

        closing = PeekCurrentChar() == '/';

        // Read content
        int charsWritten = ReadToNextOccuranceofChar('>', memory);
        if (charsWritten == 0) return 0;

        // Close tag
        {
            char c = ReadNextChar();
            if (c != '>') return 0;
        }

        // todo: attributes, self closing tag flag, etc.

        return charsWritten;
    }

    #endregion
}
