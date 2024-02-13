#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.IO;

#endregion

namespace Emotion.Editor.EditorWindows.DataEditorUtil;

public abstract class GameDataObject : IComparable<GameDataObject>
{
    public string Id = "Untitled";

    public string? Category;

    [DontSerialize]
    public string LoadedFromFile;

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