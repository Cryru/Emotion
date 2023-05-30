#region Using

using System.Threading.Tasks;
using Emotion.Game.World2D.Editor;
using Emotion.Graphics;
using Emotion.Scenography;

#endregion

#nullable enable

namespace Emotion.Game.World2D.SceneControl
{
	public abstract class World2DBaseScene<T> : Scene, IWorld2DAwareScene where T : Map2D
	{
		public T? CurrentMap { get; private set; }

		private World2DEditor _editor;

		protected World2DBaseScene()
		{
			_editor = new World2DEditor(this, typeof(T));
			_editor.InitializeEditor();
		}

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

		public Map2D GetCurrentMap()
		{
			return CurrentMap!;
		}

		public async Task ChangeMapAsync(Map2D? map)
		{
			if (map == null) return;
			var mapT = map as T;

			CurrentMap?.Dispose();
			CurrentMap = mapT;
			await map.InitAsync();
		}
	}
}