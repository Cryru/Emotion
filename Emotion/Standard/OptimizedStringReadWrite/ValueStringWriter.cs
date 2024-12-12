#nullable enable

using System.Runtime.InteropServices;
using System.Text;

namespace Emotion.Standard.OptimizedStringReadWrite;

public ref struct ValueStringWriter
{
    public StringType Type = StringType.DefaultUTF16;
    public int BytesWritten = 0;

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

    public bool WriteString(string value)
    {
        // Fast path for string builder.
        if (_builder != null)
        {
            _builder.Append(value);
            return true;
        }

        System.Text.Encoding encoding;
        Span<byte> dest;

        if (Type == StringType.UTF8)
        {
            encoding = System.Text.Encoding.UTF8;
            dest = _dataUTF8.Slice(_position);
        }
        else // if (Type == StringType.UTF8)
        {
            encoding = System.Text.Encoding.Unicode;
            dest = MemoryMarshal.Cast<char, byte>(_dataUTF16).Slice(_position);
        }
        if (dest.Length == 0) return false;

        bool success = encoding.TryGetBytes(value, dest, out int bytesWritten);
        if (!success) return false;

        _position += bytesWritten;
        BytesWritten += bytesWritten;

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

        System.Text.Encoding encoding;
        Span<byte> dest;

        if (Type == StringType.UTF8)
        {
            encoding = System.Text.Encoding.UTF8;
            dest = _dataUTF8.Slice(_position);
        }
        else // if (Type == StringType.UTF8)
        {
            encoding = System.Text.Encoding.Unicode;
            dest = MemoryMarshal.Cast<char, byte>(_dataUTF16).Slice(_position);
        }
        if (dest.Length == 0) return false;

        ReadOnlySpan<char> charAsSpan = new ReadOnlySpan<char>(ref value);
        bool success = encoding.TryGetBytes(charAsSpan, dest, out int bytesWritten);
        if (!success) return false;

        _position += bytesWritten;
        BytesWritten += bytesWritten;

        return true;
    }

    public bool WriteNumber<TNumber>(TNumber number) where TNumber : INumber<TNumber>
    {
        // This can be optimized to not allocate this ToString()
        // but would require switching over all number types :P
        return WriteString(number.ToString() ?? string.Empty);
    }
}
