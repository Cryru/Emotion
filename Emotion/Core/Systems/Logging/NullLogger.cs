#nullable enable

namespace Emotion.Core.Systems.Logging;

/// <summary>
/// Doesn't do anything.
/// </summary>
public class NullLogger : LoggingProvider
{
    public override void Log(MessageType type, string source, string message)
    {
    }

    public override void Dispose()
    {
    }
}