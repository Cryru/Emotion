#nullable enable

namespace Emotion.Standard.OptimizedStringReadWrite;

public interface IStringReader
{
    public ReadOnlySpan<byte> GetDataFromCurrentPosition();
}
