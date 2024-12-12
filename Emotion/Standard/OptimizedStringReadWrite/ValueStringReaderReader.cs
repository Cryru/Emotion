using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Standard.ByteReadWrite;

public enum StringType : byte
{
    DefaultUTF16,
    UTF8,
}

public ref struct ValueStringReaderReader
{
    public StringType Type;

    private ReadOnlySpan<byte> _dataUTF8;
    private ReadOnlySpan<char> _dataUTF16;
    private int _position;

    public ValueStringReaderReader(ReadOnlySpan<char> utf16)
    {
        _dataUTF16 = utf16;
        Type = StringType.DefaultUTF16;
    }

    public ValueStringReaderReader(ReadOnlySpan<byte> utf8)
    {
        _dataUTF8 = utf8;
        Type = StringType.UTF8;
    }

    public ValueStringReaderReader Slice(int offset, int length)
    {
        ValueStringReaderReader slice;
        if (Type == StringType.DefaultUTF16)
            slice = new ValueStringReaderReader(_dataUTF16.Slice(offset, length));
        else;// if (Type == StringType.UTF8)
            slice = new ValueStringReaderReader(_dataUTF8.Slice(offset, length));

        return slice;
    }

    public ReadOnlySpan<byte> GetDataFromCurrentPosition()
    {
        return _dataUTF8.Slice(_position);
    }
}
