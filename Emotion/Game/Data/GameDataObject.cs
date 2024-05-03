#nullable enable

#region Using

using Emotion;
using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.IO;

#endregion

namespace Emotion.Game.Data;

public abstract class GameDataObject : IComparable<GameDataObject>
{
    public string Id = "Untitled";

    public string? Category;

    [DontSerialize]
    public string LoadedFromFile;

    [DontSerialize]
    public bool LoadedFromClass;

    [DontShowInEditor]
    public int Index;

    public int CompareTo(GameDataObject? other)
    {
        if (other == null) return 0;
        return Math.Sign(other.Index - Index);
    }

    public bool Save()
    {
        string assetPath = GameDataDatabase.EditorAdapter.GetAssetPath(this);
        return XMLAsset<GameDataObject>.CreateFromContent(this, assetPath).Save();
    }
}