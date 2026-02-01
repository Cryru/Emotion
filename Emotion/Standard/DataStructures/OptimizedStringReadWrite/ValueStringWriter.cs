#nullable enable

using System.Globalization;
using System.Text;

namespace Emotion.Standard.DataStructures.OptimizedStringReadWrite;

public ref struct ValueStringWriter
{
    public StringType Type = StringType.DefaultUTF16;
    public int BytesWritten = 0;
    public readonly int CharsWritten => Type == StringType.DefaultUTF16 ? BytesWritten / 2 : BytesWritten;

    private StringBuilder? _builder = null;
    private Span<byte> _dataUTF8 = Span<byte>.Empty;
    private Span<char> _dataUTF16 = Span<char>.Empty;

    private int _position = 0;

    public ValueStringWriter(Span<char> utf16)
    {
        _dataUTF16 = utf16;
        Type = StringType.DefaultUTF16;
    }

    public ValueStringWriter(Span<byte> utf8)
    {
        _dataUTF8 = utf8;
        Type = StringType.UTF8;
    }

    public ValueStringWriter(StringBuilder builder)
    {
        _builder = builder;
        Type = StringType.DefaultUTF16;
    }

    public ValueStringWriter()
    {
        throw new NotImplementedException();
    }

    public bool WriteString(ReadOnlySpan<char> value)
    {
        // Fast path for string builder.
        if (_builder != null)
        {
            _builder.Append(value);
            return true;
        }

        if (Type == StringType.DefaultUTF16)
        {
            int idx = _position;
            Span<char> span = _dataUTF16.Slice(idx);
            if (value.Length > span.Length) return false;

            value.CopyTo(span);

            _position += value.Length;
            BytesWritten += value.Length * 2;
            return true;
        }

        if (Type == StringType.UTF8)
        {
            Encoding encoding = Encoding.UTF8;
            Span<byte> dest = _dataUTF8.Slice(_position);
            if (dest.Length == 0) return false;

            bool success = encoding.TryGetBytes(value, dest, out int bytesWritten);
            if (!success) return false;

            _position += bytesWritten;
            BytesWritten += bytesWritten;
            return true;
        }

        return true;
    }

    public bool WriteString(string value)
    {
        return WriteString(value.AsSpan());
    }

    public bool WriteChar(char value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!WriteChar(value)) return false;
        }
        return true;
    }

    public bool WriteChar(char value)
    {
        // Fast path for string builder.
        if (_builder != null)
        {
            _builder.Append(value);
            return true;
        }

        if (Type == StringType.DefaultUTF16)
        {
            if (_position == _dataUTF16.Length) return false;

            _dataUTF16[_position] = value;
            _position++;
            BytesWritten += 2;
            return true;
        }

        if (Type == StringType.UTF8)
        {
            Encoding encoding = Encoding.UTF8;
            Span<byte> dest = _dataUTF8.Slice(_position);
            if (dest.Length == 0) return false;

            ReadOnlySpan<char> charAsSpan = new ReadOnlySpan<char>(ref value);
            bool success = encoding.TryGetBytes(charAsSpan, dest, out int bytesWritten);
            if (!success) return false;

            _position += bytesWritten;
            BytesWritten += bytesWritten;
            return true;
        }

        return false;
    }

    public bool WriteNumber<TNumber>(TNumber number) where TNumber : INumber<TNumber>
    {
        // This can be optimized to not allocate this ToString()
        // but would require switching over all number types :P
        return WriteString(number.ToString() ?? string.Empty);
    }

    public bool WriteNumber<TNumber>(TNumber number, string format) where TNumber : INumber<TNumber>
    {
        if (Type != StringType.DefaultUTF16) return false;

        int idx = _position;
        Span<char> span = _dataUTF16.Slice(idx);
        if (number.TryFormat(span, out int written, format, CultureInfo.InvariantCulture))
        {
            _position += written;
            BytesWritten += written * 2;
        }
        return true;
    }

    #region Indent

    private int _indent;
    private int _indentSize = 4;

    public void PushIndent()
    {
        _indent += 1;
    }

    public void PopIndent()
    {
        _indent -= 1;
    }

    public bool WriteIndent()
    {
        return WriteChar(' ', _indent * _indentSize);
    }

    public void SetIndentSize(int size)
    {
        _indentSize = size;
    }

    #endregion

    #region Specialization

    public enum XMLTagType
    {
        Normal,
        Closing,
        SelfClosing
    }

    public bool WriteXMLTag(string tagContent, XMLTagType tagType)
    {
        if (!WriteChar('<')) return false;

        if (tagType == XMLTagType.Closing)
            if (!WriteChar('/')) return false;

        if (!WriteString(tagContent)) return false;

        if (tagType == XMLTagType.SelfClosing)
            if (!WriteChar('/')) return false;

        if (!WriteChar('>')) return false;

        return true;
    }

    #endregion
}
