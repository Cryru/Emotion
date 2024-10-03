#nullable enable

namespace Emotion.Standard.ByteReadWrite;

public interface IStringReader
{
    public ReadOnlySpan<byte> GetDataFromCurrentPosition();
}
