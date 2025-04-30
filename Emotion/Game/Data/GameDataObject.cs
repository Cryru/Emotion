#nullable enable

#region Using

using Emotion.Editor;
using Emotion.Standard.Reflector;

#endregion

namespace Emotion.Game.Data;

public abstract class GameDataObject : IComparable<GameDataObject>
{
    [DontShowInEditor]
    public string? LoadedFromModel;

    [DontShowInEditor]
    public int Index;

    public string Id = "Untitled";

    public string? Category;

    public int CompareTo(GameDataObject? other)
    {
        if (other == null) return 0;
        return Math.Sign(other.Index - Index);
    }

    public virtual GameDataObject CreateCopy()
    {
        return ReflectorEngine.CreateCopyOf(this)!;
    }

    public override string ToString()
    {
        return Id;
    }
}