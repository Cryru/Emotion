#nullable enable

using Emotion;

namespace Emotion.Graphics.Text.TextUpdate;

public interface IStringTagResolvingObject
{
    public object? ResolveParam(StringContext ctx, string paramName);
}
