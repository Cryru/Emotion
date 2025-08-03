#nullable enable

namespace Emotion.Game.Systems.UI.Text.TextUpdate;

public interface IStringTagResolvingObject
{
    public object? ResolveParam(StringContext ctx, string paramName);
}
