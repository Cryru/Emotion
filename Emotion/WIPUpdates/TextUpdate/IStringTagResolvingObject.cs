#nullable enable

using Emotion;

namespace Emotion.WIPUpdates.TextUpdate;

public interface IStringTagResolvingObject
{
    public object? ResolveParam(StringContext ctx, string paramName);
}
