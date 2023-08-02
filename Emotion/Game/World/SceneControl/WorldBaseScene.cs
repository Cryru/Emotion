#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World.Editor;
using Emotion.Graphics;
using Emotion.Scenography;

#endregion

namespace Emotion.Game.World.SceneControl;

public abstract class WorldBaseScene<T> : Scene, IWorldAwareScene<T> where T : BaseMap
{
	public T? CurrentMap { get; private set; }

	protected WorldBaseEditor _editor;

	protected WorldBaseScene()
	{
		_editor = CreateEditor();
		_editor.InitializeEditor();
	}

	protected abstract WorldBaseEditor CreateEditor();

	public override void Update()
	{
		CurrentMap?.Update(Engine.DeltaTime);
		_editor.Update(Engine.DeltaTime);
	}

	public override void Draw(RenderComposer composer)
	{
		CurrentMap?.Render(composer);
		_editor.Render(composer);
	}

	public T GetCurrentMap()
	{
		return CurrentMap!;
	}

	public async Task ChangeMapAsync(T? map)
	{
		if (map == null) return;

		CurrentMap?.Dispose();
		CurrentMap = map;
		await map.InitAsync();
	}
}