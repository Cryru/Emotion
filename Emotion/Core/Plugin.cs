#nullable enable

namespace Emotion.Core;

public interface IPlugin : IDisposable
{
    void Initialize();
}