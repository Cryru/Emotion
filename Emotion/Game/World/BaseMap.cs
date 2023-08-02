#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World;

public abstract class BaseMap
{
	/// <summary>
	/// The name of the map. Not to be confused with the asset name carried by the XMLAsset.
	/// </summary>
	public string MapName { get; set; }

	/// <summary>
	/// The file the map was loaded from, if any. Should equal the XMLAsset name.
	/// </summary>
	public string? FileName { get; set; }

	#region Events

	public event Action? OnMapReset;

	#endregion

	protected BaseMap(string mapName = "Unnamed Map")
	{
		MapName = mapName;
	}

	#region Editor Support

	/// <summary>
	/// Whether the map is currently open in the editor.
	/// Mostly used to conditionally render/represent things.
	/// </summary>
	[DontSerialize] public bool EditorMode;

	#endregion

	/// <summary>
	/// Reset the map to its "loaded from file" state.
	/// </summary>
	public virtual async Task Reset()
	{
		await InitAsync();

		OnMapReset?.Invoke();
	}

	#region Internal API

	public abstract Task InitAsync();
	public abstract void Update(float dt);
	public abstract void Render(RenderComposer c);
	public abstract void Dispose();

	#endregion
}