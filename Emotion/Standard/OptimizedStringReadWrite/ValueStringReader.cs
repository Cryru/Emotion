using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emotion.Standard.OptimizedStringReadWrite;

public ref struct ValueStringReader
{
    public StringType Type;

    private ReadOnlySpan<byte> _dataUTF8;
    private ReadOnlySpan<char> _dataUTF16;
    private int _position;

    public ValueStringReader(ReadOnlySpan<char> utf16)
    {
        _dataUTF16 = utf16;
        Type = StringType.DefaultUTF16;
    }

    public ValueStringReader(ReadOnlySpan<byte> utf8)
    {
        _dataUTF8 = utf8;
        Type = StringType.UTF8;
    }

    public ValueStringReader Slice(int offset, int length)
    {
        ValueStringReader slice;
        if (Type == StringType.DefaultUTF16)
            slice = new ValueStringReader(_dataUTF16.Slice(offset, length));
        else // if (Type == StringType.UTF8)
            slice = new ValueStringReader(_dataUTF8.Slice(offset, length));

        return slice;
    }

    public ReadOnlySpan<byte> GetDataFromCurrentPosition()
    {
        return _dataUTF8.Slice(_position);
    }
}
