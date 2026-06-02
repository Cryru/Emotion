#nullable enable

using Emotion.Core.Systems.Logging;

namespace Emotion.Core.Platform.Implementation.Web;

[DontSerialize]
public sealed class WebLogger : LoggingProvider
{
    public override void Log(MessageType type, string source, string message)
    {
        Console.WriteLine($"[{type}] {Engine.TotalTime:0} [{source}] [{Environment.CurrentManagedThreadId}] {message}");
    }

    public override void Dispose()
    {
    }
}
