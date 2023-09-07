#region Using

using Emotion.Editor.EditorComponents;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.PropertyEditors;
using Emotion.IO;

#endregion

#nullable enable

namespace Emotion.Editor;

public class AssetFileNameAttribute : Attribute
{
	public virtual EditorPanel CreateFileExplorer(PropEditorStringPath editor)
	{
		var fileExplorer = new EditorFileExplorer<OtherAsset>(asset => { editor.SetValue(asset.Name); });
		return fileExplorer;
	}
}

public class AssetFileNameAttribute<T> : AssetFileNameAttribute where T : Asset, new()
{
	// Creation of the file explorer needs to be handler here,
	// where we have a reference to the generic param.
	public override EditorPanel CreateFileExplorer(PropEditorStringPath editor)
	{
		var fileExplorer = new EditorFileExplorer<T>(asset => { editor.SetValue(asset.Name); });
		return fileExplorer;
	}
}