#nullable enable

#region Using

using Emotion.IO;

#endregion

namespace Emotion.Editor.EditorWindows.DataEditorUtil;

public abstract class GameDataObject : IComparable<GameDataObject>
{
    public string Id = "Untitled";

    [DontShowInEditor]
    public string? AssetPath;

    [DontShowInEditor]
    public int Index;

    public int CompareTo(GameDataObject? other)
    {
        if (other == null) return 0;
        return Math.Sign(other.Index - Index);
    }

    public bool Save()
    {
        if (string.IsNullOrEmpty(AssetPath)) AssetPath = GameDataDatabase.GetAssetPath(this);
        return XMLAsset<GameDataObject>.CreateFromContent(this, AssetPath).Save();
    }
}