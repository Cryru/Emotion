#nullable enable

#region Using

using Emotion.IO;

#endregion

namespace Emotion.Editor.EditorWindows.DataEditorUtil;

public abstract class GameDataObject
{
	public string Id = "Untitled";

	[DontShowInEditorAttribute] public string? AssetPath;

	public bool Save()
	{
		if (string.IsNullOrEmpty(AssetPath)) AssetPath = GameDataDatabase.GetAssetPath(this);
		return XMLAsset<GameDataObject>.CreateFromContent(this, AssetPath).Save();
	}
}